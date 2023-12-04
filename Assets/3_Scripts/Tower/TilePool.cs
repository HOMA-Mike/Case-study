using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Component used to pool TowerTiles</summary>
public class TilePool : MonoBehaviour
{
    static TilePool instance;

    [Header("Settings")]
    public int initialPoolSize;

    [Header("References")]
    public TowerTile tilePrefab;
    public TowerTile[] specialPrefabs;

    List<TowerTile> normalTiles;
    List<TowerTile>[] specialTiles;

    void Awake()
    {
        if (instance != null || !RemoteConfig.BOOL_TILES_POOLING)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        instance = this;

        normalTiles = PrewarmPool(tilePrefab);
        specialTiles = new List<TowerTile>[specialPrefabs.Length];

        for (int i = 0; i < specialPrefabs.Length; i++)
            specialTiles[i] = PrewarmPool(specialPrefabs[i]);
    }

    List<TowerTile> PrewarmPool(TowerTile prefab)
    {
        List<TowerTile> pool = new List<TowerTile>();

        int count = initialPoolSize;

        while (count > 0)
        {
            pool.Add(Instantiate(prefab, transform));
            pool[^1].gameObject.SetActive(false);

            count--;
        }

        return pool;
    }

    public static TowerTile GetTile(bool isNormal, int specialIndex, Action<TowerTile> Configure)
    {
        if (instance == null)
        {
            Debug.LogError("No instance of TilePool, make sure you placed the prefab in the scene");
            return null;
        }

        List<TowerTile> pickedPool = isNormal ? instance.normalTiles : instance.specialTiles[specialIndex];
        TowerTile available = pickedPool.Find(item => !item.gameObject.activeSelf);

        if (available == null)
        {
            available = Instantiate(isNormal ? instance.tilePrefab : instance.specialPrefabs[specialIndex], instance.transform);

            if (isNormal)
                instance.normalTiles.Add(available);
            else
                instance.specialTiles[specialIndex].Add(available);
        }

        available.ResetColor();
        available.gameObject.SetActive(true);

        Configure?.Invoke(available); // easy configuration injection
        return available;
    }

    // replace "Destroy(tile)" with this
    public static void ReturnTile(TowerTile tile)
    {
        tile.gameObject.SetActive(false);
        tile.transform.SetParent(instance.transform);
        tile.OnTileDestroyed = null;
        tile.GetComponent<Rigidbody>().velocity = Vector3.zero;
        tile.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    public static void RetrieveAllTiles()
    {
        foreach (Transform child in instance.transform)
            child.GetComponent<TowerTile>().ReturnToPool();
    }
}
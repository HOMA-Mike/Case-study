using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using static MissionData;
using Random = UnityEngine.Random;

/// <summary>Manages missions tracking and updating</summary>
public class MissionsManager : MonoBehaviour
{
    static MissionsManager instance;

    // Add new Tracker for each objective
    public static Tracker OnComboReached;
    public static Tracker OnWonGame;
    public static Tracker OnTriggeredExplosion;
    public static Tracker OnSecondsPassed;

    [Header("Settings")]
    public Color easyColor;
    public Color mediumColor;
    public Color hardColor;

    [Header("References")]
    public List<MissionData> missions;

    Mission[] currentMissions;
    MissionsPanel missionsPanel;
    float timer;

    // Event based checking so we have minimal impact on the game's performances

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += (scene, mode) => StartCoroutine(WaitForSafeInit());

        OnComboReached = new Tracker(false);
        OnWonGame = new Tracker(true);
        OnTriggeredExplosion = new Tracker(true);
        OnSecondsPassed = new Tracker(true);

        currentMissions = new Mission[3];
        currentMissions[0] = PickMission(Difficulty.Easy);
        currentMissions[1] = PickMission(Difficulty.Medium);
        currentMissions[2] = PickMission(Difficulty.Hard);
    }

    void Update()
    {
        if (GameManager.Instance.gameState != GameState.Playing)
        {
            timer = 0;
            return;
        }

        timer += Time.deltaTime;

        if (timer >= 1)
        {
            int passed = 0;

            while (timer > 1)
            {
                timer--;
                passed++;
            }

            OnSecondsPassed.Invoke(-passed);
        }
    }

    IEnumerator WaitForSafeInit()
    {
        if (!RemoteConfig.BOOl_MISSIONS_ENABLED)
            yield break;

        yield return new WaitUntil(() => missionsPanel != null);

        for (int i = 0; i < currentMissions.Length; i++)
        {
            Mission mission = currentMissions[i];

            if (mission == null)
            {
                currentMissions[i] = PickMission((Difficulty)i);
                mission = currentMissions[i];
            }

            // reset countdown when new game
            if (mission.objective == Objective.WinInTime)
                mission.TrackProgress(mission.amount);

            missionsPanel.SpawnMission(mission);
        }
    }

    Mission PickMission(Difficulty difficulty)
    {
        if (!RemoteConfig.BOOl_MISSIONS_ENABLED)
            return null;

        List<MissionData> allMissions = new List<MissionData>(missions);

        foreach (Mission data in currentMissions)
        {
            if (data == null)
                continue;

            allMissions.RemoveAll(item => item.objective == data.objective);

            if (allMissions.Count == 0)
            {
                Debug.LogError("No available missions left.");
                return null;
            }
        }

        Mission selected = new Mission(allMissions[Random.Range(0, allMissions.Count)], difficulty);

        SubscribeTracking(selected, value =>
        {
            selected.TrackProgress(value);
            CheckMissionsDone();
        });

        return selected;
    }

    // TODO : Revert to inline switch
    public static void SubscribeTracking(Mission mission, Action<int> callback)
    {
        switch (mission.objective)
        {
            case Objective.ReachCombo:
                OnComboReached.Subscribe(callback);
                break;

            case Objective.WinGames:
                OnWonGame.Subscribe(callback);
                break;

            case Objective.TriggerExplosions:
                OnTriggeredExplosion.Subscribe(callback);
                break;

            case Objective.WinInTime:
                OnSecondsPassed.Subscribe(callback, mission.amount);
                break;

            default:
                Debug.LogError(
                    "Couldn't find a tracking event for objective " + mission.objective +
                    ". Please add it at the top of this script",
                    instance
                );
                break;
        }
    }

    public static void UnsubscribeTracking(Objective objective, Action<int> callback)
    {
        switch (objective)
        {
            case Objective.ReachCombo:
                OnComboReached.CancelTracking(callback);
                break;

            case Objective.WinGames:
                OnWonGame.CancelTracking(callback);
                break;

            case Objective.TriggerExplosions:
                OnTriggeredExplosion.CancelTracking(callback);
                break;

            case Objective.WinInTime:
                OnSecondsPassed.CancelTracking(callback);
                break;

            default:
                Debug.LogError(
                    "Couldn't find a tracking event for objective " + objective +
                    ". Please add it at the top of this script",
                    instance
                );
                break;
        }
    }

    static void CheckMissionsDone()
    {
        for (int i = 0; i < instance.currentMissions.Length; i++)
        {
            Mission mission = instance.currentMissions[i];

            if (mission == null)
                continue;

            if (mission.objective == Objective.WinInTime)
            {
                if (mission.progress <= 0)
                {
                    UnsubscribeTracking(mission.objective, mission.TrackProgress);
                    instance.currentMissions[i] = null;
                }
            }
            else if (mission.progress >= 1)
            {
                CurrencyManager.AddCurrency(mission.currency, mission.reward);
                UnsubscribeTracking(mission.objective, mission.TrackProgress);
                instance.currentMissions[i] = null;
            }
        }
    }

    public static Color GetColor(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.Easy => instance.easyColor,
            Difficulty.Medium => instance.mediumColor,
            Difficulty.Hard => instance.hardColor,
            _ => Color.white
        };
    }

    public static void OnGameWon()
    {
        OnWonGame.Invoke(1);

        // custom check for WinInTime objective
        foreach (Mission mission in instance.currentMissions)
        {
            if (mission == null)
                continue;

            if (mission.objective == Objective.WinInTime && mission.progress > 0)
                CurrencyManager.AddCurrency(mission.currency, mission.reward);
        }
    }

    // Called by MissionPanel when it has spawned
    public static void SetUI(MissionsPanel panel) => instance.missionsPanel = panel;

    /// <summary>Wrapper around tracker event to contain counters and different behaviours</summary>
    public class Tracker
    {
        Action<int> trackingEvent;
        int counter;
        bool accumulates;

        public Tracker(bool accumulates)
        {
            counter = 0;
            this.accumulates = accumulates;
        }

        public void Invoke(int value)
        {
            if (accumulates)
            {
                counter += value;
                trackingEvent?.Invoke(counter);
            }
            else
                trackingEvent?.Invoke(value);
        }

        public void Subscribe(Action<int> callback, int target = 0)
        {
            trackingEvent += callback;

            if (target != 0)
                counter = target;

            Invoke(target != 0 ? 0 : counter);
        }

        public void CancelTracking(Action<int> callback)
        {
            trackingEvent -= callback;
            // counter = 0;
        }
    }
}
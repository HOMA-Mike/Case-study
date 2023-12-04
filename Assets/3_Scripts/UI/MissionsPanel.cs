using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using static MissionData;
using static UnityEngine.RectTransform;

/// <summary>Side panel holding mission tickets while in game</summary>
public class MissionsPanel : MonoBehaviour
{
    static MissionsPanel instance;

    [Header("References")]
    public Button openPopupButton;
    public RectTransform openZone;
    public RectTransform closedZone;
    public RectTransform frame;
    public Transform ticketsHolder;
    [Space]
    public MissionTicket ticketPrefab;

    public bool HasSlot => ticketsHolder.childCount < 3;

    Coroutine panelAnimation;
    bool isOpen;

    void Awake()
    {
        if (!RemoteConfig.BOOl_MISSIONS_ENABLED)
        {
            gameObject.SetActive(false);
            return;
        }

        // signals to mission manager that UI is ready
        MissionsManager.SetUI(this);
        instance = this;

        openPopupButton.onClick.AddListener(() =>
        {
            isOpen = !isOpen;

            if (panelAnimation != null)
                StopCoroutine(panelAnimation);

            panelAnimation = StartCoroutine(AnimatePanel(0.5f));
        });

        isOpen = false;
        StartCoroutine(DelayCall(() => StartCoroutine(AnimatePanel(0.5f)), 0.1f));
    }

    IEnumerator DelayCall(Action callback, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
    }

    IEnumerator AnimatePanel(float duration)
    {
        float timer = 0;
        float startSize = frame.rect.height;
        float targetSize = isOpen ? openZone.rect.height : closedZone.rect.height;
        Vector3 startPos = frame.position;
        Vector3 targetPos = isOpen ? openZone.position : closedZone.position;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float percent = timer / duration;

            frame.SetSizeWithCurrentAnchors(Axis.Vertical, Mathf.Lerp(startSize, targetSize, percent));
            frame.position = Vector3.Lerp(startPos, targetPos, percent);

            yield return null;
        }

        frame.position = targetPos;
        frame.SetSizeWithCurrentAnchors(Axis.Vertical, targetSize);
        panelAnimation = null;
    }

    public void SpawnMission(Mission mission)
    {
        MissionTicket ticket = Instantiate(ticketPrefab, ticketsHolder);
        ticket.Init(mission);

        UpdateMissionsOrder();
    }

    void UpdateMissionsOrder()
    {
        foreach (Transform child in ticketsHolder)
        {
            MissionTicket ticket = child.GetComponent<MissionTicket>();

            if (ticket.mission.difficulty == Difficulty.Easy)
                child.SetAsFirstSibling();

            if (ticket.mission.difficulty == Difficulty.Hard)
                child.SetAsLastSibling();
        }
    }

    // need this to not throw errors when changing scenes
    public static void UnsubscribeTickets()
    {
        foreach (Transform ticket in instance.ticketsHolder)
            ticket.GetComponent<MissionTicket>().UnsubscribeEvents();
    }
}
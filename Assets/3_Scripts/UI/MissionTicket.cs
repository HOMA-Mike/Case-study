using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static MissionData;

/// <summary>Displays mission objective and progress</summary>
public class MissionTicket : MonoBehaviour
{
    [Header("References")]
    public Animator anim;
    public Image[] fillers;
    public TMP_Text title;
    public Slider progressSlider;
    public Toggle progressToggle;
    public TMP_Text progressText;

    public Mission mission { get; private set; }

    public void Init(Mission mission)
    {
        this.mission = mission;

        Color difficultyColor = MissionsManager.GetColor(mission.difficulty);

        foreach (Image filler in fillers)
            filler.color = difficultyColor;

        title.color = difficultyColor;
        progressText.color = difficultyColor;
        title.text = mission.title;
        progressSlider.value = mission.progress;
        progressToggle.isOn = mission.progress >= 1;
        progressText.text = mission.progress + "/" + mission.amount;

        progressToggle.gameObject.SetActive(mission.amount == 1);
        progressSlider.gameObject.SetActive(mission.amount > 1);
        progressText.gameObject.SetActive(mission.amount > 1);

        MissionsManager.SubscribeTracking(mission, OnProgressChanged);
        anim.Play("ShowTicket");
    }

    void OnProgressChanged(int value)
    {
        if (mission.amount == 1)
            progressToggle.isOn = value >= 1;
        else
        {
            progressSlider.value = (float)value / mission.amount;
            progressText.text = value + "/" + mission.amount;
        }

        if (mission.objective == Objective.WinInTime)
        {
            if (mission.progress <= 0)
                DestroyTicket();
        }
        else if (mission.progress >= mission.amount)
            DestroyTicket();

        void DestroyTicket()
        {
            anim.Play("HideTicket");

            UnsubscribeEvents();
            Destroy(gameObject, 0.55f);
        }
    }

    // need this to not throw errors when changing scenes
    public void UnsubscribeEvents()
    {
        MissionsManager.UnsubscribeTracking(mission.objective, mission.TrackProgress);
        MissionsManager.UnsubscribeTracking(mission.objective, OnProgressChanged);
    }
}
using System;
using UnityEngine;

using static CurrencyManager;
using static MissionData;

// lightweight class for data manipulation + safe with readonly
/// <summary>Represents a mission with a difficulty level</summary>
[Serializable] // for debug purposes
public class Mission
{
    public /*readonly*/ Objective objective;
    public /*readonly*/ Difficulty difficulty;
    public /*readonly*/ int amount;
    public /*readonly*/ Currency currency;
    public /*readonly*/ int reward;
    public /*readonly*/ string title;

    public float progress { get; private set; }

    public Mission(MissionData data, Difficulty difficulty)
    {
        objective = data.objective;
        this.difficulty = difficulty;

        DifficultyLevel level = data.levelTable.Find(item => item.difficulty == difficulty);

        if (level == null)
        {
            Debug.LogError("Couldn't find Level for difficulty " + difficulty + ". Please edit the MissionData to include this Difficulty.");
            return;
        }

        amount = level.amount;
        currency = level.currency;
        reward = level.reward;

        title = string.Format(data.missionTexts[(int)data.objective], amount);
    }

    // easy to subscribe/unsubscribe
    public void TrackProgress(int value) => progress = (float)value / amount;
}
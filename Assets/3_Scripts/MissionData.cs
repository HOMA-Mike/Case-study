using System;
using System.Collections.Generic;
using UnityEngine;

using static CurrencyManager;

/// <summary>Describes a missions and it's variations given difficulty</summary>
[CreateAssetMenu(fileName = "MissionData", menuName = "Data/Mission")]
public class MissionData : ScriptableObject
{
    public enum Objective
    {
        ReachCombo = 0,
        WinGames = 1,
        TriggerExplosions = 2,
        WinInTime = 3,
        // Add objectives here
    }

    public string[] missionTexts
    {
        get
        {
            return new string[]
            {
                "Reach a combo of {0}", // 0
                "Win {0} games", // 1
                "Trigger {0} explosions", // 2
                "Win in less than {0} seconds" // 3
            };
        }
    }

    public enum Difficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    public Objective objective;
    public List<DifficultyLevel> levelTable;

    /// <summary>Describes a difficulty level of a mission</summary>
    [Serializable]
    public class DifficultyLevel
    {
        public Difficulty difficulty;
        public int amount;
        [Space]
        public Currency currency;
        public int reward;
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonLevelsDifficulty : MonoBehaviour
{
    #region Created classes

    [Serializable]
    public class StatsIncreaseOnDungeonDifficulty
    {
        public enum EnemiesStatIncrease
        {
            HP,
            DEF,
            ATK
        }

        public EnemiesStatIncrease chosenStatIncrease = EnemiesStatIncrease.HP;
        public List<StatIncreaseRequirement> allStatIncreaseRequirements = new();

        [Serializable]
        public class StatIncreaseRequirement
        {
            public int dungeonLevelToActivateStatMultiplayer;
            public float multiplayerForEachDungeonLevel;
        }
    }

    #endregion

    public static DungeonLevelsDifficulty Instance { get; private set; }

    #region Variables & References

    [SerializeField] private List<StatsIncreaseOnDungeonDifficulty> allStatsIncreaseOnDungeonDifficulty = new();
    [SerializeField] private float additionalBaseHpPercentagePerConnectedPlayer = 0.5f;
    [SerializeField] private float additionalBaseAtkPercentagePerConnectedPlayer = 0.1f;

    #endregion

    #region Initialization

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    #endregion

    #region GetIncreasedStats

    public float GetEnemyBaseStatIncreaseByCurrentConnectedPlayers(
        StatsIncreaseOnDungeonDifficulty.EnemiesStatIncrease currentStat)
    {
        var currentConnectedPlayers = AllConnectedPlayers.Instance.GetAllConnectedPlayerCount();
        var statMultiplayer = 1f;

        switch (currentStat)
        {
            case StatsIncreaseOnDungeonDifficulty.EnemiesStatIncrease.HP:
                statMultiplayer += (currentConnectedPlayers - 1) * additionalBaseHpPercentagePerConnectedPlayer;
                break;
            case StatsIncreaseOnDungeonDifficulty.EnemiesStatIncrease.ATK:
                statMultiplayer += (currentConnectedPlayers - 1) * additionalBaseAtkPercentagePerConnectedPlayer;
                break;
        }

        return statMultiplayer;
    }

    public float GetCurrentEnemyStatIncreaseMultiplayer(
        StatsIncreaseOnDungeonDifficulty.EnemiesStatIncrease currentStat)
    {
        var currentStatIncrease = 0f;
        var currentDungeonLevel = DungeonSettings.GetCurrentDungeonLevel();

        foreach (var statIncreaseOnDungeonDifficulty in allStatsIncreaseOnDungeonDifficulty)
        {
            if (statIncreaseOnDungeonDifficulty.chosenStatIncrease != currentStat) continue;

            foreach (var statIncreaseRequirement in statIncreaseOnDungeonDifficulty.allStatIncreaseRequirements)
            {
                if (statIncreaseRequirement.dungeonLevelToActivateStatMultiplayer > currentDungeonLevel) break;

                currentStatIncrease = statIncreaseRequirement.multiplayerForEachDungeonLevel * currentDungeonLevel;
            }

            break;
        }

        return currentStatIncrease;
    }

    #endregion
}

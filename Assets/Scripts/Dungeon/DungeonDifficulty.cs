using System;
using UnityEngine;

public class DungeonDifficulty : MonoBehaviour
{
    public static event EventHandler<OnDungeonDifficultyChangeEventArgs> OnDungeonDifficultyChange;

    public class OnDungeonDifficultyChangeEventArgs : EventArgs
    {
        public int newDungeonDifficulty;
    }

    [SerializeField] private int currentDungeonDifficulty;

    private void ChangeDungeonDifficulty(int newDifficulty)
    {
        currentDungeonDifficulty = newDifficulty;
        OnDungeonDifficultyChange?.Invoke(this, new OnDungeonDifficultyChangeEventArgs
        {
            newDungeonDifficulty = newDifficulty
        });
    }

    public static float GetEnemiesHpMultiplayerByDungeonDifficulty(int difficulty)
    {
        return 1f + difficulty / 100f;
    }

    public static float GetEnemiesAtkMultiplayerByDungeonDifficulty(int difficulty)
    {
        return 1f + difficulty / 250f;
    }

    public static float GetEnemiesHpMultiplayerByPlayersCount()
    {
        return 1f;
    }

    public static float GetEnemiesAtkMultiplayerByPlayersCount()
    {
        return 1f;
    }
}

using System;

public static class DungeonSettings
{
    public static event EventHandler<OnDungeonDifficultyChangeEventArgs> OnDungeonDifficultyChange;

    public class OnDungeonDifficultyChangeEventArgs : EventArgs
    {
        public int newDungeonDifficulty;
    }

    private static int currentDungeonDifficulty;
    private static readonly int AmountOfCompleteLevelsForDungeonRoomsCountIncrease = 5;
    private static int completedDungeons;

    private static int dungeonRoomsAmount = 9;

    public static void OnDungeonComplete()
    {
        completedDungeons++;

        if (completedDungeons >= AmountOfCompleteLevelsForDungeonRoomsCountIncrease)
        {
            completedDungeons = 0;
            dungeonRoomsAmount++;
        }
    }

    private static void ChangeDungeonDifficulty(int newDifficulty)
    {
        currentDungeonDifficulty = newDifficulty;
        OnDungeonDifficultyChange?.Invoke(null, new OnDungeonDifficultyChangeEventArgs
        {
            newDungeonDifficulty = newDifficulty
        });
    }

    public static int GetCurrentDungeonRoomsCount()
    {
        return dungeonRoomsAmount;
    }

    public static int GetCurrentDungeonDifficulty()
    {
        return currentDungeonDifficulty;
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

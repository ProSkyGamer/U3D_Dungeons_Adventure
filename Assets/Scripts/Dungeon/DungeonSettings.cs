public static class DungeonSettings
{
    private static readonly int AmountOfCompleteLevelsForDungeonRoomsCountIncrease = 5;
    private static int currentDungeonLevel;

    private static int dungeonRoomsAmount = 9;

    public static void OnDungeonComplete()
    {
        currentDungeonLevel++;

        if (currentDungeonLevel >= AmountOfCompleteLevelsForDungeonRoomsCountIncrease)
        {
            currentDungeonLevel = 0;
            dungeonRoomsAmount++;
        }
    }

    public static int GetCurrentDungeonRoomsCount()
    {
        return dungeonRoomsAmount;
    }

    public static int GetCurrentDungeonLevel()
    {
        return currentDungeonLevel;
    }
}

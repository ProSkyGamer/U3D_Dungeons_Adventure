public static class DungeonSettings
{
    #region Variables

    private static readonly int AmountOfCompleteLevelsForDungeonRoomsCountIncrease = 5;
    private static int currentDungeonLevel;

    private static int dungeonRoomsAmount = 9;

    #endregion

    #region Dungeon Methods

    public static void OnDungeonComplete()
    {
        currentDungeonLevel++;

        if (currentDungeonLevel >= AmountOfCompleteLevelsForDungeonRoomsCountIncrease)
        {
            currentDungeonLevel = 0;
            dungeonRoomsAmount++;
        }
    }

    #endregion

    #region Get Dungeon Data

    public static int GetCurrentDungeonRoomsCount()
    {
        return dungeonRoomsAmount;
    }

    public static int GetCurrentDungeonLevel()
    {
        return currentDungeonLevel;
    }

    #endregion
}

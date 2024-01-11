using System.Collections.Generic;

public static class StoredPlayerData
{
    private static readonly Dictionary<ulong, StoredPlayerDataSingle> AllStoredPlayerData = new();

    public class StoredPlayerDataSingle
    {
        public int currentLevelExperienceNeeded;
        public int currentSkillPointExperience;
        public int currentExperience;
        public int currentAvailableSkillPoint;

        public int currentCoins;

        public InventoryObject[] playerInventory;
        public InventoryObject[] weaponsInventory;
        public InventoryObject[] relicsInventory;

        public int currentHealth;

        public List<int> boughtUpgradesID;
    }

    public static void AddStoredPlayerData(PlayerController playerDataToSave)
    {
        var newStoredPlayerData = new StoredPlayerDataSingle
        {
            currentLevelExperienceNeeded = playerDataToSave.GetExperienceForCurrentLevel(),
            currentExperience = playerDataToSave.GetCurrentLevelExperience(),
            currentSkillPointExperience = playerDataToSave.GetCurrentSkillPointExperience(),
            currentAvailableSkillPoint = playerDataToSave.GetCurrentSkillPointsValue(),
            currentCoins = playerDataToSave.GetCurrentCoinsValue(),
            boughtUpgradesID = playerDataToSave.GetCurrentBoughtUpgradeIDs()
        };

        var playerDataToSaveInventory = playerDataToSave.GetPlayerInventory();
        newStoredPlayerData.playerInventory = new InventoryObject[playerDataToSaveInventory.GetMaxSlotsCount()];
        for (var i = 0; i < playerDataToSaveInventory.GetMaxSlotsCount(); i++)
            newStoredPlayerData.playerInventory[i] = playerDataToSaveInventory.GetInventoryObjectBySlot(i);

        var playerDataToSaveWeaponsInventory = playerDataToSave.GetPlayerWeaponsInventory();
        newStoredPlayerData.weaponsInventory = new InventoryObject[playerDataToSaveWeaponsInventory.GetMaxSlotsCount()];
        for (var i = 0; i < playerDataToSaveWeaponsInventory.GetMaxSlotsCount(); i++)
            newStoredPlayerData.weaponsInventory[i] = playerDataToSaveWeaponsInventory.GetInventoryObjectBySlot(i);

        var playerDataToSaveRelicsInventory = playerDataToSave.GetPlayerRelicsInventory();
        newStoredPlayerData.relicsInventory = new InventoryObject[playerDataToSaveRelicsInventory.GetMaxSlotsCount()];
        for (var i = 0; i < playerDataToSaveRelicsInventory.GetMaxSlotsCount(); i++)
            newStoredPlayerData.relicsInventory[i] = playerDataToSaveRelicsInventory.GetInventoryObjectBySlot(i);

        var newStoredPlayerDataHealthController = playerDataToSave.GetPlayerHealthController();
        newStoredPlayerData.currentHealth = newStoredPlayerDataHealthController.GetCurrentHealth();

        AllStoredPlayerData.Add(playerDataToSave.GetPlayerOwnerID(), newStoredPlayerData);
    }

    public static StoredPlayerDataSingle GetPersonStoredData(ulong playerOwnerID)
    {
        if (!AllStoredPlayerData.ContainsKey(playerOwnerID)) return null;

        var dataToReturn = AllStoredPlayerData[playerOwnerID];
        AllStoredPlayerData.Remove(playerOwnerID);

        return dataToReturn;
    }
}

using System.Collections.Generic;

public static class PlayerBoughtUpgrades
{
    private class BoughtUpgrades
    {
        public PlayerEffectsController.AllPlayerEffects buffType;
        public float buffValue;
        public int boughtItemID;
    }

    private static readonly List<BoughtUpgrades> boughtUpgrades = new();

    public static void AddBoughtUpgrade(PlayerEffectsController.AllPlayerEffects buffType, float buffValue, int itemID)
    {
        var newBoughtUpgrade = new BoughtUpgrades
        {
            buffType = buffType, buffValue = buffValue, boughtItemID = itemID
        };
        boughtUpgrades.Add(newBoughtUpgrade);
    }

    public static bool IsUpgradeAlreadyBought(PlayerEffectsController.AllPlayerEffects buffType, float buffValue,
        int itemID)
    {
        foreach (var boughtUpgrade in boughtUpgrades)
        {
            if (boughtUpgrade.buffType != buffType || boughtUpgrade.boughtItemID != itemID ||
                boughtUpgrade.buffValue != buffValue)
                continue;

            return true;
        }

        return false;
    }
}
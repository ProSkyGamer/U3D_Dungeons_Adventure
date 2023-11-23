using System.Collections.Generic;

public static class PlayerBoughtUpgrades
{
    private class BoughtUpgrades
    {
        public PlayerEffects.PlayerBuff.Buffs buffType;
        public float buffValue;
        public int boughtItemID;
    }

    private static readonly List<BoughtUpgrades> boughtUpgrades = new();

    public static void AddBoughtUpgrade(PlayerEffects.PlayerBuff.Buffs buffType, float buffValue, int itemID)
    {
        var newBoughtUpgrade = new BoughtUpgrades
        {
            buffType = buffType, buffValue = buffValue, boughtItemID = itemID
        };
        boughtUpgrades.Add(newBoughtUpgrade);
    }

    public static bool IsUpgradeAlreadyBought(PlayerEffects.PlayerBuff.Buffs buffType, float buffValue, int itemID)
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

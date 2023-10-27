using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class LootChest : InteractableItem
{
    [SerializeField] private int coinsInChest = 10;
    [SerializeField] private int experienceForChest = 10;

    [SerializeField] [Range(0f, 1f)] private float weaponDropChance = 1f;
    [SerializeField] private List<InventoryObject> availableWeaponToDrop = new();

    private bool isLootChestLocked;

    public void LockChest()
    {
        isLootChestLocked = true;
    }

    public void UnlockChest()
    {
        isLootChestLocked = false;
    }

    public override void OnInteract(PlayerController player)
    {
        InventoryObject weaponToDrop = null;

        base.OnInteract(player);

        if (coinsInChest != 0)
            player.ReceiveCoins(coinsInChest);

        if (experienceForChest != 0)
            player.ReceiveExperience(experienceForChest);

        if (availableWeaponToDrop.Count != 0 &&
            Random.Range(0, 100) < weaponDropChance * 100)
        {
            var weaponToDropIndex = Random.Range(0, availableWeaponToDrop.Count);
            weaponToDrop = availableWeaponToDrop[weaponToDropIndex];

            var weaponToDropNewObject = ScriptableObject.CreateInstance<InventoryObject>();
            weaponToDropNewObject.SetInventoryObject(weaponToDrop);

            if (PlayerController.Instance.GetPlayerInventory().IsHasAnyAvailableSlot())
                weaponToDropNewObject.SetInventoryParent(PlayerController.Instance.GetPlayerInventory());
        }

        Debug.Log($"Added {coinsInChest} coins, {experienceForChest} experience,  {weaponToDrop.name} weapon");

        isCanInteract = false;
        Destroy(gameObject);
    }

    public override bool IsCanInteract()
    {
        return isCanInteract && !isLootChestLocked;
    }
}

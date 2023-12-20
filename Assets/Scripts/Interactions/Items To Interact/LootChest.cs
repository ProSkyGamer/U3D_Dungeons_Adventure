using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

// ReSharper disable once CheckNamespace
public class LootChest : InteractableItem
{
    public event EventHandler OnChestOpen;

    [SerializeField] private int coinsInChest = 10;
    [SerializeField] private int experienceForChest = 10;

    [SerializeField] [Range(0f, 1f)] private float weaponDropChance = 1f;
    [SerializeField] private List<InventoryObject> availableItemsToDrop = new();

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
        base.OnInteract(player);
        isCanInteract = false;

        if (coinsInChest != 0)
            player.ReceiveCoins(coinsInChest);

        if (experienceForChest != 0)
            player.ReceiveExperience(experienceForChest);

        OpenChestServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OpenChestServerRpc()
    {
        InventoryObject itemToDrop = null;

        if (availableItemsToDrop.Count != 0 &&
            Random.Range(0, 100) < weaponDropChance * 100)
        {
            var itemToDropIndex = Random.Range(0, availableItemsToDrop.Count);
            itemToDrop = availableItemsToDrop[itemToDropIndex];

            var itemToDropNewObjectTransform = Instantiate(itemToDrop.transform);
            var itemToDropNetworkObject = itemToDropNewObjectTransform.GetComponent<NetworkObject>();
            itemToDropNetworkObject.Spawn();
            var itemToDropInventoryObject = itemToDropNewObjectTransform.GetComponent<InventoryObject>();
            itemToDropInventoryObject.SpawnInventoryObject();

            itemToDropInventoryObject.DropInventoryObjectToWorld(transform.position);
        }

        OnChestOpen?.Invoke(this, EventArgs.Empty);

        Debug.Log($"Added {coinsInChest} coins, {experienceForChest} experience,  {itemToDrop.name} weapon");

        Destroy(gameObject);
    }

    public override bool IsCanInteract()
    {
        return isCanInteract && !isLootChestLocked;
    }
}

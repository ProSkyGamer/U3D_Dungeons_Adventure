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

    [SerializeField] [Range(0f, 1f)] private List<float> multipleItemsDropChances = new();
    [SerializeField] private int maxChestDropItems = 3;

    [Serializable]
    public class ChestDropItem
    {
        public InventoryObject inventoryObjectToDrop;
        public int inventoryObjectDropChance = 1;
    }

    [SerializeField] private List<ChestDropItem> allChestDropItems = new();

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

        OpenChestServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OpenChestServerRpc()
    {
        var allConnectedPlayerController = AllConnectedPlayers.Instance.GetAllPlayerControllers();

        foreach (var connectedPlayerController in allConnectedPlayerController)
        {
            if (coinsInChest != 0)
                connectedPlayerController.ReceiveCoins(coinsInChest);

            if (experienceForChest != 0)
                connectedPlayerController.ReceiveExperience(experienceForChest);
        }

        var droppingItemsCount = GetDroppingItemsCount();
        var droppingItems = GetDroppingItems(droppingItemsCount);

        foreach (var inventoryObjectToDrop in droppingItems)
        {
            var inventoryObjectToDropNewObjectTransform = Instantiate(inventoryObjectToDrop.transform);
            var inventoryObjectToDropNetworkObject =
                inventoryObjectToDropNewObjectTransform.GetComponent<NetworkObject>();
            inventoryObjectToDropNetworkObject.Spawn();
            var newInventoryObjectToDrop = inventoryObjectToDropNewObjectTransform.GetComponent<InventoryObject>();
            newInventoryObjectToDrop.SpawnInventoryObject();

            newInventoryObjectToDrop.DropInventoryObjectToWorld(transform.position);
        }

        OnChestOpen?.Invoke(this, EventArgs.Empty);

        Destroy(gameObject);
    }

    private int GetDroppingItemsCount()
    {
        var maxDroppingItemsCount =
            allChestDropItems.Count < maxChestDropItems ? allChestDropItems.Count : maxChestDropItems;
        var currentDroppingItems = 0;

        for (var i = 0; i < multipleItemsDropChances.Count && i < maxDroppingItemsCount; i++)
        {
            var isDropping = Random.Range(0, 100) < multipleItemsDropChances[i] * 100;

            if (!isDropping) break;

            currentDroppingItems++;
        }

        return currentDroppingItems;
    }

    private List<InventoryObject> GetDroppingItems(int droppingItemsCount)
    {
        List<InventoryObject> droppingItems = new();
        var availableItemsToDrop = new List<ChestDropItem>();
        availableItemsToDrop.AddRange(allChestDropItems);

        var maxDropIndex = 0;
        foreach (var availableChestDropItem in availableItemsToDrop)
            maxDropIndex += availableChestDropItem.inventoryObjectDropChance;

        while (droppingItems.Count < droppingItemsCount)
        {
            var randomIndexInventoryObject = Random.Range(1, maxDropIndex + 1);

            var currentItemIndex = 0;
            foreach (var availableChestDropItem in availableItemsToDrop)
            {
                currentItemIndex += availableChestDropItem.inventoryObjectDropChance;

                if (currentItemIndex < randomIndexInventoryObject) continue;

                droppingItems.Add(availableChestDropItem.inventoryObjectToDrop);
                maxDropIndex -= availableChestDropItem.inventoryObjectDropChance;
                availableItemsToDrop.Remove(availableChestDropItem);
                break;
            }
        }

        return droppingItems;
    }

    public override bool IsCanInteract()
    {
        return isCanInteract && !isLootChestLocked;
    }
}

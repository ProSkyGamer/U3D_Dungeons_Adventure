using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class ShopItem : NetworkBehaviour
{
    #region Events

    public static event EventHandler OnStartTryBuyingItem;
    public static event EventHandler OnFinishBuyingItem;

    #endregion

    #region Enums

    public enum ShopItemType
    {
        Experience,
        Level,
        Relic,
        RelicReset
    }

    #endregion

    #region Variables & References

    [FormerlySerializedAs("soldItemType")] public ShopItemType soldShopItemType;
    public float boughtItemValue;
    public int coinsCost;
    public InventoryObject inventoryObjectToSold;
    public int maxBoughtCount;
    public bool isBoughtsUnlimited;
    public bool isObjectMustBeSelling;

    public TextTranslationsSO itemNameTextTranslationsSo;
    public Sprite boughtItemImage;

    #endregion

    #region Adding SHop Item To Merchant

    public void AddShopItemToMerchant(NetworkObjectReference merchantNetworkObjectReference,
        NetworkObjectReference addingPlayerNetworkObjectReference)
    {
        if (!IsServer) return;

        AddShopItemToMerchantClientRpc(merchantNetworkObjectReference, addingPlayerNetworkObjectReference);
    }

    [ClientRpc]
    private void AddShopItemToMerchantClientRpc(NetworkObjectReference merchantNetworkObjectReference,
        NetworkObjectReference addingPlayerNetworkObjectReference)
    {
        addingPlayerNetworkObjectReference.TryGet(out var addingPlayerNetworkObject);
        var addingPlayer = addingPlayerNetworkObject.GetComponent<PlayerController>();

        if (!addingPlayer.IsOwner) return;

        merchantNetworkObjectReference.TryGet(out var merchantNetworkObject);
        var merchant = merchantNetworkObject.GetComponent<Merchant>();

        merchant.AddSellingObject(this);
    }

    #endregion

    #region Buying Item

    public void TryBuyItem(PlayerController playerToBuy)
    {
        OnStartTryBuyingItem?.Invoke(this, EventArgs.Empty);

        var playerToBuyNetworkObjectReference = new NetworkObjectReference(playerToBuy.GetPlayerNetworkObject());

        TryBuyItemServerRpc(playerToBuyNetworkObjectReference);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryBuyItemServerRpc(NetworkObjectReference playerToBuyNetworkObjectReference)
    {
        playerToBuyNetworkObjectReference.TryGet(out var playerToBuyNetworkObject);
        var playerToBuy = playerToBuyNetworkObject.GetComponent<PlayerController>();

        if (!isBoughtsUnlimited && maxBoughtCount <= 0) return;
        if (!playerToBuy.IsEnoughCoins(coinsCost)) return;
        if (!IsHasOnInventory(playerToBuy, out var inventoryParent, out var inventorySlotNumber)) return;

        playerToBuy.SpendCoins(coinsCost);

        switch (soldShopItemType)
        {
            case ShopItemType.Experience:
                playerToBuy.ReceiveExperience((int)boughtItemValue);
                break;
            case ShopItemType.Level:
                var boughtItemCount = boughtItemValue;
                while (boughtItemCount > 0)
                {
                    var currentMultiplayer = boughtItemCount > 1 ? 1 : boughtItemCount;
                    boughtItemCount -= currentMultiplayer;

                    playerToBuy.ReceiveExperience(
                        (int)(playerToBuy.GetExperienceForCurrentLevel() * currentMultiplayer));
                }

                break;
            case ShopItemType.Relic:
                var newInventoryObjectTransform = Instantiate(inventoryObjectToSold.transform);
                var newInventoryObjectNetworkObject = newInventoryObjectTransform.GetComponent<NetworkObject>();
                newInventoryObjectNetworkObject.Spawn();
                var newInventoryObject = newInventoryObjectTransform.GetComponent<InventoryObject>();
                newInventoryObject.SetInventoryParent(playerToBuyNetworkObjectReference,
                    CharacterInventoryUI.InventoryType.PlayerRelicsInventory, true);

                break;
            case ShopItemType.RelicReset:
                var resettingInventoryObject = inventoryParent.GetInventoryObjectBySlot(inventorySlotNumber);
                resettingInventoryObject.RepairObject();
                resettingInventoryObject.TryNullifyRelicUsages();

                break;
        }

        FinishBoughtItemClientRpc(isBoughtsUnlimited ? 0 : maxBoughtCount - 1);
    }

    [ClientRpc]
    private void FinishBoughtItemClientRpc(int newBoughtCountValue)
    {
        maxBoughtCount = newBoughtCountValue;

        OnFinishBuyingItem?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Get Additional Info

    private bool IsHasOnInventory(PlayerController playerToCheck, out IInventoryParent inventoryParent,
        out int inventorySlot)
    {
        inventoryParent = null;
        inventorySlot = -1;

        var currentScanningPlayerInventory = playerToCheck.GetPlayerInventory();

        switch (soldShopItemType)
        {
            case ShopItemType.Experience:
                return true;
            case ShopItemType.Level:
                return true;
            case ShopItemType.Relic:
                return currentScanningPlayerInventory.IsHasAnyAvailableSlot();
        }

        if (!inventoryObjectToSold.TryGetRelicSo(out var soldRelicSo))
            Debug.LogError("Selling Not Relic");

        if (TryFindRelicInInventory(currentScanningPlayerInventory, soldRelicSo, out inventorySlot))
        {
            inventoryParent = currentScanningPlayerInventory;
            return true;
        }

        currentScanningPlayerInventory = playerToCheck.GetPlayerRelicsInventory();
        if (TryFindRelicInInventory(currentScanningPlayerInventory, soldRelicSo, out inventorySlot))
        {
            inventoryParent = currentScanningPlayerInventory;
            return true;
        }

        return false;
    }

    private bool TryFindRelicInInventory(IInventoryParent scanningInventory, RelicSO searchingRelic,
        out int foundInventorySlot)
    {
        foundInventorySlot = -1;

        for (var i = 0; i < scanningInventory.GetMaxSlotsCount(); i++)
            if (!scanningInventory.IsSlotNumberAvailable(i))
                if (scanningInventory.GetInventoryObjectBySlot(i).TryGetRelicSo(out var relicSo))
                    for (var j = 0; i < relicSo.relicApplyingEffects.Count; i++)
                    {
                        if (relicSo.relicApplyingEffects[j].appliedEffectType !=
                            searchingRelic.relicApplyingEffects[i].appliedEffectType ||
                            relicSo.relicApplyingEffects[j].effectPercentageScale !=
                            searchingRelic.relicApplyingEffects[i].effectPercentageScale ||
                            relicSo.relicApplyingEffects[j].maxUsagesLimit !=
                            searchingRelic.relicApplyingEffects[i].maxUsagesLimit)
                            continue;

                        foundInventorySlot = i;

                        return true;
                    }

        return false;
    }

    #endregion

    public static void ResetStaticData()
    {
        OnStartTryBuyingItem = null;
        OnFinishBuyingItem = null;
    }
}

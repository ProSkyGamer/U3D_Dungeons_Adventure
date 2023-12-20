using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemSingleUI : NetworkBehaviour
{
    [SerializeField] private Image shopItemImage;
    [SerializeField] private TextMeshProUGUI coinsCostText;
    [SerializeField] private TextMeshProUGUI maxSoldValue;
    [SerializeField] private TextTranslationSingleUI soldItemName;

    [SerializeField] private Transform unavailableItemTransform;
    [SerializeField] private Transform soldOutItemTransform;

    private Button shopItemButton;

    private ShopItemSO currentShopItem;

    private void Awake()
    {
        shopItemButton = GetComponent<Button>();
    }

    private void PlayerController_OnCoinsValueChange(object sender, EventArgs e)
    {
        if (currentShopItem != null)
            TryChangeItemVisual();
    }

    public void SetShopItem(ShopItemSO shopItem)
    {
        if (currentShopItem != null) return;

        currentShopItem = shopItem;

        coinsCostText.text = $"{currentShopItem.coinsCost} C";
        maxSoldValue.text =
            currentShopItem.isBoughtsUnlimited ? "INFINITY" : currentShopItem.maxBoughtCount.ToString();
        soldItemName.ChangeTextTranslationSO(currentShopItem.itemNameTextTranslationsSo);

        shopItemButton.onClick.AddListener(OnClick);

        PlayerController.Instance.OnCoinsValueChange += PlayerController_OnCoinsValueChange;

        TryChangeItemVisual();
    }

    private void OnClick()
    {
        if (IsSoldOut() || !IsEnoughCoinsToBuy() ||
            !IsHasOnInventory(out var inventoryParent, out var inventorySlotNumber)) return;

        if (PlayerController.Instance.IsEnoughCoins(currentShopItem.coinsCost))
        {
            PlayerController.Instance.SpendCoins(currentShopItem.coinsCost);
            currentShopItem.maxBoughtCount =
                currentShopItem.isBoughtsUnlimited ? 0 : currentShopItem.maxBoughtCount - 1;

            switch (currentShopItem.soldItemType)
            {
                case ShopItemSO.ItemType.Experience:
                    PlayerController.Instance.ReceiveExperience((int)currentShopItem.boughtItemValue);
                    break;
                case ShopItemSO.ItemType.Level:
                    var boughtItemCount = currentShopItem.boughtItemValue;
                    while (boughtItemCount > 0)
                    {
                        var currentMultiplayer = boughtItemCount > 1 ? 1 : boughtItemCount;
                        boughtItemCount -= currentMultiplayer;

                        PlayerController.Instance.ReceiveExperience(
                            (int)(PlayerController.Instance.GetExperienceForCurrentLevel() *
                                  currentMultiplayer));
                    }

                    break;
                case ShopItemSO.ItemType.Relic:
                    var playerNetworkObject = PlayerController.Instance.GetComponent<NetworkObject>();
                    var playerNetworkObjectReference = new NetworkObjectReference(playerNetworkObject);
                    var spawningInventoryObjectNetworkObject =
                        currentShopItem.inventoryObjectToSold.GetComponent<NetworkObject>();
                    var spawningInventoryObjectNetworkObjectReference =
                        new NetworkObjectReference(spawningInventoryObjectNetworkObject);
                    SpawnInventoryObjectServerRpc(playerNetworkObjectReference,
                        spawningInventoryObjectNetworkObjectReference);
                    break;
                case ShopItemSO.ItemType.RelicReset:
                    var resettingInventoryObject = inventoryParent.GetInventoryObjectBySlot(inventorySlotNumber);
                    resettingInventoryObject.RepairObject();
                    resettingInventoryObject.TryGetRelicSo(out var relicSo);
                    foreach (var relicApplyingEffect in relicSo.relicApplyingEffects)
                    {
                        if (!relicApplyingEffect.isUsagesLimited) continue;

                        relicApplyingEffect.currentUsages = 0;
                    }

                    break;
            }

            TryChangeItemVisual();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnInventoryObjectServerRpc(NetworkObjectReference playerInventoryNetworkObjectReference,
        NetworkObjectReference spawningInventoryObjectNetworkObjectReference)
    {
        spawningInventoryObjectNetworkObjectReference.TryGet(out var spawningInventoryObjectNetworkObject);
        var newInventoryObjectTransform = Instantiate(spawningInventoryObjectNetworkObject.transform);
        var newInventoryObjectNetworkObject = newInventoryObjectTransform.GetComponent<NetworkObject>();
        newInventoryObjectNetworkObject.Spawn();
        var newInventoryObject = newInventoryObjectTransform.GetComponent<InventoryObject>();
        newInventoryObject.SpawnInventoryObject();
        var spawnedInventoryObjectNetworkObjectReference = new NetworkObjectReference(newInventoryObjectNetworkObject);

        SpawnInventoryObjectClientRpc(playerInventoryNetworkObjectReference,
            spawnedInventoryObjectNetworkObjectReference);
    }

    [ClientRpc]
    private void SpawnInventoryObjectClientRpc(NetworkObjectReference playerInventoryNetworkObjectReference,
        NetworkObjectReference spawnedInventoryObjectNetworkObjectReference)
    {
        spawnedInventoryObjectNetworkObjectReference.TryGet(out var firstWeaponNetworkObject);
        var addedInventoryObject = firstWeaponNetworkObject.GetComponent<InventoryObject>();
        addedInventoryObject.SetInventoryParent(playerInventoryNetworkObjectReference,
            CharacterInventoryUI.InventoryType.PlayerRelicsInventory);
    }

    private void TryChangeItemVisual()
    {
        soldOutItemTransform.gameObject.SetActive(IsSoldOut());
        unavailableItemTransform.gameObject.SetActive(IsSoldOut() || !IsEnoughCoinsToBuy() ||
                                                      !IsHasOnInventory(out var _, out var _));
    }

    private bool IsSoldOut()
    {
        return !currentShopItem.isBoughtsUnlimited && currentShopItem.maxBoughtCount <= 0;
    }

    private bool IsHasOnInventory(out IInventoryParent inventoryParent, out int inventorySlot)
    {
        inventoryParent = null;
        inventorySlot = -1;

        var playerRelicsInventory = PlayerController.Instance.GetPlayerInventory();

        switch (currentShopItem.soldItemType)
        {
            case ShopItemSO.ItemType.Experience:
                return true;
            case ShopItemSO.ItemType.Level:
                return true;
            case ShopItemSO.ItemType.Relic:
                return playerRelicsInventory.IsHasAnyAvailableSlot();
        }

        if (!currentShopItem.inventoryObjectToSold.TryGetRelicSo(out var soldRelicSo))
            Debug.LogError("Selling Not Relic");

        for (var i = 0; i < playerRelicsInventory.GetMaxSlotsCount(); i++)
            if (!playerRelicsInventory.IsSlotNumberAvailable(i))
                if (playerRelicsInventory.GetInventoryObjectBySlot(i).TryGetRelicSo(out var relicSo))
                    for (var j = 0; i < relicSo.relicApplyingEffects.Count; i++)
                    {
                        if (relicSo.relicApplyingEffects[j].appliedEffectType !=
                            soldRelicSo.relicApplyingEffects[i].appliedEffectType ||
                            relicSo.relicApplyingEffects[j].effectPercentageScale !=
                            soldRelicSo.relicApplyingEffects[i].effectPercentageScale ||
                            relicSo.relicApplyingEffects[j].maxUsagesLimit !=
                            soldRelicSo.relicApplyingEffects[i].maxUsagesLimit)
                            continue;

                        inventoryParent = playerRelicsInventory;
                        inventorySlot = i;

                        return true;
                    }

        playerRelicsInventory = PlayerController.Instance.GetPlayerRelicsInventory();
        for (var i = 0; i < playerRelicsInventory.GetMaxSlotsCount(); i++)
            if (!playerRelicsInventory.IsSlotNumberAvailable(i))
                if (playerRelicsInventory.GetInventoryObjectBySlot(i).TryGetRelicSo(out var relicSo))
                    for (var j = 0; i < relicSo.relicApplyingEffects.Count; i++)
                    {
                        if (relicSo.relicApplyingEffects[j].appliedEffectType !=
                            soldRelicSo.relicApplyingEffects[i].appliedEffectType ||
                            relicSo.relicApplyingEffects[j].effectPercentageScale !=
                            soldRelicSo.relicApplyingEffects[i].effectPercentageScale ||
                            relicSo.relicApplyingEffects[j].maxUsagesLimit !=
                            soldRelicSo.relicApplyingEffects[i].maxUsagesLimit)
                            continue;

                        inventoryParent = playerRelicsInventory;
                        inventorySlot = i;

                        return true;
                    }

        return false;
    }

    private bool IsEnoughCoinsToBuy()
    {
        return PlayerController.Instance.IsEnoughCoins(currentShopItem.coinsCost);
    }

    private void OnDestroy()
    {
        PlayerController.Instance.OnCoinsValueChange -= PlayerController_OnCoinsValueChange;
    }
}

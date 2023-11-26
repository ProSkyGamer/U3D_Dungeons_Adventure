using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemSingleUI : MonoBehaviour
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
                    var relicNewInventoryObject = ScriptableObject.CreateInstance<InventoryObject>();
                    relicNewInventoryObject.SetInventoryObject(currentShopItem.inventoryObjectToSold);

                    relicNewInventoryObject.SetInventoryParent(PlayerController.Instance.GetPlayerInventory());
                    break;
                case ShopItemSO.ItemType.RelicReset:
                    inventoryParent.GetInventoryObjectBySlot(inventorySlotNumber).TryGetRelicSo(out var relicSo);
                    foreach (var relicBuff in relicSo.relicBuffs)
                    {
                        if (!relicBuff.isHasLimit) continue;

                        relicBuff.currentUsages = 0;
                    }

                    break;
            }

            TryChangeItemVisual();
        }
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
                    for (var j = 0; i < relicSo.relicBuffs.Count; i++)
                    {
                        if (relicSo.relicBuffs[j].relicBuffType != soldRelicSo.relicBuffs[i].relicBuffType ||
                            relicSo.relicBuffs[j].relicBuffScale != soldRelicSo.relicBuffs[i].relicBuffScale ||
                            relicSo.relicBuffs[j].maxUsagesLimit != soldRelicSo.relicBuffs[i].maxUsagesLimit)
                            continue;

                        inventoryParent = playerRelicsInventory;
                        inventorySlot = i;

                        return true;
                    }

        playerRelicsInventory = PlayerController.Instance.GetPlayerRelicsInventory();
        for (var i = 0; i < playerRelicsInventory.GetMaxSlotsCount(); i++)
            if (!playerRelicsInventory.IsSlotNumberAvailable(i))
                if (playerRelicsInventory.GetInventoryObjectBySlot(i).TryGetRelicSo(out var relicSo))
                    for (var j = 0; i < relicSo.relicBuffs.Count; i++)
                    {
                        if (relicSo.relicBuffs[j].relicBuffType != soldRelicSo.relicBuffs[i].relicBuffType ||
                            relicSo.relicBuffs[j].relicBuffScale != soldRelicSo.relicBuffs[i].relicBuffScale ||
                            relicSo.relicBuffs[j].maxUsagesLimit != soldRelicSo.relicBuffs[i].maxUsagesLimit)
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

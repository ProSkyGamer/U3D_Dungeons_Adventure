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

    private ShopItem currentShopItem;

    private void Awake()
    {
        shopItemButton = GetComponent<Button>();
    }

    public void SetShopItem(ShopItem shopItem)
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

    private void PlayerController_OnCoinsValueChange(object sender, EventArgs e)
    {
        if (currentShopItem != null)
            TryChangeItemVisual();
    }

    private void OnClick()
    {
        currentShopItem.TryBuyItem(PlayerController.Instance);

        TryChangeItemVisual();
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

        switch (currentShopItem.soldShopItemType)
        {
            case ShopItem.ShopItemType.Experience:
                return true;
            case ShopItem.ShopItemType.Level:
                return true;
            case ShopItem.ShopItemType.Relic:
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
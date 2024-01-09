using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItemSingleUI : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Events & Event Args

    public static event EventHandler<OnDisplaySellingItemDescriptionEventArgs> OnDisplaySellingItemDescription;

    public class OnDisplaySellingItemDescriptionEventArgs : EventArgs
    {
        public ShopItem sellingShopItem;
    }

    public static event EventHandler OnStopDisplayingSellingItemDescription;

    #endregion

    #region Variables & References

    [SerializeField] private Image shopItemImage;
    [SerializeField] private TextMeshProUGUI coinsCostText;
    [SerializeField] private TextMeshProUGUI leftText;
    [SerializeField] private TextTranslationsSO leftTextTranslationSo;
    [SerializeField] private TextTranslationSingleUI soldItemName;

    [SerializeField] private Transform unavailableItemTransform;
    [SerializeField] private Transform soldOutItemTransform;

    private Button shopItemButton;

    private ShopItem currentShopItem;

    #endregion

    #region Initialization

    private void Awake()
    {
        shopItemButton = GetComponent<Button>();
    }

    #endregion

    #region Shop Item Initialization

    public void SetShopItem(ShopItem shopItem)
    {
        if (currentShopItem != null) return;

        currentShopItem = shopItem;

        coinsCostText.text = $"{currentShopItem.coinsCost} C";

        soldItemName.ChangeTextTranslationSO(currentShopItem.itemNameTextTranslationsSo);
        var leftCountText = TextTranslationController.GetTextFromTextTranslationSOByLanguage(
            TextTranslationController.GetCurrentLanguage(), leftTextTranslationSo);
        var fullLeftCountText = string.Format(leftCountText, currentShopItem.maxBoughtCount);
        leftText.text = fullLeftCountText;

        leftText.gameObject.SetActive(!currentShopItem.isBoughtsUnlimited);

        shopItemButton.onClick.AddListener(OnClick);

        PlayerController.Instance.OnCoinsValueChange += PlayerController_OnCoinsValueChange;

        TryChangeItemVisual();
    }

    private void PlayerController_OnCoinsValueChange(object sender, EventArgs e)
    {
        if (currentShopItem != null)
            TryChangeItemVisual();
    }

    #endregion

    #region Selling

    private void OnClick()
    {
        currentShopItem.TryBuyItem(PlayerController.Instance);

        ShopItem.OnFinishBuyingItem += ShopItemOnFinishBuyingItem;
    }

    private void ShopItemOnFinishBuyingItem(object sender, EventArgs e)
    {
        TryChangeItemVisual();
    }

    #endregion

    #region Visual

    private void TryChangeItemVisual()
    {
        soldOutItemTransform.gameObject.SetActive(IsSoldOut());
        unavailableItemTransform.gameObject.SetActive(IsSoldOut() || !IsEnoughCoinsToBuy() ||
                                                      !IsHasOnInventory(out var _, out var _));
    }

    #endregion

    #region Shop Item Description

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnDisplaySellingItemDescription?.Invoke(this, new OnDisplaySellingItemDescriptionEventArgs
        {
            sellingShopItem = currentShopItem
        });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnStopDisplayingSellingItemDescription?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Get Shop Item Data

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

    #endregion

    private void OnDestroy()
    {
        PlayerController.Instance.OnCoinsValueChange -= PlayerController_OnCoinsValueChange;
    }

    public static void ResetStaticData()
    {
        OnDisplaySellingItemDescription = null;
        OnStopDisplayingSellingItemDescription = null;
    }
}

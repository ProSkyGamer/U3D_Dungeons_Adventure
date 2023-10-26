using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemSingleUI : MonoBehaviour
{
    [SerializeField] private Image shopItemImage;
    [SerializeField] private TextMeshProUGUI coinsCostText;
    [SerializeField] private TextMeshProUGUI maxSoldValue;
    [SerializeField] private TextMeshProUGUI soldItemName;

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
        soldItemName.text = currentShopItem.itemName;

        shopItemButton.onClick.AddListener(OnClick);

        PlayerController.Instance.OnCoinsValueChange += PlayerController_OnCoinsValueChange;

        TryChangeItemVisual();
    }

    private void OnClick()
    {
        if (IsSoldOut() || !IsEnoughCoinsToBuy()) return;

        if (PlayerController.Instance.IsEnoughCoins(currentShopItem.coinsCost))
        {
            PlayerController.Instance.SpendCoins(currentShopItem.coinsCost);
            currentShopItem.maxBoughtCount =
                currentShopItem.isBoughtsUnlimited ? 0 : currentShopItem.maxBoughtCount - 1;

            switch (currentShopItem.soldItemType)
            {
                case ShopItemSO.ItemType.Experience:
                    PlayerController.Instance.ReceiveExperience(currentShopItem.boughtItemValue);
                    break;
                case ShopItemSO.ItemType.Level:
                    PlayerController.Instance.ReceiveExperience(
                        PlayerController.Instance.GetExperienceForCurrentLevel());
                    break;
            }

            TryChangeItemVisual();
        }
    }

    private void TryChangeItemVisual()
    {
        soldOutItemTransform.gameObject.SetActive(IsSoldOut());
        unavailableItemTransform.gameObject.SetActive(IsSoldOut() || !IsEnoughCoinsToBuy());
    }

    private bool IsSoldOut()
    {
        return !currentShopItem.isBoughtsUnlimited && currentShopItem.maxBoughtCount <= 0;
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

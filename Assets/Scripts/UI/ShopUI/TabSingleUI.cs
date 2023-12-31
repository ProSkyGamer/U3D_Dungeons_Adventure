using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabSingleUI : MonoBehaviour
{
    [SerializeField] private TextTranslationSingleUI tabNameTextTranslationSingleUI;

    [SerializeField] private Transform shopItemsGrid;
    [SerializeField] private Transform shopItemPrefab;

    private Button tabButton;

    private List<ShopItem> allShopItems = new();

    private void Awake()
    {
        tabButton = GetComponent<Button>();

        shopItemPrefab.gameObject.SetActive(false);
    }

    private void Start()
    {
        ShopUI.Instance.OnShopClose += ShopUI_OnShopClose;
    }

    private void ShopUI_OnShopClose(object sender, EventArgs e)
    {
        ClearShopItemsGrid();
    }

    public void SetTabName(TextTranslationsSO tabNameTextTranslationsSo)
    {
        tabNameTextTranslationSingleUI.ChangeTextTranslationSO(tabNameTextTranslationsSo);
    }

    public void SetShopItemsList(List<ShopItem> tabShopItems)
    {
        if (allShopItems.Count != 0) return;

        allShopItems = tabShopItems;

        tabButton.onClick.AddListener(ShowTabShopItems);
    }

    public void ShowTabShopItems()
    {
        ClearShopItemsGrid();

        foreach (var shopItem in allShopItems)
        {
            var createdShopItem = Instantiate(shopItemPrefab, shopItemsGrid);
            createdShopItem.gameObject.SetActive(true);

            var createdShopItemSingleUI = createdShopItem.GetComponentInChildren<ShopItemSingleUI>();
            createdShopItemSingleUI.SetShopItem(shopItem);
        }
    }

    private void ClearShopItemsGrid()
    {
        List<Transform> allShopItems = new();
        allShopItems.AddRange(shopItemsGrid.GetComponentsInChildren<Transform>());

        foreach (var shopItem in allShopItems)
        {
            if (shopItem == shopItemsGrid || shopItem == shopItemPrefab) continue;

            Destroy(shopItem.gameObject);
        }
    }
}
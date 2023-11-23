using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Merchant : InteractableItem
{
    [Serializable]
    public class ShopTab
    {
        public TextTranslationsSO shopTabNameTextTranslationsSo;
        public List<ShopItemSO> shopTabItems = new();

        public bool isAllItemsWillSold;
        public int maxRandomItems = 3;
    }

    [SerializeField] private List<ShopTab> allShopTabs = new();

    private readonly List<ShopTab> currentShopTabs = new();

    private void Awake()
    {
        foreach (var shopTab in allShopTabs)
            if (shopTab.shopTabItems.Count <= shopTab.maxRandomItems || shopTab.isAllItemsWillSold)
            {
                currentShopTabs.Add(shopTab);
            }
            else
            {
                var createdShopTab = new ShopTab();
                createdShopTab.shopTabNameTextTranslationsSo = shopTab.shopTabNameTextTranslationsSo;

                for (var i = 0; i < shopTab.shopTabItems.Count; i++)
                {
                    var shopTabItem = shopTab.shopTabItems[i];
                    if (!shopTabItem.isObjectMustBeSelling) continue;

                    var newShopItem = ScriptableObject.CreateInstance<ShopItemSO>();
                    newShopItem.SetShopItem(shopTabItem);

                    createdShopTab.shopTabItems.Add(newShopItem);
                    shopTab.shopTabItems.Remove(shopTabItem);
                    i--;
                }

                for (var i = 0; i < shopTab.maxRandomItems; i++)
                {
                    var chosenItemIndex = Random.Range(0, shopTab.shopTabItems.Count);
                    var chosenItem = shopTab.shopTabItems[chosenItemIndex];

                    if (createdShopTab.shopTabItems.Contains(chosenItem)) continue;

                    var newShopItem = ScriptableObject.CreateInstance<ShopItemSO>();
                    newShopItem.SetShopItem(chosenItem);

                    createdShopTab.shopTabItems.Add(newShopItem);
                    shopTab.shopTabItems.Remove(chosenItem);
                }

                currentShopTabs.Add(createdShopTab);
            }
    }

    public override void OnInteract(PlayerController player)
    {
        base.OnInteract(player);

        isCanInteract = false;

        ShopUI.Instance.ShowShop(currentShopTabs);
        ShopUI.Instance.OnShopClose += ShopUI_OnShopClose;
    }

    private void ShopUI_OnShopClose(object sender, EventArgs e)
    {
        isCanInteract = true;
    }

    public override bool IsCanInteract()
    {
        return isCanInteract;
    }
}

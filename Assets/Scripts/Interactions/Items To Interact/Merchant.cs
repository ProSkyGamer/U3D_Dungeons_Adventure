using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Merchant : InteractableItem
{
    #region Created Classes

    [Serializable]
    public class ShopTab
    {
        public List<ShopItem.ShopItemType> sellingShopItemsCategories = new();
        public TextTranslationsSO shopTabNameTextTranslationsSo;
        public List<ShopItem> shopTabItems = new();

        public bool isAllItemsWillSold;
        public int maxRandomItems = 3;
    }

    #endregion

    #region Variables & References

    [SerializeField] private List<ShopTab> allShopTabs = new();

    [SerializeField] private List<ShopTab> currentShopTabs = new();

    private bool isFirstUpdate;

    #endregion

    #region Initialization

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        foreach (var shopTab in allShopTabs)
        {
            var newCurrentShopTab = new ShopTab
            {
                sellingShopItemsCategories = shopTab.sellingShopItemsCategories,
                shopTabNameTextTranslationsSo = shopTab.shopTabNameTextTranslationsSo
            };
            currentShopTabs.Add(newCurrentShopTab);
        }

        if (!IsServer) return;

        isFirstUpdate = true;
    }

    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
            InitializeShop();
        }
    }

    private void InitializeShop()
    {
        if (!IsServer) return;

        var allConnectedPlayers = AllConnectedPlayers.Instance.GetAllPlayerControllers();

        var merchantNetworkObject = GetComponent<NetworkObject>();
        var merchantNetworkObjectReference = new NetworkObjectReference(merchantNetworkObject);

        foreach (var connectedPlayerController in allConnectedPlayers)
        {
            var connectedPlayerControllerNetworkObjectReference =
                new NetworkObjectReference(connectedPlayerController.GetPlayerNetworkObject());

            foreach (var shopTab in allShopTabs)
                if (shopTab.shopTabItems.Count <= shopTab.maxRandomItems || shopTab.isAllItemsWillSold)
                {
                    foreach (var shopTabItem in shopTab.shopTabItems)
                    {
                        var newShopItemTransform = CreateShopItem(shopTabItem);

                        var newShopItem = newShopItemTransform.GetComponent<ShopItem>();
                        newShopItem.AddShopItemToMerchant(merchantNetworkObjectReference,
                            connectedPlayerControllerNetworkObjectReference);
                    }
                }
                else
                {
                    var tempShopTabItems = new List<ShopItem>();
                    tempShopTabItems.AddRange(shopTab.shopTabItems);
                    for (var i = 0; i < tempShopTabItems.Count; i++)
                    {
                        var shopTabItem = tempShopTabItems[i];
                        if (!shopTabItem.isObjectMustBeSelling) continue;

                        var newShopItemTransform = CreateShopItem(shopTabItem);

                        var newShopItem = newShopItemTransform.GetComponent<ShopItem>();
                        newShopItem.AddShopItemToMerchant(merchantNetworkObjectReference,
                            connectedPlayerControllerNetworkObjectReference);

                        tempShopTabItems.RemoveAt(i);
                        i--;
                    }

                    for (var i = 0; i < shopTab.maxRandomItems; i++)
                    {
                        var chosenItemIndex = Random.Range(0, tempShopTabItems.Count);
                        var chosenItem = tempShopTabItems[chosenItemIndex];

                        var newShopItemTransform = CreateShopItem(chosenItem);

                        var newShopItem = newShopItemTransform.GetComponent<ShopItem>();
                        newShopItem.AddShopItemToMerchant(merchantNetworkObjectReference,
                            connectedPlayerControllerNetworkObjectReference);

                        tempShopTabItems.Remove(chosenItem);
                    }
                }
        }
    }

    #endregion

    #region Shop Items Initialization

    private Transform CreateShopItem(ShopItem shopItemPrefab)
    {
        if (!IsServer) return null;

        var newShopItemTransform = Instantiate(shopItemPrefab.transform, transform);
        var newShopItemNetworkObject = newShopItemTransform.GetComponent<NetworkObject>();
        newShopItemNetworkObject.Spawn();

        return newShopItemTransform;
    }

    #endregion

    #region Interactable Item

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

    #endregion

    #region Shop Initialization

    public void AddSellingObject(ShopItem addingShopItem)
    {
        foreach (var shopTab in currentShopTabs)
        {
            if (!shopTab.sellingShopItemsCategories.Contains(addingShopItem.soldShopItemType)) continue;

            if (shopTab.shopTabItems.Contains(addingShopItem)) return;

            shopTab.shopTabItems.Add(addingShopItem);
            return;
        }
    }

    #endregion
}

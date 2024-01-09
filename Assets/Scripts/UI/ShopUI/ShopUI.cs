using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ShopUI : NetworkBehaviour
{
    public static ShopUI Instance { get; private set; }

    #region Events

    public event EventHandler OnShopOpen;
    public event EventHandler OnShopClose;

    #endregion

    #region Variables & References

    [SerializeField] private Button shopCloseButton;

    [SerializeField] private Transform shopTabTransform;
    [SerializeField] private Transform waitingForServerResponseNotificationTransform;
    [SerializeField] private Transform inventoriesTabTransform;

    [SerializeField] private Button inventoriesButton;
    [SerializeField] private Button inventoriesTabCloseButton;
    [SerializeField] private List<CharacterInventoryUI> allShownInventories = new();

    [SerializeField] private Transform tabsGrid;
    [SerializeField] private Transform tabPrefab;

    [SerializeField] private TextMeshProUGUI coinsValueText;


    [FormerlySerializedAs("slotDescriptionPrefab")] [SerializeField] private Transform sellingItemDescriptionPrefab;

    private bool isFirstUpdate;

    private Transform currentSellingItemDescription;

    private bool isShown;

    #endregion

    #region Initialization & Subscribed events

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        shopCloseButton.onClick.AddListener(Hide);
        inventoriesTabCloseButton.onClick.AddListener(HideInventories);

        inventoriesButton.onClick.AddListener(ShowInventories);

        tabPrefab.gameObject.SetActive(false);

        ShopItemSingleUI.OnDisplaySellingItemDescription += ShopItemSingleUI_OnDisplaySellingItemDescription;
        ShopItemSingleUI.OnStopDisplayingSellingItemDescription +=
            ShopItemSingleUI_OnStopDisplayingSellingItemDescription;

        ShopItem.OnStartTryBuyingItem += ShopItem_OnStartTryBuyingItem;
        ShopItem.OnFinishBuyingItem += ShopItem_OnFinishBuyingItem;

        PlayerController.OnPlayerSpawned += PlayerController_OnPlayerSpawned;
    }

    private void ShopItem_OnFinishBuyingItem(object sender, EventArgs e)
    {
        waitingForServerResponseNotificationTransform.gameObject.SetActive(false);

        if (isShown)
            GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
    }

    private void ShopItem_OnStartTryBuyingItem(object sender, EventArgs e)
    {
        waitingForServerResponseNotificationTransform.gameObject.SetActive(true);

        if (isShown)
            GameInput.Instance.OnPauseAction -= GameInput_OnPauseAction;
    }

    private void ShopItemSingleUI_OnDisplaySellingItemDescription(object sender,
        ShopItemSingleUI.OnDisplaySellingItemDescriptionEventArgs e)
    {
        var newSellingItemDescription = Instantiate(sellingItemDescriptionPrefab,
            GameInput.Instance.GetCurrentMousePosition(), Quaternion.identity,
            transform.GetComponentsInParent<Transform>()[1]);
        var shopItemDescription = newSellingItemDescription.GetComponent<ShopItemDescription>();
        shopItemDescription.SetShopItem(e.sellingShopItem);

        currentSellingItemDescription = newSellingItemDescription;
    }

    private void ShopItemSingleUI_OnStopDisplayingSellingItemDescription(object sender, EventArgs e)
    {
        if (currentSellingItemDescription != null)
        {
            Destroy(currentSellingItemDescription.gameObject);
            currentSellingItemDescription = null;
        }
    }

    private void PlayerController_OnPlayerSpawned(object sender, EventArgs e)
    {
        PlayerController.Instance.OnCoinsValueChange += PlayerController_OnCoinsValueChange;

        isFirstUpdate = true;

        PlayerController.OnPlayerSpawned -= PlayerController_OnPlayerSpawned;
    }

    private void PlayerController_OnCoinsValueChange(object sender, EventArgs e)
    {
        coinsValueText.text = PlayerController.Instance.GetCurrentCoinsValue().ToString();
    }

    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;

            waitingForServerResponseNotificationTransform.gameObject.SetActive(false);

            HideInventories();

            Hide();
        }
    }

    #endregion

    #region Visual

    public void ShowShop(List<Merchant.ShopTab> showTabsToShow)
    {
        gameObject.SetActive(true);

        ClearTabsGrid();

        coinsValueText.text = PlayerController.Instance.GetCurrentCoinsValue().ToString();

        for (var i = 0; i < showTabsToShow.Count; i++)
        {
            var createdTab = Instantiate(tabPrefab, tabsGrid);
            createdTab.gameObject.SetActive(true);

            var createdTabSingleUI = createdTab.GetComponent<TabSingleUI>();

            createdTabSingleUI.SetTabName(showTabsToShow[i].shopTabNameTextTranslationsSo);
            createdTabSingleUI.SetShopItemsList(showTabsToShow[i].shopTabItems);

            if (i == 0)
                createdTabSingleUI.ShowTabShopItems();
        }

        OnShopOpen?.Invoke(this, EventArgs.Empty);

        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;

        isShown = true;
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        Hide();

        GameInput.Instance.OnPauseAction -= GameInput_OnPauseAction;
    }

    private void ClearTabsGrid()
    {
        List<Transform> allTabs = new();
        allTabs.AddRange(tabsGrid.GetComponentsInChildren<Transform>());

        foreach (var tab in allTabs)
        {
            if (tab == tabsGrid || tab == tabPrefab) continue;

            Destroy(tab.gameObject);
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);

        if (currentSellingItemDescription != null)
        {
            Destroy(currentSellingItemDescription.gameObject);
            currentSellingItemDescription = null;
        }

        ClearTabsGrid();

        OnShopClose?.Invoke(this, EventArgs.Empty);

        isShown = false;
    }

    private void ShowInventories()
    {
        shopTabTransform.gameObject.SetActive(false);
        inventoriesTabTransform.gameObject.SetActive(true);

        foreach (var shownInventory in allShownInventories) shownInventory.UpdateInventory();
    }

    private void HideInventories()
    {
        inventoriesTabTransform.gameObject.SetActive(false);
        shopTabTransform.gameObject.SetActive(true);
    }

    #endregion
}

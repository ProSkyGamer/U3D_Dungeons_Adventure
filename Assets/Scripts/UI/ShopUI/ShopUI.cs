using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    public event EventHandler OnShopOpen;
    public event EventHandler OnShopClose;

    [SerializeField] private Button closeButton;

    [SerializeField] private Transform tabsGrid;
    [SerializeField] private Transform tabPrefab;

    [SerializeField] private TextMeshProUGUI coinsValueText;

    private bool isFirstUpdate = true;

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        closeButton.onClick.AddListener(Hide);

        tabPrefab.gameObject.SetActive(false);
    }

    private void Start()
    {
        PlayerController.Instance.OnCoinsValueChange += PlayerController_OnCoinsValueChange;
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
            Hide();
        }
    }

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
        ClearTabsGrid();
        OnShopClose?.Invoke(this, EventArgs.Empty);
    }
}

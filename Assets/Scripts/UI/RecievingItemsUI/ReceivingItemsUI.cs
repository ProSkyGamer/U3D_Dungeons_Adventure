using System;
using System.Collections.Generic;
using UnityEngine;

public class ReceivingItemsUI : MonoBehaviour
{
    public static ReceivingItemsUI Instance { get; private set; }

    [SerializeField] private int maxShowingItems = 6;
    [SerializeField] private Transform receivingItemsLayoutGroup;
    [SerializeField] private Transform receivingItemSinglePrefab;
    [SerializeField] private float showingReceivedItemTime = 4.5f;
    [SerializeField] private float hidingReceivedItemTime = 1.5f;

    private int currentDisplayedReceivingItemsCount;

    private class ReceivingItem
    {
        public Sprite receivingItemSprite;
        public TextTranslationsSO receivedItemNameTextTranslationSo;
        public int receivingItemValue;
        public float showingReceivedItemTime;
        public float hidingReceivedItemTime;
        public int itemPriority;
    }

    private readonly List<ReceivingItem> receivingItemsWaitingForShowing = new();

    private bool isFirstUpdate = true;

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        receivingItemSinglePrefab.gameObject.SetActive(false);
    }

    private void Start()
    {
        GiveCoinsUI.OnInterfaceShown += OnOtherInterfaceShown;
        GiveCoinsUI.OnInterfaceHidden += OnOtherInterfaceHidden;

        PauseUI.OnInterfaceShown += OnOtherInterfaceShown;
        PauseUI.OnInterfaceHidden += OnOtherInterfaceHidden;

        ShopUI.Instance.OnShopOpen += OnOtherInterfaceShown;
        ShopUI.Instance.OnShopClose += OnOtherInterfaceHidden;

        CharacterUI.OnCharacterUIOpen += OnOtherInterfaceShown;
        CharacterUI.OnCharacterUIClose += OnOtherInterfaceHidden;
    }

    private void OnOtherInterfaceHidden(object sender, EventArgs e)
    {
        Show();
    }

    private void OnOtherInterfaceShown(object sender, EventArgs e)
    {
        Hide();
    }

    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
            Hide();
        }

        if (receivingItemsWaitingForShowing.Count <= 0) return;
        if (maxShowingItems < currentDisplayedReceivingItemsCount) return;

        ReceivingItem newShowingReceivingItem = null;

        foreach (var receivingItem in receivingItemsWaitingForShowing)
            if (newShowingReceivingItem == null || newShowingReceivingItem.itemPriority > receivingItem.itemPriority)
                newShowingReceivingItem = receivingItem;


        var newReceivingItemTransform = Instantiate(receivingItemSinglePrefab, receivingItemsLayoutGroup);
        newReceivingItemTransform.gameObject.SetActive(true);
        var newReceivingItemSingle = newReceivingItemTransform.GetComponent<ReceivingItemSingle>();
        newReceivingItemSingle.SetReceivedItem(newShowingReceivingItem.receivingItemSprite,
            newShowingReceivingItem.receivedItemNameTextTranslationSo,
            newShowingReceivingItem.receivingItemValue, newShowingReceivingItem.showingReceivedItemTime,
            newShowingReceivingItem.hidingReceivedItemTime);
        newReceivingItemSingle.OnItemDestroyed += NewReceivingItemSingle_OnItemDestroyed;

        currentDisplayedReceivingItemsCount++;

        receivingItemsWaitingForShowing.Remove(newShowingReceivingItem);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void AddReceivedItem(Sprite receivingItemSprite, TextTranslationsSO receivedItemNameTextTranslationSo,
        int receivingItemValue, int itemPriority)
    {
        if (currentDisplayedReceivingItemsCount <= 0)
            Show();

        var newReceivedItem = new ReceivingItem
        {
            receivingItemSprite = receivingItemSprite,
            receivedItemNameTextTranslationSo = receivedItemNameTextTranslationSo,
            receivingItemValue = receivingItemValue,
            showingReceivedItemTime = showingReceivedItemTime,
            hidingReceivedItemTime = hidingReceivedItemTime,
            itemPriority = itemPriority
        };

        receivingItemsWaitingForShowing.Add(newReceivedItem);
    }

    private void NewReceivingItemSingle_OnItemDestroyed(object sender, EventArgs e)
    {
        var newReceivingItemSingle = sender as ReceivingItemSingle;
        if (newReceivingItemSingle == null) return;

        currentDisplayedReceivingItemsCount--;

        if (currentDisplayedReceivingItemsCount <= 0)
            Hide();

        newReceivingItemSingle.OnItemDestroyed += NewReceivingItemSingle_OnItemDestroyed;
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotSingleUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public static event EventHandler<OnStartItemDraggingEventArgs> OnStartItemDragging;

    public class OnStartItemDraggingEventArgs : EventArgs
    {
        public InventoryObject draggingInventoryObject;
    }

    public static event EventHandler OnCurrentSlotSelected;
    public static event EventHandler<OnDisplaySlotDescriptionEventArgs> OnDisplaySlotDescription;

    public class OnDisplaySlotDescriptionEventArgs : EventArgs
    {
        public InventoryObject inventoryObject;
        public CharacterInventoryUI displayedInventory;
    }

    public static event EventHandler OnStopDisplaySlotDescription;

    public static event EventHandler<OnDisplaySlotInteractButtonsEventArgs> OnDisplaySlotInteractButtons;

    public class OnDisplaySlotInteractButtonsEventArgs : EventArgs
    {
        public int slotNumber;
        public InventoryObject inventoryObject;
        public CharacterInventoryUI displayedInventory;
    }

    private CharacterInventoryUI.InventoryType inventoryType;

    [SerializeField] private Image inventorySlotImage;
    [SerializeField] private TextMeshProUGUI inventorySlotText;
    private TextTranslationSingleUI inventorySlotTextTranslationSingleUI;
    [SerializeField] private Transform lockedInventorySlotTransform;
    [SerializeField] private Transform selectedInventorySlotTransform;

    private int slotNumber = -1;
    private InventoryObject storedItem;

    private bool isCurrentlyDragging;
    private bool isCurrentSlotSelected;
    private bool isInteractable;
    private bool isShowingName;

    private bool isSubscribedToShowingButton;
    private bool isShowingButtons;

    private CharacterInventoryUI inventoryParent;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isInteractable) return;
        if (storedItem == null) return;
        if (isShowingButtons) return;

        if (inventoryType == CharacterInventoryUI.InventoryType.PlayerWeaponInventory &&
            PlayerController.Instance.GetPlayerWeaponsInventory().GetCurrentInventoryObjectsCount() <= 1) return;

        StartDraggingItem();
    }

    public void SetStarterData(int newSlotNumber, CharacterInventoryUI.InventoryType newInventoryType,
        bool isSlotInteractable, bool isShowingObjectName, CharacterInventoryUI inventoryUI)
    {
        if (slotNumber != -1) return;

        slotNumber = newSlotNumber;
        inventoryType = newInventoryType;
        isInteractable = isSlotInteractable;
        isShowingName = isShowingObjectName;
        inventoryParent = inventoryUI;
        UpdateVisual();
    }

    private void Start()
    {
        inventorySlotTextTranslationSingleUI = inventorySlotText.GetComponent<TextTranslationSingleUI>();

        OnStartItemDragging += InventorySlotSingleUI_OnStartItemDragging;
        OnDisplaySlotDescription += InventorySingleUI_OnDisplaySlotDescription;
    }

    private void InventorySingleUI_OnDisplaySlotDescription(object sender, OnDisplaySlotDescriptionEventArgs e)
    {
        isShowingButtons = false;
    }

    private void InventorySlotSingleUI_OnStartItemDragging(object sender, OnStartItemDraggingEventArgs e)
    {
        if (!isInteractable) return;

        isCurrentlyDragging = true;
        OnCurrentSlotSelected += InventorySlotSingleUI_OnCurrentSlotSelected;
        CharacterInventoryUI.OnStopItemDragging += StatsTabUI_OnStopItemDragging;

        var slotSingleUI = sender as InventorySlotSingleUI;

        if (!IsCurrentDraggingObjectMatchesCurrentInventoryType())
            lockedInventorySlotTransform.gameObject.SetActive(true);

        if (slotSingleUI != this) return;

        isCurrentSlotSelected = true;
        selectedInventorySlotTransform.gameObject.SetActive(true);
    }

    private void StatsTabUI_OnStopItemDragging(object sender, EventArgs e)
    {
        if (!isInteractable) return;

        isCurrentSlotSelected = false;
        isCurrentlyDragging = false;
        lockedInventorySlotTransform.gameObject.SetActive(false);
        selectedInventorySlotTransform.gameObject.SetActive(false);

        CharacterInventoryUI.OnStopItemDragging -= StatsTabUI_OnStopItemDragging;
    }

    private void InventorySlotSingleUI_OnCurrentSlotSelected(object sender, EventArgs e)
    {
        if (!isInteractable) return;

        var slotSingleUI = sender as InventorySlotSingleUI;

        if (slotSingleUI == this) return;

        isCurrentSlotSelected = false;
        selectedInventorySlotTransform.gameObject.SetActive(false);
    }

    private void StartDraggingItem()
    {
        if (!isInteractable) return;

        OnStartItemDragging?.Invoke(this, new OnStartItemDraggingEventArgs
        {
            draggingInventoryObject = storedItem
        });

        RemoveItem();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable) return;

        if (storedItem != null)
        {
            OnDisplaySlotDescription?.Invoke(this, new OnDisplaySlotDescriptionEventArgs
            {
                inventoryObject = storedItem, displayedInventory = inventoryParent
            });
            GameInput.Instance.OnInventorySlotInteractAction += GameInput_OnInventorySlotInteractAction;
            isSubscribedToShowingButton = true;
        }

        if (!isCurrentlyDragging) return;

        if (!IsCurrentDraggingObjectMatchesCurrentInventoryType()) return;

        selectedInventorySlotTransform.gameObject.SetActive(true);
        isCurrentSlotSelected = true;
        OnCurrentSlotSelected?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnInventorySlotInteractAction(object sender, EventArgs e)
    {
        isShowingButtons = true;
        OnDisplaySlotInteractButtons?.Invoke(this, new OnDisplaySlotInteractButtonsEventArgs
        {
            inventoryObject = storedItem, displayedInventory = inventoryParent, slotNumber = slotNumber
        });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnStopDisplaySlotDescription?.Invoke(this, EventArgs.Empty);

        if (isSubscribedToShowingButton)
        {
            GameInput.Instance.OnInventorySlotInteractAction -= GameInput_OnInventorySlotInteractAction;
            isSubscribedToShowingButton = false;
        }
    }

    public void StoreItem(InventoryObject inventoryObject)
    {
        storedItem = inventoryObject;

        UpdateVisual();
    }

    public void RemoveItem()
    {
        storedItem = null;

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (storedItem != null)
        {
            inventorySlotImage.gameObject.SetActive(true);
            inventorySlotImage.sprite = storedItem.GetInventoryObjectSprite();

            if (isShowingName)
            {
                inventorySlotText.gameObject.SetActive(true);
                inventorySlotTextTranslationSingleUI.ChangeTextTranslationSO(storedItem
                    .GetInventoryObjectNameTextTranslationSo());
            }
        }
        else
        {
            inventorySlotImage.gameObject.SetActive(false);
            inventorySlotText.gameObject.SetActive(false);
        }
    }

    private bool IsCurrentDraggingObjectMatchesCurrentInventoryType()
    {
        switch (inventoryType)
        {
            default:
                return false;
            case CharacterInventoryUI.InventoryType.PlayerInventory:
                return true;
            case CharacterInventoryUI.InventoryType.PlayerWeaponInventory:
                return CharacterInventoryUI.GetCurrentDraggingObject().TryGetWeaponSo(out _);
            case CharacterInventoryUI.InventoryType.PlayerRelicsInventory:
                return CharacterInventoryUI.GetCurrentDraggingObject().TryGetRelicSo(out _);
        }
    }

    public bool IsHasStoredItem()
    {
        return storedItem != null;
    }

    public int GetSlotNumber()
    {
        return slotNumber;
    }

    public InventoryObject GetStoredItem()
    {
        return storedItem;
    }

    public bool IsCurrentSlotSelected()
    {
        return isCurrentSlotSelected;
    }

    public static void ResetStaticData()
    {
        OnCurrentSlotSelected = null;
        OnStartItemDragging = null;
    }
}
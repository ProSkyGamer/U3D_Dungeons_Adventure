using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotSingleUI : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    public static event EventHandler<OnStartItemDraggingEventArgs> OnStartItemDragging;

    public class OnStartItemDraggingEventArgs : EventArgs
    {
        public InventoryObject draggingInventoryObject;
    }

    public static event EventHandler OnCurrentSlotSelected;

    private CharacterInventoryUI.InventoryType inventoryType;

    [SerializeField] private Image inventorySlotImage;
    [SerializeField] private TextMeshProUGUI inventorySlotText;
    [SerializeField] private Transform lockedInventorySlotTransform;
    [SerializeField] private Transform selectedInventorySlotTransform;

    private int slotNumber = -1;
    private InventoryObject storedItem;

    private bool isCurrentlyDragging;
    private bool isCurrentSlotSelected;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (storedItem == null) return;

        if (inventoryType == CharacterInventoryUI.InventoryType.PlayerWeaponInventory &&
            PlayerController.Instance.GetPlayerAttackInventory().GetCurrentInventoryObjectsCount() <= 1) return;

        StartDraggingItem();
    }

    private void Start()
    {
        OnStartItemDragging += InventorySlotSingleUI_OnStartItemDragging;
    }

    private void InventorySlotSingleUI_OnStartItemDragging(object sender, OnStartItemDraggingEventArgs e)
    {
        isCurrentlyDragging = true;
        OnCurrentSlotSelected += InventorySlotSingleUI_OnCurrentSlotSelected;
        CharacterInventoryUI.OnStopItemDragging += StatsTabUI_OnStopItemDragging;

        var slotSingleUI = sender as InventorySlotSingleUI;

        if (inventoryType != CharacterInventoryUI.InventoryType.PlayerInventory)
            if (inventoryType == CharacterInventoryUI.InventoryType.PlayerWeaponInventory)
                if (!e.draggingInventoryObject.TryGetWeaponSo(out _))
                    lockedInventorySlotTransform.gameObject.SetActive(true);

        if (slotSingleUI != this) return;

        isCurrentSlotSelected = true;
        selectedInventorySlotTransform.gameObject.SetActive(true);
    }

    private void StatsTabUI_OnStopItemDragging(object sender, EventArgs e)
    {
        isCurrentSlotSelected = false;
        isCurrentlyDragging = false;
        lockedInventorySlotTransform.gameObject.SetActive(false);
        selectedInventorySlotTransform.gameObject.SetActive(false);

        CharacterInventoryUI.OnStopItemDragging -= StatsTabUI_OnStopItemDragging;
    }

    private void InventorySlotSingleUI_OnCurrentSlotSelected(object sender, EventArgs e)
    {
        var slotSingleUI = sender as InventorySlotSingleUI;

        if (slotSingleUI == this) return;

        isCurrentSlotSelected = false;
        selectedInventorySlotTransform.gameObject.SetActive(false);
    }

    private void StartDraggingItem()
    {
        OnStartItemDragging?.Invoke(this, new OnStartItemDraggingEventArgs
        {
            draggingInventoryObject = storedItem
        });

        RemoveItem();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isCurrentlyDragging) return;

        if (inventoryType == CharacterInventoryUI.InventoryType.PlayerWeaponInventory)
            if (!CharacterInventoryUI.GetCurrentDraggingObject().TryGetWeaponSo(out _))
                return;

        selectedInventorySlotTransform.gameObject.SetActive(true);
        isCurrentSlotSelected = true;
        OnCurrentSlotSelected?.Invoke(this, EventArgs.Empty);
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

    public void SetStarterData(int newSlotNumber, CharacterInventoryUI.InventoryType newInventoryType)
    {
        if (slotNumber != -1) return;

        slotNumber = newSlotNumber;
        inventoryType = newInventoryType;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (storedItem != null)
        {
            inventorySlotImage.gameObject.SetActive(true);
            inventorySlotImage.sprite = storedItem.GetInventoryObjectSprite();

            inventorySlotText.gameObject.SetActive(true);
            inventorySlotText.text = storedItem.GetInventoryObjectName();
        }
        else
        {
            inventorySlotImage.gameObject.SetActive(false);
            inventorySlotText.gameObject.SetActive(false);
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
}

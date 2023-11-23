using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInventoryUI : MonoBehaviour
{
    public event EventHandler OnAnySlotInteractButtonPressed;
    public static event EventHandler OnStopItemDragging;

    public enum InventoryType
    {
        PlayerInventory,
        PlayerWeaponInventory,
        PlayerRelicsInventory
    }

    [SerializeField] private InventoryType inventoryType = InventoryType.PlayerInventory;

    [SerializeField] private Transform playerInventorySlotPrefab;
    [SerializeField] private Transform playerInventorySlotsGrid;
    [SerializeField] private bool isInventoryInteractable = true;
    [SerializeField] private bool isShowingObjectName = true;

    [SerializeField] private Image currentDraggingImage;
    [SerializeField] private Transform slotDescriptionPrefab;
    private Transform currentSlotDescription;
    [SerializeField] private Transform slotInteractButtonsPrefab;
    private Transform currentSlotInteractButtons;
    private static InventoryObject currentDraggingObject;

    private readonly List<InventorySlotSingleUI> allInventorySlots = new();

    private void Start()
    {
        OnAnySlotInteractButtonPressed += CharacterInventoryUI_OnAnySlotInteractButtonPressed;
        InventorySlotSingleUI.OnStartItemDragging += InventorySlotSingleUI_OnStartItemDragging;

        var maxSlotCount = 0;
        switch (inventoryType)
        {
            case InventoryType.PlayerInventory:
                maxSlotCount = PlayerController.Instance.GetPlayerInventory().GetMaxSlotsCount();
                break;
            case InventoryType.PlayerWeaponInventory:
                maxSlotCount = PlayerController.Instance.GetPlayerAttackInventory().GetMaxSlotsCount();
                break;
            case InventoryType.PlayerRelicsInventory:
                maxSlotCount = PlayerController.Instance.GetPlayerRelicsInventory().GetMaxSlotsCount();
                break;
        }

        for (var i = 0; i < maxSlotCount; i++)
        {
            var slotTransform = Instantiate(playerInventorySlotPrefab, playerInventorySlotsGrid);

            var slotSingleUI = slotTransform.GetComponent<InventorySlotSingleUI>();
            slotSingleUI.SetStarterData(i, inventoryType, isInventoryInteractable, isShowingObjectName, this);

            allInventorySlots.Add(slotSingleUI);
        }

        playerInventorySlotPrefab.gameObject.SetActive(false);
        currentDraggingImage.gameObject.SetActive(false);

        InventorySlotSingleUI.OnDisplaySlotDescription += InventorySlotSingleUI_OnDisplaySlotDescription;
        InventorySlotSingleUI.OnStopDisplaySlotDescription += InventorySlotSingleUI_OnStopDisplaySlotDescription;
        CharacterUI.OnCharacterUIClose += InventorySlotSingleUI_OnStopDisplaySlotDescription;

        InventorySlotSingleUI.OnDisplaySlotInteractButtons += InventorySlotSingleUI_OnDisplaySlotInteractButtons;
    }

    private void CharacterInventoryUI_OnAnySlotInteractButtonPressed(object sender, EventArgs e)
    {
        UpdateInventory();
    }

    private void InventorySlotSingleUI_OnDisplaySlotInteractButtons(object sender,
        InventorySlotSingleUI.OnDisplaySlotInteractButtonsEventArgs e)
    {
        if (currentSlotInteractButtons != null)
        {
            Destroy(currentSlotInteractButtons.gameObject);
            currentSlotInteractButtons = null;
        }

        if (e.displayedInventory != this) return;

        var newSlotInteractButton = Instantiate(slotInteractButtonsPrefab,
            GameInput.Instance.GetCurrentMousePosition(),
            Quaternion.identity, transform.GetComponentsInParent<Transform>()[1]);

        var slotInteractButtonsUI = newSlotInteractButton.GetComponent<InventorySlotInteractButtons>();
        slotInteractButtonsUI.SetSlotInfo(e.inventoryObject, inventoryType,
            () => { OnAnySlotInteractButtonPressed?.Invoke(this, EventArgs.Empty); });
        currentSlotInteractButtons = newSlotInteractButton;
    }

    private void InventorySlotSingleUI_OnStopDisplaySlotDescription(object sender, EventArgs e)
    {
        if (currentSlotDescription != null)
        {
            Destroy(currentSlotDescription.gameObject);
            currentSlotDescription = null;
        }
    }

    private void InventorySlotSingleUI_OnDisplaySlotDescription(object sender,
        InventorySlotSingleUI.OnDisplaySlotDescriptionEventArgs e)
    {
        if (currentSlotInteractButtons != null)
        {
            Destroy(currentSlotInteractButtons.gameObject);
            currentSlotInteractButtons = null;
        }

        if (e.displayedInventory != this) return;

        var newSlotDescription = Instantiate(slotDescriptionPrefab,
            GameInput.Instance.GetCurrentMousePosition(), Quaternion.identity,
            transform.GetComponentsInParent<Transform>()[1]);
        var inventoryItemSlotDescription = newSlotDescription.GetComponent<InventoryItemDescription>();
        inventoryItemSlotDescription.SetInventoryObject(e.inventoryObject);

        currentSlotDescription = newSlotDescription;
    }

    private void InventorySlotSingleUI_OnStartItemDragging(object sender,
        InventorySlotSingleUI.OnStartItemDraggingEventArgs e)
    {
        if (!isInventoryInteractable) return;

        currentDraggingObject = e.draggingInventoryObject;

        currentDraggingImage.gameObject.SetActive(true);
        currentDraggingImage.sprite = currentDraggingObject.GetInventoryObjectSprite();
    }

    private void Update()
    {
        if (currentDraggingObject == null) return;

        currentDraggingImage.transform.position = GameInput.Instance.GetCurrentMousePosition();

        if (GameInput.Instance.GetBindingValue(GameInput.Binding.UpgradesStartDragging) != 1f)
        {
            var selectedSlot = GetCurrentSelectedSlot();

            if (selectedSlot == null) return;

            selectedSlot.StoreItem(currentDraggingObject);

            var newSlotNumber = selectedSlot.GetSlotNumber();

            var inventoryToPlace = GetCurrentPlayerInventoryByType();

            currentDraggingObject.SetInventoryParentBySlot(inventoryToPlace,
                newSlotNumber);

            currentDraggingImage.gameObject.SetActive(false);

            currentDraggingObject = null;

            OnStopItemDragging?.Invoke(this, EventArgs.Empty);
        }
    }

    public void UpdateInventory()
    {
        currentDraggingImage.gameObject.SetActive(false);


        var playerInventory = GetCurrentPlayerInventoryByType();
        for (var i = 0; i < allInventorySlots.Count; i++)
        {
            var inventoryObject = playerInventory.GetInventoryObjectBySlot(i);

            if (inventoryObject == null)
            {
                allInventorySlots[i].RemoveItem();
                continue;
            }

            if (allInventorySlots[i].GetStoredItem() != inventoryObject)
                allInventorySlots[i].StoreItem(inventoryObject);
        }
    }

    private IInventoryParent GetCurrentPlayerInventoryByType()
    {
        switch (inventoryType)
        {
            default:
                return PlayerController.Instance.GetPlayerInventory();
            case InventoryType.PlayerInventory:
                return PlayerController.Instance.GetPlayerInventory();
            case InventoryType.PlayerWeaponInventory:
                return PlayerController.Instance.GetPlayerAttackInventory();
            case InventoryType.PlayerRelicsInventory:
                return PlayerController.Instance.GetPlayerRelicsInventory();
        }
    }

    private InventorySlotSingleUI GetCurrentSelectedSlot()
    {
        foreach (var inventorySlot in allInventorySlots)
            if (inventorySlot.IsCurrentSlotSelected())
                return inventorySlot;

        return null;
    }

    public static InventoryObject GetCurrentDraggingObject()
    {
        return currentDraggingObject;
    }

    public static void ResetStaticData()
    {
        OnStopItemDragging = null;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInventoryUI : MonoBehaviour
{
    public static event EventHandler OnStopItemDragging;

    public enum InventoryType
    {
        PlayerInventory,
        PlayerWeaponInventory
    }

    [SerializeField] private InventoryType inventoryType = InventoryType.PlayerInventory;

    [SerializeField] private Transform playerInventorySlotPrefab;
    [SerializeField] private Transform playerInventorySlotsGrid;

    [SerializeField] private Image currentDraggingImage;
    private static InventoryObject currentDraggingObject;

    private readonly List<InventorySlotSingleUI> allInventorySlots = new();

    private void Start()
    {
        InventorySlotSingleUI.OnStartItemDragging += InventorySlotSingleUI_OnStartItemDragging;

        var maxSlotCount = 0;
        switch (inventoryType)
        {
            case InventoryType.PlayerInventory:
                maxSlotCount = PlayerController.Instance.GetPlayerInventory().GetMaxSlotsCount();
                break;
            case InventoryType.PlayerWeaponInventory:
                maxSlotCount = PlayerController.Instance.GetMaxOwnedWeaponsCount();
                break;
        }

        for (var i = 0; i < maxSlotCount; i++)
        {
            var slotTransform = Instantiate(playerInventorySlotPrefab, playerInventorySlotsGrid);

            var slotSingleUI = slotTransform.GetComponent<InventorySlotSingleUI>();
            slotSingleUI.SetStarterData(i, inventoryType);

            allInventorySlots.Add(slotSingleUI);
        }

        playerInventorySlotPrefab.gameObject.SetActive(false);
        currentDraggingImage.gameObject.SetActive(false);
    }

    private void InventorySlotSingleUI_OnStartItemDragging(object sender,
        InventorySlotSingleUI.OnStartItemDraggingEventArgs e)
    {
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

            IInventoryParent inventoryToPlace;
            switch (inventoryType)
            {
                default:
                    inventoryToPlace = PlayerController.Instance.GetPlayerInventory();
                    break;
                case InventoryType.PlayerInventory:
                    inventoryToPlace = PlayerController.Instance.GetPlayerInventory();
                    break;
                case InventoryType.PlayerWeaponInventory:
                    inventoryToPlace = PlayerController.Instance.GetPlayerAttackInventory();
                    break;
            }

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

        IInventoryParent playerInventory;
        switch (inventoryType)
        {
            default:
                playerInventory = PlayerController.Instance.GetPlayerInventory();
                break;
            case InventoryType.PlayerInventory:
                playerInventory = PlayerController.Instance.GetPlayerInventory();
                break;
            case InventoryType.PlayerWeaponInventory:
                playerInventory = PlayerController.Instance.GetPlayerAttackInventory();
                break;
        }


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
}

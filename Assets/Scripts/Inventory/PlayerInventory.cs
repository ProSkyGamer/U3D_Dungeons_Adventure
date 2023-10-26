using UnityEngine;

public class PlayerInventory : MonoBehaviour, IInventoryParent
{
    [SerializeField] private int playerMaxSlots = 9;
    private InventoryObject[] storedInventoryObjects;

    private void Awake()
    {
        storedInventoryObjects = new InventoryObject[playerMaxSlots];
    }

    public void AddInventoryObject(InventoryObject inventoryObject)
    {
        var storedSlot = GetFirstAvailableSlot();
        if (storedSlot == -1) return;

        storedInventoryObjects[storedSlot] = inventoryObject;
    }

    public void AddInventoryObjectToSlot(InventoryObject inventoryObject, int slotNumber)
    {
        IsSlotNumberAvailable(slotNumber);

        storedInventoryObjects[slotNumber] = inventoryObject;
    }

    public InventoryObject GetInventoryObjectBySlot(int slotNumber)
    {
        return storedInventoryObjects[slotNumber];
    }

    public void RemoveInventoryObjectBySlot(int slotNumber)
    {
        Debug.Log("Removed");
        storedInventoryObjects[slotNumber] = null;
    }

    public bool IsHasAnyInventoryObject()
    {
        foreach (var inventoryObject in storedInventoryObjects)
            if (inventoryObject != null)
                return true;

        return false;
    }

    public bool IsSlotNumberAvailable(int slotNumber)
    {
        if (slotNumber < 0 || slotNumber >= storedInventoryObjects.Length) return false;

        return storedInventoryObjects[slotNumber] == null;
    }

    public bool IsHasAnyAvailableSlot()
    {
        foreach (var inventoryObject in storedInventoryObjects)
            if (inventoryObject == null)
                return true;

        return false;
    }

    public int GetFirstAvailableSlot()
    {
        for (var i = 0; i < storedInventoryObjects.Length; i++)
            if (storedInventoryObjects[i] == null)
                return i;

        return -1;
    }

    public int GetSlotNumberByInventoryObject(InventoryObject inventoryObject)
    {
        for (var i = 0; i < storedInventoryObjects.Length; i++)
            if (storedInventoryObjects[i] == inventoryObject)
                return i;

        return -1;
    }

    public int GetMaxSlotsCount()
    {
        return playerMaxSlots;
    }
}

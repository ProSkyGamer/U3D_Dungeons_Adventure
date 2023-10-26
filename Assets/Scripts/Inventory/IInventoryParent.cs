public interface IInventoryParent
{
    public void AddInventoryObject(InventoryObject inventoryObject);
    public void AddInventoryObjectToSlot(InventoryObject inventoryObject, int slotNumber);
    public InventoryObject GetInventoryObjectBySlot(int slotNumber);
    public void RemoveInventoryObjectBySlot(int slotNumber);
    public bool IsHasAnyInventoryObject();
    public bool IsSlotNumberAvailable(int slotNumber);
    public bool IsHasAnyAvailableSlot();
    public int GetFirstAvailableSlot();
    public int GetSlotNumberByInventoryObject(InventoryObject inventoryObject);
    public int GetMaxSlotsCount();
}

using UnityEngine;

public class InventoryObject : ScriptableObject
{
    [SerializeField] private Sprite inventoryObjectSprite;
    [SerializeField] private string inventoryObjectName;

    private IInventoryParent inventoryObjectParent;

    public void SetInventoryParent(IInventoryParent inventoryParent)
    {
        inventoryObjectParent?.RemoveInventoryObjectBySlot(inventoryObjectParent.GetSlotNumberByInventoryObject(this));
        inventoryParent.AddInventoryObject(this);
        inventoryObjectParent = inventoryParent;
    }

    public void SetInventoryParentBySlot(IInventoryParent inventoryParent, int slotNumber)
    {
        if (inventoryObjectParent.IsSlotNumberAvailable(slotNumber))
        {
            inventoryObjectParent?.RemoveInventoryObjectBySlot(
                inventoryObjectParent.GetSlotNumberByInventoryObject(this));

            inventoryObjectParent = inventoryParent;
            inventoryParent.AddInventoryObjectToSlot(this, slotNumber);
        }
    }

    public IInventoryParent GetInventoryObjectParent()
    {
        return inventoryObjectParent;
    }

    public Sprite GetInventoryObjectSprite()
    {
        return inventoryObjectSprite;
    }

    public string GetInventoryObjectName()
    {
        return inventoryObjectName;
    }

    public void SetInventoryObject(InventoryObject inventoryObject)
    {
        inventoryObjectSprite = inventoryObject.GetInventoryObjectSprite();
        inventoryObjectName = inventoryObject.GetInventoryObjectName();
    }
}

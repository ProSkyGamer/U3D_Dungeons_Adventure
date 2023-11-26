public class DroppedInventoryItemSingle : InteractableItem
{
    private InventoryObject currentDroppedObject;

    public override void OnInteract(PlayerController player)
    {
        if (!PlayerController.Instance.GetPlayerInventory().IsHasAnyAvailableSlot()) return;

        base.OnInteract(player);

        currentDroppedObject.SetInventoryParent(player.GetPlayerInventory());

        Destroy(gameObject);
    }

    public void SetDroppedItem(InventoryObject inventoryObject)
    {
        currentDroppedObject = inventoryObject;
    }

    public override bool IsCanInteract()
    {
        return isCanInteract && currentDroppedObject != null;
    }
}

using Unity.Netcode;

public class DroppedInventoryItemSingle : InteractableItem
{
    #region Variables & References

    private InventoryObject currentDroppedObject;
    private NetworkObject droppedItemNetworkObject;

    #endregion

    #region Interactable Item

    public override void OnInteract(PlayerController player)
    {
        if (!PlayerController.Instance.GetPlayerInventory().IsHasAnyAvailableSlot()) return;

        base.OnInteract(player);

        currentDroppedObject.SetInventoryParent(new NetworkObjectReference(player.GetPlayerNetworkObject()),
            CharacterInventoryUI.InventoryType.PlayerInventory, true);

        OnInteractServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnInteractServerRpc()
    {
        droppedItemNetworkObject.Despawn();
        Destroy(gameObject);
    }

    public override bool IsCanInteract()
    {
        return isCanInteract && currentDroppedObject != null;
    }

    #endregion

    #region Dropped Item Methods

    public void SetDroppedItem(InventoryObject inventoryObject)
    {
        currentDroppedObject = inventoryObject;

        droppedItemNetworkObject = GetComponent<NetworkObject>();
    }

    #endregion
}

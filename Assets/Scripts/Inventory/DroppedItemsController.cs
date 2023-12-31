using Unity.Netcode;
using UnityEngine;

public class DroppedItemsController : NetworkBehaviour
{
    public static DroppedItemsController Instance;

    [SerializeField] private Transform droppedItemPrefab;

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void AddNewDroppedItem(Vector3 dropPosition, out NetworkObjectReference droppedItemNetworkObjectReference)
    {
        droppedItemNetworkObjectReference = new NetworkObjectReference();

        if (!IsServer) return;

        var newDroppedItem = Instantiate(droppedItemPrefab, dropPosition, Quaternion.identity, transform);

        var droppedItemNetworkObject = newDroppedItem.GetComponent<NetworkObject>();
        droppedItemNetworkObject.Spawn();

        droppedItemNetworkObjectReference = new NetworkObjectReference(droppedItemNetworkObject);
    }

    public void SetDroppedItem(NetworkObjectReference droppedItemNetworkObjectReference, InventoryObject droppedObject)
    {
        droppedItemNetworkObjectReference.TryGet(out var droppedItemNetworkObject);
        var droppedItemSingleNetworkObject = droppedItemNetworkObject.GetComponent<DroppedInventoryItemSingle>();

        droppedItemSingleNetworkObject.SetDroppedItem(droppedObject);
        droppedItemSingleNetworkObject.GetComponent<AddInteractButtonUI>()
            .ChangeButtonText(droppedObject.GetInventoryObjectNameTextTranslationSo());
    }
}

using UnityEngine;

public class DroppedItemsController : MonoBehaviour
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

    public void AddNewDroppedItem(InventoryObject droppedItem, Vector3 dropPosition)
    {
        var newDroppedItem = Instantiate(droppedItemPrefab, dropPosition, Quaternion.identity, transform);
        newDroppedItem.GetComponent<DroppedInventoryItemSingle>().SetDroppedItem(droppedItem);
        newDroppedItem.GetComponent<AddInteractButtonUI>()
            .ChangeButtonText(droppedItem.GetInventoryObjectNameTextTranslationSo());
    }
}

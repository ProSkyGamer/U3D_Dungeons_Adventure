using UnityEngine;

[CreateAssetMenu]
public class InventoryObject : ScriptableObject
{
    [SerializeField] private Sprite inventoryObjectSprite;
    [SerializeField] private TextTranslationsSO inventoryObjectNameTextTranslationSo;

    [SerializeField] private WeaponSO weaponSo;
    [SerializeField] private RelicSO relicSo;
    [SerializeField] private TextTranslationsSO objectDescriptionTextTranslationSo;

    private IInventoryParent inventoryObjectParent;

    public void SetInventoryParent(IInventoryParent inventoryParent)
    {
        inventoryObjectParent?.RemoveInventoryObjectBySlot(inventoryObjectParent.GetSlotNumberByInventoryObject(this));
        inventoryParent.AddInventoryObject(this);
        inventoryObjectParent = inventoryParent;
    }

    public void SetInventoryParentBySlot(IInventoryParent inventoryParent, int slotNumber)
    {
        if (!inventoryParent.IsSlotNumberAvailable(slotNumber)) return;

        inventoryObjectParent?.RemoveInventoryObjectBySlot(
            inventoryObjectParent.GetSlotNumberByInventoryObject(this));

        inventoryParent.AddInventoryObjectToSlot(this, slotNumber);
        inventoryObjectParent = inventoryParent;
    }

    public void DropInventoryObjectToWorld(Vector3 dropPosition)
    {
        dropPosition += new Vector3(0f, 0.5f, 0f);
        DroppedItemsController.Instance.AddNewDroppedItem(this, dropPosition);
    }

    public void RemoveInventoryParent()
    {
        inventoryObjectParent?.RemoveInventoryObjectBySlot(inventoryObjectParent.GetSlotNumberByInventoryObject(this));
        inventoryObjectParent = null;
    }

    public IInventoryParent GetInventoryObjectParent()
    {
        return inventoryObjectParent;
    }

    public Sprite GetInventoryObjectSprite()
    {
        return inventoryObjectSprite;
    }

    public TextTranslationsSO GetInventoryObjectNameTextTranslationSo()
    {
        return inventoryObjectNameTextTranslationSo;
    }

    public TextTranslationsSO GetInventoryObjectDescriptionTextTranslationSo()
    {
        return objectDescriptionTextTranslationSo;
    }

    public void SetInventoryObject(InventoryObject inventoryObject)
    {
        inventoryObjectSprite = inventoryObject.GetInventoryObjectSprite();
        inventoryObjectNameTextTranslationSo = inventoryObject.GetInventoryObjectNameTextTranslationSo();
        objectDescriptionTextTranslationSo = inventoryObject.GetInventoryObjectDescriptionTextTranslationSo();

        if (inventoryObject.TryGetWeaponSo(out var newWeaponSo))
        {
            var newWeapon = CreateInstance<WeaponSO>();
            newWeapon.SetWeaponsSo(newWeaponSo);
            weaponSo = newWeapon;
        }

        if (inventoryObject.TryGetRelicSo(out var newRelicSo))
        {
            var newRelic = CreateInstance<RelicSO>();


            newRelic.SetRelicSo(newRelicSo);
            relicSo = newRelic;
        }
    }

    public bool TryGetWeaponSo(out WeaponSO gottenWeaponSo)
    {
        gottenWeaponSo = weaponSo;
        return gottenWeaponSo != null;
    }

    public bool TryGetRelicSo(out RelicSO gottenRelicSo)
    {
        gottenRelicSo = relicSo;
        return gottenRelicSo != null;
    }
}

using UnityEngine;

[CreateAssetMenu]
public class InventoryObject : ScriptableObject
{
    [SerializeField] private Sprite inventoryObjectSprite;
    [SerializeField] private string inventoryObjectName;

    [SerializeField] private WeaponSO weaponSo;
    [SerializeField] private RelicSO relicSo;

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

        inventoryObjectParent = inventoryParent;
        inventoryParent.AddInventoryObjectToSlot(this, slotNumber);
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

        inventoryObject.TryGetWeaponSo(out weaponSo);
        inventoryObject.TryGetRelicSo(out relicSo);
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

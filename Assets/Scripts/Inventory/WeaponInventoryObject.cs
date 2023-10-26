using UnityEngine;

[CreateAssetMenu]
public class WeaponInventoryObject : InventoryObject
{
    [SerializeField] private WeaponSO weaponSo;

    public WeaponSO GetWeaponSo()
    {
        return weaponSo;
    }

    public void SetInventoryObject(WeaponInventoryObject inventoryObjectWeapon)
    {
        base.SetInventoryObject(inventoryObjectWeapon);

        weaponSo = inventoryObjectWeapon.GetWeaponSo();
    }
}

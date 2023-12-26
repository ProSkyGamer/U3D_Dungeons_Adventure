using UnityEngine;

public class PlayerWeaponsVisual : MonoBehaviour
{
    private PlayerWeapons playerWeapons;

    [SerializeField] private Transform playerWeaponTransform;
    private Transform currentShownWeapon;

    private void Awake()
    {
        playerWeapons = GetComponent<PlayerWeapons>();
        playerWeapons.OnCurrentWeaponChange += PlayerWeapons_OnCurrentWeaponChange;
    }

    private void PlayerWeapons_OnCurrentWeaponChange(object sender, PlayerWeapons.OnCurrentWeaponChangeEventArgs e)
    {
        if (currentShownWeapon != null)
            Destroy(currentShownWeapon.gameObject);

        e.newWeapon.TryGetWeaponSo(out var weaponSo);
        var newShownWeapon = Instantiate(weaponSo.weaponVisual, playerWeaponTransform);
        currentShownWeapon = newShownWeapon;
    }
}

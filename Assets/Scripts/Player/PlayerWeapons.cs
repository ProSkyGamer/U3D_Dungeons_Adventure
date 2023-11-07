using System;
using UnityEngine;

public class PlayerWeapons : MonoBehaviour, IInventoryParent
{
    public event EventHandler<OnCurrentWeaponChangeEventArgs> OnCurrentWeaponChange;

    public class OnCurrentWeaponChangeEventArgs : EventArgs
    {
        public InventoryObject previousWeapon;
        public InventoryObject newWeapon;
    }

    [SerializeField] private InventoryObject firstChosenWeapon;
    private InventoryObject[] currentOwnedWeapon;
    [SerializeField] private int maxOwnedWeaponCount = 3;

    private InventoryObject currentChooseWeapon;

    private bool isFirstUpdate = true;

    private void Awake()
    {
        currentOwnedWeapon = new InventoryObject[maxOwnedWeaponCount];

        if (firstChosenWeapon == null) return;

        firstChosenWeapon.SetInventoryParent(this);
    }

    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;

            if (currentOwnedWeapon.Length > 0) TryChangeWeapon(currentOwnedWeapon[0]);
        }
    }

    public void TryChangeWeapon(InventoryObject weaponInventoryObject)
    {
        if (IsHasThisWeapon(weaponInventoryObject))
        {
            weaponInventoryObject.TryGetWeaponSo(out var weapon);
            if (weapon.comboAttackScales.Capacity >= weapon.comboAttack)
            {
                OnCurrentWeaponChange?.Invoke(this, new OnCurrentWeaponChangeEventArgs
                {
                    previousWeapon = currentChooseWeapon, newWeapon = weaponInventoryObject
                });
                currentChooseWeapon = weaponInventoryObject;
            }
            else
            {
                Debug.LogError($"Not enough weapon scales in List expected {weapon.comboAttack}," +
                               $" current {weapon.comboAttackScales.Capacity}");
            }
        }
    }

    public void TryChangeToNextWeapon()
    {
        if (GetCurrentInventoryObjectsCount() > 1)
        {
            var currentWeaponIndex = 0;
            for (var i = 0; i < currentOwnedWeapon.Length; i++)
                if (currentOwnedWeapon[i] == currentChooseWeapon)
                    currentWeaponIndex = i;

            var nextWeaponIndex = -1;
            var currentCycle = 1;
            while (nextWeaponIndex == -1)
            {
                nextWeaponIndex = currentWeaponIndex == currentOwnedWeapon.Length - 1
                    ? 0 + currentCycle - 1
                    : currentWeaponIndex + currentCycle;

                if (IsSlotNumberAvailable(nextWeaponIndex))
                    nextWeaponIndex = -1;

                currentCycle++;
            }

            TryChangeWeapon(currentOwnedWeapon[nextWeaponIndex]);
        }
    }

    public void TryDropCurrentWeapon()
    {
        if (GetCurrentInventoryObjectsCount() <= 1) return;

        var nearestWeapon = FindNearestInventoryObjectWeapon();
        OnCurrentWeaponChange?.Invoke(this, new OnCurrentWeaponChangeEventArgs
        {
            previousWeapon = currentChooseWeapon, newWeapon = nearestWeapon
        });

        currentChooseWeapon = null;

        currentChooseWeapon = nearestWeapon;
    }

    private bool IsHasThisWeapon(InventoryObject inventoryObjectWeapon)
    {
        foreach (var weapon in currentOwnedWeapon)
            if (weapon == inventoryObjectWeapon)
                return true;

        return false;
    }

    #region GetData

    public InventoryObject GetCurrentInventoryObjectWeapon()
    {
        return currentChooseWeapon;
    }

    public WeaponSO GetCurrentWeaponSo()
    {
        currentChooseWeapon.TryGetWeaponSo(out var weaponSo);

        return weaponSo;
    }

    public InventoryObject GetInventoryObjectBySlot(int slotNumber)
    {
        return currentOwnedWeapon[slotNumber];
    }

    public bool IsHasAnyInventoryObject()
    {
        foreach (var inventoryObject in currentOwnedWeapon)
            if (inventoryObject != null)
                return true;

        return false;
    }

    public bool IsSlotNumberAvailable(int slotNumber)
    {
        if (slotNumber < 0 || slotNumber >= currentOwnedWeapon.Length) return false;

        return currentOwnedWeapon[slotNumber] == null;
    }

    public bool IsHasAnyAvailableSlot()
    {
        foreach (var inventoryObject in currentOwnedWeapon)
            if (inventoryObject == null)
                return true;

        return false;
    }

    public int GetFirstAvailableSlot()
    {
        for (var i = 0; i < currentOwnedWeapon.Length; i++)
            if (currentOwnedWeapon[i] == null)
                return i;

        return -1;
    }

    public int GetSlotNumberByInventoryObject(InventoryObject inventoryObject)
    {
        for (var i = 0; i < currentOwnedWeapon.Length; i++)
            if (currentOwnedWeapon[i] == inventoryObject)
                return i;

        return -1;
    }

    public int GetMaxSlotsCount()
    {
        return maxOwnedWeaponCount;
    }

    public int GetCurrentInventoryObjectsCount()
    {
        var storedInventoryObjectsCount = 0;
        foreach (var inventoryObject in currentOwnedWeapon)
            if (inventoryObject != null)
                storedInventoryObjectsCount++;

        return storedInventoryObjectsCount;
    }

    #endregion

    public void AddInventoryObject(InventoryObject inventoryObject)
    {
        var storedSlot = GetFirstAvailableSlot();
        if (storedSlot == -1) return;

        currentOwnedWeapon[storedSlot] = inventoryObject;
    }

    public void AddInventoryObjectToSlot(InventoryObject inventoryObject, int slotNumber)
    {
        IsSlotNumberAvailable(slotNumber);

        currentOwnedWeapon[slotNumber] = inventoryObject;
    }

    public void RemoveInventoryObjectBySlot(int slotNumber)
    {
        if (currentChooseWeapon == currentOwnedWeapon[slotNumber])
        {
            var nearestWeapon = FindNearestInventoryObjectWeapon(slotNumber);
            TryChangeWeapon(nearestWeapon);
        }

        currentOwnedWeapon[slotNumber] = null;
    }

    private InventoryObject FindNearestInventoryObjectWeapon(int startingPoint = 0)
    {
        var i = startingPoint < currentOwnedWeapon.Length - 1 ? startingPoint + 1 : 0;
        while (i != startingPoint)
        {
            if (currentOwnedWeapon[i] != null)
                return currentOwnedWeapon[i];

            i = i < currentOwnedWeapon.Length - 1 ? i + 1 : 0;
        }

        return null;
    }
}

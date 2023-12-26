using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerWeaponsVisual))]
public class PlayerWeapons : NetworkBehaviour, IInventoryParent
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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        currentOwnedWeapon = new InventoryObject[maxOwnedWeaponCount];

        if (!IsOwner) return;

        if (firstChosenWeapon == null) Debug.LogError("No First Weapon");

        SetFirstWeaponServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetFirstWeaponServerRpc()
    {
        var firstChosenWeaponNewObjectTransform = Instantiate(firstChosenWeapon.transform);
        var firstChosenWeaponNetworkObject = firstChosenWeaponNewObjectTransform.GetComponent<NetworkObject>();
        firstChosenWeaponNetworkObject.Spawn();
        var firstChosenWeaponInventoryObject = firstChosenWeaponNewObjectTransform.GetComponent<InventoryObject>();
        firstChosenWeaponInventoryObject.SpawnInventoryObject();
        var networkObject = GetComponent<NetworkObject>();
        var networkObjectReference = new NetworkObjectReference(networkObject);
        firstChosenWeaponInventoryObject.SetInventoryParent(networkObjectReference,
            CharacterInventoryUI.InventoryType.PlayerWeaponInventory);
    }

    private void Start()
    {
        if (!IsOwner) return;

        GameInput.Instance.OnChangeCurrentWeaponAction += GameInput_OnChangeCurrentWeaponAction;
        GameInput.Instance.OnDropWeaponAction += GameInput_OnDropWeaponAction;
    }

    private void GameInput_OnChangeCurrentWeaponAction(object sender, EventArgs e)
    {
        OnChangeCurrentWeaponActionServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnChangeCurrentWeaponActionServerRpc()
    {
        if (GetCurrentInventoryObjectsCount() <= 1) return;

        OnChangeCurrentWeaponActionClientRpc();
    }

    [ClientRpc]
    private void OnChangeCurrentWeaponActionClientRpc()
    {
        ChangeToNextChosenWeapon();
    }

    private void GameInput_OnDropWeaponAction(object sender, EventArgs e)
    {
        OnDropWeaponActionServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnDropWeaponActionServerRpc()
    {
        if (GetCurrentInventoryObjectsCount() <= 1) return;

        var currentSelectedWeaponSlotNumber = GetSlotNumberByInventoryObject(currentChooseWeapon);
        currentOwnedWeapon[currentSelectedWeaponSlotNumber].RemoveInventoryParent();

        OnDropWeaponActionClientRpc();
    }

    [ClientRpc]
    private void OnDropWeaponActionClientRpc()
    {
        ChangeToNextChosenWeapon();
    }

    private void ChangeToNextChosenWeapon()
    {
        var currentSelectedWeaponSlotNumber = GetSlotNumberByInventoryObject(currentChooseWeapon);

        var nextChosenWeapon = FindNearestInventoryObjectWeapon(currentSelectedWeaponSlotNumber);

        TryChangeWeapon(nextChosenWeapon);
    }

    private void TryChangeWeapon(InventoryObject weaponInventoryObject)
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

        ReceivingItemsUI.Instance.AddReceivedItem(inventoryObject.GetInventoryObjectSprite(),
            inventoryObject.GetInventoryObjectNameTextTranslationSo(), 1, 1);

        if (currentChooseWeapon == null)
            ChangeToNextChosenWeapon();
    }

    public void AddInventoryObjectToSlot(InventoryObject inventoryObject, int slotNumber)
    {
        if (!IsSlotNumberAvailable(slotNumber)) return;

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

    public void ChangeInventorySize(int newSize)
    {
        if (!IsServer) return;

        ChangeInventorySizeServerRpc(newSize);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeInventorySizeServerRpc(int newSize)
    {
        if (maxOwnedWeaponCount == newSize) return;

        var newMaxOwnedWeaponCount = newSize;

        if (currentOwnedWeapon.Length > newSize)
            for (var i = newMaxOwnedWeaponCount; i < currentOwnedWeapon.Length; i++)
                currentOwnedWeapon[i].DropInventoryObjectToWorld(transform.position);

        ChangeInventorySizeClientRpc(newMaxOwnedWeaponCount);
    }

    [ClientRpc]
    private void ChangeInventorySizeClientRpc(int newMaxOwnedWeaponCount)
    {
        var newStoredRelicsInventory = new InventoryObject[newMaxOwnedWeaponCount];

        for (var i = 0; i < currentOwnedWeapon.Length; i++)
        {
            var storedRelic = currentOwnedWeapon[i];
            newStoredRelicsInventory[i] = storedRelic;
        }

        maxOwnedWeaponCount = newMaxOwnedWeaponCount;

        currentOwnedWeapon = newStoredRelicsInventory;
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

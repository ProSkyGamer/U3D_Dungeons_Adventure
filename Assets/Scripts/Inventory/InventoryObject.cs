using System;
using Unity.Netcode;
using UnityEngine;

public class InventoryObject : NetworkBehaviour
{
    public event EventHandler OnObjectRepaired;

    [SerializeField] private Sprite inventoryObjectSprite;
    [SerializeField] private Sprite brokenInventoryObjectSprite;
    [SerializeField] private TextTranslationsSO inventoryObjectNameTextTranslationSo;

    [SerializeField] private WeaponSO weaponSo;
    [SerializeField] private RelicSO relicSo;
    [SerializeField] private bool isBroken;

    private IInventoryParent inventoryObjectParent;

    public void SpawnInventoryObject()
    {
        SpawnInventoryObjectServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnInventoryObjectServerRpc()
    {
        SpawnInventoryObjectClientRpc();
    }

    [ClientRpc]
    private void SpawnInventoryObjectClientRpc()
    {
        if (weaponSo != null)
        {
            var newWeapon = ScriptableObject.CreateInstance<WeaponSO>();
            newWeapon.SetWeaponsSo(weaponSo);
            weaponSo = newWeapon;
        }

        if (relicSo != null)
        {
            var newRelic = ScriptableObject.CreateInstance<RelicSO>();

            newRelic.SetRelicSo(relicSo);
            relicSo = newRelic;
        }
    }

    public void SetInventoryParent(NetworkObjectReference inventoryNetworkObjectReference,
        CharacterInventoryUI.InventoryType inventoryType)
    {
        SetInventoryParentServerRpc(inventoryNetworkObjectReference, (int)inventoryType);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetInventoryParentServerRpc(NetworkObjectReference inventoryObjectOwnerNetworkObjectReference,
        int intInventoryType)
    {
        SetInventoryParentClientRpc(inventoryObjectOwnerNetworkObjectReference, intInventoryType);
    }

    [ClientRpc]
    private void SetInventoryParentClientRpc(NetworkObjectReference inventoryObjectOwnerNetworkObjectReference,
        int intInventoryType)
    {
        inventoryObjectOwnerNetworkObjectReference.TryGet(out var inventoryObjectOwnerNetworkObject);

        IInventoryParent storedInventory;
        switch ((CharacterInventoryUI.InventoryType)intInventoryType)
        {
            default:
                storedInventory = inventoryObjectOwnerNetworkObject.GetComponent<PlayerInventory>();
                break;
            case CharacterInventoryUI.InventoryType.PlayerInventory:
                storedInventory = inventoryObjectOwnerNetworkObject.GetComponent<PlayerInventory>();
                break;
            case CharacterInventoryUI.InventoryType.PlayerRelicsInventory:
                storedInventory = inventoryObjectOwnerNetworkObject.GetComponent<PlayerRelics>();
                break;
            case CharacterInventoryUI.InventoryType.PlayerWeaponInventory:
                storedInventory = inventoryObjectOwnerNetworkObject.GetComponent<PlayerWeapons>();
                break;
        }

        inventoryObjectParent?.RemoveInventoryObjectBySlot(inventoryObjectParent.GetSlotNumberByInventoryObject(this));
        storedInventory.AddInventoryObject(this);
        inventoryObjectParent = storedInventory;
    }

    public void SetInventoryParentBySlot(NetworkObjectReference inventoryObjectOwnerNetworkObjectReference,
        int intInventoryType, int slotNumber)
    {
        SetInventoryParentBySlotServerRpc(inventoryObjectOwnerNetworkObjectReference, intInventoryType, slotNumber);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetInventoryParentBySlotServerRpc(NetworkObjectReference inventoryObjectOwnerNetworkObjectReference,
        int intInventoryType, int newSlotNumber)
    {
        SetInventoryParentBySlotClientRpc(inventoryObjectOwnerNetworkObjectReference, intInventoryType, newSlotNumber);
    }

    [ClientRpc]
    private void SetInventoryParentBySlotClientRpc(NetworkObjectReference inventoryObjectOwnerNetworkObjectReference,
        int intInventoryType, int newSlotNumber)
    {
        inventoryObjectOwnerNetworkObjectReference.TryGet(out var inventoryObjectOwnerNetworkObject);

        IInventoryParent storedInventory;
        switch ((CharacterInventoryUI.InventoryType)intInventoryType)
        {
            default:
                storedInventory = inventoryObjectOwnerNetworkObject.GetComponent<PlayerInventory>();
                break;
            case CharacterInventoryUI.InventoryType.PlayerInventory:
                storedInventory = inventoryObjectOwnerNetworkObject.GetComponent<PlayerInventory>();
                break;
            case CharacterInventoryUI.InventoryType.PlayerRelicsInventory:
                storedInventory = inventoryObjectOwnerNetworkObject.GetComponent<PlayerRelics>();
                break;
            case CharacterInventoryUI.InventoryType.PlayerWeaponInventory:
                storedInventory = inventoryObjectOwnerNetworkObject.GetComponent<PlayerWeapons>();
                break;
        }

        if (!storedInventory.IsSlotNumberAvailable(newSlotNumber)) return;

        inventoryObjectParent?.RemoveInventoryObjectBySlot(
            inventoryObjectParent.GetSlotNumberByInventoryObject(this));

        storedInventory.AddInventoryObjectToSlot(this, newSlotNumber);
        inventoryObjectParent = storedInventory;
    }

    public void DropInventoryObjectToWorld(Vector3 dropPosition)
    {
        DropInventoryObjectToWorldServerRpc(dropPosition);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DropInventoryObjectToWorldServerRpc(Vector3 dropPosition)
    {
        dropPosition += new Vector3(0f, 0.5f, 0f);
        DroppedItemsController.Instance.AddNewDroppedItem(dropPosition, out var droppedItemNetworkObjectReference);

        DropInventoryObjectToWorldClientRpc(droppedItemNetworkObjectReference);
    }

    [ClientRpc]
    private void DropInventoryObjectToWorldClientRpc(NetworkObjectReference droppedItemNetworkObjectReference)
    {
        Debug.Log("It worked");

        DroppedItemsController.Instance.SetDroppedItem(droppedItemNetworkObjectReference, this);
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
        return isBroken ? brokenInventoryObjectSprite : inventoryObjectSprite;
    }

    public TextTranslationsSO GetInventoryObjectNameTextTranslationSo()
    {
        return inventoryObjectNameTextTranslationSo;
    }

    public void BreakObject()
    {
        if (!IsServer) return;

        ChangeBrokenObjectStateServerRpc(true);
    }

    public void RepairObject()
    {
        if (!IsServer) return;

        ChangeBrokenObjectStateServerRpc(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeBrokenObjectStateServerRpc(bool newState)
    {
        ChangeBrokenObjectStateServerRpcClientRpc(newState);
    }

    [ClientRpc]
    private void ChangeBrokenObjectStateServerRpcClientRpc(bool newState)
    {
        isBroken = newState;

        if (!isBroken)
            OnObjectRepaired?.Invoke(this, EventArgs.Empty);
    }

    public void TryNullifyRelicUsages()
    {
        if (!IsServer) return;
        if (relicSo == null) return;

        TryNullifyRelicUsagesServerRpc();
    }

    [ServerRpc]
    private void TryNullifyRelicUsagesServerRpc()
    {
        var newRelicUsagesCount = 0;

        TryNullifyRelicUsagesClientRpc(newRelicUsagesCount);
    }

    [ClientRpc]
    private void TryNullifyRelicUsagesClientRpc(int newRelicUsagesCount)
    {
        foreach (var relicApplyingEffect in relicSo.relicApplyingEffects)
        {
            if (!relicApplyingEffect.isUsagesLimited) continue;

            relicApplyingEffect.currentUsages = newRelicUsagesCount;
        }
    }

    public bool IsBroken()
    {
        return isBroken;
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
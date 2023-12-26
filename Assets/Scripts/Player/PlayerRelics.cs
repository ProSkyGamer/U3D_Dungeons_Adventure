using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerRelics : NetworkBehaviour, IInventoryParent
{
    public event EventHandler<OnRelicChangeEventArgs> OnRelicsChange;

    public class OnRelicChangeEventArgs : EventArgs
    {
        public InventoryObject addedRelic;
        public InventoryObject removedRelic;
    }

    [SerializeField] private int maxRelicsSlotsCount = 3;
    private InventoryObject[] allStoredRelics;

    private PlayerEffectsController playerEffectsController;

    private void Awake()
    {
        playerEffectsController = GetComponent<PlayerEffectsController>();

        allStoredRelics = new InventoryObject[maxRelicsSlotsCount];
    }

    private void Start()
    {
        if (IsServer)
            playerEffectsController.OnPlayerRelicOutOfUsagesCount +=
                PlayerEffectsControllerOnPlayerRelicOutOfUsagesCount;
    }

    private void PlayerEffectsControllerOnPlayerRelicOutOfUsagesCount(object sender,
        PlayerEffectsController.OnPlayerRelicOutOfUsagesCountEventArgs e)
    {
        foreach (var storedRelic in allStoredRelics)
        {
            storedRelic.TryGetRelicSo(out var relicSo);
            foreach (var relicEffect in relicSo.relicApplyingEffects)
                if (relicEffect.isUsagesLimited && relicEffect.appliedEffectType == e.relicBuff.appliedEffectType)
                {
                    storedRelic.BreakObject();
                    OnRelicsChange?.Invoke(this, new OnRelicChangeEventArgs
                    {
                        removedRelic = storedRelic
                    });
                    return;
                }
        }
    }

    public void ChangeInventorySize(int newSize)
    {
        if (!IsServer) return;

        ChangeInventorySizeServerRpc(newSize);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeInventorySizeServerRpc(int newSize)
    {
        if (maxRelicsSlotsCount == newSize) return;

        var newMaxRelicsSlotsCount = newSize;

        if (allStoredRelics.Length > newSize)
            for (var i = newMaxRelicsSlotsCount; i < allStoredRelics.Length; i++)
                allStoredRelics[i].DropInventoryObjectToWorld(transform.position);

        ChangeInventorySizeClientRpc(newMaxRelicsSlotsCount);
    }

    [ClientRpc]
    private void ChangeInventorySizeClientRpc(int newMaxRelicsSlotsCount)
    {
        var newStoredRelicsInventory = new InventoryObject[newMaxRelicsSlotsCount];

        for (var i = 0; i < allStoredRelics.Length; i++)
        {
            var storedRelic = allStoredRelics[i];
            newStoredRelicsInventory[i] = storedRelic;
        }

        maxRelicsSlotsCount = newMaxRelicsSlotsCount;

        allStoredRelics = newStoredRelicsInventory;
    }

    public void AddInventoryObject(InventoryObject inventoryObject)
    {
        var storedSlot = GetFirstAvailableSlot();
        if (storedSlot == -1) return;

        allStoredRelics[storedSlot] = inventoryObject;
        inventoryObject.OnObjectRepaired += InventoryObject_OnObjectRepaired;

        OnRelicsChange?.Invoke(this, new OnRelicChangeEventArgs
        {
            addedRelic = inventoryObject
        });

        ReceivingItemsUI.Instance.AddReceivedItem(inventoryObject.GetInventoryObjectSprite(),
            inventoryObject.GetInventoryObjectNameTextTranslationSo(), 1, 1);
    }

    private void InventoryObject_OnObjectRepaired(object sender, EventArgs e)
    {
        var inventoryObject = sender as InventoryObject;

        OnRelicsChange?.Invoke(this, new OnRelicChangeEventArgs
        {
            addedRelic = inventoryObject
        });
    }

    public void AddInventoryObjectToSlot(InventoryObject inventoryObject, int slotNumber)
    {
        if (!IsSlotNumberAvailable(slotNumber)) return;

        allStoredRelics[slotNumber] = inventoryObject;
        inventoryObject.OnObjectRepaired += InventoryObject_OnObjectRepaired;

        OnRelicsChange?.Invoke(this, new OnRelicChangeEventArgs
        {
            addedRelic = inventoryObject
        });
    }

    public InventoryObject GetInventoryObjectBySlot(int slotNumber)
    {
        return allStoredRelics[slotNumber];
    }

    public void RemoveInventoryObjectBySlot(int slotNumber)
    {
        OnRelicsChange?.Invoke(this, new OnRelicChangeEventArgs
        {
            removedRelic = allStoredRelics[slotNumber]
        });

        allStoredRelics[slotNumber] = null;
    }

    public bool IsHasAnyInventoryObject()
    {
        foreach (var inventoryObject in allStoredRelics)
            if (inventoryObject != null)
                return true;

        return false;
    }

    public bool IsSlotNumberAvailable(int slotNumber)
    {
        if (slotNumber < 0 || slotNumber >= allStoredRelics.Length) return false;

        return allStoredRelics[slotNumber] == null;
    }

    public bool IsHasAnyAvailableSlot()
    {
        foreach (var inventoryObject in allStoredRelics)
            if (inventoryObject == null)
                return true;

        return false;
    }

    public int GetFirstAvailableSlot()
    {
        for (var i = 0; i < allStoredRelics.Length; i++)
            if (allStoredRelics[i] == null)
                return i;

        return -1;
    }

    public int GetSlotNumberByInventoryObject(InventoryObject inventoryObject)
    {
        for (var i = 0; i < allStoredRelics.Length; i++)
            if (allStoredRelics[i] == inventoryObject)
                return i;

        return -1;
    }

    public int GetMaxSlotsCount()
    {
        return maxRelicsSlotsCount;
    }

    public int GetCurrentInventoryObjectsCount()
    {
        var storedInventoryObjectsCount = 0;
        foreach (var inventoryObject in allStoredRelics)
            if (inventoryObject != null)
                storedInventoryObjectsCount++;

        return storedInventoryObjectsCount;
    }
}

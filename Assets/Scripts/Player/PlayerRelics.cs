using System;
using UnityEngine;

public class PlayerRelics : MonoBehaviour, IInventoryParent
{
    public event EventHandler<OnRelicChangeEventArgs> OnRelicsChange;

    public class OnRelicChangeEventArgs : EventArgs
    {
        public InventoryObject addedRelic;
        public InventoryObject removedRelic;
    }

    [SerializeField] private int maxRelicsSlotsCount = 3;
    private InventoryObject[] allStoredRelics;

    private PlayerEffects playerEffects;

    private void Awake()
    {
        allStoredRelics = new InventoryObject[maxRelicsSlotsCount];

        playerEffects = GetComponent<PlayerEffects>();
    }

    private void Start()
    {
        playerEffects.OnPlayerRelicOutOfUsagesCount += PlayerEffects_OnPlayerRelicOutOfUsagesCount;
    }

    private void PlayerEffects_OnPlayerRelicOutOfUsagesCount(object sender,
        PlayerEffects.OnPlayerRelicOutOfUsagesCountEventArgs e)
    {
        for (var i = 0; i < allStoredRelics.Length; i++)
        {
            allStoredRelics[i].TryGetRelicSo(out var relicSo);
            foreach (var relicBuff in relicSo.relicBuffs)
                if (relicBuff.isHasLimit && relicBuff.relicBuffType == e.relicBuff.relicBuffType)
                {
                    RemoveInventoryObjectBySlot(i);
                    return;
                }
        }
    }

    public void AddInventoryObject(InventoryObject inventoryObject)
    {
        var storedSlot = GetFirstAvailableSlot();
        if (storedSlot == -1) return;

        allStoredRelics[storedSlot] = inventoryObject;

        OnRelicsChange?.Invoke(this, new OnRelicChangeEventArgs
        {
            addedRelic = inventoryObject
        });
    }

    public void AddInventoryObjectToSlot(InventoryObject inventoryObject, int slotNumber)
    {
        IsSlotNumberAvailable(slotNumber);

        allStoredRelics[slotNumber] = inventoryObject;

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

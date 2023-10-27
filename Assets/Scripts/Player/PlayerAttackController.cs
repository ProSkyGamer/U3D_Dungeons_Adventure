using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerAttackController : MonoBehaviour, IInventoryParent
{
    [SerializeField] private InventoryObject firstChosenWeapon;
    private InventoryObject[] currentOwnedWeapon;
    [SerializeField] private int maxOwnedWeaponCount = 3;

    private InventoryObject currentChooseWeapon;

    private int currentAttackCombo = -1;

    [SerializeField] private float comboAttackResetTime = 3f;
    private float comboAttackResetTimer;

    #region GeneralStats

    [SerializeField] private int baseAttack = 100;
    private int currentAttack;

    [SerializeField] private float weaponAttackRange = 3f;
    [SerializeField] private LayerMask enemiesLayer;

    private float normalAttackDamageBonus;
    private float chargedAttackDamageBonus;

    [SerializeField] private float baseCritRate = 0.05f;
    [SerializeField] private float baseCritDamage = 0.5f;
    private float currentCritRate;
    private float currentCritDamage;

    #endregion

    public event EventHandler<OnCurrentWeaponChangeEventArgs> OnCurrentWeaponChange;

    public class OnCurrentWeaponChangeEventArgs : EventArgs
    {
        public InventoryObject previousWeapon;
        public InventoryObject newWeapon;
    }

    private bool isFirstUpdate = true;

    private void Awake()
    {
        currentOwnedWeapon = new InventoryObject[maxOwnedWeaponCount];

        currentAttack = baseAttack;

        currentCritRate = baseCritRate;
        currentCritDamage = baseCritDamage;

        comboAttackResetTimer = comboAttackResetTime;

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

        if (currentAttackCombo != -1)
        {
            comboAttackResetTimer -= Time.deltaTime;
            if (comboAttackResetTimer <= 0) ResetNormalAttackCombo();
        }
    }

    public void ChangeAttackBuff(float percentageBuff = default, int flatBuff = default)
    {
        currentAttack += (int)(baseAttack * percentageBuff);
        currentAttack += flatBuff;
    }

    public void ChangeNormalAttackBuff(float percentageBuff)
    {
        normalAttackDamageBonus += percentageBuff;
    }

    public void ChangeChargedAttackBuff(float percentageBuff)
    {
        chargedAttackDamageBonus += percentageBuff;
    }

    public void ChangeCritRateBuff(float percentageBuff)
    {
        currentCritRate += percentageBuff;
    }

    public void ChangeCritDamageBuff(float percentageBuff)
    {
        currentCritDamage += percentageBuff;
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


        currentChooseWeapon.SetInventoryParent(null);

        TryChangeWeapon(currentOwnedWeapon[0]);
    }

    public void NormalAttack()
    {
        if (currentChooseWeapon == null) return;

        currentAttackCombo++;
        currentChooseWeapon.TryGetWeaponSo(out var currentChooseWeaponSo);
        if (currentAttackCombo >= currentChooseWeaponSo.comboAttack)
            currentAttackCombo = 0;

        var normalAttackDamage = CalculateDamage(currentAttack,
            currentChooseWeaponSo.comboAttackScales[currentAttackCombo],
            normalAttackDamageBonus, currentCritRate, currentCritDamage);

        var enemiesToAttack = FindEnemiesToAttack();

        foreach (var enemy in enemiesToAttack) enemy.ReceiveDamage(normalAttackDamage, transform);

        //Debug.Log($"I'm attacking with damage:{normalAttackDamage} by N.A.");
        comboAttackResetTimer = comboAttackResetTime;
    }

    private void ResetNormalAttackCombo()
    {
        comboAttackResetTimer = comboAttackResetTime;
        currentAttackCombo = -1;
    }

    public void ChargeAttack()
    {
        if (currentChooseWeapon == null) return;

        ResetNormalAttackCombo();

        currentChooseWeapon.TryGetWeaponSo(out var currentChooseWeaponSo);
        var chargeAttackDamage = CalculateDamage(currentAttack,
            currentChooseWeaponSo.chargedAttackDamageScale,
            chargedAttackDamageBonus, currentCritRate, currentCritDamage);

        var enemiesToAttack = FindEnemiesToAttack();

        foreach (var enemy in enemiesToAttack) enemy.ReceiveDamage(chargeAttackDamage, transform);

        //Debug.Log($"I'm attacking with damage:{chargeAttackDamage} by C.A.");
    }

    private int CalculateDamage(int attack, float attackScale, float damageBonus, float critCrate, float critDamage)
    {
        var attackDamage = (int)(attack * attackScale * (1 + damageBonus) * GetCritValue(critCrate, critDamage));

        return attackDamage;
    }

    private List<EnemyController> FindEnemiesToAttack()
    {
        var castPosition = transform.position;
        var castCubeLength = new Vector3(weaponAttackRange, weaponAttackRange, weaponAttackRange);

        var raycastHits = Physics.BoxCastAll(castPosition, castCubeLength, Vector3.forward,
            quaternion.identity, weaponAttackRange, enemiesLayer);

        List<EnemyController> enemiesToAttack = new();

        foreach (var hit in raycastHits)
            if (hit.transform.gameObject.TryGetComponent(out EnemyController enemyController))
                enemiesToAttack.Add(hit.transform.gameObject.GetComponent<EnemyController>());

        //Debug.Log($"Enemies to attack {enemiesToAttack.Count} {raycastHits.Length}");

        return enemiesToAttack;
    }

    private float GetCritValue(float critRate, float critDamage)
    {
        var isCrit = Random.Range(0, 1000) < critRate * 1000 ? 1 : 0;
        var critValue = 1 + critDamage * isCrit;

        return critValue;
    }

    private bool IsHasThisWeapon(InventoryObject inventoryObjectWeapon)
    {
        foreach (var weapon in currentOwnedWeapon)
            if (weapon == inventoryObjectWeapon)
                return true;

        return false;
    }

    #region GetVariablesData

    public InventoryObject GetCurrentInventoryObjectWeapon()
    {
        return currentChooseWeapon;
    }

    public int GetBaseAttack()
    {
        return baseAttack;
    }

    public int GetCurrentAttack()
    {
        return currentAttack;
    }

    public int GetCurrentCritRate()
    {
        return (int)(currentCritRate * 100);
    }

    public int GetCurrentCritDmg()
    {
        return (int)(currentCritDamage * 100);
    }

    public int GetCurrentNaDmgBonus()
    {
        return (int)(normalAttackDamageBonus * 100);
    }

    public int GetCurrentCaDmgBonus()
    {
        return (int)(chargedAttackDamageBonus * 100);
    }

    public int GetMaxOwnedWeaponsCount()
    {
        return maxOwnedWeaponCount;
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

    public InventoryObject GetInventoryObjectBySlot(int slotNumber)
    {
        return currentOwnedWeapon[slotNumber];
    }

    public void RemoveInventoryObjectBySlot(int slotNumber)
    {
        Debug.Log("Removed");
        currentOwnedWeapon[slotNumber] = null;
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
}

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerAttackController : MonoBehaviour
{
    [SerializeField]
    private List<WeaponSO> currentOwnedWeapon;

    [SerializeField] private int maxOwnedWeaponCount = 3;

    [SerializeField]
    private WeaponSO currentChooseWeapon;

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
        public WeaponSO previousWeapon;
        public WeaponSO newWeapon;
    }

    private bool isFirstUpdate = true;

    private void Awake()
    {
        currentAttack = baseAttack;

        currentCritRate = baseCritRate;
        currentCritDamage = baseCritDamage;

        comboAttackResetTimer = comboAttackResetTime;
    }

    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;

            if (currentOwnedWeapon.Count > 0)
                TryChangeWeapon(currentOwnedWeapon[0]);
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

    public void TryChangeWeapon(WeaponSO weapon)
    {
        if (currentOwnedWeapon.Contains(weapon))
        {
            if (weapon.comboAttackScales.Capacity >= weapon.comboAttack)
            {
                OnCurrentWeaponChange?.Invoke(this, new OnCurrentWeaponChangeEventArgs
                {
                    previousWeapon = currentChooseWeapon, newWeapon = weapon
                });
                currentChooseWeapon = weapon;
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
        if (currentOwnedWeapon.Count > 1)
        {
            var currentWeaponIndex = 0;
            for (var i = 0; i < currentOwnedWeapon.Count; i++)
                if (currentOwnedWeapon[i] == currentChooseWeapon)
                    currentWeaponIndex = i;

            var nextWeaponIndex = currentWeaponIndex == currentOwnedWeapon.Count - 1 ? 0 : currentWeaponIndex++;

            TryChangeWeapon(currentOwnedWeapon[nextWeaponIndex]);
        }
    }

    public void TryAddOwnedWeapon(WeaponSO weapon)
    {
        if (currentOwnedWeapon.Count > maxOwnedWeaponCount) return;
        if (currentOwnedWeapon.Contains(weapon)) return;

        currentOwnedWeapon.Add(weapon);
    }

    public void TryRemoveOwnedWeapon(WeaponSO weapon)
    {
        if (currentOwnedWeapon.Count <= 1) return;

        if (currentOwnedWeapon.Contains(weapon))
            currentOwnedWeapon.Remove(weapon);
    }

    public void TryDropCurrentWeapon()
    {
        if (currentOwnedWeapon.Count > 1)
        {
            currentOwnedWeapon.Remove(currentChooseWeapon);

            TryChangeWeapon(currentOwnedWeapon[0]);
        }
    }

    public void NormalAttack()
    {
        if (currentChooseWeapon == null) return;

        currentAttackCombo++;
        if (currentAttackCombo >= currentChooseWeapon.comboAttack)
            currentAttackCombo = 0;

        var normalAttackDamage = CalculateDamage(currentAttack,
            currentChooseWeapon.comboAttackScales[currentAttackCombo],
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

        var chargeAttackDamage = CalculateDamage(currentAttack,
            currentChooseWeapon.chargedAttackDamageScale,
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

    #region GetVariablesData

    public WeaponSO GetCurrentWeapon()
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

    #endregion
}

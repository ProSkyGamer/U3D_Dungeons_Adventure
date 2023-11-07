using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
    private int currentAttackCombo = -1;

    [SerializeField] private float comboAttackResetTime = 3f;
    private float comboAttackResetTimer;

    #region GeneralStats

    [SerializeField] private int baseAttack = 100;
    private int currentAttack;

    private float normalAttackDamageBonus;
    private float chargedAttackDamageBonus;

    [SerializeField] private float baseCritRate = 0.05f;
    [SerializeField] private float baseCritDamage = 0.5f;
    private float currentCritRate;
    private float currentCritDamage;

    #endregion

    private PlayerWeapons playerWeapons;

    private void Awake()
    {
        playerWeapons = GetComponent<PlayerWeapons>();

        currentAttack = baseAttack;

        currentCritRate = baseCritRate;
        currentCritDamage = baseCritDamage;

        comboAttackResetTimer = comboAttackResetTime;
    }

    private void Update()
    {
        if (GameStageManager.Instance.IsPause()) return;

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

    public void NormalAttack(List<EnemyController> enemiesToAttack, PlayerController attackedPlayerController)
    {
        var currentChooseWeaponSo = playerWeapons.GetCurrentWeaponSo();
        if (currentChooseWeaponSo == null) return;

        currentAttackCombo++;
        if (currentAttackCombo >= currentChooseWeaponSo.comboAttack)
            currentAttackCombo = 0;

        var normalAttackDamage = CalculateDamage(currentAttack,
            currentChooseWeaponSo.comboAttackScales[currentAttackCombo],
            normalAttackDamageBonus, currentCritRate, currentCritDamage);

        foreach (var enemy in enemiesToAttack) enemy.ReceiveDamage(normalAttackDamage, attackedPlayerController);

        //Debug.Log($"I'm attacking with damage:{normalAttackDamage} by N.A.");
        comboAttackResetTimer = comboAttackResetTime;
    }

    private void ResetNormalAttackCombo()
    {
        comboAttackResetTimer = comboAttackResetTime;
        currentAttackCombo = -1;
    }

    public void ChargeAttack(List<EnemyController> enemiesToAttack, PlayerController attackedPlayerController)
    {
        var currentChooseWeaponSo = playerWeapons.GetCurrentWeaponSo();
        if (currentChooseWeaponSo == null) return;

        ResetNormalAttackCombo();

        var chargeAttackDamage = CalculateDamage(currentAttack,
            currentChooseWeaponSo.chargedAttackDamageScale,
            chargedAttackDamageBonus, currentCritRate, currentCritDamage);

        foreach (var enemy in enemiesToAttack) enemy.ReceiveDamage(chargeAttackDamage, attackedPlayerController);

        //Debug.Log($"I'm attacking with damage:{chargeAttackDamage} by C.A.");
    }

    private int CalculateDamage(int attack, float attackScale, float damageBonus, float critCrate, float critDamage)
    {
        var attackDamage = (int)(attack * attackScale * (1 + damageBonus) * GetCritValue(critCrate, critDamage));

        return attackDamage;
    }

    private float GetCritValue(float critRate, float critDamage)
    {
        var isCrit = Random.Range(0, 1000) < critRate * 1000 ? 1 : 0;
        var critValue = 1 + critDamage * isCrit;

        return critValue;
    }

    #region GetVariablesData

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

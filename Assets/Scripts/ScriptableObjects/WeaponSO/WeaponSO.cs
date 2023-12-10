using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WeaponSO : ScriptableObject
{
    public enum WeaponType
    {
        Katana,
        Gun
    }

    [Header("All scales must be in this variant (0.753)")]
    public WeaponType weaponType;

    public Transform weaponVisual;
    public PlayerEffects.AllPlayerEffects additionalWeaponStatType = PlayerEffects.AllPlayerEffects.AtkIncrease;
    public float additionalWeaponStatTypeScale = 0.1f;
    public List<PlayerEffects.AppliedEffect> weaponPassiveTalent;
    [Range(1, 6)] public int comboAttack = 1;
    public List<float> comboAttackScales = new();
    public float chargedAttackDamageScale;
    public float chargedAttackStaminaCost = 25f;

    public void SetWeaponsSo(WeaponSO weaponToSet)
    {
        weaponType = weaponToSet.weaponType;
        weaponVisual = weaponToSet.weaponVisual;
        comboAttack = weaponToSet.comboAttack;
        comboAttackScales = weaponToSet.comboAttackScales;
        chargedAttackDamageScale = weaponToSet.chargedAttackDamageScale;
        chargedAttackStaminaCost = weaponToSet.chargedAttackStaminaCost;
        additionalWeaponStatType = weaponToSet.additionalWeaponStatType;
        additionalWeaponStatTypeScale = weaponToSet.additionalWeaponStatTypeScale;
        weaponPassiveTalent = weaponToSet.weaponPassiveTalent;
    }
}

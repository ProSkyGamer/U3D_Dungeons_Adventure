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

    [Range(1, 6)] public int comboAttack = 1;
    public List<float> comboAttackScales = new();
    public float chargedAttackDamageScale;
    public float chargedAttackStaminaCost = 25f;
    public PlayerEffects.PlayerBuff.Buffs additionalWeaponStatType = PlayerEffects.PlayerBuff.Buffs.AtkBuff;
    public float additionalWeaponStatTypeScale = 0.1f;
}
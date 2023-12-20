using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu]
public class WeaponSO : ScriptableObject, INetworkSerializable
{
    public enum WeaponType
    {
        Katana,
        Gun
    }

    [Header("All scales must be in this variant (0.753)")]
    public WeaponType weaponType;

    public Transform weaponVisual;

    public PlayerEffectsController.AllPlayerEffects additionalWeaponStatType =
        PlayerEffectsController.AllPlayerEffects.AtkIncrease;

    public float additionalWeaponStatTypeScale = 0.1f;
    public List<PlayerEffectsController.AppliedEffect> weaponPassiveTalent;
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

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref weaponType);
        serializer.SerializeValue(ref comboAttack);
        serializer.SerializeValue(ref chargedAttackDamageScale);
        serializer.SerializeValue(ref chargedAttackStaminaCost);
        serializer.SerializeValue(ref additionalWeaponStatType);
        serializer.SerializeValue(ref additionalWeaponStatTypeScale);
    }
}
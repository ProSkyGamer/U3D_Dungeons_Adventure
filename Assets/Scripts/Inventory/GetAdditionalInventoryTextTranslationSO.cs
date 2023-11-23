using UnityEngine;

public class GetAdditionalInventoryTextTranslationSo : MonoBehaviour
{
    [Header("Object Type's")]
    [SerializeField] private TextTranslationsSO noTypeTextTranslationSo;

    [SerializeField] private TextTranslationsSO relicTextTranslationSo;
    [SerializeField] private TextTranslationsSO weaponTextTranslationSo;

    [Header("Weapon Type's")]
    [SerializeField] private TextTranslationsSO noTypeWeaponTypeTextTranslationSo;

    [SerializeField] private TextTranslationsSO katanaWeaponTypeTextTranslationSo;
    [SerializeField] private TextTranslationsSO gunWeaponTypeTextTranslationSo;

    [Header("Weapon Additional Stats")]
    [SerializeField] private TextTranslationsSO noStatTypeTextTranslationSo;

    [SerializeField] private TextTranslationsSO atkTypeTextTranslationSo;
    [SerializeField] private TextTranslationsSO hpTypeTextTranslationSo;
    [SerializeField] private TextTranslationsSO defTypeTextTranslationSo;
    [SerializeField] private TextTranslationsSO critRateTypeTextTranslationSo;
    [SerializeField] private TextTranslationsSO critDmgTextTypeTranslationSo;
    [SerializeField] private TextTranslationsSO naDmgBonusTextTypeTranslationSo;
    [SerializeField] private TextTranslationsSO caDmgBonusTypeTextTranslationSo;

    [Header("Relic Buffs")]
    [SerializeField] private TextTranslationsSO noRelicBuffTextTranslationSo;

    [SerializeField] private TextTranslationsSO relicDmgAbsorptionBuffTextTranslationSo;
    [SerializeField] private TextTranslationsSO relicTakenDmgIncreaseBuffTextTranslationSo;
    [SerializeField] private TextTranslationsSO relicNaDmgBuffTextTranslationSo;
    [SerializeField] private TextTranslationsSO relicCaDmgBuffTextTranslationSo;
    [SerializeField] private TextTranslationsSO relicExpBuffTextTranslationSo;
    [SerializeField] private TextTranslationsSO relicDefBuffTextTranslationSo;
    [SerializeField] private TextTranslationsSO relicHpBuffTextTranslationSo;
    [SerializeField] private TextTranslationsSO relicRegenerationPerKillBuffTextTranslationSo;

    public static GetAdditionalInventoryTextTranslationSo Instance;

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public TextTranslationsSO GetObjectTypeTextTranslationSoByInventoryObject(InventoryObject inventoryObject)
    {
        if (inventoryObject.TryGetRelicSo(out var _))
            return relicTextTranslationSo;
        if (inventoryObject.TryGetWeaponSo(out var _))
            return weaponTextTranslationSo;

        return noTypeTextTranslationSo;
    }

    public TextTranslationsSO GetWeaponTypeTextTranslationSoByInventoryObject(InventoryObject inventoryObject)
    {
        if (!inventoryObject.TryGetWeaponSo(out var weaponSo)) return noTypeWeaponTypeTextTranslationSo;

        switch (weaponSo.weaponType)
        {
            default:
                return noTypeWeaponTypeTextTranslationSo;
            case WeaponSO.WeaponType.Katana:
                return katanaWeaponTypeTextTranslationSo;
            case WeaponSO.WeaponType.Gun:
                return gunWeaponTypeTextTranslationSo;
        }
    }

    public TextTranslationsSO GetRelicPassiveTextTranslationSoByInventoryObject(PlayerEffects.RelicBuff relicBuff)
    {
        switch (relicBuff.relicBuffType)
        {
            default:
                return noRelicBuffTextTranslationSo;
            case PlayerEffects.RelicBuffTypes.TakenDmgAbsorption:
                return relicDmgAbsorptionBuffTextTranslationSo;
            case PlayerEffects.RelicBuffTypes.TakenDmgIncrease:
                return relicTakenDmgIncreaseBuffTextTranslationSo;
            case PlayerEffects.RelicBuffTypes.NaDmgBuff:
                return relicNaDmgBuffTextTranslationSo;
            case PlayerEffects.RelicBuffTypes.CaDmgBuff:
                return relicCaDmgBuffTextTranslationSo;
            case PlayerEffects.RelicBuffTypes.ExpBuff:
                return relicExpBuffTextTranslationSo;
            case PlayerEffects.RelicBuffTypes.DefBuff:
                return relicDefBuffTextTranslationSo;
            case PlayerEffects.RelicBuffTypes.HpBuff:
                return relicHpBuffTextTranslationSo;
            case PlayerEffects.RelicBuffTypes.HpRegeneratePerKill:
                return relicRegenerationPerKillBuffTextTranslationSo;
        }
    }

    public TextTranslationsSO GetWeaponAdditionalStatTypeTextTranslationSoByInventoryObject(
        InventoryObject inventoryObject)
    {
        if (!inventoryObject.TryGetWeaponSo(out var weaponSo)) return noStatTypeTextTranslationSo;

        switch (weaponSo.additionalWeaponStatType)
        {
            default:
                return noStatTypeTextTranslationSo;
            case PlayerEffects.PlayerBuff.Buffs.AtkBuff:
                return atkTypeTextTranslationSo;
            case PlayerEffects.PlayerBuff.Buffs.HpBuff:
                return hpTypeTextTranslationSo;
            case PlayerEffects.PlayerBuff.Buffs.DefBuff:
                return defTypeTextTranslationSo;
            case PlayerEffects.PlayerBuff.Buffs.CritRateBuff:
                return critRateTypeTextTranslationSo;
            case PlayerEffects.PlayerBuff.Buffs.CritDamageBuff:
                return critDmgTextTypeTranslationSo;
            case PlayerEffects.PlayerBuff.Buffs.NormalAttackDamageBonusBuff:
                return naDmgBonusTextTypeTranslationSo;
            case PlayerEffects.PlayerBuff.Buffs.ChargedAttackDamageBonusBuff:
                return caDmgBonusTypeTextTranslationSo;
        }
    }
}

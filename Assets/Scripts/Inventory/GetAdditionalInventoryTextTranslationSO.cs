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

    [SerializeField] private TextTranslationsSO dmgAbsorptionEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO takenDmgIncreaseEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO naDmgIncreaseEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO caDmgIncreaseEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO gainedExpIncreaseEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO atkIncreaseEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO defIncreaseEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO hpIncreaseEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO critRateIncreaseEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO onHitCritRateIncreaseEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO critDmgIncreaseEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO deathSavingEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO movementSpeedIncreaseEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO onEnemyDeathGainedCoinsIncreaseEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO staminaConsumptionDecreaseEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO skillPointsExpRequirementDecreaseEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO playerInventorySlotsIncreaseEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO weaponInventorySlotsIncreaseEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO relicInventorySlotsIncreaseEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO onHitEnemySpeedDecreaseEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO onHitEnemyPoisonEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO onHitEnemyStunEffectTextTranslationSo;
    [SerializeField] private TextTranslationsSO regenerationPerKillEffectTextTranslationSo;

    [Header("Effect Conditions")]
    [SerializeField] private TextTranslationsSO effectConditionStringStart;

    [SerializeField] private TextTranslationsSO effectConditionTypeHpMoreThen;
    [SerializeField] private TextTranslationsSO effectConditionTypeHpLessThen;
    [SerializeField] private TextTranslationsSO effectConditionTypeDefMoreThen;
    [SerializeField] private TextTranslationsSO effectConditionTypeDefLessThen;
    [SerializeField] private TextTranslationsSO effectConditionIfEndless;

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

    public TextTranslationsSO GetPlayerEffectTextTranslationSoByEffect(PlayerEffects.AppliedEffect effect)
    {
        switch (effect.appliedEffectType)
        {
            default:
                return noRelicBuffTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.TakenDmgDecrease:
                return dmgAbsorptionEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.TakenDmgIncrease:
                return takenDmgIncreaseEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.NADmgIncrease:
                return naDmgIncreaseEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.CADmgIncrease:
                return caDmgIncreaseEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.GainedExpIncrease:
                return gainedExpIncreaseEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.DefIncrease:
                return defIncreaseEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.HpIncrease:
                return hpIncreaseEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.HpRegenerationPerKill:
                return regenerationPerKillEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.AtkIncrease:
                return atkIncreaseEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.CritRateIncrease:
                return critRateIncreaseEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.OnHitEnemyCritRateIncrease:
                return onHitCritRateIncreaseEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.CritDamageIncrease:
                return critDmgIncreaseEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.DeathSaving:
                return deathSavingEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.MovementSpeedIncrease:
                return movementSpeedIncreaseEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.SkillPointExpRequirementDecrease:
                return skillPointsExpRequirementDecreaseEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.PlayerInventorySlotsIncrease:
                return playerInventorySlotsIncreaseEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.WeaponInventorySlotsIncrease:
                return weaponInventorySlotsIncreaseEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.RelicInventorySlotsIncrease:
                return relicInventorySlotsIncreaseEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.OnHitEnemySpeedDecrease:
                return onHitEnemySpeedDecreaseEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.OnHitEnemyPoison:
                return onHitEnemyPoisonEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.OnHitEnemyStun:
                return onHitEnemyStunEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.GainedCoinsOnEnemyDeathIncrease:
                return onEnemyDeathGainedCoinsIncreaseEffectTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.StaminaConsumptionDecrease:
                return staminaConsumptionDecreaseEffectTextTranslationSo;
        }
    }

    public string GetEffectConditionTextTranslationByEffect(PlayerEffects.AppliedEffect effect)
    {
        var fullConditionString = "";

        if (effect.effectCondition.conditionValue != 0f)
        {
            fullConditionString += TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                TextTranslationController.GetCurrentLanguage(), effectConditionStringStart);
            switch (effect.effectCondition.conditionType)
            {
                case PlayerEffects.AppliedEffect.EffectCondition.ConditionType.CurrentHpHigherThen:
                    fullConditionString += TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                        TextTranslationController.GetCurrentLanguage(), effectConditionTypeHpMoreThen);
                    break;
                case PlayerEffects.AppliedEffect.EffectCondition.ConditionType.CurrentHpLowerThen:
                    fullConditionString += TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                        TextTranslationController.GetCurrentLanguage(), effectConditionTypeHpLessThen);
                    break;
                case PlayerEffects.AppliedEffect.EffectCondition.ConditionType.DefHigherThen:
                    fullConditionString += TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                        TextTranslationController.GetCurrentLanguage(), effectConditionTypeDefMoreThen);
                    break;
                case PlayerEffects.AppliedEffect.EffectCondition.ConditionType.DefLowerThen:
                    fullConditionString += TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                        TextTranslationController.GetCurrentLanguage(), effectConditionTypeDefLessThen);
                    break;
            }

            fullConditionString += " {0}";
            if (!effect.effectCondition.isConditionValueFlat)
                fullConditionString += "%";

            if (effect.effectCondition.isCountEndless)
                fullConditionString += TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                    TextTranslationController.GetCurrentLanguage(), effectConditionIfEndless);

            fullConditionString += "\n";
        }

        return fullConditionString;
    }

    public TextTranslationsSO GetWeaponAdditionalStatTypeTextTranslationSoByInventoryObject(
        InventoryObject inventoryObject)
    {
        if (!inventoryObject.TryGetWeaponSo(out var weaponSo)) return noStatTypeTextTranslationSo;

        switch (weaponSo.additionalWeaponStatType)
        {
            default:
                return noStatTypeTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.AtkIncrease:
                return atkTypeTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.HpIncrease:
                return hpTypeTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.DefIncrease:
                return defTypeTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.CritRateIncrease:
                return critRateTypeTextTranslationSo;
            case PlayerEffects.AllPlayerEffects.CritDamageIncrease:
                return critDmgTextTypeTranslationSo;
            case PlayerEffects.AllPlayerEffects.NADmgIncrease:
                return naDmgBonusTextTypeTranslationSo;
            case PlayerEffects.AllPlayerEffects.CADmgIncrease:
                return caDmgBonusTypeTextTranslationSo;
        }
    }
}

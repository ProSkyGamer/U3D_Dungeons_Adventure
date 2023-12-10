using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    public event EventHandler<OnPlayerRelicOutOfUsagesCountEventArgs> OnPlayerRelicOutOfUsagesCount;

    public class OnPlayerRelicOutOfUsagesCountEventArgs : EventArgs
    {
        public AppliedEffect relicBuff;
    }

    public enum AllPlayerEffects
    {
        AtkIncrease, //ADD TEXT
        HpIncrease,
        DefIncrease,
        CritRateIncrease, //ADD TEXT
        OnHitEnemyCritRateIncrease, //ADD TEXT
        CritDamageIncrease, //ADD TEXT
        NADmgIncrease,
        CADmgIncrease,
        GainedExpIncrease,
        TakenDmgDecrease,
        TakenDmgIncrease,
        HpRegenerationPerKill,
        DeathSaving, //ADD TEXT
        MovementSpeedIncrease, //ADD TEXT
        GainedCoinsOnEnemyDeathIncrease, //ADD TEXT
        StaminaConsumptionDecrease, //ADD TEXT
        SkillPointExpRequirementDecrease, //NOT ADDED //ADD TEXT
        PlayerInventorySlotsIncrease, //ADD TEXT
        RelicInventorySlotsIncrease, //ADD TEXT
        WeaponInventorySlotsIncrease, //ADD TEXT
        OnHitEnemySpeedDecrease, //ADD TEXT
        OnHitEnemyPoison, //NOT ADDED //ADD TEXT
        OnHitEnemyStun //NOT ADDED //ADD TEXT
    }

    [Serializable]
    public class AppliedEffect
    {
        [Serializable]
        public class EffectCondition
        {
            public enum ConditionType
            {
                CurrentHpLowerThen,
                CurrentHpHigherThen,
                DefHigherThen,
                DefLowerThen
            }

            public ConditionType conditionType;
            public float conditionValue;
            public bool isConditionValueFlat;

            //For effects like: For each ... hp over ...
            public bool isCountEndless;
            public float stepSize;
            public bool isStepSizeFlat;
        }

        [HideInInspector] public int appliedEffectID;
        [HideInInspector] public int effectCurrentlyAppliedTimes;
        [HideInInspector] public int currentUsages;

        public AllPlayerEffects appliedEffectType;
        public float effectPercentageScale;
        public int effectFlatScale;

        public EffectCondition effectCondition;
        public bool isUsagesLimited;
        public int maxUsagesLimit;

        public float effectLimit;
        public float effectApplyingChance;
        public float applyingEffectDuration;
        public bool isEffectEndless = true;
        public float effectRemainingTime;
    }

    public class RelicBuffEffectTriggeredEventArgs : EventArgs
    {
        public int spentValue;
        public int effectID;
    }

    #region Buffs

    private readonly List<AppliedEffect> allPlayerAppliedEffects = new();

    #endregion

    private int lastUsedEffectID;

    private PlayerHealth playerHealth;
    private PlayerAttackController playerAttackController;
    private PlayerWeapons playerWeapons;
    private PlayerRelics playerRelics;
    private PlayerInventory playerInventory;
    private StaminaController staminaController;
    private PlayerController playerController;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerAttackController = GetComponent<PlayerAttackController>();
        playerWeapons = GetComponent<PlayerWeapons>();
        playerRelics = GetComponent<PlayerRelics>();
        playerInventory = GetComponent<PlayerInventory>();
        staminaController = GetComponent<StaminaController>();
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        playerWeapons.OnCurrentWeaponChange += PlayerAttackController_OnCurrentWeaponChange;

        playerRelics.OnRelicsChange += PlayerRelics_OnRelicsChange;

        PlayerHealth.OnCurrentPlayerHealthChange += OnAnyFollowingStatChangeChange;
        PlayerHealth.OnCurrentDefenceChange += OnAnyFollowingStatChangeChange;
    }

    private void OnAnyFollowingStatChangeChange(object sender, EventArgs e)
    {
        SetAllPlayerEffectsToControllerByCondition();
    }

    private void PlayerRelics_OnRelicsChange(object sender, PlayerRelics.OnRelicChangeEventArgs e)
    {
        if (e.addedRelic != null && !e.addedRelic.IsBroken())
        {
            e.addedRelic.TryGetRelicSo(out var relicSo);
            foreach (var relicApplyingEffect in relicSo.relicApplyingEffects)
                ApplyEffect(relicApplyingEffect);
        }

        if (e.removedRelic != null)
        {
            e.removedRelic.TryGetRelicSo(out var relicSo);
            foreach (var relicApplyingEffect in relicSo.relicApplyingEffects)
                DispelEffect(relicApplyingEffect);
        }
    }

    private void PlayerAttackController_OnCurrentWeaponChange(object sender,
        PlayerWeapons.OnCurrentWeaponChangeEventArgs e)
    {
        if (e.previousWeapon != null)
        {
            e.previousWeapon.TryGetWeaponSo(out var previousWeaponSo);
            var removingEffect = new AppliedEffect
            {
                appliedEffectType = previousWeaponSo.additionalWeaponStatType,
                effectPercentageScale = previousWeaponSo.additionalWeaponStatTypeScale
            };
            DispelEffect(removingEffect);
            foreach (var passiveEffect in previousWeaponSo.weaponPassiveTalent) DispelEffect(passiveEffect);
        }

        e.newWeapon.TryGetWeaponSo(out var newWeaponSo);
        ApplyEffect(newWeaponSo.additionalWeaponStatType, 0f, newWeaponSo.additionalWeaponStatTypeScale);
        foreach (var passiveEffect in newWeaponSo.weaponPassiveTalent) ApplyEffect(passiveEffect);
    }

    private void SetAllPlayerEffectsToControllerByCondition()
    {
        foreach (var appliedEffect in allPlayerAppliedEffects)
        {
            var timesEffectShouldBeApplied = GetEffectAppliedTimesByConditions(appliedEffect);

            if (appliedEffect.effectCurrentlyAppliedTimes == timesEffectShouldBeApplied) continue;

            var currentAppliedTimes = appliedEffect.effectCurrentlyAppliedTimes;
            var isRemoving = currentAppliedTimes > timesEffectShouldBeApplied;

            while (appliedEffect.effectCurrentlyAppliedTimes != timesEffectShouldBeApplied)
            {
                AddOrRemoveEffectToController(appliedEffect, !isRemoving);

                appliedEffect.effectCurrentlyAppliedTimes = isRemoving
                    ? appliedEffect.effectCurrentlyAppliedTimes--
                    : appliedEffect.effectCurrentlyAppliedTimes++;
            }
        }
    }

    private void Update()
    {
        if (GameStageManager.Instance.IsPause()) return;

        TickBuffTimers();
    }

    private void TickBuffTimers()
    {
        foreach (var effect in allPlayerAppliedEffects)
            if (!effect.isEffectEndless)
            {
                effect.effectRemainingTime -= Time.deltaTime;
                if (effect.effectRemainingTime <= 0) DispelEffect(effect);
            }
    }

    public void ApplyEffect(AllPlayerEffects applyingEffectType, float effectTotalDuration,
        float effectPercentageScale, int effectFlatScale = 0,
        int maxUsagesLimit = 0, float effectLimit = 0f, float effectApplyingChance = 0f,
        float applyingEffectDuration = 0f)
    {
        lastUsedEffectID++;

        var applyingEffect = new AppliedEffect
        {
            appliedEffectID = lastUsedEffectID,
            appliedEffectType = applyingEffectType,
            effectPercentageScale = effectPercentageScale,
            isUsagesLimited = maxUsagesLimit > 0,
            maxUsagesLimit = maxUsagesLimit,
            effectLimit = effectLimit,
            effectApplyingChance = effectApplyingChance,
            applyingEffectDuration = applyingEffectDuration,
            effectFlatScale = effectFlatScale,
            effectRemainingTime = effectTotalDuration,
            isEffectEndless = effectTotalDuration <= 0,
            effectCurrentlyAppliedTimes = 1
        };

        AddOrRemoveEffectToController(applyingEffect);
        allPlayerAppliedEffects.Add(applyingEffect);
    }

    public void ApplyEffect(AppliedEffect applyingEffect)
    {
        lastUsedEffectID++;

        applyingEffect.appliedEffectID = lastUsedEffectID;

        var applyingTimes = GetEffectAppliedTimesByConditions(applyingEffect);
        applyingEffect.effectCurrentlyAppliedTimes = applyingTimes;

        while (applyingTimes > 0)
        {
            AddOrRemoveEffectToController(applyingEffect);
            applyingTimes--;
        }

        allPlayerAppliedEffects.Add(applyingEffect);
    }

    private int GetEffectAppliedTimesByConditions(AppliedEffect appliedEffect)
    {
        var effectCondition = appliedEffect.effectCondition;

        var maxValue = 1f;
        var currentValue = 1f;
        var isHigherThen = false;

        if (appliedEffect.effectCondition == null || appliedEffect.effectCondition.conditionValue == 0f) return 1;

        switch (effectCondition.conditionType)
        {
            case AppliedEffect.EffectCondition.ConditionType.CurrentHpHigherThen:
                maxValue = playerHealth.GetCurrentMaxHp();
                currentValue = playerHealth.GetCurrentHealth();
                isHigherThen = true;
                break;
            case AppliedEffect.EffectCondition.ConditionType.CurrentHpLowerThen:
                maxValue = playerHealth.GetCurrentMaxHp();
                currentValue = playerHealth.GetCurrentHealth();
                break;
            case AppliedEffect.EffectCondition.ConditionType.DefHigherThen:
                maxValue = playerHealth.GetCurrentDefence();
                currentValue = maxValue;
                isHigherThen = true;
                break;
            case AppliedEffect.EffectCondition.ConditionType.DefLowerThen:
                maxValue = playerHealth.GetCurrentDefence();
                currentValue = maxValue;
                break;
        }

        var comparedValue = effectCondition.isConditionValueFlat ? currentValue : currentValue / maxValue;

        if (!effectCondition.isCountEndless)
            return isHigherThen ? comparedValue >= maxValue ? 1 : 0 :
                comparedValue <= maxValue ? 1 : 0;

        var conditionValue = comparedValue - effectCondition.conditionValue;
        if (effectCondition.isStepSizeFlat && !effectCondition.isConditionValueFlat)
            conditionValue *= maxValue;
        else if (!effectCondition.isStepSizeFlat && effectCondition.isConditionValueFlat)
            conditionValue /= maxValue;

        var effectAppliedTimes = 0;
        while (conditionValue > effectCondition.stepSize)
        {
            conditionValue -= effectCondition.stepSize;
            effectAppliedTimes++;
        }

        Debug.Log(effectAppliedTimes);

        return effectAppliedTimes;
    }

    private void DispelEffect(AppliedEffect removingEffect)
    {
        foreach (var appliedEffect in allPlayerAppliedEffects)
        {
            if (appliedEffect.appliedEffectType != removingEffect.appliedEffectType ||
                appliedEffect.isEffectEndless != removingEffect.isEffectEndless ||
                appliedEffect.effectPercentageScale != removingEffect.effectPercentageScale ||
                appliedEffect.effectFlatScale != removingEffect.effectFlatScale ||
                appliedEffect.isUsagesLimited != removingEffect.isUsagesLimited ||
                appliedEffect.effectLimit != removingEffect.effectLimit ||
                appliedEffect.effectApplyingChance != removingEffect.effectApplyingChance) continue;

            allPlayerAppliedEffects.Remove(appliedEffect);
            AddOrRemoveEffectToController(appliedEffect, false);
            break;
        }
    }

    private void AddOrRemoveEffectToController(AppliedEffect appliedEffect, bool isAdding = true)
    {
        var applyingEffectType = appliedEffect.appliedEffectType;
        var effectPercentageValue = appliedEffect.effectPercentageScale;
        var effectFlatValue = appliedEffect.effectFlatScale;
        var effectID = appliedEffect.appliedEffectID;
        var effectApplyingChance = appliedEffect.effectApplyingChance;
        var effectLimit = appliedEffect.effectLimit;
        var effectApplyingDuration = appliedEffect.applyingEffectDuration;
        effectPercentageValue = isAdding ? effectPercentageValue : -effectPercentageValue;
        var isLimited = appliedEffect.isUsagesLimited;

        switch (applyingEffectType)
        {
            case AllPlayerEffects.HpIncrease:
                playerHealth.ChangeHealthBuff(effectPercentageValue, effectFlatValue);
                break;
            case AllPlayerEffects.DefIncrease:
                playerHealth.ChangeDefenceBuff(effectPercentageValue, effectFlatValue);
                break;
            case AllPlayerEffects.AtkIncrease:
                playerAttackController.ChangeAttackBuff(effectPercentageValue, effectFlatValue);
                break;
            case AllPlayerEffects.CritRateIncrease:
                playerAttackController.ChangeCritRateBuff(effectPercentageValue);
                break;
            case AllPlayerEffects.CritDamageIncrease:
                playerAttackController.ChangeCritDamageBuff(effectPercentageValue);
                break;
            case AllPlayerEffects.NADmgIncrease:
                playerAttackController.ChangeNormalAttackBuff(effectPercentageValue);
                break;
            case AllPlayerEffects.CADmgIncrease:
                playerAttackController.ChangeChargedAttackBuff(effectPercentageValue);
                break;
            case AllPlayerEffects.TakenDmgDecrease:
                playerHealth.ChangeTakenDamageAbsorptionBuff(effectPercentageValue, effectID);
                if (isLimited)
                    if (!IsAlreadyHasCurrentLimitedBuff(applyingEffectType))
                        if (isAdding)
                            playerHealth.OnHealthAbsorptionTriggered += BuffedController_OnLimitedBuffEffectTriggered;
                        else
                            playerHealth.OnHealthAbsorptionTriggered -= BuffedController_OnLimitedBuffEffectTriggered;
                break;
            case AllPlayerEffects.TakenDmgIncrease:
                playerHealth.ChangeTakenDamageIncreaseDebuff(effectPercentageValue);
                break;
            case AllPlayerEffects.GainedExpIncrease:
                playerController.ChangeExpAdditionalMultiplayer(effectPercentageValue);
                break;
            case AllPlayerEffects.HpRegenerationPerKill:
                playerController.AddRegeneratingHpAfterEnemyDeath(effectPercentageValue, effectID);
                if (isLimited)
                    if (!IsAlreadyHasCurrentLimitedBuff(applyingEffectType))
                        if (isAdding)
                            playerController.OnPlayerRegenerateHpAfterEnemyDeath +=
                                BuffedController_OnLimitedBuffEffectTriggered;
                        else
                            playerController.OnPlayerRegenerateHpAfterEnemyDeath -=
                                BuffedController_OnLimitedBuffEffectTriggered;
                break;
            case AllPlayerEffects.PlayerInventorySlotsIncrease:
                playerInventory.ChangeInventorySize(playerInventory.GetMaxSlotsCount() +
                                                    (int)(effectPercentageValue * 100));
                break;
            case AllPlayerEffects.WeaponInventorySlotsIncrease:
                playerWeapons.ChangeInventorySize(playerWeapons.GetMaxSlotsCount() +
                                                  (int)(effectPercentageValue * 100));
                break;
            case AllPlayerEffects.RelicInventorySlotsIncrease:
                playerRelics.ChangeInventorySize(playerRelics.GetMaxSlotsCount() + (int)(effectPercentageValue * 100));
                break;
            case AllPlayerEffects.MovementSpeedIncrease:
                playerController.ChangeSpeedModifier(effectPercentageValue);
                break;
            case AllPlayerEffects.StaminaConsumptionDecrease:
                staminaController.ChangeStaminaSpendMultiplayer(-effectPercentageValue);
                break;
            case AllPlayerEffects.DeathSaving:
                playerHealth.ChangeDeathSavingEffect(effectPercentageValue, effectID);
                if (isLimited)
                    if (!IsAlreadyHasCurrentLimitedBuff(applyingEffectType))
                        if (isAdding)
                            playerHealth.OnDeathSavingEffectTriggered +=
                                BuffedController_OnLimitedBuffEffectTriggered;
                        else
                            playerHealth.OnDeathSavingEffectTriggered -=
                                BuffedController_OnLimitedBuffEffectTriggered;
                break;
            case AllPlayerEffects.OnHitEnemyCritRateIncrease:
                playerAttackController.ChangeCritRateOnHitIncreaseBuff(effectPercentageValue,
                    effectLimit, effectID);
                break;
            case AllPlayerEffects.OnHitEnemySpeedDecrease:
                playerAttackController.ChangeEnemySlowOnHit(effectPercentageValue, effectApplyingDuration,
                    effectApplyingChance, effectID);
                break;
            case AllPlayerEffects.SkillPointExpRequirementDecrease:
                break;
            case AllPlayerEffects.GainedCoinsOnEnemyDeathIncrease:
                playerController.ChangeCoinsPerKillMultiplayer(effectPercentageValue);
                break;
        }
    }

    private void BuffedController_OnLimitedBuffEffectTriggered(object sender, RelicBuffEffectTriggeredEventArgs e)
    {
        var relicsToDeleteList = new List<AppliedEffect>();

        for (var i = 0; i < allPlayerAppliedEffects.Count; i++)
            if (e.effectID == allPlayerAppliedEffects[i].appliedEffectID)
            {
                Debug.Log($"{allPlayerAppliedEffects[i].currentUsages} {allPlayerAppliedEffects[i].maxUsagesLimit}");
                allPlayerAppliedEffects[i].currentUsages += e.spentValue;
                if (allPlayerAppliedEffects[i].maxUsagesLimit <= allPlayerAppliedEffects[i].currentUsages)
                    relicsToDeleteList.Add(allPlayerAppliedEffects[i]);
            }

        foreach (var relicBuff in relicsToDeleteList)
        {
            if (relicBuff.appliedEffectType == AllPlayerEffects.DeathSaving)
                ApplyEffect(AllPlayerEffects.TakenDmgDecrease, relicBuff.applyingEffectDuration, 1f);

            OnPlayerRelicOutOfUsagesCount?.Invoke(this, new OnPlayerRelicOutOfUsagesCountEventArgs
            {
                relicBuff = relicBuff
            });
        }
    }

    private bool IsAlreadyHasCurrentLimitedBuff(AllPlayerEffects relicBuffType)
    {
        foreach (var appliedEffect in allPlayerAppliedEffects)
            if (appliedEffect.isUsagesLimited && appliedEffect.appliedEffectType == relicBuffType)
                return true;

        return false;
    }
}

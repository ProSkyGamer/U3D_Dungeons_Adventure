using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RelicSO : ScriptableObject
{
    public List<PlayerEffects.AppliedEffect> relicApplyingEffects = new();

    public void SetRelicSo(RelicSO relicToSet)
    {
        foreach (var relicEffect in relicToSet.relicApplyingEffects)
        {
            var applyingEffect = new PlayerEffects.AppliedEffect
            {
                appliedEffectType = relicEffect.appliedEffectType,
                effectPercentageScale = relicEffect.effectPercentageScale,
                isUsagesLimited = relicEffect.isUsagesLimited,
                maxUsagesLimit = relicEffect.maxUsagesLimit,
                effectLimit = relicEffect.effectLimit,
                effectApplyingChance = relicEffect.effectApplyingChance,
                applyingEffectDuration = relicEffect.applyingEffectDuration,
                effectFlatScale = relicEffect.effectFlatScale,
                effectRemainingTime = relicEffect.effectRemainingTime,
                isEffectEndless = relicEffect.isEffectEndless,
                effectCondition = relicEffect.effectCondition,
                currentUsages = 0,
                effectCurrentlyAppliedTimes = 0
            };
            relicApplyingEffects.Add(applyingEffect);
        }
    }
}

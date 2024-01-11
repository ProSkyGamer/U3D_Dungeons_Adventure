using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradeDescriptionSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI upgradeApplyingEffectsText;

    public void SetUpgradeEffect(List<PlayerEffectsController.AppliedEffect> upgradeEffects)
    {
        upgradeApplyingEffectsText.text =
            GetEffectsTextFromEffectList(upgradeEffects, out var effectLimitString);
    }

    private string GetEffectsTextFromEffectList(List<PlayerEffectsController.AppliedEffect> givenEffects,
        out string fullEffectLimitString)
    {
        var fullEffectString = "";
        fullEffectLimitString = "";

        foreach (var effect in givenEffects)
        {
            var fullEffectConditionString =
                GetAdditionalInventoryTextTranslationSo.Instance.GetEffectConditionTextTranslationByEffect(effect);

            fullEffectString += string.Format(fullEffectConditionString,
                effect.effectCondition is { isConditionValueFlat: true }
                    ? effect.effectCondition.conditionValue
                    : effect.effectCondition.conditionValue * 100,
                effect.effectCondition is { isStepSizeFlat: true }
                    ? effect.effectCondition.stepSize
                    : effect.effectCondition.stepSize * 100);

            var singleEffectString = TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                TextTranslationController.GetCurrentLanguage(),
                GetAdditionalInventoryTextTranslationSo.Instance.GetPlayerEffectTextTranslationSoByEffect(
                    effect));

            fullEffectString += string.Format(singleEffectString, effect.effectPercentageScale * 100,
                effect.effectApplyingChance, effect.effectLimit, effect.applyingEffectDuration);
            fullEffectString += "\n";
        }

        return fullEffectString;
    }
}

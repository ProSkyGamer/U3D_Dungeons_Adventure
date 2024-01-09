using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopItemDescription : MonoBehaviour
{
    #region Variables & References

    [SerializeField] private TextMeshProUGUI inventoryObjectName;
    [SerializeField] private TextMeshProUGUI inventoryObjectType;

    [SerializeField] private Transform relicInfoTransform;
    [SerializeField] private TextMeshProUGUI inventoryObjectRelicWhileEquippedText;
    [SerializeField] private TextTranslationsSO inventoryObjectRelicWhileEquippedTextTranslationSo;
    [SerializeField] private TextMeshProUGUI inventoryObjectRelicPassive;
    [SerializeField] private TextTranslationsSO inventoryObjectRelicUsagesLeftTextTranslationsSo;
    [SerializeField] private TextMeshProUGUI inventoryObjectRelicUsagesLeft;

    [SerializeField] private Transform experienceInfoTransform;
    [SerializeField] private TextMeshProUGUI experienceDescriptionText;
    [SerializeField] private TextTranslationsSO experienceTextTranslationSo;
    [SerializeField] private TextTranslationsSO experiencePercentTextTranslationSo;
    [SerializeField] private TextTranslationsSO levelTextTranslationSo;

    #endregion

    #region Shop Item

    public void SetShopItem(ShopItem shopItem)
    {
        inventoryObjectName.text =
            TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                TextTranslationController.GetCurrentLanguage(), shopItem.itemNameTextTranslationsSo);

        inventoryObjectType.text =
            TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                TextTranslationController.GetCurrentLanguage(), GetAdditionalShopItemsTextTranslationSO.Instance
                    .GetShopItemTypeTextTranslationSoByShopItem(shopItem));

        switch (shopItem.soldShopItemType)
        {
            case ShopItem.ShopItemType.Experience:
                SetExperienceData(shopItem);
                break;
            case ShopItem.ShopItemType.Level:
                SetExperienceData(shopItem);
                break;
            case ShopItem.ShopItemType.Relic:
                var inventoryObject = shopItem.inventoryObjectToSold;
                inventoryObject.TryGetRelicSo(out var relicSo);
                SetRelicData(relicSo);
                break;
            case ShopItem.ShopItemType.RelicReset:
                inventoryObject = shopItem.inventoryObjectToSold;
                inventoryObject.TryGetRelicSo(out relicSo);
                SetRelicData(relicSo);
                break;
        }
    }

    #endregion

    #region Experience

    private void SetExperienceData(ShopItem shopItem)
    {
        relicInfoTransform.gameObject.SetActive(false);
        if (shopItem == null)
        {
            experienceInfoTransform.gameObject.SetActive(false);
            return;
        }

        var experienceBaseString = "";
        if (shopItem.soldShopItemType == ShopItem.ShopItemType.Experience)
            experienceBaseString =
                TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                    TextTranslationController.GetCurrentLanguage(), experienceTextTranslationSo);
        else if (shopItem.soldShopItemType == ShopItem.ShopItemType.Level && shopItem.boughtItemValue < 1f)
            experienceBaseString = TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                TextTranslationController.GetCurrentLanguage(), experiencePercentTextTranslationSo);
        else if (shopItem.soldShopItemType == ShopItem.ShopItemType.Level && shopItem.boughtItemValue >= 1f)
            experienceBaseString = TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                TextTranslationController.GetCurrentLanguage(), levelTextTranslationSo);

        var boughtItemValue = shopItem.boughtItemValue;
        var fullExperienceString = string.Format(experienceBaseString,
            boughtItemValue < 1f ? boughtItemValue * 100 : boughtItemValue);

        experienceDescriptionText.text = fullExperienceString;
    }

    #endregion

    #region Relics

    private void SetRelicData(RelicSO relicSo)
    {
        experienceInfoTransform.gameObject.SetActive(false);
        if (relicSo == null)
        {
            relicInfoTransform.gameObject.SetActive(false);
            return;
        }

        inventoryObjectRelicWhileEquippedText.text =
            TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                TextTranslationController.GetCurrentLanguage(),
                inventoryObjectRelicWhileEquippedTextTranslationSo);

        inventoryObjectRelicPassive.text =
            GetEffectsTextFromEffectList(relicSo.relicApplyingEffects, out var effectLimitString);
        if (effectLimitString == "")
            inventoryObjectRelicUsagesLeft.gameObject.SetActive(false);

        inventoryObjectRelicUsagesLeft.text = effectLimitString;
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

            if (effect.isUsagesLimited)
                fullEffectLimitString = string.Format(
                    TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                        TextTranslationController.GetCurrentLanguage(),
                        inventoryObjectRelicUsagesLeftTextTranslationsSo),
                    effect.maxUsagesLimit - effect.currentUsages);
        }

        return fullEffectString;
    }

    #endregion
}

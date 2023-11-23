using TMPro;
using UnityEngine;

public class InventoryItemDescription : MonoBehaviour
{
    [SerializeField] private TextTranslationSingleUI inventoryObjectName;
    [SerializeField] private TextTranslationSingleUI inventoryObjectType;

    [SerializeField] private Transform weaponInfoTransform;
    [SerializeField] private TextMeshProUGUI inventoryObjectWeaponTypeText;
    [SerializeField] private TextTranslationsSO inventoryObjectWeaponTypeTextTranslationSo;
    [SerializeField] private TextTranslationSingleUI inventoryObjectWeaponType;
    [SerializeField] private TextMeshProUGUI inventoryObjectWeaponAdditionalStatTypeText;
    [SerializeField] private TextTranslationsSO inventoryObjectWeaponAdditionalStatTypeTextTranslationSo;
    [SerializeField] private TextMeshProUGUI inventoryObjectWeaponAdditionalStat;
    [SerializeField] private TextMeshProUGUI inventoryObjectWeaponScalesText;
    [SerializeField] private TextTranslationsSO inventoryObjectWeaponScalesTextTranslationSo;
    [SerializeField] private TextTranslationsSO inventoryObjectWeaponNormalAttackScalesTextTranslationsSo;
    [SerializeField] private TextTranslationsSO inventoryObjectWeaponChargedAttackScaleTextTranslationsSo;
    [SerializeField] private TextMeshProUGUI inventoryObjectWeaponAttackScales;

    [SerializeField] private Transform relicInfoTransform;
    [SerializeField] private TextMeshProUGUI inventoryObjectRelicWhileEquippedText;
    [SerializeField] private TextTranslationsSO inventoryObjectRelicWhileEquippedTextTranslationSo;
    [SerializeField] private TextMeshProUGUI inventoryObjectRelicPassive;
    [SerializeField] private TextTranslationsSO inventoryObjectRelicUsagesLeftTextTranslationsSo;
    [SerializeField] private TextMeshProUGUI inventoryObjectRelicUsagesLeft;

    public void SetInventoryObject(InventoryObject inventoryObject)
    {
        inventoryObjectName.ChangeTextTranslationSO(inventoryObject.GetInventoryObjectNameTextTranslationSo());

        inventoryObjectType.ChangeTextTranslationSO(
            GetAdditionalInventoryTextTranslationSo.Instance.GetObjectTypeTextTranslationSoByInventoryObject(
                inventoryObject));

        if (!inventoryObject.TryGetWeaponSo(out var weaponSo))
        {
            weaponInfoTransform.gameObject.SetActive(false);
        }
        else
        {
            inventoryObjectWeaponTypeText.text =
                TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                    TextTranslationController.GetCurrentLanguage(), inventoryObjectWeaponTypeTextTranslationSo);
            inventoryObjectWeaponAdditionalStatTypeText.text =
                TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                    TextTranslationController.GetCurrentLanguage(),
                    inventoryObjectWeaponAdditionalStatTypeTextTranslationSo);
            inventoryObjectWeaponScalesText.text =
                TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                    TextTranslationController.GetCurrentLanguage(), inventoryObjectWeaponScalesTextTranslationSo);

            inventoryObjectWeaponType.ChangeTextTranslationSO(GetAdditionalInventoryTextTranslationSo.Instance
                .GetWeaponTypeTextTranslationSoByInventoryObject(inventoryObject));
            var additionalStatNameText = TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                TextTranslationController.GetCurrentLanguage(),
                GetAdditionalInventoryTextTranslationSo.Instance
                    .GetWeaponAdditionalStatTypeTextTranslationSoByInventoryObject(inventoryObject));
            inventoryObjectWeaponAdditionalStat.text = $"<align=left>{additionalStatNameText} " +
                                                       $"<align=right> {weaponSo.additionalWeaponStatTypeScale * 100}%";

            var singleNormalAttackWeaponScaleText = TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                TextTranslationController.GetCurrentLanguage(),
                inventoryObjectWeaponNormalAttackScalesTextTranslationsSo);
            var singleChargeAttackWeaponScaleText = TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                TextTranslationController.GetCurrentLanguage(),
                inventoryObjectWeaponChargedAttackScaleTextTranslationsSo);
            var displayedString = "";
            for (var i = 0; i < weaponSo.comboAttack; i++)
            {
                displayedString += string.Format(singleNormalAttackWeaponScaleText, i + 1,
                    weaponSo.comboAttackScales[i] * 100);
                displayedString += "\n";
            }

            displayedString += "\n";
            displayedString +=
                string.Format(singleChargeAttackWeaponScaleText, weaponSo.chargedAttackDamageScale * 100);

            inventoryObjectWeaponAttackScales.text = displayedString;
        }

        if (!inventoryObject.TryGetRelicSo(out var relicSo))
        {
            relicInfoTransform.gameObject.SetActive(false);
        }
        else
        {
            inventoryObjectRelicWhileEquippedText.text =
                TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                    TextTranslationController.GetCurrentLanguage(), inventoryObjectRelicWhileEquippedTextTranslationSo);

            var relicPassiveString = "";
            inventoryObjectRelicUsagesLeft.gameObject.SetActive(false);
            foreach (var relicBuff in relicSo.relicBuffs)
            {
                var relicBuffString = TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                    TextTranslationController.GetCurrentLanguage(),
                    GetAdditionalInventoryTextTranslationSo.Instance.GetRelicPassiveTextTranslationSoByInventoryObject(
                        relicBuff));

                relicPassiveString += string.Format(relicBuffString, relicBuff.relicBuffScale * 100);
                relicPassiveString += "\n";

                if (relicBuff.isHasLimit)
                {
                    inventoryObjectRelicUsagesLeft.gameObject.SetActive(true);
                    inventoryObjectRelicUsagesLeft.text = string.Format(
                        TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                            TextTranslationController.GetCurrentLanguage(),
                            inventoryObjectRelicUsagesLeftTextTranslationsSo),
                        relicBuff.maxUsagesLimit - relicBuff.currentUsages);
                }
            }

            inventoryObjectRelicPassive.text = relicPassiveString;
        }
    }
}

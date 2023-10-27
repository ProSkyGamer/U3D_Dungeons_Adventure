using System;
using TMPro;
using UnityEngine;

public class StatsTabUI : MonoBehaviour
{
    [SerializeField] private CharacterInventoryUI characterInventoryUI;

    [SerializeField] private TextMeshProUGUI baseHpText;
    [SerializeField] private TextMeshProUGUI additionalHpText;
    [SerializeField] private TextMeshProUGUI baseAtkText;
    [SerializeField] private TextMeshProUGUI additionalAtkText;
    [SerializeField] private TextMeshProUGUI baseDefText;
    [SerializeField] private TextMeshProUGUI additionalDefText;
    [SerializeField] private TextMeshProUGUI critRateText;
    [SerializeField] private TextMeshProUGUI critDmgText;
    [SerializeField] private TextMeshProUGUI naDmgBonusText;
    [SerializeField] private TextMeshProUGUI caDmgBonusText;

    private void Start()
    {
        CharacterUI.OnStatsTabButtonClick += CharacterUI_OnStatsTabButtonClick;
        CharacterUI.OnUpgradesTabButtonClick += CharacterUI_OnOtherTabButtonClick;
        CharacterUI.OnWeaponsTabButtonClick += CharacterUI_OnOtherTabButtonClick;
    }

    #region SubscribedEvents

    private void CharacterUI_OnOtherTabButtonClick(object sender, EventArgs e)
    {
        Hide();
    }

    private void CharacterUI_OnStatsTabButtonClick(object sender, EventArgs e)
    {
        Show();
    }

    #endregion

    private void Show()
    {
        gameObject.SetActive(true);

        UpdatePageVisual();
    }

    private void UpdatePageVisual()
    {
        UpdateStats();
        characterInventoryUI.UpdateInventory();
    }

    private void UpdateStats()
    {
        baseHpText.text = PlayerController.Instance.GetBaseHp().ToString();
        additionalHpText.text = PlayerController.Instance.GetCurrentAdditionalHp().ToString();
        baseAtkText.text = PlayerController.Instance.GetBaseAttack().ToString();
        additionalAtkText.text = PlayerController.Instance.GetCurrentAdditionalAttack().ToString();
        baseDefText.text = PlayerController.Instance.GetBaseDefence().ToString();
        additionalDefText.text = PlayerController.Instance.GetCurrentAdditionalDefence().ToString();
        critRateText.text = $"{PlayerController.Instance.GetCurrentCritRate().ToString()} %";
        critDmgText.text = $"{PlayerController.Instance.GetCurrentCritDmg().ToString()} %";
        naDmgBonusText.text = $"+ {PlayerController.Instance.GetCurrentNaDmgBonus().ToString()} %";
        caDmgBonusText.text = $"+ {PlayerController.Instance.GetCurrentCaDmgBonus().ToString()} %";
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

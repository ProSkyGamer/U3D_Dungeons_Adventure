using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsTabUI : MonoBehaviour
{
    #region Variables & References

    [SerializeField] private CharacterInventoryUI characterInventoryUI;

    [SerializeField] private TextMeshProUGUI currentOwnedCoinsTextValue;

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
    [SerializeField] private Image skillPointExperienceBar;
    [SerializeField] private TextMeshProUGUI skillPointExperienceText;
    [SerializeField] private Image levelExperienceBar;
    [SerializeField] private TextMeshProUGUI levelExperienceText;

    #endregion

    #region Initialization & Subscribed events

    private void Start()
    {
        CharacterUI.OnStatsTabButtonClick += CharacterUI_OnStatsTabButtonClick;
        CharacterUI.OnUpgradesTabButtonClick += CharacterUI_OnOtherTabButtonClick;
        CharacterUI.OnWeaponsTabButtonClick += CharacterUI_OnOtherTabButtonClick;
        CharacterUI.OnRelicsTabButtonClick += CharacterUI_OnOtherTabButtonClick;

        PlayerController.OnPlayerSpawned += PlayerController_OnPlayerSpawned;
    }

    private void PlayerController_OnPlayerSpawned(object sender, EventArgs e)
    {
        PlayerController.Instance.OnCoinsValueChange += PlayerController_OnCoinsValueChange;
        PlayerController.Instance.OnExperienceChange += PlayerController_OnExperienceChange;
    }

    private void PlayerController_OnExperienceChange(object sender, PlayerController.OnExperienceChangeEventArgs e)
    {
        skillPointExperienceText.text = $"{e.neededSkillPointExp - e.currentSkillPointExp}";
        var skillPointExpFillAmount = e.currentSkillPointExp / (float)e.neededSkillPointExp;
        skillPointExperienceBar.fillAmount = skillPointExpFillAmount;

        levelExperienceText.text = $"{e.neededLevelExp - e.currentLevelExp}";
        var levelExpFillAmount = e.currentLevelExp / (float)e.neededLevelExp;
        levelExperienceBar.fillAmount = levelExpFillAmount;
    }

    private void PlayerController_OnCoinsValueChange(object sender, EventArgs e)
    {
        UpdateStats();
    }

    private void CharacterUI_OnOtherTabButtonClick(object sender, EventArgs e)
    {
        Hide();
    }

    private void CharacterUI_OnStatsTabButtonClick(object sender, EventArgs e)
    {
        Show();
    }

    #endregion

    #region Tab Visual

    private void Show()
    {
        gameObject.SetActive(true);

        UpdatePageVisual();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void UpdatePageVisual()
    {
        UpdateOtherData();
        UpdateStats();
        characterInventoryUI.UpdateInventory();
    }

    private void UpdateOtherData()
    {
        currentOwnedCoinsTextValue.text = PlayerController.Instance.GetCurrentCoinsValue().ToString();

        var currentSkillPointExp = PlayerController.Instance.GetCurrentSkillPointExperience();
        var neededSkillPointExp = PlayerController.Instance.GetExperienceForSkillPoint();
        skillPointExperienceText.text = $"{neededSkillPointExp - currentSkillPointExp}";
        var skillPointExpFillAmount = currentSkillPointExp / (float)neededSkillPointExp;
        skillPointExperienceBar.fillAmount = skillPointExpFillAmount;

        var currentLevelExp = PlayerController.Instance.GetCurrentLevelExperience();
        var neededLevelExp = PlayerController.Instance.GetExperienceForCurrentLevel();
        levelExperienceText.text = $"{neededLevelExp - currentLevelExp}";
        var levelExpFillAmount = currentLevelExp / (float)neededLevelExp;
        levelExperienceBar.fillAmount = levelExpFillAmount;
    }

    private void UpdateStats()
    {
        currentOwnedCoinsTextValue.text = PlayerController.Instance.GetCurrentCoinsValue().ToString();

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

    #endregion
}

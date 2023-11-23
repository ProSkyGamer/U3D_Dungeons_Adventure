using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TempUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthBarValue;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI experienceText;
    [SerializeField] private Image experienceBarValue;
    [SerializeField] private Transform skillPointsNotification;
    [SerializeField] private TextMeshProUGUI skillPointsText;
    [SerializeField] private CharacterInventoryUI relicsInventory;

    private void Start()
    {
        PlayerHealth.OnCurrentPlayerHealthChange += PlayerHealth_OnCurrentPlayerHealthChange;

        StaminaController.OnStaminaChange += StaminaController_OnStaminaChange;

        PlayerController.Instance.OnExperienceChange += PlayerController_OnExperienceChange;
        PlayerController.Instance.OnSkillPointsValueChange += PlayerController_OnSkillPointsValueChange;

        GameStageManager.Instance.OnGamePause += GameStageManager_OnGamePause;

        ShopUI.Instance.OnShopOpen += OnOtherTabOpen;
        ShopUI.Instance.OnShopClose += OnOtherTabClose;

        CharacterUI.OnCharacterUIOpen += OnOtherTabOpen;
        CharacterUI.OnCharacterUIClose += OnOtherTabClose;
    }

    private void OnOtherTabClose(object sender, EventArgs e)
    {
        Show();
    }

    private void OnOtherTabOpen(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameStageManager_OnGamePause(object sender, EventArgs e)
    {
        if (GameStageManager.Instance.IsPause()) Hide();
        else Show();
    }

    private void PlayerController_OnSkillPointsValueChange(object sender, EventArgs e)
    {
        var skillPointsValue = PlayerController.Instance.GetCurrentSkillPointsValue();
        if (skillPointsValue != 0)
        {
            skillPointsNotification.gameObject.SetActive(true);
            skillPointsText.text = $"{PlayerController.Instance.GetCurrentSkillPointsValue()}";
        }
        else
        {
            skillPointsNotification.gameObject.SetActive(false);
        }
    }

    private void PlayerController_OnExperienceChange(object sender, PlayerController.OnExperienceChangeEventArgs e)
    {
        experienceText.text = $"{e.currentXp} / {e.maxXp}";
        var fillAmount = e.currentXp / (float)e.maxXp;
        experienceBarValue.fillAmount = fillAmount;
    }

    private void StaminaController_OnStaminaChange(object sender, StaminaController.OnStaminaChangeEventArgs e)
    {
        staminaText.text = $"{e.currentStamina} / {e.maxStamina}";
    }

    private void PlayerHealth_OnCurrentPlayerHealthChange(object sender,
        PlayerHealth.OnCurrentPlayerHealthChangeEventArgs e)
    {
        healthText.text = $"{e.currentHealth} / {e.maxHealth}";
        var fillAmount = e.currentHealth / (float)e.maxHealth;
        healthBarValue.fillAmount = fillAmount;
    }

    private void Show()
    {
        gameObject.SetActive(true);

        relicsInventory.UpdateInventory();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

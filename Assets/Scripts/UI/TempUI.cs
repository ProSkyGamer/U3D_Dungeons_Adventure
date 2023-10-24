using System;
using TMPro;
using UnityEngine;

public class TempUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI experienceText;
    [SerializeField] private TextMeshProUGUI skillPointsText;

    private void Start()
    {
        PlayerHealth.OnCurrentPlayerHealthChange += PlayerHealth_OnCurrentPlayerHealthChange;

        StaminaController.OnStaminaChange += StaminaController_OnStaminaChange;

        PlayerController.Instance.OnExperienceChange += PlayerController_OnExperienceChange;
        PlayerController.Instance.OnSkillPointsValueChange += PlayerController_OnSkillPointsValueChange;
    }

    private void PlayerController_OnSkillPointsValueChange(object sender, EventArgs e)
    {
        skillPointsText.text = $"{PlayerController.Instance.GetCurrentSkillPointsValue()}";
    }

    private void PlayerController_OnExperienceChange(object sender, PlayerController.OnExperienceChangeEventArgs e)
    {
        experienceText.text = $"{e.currentXp} / {e.maxXp}";
    }

    private void StaminaController_OnStaminaChange(object sender, StaminaController.OnStaminaChangeEventArgs e)
    {
        staminaText.text = $"{e.currentStamina} / {e.maxStamina}";
    }

    private void PlayerHealth_OnCurrentPlayerHealthChange(object sender,
        PlayerHealth.OnCurrentPlayerHealthChangeEventArgs e)
    {
        healthText.text = $"{e.currentHealth} / {e.maxHealth}";
    }
}

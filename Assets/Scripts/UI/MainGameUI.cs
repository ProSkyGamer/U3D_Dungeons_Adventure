using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MainGameUI : NetworkBehaviour
{
    [SerializeField] private Transform healthBarPrefab;
    [SerializeField] private Transform healthBarsLayoutGroup;
    private readonly Dictionary<PlayerHealthController, HealthBarUI> allPlayersHealthBars = new();
    [SerializeField] private Transform staminaBar;
    [SerializeField] private Image staminaBarValue;
    [SerializeField] private TextMeshProUGUI experienceText;
    [SerializeField] private Image experienceBarValue;
    [SerializeField] private Transform skillPointsNotification;
    [SerializeField] private TextMeshProUGUI skillPointsText;
    [SerializeField] private CharacterInventoryUI relicsInventory;

    private bool isFirstUpdate;

    private void Start()
    {
        healthBarPrefab.gameObject.SetActive(false);

        AllConnectedPlayers.Instance.OnNewPlayerConnected += AllConnectedPlayers_OnNewPlayerConnected;
        var allConnectedPlayerControllers = AllConnectedPlayers.Instance.GetAllPlayerControllers();
        foreach (var connectPlayerController in allConnectedPlayerControllers)
        {
            var newPlayerHealthController = connectPlayerController.GetPlayerHealthController();

            SubscribeToNewPlayerHealthController(newPlayerHealthController);
        }

        GameStageManager.Instance.OnGamePause += GameStageManager_OnGamePause;

        ShopUI.Instance.OnShopOpen += OnOtherTabOpen;
        ShopUI.Instance.OnShopClose += OnOtherTabClose;

        CharacterUI.OnCharacterUIOpen += OnOtherTabOpen;
        CharacterUI.OnCharacterUIClose += OnOtherTabClose;
    }

    private void AllConnectedPlayers_OnNewPlayerConnected(object sender,
        AllConnectedPlayers.OnNewPlayerConnectedEventArgs e)
    {
        var newPlayerHealthController = e.newConnectedPlayerController.GetPlayerHealthController();

        SubscribeToNewPlayerHealthController(newPlayerHealthController);
    }

    private void SubscribeToNewPlayerHealthController(PlayerHealthController newPlayerHealthController)
    {
        var newPlayerHealthBar = Instantiate(healthBarPrefab, healthBarsLayoutGroup);
        newPlayerHealthBar.gameObject.SetActive(true);
        var newHealthBarUI = newPlayerHealthBar.GetComponent<HealthBarUI>();

        allPlayersHealthBars.Add(newPlayerHealthController, newHealthBarUI);

        newPlayerHealthController.OnCurrentPlayerHealthChange += PlayerHealth_OnCurrentPlayerHealthChange;
    }

    public override void OnNetworkSpawn()
    {
        isFirstUpdate = true;
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
        staminaBar.gameObject.SetActive(true);

        var fillAmount = e.currentStamina / (float)e.maxStamina;
        staminaBarValue.fillAmount = fillAmount;

        if (fillAmount == 1f)
            staminaBar.gameObject.SetActive(false);
    }

    private void PlayerHealth_OnCurrentPlayerHealthChange(object sender,
        PlayerHealthController.OnCurrentPlayerHealthChangeEventArgs e)
    {
        var playerHealthController = sender as PlayerHealthController;
        if (playerHealthController == null) return;

        var healthBar = allPlayersHealthBars[playerHealthController];

        healthBar.ChangeHealthBarValue(e.currentHealth, e.maxHealth);
    }

    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;

            PlayerController.Instance.GetPlayerStaminaController().OnStaminaChange += StaminaController_OnStaminaChange;

            PlayerController.Instance.OnExperienceChange += PlayerController_OnExperienceChange;
            PlayerController.Instance.OnSkillPointsValueChange += PlayerController_OnSkillPointsValueChange;
        }
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

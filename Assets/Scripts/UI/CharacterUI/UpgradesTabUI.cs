using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UpgradesTabUI : NetworkBehaviour
{
    public static UpgradesTabUI Instance { get; private set; }

    #region Events

    public event EventHandler<OnNewUpgradeBoughtEventArgs> OnNewUpgradeBought;

    public class OnNewUpgradeBoughtEventArgs : EventArgs
    {
        public int boughtUpgradeID;
    }

    #endregion

    #region Created Classes

    [Serializable]
    public class UpgradeButtons
    {
        public PlayerEffectsController.AllPlayerEffects buffType = PlayerEffectsController.AllPlayerEffects.AtkIncrease;
        public float buffValue = 0.1f;
    }

    #endregion

    #region Variables & References

    [SerializeField] private Transform allUpgradesField;
    [SerializeField] private float draggingSensitivity = 2f;
    [SerializeField] private Vector2 movingPartSize = new(3000, 2000);
    [SerializeField] private float distanceBetweenUpgrades = 150f;

    [SerializeField] private TextMeshProUGUI availableSkillPointsText;

    [SerializeField] private List<UpgradeSingleUI> allAvailableUpgrades = new();
    [SerializeField] private float minUpgradesFieldSize = 0.5f;
    [SerializeField] private float maxUpgradesFieldSize = 2.5f;

    private bool isDragging;
    private bool isFirstUpdate;

    private Transform currentUpgradeDescription;

    #endregion

    #region Inititalization & Subscribed events

    public override void OnNetworkSpawn()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        PlayerController.OnPlayerSpawned += PlayerController_OnPlayerSpawned;

        UpgradeSingleUI.OnShowUpgradeDescription += UpgradeSingleUI_OnShowUpgradeDescription;
        UpgradeSingleUI.OnStopShowingUpgradeDescription += UpgradeSingleUI_OnStopShowingUpgradeDescription;
    }

    private void UpgradeSingleUI_OnStopShowingUpgradeDescription(object sender, EventArgs e)
    {
        if (currentUpgradeDescription != null)
        {
            Destroy(currentUpgradeDescription.gameObject);
            currentUpgradeDescription = null;
        }
    }

    private void UpgradeSingleUI_OnShowUpgradeDescription(object sender,
        UpgradeSingleUI.OnShowUpgradeDescriptionEventArgs e)
    {
        if (currentUpgradeDescription != null)
        {
            Destroy(currentUpgradeDescription.gameObject);
            currentUpgradeDescription = null;
        }

        var upgradesDescriptionPrefab = GetAdditionalUIPrefabs.Instance.GetUpgradeDescriptionPrefab();
        var currentMousePosition = GameInput.Instance.GetCurrentMousePosition();
        var newUpgradesDescription =
            Instantiate(upgradesDescriptionPrefab, currentMousePosition, Quaternion.identity, transform);
        var newUpgradesDescriptionSingleUI = newUpgradesDescription.GetComponent<UpgradeDescriptionSingleUI>();
        newUpgradesDescriptionSingleUI.SetUpgradeEffect(e.upgradeAppliedEffects);
        currentUpgradeDescription = newUpgradesDescription;
    }

    private void PlayerController_OnPlayerSpawned(object sender, EventArgs e)
    {
        isFirstUpdate = true;
        PlayerController.Instance.OnNewUpgradeBought += PlayerController_OnNewUpgradeBought;

        PlayerController.OnPlayerSpawned -= PlayerController_OnPlayerSpawned;
    }

    private void PlayerController_OnNewUpgradeBought(object sender, PlayerController.OnNewUpgradeBoughtEventArgs e)
    {
        foreach (var availableUpgrade in allAvailableUpgrades)
        {
            if (availableUpgrade.GetUpgradeID() != e.boughtUpgradeID) continue;

            availableUpgrade.SetUpgradeAsBought();
        }
    }

    private void Start()
    {
        GameInput.Instance.OnUpgradesStartDragging += GameInput_OnUpgradesStartDragging;

        CharacterUI.OnUpgradesTabButtonClick += CharacterUI_OnUpgradesTabButtonClick;
        CharacterUI.OnStatsTabButtonClick += CharacterUI_OnOtherTabButtonClick;
        CharacterUI.OnWeaponsTabButtonClick += CharacterUI_OnOtherTabButtonClick;
        CharacterUI.OnRelicsTabButtonClick += CharacterUI_OnOtherTabButtonClick;

        for (var i = 0; i < allAvailableUpgrades.Count; i++)
        {
            var availableUpgrade = allAvailableUpgrades[i];
            availableUpgrade.SetUpgradeID(i);
            availableUpgrade.OnUpgradeBuy += AvailableUpgrade_OnUpgradeBuy;
        }
    }

    private void AvailableUpgrade_OnUpgradeBuy(object sender, EventArgs e)
    {
        var availableUpgrade = sender as UpgradeSingleUI;
        if (availableUpgrade == null) return;

        OnNewUpgradeBought?.Invoke(this, new OnNewUpgradeBoughtEventArgs
        {
            boughtUpgradeID = availableUpgrade.GetUpgradeID()
        });

        availableUpgrade.OnUpgradeBuy -= AvailableUpgrade_OnUpgradeBuy;
    }

    private void PlayerController_OnSkillPointsValueChange(object sender, EventArgs e)
    {
        availableSkillPointsText.text = PlayerController.Instance.GetCurrentSkillPointsValue().ToString();
    }

    #region SubscribedEvents

    private void GameInput_OnUpgradesStartDragging(object sender, EventArgs e)
    {
        isDragging = true;
    }

    private void CharacterUI_OnOtherTabButtonClick(object sender, EventArgs e)
    {
        Hide();
    }

    private void CharacterUI_OnUpgradesTabButtonClick(object sender, EventArgs e)
    {
        Show();
    }

    #endregion

    #endregion

    #region Update

    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;

            PlayerController.Instance.OnSkillPointsValueChange += PlayerController_OnSkillPointsValueChange;
        }

        var currentUpgradesFieldSize = allUpgradesField.localScale;
        if (isDragging)
            if (GameInput.Instance.GetBindingValue(GameInput.Binding.UpgradesStartDragging) == 1f)
            {
                var mouseDelta = GameInput.Instance.GetMousePosition() * draggingSensitivity;

                var desiredPosition = allUpgradesField.position + (Vector3)mouseDelta;

                var screenSize = new Vector2(Screen.width, Screen.height);

                var minMaxXOffset = movingPartSize.x / 2 - screenSize.x / 2;
                if (desiredPosition.x - screenSize.x / 2 > minMaxXOffset ||
                    desiredPosition.x - screenSize.x / 2 < -minMaxXOffset)
                    desiredPosition.x = allUpgradesField.position.x;

                var minMaxYOffset = movingPartSize.y / 2 - screenSize.y / 2;
                if (desiredPosition.y - screenSize.y / 2 > minMaxYOffset ||
                    desiredPosition.y - screenSize.y / 2 < -minMaxYOffset)
                    desiredPosition.y = allUpgradesField.position.y;

                allUpgradesField.position = desiredPosition;
            }
            else
            {
                isDragging = false;
            }

        var mouseScroll = GameInput.Instance.GetMouseScroll() * Time.deltaTime;
        if (mouseScroll == 0) return;

        mouseScroll = mouseScroll > 0 ? 1 : -1;
        mouseScroll *= Time.deltaTime;
        var newUpgradesFieldSizeScale = Mathf.Clamp(currentUpgradesFieldSize.x +
                                                    mouseScroll, minUpgradesFieldSize, maxUpgradesFieldSize);
        var newUpgradesFieldSize =
            new Vector3(newUpgradesFieldSizeScale, newUpgradesFieldSizeScale, currentUpgradesFieldSize.z);
        allUpgradesField.localScale = newUpgradesFieldSize;
    }

    #endregion

    #region Tab Visual

    private void Show()
    {
        gameObject.SetActive(true);
        availableSkillPointsText.text = PlayerController.Instance.GetCurrentSkillPointsValue().ToString();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    #endregion

    #region Get Upgrades Data

    public List<PlayerEffectsController.AppliedEffect> GetUpgradeApplyingEffectsByUpgradeID(int upgradeID)
    {
        List<PlayerEffectsController.AppliedEffect> upgradeApplyingEffects = new();

        foreach (var upgradeSingle in allAvailableUpgrades)
        {
            if (upgradeSingle.GetUpgradeID() != upgradeID) continue;

            upgradeApplyingEffects = upgradeSingle.GetAllUpgradesApplyingEffect();
        }

        return upgradeApplyingEffects;
    }

    #endregion
}

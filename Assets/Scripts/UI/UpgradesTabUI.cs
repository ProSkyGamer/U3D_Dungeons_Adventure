using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradesTabUI : MonoBehaviour
{
    [SerializeField] private Transform allUpgradesField;
    [SerializeField] private float draggingSensitivity = 2f;
    [SerializeField] private Vector2 movingPartSize = new(3000, 2000);
    [SerializeField] private float distanceBetweenUpgrades = 150f;

    [SerializeField] private TextMeshProUGUI availableSkillPointsText;

    [SerializeField] private Transform lineBetweenUpgradeTransform;

    [SerializeField] private UpgradeSingleUI firstAtkUpgradeSingleUI;
    [SerializeField] private List<UpgradeButtons> allAtkUpgrades = new();
    [SerializeField] private UpgradeSingleUI firstHpUpgradeSingleUI;
    [SerializeField] private List<UpgradeButtons> allHpUpgrades = new();
    [SerializeField] private UpgradeSingleUI firstDefUpgradeSingleUI;
    [SerializeField] private List<UpgradeButtons> allDefUpgrades = new();
    [SerializeField] private UpgradeSingleUI firstCritRateUpgradeSingleUI;
    [SerializeField] private List<UpgradeButtons> allCritRateUpgrades = new();
    [SerializeField] private UpgradeSingleUI firstCritDmgUpgradeSingleUI;
    [SerializeField] private List<UpgradeButtons> allCritDmgUpgrades = new();

    [Serializable]
    public class UpgradeButtons
    {
        public PlayerEffects.PlayerBuff.Buffs buffType = PlayerEffects.PlayerBuff.Buffs.AtkBuff;
        public float buffValue = 0.1f;
    }

    private bool isDragging;

    private void Awake()
    {
        InitializeUpgradeLine(firstAtkUpgradeSingleUI, allAtkUpgrades);
        InitializeUpgradeLine(firstHpUpgradeSingleUI, allHpUpgrades);
        InitializeUpgradeLine(firstDefUpgradeSingleUI, allDefUpgrades);
        InitializeUpgradeLine(firstCritRateUpgradeSingleUI, allCritRateUpgrades);
        InitializeUpgradeLine(firstCritDmgUpgradeSingleUI, allCritDmgUpgrades);

        lineBetweenUpgradeTransform.gameObject.SetActive(false);
    }

    private void InitializeUpgradeLine(UpgradeSingleUI firstUpgrade, List<UpgradeButtons> allUpgrades)
    {
        var upgradeButtonTemplate = firstUpgrade.transform;
        var previousUpgrade = firstUpgrade;

        for (var i = 1; i < allUpgrades.Count; i++)
        {
            var upgradeTransform = Instantiate(upgradeButtonTemplate,
                upgradeButtonTemplate.position + new Vector3(distanceBetweenUpgrades * i, 0, 0),
                Quaternion.identity, allUpgradesField);

            var upgradeSingleUI = upgradeTransform.gameObject.GetComponent<UpgradeSingleUI>();

            upgradeSingleUI.SetUpgradeType(allUpgrades[i].buffType, allUpgrades[i].buffValue);
            upgradeSingleUI.AddLockUpgrade(previousUpgrade);

            Instantiate(lineBetweenUpgradeTransform,
                upgradeButtonTemplate.position +
                new Vector3(distanceBetweenUpgrades * i - distanceBetweenUpgrades / 2, 0, 0), Quaternion.identity,
                allUpgradesField);

            previousUpgrade = upgradeSingleUI;
        }

        firstUpgrade.SetUpgradeType(allUpgrades[0].buffType, allUpgrades[0].buffValue);
    }

    private void Start()
    {
        GameInput.Instance.OnUpgradesStartDragging += GameInput_OnUpgradesStartDragging;

        CharacterUI.OnUpgradesTabButtonClick += CharacterUI_OnUpgradesTabButtonClick;
        CharacterUI.OnStatsTabButtonClick += CharacterUI_OnOtherTabButtonClick;
        CharacterUI.OnWeaponsTabButtonClick += CharacterUI_OnOtherTabButtonClick;

        PlayerController.Instance.OnSkillPointsValueChange += PlayerController_OnSkillPointsValueChange;
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

    private void Update()
    {
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
    }

    private void Show()
    {
        gameObject.SetActive(true);
        availableSkillPointsText.text = PlayerController.Instance.GetCurrentSkillPointsValue().ToString();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

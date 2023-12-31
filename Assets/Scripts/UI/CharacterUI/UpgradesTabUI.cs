using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UpgradesTabUI : NetworkBehaviour
{
    [SerializeField] private Transform allUpgradesField;
    [SerializeField] private float draggingSensitivity = 2f;
    [SerializeField] private Vector2 movingPartSize = new(3000, 2000);
    [SerializeField] private float distanceBetweenUpgrades = 150f;

    [SerializeField] private TextMeshProUGUI availableSkillPointsText;

    [SerializeField] private Transform lineBetweenUpgradeTransform;
    [SerializeField] private float minUpgradesFieldSize = 0.5f;
    [SerializeField] private float maxUpgradesFieldSize = 2.5f;

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

    [SerializeField] private TextTranslationsSO atkTextTranslationsSo;
    [SerializeField] private TextTranslationsSO hpTextTranslationsSo;
    [SerializeField] private TextTranslationsSO defTextTranslationsSo;
    [SerializeField] private TextTranslationsSO critRateTextTranslationsSo;
    [SerializeField] private TextTranslationsSO critDmgTranslationsSo;

    [Serializable]
    public class UpgradeButtons
    {
        public PlayerEffectsController.AllPlayerEffects buffType = PlayerEffectsController.AllPlayerEffects.AtkIncrease;
        public float buffValue = 0.1f;
    }

    private bool isDragging;

    private bool isFirstUpdate;

    private void InitializeUpgradeLine(UpgradeSingleUI firstUpgrade, List<UpgradeButtons> allUpgrades,
        TextTranslationsSO lineTypeTextTranslationsSo)
    {
        var upgradeButtonTemplate = firstUpgrade.transform;
        var previousUpgrade = firstUpgrade;

        for (var i = 1; i < allUpgrades.Count; i++)
        {
            var upgradeTransform = Instantiate(upgradeButtonTemplate,
                upgradeButtonTemplate.position + new Vector3(distanceBetweenUpgrades * i, 0, 0),
                Quaternion.identity, allUpgradesField);

            var upgradeSingleUI = upgradeTransform.gameObject.GetComponent<UpgradeSingleUI>();

            upgradeSingleUI.SetUpgradeType(allUpgrades[i].buffType, allUpgrades[i].buffValue,
                lineTypeTextTranslationsSo, i);
            upgradeSingleUI.AddLockUpgrade(previousUpgrade);

            Instantiate(lineBetweenUpgradeTransform,
                upgradeButtonTemplate.position +
                new Vector3(distanceBetweenUpgrades * i - distanceBetweenUpgrades / 2, 0, 0), Quaternion.identity,
                allUpgradesField);

            previousUpgrade = upgradeSingleUI;
        }

        firstUpgrade.SetUpgradeType(allUpgrades[0].buffType, allUpgrades[0].buffValue, lineTypeTextTranslationsSo, 0);
    }

    public override void OnNetworkSpawn()
    {
        isFirstUpdate = true;
    }

    private void Start()
    {
        GameInput.Instance.OnUpgradesStartDragging += GameInput_OnUpgradesStartDragging;

        CharacterUI.OnUpgradesTabButtonClick += CharacterUI_OnUpgradesTabButtonClick;
        CharacterUI.OnStatsTabButtonClick += CharacterUI_OnOtherTabButtonClick;
        CharacterUI.OnWeaponsTabButtonClick += CharacterUI_OnOtherTabButtonClick;
        CharacterUI.OnRelicsTabButtonClick += CharacterUI_OnOtherTabButtonClick;
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
        if (isFirstUpdate)
        {
            isFirstUpdate = false;

            InitializeUpgradeLine(firstAtkUpgradeSingleUI, allAtkUpgrades, atkTextTranslationsSo);
            InitializeUpgradeLine(firstHpUpgradeSingleUI, allHpUpgrades, hpTextTranslationsSo);
            InitializeUpgradeLine(firstDefUpgradeSingleUI, allDefUpgrades, defTextTranslationsSo);
            InitializeUpgradeLine(firstCritRateUpgradeSingleUI, allCritRateUpgrades, critRateTextTranslationsSo);
            InitializeUpgradeLine(firstCritDmgUpgradeSingleUI, allCritDmgUpgrades, critDmgTranslationsSo);

            lineBetweenUpgradeTransform.gameObject.SetActive(false);

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
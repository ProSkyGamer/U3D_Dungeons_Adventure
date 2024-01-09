using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSingleUI : NetworkBehaviour
{
    #region Events

    public event EventHandler OnUpgradeBuy;

    #endregion

    #region Variables & References

    [SerializeField] private TextTranslationSingleUI upgradeTypeTextTranslationSingle;
    [SerializeField] private TextMeshProUGUI upgradeValueText;

    private PlayerEffectsController.AllPlayerEffects buffType;
    private float buffValue;
    private int itemUpgradeID;
    [SerializeField] private Transform lockedObjectTransform;

    private bool isBought;

    private readonly List<UpgradeSingleUI> upgradesThatLock = new();

    private Button upgradeButton;

    private bool isFirstUpdate;

    #endregion

    #region Inititalization & Subscribed events

    private void Awake()
    {
        upgradeButton = GetComponent<Button>();

        upgradeButton.onClick.AddListener(BuyUpgrade);
    }

    public override void OnNetworkSpawn()
    {
        PlayerController.OnPlayerSpawned += PlayerController_OnPlayerSpawned;
    }

    private void PlayerController_OnPlayerSpawned(object sender, EventArgs e)
    {
        isFirstUpdate = true;

        PlayerController.OnPlayerSpawned -= PlayerController_OnPlayerSpawned;
    }

    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;

            PlayerController.Instance.OnSkillPointsValueChange += PlayerController_OnSkillPointsValueChange;

            CharacterUI.OnStatsTabButtonClick += CharacterUI_OnStatsTabButtonClick;
        }
    }

    private void CharacterUI_OnStatsTabButtonClick(object sender, EventArgs e)
    {
        UpdateVisual();
    }

    private void PlayerController_OnSkillPointsValueChange(object sender, EventArgs e)
    {
        UpdateVisual();
    }

    #endregion

    #region Upgrade Methods

    private void BuyUpgrade()
    {
        PlayerController.Instance.SpendSkillPoints(1);
        PlayerController.Instance.GetPlayerEffects().ApplyEffect(
            buffType, 0, buffValue);
        isBought = true;

        PlayerBoughtUpgrades.AddBoughtUpgrade(buffType, buffValue, itemUpgradeID);

        OnUpgradeBuy?.Invoke(this, EventArgs.Empty);
        UpdateVisual();
    }

    private void UpgradeLock_OnUpgradeBuy(object sender, EventArgs e)
    {
        var upgradeSingle = sender as UpgradeSingleUI;

        upgradesThatLock.Remove(upgradeSingle);

        UpdateVisual();
    }

    #endregion

    #region Upgrade Initialization

    public void SetUpgradeType(PlayerEffectsController.AllPlayerEffects upgradeBuffType, float upgradeBuffValue,
        TextTranslationsSO upgradeTypeTextTranslationSo, int id)
    {
        if (buffValue != 0f) return;

        buffType = upgradeBuffType;
        buffValue = upgradeBuffValue;
        itemUpgradeID = id;

        upgradeTypeTextTranslationSingle.ChangeTextTranslationSO(upgradeTypeTextTranslationSo);
        upgradeValueText.text = $"{upgradeBuffValue * 100} %";

        if (!PlayerBoughtUpgrades.IsUpgradeAlreadyBought(buffType, buffValue, itemUpgradeID)) return;

        PlayerController.Instance.GetPlayerEffects().ApplyEffect(
            buffType, 0, buffValue);
        isBought = true;

        OnUpgradeBuy?.Invoke(this, EventArgs.Empty);
        UpdateVisual();
    }

    public void AddLockUpgrade(UpgradeSingleUI upgradeLock)
    {
        upgradesThatLock.Add(upgradeLock);

        upgradeLock.OnUpgradeBuy += UpgradeLock_OnUpgradeBuy;
        UpdateVisual();
    }

    #endregion

    #region Upgrade Visual

    private void UpdateVisual()
    {
        lockedObjectTransform.gameObject.SetActive(isBought || IsLocked() ||
                                                   PlayerController.Instance.GetCurrentSkillPointsValue() <= 0);

        if (upgradeButton == null)
            upgradeButton = GetComponent<Button>();
        upgradeButton.interactable =
            !isBought && !IsLocked() && PlayerController.Instance.GetCurrentSkillPointsValue() > 0;
    }

    #endregion

    #region Get Upgrade Data

    private bool IsLocked()
    {
        return upgradesThatLock.Count > 0;
    }

    #endregion
}

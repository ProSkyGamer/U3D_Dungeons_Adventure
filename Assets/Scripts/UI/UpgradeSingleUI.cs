using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSingleUI : MonoBehaviour
{
    public event EventHandler OnUpgradeBuy;

    [SerializeField] private TextMeshProUGUI upgradeTypeText;
    [SerializeField] private TextMeshProUGUI upgradeValueText;

    private PlayerEffects.PlayerBuff.Buffs buffType;
    private float buffValue;
    [SerializeField] private Transform lockedObjectTransform;

    private bool isBought;

    private readonly List<UpgradeSingleUI> upgradesThatLock = new();

    private Button upgradeButton;

    private void Awake()
    {
        upgradeButton = GetComponent<Button>();

        upgradeButton.onClick.AddListener(BuyUpgrade);
    }

    private void Start()
    {
        PlayerController.Instance.OnSkillPointsValueChange += PlayerController_OnSkillPointsValueChange;

        CharacterUI.OnStatsTabButtonClick += CharacterUI_OnStatsTabButtonClick;
    }

    private void CharacterUI_OnStatsTabButtonClick(object sender, EventArgs e)
    {
        UpdateVisual();
    }

    private void PlayerController_OnSkillPointsValueChange(object sender, EventArgs e)
    {
        UpdateVisual();
    }

    private void BuyUpgrade()
    {
        PlayerController.Instance.SpendSkillPoints(1);
        PlayerController.Instance.GetPlayerEffects().ApplyBuff(
            buffType, 0, true, buffValue);
        isBought = true;

        OnUpgradeBuy?.Invoke(this, EventArgs.Empty);
        UpdateVisual();
    }

    public void SetUpgradeType(PlayerEffects.PlayerBuff.Buffs upgradeBuffType, float upgradeBuffValue)
    {
        if (buffValue != 0f) return;

        buffType = upgradeBuffType;
        buffValue = upgradeBuffValue;

        var buffTypeString = "";
        switch (upgradeBuffType)
        {
            case PlayerEffects.PlayerBuff.Buffs.AtkBuff:
                buffTypeString = "Atk";
                break;
            case PlayerEffects.PlayerBuff.Buffs.DefBuff:
                buffTypeString = "Def";
                break;
            case PlayerEffects.PlayerBuff.Buffs.HpBuff:
                buffTypeString = "HP";
                break;
            case PlayerEffects.PlayerBuff.Buffs.CritRateBuff:
                buffTypeString = "Crit R.";
                break;
            case PlayerEffects.PlayerBuff.Buffs.CritDamageBuff:
                buffTypeString = "Crit D.";
                break;
        }

        upgradeTypeText.text = buffTypeString;
        upgradeValueText.text = $"{upgradeBuffValue * 100} %";
    }

    public void AddLockUpgrade(UpgradeSingleUI upgradeLock)
    {
        upgradesThatLock.Add(upgradeLock);

        upgradeLock.OnUpgradeBuy += UpgradeLock_OnUpgradeBuy;
        UpdateVisual();
    }

    private void UpgradeLock_OnUpgradeBuy(object sender, EventArgs e)
    {
        var upgradeSingle = sender as UpgradeSingleUI;

        upgradesThatLock.Remove(upgradeSingle);

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        lockedObjectTransform.gameObject.SetActive(isBought || IsLocked() ||
                                                   PlayerController.Instance.GetCurrentSkillPointsValue() <= 0);

        upgradeButton.interactable =
            !isBought && !IsLocked() && PlayerController.Instance.GetCurrentSkillPointsValue() > 0;
    }

    private bool IsLocked()
    {
        return upgradesThatLock.Count > 0;
    }
}

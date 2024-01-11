using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeSingleUI : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Events

    public event EventHandler OnUpgradeBuy;

    public static event EventHandler<OnShowUpgradeDescriptionEventArgs> OnShowUpgradeDescription;

    public class OnShowUpgradeDescriptionEventArgs : EventArgs
    {
        public List<PlayerEffectsController.AppliedEffect> upgradeAppliedEffects;
    }

    public static event EventHandler OnStopShowingUpgradeDescription;

    #endregion

    #region Variables & References

    [SerializeField] private List<PlayerEffectsController.AppliedEffect> upgradeApplyingEffects;

    private float buffValue;
    private int itemUpgradeID;
    [SerializeField] private Transform lockedObjectTransform;

    private bool isBought;

    [SerializeField] private List<UpgradeSingleUI> upgradesThatLock = new();

    private Button upgradeButton;

    private bool isFirstUpdate;

    #endregion

    #region Inititalization & Subscribed events

    private void Awake()
    {
        upgradeButton = GetComponent<Button>();

        upgradeButton.onClick.AddListener(BuyUpgrade);

        foreach (var upgradeThatLock in upgradesThatLock) upgradeThatLock.OnUpgradeBuy += UpgradeLock_OnUpgradeBuy;
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
        OnUpgradeBuy?.Invoke(this, EventArgs.Empty);
    }

    private void UpgradeLock_OnUpgradeBuy(object sender, EventArgs e)
    {
        var upgradeSingle = sender as UpgradeSingleUI;
        if (upgradeSingle == null) return;

        upgradesThatLock.Remove(upgradeSingle);

        upgradeSingle.OnUpgradeBuy -= UpgradeLock_OnUpgradeBuy;
    }

    public void SetUpgradeAsBought()
    {
        isBought = true;

        UpdateVisual();
    }

    public void SetUpgradeID(int newUpgradeID)
    {
        itemUpgradeID = newUpgradeID;
    }

    #endregion

    #region Upgrade Description

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnShowUpgradeDescription?.Invoke(this, new OnShowUpgradeDescriptionEventArgs
        {
            upgradeAppliedEffects = upgradeApplyingEffects
        });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnStopShowingUpgradeDescription?.Invoke(this, EventArgs.Empty);
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

    public List<PlayerEffectsController.AppliedEffect> GetAllUpgradesApplyingEffect()
    {
        return upgradeApplyingEffects;
    }

    public int GetUpgradeID()
    {
        return itemUpgradeID;
    }

    #endregion

    public static void ResetStaticData()
    {
        OnShowUpgradeDescription = null;
        OnStopShowingUpgradeDescription = null;
    }
}

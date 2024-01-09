using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : NetworkBehaviour
{
    #region Events

    public static event EventHandler OnCharacterUIOpen;
    public static event EventHandler OnCharacterUIClose;

    public static event EventHandler OnStatsTabButtonClick;
    public static event EventHandler OnUpgradesTabButtonClick;
    public static event EventHandler OnWeaponsTabButtonClick;
    public static event EventHandler OnRelicsTabButtonClick;

    #endregion

    #region Variables & References

    [SerializeField] private Button closeButton;
    [SerializeField] private Button statsTabButton;
    [SerializeField] private Button upgradesTabButton;
    [SerializeField] private Button weaponsTabButton;
    [SerializeField] private Button relicsTabButton;

    private bool isFirstUpdate;

    private bool isSubscribed;

    #endregion

    #region Initialization & Subscribed events

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);

        statsTabButton.onClick.AddListener(() => { OnStatsTabButtonClick?.Invoke(this, EventArgs.Empty); });
        upgradesTabButton.onClick.AddListener(() => { OnUpgradesTabButtonClick?.Invoke(this, EventArgs.Empty); });
        weaponsTabButton.onClick.AddListener(() => { OnWeaponsTabButtonClick?.Invoke(this, EventArgs.Empty); });
        relicsTabButton.onClick.AddListener(() => { OnRelicsTabButtonClick?.Invoke(this, EventArgs.Empty); });
    }

    private void Start()
    {
        ShopUI.Instance.OnShopOpen += OnOtherTabOpened;
        ShopUI.Instance.OnShopClose += OnOtherTabClose;

        PauseUI.OnSettingsButtonClick += OnOtherTabOpened;
        SettingsUI.OnSettingsClose += OnOtherTabClose;

        GiveCoinsUI.OnInterfaceShown += OnOtherTabOpened;
        GiveCoinsUI.OnInterfaceHidden += OnOtherTabClose;

        GameInput.Instance.OnOpenCharacterInfoAction += GameInput_OnOpenCharacterInfoAction;
        isSubscribed = true;
    }

    private void OnOtherTabOpened(object sender, EventArgs e)
    {
        GameInput.Instance.OnOpenCharacterInfoAction -= GameInput_OnOpenCharacterInfoAction;
        isSubscribed = false;
    }

    private void OnOtherTabClose(object sender, EventArgs e)
    {
        if (isSubscribed) return;

        GameInput.Instance.OnOpenCharacterInfoAction += GameInput_OnOpenCharacterInfoAction;
        isSubscribed = true;
    }

    private void GameInput_OnOpenCharacterInfoAction(object sender, EventArgs e)
    {
        Show();
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
            Hide();
        }
    }

    #endregion

    #region Tab Visual

    private void Show()
    {
        gameObject.SetActive(true);

        OnCharacterUIOpen?.Invoke(this, EventArgs.Empty);
        OnStatsTabButtonClick?.Invoke(this, EventArgs.Empty);

        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
    }

    private void Hide()
    {
        gameObject.SetActive(false);

        OnCharacterUIClose?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        Hide();

        GameInput.Instance.OnPauseAction -= GameInput_OnPauseAction;
    }

    #endregion

    public static void ResetStaticData()
    {
        OnStatsTabButtonClick = null;
        OnUpgradesTabButtonClick = null;
        OnWeaponsTabButtonClick = null;
        OnRelicsTabButtonClick = null;
        OnCharacterUIOpen = null;
        OnCharacterUIClose = null;
    }
}

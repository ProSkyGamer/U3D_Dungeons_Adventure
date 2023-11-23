using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    public static event EventHandler OnCharacterUIOpen;
    public static event EventHandler OnCharacterUIClose;

    [SerializeField] private Button closeButton;

    [SerializeField] private Button statsTabButton;
    public static event EventHandler OnStatsTabButtonClick;

    [SerializeField] private Button upgradesTabButton;
    public static event EventHandler OnUpgradesTabButtonClick;

    [SerializeField] private Button weaponsTabButton;
    public static event EventHandler OnWeaponsTabButtonClick;

    [SerializeField] private Button relicsTabButton;
    public static event EventHandler OnRelicsTabButtonClick;

    private bool isFirstUpdate = true;

    private bool isSubscribed;

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

    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
            Hide();
        }
    }

    private void GameInput_OnOpenCharacterInfoAction(object sender, EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);

        OnCharacterUIOpen?.Invoke(this, EventArgs.Empty);
        OnStatsTabButtonClick?.Invoke(this, EventArgs.Empty);

        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        Hide();

        GameInput.Instance.OnPauseAction -= GameInput_OnPauseAction;
    }

    private void Hide()
    {
        gameObject.SetActive(false);

        OnCharacterUIClose?.Invoke(this, EventArgs.Empty);
    }

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

using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    #region Events

    public static event EventHandler OnResumeButtonClick;
    public static event EventHandler OnSettingsButtonClick;

    public static event EventHandler OnInterfaceShown;
    public static event EventHandler OnInterfaceHidden;

    #endregion

    #region Variables & References

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;

    private bool isFirstUpdate;

    #endregion

    #region Initialization & Subscribed events

    private void Awake()
    {
        PlayerController.OnPlayerSpawned += PlayerController_OnPlayerSpawned;

        resumeButton.onClick.AddListener(() => { OnResumeButtonClick?.Invoke(this, EventArgs.Empty); });
        settingsButton.onClick.AddListener(() =>
        {
            OnSettingsButtonClick?.Invoke(this, EventArgs.Empty);
            Hide();
            SettingsUI.OnSettingsClose += SettingsUI_OnSettingsClose;
        });
        mainMenuButton.onClick.AddListener(() => { });
    }

    private void SettingsUI_OnSettingsClose(object sender, EventArgs e)
    {
        Show();

        SettingsUI.OnSettingsClose -= SettingsUI_OnSettingsClose;
    }

    private void Start()
    {
        GameStageManager.Instance.OnGamePause += GameStageManager_OnGamePause;
    }

    private void PlayerController_OnPlayerSpawned(object sender, EventArgs e)
    {
        isFirstUpdate = true;

        PlayerController.OnPlayerSpawned -= PlayerController_OnPlayerSpawned;
    }

    private void GameStageManager_OnGamePause(object sender, EventArgs e)
    {
        if (GameStageManager.Instance.IsPause()) Show();
        else Hide();
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

    #region Visual

    private void Show()
    {
        gameObject.SetActive(true);

        OnInterfaceShown?.Invoke(this, EventArgs.Empty);
    }

    private void Hide()
    {
        gameObject.SetActive(false);

        OnInterfaceHidden?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    public static void ResetStaticData()
    {
        OnResumeButtonClick = null;
        OnSettingsButtonClick = null;
        OnInterfaceShown = null;
        OnInterfaceHidden = null;
    }
}

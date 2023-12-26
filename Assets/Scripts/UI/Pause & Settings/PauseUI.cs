using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    public static event EventHandler OnResumeButtonClick;
    public static event EventHandler OnSettingsButtonClick;

    public static event EventHandler OnInterfaceShown;
    public static event EventHandler OnInterfaceHidden;

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;

    private bool isFirstUpdate = true;

    private void Awake()
    {
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

    public static void ResetStaticData()
    {
        OnResumeButtonClick = null;
        OnSettingsButtonClick = null;
    }
}

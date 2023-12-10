using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public static event EventHandler OnSettingsClose;

    [SerializeField] private Button closeSettingsButton;

    [SerializeField] private Button keymapsTabButton;
    public static event EventHandler OnKeymapsButtonClick;

    [SerializeField] private Button languagesTabButton;
    public static event EventHandler OnLanguagesButtonClick;

    [SerializeField] private Button minimapTabButton;
    public static event EventHandler OnMinimapButtonClick;

    private bool isFirstUpdate = true;

    private void Awake()
    {
        closeSettingsButton.onClick.AddListener(Hide);

        keymapsTabButton.onClick.AddListener(() => { OnKeymapsButtonClick?.Invoke(this, EventArgs.Empty); });
        languagesTabButton.onClick.AddListener(() => { OnLanguagesButtonClick?.Invoke(this, EventArgs.Empty); });
        minimapTabButton.onClick.AddListener(() => { OnMinimapButtonClick?.Invoke(this, EventArgs.Empty); });
    }

    private void Start()
    {
        PauseUI.OnSettingsButtonClick += PauseUI_OnSettingsButtonClick;
    }

    private void PauseUI_OnSettingsButtonClick(object sender, EventArgs e)
    {
        Show();
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

        OnKeymapsButtonClick?.Invoke(this, EventArgs.Empty);

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

        OnSettingsClose?.Invoke(this, EventArgs.Empty);
    }

    public static void ResetStaticData()
    {
        OnSettingsClose = null;
        OnKeymapsButtonClick = null;
        OnLanguagesButtonClick = null;
        OnMinimapButtonClick = null;
    }
}

using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuSettingsUI : MonoBehaviour
{
    #region Events

    public static event EventHandler OnSettingsClose;
    public static event EventHandler OnKeymapsButtonClick;
    public static event EventHandler OnLanguagesButtonClick;
    public static event EventHandler OnMinimapButtonClick;

    #endregion

    #region Variables & References

    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private Button keymapsTabButton;
    [SerializeField] private Button languagesTabButton;
    [SerializeField] private Button minimapTabButton;


    private bool isFirstUpdate = true;

    #endregion

    #region Initialization & Subscribed events

    private void Awake()
    {
        closeSettingsButton.onClick.AddListener(Hide);

        keymapsTabButton.onClick.AddListener(() => { OnKeymapsButtonClick?.Invoke(this, EventArgs.Empty); });
        languagesTabButton.onClick.AddListener(() => { OnLanguagesButtonClick?.Invoke(this, EventArgs.Empty); });
        minimapTabButton.onClick.AddListener(() => { OnMinimapButtonClick?.Invoke(this, EventArgs.Empty); });
    }

    private void Start()
    {
        MainMenuUI.OnMainMenuSettingsButtonTriggered += MainMenuUI_OnMainMenuSettingsButtonTriggered;
    }

    private void MainMenuUI_OnMainMenuSettingsButtonTriggered(object sender, EventArgs e)
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

    #endregion

    #region Visual

    private void Show()
    {
        gameObject.SetActive(true);

        OnKeymapsButtonClick?.Invoke(this, EventArgs.Empty);

        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
    }

    private void Hide()
    {
        gameObject.SetActive(false);

        OnSettingsClose?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        Hide();

        GameInput.Instance.OnPauseAction -= GameInput_OnPauseAction;
    }

    #endregion

    public static void ResetStaticData()
    {
        OnSettingsClose = null;
        OnKeymapsButtonClick = null;
        OnLanguagesButtonClick = null;
        OnMinimapButtonClick = null;
    }
}

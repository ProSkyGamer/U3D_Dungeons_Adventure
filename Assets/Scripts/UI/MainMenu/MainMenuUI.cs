using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    #region Events

    public static event EventHandler OnMainMenuSettingsButtonTriggered;
    public static event EventHandler OnStartSingleplayerButtonTriggered;
    public static event EventHandler OnStartMultiplayerButtonTriggered;

    #endregion

    #region Variables & References

    [SerializeField] private Button startSingleplayerButton;
    [SerializeField] private Button startMultiplayerButton;
    [SerializeField] private Button mainMenuSettingsButton;
    [SerializeField] private Button exitButton;

    #endregion

    #region Initialization & Subscribed events

    private void Awake()
    {
        startSingleplayerButton.onClick.AddListener(() =>
        {
            OnStartSingleplayerButtonTriggered?.Invoke(this, EventArgs.Empty);
        });

        startMultiplayerButton.onClick.AddListener(() =>
        {
            OnStartMultiplayerButtonTriggered?.Invoke(this, EventArgs.Empty);
            Hide();
        });

        mainMenuSettingsButton.onClick.AddListener(() =>
        {
            OnMainMenuSettingsButtonTriggered?.Invoke(this, EventArgs.Empty);
            Hide();
        });

        exitButton.onClick.AddListener(Application.Quit);
    }

    private void Start()
    {
        LobbyUI.OnLobbyUIClose += OnOtherInterfaceClose;
        MainMenuSettingsUI.OnSettingsClose += OnOtherInterfaceClose;
    }

    private void OnOtherInterfaceClose(object sender, EventArgs e)
    {
        Show();
    }

    #endregion

    #region Visual

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    #endregion

    public static void ResetStaticData()
    {
        OnMainMenuSettingsButtonTriggered = null;
        OnStartSingleplayerButtonTriggered = null;
        OnStartMultiplayerButtonTriggered = null;
    }
}

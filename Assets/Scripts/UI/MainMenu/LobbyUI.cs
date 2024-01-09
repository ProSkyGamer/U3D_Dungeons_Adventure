using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    #region Events

    public static event EventHandler OnLobbyUIClose;
    public static event EventHandler OnConnectByLobbyCodeButton;

    #endregion

    #region Variables & References

    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_InputField playerNameInputField;
    private string currentPlayerName;
    private const string SAVED_PLAYER_NAME_PLAYER_PREFS = "PlayerNamePlayerPrefs";
    [SerializeField] private Transform allFoundPublicLobbiesTransform;
    [SerializeField] private Transform foundLobbyPrefab;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button connectByLobbyCodeButton;

    [SerializeField] private TMP_InputField lobbyNameInputField;
    private string currentLobbyName;
    private const string SAVED_LOBBY_NAME_PLAYER_PREFS = "LobbyNamePlayerPrefs";
    [SerializeField] private Toggle isLobbyPublicToggle;
    private bool isCurrentLobbyPublic;
    private const string SAVED_LOBBY_PUBLIC_SETTINGS_PLAYER_PREFS = "LobbyPublicStatePlayerPrefs";
    [SerializeField] private Button createLobbyButton;

    private bool isFirstUpdate = true;

    #endregion

    #region Initialization

    private void Awake()
    {
        exitButton.onClick.AddListener(() =>
        {
            OnLobbyUIClose?.Invoke(this, EventArgs.Empty);
            Hide();
        });

        var savedPlayerName = PlayerPrefs.GetString(SAVED_PLAYER_NAME_PLAYER_PREFS, "U'r name");
        if (savedPlayerName != "")
        {
            playerNameInputField.text = savedPlayerName;
            currentPlayerName = savedPlayerName;
        }

        playerNameInputField.onValueChanged.AddListener(ChangePlayerName);

        connectByLobbyCodeButton.onClick.AddListener(() =>
        {
            //Input Esc UNSubscribe
            OnConnectByLobbyCodeButton?.Invoke(this, EventArgs.Empty);
        });

        quickJoinButton.onClick.AddListener(() => { GameLobby.Instance.QuickJoin(); });

        lobbyNameInputField.onValueChanged.AddListener(ChangeLobbyName);
        var savedLobbyName = PlayerPrefs.GetString(SAVED_LOBBY_NAME_PLAYER_PREFS, "Lobby name");
        if (savedLobbyName != "")
        {
            lobbyNameInputField.text = savedLobbyName;
            currentLobbyName = savedLobbyName;
        }

        isLobbyPublicToggle.onValueChanged.AddListener(ChangeLobbyPublicSettings);
        var savedLobbyPublicSettings = PlayerPrefs.GetInt(SAVED_LOBBY_PUBLIC_SETTINGS_PLAYER_PREFS, 1) == 1;
        if (savedLobbyPublicSettings)
        {
            isLobbyPublicToggle.isOn = savedLobbyPublicSettings;
            isCurrentLobbyPublic = savedLobbyPublicSettings;
        }

        createLobbyButton.onClick.AddListener(() =>
        {
            //Input Esc UNSubscribe
            GameLobby.Instance.CreateLobby(currentLobbyName, !isCurrentLobbyPublic);
        });
        createLobbyButton.interactable = currentPlayerName != "";
        foundLobbyPrefab.gameObject.SetActive(false);
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

    #region Subscribed events

    private void Start()
    {
        MainMenuUI.OnStartMultiplayerButtonTriggered += MainMenuUI_OnStartMultiplayerButtonTriggered;
        JoinByCodeUI.OnJoinByCodeClose += JoinByCodeUI_OnJoinByCodeClose;

        GameLobby.Instance.OnLobbyListChanged += KitchenGameLobby_OnLobbyListChanged;
    }

    private void JoinByCodeUI_OnJoinByCodeClose(object sender, EventArgs e)
    {
        //Input Esc Subscribe
    }

    private void MainMenuUI_OnStartMultiplayerButtonTriggered(object sender, EventArgs e)
    {
        Show();

        //Input Esc Subscribe
    }

    private void KitchenGameLobby_OnLobbyListChanged(object sender, GameLobby.OnLobbyListChangedEventArgs e)
    {
        UpdateAvailableLobbiesList(e.lobbyList);
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

    private void UpdateAvailableLobbiesList(List<Lobby> allLobbies)
    {
        var previousCreatedLobbies = allFoundPublicLobbiesTransform.GetComponentsInChildren<Transform>();

        foreach (var createdLobbiesTransform in previousCreatedLobbies)
        {
            if (createdLobbiesTransform == allFoundPublicLobbiesTransform ||
                createdLobbiesTransform == foundLobbyPrefab) continue;

            Destroy(createdLobbiesTransform.gameObject);
        }

        foreach (var foundLobby in allLobbies)
        {
            var newFoundLobby = Instantiate(foundLobbyPrefab, allFoundPublicLobbiesTransform);
            newFoundLobby.gameObject.SetActive(true);
            var foundLobbySingleUI = newFoundLobby.GetComponent<LobbySingleUI>();
            foundLobbySingleUI.SetLobby(foundLobby);
        }
    }

    #endregion

    #region Lobby Methods

    private void ChangePlayerName(string newName)
    {
        currentPlayerName = newName;

        PlayerPrefs.SetString(SAVED_PLAYER_NAME_PLAYER_PREFS, newName);

        GameMultiplayer.Instance.SetPlayerName(currentPlayerName);
    }

    private void ChangeLobbyName(string newName)
    {
        currentLobbyName = newName;

        PlayerPrefs.SetString(SAVED_LOBBY_NAME_PLAYER_PREFS, newName);

        createLobbyButton.interactable = currentPlayerName != "";
    }

    private void ChangeLobbyPublicSettings(bool newValue)
    {
        isCurrentLobbyPublic = newValue;

        PlayerPrefs.SetInt(SAVED_LOBBY_PUBLIC_SETTINGS_PLAYER_PREFS, newValue ? 1 : 0);
    }

    #endregion

    private void OnDestroy()
    {
        GameLobby.Instance.OnLobbyListChanged -= KitchenGameLobby_OnLobbyListChanged;
    }

    public static void ResetStaticData()
    {
        OnLobbyUIClose = null;
        OnConnectByLobbyCodeButton = null;
    }
}

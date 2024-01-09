using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinByCodeUI : MonoBehaviour
{
    #region Events

    public static event EventHandler OnJoinByCodeClose;

    #endregion

    #region Variables & References

    [SerializeField] private Button closeJoinByCodeButton;
    [SerializeField] private TMP_InputField lobbyCodeInputField;
    private string currentEnteredCode;
    [SerializeField] private Button joinLobbyButton;

    private bool isFirstUpdate = true;

    #endregion

    #region Initialization & Subscribed events

    private void Awake()
    {
        closeJoinByCodeButton.onClick.AddListener(() =>
        {
            Hide();
            OnJoinByCodeClose?.Invoke(this, EventArgs.Empty);
        });

        lobbyCodeInputField.onValueChanged.AddListener(ChangeLobbyCode);

        joinLobbyButton.onClick.AddListener(() =>
        {
            TryJoinLobby(currentEnteredCode);
            Hide();
        });
    }

    private void Start()
    {
        LobbyUI.OnConnectByLobbyCodeButton += LobbyUI_OnConnectByLobbyCodeButton;
    }

    private void LobbyUI_OnConnectByLobbyCodeButton(object sender, EventArgs e)
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
        //Input Esc Subscribe

        joinLobbyButton.interactable = false;

        gameObject.SetActive(true);
    }

    private void Hide()
    {
        //Input Esc UNSubscribe

        gameObject.SetActive(false);
    }

    #endregion

    #region Lobby Methods

    private void ChangeLobbyCode(string newCode)
    {
        currentEnteredCode = newCode;

        joinLobbyButton.interactable = newCode != "";
    }

    private void TryJoinLobby(string lobbyCode)
    {
        GameLobby.Instance.JoinWithCode(lobbyCode);
    }

    #endregion

    public static void ResetStaticData()
    {
        OnJoinByCodeClose = null;
    }
}

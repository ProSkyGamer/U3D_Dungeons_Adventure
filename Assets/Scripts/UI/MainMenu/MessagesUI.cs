using System;
using UnityEngine;
using UnityEngine.UI;

public class MessagesUI : MonoBehaviour
{
    #region Variables & References

    [SerializeField] private Transform createLobbyStartedNotificationTransform;
    [SerializeField] private Transform joinLobbyStartedNotificationTransform;
    [SerializeField] private Transform createLobbyFailedNotificationTransform;
    [SerializeField] private Transform quickJoinFailedNotificationTransform;
    [SerializeField] private Transform joinFailedNotificationTransform;
    [SerializeField] private Button closeNotificationButton;

    private bool isFirstUpdate = true;

    #endregion

    #region Inititlization & Subscribed events

    private void Awake()
    {
        closeNotificationButton.onClick.AddListener(() =>
        {
            HideAllNotifications();
            Hide();
        });
    }

    private void Start()
    {
        GameLobby.Instance.OnCreateLobbyStarted += GameLobby_OnCreateLobbyStarted;
        GameLobby.Instance.OnJoinStarted += GameLobby_OnJoinStarted;
        GameLobby.Instance.OnCreateLobbyFailed += GameLobby_OnCreateLobbyFailed;
        GameLobby.Instance.OnQuickJoinFailed += GameLobby_OnQuickJoinFailed;
        GameLobby.Instance.OnJoinFailed += GameLobby_OnJoinFailed;
    }

    private void GameLobby_OnJoinFailed(object sender, EventArgs e)
    {
        Show();

        HideAllNotifications();

        joinFailedNotificationTransform.gameObject.SetActive(true);

        closeNotificationButton.gameObject.SetActive(true);
    }

    private void GameLobby_OnQuickJoinFailed(object sender, EventArgs e)
    {
        Show();

        HideAllNotifications();

        quickJoinFailedNotificationTransform.gameObject.SetActive(true);

        closeNotificationButton.gameObject.SetActive(true);
    }

    private void GameLobby_OnCreateLobbyFailed(object sender, EventArgs e)
    {
        Show();

        HideAllNotifications();

        createLobbyFailedNotificationTransform.gameObject.SetActive(true);

        closeNotificationButton.gameObject.SetActive(true);
    }

    private void GameLobby_OnJoinStarted(object sender, EventArgs e)
    {
        Show();

        HideAllNotifications();

        joinLobbyStartedNotificationTransform.gameObject.SetActive(true);
    }

    private void GameLobby_OnCreateLobbyStarted(object sender, EventArgs e)
    {
        Show();

        HideAllNotifications();

        createLobbyStartedNotificationTransform.gameObject.SetActive(true);
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
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void HideAllNotifications()
    {
        createLobbyStartedNotificationTransform.gameObject.SetActive(false);
        joinLobbyStartedNotificationTransform.gameObject.SetActive(false);
        createLobbyFailedNotificationTransform.gameObject.SetActive(false);
        quickJoinFailedNotificationTransform.gameObject.SetActive(false);
        joinFailedNotificationTransform.gameObject.SetActive(false);

        closeNotificationButton.gameObject.SetActive(false);
    }

    #endregion
}

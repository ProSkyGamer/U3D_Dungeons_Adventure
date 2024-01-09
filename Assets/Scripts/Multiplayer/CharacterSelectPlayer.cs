using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPlayer : NetworkBehaviour
{
    #region Variables & References

    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject readyGameObject;
    [SerializeField] private Button kickPlayerButton;
    [SerializeField] private TextMeshPro playerNameText;

    #endregion

    #region Initialization & Subscribed events

    private void Awake()
    {
        kickPlayerButton.onClick.AddListener(() =>
        {
            var playerData =
                GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            GameLobby.Instance.KickPlayer(playerData.playerID.ToString());
            GameMultiplayer.Instance.KickPlayer(playerData.clientID);
        });
    }

    private void Start()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged +=
            KitchenGameManager_OnPlayerDataNetworkListChanged;
        CharacterSelectReady.Instance.OnReadyChanged +=
            CharacterSelectReady_OnReadyChanged;

        kickPlayerButton.gameObject.SetActive(NetworkManager.Singleton.IsServer &&
                                              playerIndex != 0);

        UpdatePlayer();
    }

    private void CharacterSelectReady_OnReadyChanged(object sender, EventArgs e)
    {
        UpdatePlayer();
    }

    private void KitchenGameManager_OnPlayerDataNetworkListChanged(object sender,
        EventArgs e)
    {
        UpdatePlayer();
    }

    #endregion

    #region Visibility

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    #endregion

    #region Update Player

    private void UpdatePlayer()
    {
        if (GameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex))
        {
            Show();
            var playerData =
                GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);

            readyGameObject.SetActive(
                CharacterSelectReady.Instance.IsPlayerReady(playerData.clientID));

            playerNameText.text = playerData.playerName.ToString();
        }
        else
        {
            Hide();
        }
    }

    #endregion

    private new void OnDestroy()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -=
            KitchenGameManager_OnPlayerDataNetworkListChanged;
    }
}

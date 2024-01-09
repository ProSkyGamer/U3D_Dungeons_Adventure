using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSelectReady : NetworkBehaviour
{
    #region Events

    public event EventHandler OnReadyChanged;

    #endregion

    #region Variables & References

    [SerializeField] private TextTranslationsSO notAllPlayersReadyTextTranslationsSo;

    private Dictionary<ulong, bool> playerReadyDictionary;

    #endregion

    public static CharacterSelectReady Instance { get; private set; }

    #region Initialization

    private void Awake()
    {
        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    #endregion

    #region Player Ready

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);

        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;
    }

    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientID)
    {
        playerReadyDictionary[clientID] = true;

        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Start Game

    public void StartGame()
    {
        if (!IsServer) return;

        StartGameServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartGameServerRpc()
    {
        var allClientReady = true;
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            if (!playerReadyDictionary.ContainsKey(clientId) ||
                !playerReadyDictionary[clientId])
            {
                allClientReady = false;
                break;
            }

        if (allClientReady)
        {
            GameLobby.Instance.DeleteLobby();
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }
        else
        {
            NotificationsUI.Instance.AddNotification(notAllPlayersReadyTextTranslationsSo);
        }
    }

    #endregion

    #region Get Player Ready

    public bool IsPlayerReady(ulong clientID)
    {
        return playerReadyDictionary.ContainsKey(clientID) &&
               playerReadyDictionary[clientID];
    }

    #endregion
}

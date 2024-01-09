using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameMultiplayer : NetworkBehaviour
{
    public static GameMultiplayer Instance { get; private set; }

    #region Events

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;
    public event EventHandler OnSinglePlayerServerStarted;

    #endregion

    #region Variables & References

    public const int MAX_PLAYER_AMOUNT = 4;
    private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";

    public static bool playerMultiplayer;

    [SerializeField] private List<Color> playerColorList;

    private NetworkList<PlayerData> playerDataNetworkList;
    private string playerName;

    #endregion

    #region Initialization & Subsctibed events

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER,
            "PlayerName" + Random.Range(100, 1000));

        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    private void PlayerDataNetworkList_OnListChanged(
        NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Start()
    {
        MainMenuUI.OnStartSingleplayerButtonTriggered += MainMenuUI_OnStartSingleplayerButtonTriggered;
        Loader.OnSingleplayerLoadingSceneOpened += Loader_OnSingleplayerLoadingSceneOpened;
    }

    private async void Loader_OnSingleplayerLoadingSceneOpened(object sender, EventArgs e)
    {
        var allocation = await AllocateRelay();
        NetworkManager.Singleton.GetComponent<UnityTransport>()
            .SetRelayServerData(new RelayServerData(allocation, "dtls"));
        StartHost();

        OnSinglePlayerServerStarted?.Invoke(this, EventArgs.Empty);
    }

    private void MainMenuUI_OnStartSingleplayerButtonTriggered(object sender, EventArgs e)
    {
        Loader.Load(Loader.Scene.GameScene, true);
    }

    #endregion

    #region Relay

    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            var allocation =
                await RelayService.Instance.CreateAllocationAsync(MAX_PLAYER_AMOUNT - 1);

            return allocation;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);

            return default;
        }
    }

    #endregion

    #region Player

    public string GetPlayerName()
    {
        return playerName;
    }

    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;

        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerName);
    }

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }

    public PlayerData GetPlayerDataFromClientID(ulong clientID)
    {
        foreach (var playerData in playerDataNetworkList)
            if (playerData.clientID == clientID)
                return playerData;

        return default;
    }

    public PlayerData GetPlayerData()
    {
        return GetPlayerDataFromClientID(NetworkManager.Singleton.LocalClient.ClientId);
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }

    public int GetPlayerDataIndexFromClientID(ulong clientID)
    {
        for (var i = 0; i < playerDataNetworkList.Count; i++)
            if (playerDataNetworkList[i].clientID == clientID)
                return i;

        return -1;
    }

    private bool IsColorAvailable(int colorID)
    {
        foreach (var playerData in playerDataNetworkList)
            if (playerData.colorID == colorID)
                return false;

        return true;
    }

    private int GetFirstUnusedColorID()
    {
        for (var i = 0; i < playerColorList.Count; i++)
            if (IsColorAvailable(i))
                return i;

        return -1;
    }

    #endregion

    #region Host & Approval

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback +=
            NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback +=
            NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback +=
            NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientID)
    {
        for (var i = 0; i < playerDataNetworkList.Count; i++)
        {
            var playerData = playerDataNetworkList[i];
            if (playerData.clientID == clientID) playerDataNetworkList.RemoveAt(i);
        }
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientID)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientID = clientID,
            colorID = GetFirstUnusedColorID()
        });
        SetPlayerNameServerRpc(GetPlayerName());
    }

    private void NetworkManager_ConnectionApprovalCallback(
        NetworkManager.ConnectionApprovalRequest connectionApprovalRequest,
        NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if (SceneManager.GetActiveScene().name !=
            Loader.Scene.CharacterSelectScene.ToString())
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started!";
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full!";
            return;
        }

        connectionApprovalResponse.Approved = true;
    }

    public void KickPlayer(ulong clientID)
    {
        NetworkManager.Singleton.DisconnectClient(clientID);
        NetworkManager_Server_OnClientDisconnectCallback(clientID);
    }

    #endregion

    #region Client

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback +=
            NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback +=
            NetworkManager_Client_OnClientConnectedCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientConnectedCallback(ulong clientID)
    {
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIDServerRpc(AuthenticationService.Instance.PlayerId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName,
        ServerRpcParams serverRpcParams = default)
    {
        var playerDataIndex =
            GetPlayerDataIndexFromClientID(serverRpcParams.Receive.SenderClientId);

        var playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerName = playerName;

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIDServerRpc(string playerID,
        ServerRpcParams serverRpcParams = default)
    {
        var playerDataIndex =
            GetPlayerDataIndexFromClientID(serverRpcParams.Receive.SenderClientId);

        var playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerID = playerID;

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong ClientID)
    {
        OnFailToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}

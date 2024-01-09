using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnPlayers : NetworkBehaviour
{
    #region Events

    public static event EventHandler OnAllPlayersSpawned;
    public static bool isAllPlayersSpawned;

    #endregion

    public static SpawnPlayers Instance { get; private set; }

    #region Variables & References

    [SerializeField] private Transform playerPrefab;

    private int waitingPlayersToSpawn;
    private int localWaitingPlayersToSpawn;

    #endregion

    #region Initialization & Subscribed events

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        if (!IsServer) return;

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (var clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID, true);
        }

        waitingPlayersToSpawn = NetworkManager.Singleton.ConnectedClientsIds.Count;
        SetWaitingPlayersCountClientRpc(waitingPlayersToSpawn);
    }

    [ClientRpc]
    private void SetWaitingPlayersCountClientRpc(int waitingPlayersCount)
    {
        localWaitingPlayersToSpawn += waitingPlayersCount;

        if (localWaitingPlayersToSpawn == 0)
            SetPlayerAsSpawnedServerRpc();
    }

    public void SetPlayerAsSpawned()
    {
        localWaitingPlayersToSpawn--;

        if (localWaitingPlayersToSpawn == 0)
            SetPlayerAsSpawnedServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerAsSpawnedServerRpc()
    {
        waitingPlayersToSpawn--;

        if (waitingPlayersToSpawn <= 0) OnAllPlayerSpawnedClientRpc();
    }

    [ClientRpc]
    private void OnAllPlayerSpawnedClientRpc()
    {
        OnAllPlayersSpawned?.Invoke(this, EventArgs.Empty);

        isAllPlayersSpawned = true;
    }

    #endregion

    public static void ResetStaticData()
    {
        OnAllPlayersSpawned = null;
    }
}

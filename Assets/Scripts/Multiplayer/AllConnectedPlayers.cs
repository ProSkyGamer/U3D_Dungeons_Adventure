using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AllConnectedPlayers : NetworkBehaviour
{
    public static AllConnectedPlayers Instance { get; private set; }

    public event EventHandler<OnNewPlayerConnectedEventArgs> OnNewPlayerConnected;

    public class OnNewPlayerConnectedEventArgs : EventArgs
    {
        public PlayerController newConnectedPlayerController;
    }

    [SerializeField] private List<PlayerController> allConnectedPlayerControllers = new();

    private bool isInitialized;

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        InitializeConnectedPlayerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void InitializeConnectedPlayerServerRpc()
    {
        var allPlayersNetworkObjectReferences = new NetworkObjectReference[allConnectedPlayerControllers.Count];

        for (var i = 0; i < allConnectedPlayerControllers.Count; i++)
            allPlayersNetworkObjectReferences[i] =
                new NetworkObjectReference(allConnectedPlayerControllers[i].GetPlayerNetworkObject());

        InitializeConnectedPlayerClientRpc(allPlayersNetworkObjectReferences);
    }

    [ClientRpc]
    private void InitializeConnectedPlayerClientRpc(NetworkObjectReference[] allPlayersNetworkObjectReferences)
    {
        if (IsServer) return;
        if (isInitialized) return;

        foreach (var playerNetworkObjectReference in allPlayersNetworkObjectReferences)
        {
            playerNetworkObjectReference.TryGet(out var playerNetworkObject);
            var playerController = playerNetworkObject.GetComponent<PlayerController>();

            if (allConnectedPlayerControllers.Contains(playerController)) continue;

            allConnectedPlayerControllers.Add(playerController);
            OnNewPlayerConnected?.Invoke(this, new OnNewPlayerConnectedEventArgs
            {
                newConnectedPlayerController = playerController
            });
        }

        isInitialized = true;
    }

    public void AddConnectedPlayerController(PlayerController newConnectedPlayerController)
    {
        if (!IsServer) return;

        var playerControllerNetworkObject = newConnectedPlayerController.GetPlayerNetworkObject();
        var playerControllerNetworkObjectReference = new NetworkObjectReference(playerControllerNetworkObject);

        AddConnectedPlayerControllerServerRpc(playerControllerNetworkObjectReference);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddConnectedPlayerControllerServerRpc(NetworkObjectReference playerControllerNetworkObjectReference)
    {
        AddConnectedPlayerControllerClientRpc(playerControllerNetworkObjectReference);
    }

    [ClientRpc]
    private void AddConnectedPlayerControllerClientRpc(NetworkObjectReference playerControllerNetworkObjectReference)
    {
        playerControllerNetworkObjectReference.TryGet(out var playerControllerNetworkObject);
        var newConnectedPlayerController = playerControllerNetworkObject.GetComponent<PlayerController>();

        if (allConnectedPlayerControllers.Contains(newConnectedPlayerController)) return;

        allConnectedPlayerControllers.Add(newConnectedPlayerController);

        OnNewPlayerConnected?.Invoke(this, new OnNewPlayerConnectedEventArgs
        {
            newConnectedPlayerController = newConnectedPlayerController
        });
    }

    public List<PlayerController> GetAllPlayerControllers()
    {
        return allConnectedPlayerControllers;
    }

    public int GetAllConnectedPlayerCount()
    {
        return allConnectedPlayerControllers.Count;
    }
}

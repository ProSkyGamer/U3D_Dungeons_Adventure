using System;
using Unity.Netcode;
using UnityEngine;

public class MerchantDungeonRoom : NetworkBehaviour
{
    [SerializeField] private Merchant merchantToSpawn;

    private void Awake()
    {
        StartingDungeonRoom.OnNavMeshBuild += StartingDungeonRoom_OnNavMeshBuild;
    }

    private void StartingDungeonRoom_OnNavMeshBuild(object sender, EventArgs e)
    {
        if (!IsServer) return;

        var merchantTransform = Instantiate(merchantToSpawn.transform, transform);
        var merchantNetworkObject = merchantTransform.GetComponent<NetworkObject>();
        merchantNetworkObject.Spawn();
    }
}

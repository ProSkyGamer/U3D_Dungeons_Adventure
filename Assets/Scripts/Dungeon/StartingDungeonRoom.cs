using System;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;

public class StartingDungeonRoom : InteractableItem
{
    public static event EventHandler OnDungeonStart;
    public static event EventHandler OnNavMeshBuild;

    private DungeonRoom dungeonRoom;
    [SerializeField] private NavMeshSurface navMeshSurface;

    private void Awake()
    {
        dungeonRoom = GetComponentInParent<DungeonRoom>();
    }

    public override void OnInteract(PlayerController player)
    {
        if (!IsServer) return;

        base.OnInteract(player);

        ProcedureDungeonGeneration.OnDungeonGenerationFinished +=
            ProcedureDungeonGeneration_OnDungeonGenerationFinished;

        UnlockDoorClientRpc();
    }

    private void ProcedureDungeonGeneration_OnDungeonGenerationFinished(object sender, EventArgs e)
    {
        dungeonRoom.UnlockAllStartExits();

        navMeshSurface.BuildNavMesh();
        OnNavMeshBuild?.Invoke(this, EventArgs.Empty);
    }

    [ClientRpc]
    private void UnlockDoorClientRpc()
    {
        OnDungeonStart?.Invoke(this, EventArgs.Empty);
    }

    public override bool IsCanInteract()
    {
        return isCanInteract && GameStageManager.Instance.IsWaitingForStart() && IsServer;
    }
}

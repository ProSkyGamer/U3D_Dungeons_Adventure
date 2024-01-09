using System;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;

public class StartingDungeonRoom : InteractableItem
{
    #region Events

    public static event EventHandler OnDungeonStart;
    public static event EventHandler OnNavMeshBuild;

    #endregion

    #region Vatiables & References

    private DungeonRoom dungeonRoom;
    [SerializeField] private NavMeshSurface navMeshSurface;

    #endregion

    #region Initialization

    private void Awake()
    {
        dungeonRoom = GetComponentInParent<DungeonRoom>();
    }

    #endregion

    #region Interactable Item

    public override void OnInteract(PlayerController player)
    {
        if (!IsServer) return;

        base.OnInteract(player);

        ProcedureDungeonGeneration.OnDungeonGenerationFinished +=
            ProcedureDungeonGeneration_OnDungeonGenerationFinished;

        UnlockDoorClientRpc();
    }

    public override bool IsCanInteract()
    {
        return isCanInteract && GameStageManager.Instance.IsWaitingForStart() && IsServer;
    }

    #endregion

    #region On Dungeon Start

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

    #endregion

    public static void ResetStaticData()
    {
        OnDungeonStart = null;
        OnNavMeshBuild = null;
    }
}

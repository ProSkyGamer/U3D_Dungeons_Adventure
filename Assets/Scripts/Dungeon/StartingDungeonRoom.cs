using System;
using Unity.AI.Navigation;
using UnityEngine;

public class StartingDungeonRoom : InteractableItem
{
    public static event EventHandler OnDungeonStart;

    private DungeonRoom dungeonRoom;
    [SerializeField] private NavMeshSurface navMeshSurface;

    private void Awake()
    {
        dungeonRoom = GetComponentInParent<DungeonRoom>();
    }

    public override void OnInteract(PlayerController player)
    {
        base.OnInteract(player);

        dungeonRoom.UnlockAllStartExits();

        navMeshSurface.BuildNavMesh();

        OnDungeonStart?.Invoke(this, EventArgs.Empty);
    }

    public override bool IsCanInteract()
    {
        return isCanInteract && GameStageManager.Instance.IsWaitingForStart();
    }
}

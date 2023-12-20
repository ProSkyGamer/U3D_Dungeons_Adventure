using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DungeonRoomSettings : NetworkBehaviour
{
    public event EventHandler OnAllEnemiesDefeated;

    [SerializeField] private List<Transform> enemiesToSpawn = new();

    [SerializeField] private Transform minimapRoomIcon;
    [SerializeField] private Transform minimapRoomIconCleared;

    private readonly List<EnemyController> remainingEnemies = new();

    private void Awake()
    {
        StartingDungeonRoom.OnNavMeshBuild += StartingDungeonRoom_OnNavMeshBuild;
    }

    private void StartingDungeonRoom_OnNavMeshBuild(object sender, EventArgs e)
    {
        if (!IsServer) return;

        foreach (var enemy in enemiesToSpawn)
        {
            var enemyTransform = Instantiate(enemy, transform.position, Quaternion.identity);
            enemyTransform.TryGetComponent(out EnemyController enemyController);
            var enemyNetworkObject = enemyTransform.GetComponent<NetworkObject>();
            enemyNetworkObject.Spawn();

            remainingEnemies.Add(enemyController);
            enemyController.OnEnemyDeath += EnemyController_OnEnemyDeath;
        }
    }

    private void EnemyController_OnEnemyDeath(object sender, EventArgs e)
    {
        var enemy = sender as EnemyController;

        remainingEnemies.Remove(enemy);

        if (remainingEnemies.Count == 0)
            OnAllEnemiesDefeated?.Invoke(this, EventArgs.Empty);
    }

    public void SetRoomAsClear()
    {
        minimapRoomIcon.gameObject.SetActive(false);
        minimapRoomIconCleared.gameObject.SetActive(true);
    }

    public bool IsHasAnyEnemiesToKill()
    {
        return enemiesToSpawn.Count > 0;
    }
}

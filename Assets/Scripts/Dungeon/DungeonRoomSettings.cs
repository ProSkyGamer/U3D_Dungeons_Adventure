using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DungeonRoomSettings : NetworkBehaviour
{
    #region Events

    public event EventHandler OnAllEnemiesDefeated;

    #endregion

    #region Variables & References

    [SerializeField] private List<Transform> enemiesToSpawn = new();

    [SerializeField] private Transform minimapRoomIcon;
    [SerializeField] private Transform minimapRoomIconCleared;

    private readonly List<EnemyController> remainingEnemies = new();

    #endregion

    #region Initialization & Subscribes events

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

    #endregion

    #region Room Methods

    public void SetRoomAsClear()
    {
        minimapRoomIcon.gameObject.SetActive(false);
        minimapRoomIconCleared.gameObject.SetActive(true);
    }

    #endregion

    #region Get Room Data

    public bool IsHasAnyEnemiesToKill()
    {
        return enemiesToSpawn.Count > 0;
    }

    #endregion
}

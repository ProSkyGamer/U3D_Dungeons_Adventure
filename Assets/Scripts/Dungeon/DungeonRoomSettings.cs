using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRoomSettings : MonoBehaviour
{
    [SerializeField] private List<Transform> enemiesToSpawn = new();

    [SerializeField] private Transform roomLootChest;
    private LootChest lootChest;
    [SerializeField] private bool isLootChetLocked = true;
    [SerializeField] private bool isChestSpawnedAfterDefeatingAllEnemies;

    private readonly List<EnemyController> enemiesToDefeatForUnlockingChest = new();


    private void Start()
    {
        StartingDungeonRoom.OnDungeonStart += StartingDungeonRoom_OnDungeonStart;
    }

    private void StartingDungeonRoom_OnDungeonStart(object sender, EventArgs e)
    {
        if (roomLootChest != null)
            if (!isChestSpawnedAfterDefeatingAllEnemies)
            {
                var lootChestTransform = Instantiate(roomLootChest,
                    transform.position, Quaternion.identity, transform);

                lootChestTransform.TryGetComponent(out lootChest);
                if (isLootChetLocked && enemiesToSpawn.Count != 0)
                    lootChest.LockChest();
            }
            else if (enemiesToSpawn.Count == 0)
            {
                Instantiate(roomLootChest, transform.position, Quaternion.identity, transform);
            }

        foreach (var enemy in enemiesToSpawn)
        {
            var enemyTransform = Instantiate(enemy, transform.position, Quaternion.identity);
            enemyTransform.TryGetComponent(out EnemyController enemyController);

            enemiesToDefeatForUnlockingChest.Add(enemyController);
            enemyController.OnEnemyDeath += EnemyController_OnEnemyDeath;
        }
    }

    private void EnemyController_OnEnemyDeath(object sender, EventArgs e)
    {
        var enemy = sender as EnemyController;

        enemiesToDefeatForUnlockingChest.Remove(enemy);

        TryUnlockLootChest();
    }

    private void TryUnlockLootChest()
    {
        if (enemiesToDefeatForUnlockingChest.Count != 0) return;

        if (!isChestSpawnedAfterDefeatingAllEnemies)
            lootChest.UnlockChest();
        else
            Instantiate(roomLootChest, transform.position, Quaternion.identity, transform);
    }
}

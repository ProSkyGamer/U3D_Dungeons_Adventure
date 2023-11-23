using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRoomSettings : MonoBehaviour
{
    public event EventHandler OnAllEnemiesDefeated;

    [SerializeField] private List<Transform> enemiesToSpawn = new();

    private readonly List<EnemyController> remainingEnemies = new();


    private void Start()
    {
        GameStageManager.Instance.OnGameStart += StartingDungeonRoom_OnDungeonStart;
    }

    private void StartingDungeonRoom_OnDungeonStart(object sender, EventArgs e)
    {
        foreach (var enemy in enemiesToSpawn)
        {
            var enemyTransform = Instantiate(enemy, transform.position, Quaternion.identity);
            enemyTransform.TryGetComponent(out EnemyController enemyController);

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
}

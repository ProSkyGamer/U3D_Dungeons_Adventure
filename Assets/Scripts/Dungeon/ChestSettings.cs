using System;
using UnityEngine;

public class ChestSettings : MonoBehaviour
{
    [SerializeField] private Transform roomLootChest;
    private LootChest lootChest;
    [SerializeField] private bool isChestSpawnedAfterDefeatingAllEnemies;

    private DungeonRoomSettings dungeonRoomSettings;

    private void Awake()
    {
        dungeonRoomSettings = GetComponent<DungeonRoomSettings>();
    }

    private void Start()
    {
        GameStageManager.Instance.OnGameStart += GameStageManager_OnGameStart;

        dungeonRoomSettings.OnAllEnemiesDefeated += DungeonRoomSettings_OnAllEnemiesDefeated;
    }

    private void GameStageManager_OnGameStart(object sender, EventArgs e)
    {
        if (roomLootChest != null)
            if (!isChestSpawnedAfterDefeatingAllEnemies)
            {
                var lootChestTransform = Instantiate(roomLootChest,
                    transform.position, Quaternion.identity, transform);

                lootChestTransform.TryGetComponent(out lootChest);

                lootChest.OnChestOpen += LootChest_OnChestOpen;

                if (dungeonRoomSettings.IsHasAnyEnemiesToKill())
                    lootChest.LockChest();
            }
    }

    private void LootChest_OnChestOpen(object sender, EventArgs e)
    {
        dungeonRoomSettings.SetRoomAsClear();
    }

    private void DungeonRoomSettings_OnAllEnemiesDefeated(object sender, EventArgs e)
    {
        if (!isChestSpawnedAfterDefeatingAllEnemies)
        {
            lootChest.UnlockChest();
        }
        else
        {
            var lootChestTransform = Instantiate(roomLootChest, transform.position, Quaternion.identity, transform);
            lootChestTransform.TryGetComponent(out lootChest);
            lootChest.OnChestOpen += LootChest_OnChestOpen;
        }
    }
}

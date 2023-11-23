using System;
using UnityEngine.SceneManagement;

public class FinalDungeonRoom : InteractableItem
{
    private DungeonRoomSettings dungeonRoomSettings;

    private bool isBossKilled;

    private void Awake()
    {
        dungeonRoomSettings = GetComponent<DungeonRoomSettings>();
    }

    private void Start()
    {
        dungeonRoomSettings.OnAllEnemiesDefeated += DungeonRoomSettings_OnAllEnemiesDefeated;
    }

    private void DungeonRoomSettings_OnAllEnemiesDefeated(object sender, EventArgs e)
    {
        isBossKilled = true;
    }

    public override void OnInteract(PlayerController player)
    {
        base.OnInteract(player);

        DungeonSettings.OnDungeonComplete();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        ResetStaticData.ResetData();
    }

    public override bool IsCanInteract()
    {
        return isCanInteract && isBossKilled;
    }
}

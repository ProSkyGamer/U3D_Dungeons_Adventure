using System;
using UnityEngine;

public class WaitingForOtherPlayersUI : MonoBehaviour
{
    #region Initialization & Subscribed events

    private void Awake()
    {
        SpawnPlayers.OnAllPlayersSpawned += SpawnPlayers_OnAllPlayersSpawned;
    }

    private void SpawnPlayers_OnAllPlayersSpawned(object sender, EventArgs e)
    {
        Hide();
    }

    #endregion

    #region Visual

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    #endregion
}

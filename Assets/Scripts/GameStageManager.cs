using System;
using UnityEngine;

public class GameStageManager : MonoBehaviour
{
    public static GameStageManager Instance { get; private set; }

    private enum GameStages
    {
        WaitingForStart,
        Playing,
        Pause,
        Ended
    }

    private GameStages currentGameStage = GameStages.WaitingForStart;

    private void Awake()
    {
        if (Instance != null)
            Destroy(this);
        else
            Instance = this;
    }

    private void Start()
    {
        StartingDungeonRoom.OnDungeonStart += StartingDungeonRoom_OnDungeonStart;
    }

    private void StartingDungeonRoom_OnDungeonStart(object sender, EventArgs e)
    {
        currentGameStage = GameStages.Playing;
    }

    public bool IsWaitingForStart()
    {
        return currentGameStage == GameStages.WaitingForStart;
    }

    public bool IsPlaying()
    {
        return currentGameStage == GameStages.Playing;
    }

    public bool IsPause()
    {
        return currentGameStage == GameStages.Pause;
    }

    public bool IsEnded()
    {
        return currentGameStage == GameStages.Ended;
    }
}

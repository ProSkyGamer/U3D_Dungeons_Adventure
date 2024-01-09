using System;
using UnityEngine;

public class GameStageManager : MonoBehaviour
{
    public static GameStageManager Instance { get; private set; }

    #region Events

    public event EventHandler OnGameStart;
    public event EventHandler OnGamePause;
    public event EventHandler OnGameEnd;

    #endregion

    #region Enums

    private enum GameStages
    {
        WaitingForStart,
        Playing,
        Pause,
        Ended
    }

    #endregion

    #region Variables

    private GameStages currentGameStage = GameStages.WaitingForStart;
    private GameStages gameStageBeforePause;

    private bool isPauseOnPressing = true;

    #endregion

    #region Initialization & Subscribed events

    private void Awake()
    {
        if (Instance != null)
            Destroy(this);
        else
            Instance = this;
    }

    private void Start()
    {
        ProcedureDungeonGeneration.OnDungeonGenerationFinished +=
            ProcedureDungeonGeneration_OnDungeonGenerationFinished;

        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        PauseUI.OnResumeButtonClick += GameInput_OnPauseAction;
        SettingsUI.OnSettingsClose += UIElement_OnRestorePausingByButton;

        GameInput.Instance.OnOpenCharacterInfoAction += UIElement_OnStopPausingByButton;
        ShopUI.Instance.OnShopOpen += UIElement_OnStopPausingByButton;
        PauseUI.OnSettingsButtonClick += UIElement_OnStopPausingByButton;
    }

    private void UIElement_OnRestorePausingByButton(object sender, EventArgs e)
    {
        isPauseOnPressing = true;
    }

    private void UIElement_OnStopPausingByButton(object sender, EventArgs e)
    {
        isPauseOnPressing = false;
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        if (!isPauseOnPressing)
        {
            isPauseOnPressing = true;
            return;
        }

        if (currentGameStage != GameStages.Pause)
        {
            gameStageBeforePause = currentGameStage;
            currentGameStage = GameStages.Pause;
        }
        else
        {
            currentGameStage = gameStageBeforePause;
        }

        OnGamePause?.Invoke(this, EventArgs.Empty);
    }

    private void ProcedureDungeonGeneration_OnDungeonGenerationFinished(object sender, EventArgs e)
    {
        currentGameStage = GameStages.Playing;
        OnGameStart?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Get Stages Data

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

    #endregion

    public void OnDestroy()
    {
        OnGameStart = null;
        OnGamePause = null;
        OnGameEnd = null;
    }
}

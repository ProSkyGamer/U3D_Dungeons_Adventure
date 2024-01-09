using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public static class Loader
{
    public static event EventHandler OnSingleplayerLoadingSceneOpened;

    public enum Scene
    {
        MainMenuScene,
        GameScene,
        LoadingScene,
        CharacterSelectScene
    }

    private static Scene targetScene;
    private static bool isLoadingNetwork;
    private static bool isSingleplayer;

    public static void Load(Scene newTargetScene, bool isSinglePlayer)
    {
        targetScene = newTargetScene;
        isLoadingNetwork = false;
        isSingleplayer = isSinglePlayer;

        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    public static void LoadNetwork(Scene newTargetScene)
    {
        targetScene = newTargetScene;
        isLoadingNetwork = true;

        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }

    public static void LoaderCallback()
    {
        ResetStaticData.ResetData();

        if (!isLoadingNetwork)
        {
            if (isSingleplayer)
            {
                OnSingleplayerLoadingSceneOpened?.Invoke(null, EventArgs.Empty);

                GameMultiplayer.Instance.OnSinglePlayerServerStarted += GameMultiplayer_OnSinglePlayerServerStarted;
            }
            else
            {
                SceneManager.LoadScene(targetScene.ToString());
            }
        }
        else
        {
            NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
        }

        isSingleplayer = false;
    }

    private static void GameMultiplayer_OnSinglePlayerServerStarted(object sender, EventArgs e)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);

        GameMultiplayer.Instance.OnSinglePlayerServerStarted -= GameMultiplayer_OnSinglePlayerServerStarted;
    }
}

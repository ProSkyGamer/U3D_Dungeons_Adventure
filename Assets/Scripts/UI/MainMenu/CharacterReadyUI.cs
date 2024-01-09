using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterReadyUI : NetworkBehaviour
{
    #region Vatiables & References

    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startGameButton;

    #endregion

    #region Initialization

    private void Awake()
    {
        leaveLobbyButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.LeaveLobby();
            Loader.Load(Loader.Scene.MainMenuScene, false);
        });

        readyButton.onClick.AddListener(() => { CharacterSelectReady.Instance.SetPlayerReady(); });

        startGameButton.onClick.AddListener(() => { CharacterSelectReady.Instance.StartGame(); });
    }

    private void Start()
    {
        readyButton.gameObject.SetActive(!IsServer);
        startGameButton.gameObject.SetActive(IsServer);

        if (!IsServer) return;

        CharacterSelectReady.Instance.SetPlayerReady();
    }

    #endregion
}

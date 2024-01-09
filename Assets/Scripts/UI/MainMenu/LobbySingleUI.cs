using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbySingleUI : MonoBehaviour
{
    #region Variables & References

    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyPeopleCountText;
    [SerializeField] private Button lobbyJoinButton;
    [SerializeField] private TextMeshProUGUI lobbyJoinButtonText;
    [SerializeField] private TextTranslationsSO lobbyJoinButtonTextTranslationsSo;

    private Lobby lobby;

    #endregion

    #region Lobby Methods

    public void SetLobby(Lobby lobbyToSet)
    {
        lobby = lobbyToSet;

        lobbyNameText.text = lobby.Name;
        lobbyPeopleCountText.text = $"{lobby.Players.Count} / {lobby.MaxPlayers}";

        lobbyJoinButtonText.text =
            TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                TextTranslationController.GetCurrentLanguage(), lobbyJoinButtonTextTranslationsSo);

        Debug.Log(lobbyToSet);
        Debug.Log(lobbyToSet.Id);
        lobbyJoinButton.onClick.AddListener(() => { GameLobby.Instance.JoinWithID(lobbyToSet.Id); });
    }

    #endregion
}

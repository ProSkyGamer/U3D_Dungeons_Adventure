using UnityEngine;

public class ResetMultiplayerData : MonoBehaviour
{
    private void Awake()
    {
        if (GameMultiplayer.Instance != null)
            Destroy(GameMultiplayer.Instance.gameObject);

        if (GameLobby.Instance != null)
            Destroy(GameLobby.Instance.gameObject);
    }
}

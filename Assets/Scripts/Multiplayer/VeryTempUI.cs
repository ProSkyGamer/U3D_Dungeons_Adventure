using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class VeryTempUI : MonoBehaviour
{
    #region Variables & References

    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    #endregion

    #region Initialization

    private void Awake()
    {
        startHostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            gameObject.SetActive(false);
        });
        startClientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            gameObject.SetActive(false);
        });
    }

    #endregion
}

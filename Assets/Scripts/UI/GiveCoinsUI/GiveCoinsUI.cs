using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GiveCoinsUI : MonoBehaviour
{
    #region Events

    public static event EventHandler OnInterfaceShown;
    public static event EventHandler OnInterfaceHidden;

    #endregion

    #region Variables & References

    [SerializeField] private TMP_InputField insertedCoinsAmount;
    [SerializeField] private TextMeshProUGUI currentOwnedCoinsAmountText;

    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [SerializeField] private ConfirmTransferNotificationUI notificationUI;

    private int currentTransportingCoins;
    private PlayerController playerToTransferCoins;

    private bool isFirstUpdate;

    #endregion

    #region Initialization & Subscribed events

    private void Awake()
    {
        PlayerController.OnPlayerSpawned += PlayerController_OnPlayerSpawned;

        confirmButton.onClick.AddListener(() =>
        {
            notificationUI.Show(currentTransportingCoins.ToString(), "Soon there be a name...");
            notificationUI.OnNotificationConfirmed += NotificationUI_OnNotificationConfirmed;
        });

        cancelButton.onClick.AddListener(Hide);

        insertedCoinsAmount.onValueChanged.AddListener(value =>
        {
            if (int.TryParse(value, out var parsedInt))
            {
                currentTransportingCoins = parsedInt;
                confirmButton.interactable = PlayerController.Instance.IsEnoughCoins(currentTransportingCoins);
            }
            else
            {
                confirmButton.interactable = false;
            }
        });
    }

    private void NotificationUI_OnNotificationConfirmed(object sender, EventArgs e)
    {
        PlayerController.Instance.TransferCoinsTo(playerToTransferCoins, currentTransportingCoins);
        Hide();

        notificationUI.OnNotificationConfirmed -= NotificationUI_OnNotificationConfirmed;
    }

    private void Start()
    {
        CharacterTradeCoins.OnStartCharacterTradeCoins += CharacterTradeCoins_OnStartCharacterTradeCoins;

        notificationUI.OnNotificationShown += NotificationUI_OnNotificationShown;
        notificationUI.OnNotificationHidden += NotificationUI_OnNotificationHidden;
    }

    private void PlayerController_OnPlayerSpawned(object sender, EventArgs e)
    {
        PlayerController.Instance.OnCoinsValueChange += PlayerController_OnCoinsValueChange;

        isFirstUpdate = true;
    }

    private void PlayerController_OnCoinsValueChange(object sender, EventArgs e)
    {
        currentOwnedCoinsAmountText.text = PlayerController.Instance.GetCurrentCoinsValue().ToString();
    }

    private void NotificationUI_OnNotificationHidden(object sender, EventArgs e)
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;

        notificationUI.OnNotificationConfirmed -= NotificationUI_OnNotificationConfirmed;
    }

    private void NotificationUI_OnNotificationShown(object sender, EventArgs e)
    {
        GameInput.Instance.OnPauseAction -= GameInput_OnPauseAction;
    }

    private void CharacterTradeCoins_OnStartCharacterTradeCoins(object sender,
        CharacterTradeCoins.OnStartCharacterTradeCoinsEventArgs e)
    {
        Show(e.characterToReceiveCoins);
    }

    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;

            Hide();
        }
    }

    #endregion

    #region Visual

    private void Show(PlayerController playerToTransfer)
    {
        gameObject.SetActive(true);
        playerToTransferCoins = playerToTransfer;
        OnInterfaceShown?.Invoke(this, EventArgs.Empty);

        currentOwnedCoinsAmountText.text = PlayerController.Instance.GetCurrentCoinsValue().ToString();

        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
    }

    private void Hide()
    {
        notificationUI.Hide();
        gameObject.SetActive(false);
        OnInterfaceHidden?.Invoke(this, EventArgs.Empty);

        GameInput.Instance.OnPauseAction -= GameInput_OnPauseAction;
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        Hide();
    }

    #endregion

    public static void ResetStaticData()
    {
        OnInterfaceShown = null;
        OnInterfaceShown = null;
    }
}

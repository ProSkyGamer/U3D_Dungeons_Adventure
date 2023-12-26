using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GiveCoinsUI : MonoBehaviour
{
    public static event EventHandler OnInterfaceShown;
    public static event EventHandler OnInterfaceHidden;

    [SerializeField] private TMP_InputField insertedCoinsAmount;
    [SerializeField] private TextMeshProUGUI currentInsertedCoinsAmountText;
    [SerializeField] private TextTranslationsSO incorrectValueTextTranslationsSo;

    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [SerializeField] private ConfirmTransferNotificationUI notificationUI;

    private int currentTransportingCoins;
    private PlayerController playerToTransferCoins;

    private bool isFirstUpdate = true;

    private void Awake()
    {
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
                currentInsertedCoinsAmountText.text = currentTransportingCoins.ToString();
                confirmButton.interactable = PlayerController.Instance.IsEnoughCoins(currentTransportingCoins);
            }
            else
            {
                currentInsertedCoinsAmountText.text =
                    TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                        TextTranslationController.GetCurrentLanguage(), incorrectValueTextTranslationsSo);
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

    private void Show(PlayerController playerToTransfer)
    {
        gameObject.SetActive(true);
        playerToTransferCoins = playerToTransfer;
        OnInterfaceShown?.Invoke(this, EventArgs.Empty);

        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        Hide();
    }

    private void Hide()
    {
        notificationUI.Hide();
        gameObject.SetActive(false);
        OnInterfaceHidden?.Invoke(this, EventArgs.Empty);

        GameInput.Instance.OnPauseAction -= GameInput_OnPauseAction;
    }

    private void OnDestroy()
    {
        OnInterfaceShown = null;
        OnInterfaceShown = null;
    }
}
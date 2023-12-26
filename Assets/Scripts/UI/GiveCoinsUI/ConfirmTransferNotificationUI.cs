using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmTransferNotificationUI : MonoBehaviour
{
    public event EventHandler OnNotificationShown;
    public event EventHandler OnNotificationHidden;
    public event EventHandler OnNotificationConfirmed;

    [SerializeField] private TextTranslationsSO notificationTextTranslationSo;
    [SerializeField] private TextMeshProUGUI notificationText;

    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private void Awake()
    {
        confirmButton.onClick.AddListener(() => { OnNotificationConfirmed?.Invoke(this, EventArgs.Empty); });

        cancelButton.onClick.AddListener(Hide);
    }

    public void Show(string coinsToTransfer, string namePlayerToTransfer)
    {
        OnNotificationShown?.Invoke(this, EventArgs.Empty);

        gameObject.SetActive(true);

        var currentLanguageNotificationText =
            TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                TextTranslationController.GetCurrentLanguage(), notificationTextTranslationSo);
        var currentNotificationText =
            string.Format(currentLanguageNotificationText, coinsToTransfer, namePlayerToTransfer);

        notificationText.text = currentNotificationText;
    }

    public void Hide()
    {
        OnNotificationHidden?.Invoke(this, EventArgs.Empty);

        gameObject.SetActive(false);
    }
}

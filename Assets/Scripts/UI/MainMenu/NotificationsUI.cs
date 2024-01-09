using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationsUI : MonoBehaviour
{
    public static NotificationsUI Instance { get; private set; }

    #region Variables & References

    [SerializeField] private Transform notificationsLayoutGroupTransform;
    [SerializeField] private Transform notificationTextPrefab;
    private readonly Dictionary<Transform, float> allShowedNotifications = new();

    #endregion

    #region Initialization

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        notificationTextPrefab.gameObject.SetActive(false);
    }

    #endregion

    #region Update

    private void Update()
    {
        List<Transform> notificationsToDelete = new();
        foreach (var showedNotification in allShowedNotifications)
        {
            allShowedNotifications[showedNotification.Key] -= Time.deltaTime;

            if (showedNotification.Value <= 0)
                notificationsToDelete.Add(showedNotification.Key);
        }

        foreach (var notificationToDelete in notificationsToDelete) allShowedNotifications.Remove(notificationToDelete);
    }

    #endregion

    #region Notifications

    public void AddNotification(TextTranslationsSO notificationTextTranslationSo, float notificationLifeTime = 3.5f)
    {
        if (allShowedNotifications.Count <= 0)
            Show();

        var newNotification = Instantiate(notificationTextPrefab, notificationsLayoutGroupTransform);
        var newNotificationText = newNotification.GetComponent<TextMeshProUGUI>();
        newNotificationText.text =
            TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                TextTranslationController.GetCurrentLanguage(), notificationTextTranslationSo);

        allShowedNotifications.Add(newNotification, notificationLifeTime);
    }

    #endregion

    #region Visual

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    #endregion
}
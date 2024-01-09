using System;
using UnityEngine;

public class MainMenuLanguagesTabUI : MonoBehaviour
{
    #region Initialization & Subscribed events

    private void Start()
    {
        MainMenuSettingsUI.OnLanguagesButtonClick += SettingsUI_OnLanguagesButtonClick;

        MainMenuSettingsUI.OnKeymapsButtonClick += SettingsUI_OnOtherButtonClick;
        MainMenuSettingsUI.OnMinimapButtonClick += SettingsUI_OnOtherButtonClick;
    }

    private void SettingsUI_OnOtherButtonClick(object sender, EventArgs e)
    {
        Hide();
    }

    private void SettingsUI_OnLanguagesButtonClick(object sender, EventArgs e)
    {
        Show();
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

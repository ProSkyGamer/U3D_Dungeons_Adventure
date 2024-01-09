using System;
using UnityEngine;

public class MainMenuKeymapsTabUI : MonoBehaviour
{
    #region Initialization & Subscribed events

    private void Start()
    {
        MainMenuSettingsUI.OnKeymapsButtonClick += SettingsUI_OnKeymapsButtonClick;

        MainMenuSettingsUI.OnLanguagesButtonClick += SettingsUI_OnOtherButtonClick;
        MainMenuSettingsUI.OnMinimapButtonClick += SettingsUI_OnOtherButtonClick;
    }

    private void SettingsUI_OnOtherButtonClick(object sender, EventArgs e)
    {
        Hide();
    }

    private void SettingsUI_OnKeymapsButtonClick(object sender, EventArgs e)
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

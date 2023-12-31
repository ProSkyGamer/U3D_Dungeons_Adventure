using System;
using UnityEngine;

public class LanguagesTabUI : MonoBehaviour
{
    private void Start()
    {
        SettingsUI.OnLanguagesButtonClick += SettingsUI_OnLanguagesButtonClick;

        SettingsUI.OnKeymapsButtonClick += SettingsUI_OnOtherButtonClick;
        SettingsUI.OnMinimapButtonClick += SettingsUI_OnOtherButtonClick;
    }

    private void SettingsUI_OnOtherButtonClick(object sender, EventArgs e)
    {
        Hide();
    }

    private void SettingsUI_OnLanguagesButtonClick(object sender, EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

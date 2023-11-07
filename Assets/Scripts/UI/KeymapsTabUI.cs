using System;
using UnityEngine;

public class KeymapsTabUI : MonoBehaviour
{
    private void Start()
    {
        SettingsUI.OnKeymapsButtonClick += SettingsUI_OnKeymapsButtonClick;

        SettingsUI.OnLanguagesButtonClick += SettingsUI_OnOtherButtonClick;
    }

    private void SettingsUI_OnOtherButtonClick(object sender, EventArgs e)
    {
        Hide();
    }

    private void SettingsUI_OnKeymapsButtonClick(object sender, EventArgs e)
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

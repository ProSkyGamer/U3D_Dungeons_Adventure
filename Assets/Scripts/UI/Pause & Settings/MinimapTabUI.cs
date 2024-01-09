using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinimapTabUI : MonoBehaviour
{
    #region Variables & References

    [SerializeField] private Toggle fixedMinimapToggle;
    private const string FIXED_MINIMAP_TOGGLE_PLAYER_PREFS = "FixedMinimapTogglePlayerPrefs";

    [SerializeField] private TMP_Dropdown minimapRenderingSizeDropdown;
    private const string MINIMAP_RENDERING_SIZE_PLAYER_PREFS = "FixedMinimapTogglePlayerPrefs";

    #endregion

    #region Initialization & Subscribed events

    private void Start()
    {
        var minimapRenderingSizeTypesArray = Enum.GetNames(typeof(MinimapCameraController.MinimapRenderingSize));

        minimapRenderingSizeDropdown.ClearOptions();
        List<string> minimapRenderingSizeTypesList = new();
        foreach (var minimapRenderingSizeType in minimapRenderingSizeTypesArray)
            minimapRenderingSizeTypesList.Add(minimapRenderingSizeType);
        minimapRenderingSizeDropdown.AddOptions(minimapRenderingSizeTypesList);

        SettingsUI.OnMinimapButtonClick += SettingsUI_OnMinimapButtonClick;

        SettingsUI.OnKeymapsButtonClick += SettingsUI_OnOtherButtonClick;
        SettingsUI.OnLanguagesButtonClick += SettingsUI_OnOtherButtonClick;

        var currentSavedFixedMinimapValue = PlayerPrefs.GetInt(FIXED_MINIMAP_TOGGLE_PLAYER_PREFS, 1);

        MinimapCameraController.Instance.ChangeCameraFixedMode(currentSavedFixedMinimapValue == 1);
        fixedMinimapToggle.isOn = currentSavedFixedMinimapValue == 1;

        var currentSavedMinimapRenderingSizeValue = PlayerPrefs.GetInt(MINIMAP_RENDERING_SIZE_PLAYER_PREFS, 1);

        MinimapCameraController.Instance.ChangeCameraRenderingSize(
            (MinimapCameraController.MinimapRenderingSize)currentSavedMinimapRenderingSizeValue);
        minimapRenderingSizeDropdown.value = currentSavedMinimapRenderingSizeValue;

        fixedMinimapToggle.onValueChanged.AddListener(ChangeMinimapFixedMode);
        minimapRenderingSizeDropdown.onValueChanged.AddListener(ChangeMinimapRenderingSizeMode);
    }

    private void SettingsUI_OnOtherButtonClick(object sender, EventArgs e)
    {
        Hide();
    }

    private void SettingsUI_OnMinimapButtonClick(object sender, EventArgs e)
    {
        Show();
    }

    #endregion

    #region Visual

    private void Show()
    {
        fixedMinimapToggle.isOn = MinimapCameraController.Instance.IsMinimapFixed();

        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    #endregion

    #region Minimap Change Data Methods

    private void ChangeMinimapFixedMode(bool isFixed)
    {
        MinimapCameraController.Instance.ChangeCameraFixedMode(isFixed);
        PlayerPrefs.SetInt(FIXED_MINIMAP_TOGGLE_PLAYER_PREFS, isFixed ? 1 : 0);
    }

    private void ChangeMinimapRenderingSizeMode(int sizeMode)
    {
        MinimapCameraController.Instance.ChangeCameraRenderingSize(
            (MinimapCameraController.MinimapRenderingSize)sizeMode);
        PlayerPrefs.SetInt(MINIMAP_RENDERING_SIZE_PLAYER_PREFS, sizeMode);
    }

    #endregion
}

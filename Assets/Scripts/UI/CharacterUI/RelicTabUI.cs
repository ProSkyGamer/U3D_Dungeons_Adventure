using System;
using UnityEngine;

public class RelicTabUI : MonoBehaviour
{
    #region Variables & References

    [SerializeField] private CharacterInventoryUI characterInventoryUI;
    [SerializeField] private CharacterInventoryUI characterRelicsInventoryUI;

    #endregion

    #region Inititalization & Subscribed events

    private void Start()
    {
        CharacterUI.OnRelicsTabButtonClick += CharacterUI_OnWeaponsTabButtonClick;
        CharacterUI.OnStatsTabButtonClick += CharacterUI_OnOtherTabButtonClick;
        CharacterUI.OnUpgradesTabButtonClick += CharacterUI_OnOtherTabButtonClick;
        CharacterUI.OnWeaponsTabButtonClick += CharacterUI_OnOtherTabButtonClick;
    }

    private void CharacterUI_OnOtherTabButtonClick(object sender, EventArgs e)
    {
        Hide();
    }

    private void CharacterUI_OnWeaponsTabButtonClick(object sender, EventArgs e)
    {
        Show();
    }

    #endregion

    #region Tab Visual

    private void Show()
    {
        gameObject.SetActive(true);

        UpdatePageVisual();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void UpdatePageVisual()
    {
        characterInventoryUI.UpdateInventory();
        characterRelicsInventoryUI.UpdateInventory();
    }

    #endregion
}

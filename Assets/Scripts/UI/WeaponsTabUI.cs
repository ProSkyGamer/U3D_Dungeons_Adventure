using System;
using UnityEngine;

public class WeaponsTabUI : MonoBehaviour
{
    [SerializeField] private CharacterInventoryUI characterInventoryUI;
    [SerializeField] private CharacterInventoryUI characterWeaponsInventoryUI;

    private void Start()
    {
        CharacterUI.OnWeaponsTabButtonClick += CharacterUI_OnWeaponsTabButtonClick;
        CharacterUI.OnStatsTabButtonClick += CharacterUI_OnOtherTabButtonClick;
        CharacterUI.OnUpgradesTabButtonClick += CharacterUI_OnOtherTabButtonClick;
    }

    #region SubscribedEvents

    private void CharacterUI_OnOtherTabButtonClick(object sender, EventArgs e)
    {
        Hide();
    }

    private void CharacterUI_OnWeaponsTabButtonClick(object sender, EventArgs e)
    {
        Show();
    }

    #endregion

    private void Show()
    {
        gameObject.SetActive(true);

        UpdatePageVisual();
    }

    private void UpdatePageVisual()
    {
        characterInventoryUI.UpdateInventory();
        characterWeaponsInventoryUI.UpdateInventory();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

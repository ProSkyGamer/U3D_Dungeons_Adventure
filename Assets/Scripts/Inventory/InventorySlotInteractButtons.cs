using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotInteractButtons : MonoBehaviour
{
    [SerializeField] private Button unEquipItem;
    [SerializeField] private Button equipItem;
    [SerializeField] private Button dropItem;
    [SerializeField] private Button deleteItem;

    [SerializeField] private TextMeshProUGUI unEquipItemText;
    [SerializeField] private TextTranslationsSO unEquipItemTextTranslationsSo;
    [SerializeField] private TextMeshProUGUI equipItemText;
    [SerializeField] private TextTranslationsSO equipItemTextTranslationsSo;
    [SerializeField] private TextMeshProUGUI dropItemText;
    [SerializeField] private TextTranslationsSO dropItemTextTranslationsSo;
    [SerializeField] private TextMeshProUGUI deleteItemText;
    [SerializeField] private TextTranslationsSO deleteItemTextTranslationsSo;

    public void SetSlotInfo(InventoryObject inventoryObject,
        CharacterInventoryUI.InventoryType storedInventoryType, Action onClickAction)
    {
        deleteItemText.text =
            TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                TextTranslationController.GetCurrentLanguage(), deleteItemTextTranslationsSo);
        dropItemText.text = TextTranslationController.GetTextFromTextTranslationSOByLanguage(
            TextTranslationController.GetCurrentLanguage(), dropItemTextTranslationsSo);

        deleteItem.onClick.AddListener(() =>
        {
            inventoryObject.RemoveInventoryParent();
            onClickAction();
        });

        dropItem.onClick.AddListener(() =>
        {
            inventoryObject.DropInventoryObjectToWorld(PlayerController.Instance.transform.position);
            onClickAction();
        });

        if (storedInventoryType == CharacterInventoryUI.InventoryType.PlayerWeaponInventory &&
            PlayerController.Instance.GetPlayerAttackInventory().GetCurrentInventoryObjectsCount() <= 1)
        {
            unEquipItem.interactable = false;
            equipItem.interactable = false;
            dropItem.interactable = false;
            deleteItem.interactable = false;
        }

        if (storedInventoryType == CharacterInventoryUI.InventoryType.PlayerInventory)
        {
            equipItemText.text = TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                TextTranslationController.GetCurrentLanguage(), equipItemTextTranslationsSo);

            unEquipItem.gameObject.SetActive(false);

            IInventoryParent newInventory = null;
            if (inventoryObject.TryGetWeaponSo(out var _))
                newInventory = PlayerController.Instance.GetPlayerAttackInventory();
            else if (inventoryObject.TryGetRelicSo(out var _))
                newInventory = PlayerController.Instance.GetPlayerRelicsInventory();

            if (newInventory != null)
            {
                if (newInventory.IsHasAnyAvailableSlot())
                    equipItem.onClick.AddListener(() =>
                    {
                        inventoryObject.SetInventoryParent(newInventory);
                        onClickAction();
                    });

                return;
            }

            equipItem.interactable = false;
        }
        else
        {
            unEquipItemText.text = TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                TextTranslationController.GetCurrentLanguage(), unEquipItemTextTranslationsSo);

            equipItem.gameObject.SetActive(false);

            var playerInventory = PlayerController.Instance.GetPlayerInventory();
            if (playerInventory.IsHasAnyAvailableSlot())
            {
                unEquipItem.onClick.AddListener(() =>
                {
                    inventoryObject.SetInventoryParent(playerInventory);
                    onClickAction();
                });
                return;
            }


            unEquipItem.interactable = false;
        }
    }
}

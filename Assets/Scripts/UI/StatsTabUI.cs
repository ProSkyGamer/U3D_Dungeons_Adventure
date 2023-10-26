using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsTabUI : MonoBehaviour
{
    public static event EventHandler OnStopItemDragging;

    [SerializeField] private Transform playerInventorySlotPrefab;
    [SerializeField] private Transform playerInventorySlotsGrid;

    [SerializeField] private Image currentDraggingImage;
    private InventoryObject currentDraggingObject;

    private readonly List<InventorySlotSingleUI> allInventorySlots = new();

    [SerializeField] private TextMeshProUGUI baseHpText;
    [SerializeField] private TextMeshProUGUI additionalHpText;
    [SerializeField] private TextMeshProUGUI baseAtkText;
    [SerializeField] private TextMeshProUGUI additionalAtkText;
    [SerializeField] private TextMeshProUGUI baseDefText;
    [SerializeField] private TextMeshProUGUI additionalDefText;
    [SerializeField] private TextMeshProUGUI critRateText;
    [SerializeField] private TextMeshProUGUI critDmgText;
    [SerializeField] private TextMeshProUGUI naDmgBonusText;
    [SerializeField] private TextMeshProUGUI caDmgBonusText;

    private void Start()
    {
        CharacterUI.OnStatsTabButtonClick += CharacterUI_OnStatsTabButtonClick;
        CharacterUI.OnUpgradesTabButtonClick += CharacterUI_OnOtherTabButtonClick;

        InventorySlotSingleUI.OnStartItemDragging += InventorySlotSingleUI_OnStartItemDragging;

        for (var i = 0; i < PlayerController.Instance.GetPlayerInventory().GetMaxSlotsCount(); i++)
        {
            var slotTransform = Instantiate(playerInventorySlotPrefab, playerInventorySlotsGrid);

            var slotSingleUI = slotTransform.GetComponent<InventorySlotSingleUI>();
            slotSingleUI.SetInventorySlotNumber(i);

            allInventorySlots.Add(slotSingleUI);
        }

        playerInventorySlotPrefab.gameObject.SetActive(false);
        currentDraggingImage.gameObject.SetActive(false);
    }

    #region SubscribedEvents

    private void InventorySlotSingleUI_OnStartItemDragging(object sender,
        InventorySlotSingleUI.OnStartItemDraggingEventArgs e)
    {
        currentDraggingObject = e.draggingInventoryObject;

        currentDraggingImage.gameObject.SetActive(true);
        currentDraggingImage.sprite = currentDraggingObject.GetInventoryObjectSprite();
    }

    private void CharacterUI_OnOtherTabButtonClick(object sender, EventArgs e)
    {
        Hide();
    }

    private void CharacterUI_OnStatsTabButtonClick(object sender, EventArgs e)
    {
        Show();
    }

    #endregion

    private void Update()
    {
        if (currentDraggingObject != null)
        {
            currentDraggingImage.transform.position = GameInput.Instance.GetCurrentMousePosition();

            if (GameInput.Instance.GetBindingValue(GameInput.Binding.UpgradesStartDragging) != 1f)
            {
                var selectedSlot = GetCurrentSelectedSlot();

                selectedSlot.StoreItem(currentDraggingObject);

                var newSlotNumber = selectedSlot.GetSlotNumber();
                Debug.Log($"Trying to put in slot {newSlotNumber}");

                currentDraggingObject.SetInventoryParentBySlot(PlayerController.Instance.GetPlayerInventory(),
                    newSlotNumber);

                currentDraggingImage.gameObject.SetActive(false);

                currentDraggingObject = null;

                OnStopItemDragging?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);

        UpdatePageVisual();
    }

    private void UpdatePageVisual()
    {
        UpdateStats();
        UpdateInventory();
    }

    private void UpdateStats()
    {
        baseHpText.text = PlayerController.Instance.GetBaseHp().ToString();
        additionalHpText.text = PlayerController.Instance.GetCurrentAdditionalHp().ToString();
        baseAtkText.text = PlayerController.Instance.GetBaseAttack().ToString();
        additionalAtkText.text = PlayerController.Instance.GetCurrentAdditionalAttack().ToString();
        baseDefText.text = PlayerController.Instance.GetBaseDefence().ToString();
        additionalDefText.text = PlayerController.Instance.GetCurrentAdditionalDefence().ToString();
        critRateText.text = $"{PlayerController.Instance.GetCurrentCritRate().ToString()} %";
        critDmgText.text = $"{PlayerController.Instance.GetCurrentCritDmg().ToString()} %";
        naDmgBonusText.text = $"+ {PlayerController.Instance.GetCurrentNaDmgBonus().ToString()} %";
        caDmgBonusText.text = $"+ {PlayerController.Instance.GetCurrentCaDmgBonus().ToString()} %";
    }

    private void UpdateInventory()
    {
        var playerInventory = PlayerController.Instance.GetPlayerInventory();

        for (var i = 0; i < allInventorySlots.Count; i++)
        {
            var inventoryObject = playerInventory.GetInventoryObjectBySlot(i);

            if (inventoryObject == null) continue;

            if (allInventorySlots[i].GetStoredItem() != inventoryObject)
                allInventorySlots[i].StoreItem(inventoryObject);
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private InventorySlotSingleUI GetCurrentSelectedSlot()
    {
        foreach (var inventorySlot in allInventorySlots)
            if (inventorySlot.IsCurrentSlotSelected())
                return inventorySlot;

        return null;
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReceivingItemSingleUI : MonoBehaviour
{
    #region Events

    public event EventHandler OnItemDestroyed;

    #endregion

    #region Variables & References

    [SerializeField] private Image receivingItemImage;
    [SerializeField] private Image receivingItemBackground;
    [SerializeField] private TextMeshProUGUI receivingItemName;
    [SerializeField] private TextMeshProUGUI receivingItemValue;
    private float showingReceivedItemTime = 3f;
    private float hidingReceivedItemTime = 1.5f;

    private bool isInitialized;

    #endregion

    #region Update

    private void Update()
    {
        if (!isInitialized) return;
        if (GameStageManager.Instance.IsPause()) return;

        if (showingReceivedItemTime > 0f)
        {
            showingReceivedItemTime -= Time.deltaTime;
        }
        else if (hidingReceivedItemTime > 0f)
        {
            hidingReceivedItemTime -= Time.deltaTime;

            var newReceivingItemImageColor = receivingItemImage.color;
            var newReceivingItemBackgroundColor = receivingItemBackground.color;
            var newReceivingItemNameColor = receivingItemName.color;
            var newReceivingItemValueColor = receivingItemValue.color;

            var newItemOpacity = newReceivingItemImageColor.a - Time.deltaTime * hidingReceivedItemTime;

            newReceivingItemImageColor.a = newItemOpacity;
            newReceivingItemBackgroundColor.a = newItemOpacity;
            newReceivingItemNameColor.a = newItemOpacity;
            newReceivingItemValueColor.a = newItemOpacity;

            receivingItemImage.color = newReceivingItemImageColor;
            receivingItemBackground.color = newReceivingItemBackgroundColor;
            receivingItemName.color = newReceivingItemNameColor;
            receivingItemValue.color = newReceivingItemValueColor;
        }
        else
        {
            OnItemDestroyed?.Invoke(this, EventArgs.Empty);
            Destroy(gameObject);
        }
    }

    #endregion

    #region Recieving Item

    public void SetReceivedItem(Sprite receivingItemSprite, TextTranslationsSO receivedItemNameTextTranslationSo,
        int receivingItemValue, float showingReceivedItemTime, float hidingReceivedItemTime)
    {
        receivingItemImage.sprite = receivingItemSprite;
        receivingItemName.text =
            TextTranslationController.GetTextFromTextTranslationSOByLanguage(
                TextTranslationController.GetCurrentLanguage(), receivedItemNameTextTranslationSo);
        this.receivingItemValue.text = $"x {receivingItemValue}";
        this.showingReceivedItemTime = showingReceivedItemTime;
        this.hidingReceivedItemTime = hidingReceivedItemTime;

        isInitialized = true;
    }

    #endregion
}

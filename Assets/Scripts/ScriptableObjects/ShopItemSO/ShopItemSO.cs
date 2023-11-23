using UnityEngine;

[CreateAssetMenu]
public class ShopItemSO : ScriptableObject
{
    public enum ItemType
    {
        Experience,
        Level,
        Relic,
        RelicReset
    }

    public ItemType soldItemType;
    public float boughtItemValue;
    public int coinsCost;
    public InventoryObject inventoryObjectToSold;
    public int maxBoughtCount;
    public bool isBoughtsUnlimited;
    public bool isObjectMustBeSelling;

    public TextTranslationsSO itemNameTextTranslationsSo;
    public Sprite boughtItemImage;

    public void SetShopItem(ShopItemSO shopItemSo)
    {
        soldItemType = shopItemSo.soldItemType;
        boughtItemValue = shopItemSo.boughtItemValue;
        coinsCost = shopItemSo.coinsCost;
        inventoryObjectToSold = shopItemSo.inventoryObjectToSold;
        maxBoughtCount = shopItemSo.maxBoughtCount;
        isBoughtsUnlimited = shopItemSo.isBoughtsUnlimited;
        isObjectMustBeSelling = shopItemSo.isObjectMustBeSelling;
        itemNameTextTranslationsSo = shopItemSo.itemNameTextTranslationsSo;
        boughtItemImage = shopItemSo.boughtItemImage;
    }
}

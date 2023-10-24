using UnityEngine;

[CreateAssetMenu]
public class ShopItemSO : ScriptableObject
{
    public enum ItemType
    {
        Experience,
        Level
    }

    public ItemType soldItemType;
    public int boughtItemValue;
    public int coinsCost;
    public int maxBoughtCount;
    public bool isBoughtsUnlimited;

    public string itemName;
    public Sprite boughtItemImage;
}

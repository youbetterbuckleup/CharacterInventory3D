using System;
using UnityEngine;

public enum InventoryItemType
{
    SmallPotion,
    LargePotion
}

[Serializable]
public class InventoryItemData
{
    [SerializeField] private InventoryItemType _itemType;
    [SerializeField] private string _itemName;
    [SerializeField, TextArea] private string _description;
    [SerializeField, Min(1)] private int _healAmount = 35;
    [SerializeField] private Color _color = Color.green;

    public InventoryItemType ItemType
    {
        get { return _itemType; }
    }

    public string ItemName
    {
        get { return _itemName; }
    }

    public string Description
    {
        get { return _description; }
    }

    public int HealAmount
    {
        get { return _healAmount; }
    }

    public Color Color
    {
        get { return _color; }
    }

}

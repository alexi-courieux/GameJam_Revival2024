using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponDisplay : MonoBehaviour, IInteractable, IHandleItems
{
    [SerializeField] private Transform[] itemSlots;
    [SerializeField] private FinalProductSo.ItemType[] allowedItemTypes;
    private List<FinalProduct> _items;
    private List<FinalProductSo.ItemType> _itemTypes;
    private int _capacity;

    private void Awake()
    {
        _items = new List<FinalProduct>();
        _itemTypes = new List<FinalProductSo.ItemType>();
        _capacity = itemSlots.Length;
    }
    public void Interact()
    {
        if (Player.Instance.HandleSystem.HaveAnyItems())
        {
            Item playerItem = Player.Instance.HandleSystem.GetItem();
            if (playerItem is not FinalProduct fp) return;
            if (!HasAvailableSlot(playerItem)) return;
            
            fp.SetParent(this);
        }
        else
        {
            if (_items.Count <= 0) return;
            
            Item item = _items[0];
            item.SetParent(Player.Instance.HandleSystem);
        }
    }
        public void AddItem(Item newItem)
    {
        if (newItem is not FinalProduct fp)
        {
            return;
        }
        _items.Add(fp);
        _itemTypes.Add(fp.FinalProductSo.itemType);
    }

    public Item[] GetItems<T>() where T : Item
    {
        if (typeof(T) != typeof(FinalProduct)) return null;
        
        return _items.Cast<Item>().ToArray();
    }

    public void ClearItem(Item itemToClear)
    {
        if (itemToClear is not FinalProduct fp) throw new Exception("This station can only hold final products!");
        _items.Remove(fp);
        _itemTypes.Remove(fp.FinalProductSo.itemType);
    }

    public bool HaveItems<T>() where T : Item
    {
        if (typeof(T) != typeof(FinalProduct))
        {
            Debug.LogWarning("This station can only hold final products!");
            return false;
        }
    
        return _items.Count > 0;
    }

    public bool HaveAnyItems()
    {
        return _items.Count > 0;
    }

    public Transform GetAvailableItemSlot(Item item)
    {
        if (item is not FinalProduct fp) return null;
        switch (fp.FinalProductSo.itemType)
        {
            case FinalProductSo.ItemType.Weapon:
                return itemSlots[_items.Count];
            default:
                return null;
        }
    }

    public bool HasAvailableSlot(Item item)
    {
        if (item is not FinalProduct fp) return false;
        if (allowedItemTypes.Length > 0 
            && !allowedItemTypes.Contains(fp.FinalProductSo.itemType)) return false;
        return _items.Count < _capacity;
    }

}
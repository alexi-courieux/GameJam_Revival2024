﻿using System;
using UnityEngine;

public class SingleItemChestStation : MonoBehaviour, IInteractable, IFocusable
{
    
    public event EventHandler OnFocus;
    public event EventHandler OnStopFocus;
    public event EventHandler OnProductAmountChanged;
    
    [SerializeField] private ProductSo productSo;
    [SerializeField] private int productAmount = 1;
    public void Interact()
    {
        if (!Player.Instance.HandleSystem.HaveAnyItemSelected())
        {
            // If player doesn't have any items in his hands, try to take from the chest
            if (productAmount <= 0) return;
            
            Item.SpawnItem(productSo.prefab, Player.Instance.HandleSystem);
            productAmount--;
            OnProductAmountChanged?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            // If player has items, try to put in the chest
            Item playerItem = Player.Instance.HandleSystem.GetSelectedItem();
            if (playerItem is not Product playerProduct) return;
            if (!playerProduct.ProductSo.Equals(productSo))  return;
            
            productAmount++;
            OnProductAmountChanged?.Invoke(this, EventArgs.Empty);
            playerItem.DestroySelf();
        }
    }
    
    public void Focus()
    {
        OnFocus?.Invoke(this, EventArgs.Empty);
    }
    
    public void StopFocus()
    {
        OnStopFocus?.Invoke(this, EventArgs.Empty);
    }

    public int GetProductAmount()
    {
        return productAmount;
    }
    
    public ProductSo GetProductSo()
    {
        return productSo;
    }
}
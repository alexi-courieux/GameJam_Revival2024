﻿using System;
using AshLight.BakerySim;
using UnityEngine;

public abstract class Item : MonoBehaviour, IInteractable, IInteractableAlt, IFocusable
{
    public event EventHandler OnFocus;
    public event EventHandler OnStopFocus;
    
    private IHandleItems _parent;

    public static void SpawnItem<T>(Transform itemPrefab, IHandleItems parent) where T : Item
    {
        Transform itemTransform = Instantiate(itemPrefab);
        Item item = itemTransform.GetComponent<Item>();
        item.SetParent<T>(parent);
    }

    /// <summary>
    /// Try to change the parent of this item to another one, it must have an available slot or be null
    /// </summary>
    /// <param name="targetParent">new parent</param>
    /// <returns>true if the parent have changed</returns>
    public void SetParent<T>(IHandleItems targetParent) where T : Item
    {
        if (targetParent is null)
        {
            throw new Exception("SetParent<T> must have a non-null parent, use Drop or DestroySelf instead");
        }

        if (!targetParent.HasAvailableSlot<T>())
        {
            throw new ArgumentException("The parent must have an available slot or be null");
        }

        _parent?.ClearItem(this);

        _parent = targetParent;

        if (_parent is not null)
        {
            transform.parent = _parent.GetAvailableItemSlot<T>();
            _parent.AddItem<T>(this);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }

    public void DestroySelf()
    {
        _parent.ClearItem(this);
        Destroy(gameObject);
    }

    public void Interact()
    {
        if (_parent is IInteractable interactableParent)
        {
            interactableParent.Interact();
        }
        else
        {
            SetParent<Item>(Player.Instance.HandleSystem);
        }
    }

    public void InteractAlt()
    {
        if (_parent is IInteractableAlt interactableParent)
        {
            interactableParent.InteractAlt();
        }
    }

    public void Focus()
    {
        OnFocus?.Invoke(this, EventArgs.Empty);
        if (_parent is IFocusable focusableParent)
        {
            focusableParent.Focus();
        }
    }

    public void StopFocus()
    {
        OnStopFocus?.Invoke(this, EventArgs.Empty);
        if (_parent is IFocusable focusableParent)
        {
            focusableParent.StopFocus();
        }
    }
}
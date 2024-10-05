using System;
using System.Linq;
using UnityEngine;
using Utils;

public class BlenderStation : MonoBehaviour, IInteractable, IInteractableAlt, IHandleItems
{
    public EventHandler OnPutIn;
    public EventHandler OnTakeOut;
    public EventHandler<State> OnStateChanged;

    public enum State
    {
        Idle,
        Processing
    }

    [SerializeField] private Transform itemSlot;
    [SerializeField] private RecipesDictionarySo recipesDictionarySo;
    private readonly StackList<Product> _items = new();
    private State _state;
    private State CurrentState
    {
        get => _state;
        set
        {
            _state = value;
            OnStateChanged?.Invoke(this, _state);
        }
    }
    private BlenderRecipeSo _blenderRecipeSo;
    private float _timeToProcess;

    private void Update()
    {
        switch (CurrentState)
        {
            case State.Idle:
                break;
            case State.Processing:
                _timeToProcess -= Time.deltaTime;
                if (_timeToProcess <= 0f)
                {
                    _items.ToList().ForEach(i => i.DestroySelf());
                    Item.SpawnItem<Product>(_blenderRecipeSo.output.prefab, this);
                    CheckForRecipe();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Interact()
    {
        if (CurrentState is not State.Idle) return;

        if (Player.Instance.HandleSystem.HaveAnyItems())
        {
            if (!Player.Instance.HandleSystem.HaveItems<Product>())
            {
                Debug.LogWarning("Station can only hold products!");
                return;
            }
            Player.Instance.HandleSystem.GetItem().SetParent<Product>(this);
            OnPutIn?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            if (_items.Count > 0) 
            {
                Item item = _items.Pop();
                item.SetParent<Item>(Player.Instance.HandleSystem);
                OnTakeOut?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void InteractAlt()
    {
        if (_items.Count > 0)
        {
            if (CurrentState == State.Idle)
            {
                CheckForRecipe();
            }
            else
            {
                CurrentState = State.Idle;
            }
        }
    
    }

    private void CheckForRecipe()
    {
        string[] itemsSo = GetItems<Product>()
            .Cast<Product>()
            .Select((i) => i.ProductSo.itemName)
            .OrderBy(n => n)
            .ToArray();
        BlenderRecipeSo recipe = recipesDictionarySo.blenderRecipes.FirstOrDefault(r =>
        {
            string[] recipeItemsSo = r.inputs
                .Select(i => i.itemName)
                .OrderBy(n => n)
                .ToArray();
            return itemsSo.SequenceEqual(recipeItemsSo);
        });
        if (recipe is not null)
        {
            CurrentState = State.Processing;
            _timeToProcess = recipe.timeToProcess;
            _blenderRecipeSo = recipe;
        }
        else
        {
            CurrentState = State.Idle;
        }
    }

    public void AddItem<T>(Item item) where T : Item
    {
        if (typeof(T) != typeof(Product))
        {
            Debug.LogWarning("This station can only hold products!");
            return;
        }
    
        _items.Push(item as Product);
    }

    public Item[] GetItems<T>() where T : Item
    {
        if (typeof(T) != typeof(Product))
        {
            Debug.LogWarning("This station can only hold products!");
            return null;
        }
    
        return _items.Cast<Item>().ToArray();
    }

    public void ClearItem(Item item)
    {
        _items.Remove(item as Product);
    }

    public bool HaveItems<T>() where T : Item
    {
        if (typeof(T) != typeof(Product))
        {
            Debug.LogWarning("This station can only hold products!");
            return false;
        }
    
        return _items.Count > 0;
    }

    public bool HaveAnyItems()
    {
        return _items.Count > 0;
    }

    public Transform GetAvailableItemSlot<T>() where T : Item
    {
        if (typeof(T) != typeof(Product))
        {
            Debug.LogWarning("This station can only hold products!");
            return null;
        }
    
        return itemSlot;
    }

    public bool HasAvailableSlot<T>() where T : Item
    {
        if (typeof(T) != typeof(Product))
        {
            Debug.LogWarning("This station can only hold products!");
            return false;
        }
    
        return true;
    }
}
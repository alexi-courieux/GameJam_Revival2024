using System;
using System.Linq;
using UnityEngine;

public class SmelterStation : MonoBehaviour, IInteractable, IInteractableAlt, IHandleItems, IHasProgress, ISelectableRecipe, IInteractablePrevious, IInteractableNext, IFocusable
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler<RecipeSelectedEventArgs> OnRecipeSelected;
    public event EventHandler OnFocus;
    public event EventHandler OnStopFocus;

    private enum State
    {
        Idle,
        Processing
    }

    [SerializeField] private Transform itemSlot;
    [SerializeField] private RecipesDictionarySo recipesDictionarySo;
    private SmelterRecipeSo _selectedRecipeSo;
    private SmelterRecipeSo[] _availableRecipes;
    private Product _product;
    private float _timeToProcessMax = float.MaxValue;
    private float _timeToProcess;

    private State _state;

    private void Start()
    {
        _state = State.Idle;
    }

    private void Update()
    {
        if (_state is not State.Processing) return;
        
        _timeToProcess -= Time.deltaTime;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
            progressNormalized = 1 - _timeToProcess / _timeToProcessMax
        });
        
        if (_timeToProcess <= 0)
        {
            Transform();
        }
    }

    private void Transform()
    {
        _product.DestroySelf();
        Item.SpawnItem<Product>(_selectedRecipeSo.output.prefab, this);
        _state = State.Idle;
        CheckForRecipes();
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
            progressNormalized = 0f
        });
    }


    public void Interact()
    {
        bool isPlayerHoldingProduct = Player.Instance.HandleSystem.HaveItems<Product>();
        
        if (HaveItems<Product>())
        {
            if (isPlayerHoldingProduct) return;
            _product.SetParent<Item>(Player.Instance.HandleSystem);
            _state = State.Idle;
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                progressNormalized = 0f
            });
            OnRecipeSelected?.Invoke(this, new RecipeSelectedEventArgs(null, 0));
        }
        else
        {
            if (!isPlayerHoldingProduct) return;
            Item item = Player.Instance.HandleSystem.GetItem();
            if (item is not Product product)
            {
                Debug.LogWarning("Station can only hold products!");
                return;
            }
            product.SetParent<Product>(this);
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                progressNormalized = 0f
            });
            CheckForRecipes();
        }
    }

    public void InteractAlt()
    {
        if (_product is null) return;

        if (_state is State.Idle && _selectedRecipeSo is not null)
        {
            _state = State.Processing;
            return;
        }

        if (_state is not State.Processing) return;
        
        _state = State.Idle;
    }
    private void CheckForRecipes()
    {
        _availableRecipes = recipesDictionarySo.smelterRecipeSo.Where(r => r.input == _product.ProductSo).ToArray();
        if (_availableRecipes.Length > 0)
        {
            SelectRecipe(_availableRecipes[0]);
        }
        else
        {
            ClearRecipe();
        }
    }

    private void SelectRecipe(SmelterRecipeSo recipe)
    {
        _selectedRecipeSo = recipe;
        _timeToProcessMax = recipe.timeToProcess;
        _timeToProcess = _timeToProcessMax;
        OnRecipeSelected?.Invoke(this, new RecipeSelectedEventArgs(recipe.output, 0));
    }
    
    private void ClearRecipe()
    {
        _selectedRecipeSo = null;
        OnRecipeSelected?.Invoke(this, new RecipeSelectedEventArgs(null, 0));
    }

    public void AddItem<T>(Item newItem) where T : Item
    {
        if(typeof(T) != typeof(Product))
        {
            throw new Exception("This station can only hold products!");
        }
    
        _product = newItem as Product;
    }

    public Item[] GetItems<T>() where T : Item
    {
        if (typeof(T) == typeof(Product))
        {
            return new Item[]{_product};
        }
        Debug.LogWarning($"This station doesn't have items of the specified type : {typeof(T)}");
        return new Item[]{};
    }

    public void ClearItem(Item itemToClear)
    {
        if (_product == itemToClear as Product)
        {
            _product = null;
        }
    }

    public bool HaveItems<T>() where T : Item
    {
        if (typeof(T) == typeof(Product))
        {
            return _product is not null;
        }
        Debug.LogWarning($"This station doesn't have items of the specified type : {typeof(T)}");
        return false;
    }

    public bool HaveAnyItems()
    {
        return _product is not null;
    }

    public Transform GetAvailableItemSlot<T>() where T : Item
    {
        if (typeof(T) == typeof(Product))
        {
            return itemSlot;
        }
        Debug.LogWarning($"This station doesn't have items of the specified type : {typeof(T)}");
        return null;
    }

    public bool HasAvailableSlot<T>() where T : Item
    {
        if(typeof(T) == typeof(Product))
        {
            return _product is null;
        }
        Debug.LogWarning($"This station doesn't have items of the specified type : {typeof(T)}");
        return false;
    }
    
    public void InteractPrevious()
    {
        if (_state is State.Processing) return;
        if (_selectedRecipeSo is null) return;
        
        int index = Array.IndexOf(_availableRecipes, _selectedRecipeSo);
        index--;
        if (index < 0)
        {
            index = _availableRecipes.Length - 1;
        }
        SelectRecipe(_availableRecipes[index]);
    }
    
    public void InteractNext()
    {
        if (_state is State.Processing) return;
        if (_selectedRecipeSo is null) return;
        
        int index = Array.IndexOf(_availableRecipes, _selectedRecipeSo);
        index++;
        if (index >= _availableRecipes.Length)
        {
            index = 0;
        }
        SelectRecipe(_availableRecipes[index]);
    }
    public void Focus()
    {
        OnFocus?.Invoke(this, EventArgs.Empty);
    }
    public void StopFocus()
    {
        OnStopFocus?.Invoke(this, EventArgs.Empty);
    }
}

using Godot;
using System;
using System.Collections.Generic;

public class CharacterInventory : Control
{
    private PnlInventory _invAmulet;
    private PnlInventory _invArmour;
    private PnlInventory _invWeapon;
    private PnlInventory _invPotions;
    private PnlInventory _invBag;
    private ItemBuilder _itemBuilder = new ItemBuilder();

    private UnitData _currentUnitData;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _invAmulet = GetNode<PnlInventory>("PnlInventoryAmulet");
        _invAmulet.Start(1,1, new Vector2(128,128));
        _invAmulet.InventoryItemHovered+=this.OnHover;
        _invAmulet.InventoryItemSelected+=this.OnClicked;
        _invAmulet.MouseEnteredInventory+=this.OnMouseEnteredInventory;
        _invAmulet.CurrentInventoryMode = PnlInventory.InventoryMode.AmuletInventory;

        _invArmour = GetNode<PnlInventory>("PnlInventoryArmour");
        _invArmour.Start(1,1, new Vector2(128,128));
        _invArmour.InventoryItemHovered+=this.OnHover;
        _invArmour.InventoryItemSelected+=this.OnClicked;
        _invArmour.MouseEnteredInventory+=this.OnMouseEnteredInventory;
        _invArmour.CurrentInventoryMode = PnlInventory.InventoryMode.ArmourInventory;

        _invWeapon = GetNode<PnlInventory>("PnlInventoryWeapon");
        _invWeapon.Start(1,1, new Vector2(128,128));
        _invWeapon.InventoryItemHovered+=this.OnHover;
        _invWeapon.InventoryItemSelected+=this.OnClicked;
        _invWeapon.MouseEnteredInventory+=this.OnMouseEnteredInventory;
        _invWeapon.CurrentInventoryMode = PnlInventory.InventoryMode.WeaponInventory;

        _invPotions = GetNode<PnlInventory>("PnlInventoryPotions");
        _invPotions.Start(3,1, new Vector2(128,128));
        _invPotions.InventoryItemHovered+=this.OnHover;
        _invPotions.InventoryItemSelected+=this.OnClicked;
        _invPotions.MouseEnteredInventory+=this.OnMouseEnteredInventory;
        _invPotions.CurrentInventoryMode = PnlInventory.InventoryMode.PotionInventory;

        _invBag = GetNode<PnlInventory>("ScrollContainerBag/PnlInventory");
        _invBag.InventoryItemHovered+=this.OnHover;
        _invBag.InventoryItemSelected+=this.OnClicked;
        _invBag.MouseEnteredInventory+=this.OnMouseEnteredInventory;
        _invBag.CurrentInventoryMode = PnlInventory.InventoryMode.BagInventory;
    }

    public void Start(UnitData unitData)
    {
        _currentUnitData = unitData;
        // _invBag.Start(5, Math.Max(4,Convert.ToInt32(Math.Ceiling(itemsHeld.Count/5f))), new Vector2(128,128));
        PopulateGridBag(unitData.CurrentBattleUnitData.ItemsHeld);
        PopulateAmuletSlot(unitData.CurrentBattleUnitData.AmuletEquipped);
        PopulateArmourSlot(unitData.CurrentBattleUnitData.ArmourEquipped);
        PopulateWeaponSlot(unitData.CurrentBattleUnitData.WeaponEquipped);
        PopulatePotionSlots(unitData.CurrentBattleUnitData.PotionsEquipped);
        SetGold(unitData.Gold);

    }

    private void PopulateAmuletSlot(PnlInventory.ItemMode amuletItem)
    {
        _invAmulet.ClearGrid();
        if (_invAmulet.IsAmulet(amuletItem))
        {
            _invAmulet.AddItemToNextEmpty(_itemBuilder.BuildAmulet(amuletItem));
        }
    }
    private void PopulateArmourSlot(PnlInventory.ItemMode armourItem)
    {
        _invArmour.ClearGrid();
        if (_invArmour.IsArmour(armourItem))
        {
            _invArmour.AddItemToNextEmpty(_itemBuilder.BuildArmour(armourItem));
        }
    }
    private void PopulateWeaponSlot(PnlInventory.ItemMode weaponItem)
    {
        _invWeapon.ClearGrid();
        if (_invWeapon.IsWeapon(weaponItem))
        {
            _invWeapon.AddItemToNextEmpty(_itemBuilder.BuildWeapon(weaponItem));
        }
    }

    private void PopulatePotionSlots(PnlInventory.ItemMode[] potionItems)
    {
        _invPotions.ClearGrid();
        foreach (PnlInventory.ItemMode potionItem in potionItems)
        {
            if (potionItem == PnlInventory.ItemMode.Empty)
            {
                continue;
            }
            _invPotions.AddItemToNextEmpty(_itemBuilder.BuildPotion(potionItem));
        }
    }

    private void SetGold(int num)
    {
        GetNode<Label>("TexRectGold/LblGold").Text = num.ToString();
    }

    private void PopulateGridBag(List<PnlInventory.ItemMode> itemsHeld)
    {
        _invBag.ClearGrid();
        // int halfSize = Convert.ToInt32(Math.Ceiling(potions.Count/2f));
        _invBag.Start(5, Math.Max(4,Convert.ToInt32(Math.Ceiling(itemsHeld.Count/5f))), new Vector2(128,128));
        foreach (PnlInventory.ItemMode itemMode in itemsHeld)
        {
            if (itemMode == PnlInventory.ItemMode.Empty)
            {
                _invBag.AddItemToNextEmpty(new InventoryItemEmpty());
            }
            if (_invBag.IsPotion(itemMode))
            {
                _invBag.AddItemToNextEmpty(_itemBuilder.BuildPotion(itemMode));
            }
            else if (_invBag.IsWeapon(itemMode))
            {
                _invBag.AddItemToNextEmpty(_itemBuilder.BuildWeapon(itemMode));
            }
            else if (_invBag.IsArmour(itemMode))
            {
                _invBag.AddItemToNextEmpty(_itemBuilder.BuildArmour(itemMode));
            }
            else if (_invBag.IsAmulet(itemMode))
            {
                _invBag.AddItemToNextEmpty(_itemBuilder.BuildAmulet(itemMode));
            }
        }
        // RectGlobalPosition = new Vector2(GetViewportRect().Size.x / 2f - RectSize.x/2f, GetViewportRect().Size.y / 2f - RectSize.y/2f);
    }

    public void OnHover(IInventoryPlaceable item, PnlInventory source)
    {
        // GD.Print(item.Name);
        // _targetSource = source;
        if (item is InventoryItemEmpty)
        {
            GetNode<Label>("PanelStatus/LblStatusText").Text = "";
        }
        else
        {
            GetNode<Label>("PanelStatus/LblStatusText").Text = item.Name;
        }
        
    }

    private void OnMouseEnteredInventory(PnlInventory source)
    {
        _targetSource = source;
        // GD.Print("new souce");
    }

    private IInventoryPlaceable _currentlySelectedItem;
    private PnlInventory _currentSource;
    private PnlInventory _targetSource;

    private TextureRect _currentItemTexRect;
    private int[] _initialGridPos;

    public void OnClicked(IInventoryPlaceable item, PnlInventory source)
    {
        // IInventoryPlaceable tempItem = item;
        // source.RemoveItem(item);
        _currentSource = source;
        _currentlySelectedItem = item;
        _currentItemTexRect = (TextureRect) item.TexRect.Duplicate();
        AddChild(_currentItemTexRect);
        _currentlySelectedItem.TexRect.Visible = false;
        _initialGridPos = source.GetCellGridPosition(_currentlySelectedItem.TexRect.RectPosition + _currentItemTexRect.RectSize/2);
        GD.Print(_initialGridPos[0] + ", " + _initialGridPos[1]);
        _currentItemTexRect.RectGlobalPosition = GetGlobalMousePosition() - _currentItemTexRect.RectSize/2;
        // _pickedUp = true;
        GD.Print("picking up item ", item.Name);
    }
    private bool _leftButtonPressed = false;

    private bool IsItemHeld()
    {
        if (_currentlySelectedItem != null)
        {
            if (!(_currentlySelectedItem is InventoryItemEmpty))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsValidTarget(IInventoryPlaceable heldItem)
    {
        switch (_targetSource.CurrentInventoryMode)
        {
            case PnlInventory.InventoryMode.BagInventory:
                return true;
            case PnlInventory.InventoryMode.WeaponInventory:
                if (heldItem is WeaponItem)
                {
                    return true;
                }
                break;
            case PnlInventory.InventoryMode.PotionInventory:
                if (heldItem is PotionEffect)
                {
                    return true;
                }
                break;
            case PnlInventory.InventoryMode.ArmourInventory:
                if (heldItem is ArmourItem)
                {
                    return true;
                }
                break;
            case PnlInventory.InventoryMode.AmuletInventory:
                if (heldItem is AmuletItem)
                {
                    return true;
                }
                break;
            default:
                return false;
        }
        return false;
    }

    private void DropItem()
    {
        if (!IsItemHeld())
        {
            return;
        }

        GD.Print(_targetSource.Name);

        if (_targetSource.WorldPositionIsOutOfBounds(_targetSource.GetLocalMousePosition()) || !IsValidTarget(_currentlySelectedItem))
        {
            ResetSelection();
            return;
        }

        IInventoryPlaceable oldItem = _targetSource.GetItemAtWorldPosition(_targetSource.GetLocalMousePosition());
        
        if (oldItem is InventoryItemEmpty)
        {
            _currentSource.RemoveItem(_currentlySelectedItem);
            _targetSource.AddItemAtWorldPosition(_targetSource.GetLocalMousePosition(), _currentlySelectedItem);
            UpdateUnitData();
            ResetSelection();
            return;            
        }
        _targetSource.RemoveItem(oldItem);
        _currentSource.RemoveItem(_currentlySelectedItem);
        _targetSource.AddItemAtWorldPosition(_targetSource.GetLocalMousePosition(), _currentlySelectedItem);
        _currentSource.AddItemAtGridPosition(_initialGridPos[0], _initialGridPos[1], oldItem);
        UpdateUnitData();
        ResetSelection();
    }

    private void UpdateUnitData()
    {
        _currentUnitData.CurrentBattleUnitData.AmuletEquipped = _invAmulet.GetItemAtGridPosition(0,0).CurrentItemMode;
        _currentUnitData.CurrentBattleUnitData.ArmourEquipped = _invArmour.GetItemAtGridPosition(0,0).CurrentItemMode;
        _currentUnitData.CurrentBattleUnitData.WeaponEquipped = _invWeapon.GetItemAtGridPosition(0,0).CurrentItemMode;
        _currentUnitData.CurrentBattleUnitData.PotionsEquipped = new PnlInventory.ItemMode[3] { // in potion slots
            _invPotions.GetItemAtGridPosition(0,0).CurrentItemMode,
            _invPotions.GetItemAtGridPosition(1,0).CurrentItemMode,
            _invPotions.GetItemAtGridPosition(2,0).CurrentItemMode,
        };
        _currentUnitData.CurrentBattleUnitData.ItemsHeld.Clear();
        foreach (IInventoryPlaceable item in _invBag.GetAllItems(includeEmpty:true))
        {
            // if (item.CurrentItemMode != PnlInventory.ItemMode.Empty)
            // {
                _currentUnitData.CurrentBattleUnitData.ItemsHeld.Add(item.CurrentItemMode);
                GD.Print(Enum.GetName(typeof(PnlInventory.ItemMode),item.CurrentItemMode));
            // }
        }
        // List<PnlInventory.ItemMode> newHeld = new List<PnlInventory.ItemMode>();
        // for (int i =)
        PopulateGridBag(_currentUnitData.CurrentBattleUnitData.ItemsHeld);
        // foreach (PnlInventory.ItemMode pot in _currentUnitData.CurrentBattleUnitData.PotionsEquipped)
        // {
        //     GD.Print(Enum.GetName(typeof(PnlInventory.ItemMode),pot));
        // }
        // GD.Print(Enum.GetName(typeof(PnlInventory.ItemMode),_currentUnitData.CurrentBattleUnitData.AmuletEquipped));
    }

    private void ResetSelection()
    {
        _currentlySelectedItem.TexRect.Visible = true;
        _currentlySelectedItem = null;
        _currentSource = null;
        _currentItemTexRect.QueueFree();
    }
    

    public override void _Input(InputEvent ev)
    {
       
        if (ev is InputEventMouseButton btn)
        {
            if (btn.ButtonIndex == (int) ButtonList.Left)
            {
                _leftButtonPressed = btn.Pressed;
            }
            if (!btn.Pressed)
            {
                DropItem();
            }
        }        
        if (ev is InputEventMouseMotion)
        {
            if (_currentlySelectedItem != null)
            {
                if (!(_currentlySelectedItem is InventoryItemEmpty))
                {
                    if (_leftButtonPressed)
                    {
                        _currentItemTexRect.RectGlobalPosition = GetGlobalMousePosition() - _currentItemTexRect.RectSize/2;
                    }
                }
            }
        }
    }

    public void Die()
    {
        _invAmulet.Die();
        _invArmour.Die();
        _invBag.Die();
        _invWeapon.Die();
    }
}

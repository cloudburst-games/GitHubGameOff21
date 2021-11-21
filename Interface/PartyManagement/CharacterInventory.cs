using Godot;
using System;
using System.Collections.Generic;

public class CharacterInventory : Control
{
    [Signal]
    public delegate void GivingItemToAnotherCharacter(string id, PnlInventory.ItemMode item);

    [Signal]
    public delegate void CharacterStatsChanged(Dictionary<BattleUnitData.DerivedStat, float> stats);
    
    [Signal]
    public delegate void CharacterAttributesChanged(Dictionary<UnitData.Attribute, int> atts, float armour);

    private PnlInventory _invAmulet;
    private PnlInventory _invArmour;
    private PnlInventory _invWeapon;
    private PnlInventory _invPotions;
    private PnlInventory _invBag;
    private ItemBuilder _itemBuilder = new ItemBuilder();
    private IInventoryPlaceable _currentlySelectedItem;
    private PnlInventory _currentSource;
    private PnlInventory _targetSource;
    private bool _leftButtonPressed = false;
    private TextureRect _currentItemTexRect;
    private int[] _initialGridPos;
    private UnitData _currentUnitData;
    private Dictionary<string, bool> _insidePortraitIDs = new Dictionary<string, bool>();

    
    public override void _Ready()
    {
        // TODO use a loop for below, something like:
        // PnlInventory[] inventories = new PnlInventory[5] {_invAmulet, _invArmour, _invWeapon,_invPotions, _invBag};
        // foreach (PnlInventory inv in inventories)
        // {
        //     inv.InventoryItemHovered+=this.OnHover;
        //     inv.InventoryItemSelected+=this.OnClicked;
        //     inv.MouseEnteredInventory+=this.OnMouseEnteredInventory;
        // }

        // on the other hand if it aint broke dont fix it
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
        _insidePortraitIDs.Clear();
        // _invBag.Start(5, Math.Max(4,Convert.ToInt32(Math.Ceiling(itemsHeld.Count/5f))), new Vector2(128,128));
        PopulateGridBag(unitData.CurrentBattleUnitData.ItemsHeld);
        PopulateAmuletSlot(unitData.CurrentBattleUnitData.AmuletEquipped);
        PopulateArmourSlot(unitData.CurrentBattleUnitData.ArmourEquipped);
        PopulateWeaponSlot(unitData.CurrentBattleUnitData.WeaponEquipped);
        PopulatePotionSlots(unitData.CurrentBattleUnitData.PotionsEquipped);

    }

    private void PopulateAmuletSlot(PnlInventory.ItemMode amuletItem)
    {
        _invAmulet.ClearGrid();
        if (_itemBuilder.IsAmulet(amuletItem))
        {
            _invAmulet.AddItemToNextEmpty(_itemBuilder.BuildAmulet(amuletItem));
        }
    }
    private void PopulateArmourSlot(PnlInventory.ItemMode armourItem)
    {
        _invArmour.ClearGrid();
        if (_itemBuilder.IsArmour(armourItem))
        {
            _invArmour.AddItemToNextEmpty(_itemBuilder.BuildArmour(armourItem));
        }
    }
    private void PopulateWeaponSlot(PnlInventory.ItemMode weaponItem)
    {
        _invWeapon.ClearGrid();
        if (_itemBuilder.IsWeapon(weaponItem))
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

    // make a 5 by 4 to however big we need, bag
    // it would have been more interesting if there was a weight limit or max inventory limit but no time
    // might as well have made a single inventory for the party but oh well
    private void PopulateGridBag(List<PnlInventory.ItemMode> itemsHeld)
    {
        _invBag.ClearGrid();
        _invBag.Start(5, 1+Math.Max(4,Convert.ToInt32(Math.Ceiling(itemsHeld.Count/5f))), new Vector2(128,128));
        foreach (PnlInventory.ItemMode itemMode in itemsHeld)
        {
            if (itemMode == PnlInventory.ItemMode.Empty)
            {
                continue;
            }
            if (_itemBuilder.IsPotion(itemMode))
            {
                _invBag.AddItemToNextEmptyRowsFirst(_itemBuilder.BuildPotion(itemMode));
            }
            else if (_itemBuilder.IsWeapon(itemMode))
            {
                _invBag.AddItemToNextEmptyRowsFirst(_itemBuilder.BuildWeapon(itemMode));
            }
            else if (_itemBuilder.IsArmour(itemMode))
            {
                _invBag.AddItemToNextEmptyRowsFirst(_itemBuilder.BuildArmour(itemMode));
            }
            else if (_itemBuilder.IsAmulet(itemMode))
            {
                _invBag.AddItemToNextEmptyRowsFirst(_itemBuilder.BuildAmulet(itemMode));
            }
        }
    }

    public void OnHover(IInventoryPlaceable item, PnlInventory source)
    {
        if (item is InventoryItemEmpty)
        {
            GetNode<Label>("PanelStatus/LblStatusText").Text = "";
        }
        else
        {
            GetNode<Label>("PanelStatus/LblStatusText").Text = item.Name + ": " + item.Tooltip;
        }
        
    }

    private void OnMouseEnteredInventory(PnlInventory source)
    {
        _targetSource = source;
    }

    public void OnClicked(IInventoryPlaceable item, PnlInventory source)
    {
        _currentSource = source;
        _currentlySelectedItem = item;
        _currentItemTexRect = (TextureRect) item.TexRect.Duplicate();
        AddChild(_currentItemTexRect);
        _currentlySelectedItem.TexRect.Visible = false;
        _initialGridPos = source.GetCellGridPosition(_currentlySelectedItem.TexRect.RectPosition + _currentItemTexRect.RectSize/2);
        _currentItemTexRect.RectGlobalPosition = GetGlobalMousePosition() - _currentItemTexRect.RectSize/2;
    }

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
        // dont drop anything if nothing held
        if (!IsItemHeld())
        {
            return;
        }

        // check if over portraits, if so, give the item to the portrait owner (if it isnt the character holding the item)
        foreach (KeyValuePair<string,bool> kv in _insidePortraitIDs)
        {
            if (kv.Value)
            {
                if (kv.Key == _currentUnitData.ID)
                {
                    ResetSelection();
                    return;
                }
                EmitSignal(nameof(GivingItemToAnotherCharacter), kv.Key, _currentlySelectedItem.CurrentItemMode);
                _currentSource.RemoveItem(_currentlySelectedItem);
                UpdateUnitData();
                ResetSelection();
                return;
            }
        }

        // otherwise, check where we are. if we are not over the correct inventory do nothing
        if (!IsValidTarget(_currentlySelectedItem))
        {
            ResetSelection();
            return;
        }

        // if we were over the right inventory, but are now out of bounds, do nothing
        if (_targetSource.WorldPositionIsOutOfBounds(_targetSource.GetLocalMousePosition()))
        {
            ResetSelection();
            return;
        }

        // store a local reference to the item or slot that we are about to release the held item
        IInventoryPlaceable oldItem = _targetSource.GetItemAtWorldPosition(_targetSource.GetLocalMousePosition());
        
        // if the slot is empty, just deposit the held item
        if (oldItem is InventoryItemEmpty)
        {
            _currentSource.RemoveItem(_currentlySelectedItem);
            _targetSource.AddItemAtWorldPosition(_targetSource.GetLocalMousePosition(), _currentlySelectedItem);
            UpdateUnitData();
            ResetSelection();
            return;            
        }

        // otherwise, swap out the old item
        _targetSource.RemoveItem(oldItem);
        _currentSource.RemoveItem(_currentlySelectedItem);
        _targetSource.AddItemAtWorldPosition(_targetSource.GetLocalMousePosition(), _currentlySelectedItem);
        // if the place we swapped out from is valid, put the old item there
        _targetSource = _currentSource;
        if (IsValidTarget(oldItem))
        {
            _targetSource.AddItemAtGridPosition(_initialGridPos[0], _initialGridPos[1], oldItem);
        }
        // otherwise, place it in our bag
        else
        {
            _invBag.AddItemToNextEmpty(oldItem);
        }
        // update the unitdata with our inventory changes
        UpdateUnitData();
        ResetSelection();
    }

    private void UpdateUnitData()
    {
        // _currentUnitData.CurrentBattleUnitData.AmuletEquipped = PnlInventory.ItemMode.Empty;
        // _currentUnitData.CurrentBattleUnitData.ArmourEquipped = PnlInventory.ItemMode.Empty;
        // _currentUnitData.CurrentBattleUnitData.WeaponEquipped = PnlInventory.ItemMode.Empty;
        _currentUnitData.RemoveEquippedAmulet();
        _currentUnitData.RemoveEquippedArmour();
        _currentUnitData.RemoveEquippedWeapon();

        _currentUnitData.EquipAmulet(_invAmulet.GetItemAtGridPosition(0,0).CurrentItemMode);
        _currentUnitData.EquipArmour(_invArmour.GetItemAtGridPosition(0,0).CurrentItemMode);
        _currentUnitData.EquipWeapon(_invWeapon.GetItemAtGridPosition(0,0).CurrentItemMode);

        _currentUnitData.CurrentBattleUnitData.PotionsEquipped = new PnlInventory.ItemMode[3] { // in potion slots
            _invPotions.GetItemAtGridPosition(0,0).CurrentItemMode,
            _invPotions.GetItemAtGridPosition(1,0).CurrentItemMode,
            _invPotions.GetItemAtGridPosition(2,0).CurrentItemMode,
        };
        // we currently dont store grid positions
        // if i were to do this again, i would make a InventoryGrid object so that items can be stored by position
        // this would have to be able to dynamically grow, so ability to add rows as the object grows near capacity
        _currentUnitData.CurrentBattleUnitData.ItemsHeld.Clear();
        foreach (IInventoryPlaceable item in _invBag.GetAllItems(includeEmpty:false))
        {
            _currentUnitData.CurrentBattleUnitData.ItemsHeld.Add(item.CurrentItemMode);
        }
        _currentUnitData.UpdateDerivedStatsFromAttributes();
        EmitSignal(nameof(CharacterStatsChanged), _currentUnitData.CurrentBattleUnitData.Stats);
        EmitSignal(nameof(CharacterAttributesChanged), _currentUnitData.Attributes, _currentUnitData.GetArmourBonus());
    }

    // set all to defaults
    private void ResetSelection()
    {
        _currentlySelectedItem.TexRect.Visible = true;
        _currentlySelectedItem = null;
        _currentSource = null;
        _currentItemTexRect.QueueFree();

    }
    
    public void OnInsideButtonOfCharacter(string ID, bool inside)
    {
        _insidePortraitIDs[ID] = inside;
        foreach (KeyValuePair<string,bool> kv in _insidePortraitIDs)
        {
            GD.Print("inside: ", kv.Key, ": ", kv.Value);
        }
        
    }

    public override void _Input(InputEvent ev)
    {
        // if we are holding down the mouse button, store it so that when the mouse moves....(1)
        if (ev is InputEventMouseButton btn)
        {
            if (btn.ButtonIndex == (int) ButtonList.Left)
            {
                _leftButtonPressed = btn.Pressed;
            }
            // if we release the mouse button, try to drop the item (of course we check if we arent holding anything!)
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
                    // ... (1) the item sprite moves with the cursor
                    if (_leftButtonPressed)
                    {
                        _currentItemTexRect.RectGlobalPosition = GetGlobalMousePosition() - _currentItemTexRect.RectSize/2;
                    }
                }
            }
        }
    }

    // we use c# events here so kill everything upon exit
    public void Die()
    {
        _invAmulet.Die();
        _invArmour.Die();
        _invBag.Die();
        _invWeapon.Die();
    }
}

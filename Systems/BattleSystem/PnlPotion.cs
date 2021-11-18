using Godot;
using System;
using System.Collections.Generic;

public class PnlPotion : PnlInventory
{
    // [Signal]
    public delegate void PotionSelectedDelegate(PnlInventory.ItemMode potionMode);
    public event PotionSelectedDelegate PotionSelected;
    private const int _EXPANDMARGIN = 50;
    public override void _Ready()
    {
        base._Ready();
        Visible = false;

        if (!IsConnected("mouse_entered", this, nameof(PnlInventory.OnPnlInventoryMouseEntered)))
        {
            Connect("mouse_entered", this, nameof(PnlInventory.OnPnlInventoryMouseEntered));
        }
        if (!IsConnected("mouse_exited", this, nameof(PnlInventory.OnPnlInventoryMouseExited)))
        {
            Connect("mouse_exited", this, nameof(PnlInventory.OnPnlInventoryMouseExited));
        }

        // Connect(nameof(InventoryItemHovered), this, nameof(OnInventoryItemHover));
        InventoryItemHovered+=this.OnInventoryItemHover;
        InventoryItemSelected+=this.OnInventoryItemSelected;
    }

    public bool CursorInsidePanel()
    {
        return GetGlobalMousePosition().x > RectGlobalPosition.x - _EXPANDMARGIN && GetGlobalMousePosition().x < RectGlobalPosition.x + RectSize.x + _EXPANDMARGIN
            && GetGlobalMousePosition().y > RectGlobalPosition.y - _EXPANDMARGIN && GetGlobalMousePosition().y < RectGlobalPosition.y + RectSize.y + _EXPANDMARGIN;
    }

    public void PopulateGrid(List<PnlInventory.ItemMode> potions)
    {
        ClearGrid();
        // int halfSize = Convert.ToInt32(Math.Ceiling(potions.Count/2f));
        Start(3, 1, new Vector2(128,128));
        foreach (PnlInventory.ItemMode potionMode in potions)
        {
            if (potionMode == PnlInventory.ItemMode.Empty)
            {
                continue;
            }
            PotionEffect newPotion = PotionBuilder.BuildPotion(potionMode);
            AddItemToNextEmpty(newPotion);
        }
        RectGlobalPosition = new Vector2(GetViewportRect().Size.x / 2f - RectSize.x/2f, GetViewportRect().Size.y / 2f - RectSize.y/2f);

    }

    public void OnInventoryItemHover(IInventoryPlaceable item, PnlInventory source)
    {
        GD.Print("trying to hover item");
        if (!Visible)
        {
            return;
        }
        if (item is InventoryItemEmpty)
        {
            GetNode<Label>("LblHover").Text = "";
        }
        else
        {
            GetNode<Label>("LblHover").Text = item.Name;
        }
    }

    public void OnInventoryItemSelected(IInventoryPlaceable item, PnlInventory source)
    {
        if (!Visible)
        {
            return;
        }
        if (!(item is PotionEffect pot))
        {
            GD.Print("something went wrong OnInventoryItemSelected PnlPotion.cs");
        }
        RemoveItem(item);
        // EmitSignal(nameof(PotionSelected), item);
        PotionSelected?.Invoke(((PotionEffect)item).CurrentItemMode);
        GetNode<Label>("LblHover").Text = "";
    }

    public override void Die()
    {
        base.Die();
        PotionSelected = null;
    }

    // public void OnTestPotionPressed()
    // {
    //     PotionEffect pot = new PotionEffect() {
    //         StatsAffected = new System.Collections.Generic.List<BattleUnitData.DerivedStat>() {
    //             BattleUnitData.DerivedStat.Health
    //             },
    //         Magnitude = 5f
    //     };
    //     EmitSignal(nameof(PotionSelected), pot);
    // }

    // public override void Die()
    // {
    //     base.Die();
    // }
}

using Godot;
using System.Collections.Generic;
using System;
using System.Linq;

public class Shop : StaticBody2D
{
    [Export]
    private Godot.Collections.Array<PnlInventory.ItemMode> _startingItemsStocked = new Godot.Collections.Array<PnlInventory.ItemMode>();

    [Export]
    private string _introText = "Welcome, my lord! Please, peruse my humble wares.";
    [Export]
    private string _shopTitle = "Shop";
    [Export]
    private string _buyMessage = "Hmmph. Trash. I'll take it of your hands for {0} gold.";
    [Export]
    private string _sellMessage = "A bargain for {0} gold!";
    [Export]
    private float _stinginess = 2f;
    [Export]
    private Texture _shopBackgroundTexture = GD.Load<Texture>("res://Effects/SpellEffects/Art/WhiteSphericalParticle.png");

    public ShopData CurrentShopData {get; set;} = new ShopData();
    public override void _Ready()
    {
        // probably better to emit a signal
        GetNode<ShopInteractableArea>("InteractableArea").CurrentShop = this;
        SetStartingData();
    }

    public void SetStartingData()
    {
        CurrentShopData.ItemsStocked = _startingItemsStocked.ToList();
        CurrentShopData.ShopBackgroundTexturePath = _shopBackgroundTexture.ResourcePath;
        CurrentShopData.IntroText = _introText;
        CurrentShopData.ShopTitle = _shopTitle;
        CurrentShopData.Stinginess = _stinginess;
        CurrentShopData.GlobalStartPosition = this.GlobalPosition;
        CurrentShopData.SellMessage = _sellMessage;
        CurrentShopData.BuyMessage = _buyMessage;
    }

    // MUST BE RUN AFTER ADDED CHILD to overwrite starting data
    public void LoadStartingData(ShopData shopData)
    {
        CurrentShopData = shopData;
        GlobalPosition = CurrentShopData.GlobalStartPosition;
    }

    // public void Start(UnitData buyerUnitData)
    // {
    //     GD.Print("test shop start");
    //     GD.Print(CurrentShopData.IntroText);
    //     GD.Print("Items: ");
    //     // ItemBuilder itemBuilder = new ItemBuilder();
    //     // PnlInventory inventory = new PnlInventory();
    //     foreach (PnlInventory.ItemMode item in CurrentShopData.ItemsStocked)
    //     {
    //         // if (inventory.IsPotion(item))
    //         // GD.Print(itemBuilder.BuildPotion(item).Name);
    //         // if (inventory.IsWeapon(item))
    //         // GD.Print(itemBuilder.BuildWeapon(item).Name);
    //         // if (inventory.IsArmour(item))
    //         // GD.Print(itemBuilder.BuildArmour(item).Name);
    //         // GD.Print("item anpother");
    //         GD.Print(Enum.GetName(typeof(PnlInventory.ItemMode), item));
    //     }
        
    // }
    // private void OnInteractableAreaBodyEntered(Godot.Object body)
    // {
    //     if (body is Unit unit)
    //     {
    //         if (unit.CurrentUnitData.Player)
    //         {
    //             GetNode<Panel>("PnlInfo").Visible = true;
    //         }
    //     }
    // }
    // private void OnInteractableAreaBodyExited(Godot.Object body)
    // {
    //     if (body is Unit unit)
    //     {
    //         if (unit.CurrentUnitData.Player)
    //         {
    //             GetNode<Panel>("PnlInfo").Visible = false;
    //         }
    //     }
    // }    
    private void OnInteractableAreaAreaEntered(Godot.Object area)
    {
        if (area is Area2D a)
        {
            if (a.Name == "NPCEnableInteractionArea")
            {
                if (a.GetParent() is Unit unit)
                {
                    if (unit.CurrentControlState is PlayerUnitControlState)
                    {
                        // GetNode<Panel>("PnlInfo").Visible = true;
                        SetHighlight(true);
                    }
                }
            }
        }
    }
    private void OnInteractableAreaAreaExited(Godot.Object area)
    {
        if (area is Area2D a)
        {
            if (a.Name == "NPCEnableInteractionArea")
            {
                if (a.GetParent() is Unit unit)
                {
                    if (unit.CurrentControlState is PlayerUnitControlState)
                    {
                        // GetNode<Panel>("PnlInfo").Visible = false;
                        GetNode<Sprite>("Sprite").Material = null;
                    }
                }
            }
        }
    }


    public void SetHighlight(bool enable)
    {
        if (enable)
        {
            if (GetNode<Sprite>("Sprite").Material == null)
            {
                ShaderMaterial shaderMaterial = (ShaderMaterial) GD.Load<ShaderMaterial>("res://Shaders/Flash/FlashShader.tres").Duplicate();
                shaderMaterial.SetShaderParam("speed", 12f);
                shaderMaterial.SetShaderParam("flash_colour_original", new Color(1f,1f,1f));
                shaderMaterial.SetShaderParam("flash_depth", 0.4f);
                GetNode<Sprite>("Sprite").Material = shaderMaterial;
            }
        }
        else
        {
            // make sure the player is not overlapping first
            if (GetNode<Area2D>("InteractableArea").GetOverlappingAreas().Count > 0)
            {
                foreach (Godot.Object area in GetNode<Area2D>("InteractableArea").GetOverlappingAreas())
                {
                    if (area is Area2D a)
                    {
                        if (a.Name == "NPCEnableInteractionArea")
                        {
                            if (a.GetParent() is Unit unit)
                            {
                                if (unit.CurrentControlState is PlayerUnitControlState)
                                {
                                    return;
                                    // GetNode<Panel>("PnlInfo").Visible = true;
                                }
                            }
                        }
                    }
                }
            }
            GetNode<Sprite>("Sprite").Material = null;
        }
    }
}

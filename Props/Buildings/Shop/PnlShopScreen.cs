// TODO
// integrating into game

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class PnlShopScreen : Panel
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Visible = false;
        GetNode<HBoxPortraits>("HBoxPortraits").InCharacterManager = true;
        GetNode<HBoxPortraits>("HBoxPortraits").Connect(nameof(HBoxPortraits.PortraitPressed), this, nameof(OnHBoxPortraitsPortraitPressed));
        GetNode<PnlInventory>("TabContainer/Buy/PnlInventoryBuy").InventoryItemHovered+=this.OnItemHovered;
        GetNode<PnlInventory>("TabContainer/Sell/PnlInventorySell").InventoryItemHovered+=this.OnItemHovered;
        GetNode<PnlInventory>("TabContainer/Buy/PnlInventoryBuy").InventoryItemSelected+=this.OnItemSelected;
        GetNode<PnlInventory>("TabContainer/Sell/PnlInventorySell").InventoryItemSelected+=this.OnItemSelected;
        //TESTING. please keep this commented when doing release version

        if (GetParent() == GetTree().Root && ProjectSettings.GetSetting("application/run/main_scene") != Filename)
        {
            Test();
        }
    }
    public void Test()
    {
        Unit player = new Unit();
        // STARTING PLAYER DATA HERE
        player.CurrentUnitData = new UnitData() {
            Player = true,
            Name = "Khepri",
            ID = "khepri",
            BasePhysicalDamageRange = 1f, // ideally this would change depending on weapon equipped
            Modified = true,
            PortraitPath = "res://Systems/BattleSystem/GridAttackAPPoint.png",
            PortraitPathSmall = "res://Systems/BattleSystem/GridAttackAPPoint.png"
        };
        
        // set starting attributes
        foreach (UnitData.Attribute att in player.CurrentUnitData.Attributes.Keys.ToList())
        {
            player.CurrentUnitData.Attributes[att] = 10;
        }
        player.CurrentUnitData.UpdateDerivedStatsFromAttributes();
        // set starting spells
        player.CurrentUnitData.CurrentBattleUnitData.Spell1 = SpellEffectManager.SpellMode.SolarBolt;
        player.CurrentUnitData.CurrentBattleUnitData.Spell2 = SpellEffectManager.SpellMode.Empty;
        player.CurrentUnitData.CurrentBattleUnitData.SpellGainedAtHigherLevel = SpellEffectManager.SpellMode.SolarBlast;
        player.CurrentUnitData.Gold = 41;
        // set starting equipment
        player.CurrentUnitData.EquipAmulet(PnlInventory.ItemMode.Empty);
        player.CurrentUnitData.EquipArmour(PnlInventory.ItemMode.Empty);
        player.CurrentUnitData.EquipWeapon(PnlInventory.ItemMode.Empty);
        player.CurrentUnitData.CurrentBattleUnitData.PotionsEquipped = new PnlInventory.ItemMode[3] {
            PnlInventory.ItemMode.CharismaPot, PnlInventory.ItemMode.Empty, PnlInventory.ItemMode.ManaPot
        };
        player.CurrentUnitData.CurrentBattleUnitData.ItemsHeld = new List<PnlInventory.ItemMode>() {
            PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.ManaPot,
            PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.CharismaPot,
            PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.LuckPot,
            // PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.ResiliencePot,
            PnlInventory.ItemMode.VigourPot, PnlInventory.ItemMode.ObsidianPlate,
            PnlInventory.ItemMode.ManaPot, PnlInventory.ItemMode.RustedArmour,
            PnlInventory.ItemMode.ScarabAmulet, PnlInventory.ItemMode.RustedMace,
            PnlInventory.ItemMode.VigourPot, PnlInventory.ItemMode.SilverMace,
            // PnlInventory.ItemMode.IntellectPot, PnlInventory.ItemMode.ManaPot,
            PnlInventory.ItemMode.LuckPot, PnlInventory.ItemMode.ManaPot,
            PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.ManaPot,
            PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.CharismaPot,
            PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.LuckPot,
            PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.ResiliencePot,
            PnlInventory.ItemMode.VigourPot, PnlInventory.ItemMode.ObsidianPlate,
            PnlInventory.ItemMode.ManaPot, PnlInventory.ItemMode.RustedArmour,
            PnlInventory.ItemMode.ScarabAmulet, PnlInventory.ItemMode.RustedMace,
            PnlInventory.ItemMode.VigourPot, PnlInventory.ItemMode.SilverMace,
            PnlInventory.ItemMode.IntellectPot, PnlInventory.ItemMode.ManaPot,
            PnlInventory.ItemMode.LuckPot, PnlInventory.ItemMode.ManaPot,
        };
               Unit npc1 = new Unit();
        // STARTING PLAYER DATA HERE
        npc1.CurrentUnitData = new UnitData() {
            Player = true,
            Name = "no 1",
            ID = "npc1",
            BasePhysicalDamageRange = 1f, // ideally this would change depending on weapon equipped
            Modified = true,
            PortraitPath = "res://Interface/Icons/Food.png",
            PortraitPathSmall = "res://Interface/Icons/Food.png"
        };
        
        // set starting attributes
        foreach (UnitData.Attribute att in npc1.CurrentUnitData.Attributes.Keys.ToList())
        {
            npc1.CurrentUnitData.Attributes[att] = 10;
        }
        npc1.CurrentUnitData.UpdateDerivedStatsFromAttributes();
        // set starting spells
        npc1.CurrentUnitData.CurrentBattleUnitData.Spell1 = SpellEffectManager.SpellMode.SolarBolt;
        npc1.CurrentUnitData.CurrentBattleUnitData.Spell2 = SpellEffectManager.SpellMode.Empty;
        npc1.CurrentUnitData.CurrentBattleUnitData.SpellGainedAtHigherLevel = SpellEffectManager.SpellMode.SolarBlast;
        npc1.CurrentUnitData.Gold = 41;
        // set starting equipment
        npc1.CurrentUnitData.EquipAmulet(PnlInventory.ItemMode.Empty);
        npc1.CurrentUnitData.EquipArmour(PnlInventory.ItemMode.Empty);
        npc1.CurrentUnitData.EquipWeapon(PnlInventory.ItemMode.Empty);
        npc1.CurrentUnitData.CurrentBattleUnitData.PotionsEquipped = new PnlInventory.ItemMode[3] {
            PnlInventory.ItemMode.CharismaPot, PnlInventory.ItemMode.Empty, PnlInventory.ItemMode.ManaPot
        };
        npc1.CurrentUnitData.CurrentBattleUnitData.ItemsHeld = new List<PnlInventory.ItemMode>() {
            PnlInventory.ItemMode.CharismaPot, PnlInventory.ItemMode.ManaPot,
            PnlInventory.ItemMode.ResiliencePot, PnlInventory.ItemMode.CharismaPot,
            PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.LuckPot,
            PnlInventory.ItemMode.RustedArmour, PnlInventory.ItemMode.ResiliencePot,
            PnlInventory.ItemMode.VigourPot, PnlInventory.ItemMode.ObsidianPlate,
            PnlInventory.ItemMode.RustedMace, PnlInventory.ItemMode.RustedArmour,
            PnlInventory.ItemMode.ScarabAmulet, PnlInventory.ItemMode.RustedMace,
            PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.SilverMace,
            PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.ManaPot,
            PnlInventory.ItemMode.LuckPot, PnlInventory.ItemMode.ManaPot,
        };
        Unit npc2 = new Unit();
        // STARTING PLAYER DATA HERE
        npc2.CurrentUnitData = new UnitData() {
            Player = true,
            Name = "npc22",
            ID = "npc2",
            BasePhysicalDamageRange = 1f, // ideally this would change depending on weapon equipped
            Modified = true,
            PortraitPath = "res://Interface/Icons/Water.png",
            PortraitPathSmall = "res://Interface/Icons/Water.png"
        };
        
        // set starting attributes
        foreach (UnitData.Attribute att in npc2.CurrentUnitData.Attributes.Keys.ToList())
        {
            npc2.CurrentUnitData.Attributes[att] = 10;
        }
        npc2.CurrentUnitData.UpdateDerivedStatsFromAttributes();
        // set starting spells
        npc2.CurrentUnitData.CurrentBattleUnitData.Spell1 = SpellEffectManager.SpellMode.SolarBolt;
        npc2.CurrentUnitData.CurrentBattleUnitData.Spell2 = SpellEffectManager.SpellMode.Empty;
        npc2.CurrentUnitData.CurrentBattleUnitData.SpellGainedAtHigherLevel = SpellEffectManager.SpellMode.SolarBlast;
        npc2.CurrentUnitData.Gold = 41;
        // set starting equipment
        npc2.CurrentUnitData.EquipAmulet(PnlInventory.ItemMode.Empty);
        npc2.CurrentUnitData.EquipArmour(PnlInventory.ItemMode.Empty);
        npc2.CurrentUnitData.EquipWeapon(PnlInventory.ItemMode.Empty);
        npc2.CurrentUnitData.CurrentBattleUnitData.PotionsEquipped = new PnlInventory.ItemMode[3] {
            PnlInventory.ItemMode.CharismaPot, PnlInventory.ItemMode.Empty, PnlInventory.ItemMode.ManaPot
        };
        npc2.CurrentUnitData.CurrentBattleUnitData.ItemsHeld = new List<PnlInventory.ItemMode>() {
            PnlInventory.ItemMode.ManaPot, PnlInventory.ItemMode.ManaPot,
            PnlInventory.ItemMode.ManaPot, PnlInventory.ItemMode.ManaPot,
            PnlInventory.ItemMode.LuckPot, PnlInventory.ItemMode.ManaPot,
            PnlInventory.ItemMode.ManaPot, PnlInventory.ItemMode.ManaPot,
            PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.ScarabAmulet,
            
        };

        ShopData shopData = new ShopData() {
            ShopBackgroundTexturePath = "res://Interface/Buttons/BtnBase/BtnDisabled.png",
            ShopTitle = "Test shoppe",
            IntroText = "welcome to my test shop!",
            ItemsStocked = new List<PnlInventory.ItemMode>() { 
            PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.CharismaPot,
            PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.LuckPot,
            PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.ResiliencePot,
            PnlInventory.ItemMode.VigourPot, PnlInventory.ItemMode.ObsidianPlate, }
        };
        //  -- from stageworld
        GetNode<HBoxPortraits>("HBoxPortraits").SetSingleUnitBtnByID(0, player.CurrentUnitData.ID);
        GetNode<HBoxPortraits>("HBoxPortraits").SetPortrait(player.CurrentUnitData.ID, GD.Load<Texture>(player.CurrentUnitData.PortraitPathSmall));
        GetNode<HBoxPortraits>("HBoxPortraits").SetSingleUnitBtnByID(1, npc1.CurrentUnitData.ID);
        GetNode<HBoxPortraits>("HBoxPortraits").SetPortrait(npc1.CurrentUnitData.ID, GD.Load<Texture>(npc1.CurrentUnitData.PortraitPathSmall));
        GetNode<HBoxPortraits>("HBoxPortraits").SetSingleUnitBtnByID(2, npc2.CurrentUnitData.ID);
        GetNode<HBoxPortraits>("HBoxPortraits").SetPortrait(npc2.CurrentUnitData.ID, GD.Load<Texture>(npc2.CurrentUnitData.PortraitPathSmall));
        //

        Start(player.CurrentUnitData, new List<UnitData>() {npc1.CurrentUnitData, npc2.CurrentUnitData}, shopData);
        
        
    }

    private List<UnitData> _currentBuyersUnitData = new List<UnitData>();
    private UnitData _activeBuyerUnitData;
    private ShopData _shopData;

    public void Start(UnitData playerData, List<UnitData> companionDatas, ShopData shopData)
    {
        _currentBuyersUnitData.Clear();
        _currentBuyersUnitData.Add(playerData);
        foreach (UnitData unitData in companionDatas)
        {
            _currentBuyersUnitData.Add(unitData);
        }
        _shopData = shopData;
        GetNode<Label>("LblTitle").Text = shopData.ShopTitle;
        GetNode<Label>("LblIntroText").Text = shopData.IntroText;
        GetNode<TextureRect>("TexRectImage").Texture = GD.Load<Texture>(shopData.ShopBackgroundTexturePath);
        UpdateGoldDisplay(playerData.Gold);

        // update BUY from merchant stock
        UpdateInventoryFromStock(shopData.ItemsStocked, GetNode<PnlInventory>("TabContainer/Buy/PnlInventoryBuy"));

        GetNode<HBoxPortraits>("HBoxPortraits").OnPortraitButtonPressed(playerData.ID);


        Visible = true;
    }

    public void UpdateGoldDisplay(int goldAmount)
    {
        GetNode<Label>("TexRectGold/LblGold").Text = goldAmount.ToString();
    }

    private ItemBuilder _itemBuilder = new ItemBuilder();

    private void UpdateInventoryFromStock(List<PnlInventory.ItemMode> itemsHeld, PnlInventory inventory)
    {
        inventory.ClearGrid();
        inventory.Start(6, Math.Max(4,Convert.ToInt32(Math.Ceiling(itemsHeld.Count/5f))), new Vector2(128,128));
        foreach (PnlInventory.ItemMode itemMode in itemsHeld)
        {
            if (itemMode == PnlInventory.ItemMode.Empty)
            {
                continue;
            }
            if (inventory.IsPotion(itemMode))
            {
                inventory.AddItemToNextEmptyRowsFirst(_itemBuilder.BuildPotion(itemMode));
            }
            else if (inventory.IsWeapon(itemMode))
            {
                inventory.AddItemToNextEmptyRowsFirst(_itemBuilder.BuildWeapon(itemMode));
            }
            else if (inventory.IsArmour(itemMode))
            {
                inventory.AddItemToNextEmptyRowsFirst(_itemBuilder.BuildArmour(itemMode));
            }
            else if (inventory.IsAmulet(itemMode))
            {
                inventory.AddItemToNextEmptyRowsFirst(_itemBuilder.BuildAmulet(itemMode));
            }
        }
    }


    // update SELL from specified unitID
    public void OnHBoxPortraitsPortraitPressed(string unitID)
    {
        UnitData unitData = _currentBuyersUnitData.Find(x => x.ID == unitID);
        _activeBuyerUnitData = unitData;
        List<PnlInventory.ItemMode> itemsHeld = unitData.CurrentBattleUnitData.ItemsHeld;
        PnlInventory sellerInventory = GetNode<PnlInventory>("TabContainer/Sell/PnlInventorySell");
        GetNode<Label>("LblSeller").Text = unitData.Name;
        UpdateInventoryFromStock(itemsHeld, sellerInventory);
    }

    public void OnItemHovered(IInventoryPlaceable item, PnlInventory source)
    {
        string buyFor = "";
        int cost = item.Cost;
        if (source == GetNode<PnlInventory>("TabContainer/Sell/PnlInventorySell"))
        {
            buyFor = " " + _shopData.BuyMessage + " ";
            cost = Convert.ToInt32(Math.Floor(cost/2f));
        }
        else
        {
            buyFor = " " + _shopData.SellMessage + " ";
            cost = Convert.ToInt32(Math.Ceiling(cost*2f));
        }
        if (item is InventoryItemEmpty)
        {
            GetNode<Label>("PnlStatus/LblStatus").Text = "";
        }
        else
        {
            GetNode<Label>("PnlStatus/LblStatus").Text = item.Name + "." + buyFor + cost + " gold.";// (source != GetNode<PnlInventory>("TabContainer/Sell/PnlInventorySell") ? " gold!" : " gold.");
        }
    }

    // TO FIX!!
    public void OnItemSelected(IInventoryPlaceable item, PnlInventory source)
    {
        if (item is InventoryItemEmpty)
        {
            return;
        }
        int cost = item.Cost;
        // selling items
        if (source == GetNode<PnlInventory>("TabContainer/Sell/PnlInventorySell"))
        {
            cost = Convert.ToInt32(Math.Floor(cost/_shopData.Stinginess));
            GetNode<PnlInventory>("TabContainer/Sell/PnlInventorySell").RemoveItem(item);
            _shopData.ItemsStocked.Add(item.CurrentItemMode);
            _currentBuyersUnitData[0].Gold += cost;
            _activeBuyerUnitData.CurrentBattleUnitData.ItemsHeld.Remove(item.CurrentItemMode);
            UpdateGoldDisplay(_currentBuyersUnitData[0].Gold);
            UpdateInventoryFromStock(_shopData.ItemsStocked, GetNode<PnlInventory>("TabContainer/Buy/PnlInventoryBuy"));
        }
        // buying items
        else
        {
            cost = Convert.ToInt32(Math.Ceiling(cost*_shopData.Stinginess));

            if (cost <= _currentBuyersUnitData[0].Gold)
            {
                GetNode<PnlInventory>("TabContainer/Buy/PnlInventoryBuy").RemoveItem(item);
                _shopData.ItemsStocked.Remove(item.CurrentItemMode);
                _currentBuyersUnitData[0].Gold -= cost;
                _activeBuyerUnitData.CurrentBattleUnitData.ItemsHeld.Add(item.CurrentItemMode);
                UpdateGoldDisplay(_currentBuyersUnitData[0].Gold);
                UpdateInventoryFromStock(_activeBuyerUnitData.CurrentBattleUnitData.ItemsHeld, GetNode<PnlInventory>("TabContainer/Sell/PnlInventorySell"));
            }
            else
            {
                GetNode<Label>("PnlStatus/LblStatus").Text = "You don't have enough gold!";
            }
        }
    }

    // at the end, as well as whenever switching inventory, update everything's list
    public void UpdateItemListsFromCurrentInventories() // for both shop and current buyer (before switching)
    {

    }

    public void Die()
    {
        GetNode<PnlInventory>("TabContainer/Sell/PnlInventorySell").Die();
        GetNode<PnlInventory>("TabContainer/Buy/PnlInventoryBuy").Die();
    }

}

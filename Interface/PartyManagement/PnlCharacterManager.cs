using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class PnlCharacterManager : Panel
{

    [Signal]
    public delegate void RequestedPause(bool pauseEnable);
    private PnlCharacterManagementAttributes _pnlAttributes;
    private PnlCharacterManagementStats _pnlStats;
    private CharacterInventory _characterInventory;
    public override void _Ready()
    {
        _pnlAttributes = GetNode<PnlCharacterManagementAttributes>("TabContainer/Character/TabContainer/Attributes");
        _pnlStats = GetNode<PnlCharacterManagementStats>("TabContainer/Character/TabContainer/Derived Stats (Advanced)");
        _characterInventory = GetNode<CharacterInventory>("TabContainer/Inventory");
        _characterInventory.Connect(nameof(CharacterInventory.CharacterStatsChanged), _pnlStats, nameof(PnlCharacterManagementStats.UpdateStatDisplay));
        _characterInventory.Connect(nameof(CharacterInventory.CharacterAttributesChanged), _pnlAttributes, nameof(PnlCharacterManagementAttributes.OnAttributesChanged));
        _pnlAttributes.Connect(nameof(PnlCharacterManagementAttributes.AttributePointSpent), _pnlStats, nameof(PnlCharacterManagementStats.UpdateStatDisplay));
        GetNode<HBoxPortraits>("HBoxPortraits").InCharacterManager = true;
        GetNode<HBoxPortraits>("HBoxPortraits").Connect(nameof(HBoxPortraits.InsideButtonOfCharacter), _characterInventory, nameof(CharacterInventory.OnInsideButtonOfCharacter));
        Visible = false;

        //TESTING. please keep this commented when doing release version

        // if (GetParent() == GetTree().Root && ProjectSettings.GetSetting("application/run/main_scene") != Filename)
        // {
        //     Test();
        // }

        //
    }

    public override void _Input(InputEvent ev)
    {
        base._Input(ev);
        if (!Visible)
        {
            return;
        }
        if (ev.IsActionPressed("Party 1") && !ev.IsEcho())
        {
            GetNode<HBoxPortraits>("HBoxPortraits").OnPortraitButtonPressedByIndex(0);
        }
        else if (ev.IsActionPressed("Party 2") && !ev.IsEcho())
        {
            GetNode<HBoxPortraits>("HBoxPortraits").OnPortraitButtonPressedByIndex(1);
        }
        else if (ev.IsActionPressed("Party 3") && !ev.IsEcho())
        {
            GetNode<HBoxPortraits>("HBoxPortraits").OnPortraitButtonPressedByIndex(2);
        }
        else if (ev.IsActionPressed("Inventory") && !ev.IsEcho())
        {
            GetNode<TabContainer>("TabContainer").CurrentTab = 1;
        }     
        // else if (ev.IsActionPressed("Pause") && !ev.IsEcho())
        // {
        //     OnBtnClosePressed();
        // }         
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
            // PnlInventory.ItemMode.LuckPot, PnlInventory.ItemMode.ManaPot,
            // PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.ManaPot,
            // PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.CharismaPot,
            // PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.LuckPot,
            // PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.ResiliencePot,
            // PnlInventory.ItemMode.VigourPot, PnlInventory.ItemMode.ObsidianPlate,
            // PnlInventory.ItemMode.ManaPot, PnlInventory.ItemMode.RustedArmour,
            // PnlInventory.ItemMode.ScarabAmulet, PnlInventory.ItemMode.RustedMace,
            // PnlInventory.ItemMode.VigourPot, PnlInventory.ItemMode.SilverMace,
            // PnlInventory.ItemMode.IntellectPot, PnlInventory.ItemMode.ManaPot,
            // PnlInventory.ItemMode.LuckPot, PnlInventory.ItemMode.ManaPot,
        };

        SetGold(player.CurrentUnitData.Gold);
        Start(player.CurrentUnitData, 1);
        Visible = true;
    }

    public void Start(UnitData unitData, int mode) // 0 = character, 1 = inventory
    {
        // load unit data
        GetNode<Label>("LblCharacterName").Text = unitData.Name;
        GetNode<TextureRect>("TabContainer/Character/TexRectPortrait").Texture = GD.Load<Texture>(unitData.PortraitPath);
        GetNode<Label>("TabContainer/Character/LblLevelExperience").Text = String.Format("Level: {0}\nExperience {1}/{2}",
            unitData.CurrentBattleUnitData.Level, Math.Floor(unitData.CurrentBattleUnitData.Experience), 
            unitData.ExperienceManager.GetTotalExperienceValueOfLevel(unitData.CurrentBattleUnitData.Level+1));
        
        _pnlAttributes.Start(unitData);
        _pnlStats.Start(unitData.CurrentBattleUnitData.Stats, new SpellEffectManager.SpellMode[2] 
            {unitData.CurrentBattleUnitData.Spell1, unitData.CurrentBattleUnitData.Spell2});
        _characterInventory.Start(unitData);
        GetNode<HBoxPortraits>("HBoxPortraits").DisableOnePortraitButtonByID(unitData.ID);
        // show relevant tab
        Visible = true;
        GetNode<TabContainer>("TabContainer").CurrentTab = mode;
    }

    public void SetGold(int num) // gold isuniversal
    {
        GetNode<Label>("TexRectGold/LblGold").Text = num.ToString();
    }
    
    public void OnBtnClosePressed()
    {
        // set sub-tabs to defaults
        GetNode<TabContainer>("TabContainer/Character/TabContainer").CurrentTab = 0;
        EmitSignal(nameof(RequestedPause), false);
        Visible = false;
    }

    public void Die() // we use c# events before we figured out that you can make a wrapper for those pesky unmarshalable variables
    {
        _characterInventory.Die();
    }

    
}

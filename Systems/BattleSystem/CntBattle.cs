// **TODO**
// implement attack
// implement take damage and get hit
// implement die
// implement cast spell: allied target, enemy target, enemy aoe, empty hex, is there more? check list

// AI. Something simple. E.g. start with either helper personality, or aggressive personality
// If not in range to attack/aggressive spell, if aggressive then move in range.
// // if not aggressive, then move away max AP and cast helper spell if available. otherwise move in range.
// If in range to attack and aggressive, then cast spell or attack if no mana.
// // if in range to attack but not aggressive, move away max AP then cast helper spell.

// *UI / ease of use polish:*
// Each unit health, mana, and faction UI
// Right click units. Probably need a ? over units that aren't attackable/targetable with the current action. Brings up info.
// Show AP cost next to mouse cursor (e.g. 5 out of 8)
// Show remaining AP somewhere at the bottom bar. Current turn: x. Remaining AP: y.
// Sort out flashtile colours
// Menu -> disable input when it is open
// refactor and comment

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class CntBattle : Control
{
    [Signal]
    public delegate void BattleEnded();
    public new bool Visible {
        get {
            return base.Visible;
        }
        set {
            base.Visible = value;
            GetNode<Control>("Panel/BattleHUD/CtrlTheme").Visible = value;
        }
    }
    // private ShaderMaterial _flashShader = GD.Load<ShaderMaterial>("res://Shaders/Outline/OutlineShader.tres");
    // private ShaderMaterial _flashShaderBlue;
    // private ShaderMaterial _flashShaderRed;
    private PackedScene _battleUnitScn = GD.Load<PackedScene>("res://Actors/BattleUnit/BattleUnit.tscn");
    private BattleGrid _battleGrid;
    private BattleHUD _battleHUD;
    private CursorControl _cursorControl;
    private BattleInteractionHandler _battleInteractionHandler = new BattleInteractionHandler();
    private BattleUnitData _playerData;
    private BattleUnitData _enemyCommanderData;
    private List<BattleUnitData> _friendliesData;
    private List<BattleUnitData> _hostilesData;
    private YSort _battleUnitsContainer;
    private enum StartPositionMode {Ally1, Ally2, Ally3, Ally4, Ally5, Ally6, Enemy1, Enemy2, Enemy3, Enemy4, Enemy5, Enemy6}
    public enum ActionMode { Move, Melee, Spell1, Spell2, Wait}
    private Dictionary<StartPositionMode, Vector2> _startPositions;
    // private Dictionary<Button, ActionMode> _btnActionModes;
    
    private List<BattleUnit> _turnList = new List<BattleUnit>();
    private ActionMode _currentSelectedAction = ActionMode.Move;
    private float _currentSpeedSetting = 1f;
    private int _round = 0;
    public override void _Ready()
    {
        Visible = false;
        
        _battleGrid = GetNode<BattleGrid>("Panel/BattleGrid");
        _battleUnitsContainer = _battleGrid.GetNode<YSort>("All/BattleUnits");
        _battleHUD = GetNode<BattleHUD>("Panel/BattleHUD");
        _cursorControl = GetNode<CursorControl>("Panel/BattleHUD/CursorControl");
        // _btnActionModes = new Dictionary<Button, ActionMode>() {

        // }
        for (int i = 0; i < _battleHUD.ActionButtons.Count; i++)
        {
            _battleHUD.ActionButtons.ElementAt(i).Value.Connect("pressed", this, nameof(SetSelectedAction), new Godot.Collections.Array{
                _battleHUD.ActionButtons.ElementAt(i).Key});
        }

        _startPositions = new Dictionary<StartPositionMode, Vector2>() {
            {StartPositionMode.Ally1, _battleGrid.GetCentredWorldPosFromWorldPos(_battleGrid.GetNode<Position2D>("StartPositions/AllyStartPositions/Position2D").GlobalPosition)},
            {StartPositionMode.Ally2, _battleGrid.GetCentredWorldPosFromWorldPos(_battleGrid.GetNode<Position2D>("StartPositions/AllyStartPositions/Position2D2").GlobalPosition)},
            {StartPositionMode.Ally3, _battleGrid.GetCentredWorldPosFromWorldPos(_battleGrid.GetNode<Position2D>("StartPositions/AllyStartPositions/Position2D3").GlobalPosition)},
            {StartPositionMode.Ally4, _battleGrid.GetCentredWorldPosFromWorldPos(_battleGrid.GetNode<Position2D>("StartPositions/AllyStartPositions/Position2D4").GlobalPosition)},
            {StartPositionMode.Ally5, _battleGrid.GetCentredWorldPosFromWorldPos(_battleGrid.GetNode<Position2D>("StartPositions/AllyStartPositions/Position2D5").GlobalPosition)},
            {StartPositionMode.Ally6, _battleGrid.GetCentredWorldPosFromWorldPos(_battleGrid.GetNode<Position2D>("StartPositions/AllyStartPositions/Position2D6").GlobalPosition)},
            {StartPositionMode.Enemy1, _battleGrid.GetCentredWorldPosFromWorldPos(_battleGrid.GetNode<Position2D>("StartPositions/EnemyStartPositions/Position2D7").GlobalPosition)},
            {StartPositionMode.Enemy2, _battleGrid.GetCentredWorldPosFromWorldPos(_battleGrid.GetNode<Position2D>("StartPositions/EnemyStartPositions/Position2D8").GlobalPosition)},
            {StartPositionMode.Enemy3, _battleGrid.GetCentredWorldPosFromWorldPos(_battleGrid.GetNode<Position2D>("StartPositions/EnemyStartPositions/Position2D9").GlobalPosition)},
            {StartPositionMode.Enemy4, _battleGrid.GetCentredWorldPosFromWorldPos(_battleGrid.GetNode<Position2D>("StartPositions/EnemyStartPositions/Position2D10").GlobalPosition)},
            {StartPositionMode.Enemy5, _battleGrid.GetCentredWorldPosFromWorldPos(_battleGrid.GetNode<Position2D>("StartPositions/EnemyStartPositions/Position2D11").GlobalPosition)},
            {StartPositionMode.Enemy6, _battleGrid.GetCentredWorldPosFromWorldPos(_battleGrid.GetNode<Position2D>("StartPositions/EnemyStartPositions/Position2D12").GlobalPosition)},
        };

        for (int i = 1; i < GetNode("Panel/BattleHUD/CtrlTheme/PnlUI/HBoxAnimSpeed").GetChildCount(); i++)
        {
            GetNode("Panel/BattleHUD/CtrlTheme/PnlUI/HBoxAnimSpeed").GetChild(i).Connect(
                "pressed", this, nameof(OnBtnAnimSpeedPressed), new Godot.Collections.Array{i});
        }
        OnBtnAnimSpeedPressed(1);

        //TEST
        Test();
    }

    public void Test()
    {
        BattleUnitData playerData = new BattleUnitData() {
            Name = "Khepri sun",
            Combatant = BattleUnit.Combatant.Beetle,
            Level = 3,
            Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                {BattleUnitData.DerivedStat.Health, 10},
                {BattleUnitData.DerivedStat.TotalHealth, 10},
                {BattleUnitData.DerivedStat.Mana, 10},
                {BattleUnitData.DerivedStat.TotalMana, 10},
                {BattleUnitData.DerivedStat.HealthRegen, 1},
                {BattleUnitData.DerivedStat.ManaRegen, 1},
                {BattleUnitData.DerivedStat.MagicResist, 10},
                {BattleUnitData.DerivedStat.PhysicalResist, 10},
                {BattleUnitData.DerivedStat.Dodge, 5},
                {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 3},
                {BattleUnitData.DerivedStat.SpellDamage, 10},
                {BattleUnitData.DerivedStat.Speed, 6},
                {BattleUnitData.DerivedStat.Initiative, 5},
                {BattleUnitData.DerivedStat.Leadership, 1},
                {BattleUnitData.DerivedStat.CriticalChance, 1},
                {BattleUnitData.DerivedStat.CurrentAP, 6}
            }
        };
        BattleUnitData enemyCommanderData = new BattleUnitData() {
            Name = "Mr Commander",
            Combatant = BattleUnit.Combatant.Wasp,
            Level = 3,
            Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                {BattleUnitData.DerivedStat.Health, 10},
                {BattleUnitData.DerivedStat.TotalHealth, 10},
                {BattleUnitData.DerivedStat.Mana, 10},
                {BattleUnitData.DerivedStat.TotalMana, 10},
                {BattleUnitData.DerivedStat.HealthRegen, 1},
                {BattleUnitData.DerivedStat.ManaRegen, 1},
                {BattleUnitData.DerivedStat.MagicResist, 10},
                {BattleUnitData.DerivedStat.PhysicalResist, 10},
                {BattleUnitData.DerivedStat.Dodge, 5},
                {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 2},
                {BattleUnitData.DerivedStat.SpellDamage, 10},
                {BattleUnitData.DerivedStat.Speed, 6},
                {BattleUnitData.DerivedStat.Initiative, 4},
                {BattleUnitData.DerivedStat.Leadership, 1},
                {BattleUnitData.DerivedStat.CriticalChance, 1},
                {BattleUnitData.DerivedStat.CurrentAP, 6}
            }
        };
        // GD.Print("fact: ", enemyCommanderData.PlayerFaction);
        enemyCommanderData.Stats[BattleUnitData.DerivedStat.Initiative] = 5;
        List<BattleUnitData> friendliesData = new List<BattleUnitData>() {
            new BattleUnitData() {
                Combatant = BattleUnit.Combatant.Noob,
                Level = 1,
                Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                    {BattleUnitData.DerivedStat.Health, 10},
                    {BattleUnitData.DerivedStat.TotalHealth, 10},
                    {BattleUnitData.DerivedStat.Mana, 10},
                    {BattleUnitData.DerivedStat.TotalMana, 10},
                    {BattleUnitData.DerivedStat.HealthRegen, 1},
                    {BattleUnitData.DerivedStat.ManaRegen, 1},
                    {BattleUnitData.DerivedStat.MagicResist, 10},
                    {BattleUnitData.DerivedStat.PhysicalResist, 10},
                    {BattleUnitData.DerivedStat.Dodge, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamageRange, 3},
                    {BattleUnitData.DerivedStat.SpellDamage, 10},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 3},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                }
            },
            new BattleUnitData() {
                Combatant = BattleUnit.Combatant.Noob,
                Level = 2,
                Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                    {BattleUnitData.DerivedStat.Health, 10},
                    {BattleUnitData.DerivedStat.TotalHealth, 10},
                    {BattleUnitData.DerivedStat.Mana, 10},
                    {BattleUnitData.DerivedStat.TotalMana, 10},
                    {BattleUnitData.DerivedStat.HealthRegen, 1},
                    {BattleUnitData.DerivedStat.ManaRegen, 1},
                    {BattleUnitData.DerivedStat.MagicResist, 10},
                    {BattleUnitData.DerivedStat.PhysicalResist, 10},
                    {BattleUnitData.DerivedStat.Dodge, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 3},
                    {BattleUnitData.DerivedStat.SpellDamage, 10},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 3},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                }
            },
            new BattleUnitData() {
                Combatant = BattleUnit.Combatant.Noob,
                Level = 1,
                Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                    {BattleUnitData.DerivedStat.Health, 10},
                    {BattleUnitData.DerivedStat.TotalHealth, 10},
                    {BattleUnitData.DerivedStat.Mana, 10},
                    {BattleUnitData.DerivedStat.TotalMana, 10},
                    {BattleUnitData.DerivedStat.HealthRegen, 1},
                    {BattleUnitData.DerivedStat.ManaRegen, 1},
                    {BattleUnitData.DerivedStat.MagicResist, 10},
                    {BattleUnitData.DerivedStat.PhysicalResist, 10},
                    {BattleUnitData.DerivedStat.Dodge, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 4},
                    {BattleUnitData.DerivedStat.SpellDamage, 10},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 2},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                }
            }
        };
        List<BattleUnitData> hostilesData = new List<BattleUnitData>() {
            new BattleUnitData() {
                Combatant = BattleUnit.Combatant.Wasp,
                Level = 2,
                Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                    {BattleUnitData.DerivedStat.Health, 10},
                    {BattleUnitData.DerivedStat.TotalHealth, 10},
                    {BattleUnitData.DerivedStat.Mana, 10},
                    {BattleUnitData.DerivedStat.TotalMana, 10},
                    {BattleUnitData.DerivedStat.HealthRegen, 1},
                    {BattleUnitData.DerivedStat.ManaRegen, 1},
                    {BattleUnitData.DerivedStat.MagicResist, 10},
                    {BattleUnitData.DerivedStat.PhysicalResist, 10},
                    {BattleUnitData.DerivedStat.Dodge, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 3},
                    {BattleUnitData.DerivedStat.SpellDamage, 10},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 1},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                }
            },
            new BattleUnitData() {
                Combatant = BattleUnit.Combatant.Beetle,
                Level = 1,
                Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                    {BattleUnitData.DerivedStat.Health, 10},
                    {BattleUnitData.DerivedStat.TotalHealth, 10},
                    {BattleUnitData.DerivedStat.Mana, 10},
                    {BattleUnitData.DerivedStat.TotalMana, 10},
                    {BattleUnitData.DerivedStat.HealthRegen, 1},
                    {BattleUnitData.DerivedStat.ManaRegen, 1},
                    {BattleUnitData.DerivedStat.MagicResist, 10},
                    {BattleUnitData.DerivedStat.PhysicalResist, 10},
                    {BattleUnitData.DerivedStat.Dodge, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 3},
                    {BattleUnitData.DerivedStat.SpellDamage, 10},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 2},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                }
            },
            new BattleUnitData() {
                Combatant = BattleUnit.Combatant.Wasp,
                Level = 1,
                Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                    {BattleUnitData.DerivedStat.Health, 10},
                    {BattleUnitData.DerivedStat.TotalHealth, 10},
                    {BattleUnitData.DerivedStat.Mana, 10},
                    {BattleUnitData.DerivedStat.TotalMana, 10},
                    {BattleUnitData.DerivedStat.HealthRegen, 1},
                    {BattleUnitData.DerivedStat.ManaRegen, 1},
                    {BattleUnitData.DerivedStat.MagicResist, 10},
                    {BattleUnitData.DerivedStat.PhysicalResist, 10},
                    {BattleUnitData.DerivedStat.Dodge, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 3},
                    {BattleUnitData.DerivedStat.SpellDamage, 10},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 3},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                }
            },
            new BattleUnitData() {
                Combatant = BattleUnit.Combatant.Noob,
                Level = 1,
                Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                    {BattleUnitData.DerivedStat.Health, 10},
                    {BattleUnitData.DerivedStat.TotalHealth, 10},
                    {BattleUnitData.DerivedStat.Mana, 10},
                    {BattleUnitData.DerivedStat.TotalMana, 10},
                    {BattleUnitData.DerivedStat.HealthRegen, 1},
                    {BattleUnitData.DerivedStat.ManaRegen, 1},
                    {BattleUnitData.DerivedStat.MagicResist, 10},
                    {BattleUnitData.DerivedStat.PhysicalResist, 10},
                    {BattleUnitData.DerivedStat.Dodge, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 1},
                    {BattleUnitData.DerivedStat.SpellDamage, 10},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 3},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                }
            },
            new BattleUnitData() {
                Combatant = BattleUnit.Combatant.Noob,
                Level = 1,
                Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                    {BattleUnitData.DerivedStat.Health, 10},
                    {BattleUnitData.DerivedStat.TotalHealth, 10},
                    {BattleUnitData.DerivedStat.Mana, 10},
                    {BattleUnitData.DerivedStat.TotalMana, 10},
                    {BattleUnitData.DerivedStat.HealthRegen, 1},
                    {BattleUnitData.DerivedStat.ManaRegen, 1},
                    {BattleUnitData.DerivedStat.MagicResist, 10},
                    {BattleUnitData.DerivedStat.PhysicalResist, 10},
                    {BattleUnitData.DerivedStat.Dodge, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 3},
                    {BattleUnitData.DerivedStat.SpellDamage, 10},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 3},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                }
            }
        };
        Start(playerData, enemyCommanderData, friendliesData, hostilesData);
    }

    private void SetFaction(List<BattleUnitData> combatants, bool playerFaction)
    {
        foreach (BattleUnitData unitData in combatants)
        {
            unitData.PlayerFaction = playerFaction;
            // GetBattleUnitFromData(unitData).SetFactionPanel();
        }
    }


    public void Start(BattleUnitData playerData, BattleUnitData enemyCommanderData, List<BattleUnitData> friendliesData, List<BattleUnitData> hostilesData)
    {
        Visible = true;
        // Save references locally
        _playerData = playerData;
        _enemyCommanderData = enemyCommanderData;
        _friendliesData = friendliesData;
        _hostilesData = hostilesData;

        // Generate playing board - DONE

        // Clear all battle units ?do this at end
        foreach (Node n in _battleUnitsContainer.GetChildren())
        {
            n.QueueFree();
        }

        // Generate each board piece from data and place on either side
        GenerateCombatants();

        // Set factions
        _playerData.PlayerFaction = true;
        SetFaction(_friendliesData, true);
        _enemyCommanderData.PlayerFaction = false;
        SetFaction(_hostilesData, false);
        
        // Initiate turns by initiative
        _round += 1;
        _battleHUD.LogEntry(String.Format("Round {0}!", _round));
        PopulateTurnListByInitiative();
        UpdateAllBattleUnitsHealthManaFactionBars();
        OnActiveBattleUnitTurnStart();
        // Battle loop

        // If all friendlies die, lose

        // If all enemies die, win and resurrect all if dead (box to say by divine will you and your allies are revitalised)

        // Calculate and show experience and level changes
        
        // emit signal battle over and back to adventure map, 
        // passing playerData and friendliesData to update battle unit data (do i need to..? cant i update from here)

        // player can adjust and see battleunit stats from adventure map
    }

    private void GenerateCombatants()
    {
        if (_friendliesData.Count > 5 || _hostilesData.Count > 5)
        {
            GD.Print("Error! Too many combatants passed into battle!");
            throw new Exception();
        }
        GenerateBattleUnit(StartPositionMode.Ally1, _playerData);
        for (int i = 0; i < _friendliesData.Count; i++)
        {
            GenerateBattleUnit((StartPositionMode)i+1, _friendliesData[i]);
        }
        GenerateBattleUnit(StartPositionMode.Enemy1, _enemyCommanderData);
        for (int i = 0; i < _hostilesData.Count; i++)
        {
            GenerateBattleUnit((StartPositionMode)i+7, _hostilesData[i]);
        }
    }

    // private void UpdateHealthManaBars(BattleUnit battleUnit)
    // {

    // }

    private void GenerateBattleUnit(StartPositionMode startPosMode, BattleUnitData data)
    {
        BattleUnit newBattleUnit = (BattleUnit) _battleUnitScn.Instance();
        _battleUnitsContainer.AddChild(newBattleUnit);
        newBattleUnit.GlobalPosition = _startPositions[startPosMode];
        newBattleUnit.CurrentBattleUnitData = data;

        newBattleUnit.CurrentBattleUnitData.Name = 
            newBattleUnit.CurrentBattleUnitData.Name == "" ? Enum.GetName(typeof(BattleUnit.Combatant), data.Combatant) 
            : newBattleUnit.CurrentBattleUnitData.Name;

        newBattleUnit.Direction = (data == _playerData || _friendliesData.Contains(data)) ? BattleUnit.DirectionFacingMode.UpRight
            : BattleUnit.DirectionFacingMode.DownLeft;
        newBattleUnit.SetSprite(data.Combatant);
        newBattleUnit.SetActionState(BattleUnit.ActionStateMode.Idle);
    }
    
    public void OnBtnEndTestPressed()
    {
        OnBattleEnded();
    }

    public void OnBattleEnded()
    {
        GD.Print("battle ended signal");
        EmitSignal(nameof(BattleEnded));
    }

    private List<BattleUnit> GetBattleUnits()
    {
        List<BattleUnit> allBattleUnits = new List<BattleUnit>();
        foreach (Node n in _battleGrid.GetNode("All/BattleUnits").GetChildren())
        {
            if (n is BattleUnit battleUnit)
            {
                // skip the ones that die (remember we free all battleunits at conclusion anyway)
                if (battleUnit.Dead)
                {
                    continue;
                }
                allBattleUnits.Add(battleUnit);
            }
        }
        return allBattleUnits;
    }

    public BattleUnit GetBattleUnitFromData(BattleUnitData data)
    {
        return GetBattleUnits().Find(x => x.CurrentBattleUnitData == data);
    }

    private void PopulateTurnListByInitiative()
    {
        _turnList.Clear(); // i don't think this is needed
        _turnList = GetBattleUnits().OrderByDescending(x => x.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Initiative]).ToList();
    }

    private BattleUnit GetActiveBattleUnit()
    {
        return _turnList[0];
    }

    private void OnActiveBattleUnitTurnStart()
    {
        RecalculateObstacles();
        GetActiveBattleUnit().SetOutlineShader(new float[3] {.8f, .7f, 0f});
        GetActiveBattleUnit().CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.CurrentAP] = GetActiveBattleUnit().CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Speed];
        HighlightMoveableSquares();
        SetSelectedAction(ActionMode.Move);
    }

    private void RecalculateObstacles()
    {
        List<Vector2> currentBattleUnitPositions = new List<Vector2>();
        foreach (BattleUnit battleUnit in GetBattleUnits())
        {
            if (battleUnit == GetActiveBattleUnit() || battleUnit.Dead)
            {
                continue;
            }
            Vector2 mapPos = _battleGrid.GetCorrectedGridPosition(battleUnit.GlobalPosition);
            currentBattleUnitPositions.Add(mapPos);
        }
        _battleGrid.RecalculateAStarMap(currentBattleUnitPositions);
    }

    public override void _Input(InputEvent ev)
    {
        if (_turnList.Count == 0)
        {
            return;
        }
        if (ev is InputEventMouseMotion)
        {
            SetNonActiveBattleUnitOutlines();
            // SetCursorOnMouseMotion();



            // Vector2 gridPos = _battleGrid.GetCorrectedGridPosition(GetGlobalMousePosition());
            // GD.Print("Current tile ID is: " + _battleGrid.GetNode<TileMap>("TileMapShadedTiles").GetCell((int)gridPos.x, (int)gridPos.y));
        }    
        if (ev is InputEventMouseButton btn)
        {
            if (btn.Pressed && !ev.IsEcho())
            {
                if (btn.ButtonIndex == (int)ButtonList.Left)
                {
                    OnLeftClick();

                    //

                }
                else if (btn.ButtonIndex == (int)ButtonList.Right)
                {
                    if (AreAllUnitsIdle())
                    {
                        if (GetBattleUnitAtGridPosition(_battleGrid.GetCorrectedGridPosition(GetGlobalMousePosition())) == null)
                        {
                            return;
                        }
                        if (GetBattleUnitAtGridPosition(_battleGrid.GetCorrectedGridPosition(GetGlobalMousePosition())).Dead)
                        {
                            return;
                        }
                        BattleUnitData battleUnitData = GetBattleUnitAtGridPosition(_battleGrid.GetCorrectedGridPosition(GetGlobalMousePosition())).CurrentBattleUnitData;
                        _battleHUD.UpdateAndShowUnitInfoPanel(battleUnitData.Name, battleUnitData.Stats);
                    }
                }
            }
        }
    }


    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (!AreAllUnitsIdle())
        {
            _cursorControl.SetCursor(CursorControl.CursorMode.Wait);
            _battleHUD.SetDisableAllActionButtons(true);
            _battleHUD.SetDisableAllAnimSpeedButtons(true);
            
        }
        else
        {
            SetCursorByAction();
            HighlightPathSquares();
            _battleHUD.SetDisableAllAnimSpeedButtons(false);
            _battleHUD.SetDisableSingleButton(GetNode<Button>("Panel/BattleHUD/CtrlTheme/PnlUI/HBoxAnimSpeed/BtnSpeed" + _currentSpeedSetting), true);
        }
        
        
        // GetNode<Button>("Panel/BattleHUD/CtrlTheme/PnlUI/BtnEndTurn").Disabled = !AreAllUnitsIdle();
        
    }

    private void OnBtnAnimSpeedPressed(int speedSetting)
    {
        _currentSpeedSetting = speedSetting;
        _battleHUD.SetDisableAllAnimSpeedButtons(false);
        _battleHUD.SetDisableSingleButton(GetNode<Button>("Panel/BattleHUD/CtrlTheme/PnlUI/HBoxAnimSpeed/BtnSpeed" + _currentSpeedSetting), true);
        SetAllBattleUnitSpeed(speedSetting*2);
    }

    // private void OnBtnActionPressed(A)

    private void SetSelectedAction(ActionMode actionMode)
    {
        _battleHUD.SetDisableAllActionButtons(false);
        _battleHUD.SetDisableSingleButton(_battleHUD.ActionButtons[actionMode], true);
        _currentSelectedAction = actionMode;
    }

    private void OnMoveActionApRemaining()
    {
        SetSelectedAction(ActionMode.Move);
        if (GetUnitAP(GetActiveBattleUnit()) < GetUnitSpeed(GetActiveBattleUnit())/2f)
        {
            _battleHUD.SetDisableAllActionButtons(true);
            _battleHUD.SetDisableSingleButton(_battleHUD.ActionButtons[ActionMode.Move], true);
            _battleHUD.SetDisableSingleButton(_battleHUD.ActionButtons[ActionMode.Wait], false);
        }
        HighlightMoveableSquares();
        // SetAllBattleUnitSpeed();
    }

    // private void On

    private bool AreAllUnitsIdle()
    {
        foreach (BattleUnit unit in GetBattleUnits())
        {
            if (unit.CurrentActionStateMode != BattleUnit.ActionStateMode.Idle)
            {
                return false;
            }
        }
        return true;
    }

    private void SetAllBattleUnitSpeed(float speed=2f)
    {
        foreach (BattleUnit battleUnit in GetBattleUnits())
        {
            battleUnit.SetAnimSpeed(speed);
        }
    }

    private void HighlightPathSquares()
    {

        if (AreAllUnitsIdle())
        {
            
            switch (_currentSelectedAction) // todo -convert to state pattern..
            {
                case ActionMode.Move:
                    HighlightMoveSquares();
                    break;
                case ActionMode.Melee:
                    HighlightMeleeSquares();
                    break;
                
            }
        }
    }

    private void HighlightMeleeSquares()
    {
        _battleGrid.GetNode<TileMap>("TileMapShadedTilesPath").Clear();
        Vector2 startMapPos = _battleGrid.GetCorrectedGridPosition(GetActiveBattleUnit().GlobalPosition);
        List<Vector2> neighbours = _battleGrid.GetHorizontalNeighbours(startMapPos);
        foreach (Vector2 point in neighbours)
        {
            // if (PermittedAttack(point))
            // {
            _battleGrid.GetNode<TileMap>("TileMapShadedTilesPath").SetCellv(point, 4);
            // }
        }
    }

    public bool PermittedAttack(Vector2 targetMapPos)
    {
        Vector2 startMapPos = _battleGrid.GetCorrectedGridPosition(GetActiveBattleUnit().GlobalPosition);
        List<Vector2> neighbours = _battleGrid.GetHorizontalNeighbours(startMapPos);
        if (!neighbours.Contains(targetMapPos))
        {
            return false;
        }

        if (GetBattleUnitAtGridPosition(targetMapPos) == null)
        {
            return false;
        }

        if (GetBattleUnitAtGridPosition(targetMapPos).CurrentBattleUnitData.PlayerFaction == GetActiveBattleUnit().CurrentBattleUnitData.PlayerFaction)
        {
            return false;
        }

        return true;

    }

    private BattleUnit GetBattleUnitAtGridPosition(Vector2 gridPos)
    {
        foreach (BattleUnit battleUnit in GetBattleUnits())
        {
            if (_battleGrid.GetCorrectedGridPosition(battleUnit.GlobalPosition) == gridPos)
            {
                return battleUnit;
            }
        }
        return null;
    }

    private void HighlightMoveSquares()
    {
        _battleGrid.GetNode<TileMap>("TileMapShadedTilesPath").Clear(); // consider making a new hex in krita
        Vector2 startMapPos = _battleGrid.GetCorrectedGridPosition(GetActiveBattleUnit().GlobalPosition);
        Vector2 mouseMapPos = _battleGrid.GetCorrectedGridPosition(GetGlobalMousePosition());
        if (PermittedMove(startMapPos, mouseMapPos))
        {
            foreach (Vector2 point in _battleGrid.CalculatePath(startMapPos, mouseMapPos))
            {   
                _battleGrid.GetNode<TileMap>("TileMapShadedTilesPath").SetCellv(point, 4);
            }
        }
    }

    public void OnLeftClick()
    {
        // don't allow click action if units are not idle or if mouse is over the UI
        if (!AreAllUnitsIdle() || IsMouseCursorOverUIPanel())
        {
            // GetActiveBattleUnit().SetAnimSpeed(8);
            return;
        }

        switch (_currentSelectedAction)
        {
            case ActionMode.Move:
                OnLeftClickMove();
                break;
            case ActionMode.Melee:
                OnLeftClickMelee();
                break;
            
        }

    }

    public async void OnLeftClickMelee()
    {
        Vector2 mouseMapPos = _battleGrid.GetCorrectedGridPosition(GetGlobalMousePosition());
        if (PermittedAttack(mouseMapPos))
        {
            GetActiveBattleUnit().TargetWorldPos = _battleGrid.GetCorrectedWorldPosition(mouseMapPos);
            GetActiveBattleUnit().SetActionState(BattleUnit.ActionStateMode.Casting);
            GetActiveBattleUnit().DetectingHalfway = true;
            await ToSignal(GetActiveBattleUnit(), nameof(BattleUnit.ReachedHalfwayAnimation));
            // GD.Print("reached halfway through animation");
            BattleUnit targetUnit = GetBattleUnitAtGridPosition(mouseMapPos);
            targetUnit.TargetWorldPos = _battleGrid.GetCentredWorldPosFromWorldPos(GetActiveBattleUnit().GlobalPosition);
            
            // do battle calculations
            float[] result = _battleInteractionHandler.CalculateMelee(GetActiveBattleUnit().CurrentBattleUnitData, targetUnit.CurrentBattleUnitData);
            targetUnit.UpdateHealthManaBars();
            _battleHUD.LogMeleeEntry(GetActiveBattleUnit().CurrentBattleUnitData.Name, targetUnit.CurrentBattleUnitData.Name, result,
                targetUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Health] < 0.1f);

            if (targetUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Health] < 0.1f)
            {
                _turnList.Remove(targetUnit);
            }
            targetUnit.SetActionState(targetUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Health] < 0.1f 
                ? BattleUnit.ActionStateMode.Dying : BattleUnit.ActionStateMode.Hit);
            await ToSignal(targetUnit, nameof(BattleUnit.CurrentActionCompleted));
            if (GetActiveBattleUnit().CurrentActionStateMode != BattleUnit.ActionStateMode.Idle)
            {
                await ToSignal(GetActiveBattleUnit(), nameof(BattleUnit.CurrentActionCompleted));
            }
            GetActiveBattleUnit().CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.CurrentAP] = 0;
            OnBtnEndTurnPressed();
        }
        
    }

    public async void OnLeftClickMove()
    {
        Vector2 startMapPos = _battleGrid.GetCorrectedGridPosition(GetActiveBattleUnit().GlobalPosition);
        Vector2 mouseMapPos = _battleGrid.GetCorrectedGridPosition(GetGlobalMousePosition());
        if (PermittedMove(startMapPos, mouseMapPos))
        {
            // get the series of world positions to move
            List<Vector2> worldPoints = new List<Vector2>();
            foreach (Vector2 point in _battleGrid.CalculatePath(startMapPos, mouseMapPos))
            {
                worldPoints.Add(_battleGrid.GetCorrectedWorldPosition(point));

            }
            // subtract AP cost from total AP
            float apCost = _battleGrid.GetDistanceToPoint(startMapPos, mouseMapPos);
            GetActiveBattleUnit().CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.CurrentAP] -=  apCost;
            
            // commence animation

            _battleGrid.GetNode<TileMap>("TileMapShadedTiles").Clear();
            _battleGrid.GetNode<TileMap>("TileMapShadedTilesLong").Clear();
            GetActiveBattleUnit().MoveAlongPoints(worldPoints);
            
            await ToSignal(GetActiveBattleUnit(), nameof(BattleUnit.CurrentActionCompleted));

            _battleGrid.GetNode<TileMap>("TileMapShadedTilesPath").Clear();
            // GD.Print(GetUnitAP(GetActiveBattleUnit()));
            if (GetUnitAP(GetActiveBattleUnit()) == 0)
            {
                OnBtnEndTurnPressed();
            }
            else
            {
                OnMoveActionApRemaining();
            }
        }
        else //if (PermittedAttack(mouseMapPos))
        {
            OnLeftClickMelee();
        }
    }
    
    private void OnBtnEndTurnPressed()
    {            
        EndTurn();
        OnActiveBattleUnitTurnStart();
    }

    private bool IsMouseCursorOverUIPanel()
    {
        return GetGlobalMousePosition().y > GetNode<Panel>("Panel/BattleHUD/CtrlTheme/PnlUI").RectGlobalPosition.y;
    }

    private void SetCursorByAction() // TODO: replace with state pattern
    {
        
        Vector2 mouseMapPos = _battleGrid.GetCorrectedGridPosition(GetGlobalMousePosition());
        if (IsMouseCursorOverUIPanel())
        {
            _cursorControl.SetCursor(CursorControl.CursorMode.Select);
        }
        else if (_currentSelectedAction == ActionMode.Move)
        {
            if (PermittedMove(_battleGrid.GetCorrectedGridPosition(GetActiveBattleUnit().GlobalPosition),mouseMapPos))
            {
                _cursorControl.SetCursor(CursorControl.CursorMode.Move);
            }
            else if (PermittedAttack(mouseMapPos))
            {
                _cursorControl.SetCursor(CursorControl.CursorMode.Attack);
            }
            else if (_battleGrid.TraversableCells.Contains(mouseMapPos) || IsObstacleAt(mouseMapPos))
            {
                _cursorControl.SetCursor(CursorControl.CursorMode.Invalid);
            }
        }
        else if (_currentSelectedAction == ActionMode.Melee)
        {
            if (PermittedAttack(mouseMapPos))
            {
                _cursorControl.SetCursor(CursorControl.CursorMode.Attack);
            }
            else
            {
                _cursorControl.SetCursor(CursorControl.CursorMode.Invalid);
            }
        }
        else if (_currentSelectedAction == ActionMode.Spell1 || _currentSelectedAction == ActionMode.Spell2)
        {
            _cursorControl.SetCursor(CursorControl.CursorMode.Spell);
        }
        else
        {
            _cursorControl.SetCursor(CursorControl.CursorMode.Select);
        }
    }

    private void HighlightMoveableSquares()
    {
        _battleGrid.GetNode<TileMap>("TileMapShadedTiles").Clear();
        _battleGrid.GetNode<TileMap>("TileMapShadedTilesLong").Clear();
        foreach (Vector2 cell in _battleGrid.TraversableCells)
        {
            if (PermittedMove(_battleGrid.GetCorrectedGridPosition(GetActiveBattleUnit().GlobalPosition), cell))
            {
                float abilAPCost = GetUnitSpeed(GetActiveBattleUnit())/2f;

                if ( _battleGrid.GetDistanceToPoint(_battleGrid.GetCorrectedGridPosition(GetActiveBattleUnit().GlobalPosition), cell)
                    > GetUnitAP(GetActiveBattleUnit()) - abilAPCost)
                {
                    _battleGrid.GetNode<TileMap>("TileMapShadedTilesLong").SetCellv(cell, 6);
                }
                else
                {
                    _battleGrid.GetNode<TileMap>("TileMapShadedTiles").SetCellv(cell, 4);
                }
            }
        }
    }

    private bool PermittedMove(Vector2 originMapPos, Vector2 targetMapPos)
    {
        int distance = _battleGrid.GetDistanceToPoint(originMapPos, targetMapPos);
        float currentAP = GetUnitAP(GetActiveBattleUnit());
        if (currentAP >= distance && distance > 0 && TargetWorldPosIsFree(targetMapPos))
        {
            return true;
        }
        return false;
    }

    private bool IsObstacleAt(Vector2 targetMapPos)
    {
        return _battleGrid.ObstacleCells.Contains(targetMapPos);
    }

    private float GetUnitAP(BattleUnit battleUnit)
    {
        return battleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.CurrentAP];
    }
    private float GetUnitSpeed(BattleUnit battleUnit)
    {
        return battleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Speed];
    }

    private bool TargetWorldPosIsFree(Vector2 targetMapPos)
    {
        foreach (BattleUnit battleUnit in GetBattleUnits())
        {
            if (_battleGrid.GetCorrectedGridPosition(battleUnit.GlobalPosition) == targetMapPos)
            {
                return false;
            }
        }
        return true;
    }

    private void SetNonActiveBattleUnitOutlines()
    {
        Vector2 mouseCentrePos = _battleGrid.GetCorrectedGridPosition(GetGlobalMousePosition());
        // GD.Print(mouseCentrePos);
        foreach (BattleUnit battleUnit in GetBattleUnits())
        {
            if (battleUnit == GetActiveBattleUnit())
            {
                continue;
            }
            Vector2 battleUnitCentrePos = _battleGrid.GetCorrectedGridPosition(battleUnit.GlobalPosition);
            if (mouseCentrePos == battleUnitCentrePos)
            {
                if (battleUnit.CurrentBattleUnitData == _playerData || _friendliesData.Contains(battleUnit.CurrentBattleUnitData))
                {
                    battleUnit.SetOutlineShader(new float[3] {0f, .6f, .7f});
                }
                else
                {
                    battleUnit.SetOutlineShader(new float[3] {0.7f, 0f, 0f});
                }
            }
            else
            {
                battleUnit.SetOutlineShader(null);
            }
        }
    }

    private void EndTurn()
    {
        GetActiveBattleUnit().SetOutlineShader(null);
        _turnList.RemoveAt(0);

        // NEW ROUND
        if (_turnList.Count == 0)
        {
            _round += 1;
            _battleHUD.LogEntry(String.Format("Round {0}!", _round));
            PopulateTurnListByInitiative();
            UpdateAllBattleUnitsHealthManaFactionBars();
        }
    }

    private void UpdateAllBattleUnitsHealthManaFactionBars()
    {
        foreach (BattleUnit battleUnit in GetBattleUnits())
        {
            battleUnit.UpdateHealthManaBars();
            battleUnit.SetFactionPanel();
        }
    }

// currently testing
    // public override void _Input(InputEvent ev)
    // {
    //     base._Input(ev);

    //     if (ev is InputEventMouseButton btn)
    //     {
    //         if (btn.Pressed && !ev.IsEcho())
    //         {
    //             if (btn.ButtonIndex == (int)ButtonList.Left)
    //             {
    //                 EndTurn();
    //                 OnActiveBattleUnitTurnStart();
    //             }
    //         }
    //     }
    // }
}
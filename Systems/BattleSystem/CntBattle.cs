// **TODO**
// implement attack DONE
// implement take damage and get hit DONE (for melee - can easily add take spell damage)
// implement die DONE
// implement cast spell: allied target, enemy target, enemy aoe, empty hex, is there more? check list DONE
// implement consumables: health food, mana water, potion of attribute
// probably utilise spelleffectmanager as this is simply the buff spell
// and make a separate potion class, from which multiple potions inherit
// click potion button -> bring up a grid of all potions unit possess (make a generic inventory grid that can be re-used)
// -> click potion -> spelleffectmanager to apply the effect (pass in the potioneffect) ALL DONE
// show status fx on right click DONE
// path hints for spells depending on the spell[0] target e.g. empty, area, self. DONE
// symbols and tooltips for each spell button DONE

// AI. Something simple. E.g. start with either helper personality, or aggressive personality DONE
// If not in range to attack/aggressive spell, if aggressive then move in range.
// // if not aggressive, then move away max AP and cast helper spell if available. otherwise move in range.
// If in range to attack and aggressive, then cast spell or attack if no mana.
// // if in range to attack but not aggressive, move away max AP then cast helper spell.

// Detect Defeat and Victory and end the battle, passing results onto the signal DONE
// Defeat - if all playerfaction units die
// Victory - if only playerfaction units remain

// *UI / ease of use polish:*
// UI - show current effects on right click DONE
// Right click units. Probably need a ? over units that aren't attackable/targetable with the current action. Brings up info. DONE
// Show AP cost next to mouse cursor (e.g. 5 out of 8) NO NEED
// Show remaining AP somewhere at the bottom bar. Current turn: x. Remaining AP: y. NO NEED
// Sort out flashtile colours - wait for sophia's battle grid background
// Menu -> disable input when it is open DONE

// refactor and comment.. uuhh ill just rewrite it if we continue.. use state pattern and divide battle into:
// // UnitInputState -> PlayerUnitInputState / AIUnitInputState
// // UnitAnimationState
// // RoundTransitionState
// and separate logic into components, e.g. GridHighlightController which is managed in UnitInputState...
// and maybe use builder pattern to construct multiple spells from basic effects
// and to construct potions from basic effects (decide colour by highest magnitude effect)
// and use hexes instead of squares...

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class CntBattle : Control
{
    [Signal]
    public delegate void BattleEnded(bool quitToMainMenu, bool victory, BattleUnitDataSignalWrapper wrappedEnemyCombatant); // does battleunitdata persist on end?
    // if it is not updated, we may need to pass it to this signal. e.g. to update the list of potions after battle.

    public new bool Visible {
        get {
            return base.Visible;
        }
        set {
            base.Visible = value;
            GetNode<Control>("Panel/BattleHUD/CtrlTheme").Visible = value;
        }
    }
    private PackedScene _battleUnitScn = GD.Load<PackedScene>("res://Actors/BattleUnit/BattleUnit.tscn");
    public BattleGrid BattleGrid {get; set;}
    private BattleHUD _battleHUD;
    private PnlPotion _pnlPotion;
    private PnlLog _pnlLog;
    private BattleHUDPnlMenu _pnlMenu;
    public PnlSettings PnlSettings {get; set;}
    private CursorControl _cursorControl;
    private BattleInteractionHandler _battleInteractionHandler = new BattleInteractionHandler();
    public SpellEffectManager CurrentSpellEffectManager {get; set;}
    private AITurnHandler _aiTurnHandler = new AITurnHandler();
    private BattleUnitData _playerData;
    private BattleUnitData _enemyCommanderData;
    private List<BattleUnitData> _friendliesData;
    private List<BattleUnitData> _hostilesData;
    private YSort _battleUnitsContainer;
    private enum StartPositionMode {Ally1, Ally2, Ally3, Ally4, Ally5, Ally6, Enemy1, Enemy2, Enemy3, Enemy4, Enemy5, Enemy6}
    public enum ActionMode { Move, Melee, Spell1, Spell2, Wait, Potion}
    private Dictionary<StartPositionMode, Vector2> _startPositions;
    // private Dictionary<Button, ActionMode> _btnActionModes;
    
    private List<BattleUnit> _turnList = new List<BattleUnit>();
    private ActionMode _currentSelectedAction = ActionMode.Move;
    private ActionMode _previousSelectedAction;
    private float _currentSpeedSetting = 1f;
    private int _round = 0;
    private bool _endTurnEffectsInProgress = false;

    public override void _Ready()
    {
        Visible = false;
        SetPhysicsProcess(false);
        SetProcessInput(false);
        BattleGrid = GetNode<BattleGrid>("Panel/BattleGrid");
        _battleUnitsContainer = BattleGrid.GetNode<YSort>("All/BattleUnits");
        _battleHUD = GetNode<BattleHUD>("Panel/BattleHUD");
        _cursorControl = GetNode<CursorControl>("Panel/BattleHUD/CursorControl");
        _pnlPotion = GetNode<PnlPotion>("Panel/BattleHUD/CtrlTheme/PnlPotion");
        _pnlLog = GetNode<PnlLog>("Panel/BattleHUD/CtrlTheme/PnlLog");
        _pnlMenu = GetNode<BattleHUDPnlMenu>("Panel/BattleHUD/CtrlTheme/PnlMenu");
        CurrentSpellEffectManager = new SpellEffectManager(_battleInteractionHandler, BattleGrid.GetNode<Node2D>("SpellEffects"));
        CurrentSpellEffectManager.Connect(nameof(SpellEffectManager.SpellEffectFinished), this, nameof(OnSpellEffectFinished));
        CurrentSpellEffectManager.Connect(nameof(SpellEffectManager.AnnouncingSpell), this, nameof(OnAnnouncingSpell));
        // _pnlPotion.Connect(nameof(PnlPotion.PotionSelected), this, nameof(OnPnlPotionPotionSelected));
        _pnlPotion.PotionSelected+=this.OnPnlPotionPotionSelected;
        // _btnActionModes = new Dictionary<Button, ActionMode>() {

        // }
        for (int i = 0; i < _battleHUD.ActionButtons.Count; i++)
        {
            _battleHUD.ActionButtons.ElementAt(i).Value.Connect("pressed", this, nameof(SetSelectedAction), new Godot.Collections.Array{
                _battleHUD.ActionButtons.ElementAt(i).Key});
            _battleHUD.ActionButtons.ElementAt(i).Value.Connect("mouse_entered", _battleHUD, nameof(BattleHUD.OnMouseEnteredActionButton),
                new Godot.Collections.Array{ _battleHUD.ActionButtons.ElementAt(i).Value});
            _battleHUD.ActionButtons.ElementAt(i).Value.Connect("mouse_exited", _battleHUD, nameof(BattleHUD.OnMouseExitedActionButton),
                new Godot.Collections.Array{ _battleHUD.ActionButtons.ElementAt(i).Value});
        }

        _startPositions = new Dictionary<StartPositionMode, Vector2>() {
            {StartPositionMode.Ally1, BattleGrid.GetCentredWorldPosFromWorldPos(BattleGrid.GetNode<Position2D>("StartPositions/AllyStartPositions/Position2D").GlobalPosition)},
            {StartPositionMode.Ally2, BattleGrid.GetCentredWorldPosFromWorldPos(BattleGrid.GetNode<Position2D>("StartPositions/AllyStartPositions/Position2D2").GlobalPosition)},
            {StartPositionMode.Ally3, BattleGrid.GetCentredWorldPosFromWorldPos(BattleGrid.GetNode<Position2D>("StartPositions/AllyStartPositions/Position2D3").GlobalPosition)},
            {StartPositionMode.Ally4, BattleGrid.GetCentredWorldPosFromWorldPos(BattleGrid.GetNode<Position2D>("StartPositions/AllyStartPositions/Position2D4").GlobalPosition)},
            {StartPositionMode.Ally5, BattleGrid.GetCentredWorldPosFromWorldPos(BattleGrid.GetNode<Position2D>("StartPositions/AllyStartPositions/Position2D5").GlobalPosition)},
            {StartPositionMode.Ally6, BattleGrid.GetCentredWorldPosFromWorldPos(BattleGrid.GetNode<Position2D>("StartPositions/AllyStartPositions/Position2D6").GlobalPosition)},
            {StartPositionMode.Enemy1, BattleGrid.GetCentredWorldPosFromWorldPos(BattleGrid.GetNode<Position2D>("StartPositions/EnemyStartPositions/Position2D7").GlobalPosition)},
            {StartPositionMode.Enemy2, BattleGrid.GetCentredWorldPosFromWorldPos(BattleGrid.GetNode<Position2D>("StartPositions/EnemyStartPositions/Position2D8").GlobalPosition)},
            {StartPositionMode.Enemy3, BattleGrid.GetCentredWorldPosFromWorldPos(BattleGrid.GetNode<Position2D>("StartPositions/EnemyStartPositions/Position2D9").GlobalPosition)},
            {StartPositionMode.Enemy4, BattleGrid.GetCentredWorldPosFromWorldPos(BattleGrid.GetNode<Position2D>("StartPositions/EnemyStartPositions/Position2D10").GlobalPosition)},
            {StartPositionMode.Enemy5, BattleGrid.GetCentredWorldPosFromWorldPos(BattleGrid.GetNode<Position2D>("StartPositions/EnemyStartPositions/Position2D11").GlobalPosition)},
            {StartPositionMode.Enemy6, BattleGrid.GetCentredWorldPosFromWorldPos(BattleGrid.GetNode<Position2D>("StartPositions/EnemyStartPositions/Position2D12").GlobalPosition)},
        };

        for (int i = 1; i < GetNode("Panel/BattleHUD/CtrlTheme/PnlUI/HBoxAnimSpeed").GetChildCount(); i++)
        {
            GetNode("Panel/BattleHUD/CtrlTheme/PnlUI/HBoxAnimSpeed").GetChild(i).Connect(
                "pressed", this, nameof(OnBtnAnimSpeedPressed), new Godot.Collections.Array{i});
        }
        OnBtnAnimSpeedPressed(1);

        //TEST
        if (GetParent() == GetTree().Root && ProjectSettings.GetSetting("application/run/main_scene") != Filename)
        {
            Test();
        }
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
                {BattleUnitData.DerivedStat.SpellDamage, 5},
                {BattleUnitData.DerivedStat.Speed, 6},
                {BattleUnitData.DerivedStat.Initiative, 5},
                {BattleUnitData.DerivedStat.Leadership, 1},
                {BattleUnitData.DerivedStat.CriticalChance, 1},
                {BattleUnitData.DerivedStat.CurrentAP, 6}
            },
            Spell1 = SpellEffectManager.SpellMode.SolarBolt,
            Spell2 = SpellEffectManager.SpellMode.SolarBlast
        };
        BattleUnitData enemyCommanderData = new BattleUnitData() {
            Name = "Mr Commander",
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
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 2},
                {BattleUnitData.DerivedStat.SpellDamage, 5},
                {BattleUnitData.DerivedStat.Speed, 6},
                {BattleUnitData.DerivedStat.Initiative, 4},
                {BattleUnitData.DerivedStat.Leadership, 15},
                {BattleUnitData.DerivedStat.CriticalChance, 1},
                {BattleUnitData.DerivedStat.CurrentAP, 6}
            },
            Spell1 = SpellEffectManager.SpellMode.PerilOfOsiris,
            Spell2 = SpellEffectManager.SpellMode.Teleport
        };
        // GD.Print("fact: ", enemyCommanderData.PlayerFaction);
        enemyCommanderData.Stats[BattleUnitData.DerivedStat.Initiative] = 5;
        List<BattleUnitData> friendliesData = new List<BattleUnitData>() {
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
                    {BattleUnitData.DerivedStat.SpellDamage, 5},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 3},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                },
                Spell1 = SpellEffectManager.SpellMode.WeighingOfTheHeart,
                Spell2 = SpellEffectManager.SpellMode.GazeOfTheDead
            },
            new BattleUnitData() {
                Combatant = BattleUnit.Combatant.Beetle,
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
                    {BattleUnitData.DerivedStat.SpellDamage, 5},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 3},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                },
                Spell1 = SpellEffectManager.SpellMode.Teleport,
                Spell2 = SpellEffectManager.SpellMode.LunarBlast
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
                    {BattleUnitData.DerivedStat.PhysicalDamageRange, 4},
                    {BattleUnitData.DerivedStat.SpellDamage, 5},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 2},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                },
                Spell1 = SpellEffectManager.SpellMode.HymnOfTheUnderworld,
                Spell2 = SpellEffectManager.SpellMode.PerilOfOsiris
            }
        };
        List<BattleUnitData> hostilesData = new List<BattleUnitData>() {
            new BattleUnitData() {
                Combatant = BattleUnit.Combatant.Beetle,
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
                    {BattleUnitData.DerivedStat.SpellDamage, 5},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 1},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                },
                Spell1 = SpellEffectManager.SpellMode.GazeOfTheDead,
                Spell2 = SpellEffectManager.SpellMode.HymnOfTheUnderworld
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
                    {BattleUnitData.DerivedStat.SpellDamage, 5},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 2},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                },
                Spell1 = SpellEffectManager.SpellMode.LunarBlast,
                Spell2 = SpellEffectManager.SpellMode.WeighingOfTheHeart
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
                    {BattleUnitData.DerivedStat.SpellDamage, 5},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 3},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                },
                Spell1 = SpellEffectManager.SpellMode.ComingForthByDay,
                Spell2 = SpellEffectManager.SpellMode.GazeOfTheDead
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
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 1},
                    {BattleUnitData.DerivedStat.SpellDamage, 5},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 3},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                },
                Spell1 = SpellEffectManager.SpellMode.HymnOfTheUnderworld,
                Spell2 = SpellEffectManager.SpellMode.SolarBlast
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
                    {BattleUnitData.DerivedStat.SpellDamage, 5},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 3},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                },
                Spell1 = SpellEffectManager.SpellMode.SolarBolt,
                Spell2 = SpellEffectManager.SpellMode.Teleport
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
        SetPhysicsProcess(true);
        SetProcessInput(true);
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
        _battleHUD.ClearLog();
        _battleHUD.LogEntry(String.Format("Round {0}!", _round));
        PopulateTurnListByInitiative();
        ApplyLeadershipBonus();
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
        OnBattleEnded(true);
    }

    public void OnBattleEnded(bool victory)
    {
        // GD.Print("battle ended signal");        
        
        foreach (Node n in BattleGrid.GetNode("All/BattleUnits").GetChildren())
        {
            n.QueueFree();
        }
        _round = 0;
        // set cursor to whatever is default
        _cursorControl.SetCursor(CursorControl.CursorMode.Select);
        SetPhysicsProcess(false);
        SetProcessInput(false);
        EmitSignal(nameof(BattleEnded), false, victory, new BattleUnitDataSignalWrapper() { CurrentBattleUnitData = _enemyCommanderData });
    }

    public List<BattleUnit> GetBattleUnits()
    {
        List<BattleUnit> allBattleUnits = new List<BattleUnit>();
        foreach (Node n in BattleGrid.GetNode("All/BattleUnits").GetChildren())
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

    public BattleUnit GetActiveBattleUnit()
    {
        if (_turnList.Count > 0)
        {
            return _turnList[0];
        }
        else
        {
            GD.Print("no units to get for next turn! we should probably end combat here or it will crash");
            return null;
        }
    }

    private async void OnActiveBattleUnitTurnStart()
    {        
        if (GetActiveBattleUnit() == null)
        {
            GD.Print("start of the active battle unit turn but there is no active battle unit!");
            //end combat here
        }

        // if (_roundEnd)
        // {
        //     await ToSignal(this, nameof(EndOfRoundEffectsFinished));
        //     _roundEnd = false;
        // }

        if (GetActiveBattleUnit().CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Speed] <= 0)
        {
            OnBtnEndTurnPressed();
        }

        _battleHUD.SetSpellUI(
            CurrentSpellEffectManager.SpellEffects[GetActiveBattleUnit().CurrentBattleUnitData.Spell1][0],
            CurrentSpellEffectManager.SpellEffects[GetActiveBattleUnit().CurrentBattleUnitData.Spell2][0]);
        
        RecalculateObstacles();
        GetActiveBattleUnit().SetOutlineShader(new float[3] {.8f, .7f, 0f});
        // GetActiveBattleUnit().CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.CurrentAP] = GetActiveBattleUnit().CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Speed];
        HighlightMoveableSquares();

        if (!GetActiveBattleUnit().CurrentBattleUnitData.PlayerFaction)
        {
            // GD.Print("do AI stuff here");
            
            // don't allow click action if units are not idle or if mouse is over the UI
            while (!AreAllUnitsIdle())
            {
                await ToSignal(GetTree(), "idle_frame");
                // GD.Print("waiting");
            }
            // GD.Print("ok rdy");
            _aiTurnHandler.SetAITurnState(GetActiveBattleUnit().CurrentBattleUnitData.CurrentAITurnStateMode);
            _aiTurnHandler.OnAITurn(this);
            return;
        }

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
            Vector2 mapPos = BattleGrid.GetCorrectedGridPosition(battleUnit.GlobalPosition);
            currentBattleUnitPositions.Add(mapPos);
        }
        BattleGrid.RecalculateAStarMap(currentBattleUnitPositions);
    }

    public override void _Input(InputEvent ev)
    {
        if (_turnList.Count == 0)
        {
            return;
        }

        if (GetActiveBattleUnit().CurrentBattleUnitData.PlayerFaction == false)
        {
            // dont allow player to input whilst AI turn
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

                    if (AreAllUnitsIdle())
                    {
                        if (_cursorControl.GetCursor() == CursorControl.CursorMode.Hint) // terrible
                        {
                            ShowUnitInfo();
                        }
                    }

                    //

                }
                else if (btn.ButtonIndex == (int)ButtonList.Right)
                {
                    if (AreAllUnitsIdle())
                    {
                        if (GetBattleUnitAtGridPosition(BattleGrid.GetCorrectedGridPosition(GetGlobalMousePosition())) == null)
                        {
                            return;
                        }
                        if (GetBattleUnitAtGridPosition(BattleGrid.GetCorrectedGridPosition(GetGlobalMousePosition())).Dead)
                        {
                            return;
                        }
                        ShowUnitInfo();
                    }
                }
            }
        }
        if (ev.IsActionPressed("Pause") && (!ev.IsEcho()))
        {
            if (_battleHUD.GetNode<Panel>("CtrlTheme/PnlMenu").Visible)
            {
                OnBtnResumePressed();
            }
            else
            {
                OnBtnMenuPressed();
            }
        }
    }

    private void ShowUnitInfo()
    {
        BattleUnitData battleUnitData = GetBattleUnitAtGridPosition(BattleGrid.GetCorrectedGridPosition(GetGlobalMousePosition())).CurrentBattleUnitData;
        Dictionary<SpellEffectManager.SpellMode, string> spellNames = new Dictionary<SpellEffectManager.SpellMode, string>();
        foreach (SpellEffectManager.SpellMode spell in CurrentSpellEffectManager.SpellEffects.Keys)
        {
            if (spell == SpellEffectManager.SpellMode.Empty)
            {
                continue;
            }
            spellNames.Add(spell, CurrentSpellEffectManager.SpellEffects[spell][0].Name);
        }
        List<string> spellNameList = new List<string>();
        
        if (battleUnitData.Spell1 != SpellEffectManager.SpellMode.Empty)
        {
            spellNameList.Add(spellNames[battleUnitData.Spell1]);
        }
        if (battleUnitData.Spell2 != SpellEffectManager.SpellMode.Empty)
        {
            spellNameList.Add(spellNames[battleUnitData.Spell2]);
        }
            
            
        _battleHUD.UpdateAndShowUnitInfoPanel(battleUnitData.Name, battleUnitData.Stats, battleUnitData.CurrentStatusEffects, 
            spellNames, spellNameList);
    }



    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if (_turnList.Count == 0)
        {
            return;
        }

        if (!AreAllUnitsIdle())
        {
            if (PnlSettings != null)
            {
                _cursorControl.SetCursor(!_pnlPotion.Visible && !_pnlMenu.Visible && !PnlSettings.Visible && !_pnlLog.Visible? CursorControl.CursorMode.Wait :
                    (_pnlPotion.CursorInsidePanel() && _pnlPotion.Visible) || (_pnlMenu.CursorInsidePanel() && _pnlMenu.Visible) ||
                    (PnlSettings.CursorInsidePanel() && PnlSettings.Visible) || (_pnlLog.CursorInsidePanel() && _pnlLog.Visible)
                    ? CursorControl.CursorMode.Select : CursorControl.CursorMode.Invalid);
            }
            else
            {
            _cursorControl.SetCursor(!_pnlPotion.Visible && !_pnlMenu.Visible && !_pnlLog.Visible? CursorControl.CursorMode.Wait :
                (_pnlPotion.CursorInsidePanel() && _pnlPotion.Visible) || (_pnlMenu.CursorInsidePanel() && _pnlMenu.Visible)  
                || (_pnlLog.CursorInsidePanel() && _pnlLog.Visible)
                ? CursorControl.CursorMode.Select : CursorControl.CursorMode.Invalid);
            }
            
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
        CurrentSpellEffectManager.AnimSpeed = speedSetting*2;
    }

    // private void OnBtnActionPressed(A)

    private void SetSelectedAction(ActionMode actionMode)
    {
        if (actionMode == ActionMode.Potion)
        {
            BattleGrid.GetNode<TileMap>("TileMapShadedTilesPath").Clear();
            _pnlPotion.PopulateGrid(GetActiveBattleUnit().CurrentBattleUnitData.Potions);
            _pnlPotion.Visible = true;
            
            
            return;
        }
        

        // UI - RELAYING INABILITY TO CAST SPELL TO PLAYER

        // APPROACH 1 - DISABLE BUTTONS:

        // _battleHUD.SetDisableAllActionButtons(false);
        // foreach (SpellEffectManager.SpellMode spell in new SpellEffectManager.SpellMode[] 
        //     {GetActiveBattleUnit().CurrentBattleUnitData.Spell1, GetActiveBattleUnit().CurrentBattleUnitData.Spell2})
        // {
        //     if (!SufficientManaToCastSpell(spell) || !AnyValidTargetsWithinRangeToCastSpell(spell))
        //     {
        //          _battleHUD.SetDisableSingleButton(_battleHUD.ActionButtons[
        //              spell == GetActiveBattleUnit().CurrentBattleUnitData.Spell1 ? ActionMode.Spell1 : ActionMode.Spell2], true);//GD.Print("insufficient mana");
        //     }
        // }

        // APPROACH 2 - WARN PLAYER WHEN CLICKING BUTTON -seems to be better

        SpellEffectManager.SpellMode spellTryingToCast = actionMode == ActionMode.Spell1 
            ? GetActiveBattleUnit().CurrentBattleUnitData.Spell1
            : GetActiveBattleUnit().CurrentBattleUnitData.Spell2;
        if (actionMode == ActionMode.Spell1 || actionMode == ActionMode.Spell2)
        {
            if (!SufficientManaToCastSpell(spellTryingToCast))
            {
                 LblFloatScore floatLabel = (LblFloatScore) GD.Load<PackedScene>("res://Interface/Labels/FloatScoreLabel/LblFloatScore.tscn").Instance();
                floatLabel.Text = "Insufficient mana";
                 _battleHUD.ActionButtons[actionMode].AddChild(floatLabel);
                 floatLabel.Start(_battleHUD.ActionButtons[actionMode].RectGlobalPosition + new Vector2(0, -50));
                 return;

            }
            else if (!AnyValidTargetsWithinRangeToCastSpell(spellTryingToCast))
            {
                 LblFloatScore floatLabel = (LblFloatScore) GD.Load<PackedScene>("res://Interface/Labels/FloatScoreLabel/LblFloatScore.tscn").Instance();
                floatLabel.Text = "No valid targets in range";
                 _battleHUD.ActionButtons[actionMode].AddChild(floatLabel);
                 floatLabel.Start(_battleHUD.ActionButtons[actionMode].RectGlobalPosition + new Vector2(0, -50));
                return;
            }
        }
        _battleHUD.SetDisableAllActionButtons(false);
        //


        if (GetActiveBattleUnit().CurrentBattleUnitData.Spell1 == SpellEffectManager.SpellMode.Empty)
        {
             _battleHUD.SetDisableSingleButton(_battleHUD.ActionButtons[ActionMode.Spell1], true);
        }
        if (GetActiveBattleUnit().CurrentBattleUnitData.Spell2 == SpellEffectManager.SpellMode.Empty)
        {
             _battleHUD.SetDisableSingleButton(_battleHUD.ActionButtons[ActionMode.Spell2], true);
        }
        _battleHUD.SetDisableSingleButton(_battleHUD.ActionButtons[actionMode], true);
        _currentSelectedAction = actionMode;
    }

    public void OnPnlPotionPotionSelected(PotionEffect.PotionMode potionMode)
    {
        // GD.Print(potionEffect);
        _pnlPotion.Visible = false;
        // remove item from inventory
        GetActiveBattleUnit().CurrentBattleUnitData.Potions.Remove(potionMode);
        // use the spell code to apply the potion
        CurrentSpellEffectManager.ApplyPotionEffect(GetActiveBattleUnit(), _pnlPotion.PotionBuilder.BuildPotion(potionMode));
        // OnSpellEffectFinished is now called, ending the turn after LogEntry
    }

    public void OnPnlPotionBtnCancelPressed()
    {
        _pnlPotion.Visible = false;
        SetSelectedAction(_currentSelectedAction);
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

    public void OnBtnMenuPressed()
    {
        BattleGrid.GetNode<TileMap>("TileMapShadedTilesPath").Clear();
        _battleHUD.GetNode<Panel>("CtrlTheme/PnlMenu").Visible = true;
    }
    public void OnBtnResumePressed()
    {
        _battleHUD.GetNode<Panel>("CtrlTheme/PnlMenu").Visible = false;
        SetSelectedAction(_currentSelectedAction);
    }

    public void OnLogBtnClosePressed()
    {
        _pnlLog.Visible = false;
        SetSelectedAction(_currentSelectedAction);
    }

    public bool AreAllUnitsIdle()
    {
        if (_pnlPotion.Visible || _pnlMenu.Visible || _pnlLog.Visible)
        {
            return false;
        }
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

        BattleGrid.GetNode<TileMap>("TileMapShadedTilesPath").Clear();
        BattleGrid.GetNode<TileMap>("TileMapShadedTilesAOE").Clear();
        if (!GetActiveBattleUnit().CurrentBattleUnitData.PlayerFaction)
        {
            return;
        }
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
                case ActionMode.Spell1:
                    if (GetActiveBattleUnit().CurrentBattleUnitData.Spell1 == SpellEffectManager.SpellMode.Empty)
                    {
                        break;
                    }
                    HighlightSpellSquares(GetActiveBattleUnit().CurrentBattleUnitData.Spell1);
                    break;
                case ActionMode.Spell2:
                    if (GetActiveBattleUnit().CurrentBattleUnitData.Spell2 == SpellEffectManager.SpellMode.Empty)
                    {
                        break;
                    }
                    HighlightSpellSquares(GetActiveBattleUnit().CurrentBattleUnitData.Spell2);
                    break;
                
            }
        }
    }
    
    private void HighlightSpellSquares(SpellEffectManager.SpellMode spell)
    {
        // _battleGrid.GetNode<TileMap>("TileMapShadedTilesPath").Clear();
        // _battleGrid.GetNode<TileMap>("TileMapShadedTilesAOE").Clear();
        // Vector2 targetMapPos = _battleGrid.GetCorrectedGridPosition(GetGlobalMousePosition());
        if (spell == SpellEffectManager.SpellMode.Empty)
        {
            return;
        }
        foreach (Vector2 point in BattleGrid.GetAllCells())
        {
            if (PermittedSpell(spell, point))
            {
                BattleGrid.GetNode<TileMap>("TileMapShadedTilesPath").SetCellv(point, 4);
            }
        }
        Vector2 mouseMapPos = BattleGrid.GetCorrectedGridPosition(GetGlobalMousePosition());
        if (CurrentSpellEffectManager.SpellEffects[spell][0].AreaSquares > 0)
        {
            if (PermittedSpell(spell, mouseMapPos))
            {
                foreach (Vector2 point in GetCells(CurrentSpellEffectManager.SpellEffects[spell][0].AreaSquares, mouseMapPos))
                {
                    BattleGrid.GetNode<TileMap>("TileMapShadedTilesAOE").SetCellv(point, 4);
                }
            }
        }
    }

    private void HighlightMeleeSquares()
    {
        BattleGrid.GetNode<TileMap>("TileMapShadedTilesPath").Clear();
        Vector2 startMapPos = BattleGrid.GetCorrectedGridPosition(GetActiveBattleUnit().GlobalPosition);
        List<Vector2> neighbours = BattleGrid.GetHorizontalNeighbours(startMapPos);
        foreach (Vector2 point in neighbours)
        {
            // if (PermittedAttack(point))
            // {
            BattleGrid.GetNode<TileMap>("TileMapShadedTilesPath").SetCellv(point, 4);
            // }
        }
    }

    public bool PermittedAttack(Vector2 targetMapPos)
    {
        Vector2 startMapPos = BattleGrid.GetCorrectedGridPosition(GetActiveBattleUnit().GlobalPosition);
        List<Vector2> neighbours = BattleGrid.GetHorizontalNeighbours(startMapPos);
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

    public BattleUnit GetBattleUnitAtGridPosition(Vector2 gridPos)
    {
        foreach (BattleUnit battleUnit in GetBattleUnits())
        {
            if (BattleGrid.GetCorrectedGridPosition(battleUnit.GlobalPosition) == gridPos)
            {
                return battleUnit;
            }
        }
        return null;
    }

    private void HighlightMoveSquares()
    {
        BattleGrid.GetNode<TileMap>("TileMapShadedTilesPath").Clear(); // consider making a new hex in krita
        Vector2 startMapPos = BattleGrid.GetCorrectedGridPosition(GetActiveBattleUnit().GlobalPosition);
        Vector2 mouseMapPos = BattleGrid.GetCorrectedGridPosition(GetGlobalMousePosition());
        if (PermittedMove(startMapPos, mouseMapPos))
        {
            foreach (Vector2 point in BattleGrid.CalculatePath(startMapPos, mouseMapPos))
            {   
                BattleGrid.GetNode<TileMap>("TileMapShadedTilesPath").SetCellv(point, 4);
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

        // GD.Print(_currentSelectedAction);
        Vector2 mouseMapPos = BattleGrid.GetCorrectedGridPosition(GetGlobalMousePosition());
        switch (_currentSelectedAction)
        {
            case ActionMode.Move:
                OnLeftClickMove(mouseMapPos);
                break;
            case ActionMode.Melee:
                OnLeftClickMelee(mouseMapPos);
                break;
            case ActionMode.Spell1:
                if (GetActiveBattleUnit().CurrentBattleUnitData.Spell1 == SpellEffectManager.SpellMode.Empty)
                {
                    break;
                }
                OnLeftClickSpell(GetActiveBattleUnit().CurrentBattleUnitData.Spell1);
                break;
            case ActionMode.Spell2:
                if (GetActiveBattleUnit().CurrentBattleUnitData.Spell2 == SpellEffectManager.SpellMode.Empty)
                {
                    break;
                }
                OnLeftClickSpell(GetActiveBattleUnit().CurrentBattleUnitData.Spell2);
                // GetBattleUnitsAtArea(1, _battleGrid.GetCorrectedGridPosition(GetGlobalMousePosition()));
                break;    
        }

    }

    private void OnLeftClickSpell(SpellEffectManager.SpellMode spellMode)
    {
        Vector2 targetMapPos = BattleGrid.GetCorrectedGridPosition(GetGlobalMousePosition());
        Vector2 startingMapPos = BattleGrid.GetCorrectedGridPosition(GetActiveBattleUnit().GlobalPosition);

        if (spellMode == SpellEffectManager.SpellMode.Empty)
        {
            return;
        }
        if (PermittedSpell(spellMode, targetMapPos))
        {
            CurrentSpellEffectManager.SpellMethods[spellMode]
                (GetActiveBattleUnit(),
                GetBattleUnitAtGridPosition(targetMapPos),
                GetBattleUnitsAtArea(CurrentSpellEffectManager.SpellEffects[spellMode][0].AreaSquares, targetMapPos),
                BattleGrid.GetCorrectedWorldPosition(targetMapPos));            

            // GD.Print(_spellEffectManager.SpellEffects[spellMode].RangeSquares);
        }
    }

    public void OnAnnouncingSpell(string announceText)
    {
        _battleHUD.LogEntry(announceText);
    }

    private void OnSpellEffectFinished(string announceText, BattleUnit target, List<BattleUnit> unitsAtArea)
    {
        // _battleHUD.LogEntry(announceText);
        if (target != null)
        {
            if (target.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Health] < 0.1f)
            {
                _turnList.Remove(target);
            }
        }
        if (unitsAtArea != null)
        {
            foreach (BattleUnit tarBattleUnit in unitsAtArea)
            {
                if (tarBattleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Health] < 0.1f)
                {
                    _turnList.Remove(tarBattleUnit);
                }
            }
        }
        // if (GetActiveBattleUnit() != null)
        // {
        //     GetActiveBattleUnit().CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.CurrentAP] = 0;
        // }
        OnBtnEndTurnPressed();
    }

    public bool SufficientManaToCastSpell(SpellEffectManager.SpellMode spell)
    {
        float totalCost = 0;
        foreach (SpellEffect spellEffect in CurrentSpellEffectManager.SpellEffects[spell])
        {

            totalCost += spellEffect.ManaCost;
        }

        // check cost
        if (totalCost > GetActiveBattleUnit().CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Mana])
        {
            return false;
        }
        return true;
    }

    public bool AnyValidTargetsWithinRangeToCastSpell(SpellEffectManager.SpellMode spell) // also checks mana however
    {
        Vector2 startingMapPos = BattleGrid.GetCorrectedGridPosition(GetActiveBattleUnit().GlobalPosition);
        foreach (Vector2 point in BattleGrid.GetAllCells())
        {
            if (PermittedSpell(spell, point))
            {
                // GD.Print("insufficient range");
                return true;
            }
        }
        return false;
    }

    public bool PermittedSpell(SpellEffectManager.SpellMode spell, Vector2 targetMapPos)
    {
        if (spell == SpellEffectManager.SpellMode.Empty)
        {
            return false;
        }
        Vector2 startingMapPos = BattleGrid.GetCorrectedGridPosition(GetActiveBattleUnit().GlobalPosition);
        
        float totalCost = 0;
        foreach (SpellEffect spellEffect in CurrentSpellEffectManager.SpellEffects[spell])
        {
            // check range
            if (GetDistanceInSquares(startingMapPos, targetMapPos) > spellEffect.RangeSquares)
            {
                // GD.Print("insufficient range");
                return false;
            }

            // add cost
            totalCost += spellEffect.ManaCost;

            // check target meets target criteria
            if (spellEffect.Target == SpellEffect.TargetMode.Hostile)
            {
                if (GetBattleUnitAtGridPosition(targetMapPos) == null)
                {
                    return false;
                }
                if (GetActiveBattleUnit().CurrentBattleUnitData.PlayerFaction == GetBattleUnitAtGridPosition(targetMapPos).CurrentBattleUnitData.PlayerFaction)
                {
                    return false;
                }
            }
            else if (spellEffect.Target == SpellEffect.TargetMode.Ally)
            {   
                if (GetBattleUnitAtGridPosition(targetMapPos) == null)
                {
                    return false;
                }
                if (GetActiveBattleUnit().CurrentBattleUnitData.PlayerFaction != GetBattleUnitAtGridPosition(targetMapPos).CurrentBattleUnitData.PlayerFaction)
                {
                    return false;
                }
            }
            else if (spellEffect.Target == SpellEffect.TargetMode.Area)
            {
                // any criteria?
            }
            else if (spellEffect.Target == SpellEffect.TargetMode.Empty)
            {
                if (BattleGrid.ObstacleCells.Contains(targetMapPos))
                {
                    return false;
                }
                if (!BattleGrid.TraversableCells.Contains(targetMapPos))
                {
                    return false;
                }
                if (GetBattleUnitAtGridPosition(targetMapPos) != null)
                {
                    return false;
                }
            }
        }
        // check cost
        if (totalCost > GetActiveBattleUnit().CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Mana])
        {
            return false;
        }

        return true;
    }

    public List<BattleUnit> GetBattleUnitsAtArea(int radius, Vector2 tarGridPos)
    {
        List<BattleUnit> result = new List<BattleUnit>();
        for (float x = tarGridPos.x - radius; x <= tarGridPos.x + radius; x++)
        {
            for (float y = tarGridPos.y - radius; y <= tarGridPos.y + radius; y++)
            {
                Vector2 newGridPos = new Vector2(x, y);
                if (GetBattleUnitAtGridPosition(newGridPos) != null)
                {
                    result.Add(GetBattleUnitAtGridPosition(newGridPos));
                }
            }
        }
        
        return result;
    }

    public List<BattleUnit> GetNeighbouringBattleUnits(Vector2 tarGridPos) // within 1 square-melee strike distance
    {
        List<BattleUnit> result = new List<BattleUnit>();
        foreach (Vector2 gridPos in BattleGrid.GetHorizontalNeighbours(tarGridPos))
        {
            if (GetBattleUnitAtGridPosition(gridPos) != null)
            {
                result.Add(GetBattleUnitAtGridPosition(gridPos));
            }
        }
        return result;
    }

    private List<Vector2> GetCells(int radius, Vector2 tarGridPos)
    {
        List<Vector2> result = new List<Vector2>();
        for (float x = tarGridPos.x - radius; x <= tarGridPos.x + radius; x++)
        {
            for (float y = tarGridPos.y - radius; y <= tarGridPos.y + radius; y++)
            {
                Vector2 cell = new Vector2(x, y);
                if (BattleGrid.GetAllCells().Contains(cell))
                {
                    result.Add(cell);
                }
            }
        }
        return result;
    }

    private int GetDistanceInSquares(Vector2 originGridPos, Vector2 tarGridPos)
    {
        return Convert.ToInt32(Math.Abs(tarGridPos.x - originGridPos.x) + Math.Abs(tarGridPos.y - originGridPos.y));
    }

    public async void OnLeftClickMelee(Vector2 targetGridPos)
    {
        
        if (PermittedAttack(targetGridPos))
        {
            GetActiveBattleUnit().TargetWorldPos = BattleGrid.GetCorrectedWorldPosition(targetGridPos);
            GetActiveBattleUnit().SetActionState(BattleUnit.ActionStateMode.Casting);
            GetActiveBattleUnit().DetectingHalfway = true;
            await ToSignal(GetActiveBattleUnit(), nameof(BattleUnit.ReachedHalfwayAnimation));
            // GD.Print("reached halfway through animation");
            BattleUnit targetUnit = GetBattleUnitAtGridPosition(targetGridPos);
            targetUnit.TargetWorldPos = BattleGrid.GetCentredWorldPosFromWorldPos(GetActiveBattleUnit().GlobalPosition);
            
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
            // GetActiveBattleUnit().CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.CurrentAP] = 0;
            OnBtnEndTurnPressed();
        }
        
    }

    public async void OnLeftClickMove(Vector2 tarGridPos, bool finalMovement = false)
    {
        Vector2 startMapPos = BattleGrid.GetCorrectedGridPosition(GetActiveBattleUnit().GlobalPosition);
        // Vector2 mouseMapPos = BattleGrid.GetCorrectedGridPosition(GetGlobalMousePosition());
        if (PermittedMove(startMapPos, tarGridPos))
        {
            // get the series of world positions to move
            List<Vector2> worldPoints = new List<Vector2>();
            foreach (Vector2 point in BattleGrid.CalculatePath(startMapPos, tarGridPos))
            {
                worldPoints.Add(BattleGrid.GetCorrectedWorldPosition(point));

            }
            // subtract AP cost from total AP
            float apCost = BattleGrid.GetDistanceToPoint(startMapPos, tarGridPos);
            // GD.Print("distance is: ", apCost);
            GetActiveBattleUnit().CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.CurrentAP] -=  apCost;
            
            // commence animation

            BattleGrid.GetNode<TileMap>("TileMapShadedTiles").Clear();
            BattleGrid.GetNode<TileMap>("TileMapShadedTilesLong").Clear();
            GetActiveBattleUnit().MoveAlongPoints(worldPoints);

            _battleHUD.LogEntry(String.Format("{0} moves {1} step{3}. {2} action points remaining.",
                GetActiveBattleUnit().CurrentBattleUnitData.Name, apCost, GetUnitAP(GetActiveBattleUnit()), apCost > 1 ? "s" : ""));
            
            await ToSignal(GetActiveBattleUnit(), nameof(BattleUnit.CurrentActionCompleted));

            BattleGrid.GetNode<TileMap>("TileMapShadedTilesPath").Clear();

            if (GetUnitAP(GetActiveBattleUnit()) == 0 || finalMovement)
            {
                // GD.Print("out of AP or ideaz if i am roobot.. so stopping turn");
                OnBtnEndTurnPressed();
            }
            else
            {
                OnMoveActionApRemaining();
            }
        }
        else //if (PermittedAttack(mouseMapPos))
        {
            OnLeftClickMelee(tarGridPos);
        }
    }
    
    public void OnBtnEndTurnPressed()
    {            
        GetActiveBattleUnit().CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.CurrentAP] = 0;

        // _battleHUD.LogEntry(String.Format("{0}'s turn ends.", GetActiveBattleUnit().CurrentBattleUnitData.Name));

        // victory and defeat checks
        // defeat: no player units left
        if (GetBattleUnits().FindAll(x => x.CurrentBattleUnitData.PlayerFaction).Count == 0)
        {
            if (GetParent() == GetTree().Root && ProjectSettings.GetSetting("application/run/main_scene") != Filename) //TEST
            {
                GD.Print("DEFEAT");
                GetTree().Quit();
            }
            // EmitSignal(nameof(BattleEnded), false, false);
            OnBattleEnded(false);
            return;
        }
        // victory: no enemyuntis left AND at least 1 player unit left
        else if (GetBattleUnits().FindAll(x => !x.CurrentBattleUnitData.PlayerFaction).Count == 0 &&
            GetBattleUnits().FindAll(x => x.CurrentBattleUnitData.PlayerFaction).Count > 0)
        {
            if (GetParent() == GetTree().Root && ProjectSettings.GetSetting("application/run/main_scene") != Filename)
            {
                GD.Print("VICTORY");
                GetTree().Quit();
            }
            OnBattleEnded(true);
            return;
        }


        EndTurn();
        if (!_endTurnEffectsInProgress)
        {
            OnActiveBattleUnitTurnStart();
        }
    }

    private bool IsMouseCursorOverUIPanel()
    {
        return GetGlobalMousePosition().y > GetNode<Panel>("Panel/BattleHUD/CtrlTheme/PnlUI").RectGlobalPosition.y;
    }

    private void SetCursorByAction() // TODO: replace with state pattern
    {
        
        Vector2 mouseMapPos = BattleGrid.GetCorrectedGridPosition(GetGlobalMousePosition());
        if (IsMouseCursorOverUIPanel())
        {
            _cursorControl.SetCursor(CursorControl.CursorMode.Select);
        }
        else if (_currentSelectedAction == ActionMode.Move)
        {
            if (PermittedMove(BattleGrid.GetCorrectedGridPosition(GetActiveBattleUnit().GlobalPosition),mouseMapPos))
            {
                _cursorControl.SetCursor(CursorControl.CursorMode.Move);
            }
            else if (PermittedAttack(mouseMapPos))
            {
                _cursorControl.SetCursor(CursorControl.CursorMode.Attack);
            }
            else if (GetBattleUnitAtGridPosition(mouseMapPos) != null)
            {
                _cursorControl.SetCursor(CursorControl.CursorMode.Hint);//
            }
            else if (BattleGrid.TraversableCells.Contains(mouseMapPos) || IsObstacleAt(mouseMapPos))
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
                if (GetBattleUnitAtGridPosition(mouseMapPos) != null)
                {
                    _cursorControl.SetCursor(CursorControl.CursorMode.Hint);//
                }
                else
                {
                    _cursorControl.SetCursor(CursorControl.CursorMode.Invalid);
                }
            }
        }
        else if (_currentSelectedAction == ActionMode.Spell1 || _currentSelectedAction == ActionMode.Spell2)
        {
            if (PermittedSpell(
                _currentSelectedAction == ActionMode.Spell1 
                    ? GetActiveBattleUnit().CurrentBattleUnitData.Spell1
                    : GetActiveBattleUnit().CurrentBattleUnitData.Spell2,
                mouseMapPos))
            {
                _cursorControl.SetCursor(CursorControl.CursorMode.Spell);
            }
            else if (GetBattleUnitAtGridPosition(mouseMapPos) != null)
            {
                _cursorControl.SetCursor(CursorControl.CursorMode.Hint);//
            }
            else
            {
                _cursorControl.SetCursor(CursorControl.CursorMode.Invalid);
            }
        }
        else
        {
            _cursorControl.SetCursor(CursorControl.CursorMode.Select);
        }
    }

    private void HighlightMoveableSquares()
    {
        BattleGrid.GetNode<TileMap>("TileMapShadedTiles").Clear();
        BattleGrid.GetNode<TileMap>("TileMapShadedTilesLong").Clear();
        
        if (!GetActiveBattleUnit().CurrentBattleUnitData.PlayerFaction)
        {
            return;
        }
        foreach (Vector2 cell in BattleGrid.TraversableCells)
        {
            if (PermittedMove(BattleGrid.GetCorrectedGridPosition(GetActiveBattleUnit().GlobalPosition), cell))
            {
                float abilAPCost = GetUnitSpeed(GetActiveBattleUnit())/2f;

                if ( BattleGrid.GetDistanceToPoint(BattleGrid.GetCorrectedGridPosition(GetActiveBattleUnit().GlobalPosition), cell)
                    > GetUnitAP(GetActiveBattleUnit()) - abilAPCost)
                {
                    BattleGrid.GetNode<TileMap>("TileMapShadedTilesLong").SetCellv(cell, 6);
                }
                else
                {
                    BattleGrid.GetNode<TileMap>("TileMapShadedTiles").SetCellv(cell, 4);
                }
            }
        }
    }

    public bool PermittedMove(Vector2 originMapPos, Vector2 targetMapPos)
    {
        int distance = BattleGrid.GetDistanceToPoint(originMapPos, targetMapPos);
        float currentAP = GetUnitAP(GetActiveBattleUnit());
        if (currentAP >= distance && distance > 0 && TargetWorldPosIsFree(targetMapPos))
        {
            return true;
        }
        return false;
    }

    private bool IsObstacleAt(Vector2 targetMapPos)
    {
        return BattleGrid.ObstacleCells.Contains(targetMapPos);
    }

    public float GetUnitAP(BattleUnit battleUnit)
    {
        return battleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.CurrentAP];
    }
    public float GetUnitSpeed(BattleUnit battleUnit)
    {
        return battleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Speed];
    }

    private bool TargetWorldPosIsFree(Vector2 targetMapPos)
    {
        foreach (BattleUnit battleUnit in GetBattleUnits())
        {
            if (BattleGrid.GetCorrectedGridPosition(battleUnit.GlobalPosition) == targetMapPos)
            {
                return false;
            }
        }
        return true;
    }

    private void SetNonActiveBattleUnitOutlines()
    {
        Vector2 mouseCentrePos = BattleGrid.GetCorrectedGridPosition(GetGlobalMousePosition());
        // GD.Print(mouseCentrePos);
        foreach (BattleUnit battleUnit in GetBattleUnits())
        {
            if (battleUnit == GetActiveBattleUnit())
            {
                continue;
            }
            Vector2 battleUnitCentrePos = BattleGrid.GetCorrectedGridPosition(battleUnit.GlobalPosition);
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
            OnNewRound();
        }
    }

    private void OnNewRound()
    {
        // _roundEnd = true;
        _round += 1;
        _battleHUD.LogEntry(String.Format("Round {0}!", _round));
        UpdateSpellDurationsAndRegen();
        PopulateTurnListByInitiative();
        UpdateAllBattleUnitsHealthManaFactionBars();
    }

    private void ApplyLeadershipBonus()
    {
        foreach (BattleUnit battleUnit in GetBattleUnits())
        {
            float leadership = battleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Leadership];
            float bonus = leadership/10f;
            
            List<BattleUnit> surroundingBattleUnits = GetBattleUnitsAtArea(1, BattleGrid.GetCorrectedGridPosition(battleUnit.GlobalPosition));
            foreach (BattleUnit surroundingUnit in surroundingBattleUnits)
            {
                if (surroundingUnit == battleUnit)
                {
                    continue;
                }
                if (surroundingUnit.CurrentBattleUnitData.PlayerFaction != battleUnit.CurrentBattleUnitData.PlayerFaction)
                {
                    continue;
                }
                
                if (surroundingUnit.CurrentBattleUnitData.CurrentStatusEffects.ContainsKey(SpellEffectManager.SpellMode.LeadershipBonus))
                {
                    continue;
                }
                foreach (BattleUnitData.DerivedStat stat in CurrentSpellEffectManager.SpellEffects[SpellEffectManager.SpellMode.LeadershipBonus][0].TargetStats)
                {
                    surroundingUnit.CurrentBattleUnitData.Stats[stat] = surroundingUnit.CurrentBattleUnitData.Stats[stat] + bonus;
                }
                surroundingUnit.CurrentBattleUnitData.CurrentStatusEffects.Add(SpellEffectManager.SpellMode.LeadershipBonus,
                    new Tuple<int, float>(1, bonus));
            }
        }
    }
    
    private async void UpdateSpellDurationsAndRegen()
    {
        foreach (BattleUnit battleUnit in GetBattleUnits())
        {
            foreach (SpellEffectManager.SpellMode spell in battleUnit.CurrentBattleUnitData.CurrentStatusEffects.Keys.ToList())
            {
                if (spell == SpellEffectManager.SpellMode.PerilOfOsiris) // this is bad but its the only dot in the game so...
                {
                    battleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Health] += battleUnit.CurrentBattleUnitData.CurrentStatusEffects[spell].Item2;
                    // i am here -- implement player death if health drop low
                    if (battleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Health] < 0.1f)
                    {
                        _turnList.Remove(battleUnit);
                    }
                    _endTurnEffectsInProgress = true;
                    battleUnit.SetActionState(battleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Health] < 0.1f 
                        ? BattleUnit.ActionStateMode.Dying : BattleUnit.ActionStateMode.Hit);
                    await ToSignal(battleUnit, nameof(BattleUnit.CurrentActionCompleted));
                }
                battleUnit.CurrentBattleUnitData.CurrentStatusEffects[spell] = new Tuple<int, float>(
                    battleUnit.CurrentBattleUnitData.CurrentStatusEffects[spell].Item1 - 1,
                    battleUnit.CurrentBattleUnitData.CurrentStatusEffects[spell].Item2
                );
                if (battleUnit.CurrentBattleUnitData.CurrentStatusEffects[spell].Item1 == 0)
                {
                    CurrentSpellEffectManager.ReverseEffect(battleUnit, spell, battleUnit.CurrentBattleUnitData.CurrentStatusEffects[spell].Item2);
                    battleUnit.CurrentBattleUnitData.CurrentStatusEffects.Remove(spell);
                }
            }
            if (!battleUnit.Dead)
            {
                battleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Mana] = 
                    Math.Min(battleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Mana] + 
                        battleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.ManaRegen],
                        battleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.TotalMana]);
                battleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Health] = 
                    Math.Min(battleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Health] + 
                        battleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.HealthRegen],
                        battleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.TotalHealth]);
                battleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.CurrentAP] = battleUnit.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Speed];
            }
        }
        ApplyLeadershipBonus();
        // if (!AreAllUnitsIdle())
        // {
        //     foreach (BattleUnit battleUnit in GetBattleUnits())
        //     {
        //         if (battleUnit.CurrentActionStateMode != BattleUnit.ActionStateMode.Idle)
        //         {
        //             await ToSignal(battleUnit, nameof(BattleUnit.CurrentActionCompleted));
        //         }
        //     }
        // }

        if (_endTurnEffectsInProgress)
        {
            OnActiveBattleUnitTurnStart();
            _endTurnEffectsInProgress = false;
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

    public void OnBtnQuitPressed()
    {
        SetPhysicsProcess(false);
        SetProcessInput(false);
        EmitSignal(nameof(BattleEnded), true, false,  new BattleUnitDataSignalWrapper() { CurrentBattleUnitData = _enemyCommanderData });
    }

    public void Die()    // MUST CALL THIS WHEN FREEING THE PARENT SCENE (E.G. QUITTING TO MENU)
    {
        _pnlPotion.Die();
    }
}
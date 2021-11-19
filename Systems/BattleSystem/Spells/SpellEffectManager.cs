using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// *Khepri: sun theme. Average stats.*
// - "Solar Bolt": do x magical damage to target within 5 squares. Cheap
// - "Solar Blast": do x magical damage to group of targets at and around radius of 4 squares. Range 4. Friendly fire. Expensive.
// *Companion 1: helps allies in combat, and does mainly melee damage. High health but slow.*
// - "Coming Forth By Day": increase the Vigour of target ally by x. Cheap
// - "Preservation": increase the Resilience of target ally by x. Cheap
// *Companion 2: control specialist. Fast but does not do much damage.*
// - "Weighing of the Heart": reduce the Swiftness of target enemy by x. Cheap
// - "Gaze of the Dead": does x magical damage and target loses next turn. Expensive
// *Companion 3: trickery. Fast. Low physical damage and low health. High resilience.*
// - "Teleport": move self to square B. Cheap.
// - "Lunar bolt": bolt that does x damage to enemy target, and restores health of nearby ally for half the damage. Expensive.
// *Companion 4: assassin. Fast. High damage. Low health and resilience.*
// - "Hymn of the underworld": increase self Vigour by large amount, and teleport behind target enemy and do melee attack. Short range. Cheap.
// - "Peril of Osiris": throw an enchanted poison dart at the opponent, doing x damage every round for 3 rounds. Medium range. Expensive.

    // public enum TargetMode { Hostile, Ally, Area}
    // public int RangeSquares = 1;
    // public int DurationRounds = 0;
    // public float Magnitude = 5f;
    // public int AreaSquares = 1;
    // public float ManaCost = 5f;
    // public BattleUnitData.DerivedStat TargetStat = BattleUnitData.DerivedStat.Health;
    // public TargetMode Target = TargetMode.Hostile;
public class SpellEffectManager : Reference
{
    [Signal]
    public delegate void SpellEffectFinished(string announceText, BattleUnit target, List<BattleUnit> unitsAtArea);

    [Signal]
    public delegate void MultiSpellEffectFinished();
    [Signal]
    public delegate void AnnouncingSpell(string announceText);

    private BattleGrid _battleGrid;
    public int AnimSpeed {get; set;} = 2;
    private int _effectsOngoing = 0;
    private Random _rand = new Random();
    private BattleInteractionHandler _battleInteractionHandler;
    private Node2D _spellEffectContainer;

    public enum SpellMode { SolarBolt, SolarBlast, ComingForthByDay, Preservation, WeighingOfTheHeart, GazeOfTheDead, Teleport, LunarBlast, HymnOfTheUnderworld, PerilOfOsiris,
        CharismaPotion, HealthPotion, IntellectPotion, LuckPotion, ManaPotion, ResiliencePotion, SwiftnessPotion, VigourPotion, LeadershipBonus, Empty }

    public Dictionary<SpellMode, List<SpellEffect>> SpellEffects;
    public Dictionary<SpellMode, Action<BattleUnit,BattleUnit, List<BattleUnit>, Vector2>> SpellMethods;
    private Tween _currentMissileMoveTween;

    public SpellMode GetRandomSpell()
    {
        return (SpellMode) _rand.Next(0,10);
    }

    public SpellEffectManager()
    {
        // GD.Print("empty");
        SetSpellEffects();
    }
    private void SetSpellEffects()
    {
        SpellEffects = new Dictionary<SpellMode, List<SpellEffect>> {
            {SpellMode.Empty, new List<SpellEffect>(){ new SpellEffect(){
                Name = "Empty Spell Slot",
                RangeSquares = 5,
                DurationRounds = 0,
                Magnitude = 2,
                AreaSquares = 0,
                ManaCost = 3f,
                TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.Health},
                Target = SpellEffect.TargetMode.Hostile,
                ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/SolarBoltEffect.tscn"),
                IconTex = GD.Load<Texture>("res://Interface/Cursors/Art/Hint.PNG"),
                ToolTip = "This character has not learned a spell for this spell slot."
            }}
            },
            {SpellMode.SolarBolt, new List<SpellEffect>(){ new SpellEffect(){
                Name = "Solar Bolt",
                RangeSquares = 5,
                DurationRounds = 0,
                Magnitude = 2,
                AreaSquares = 0,
                ManaCost = 3f,
                TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.Health},
                Target = SpellEffect.TargetMode.Hostile,
                ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/SolarBoltEffect.tscn"),
                IconTex = GD.Load<Texture>("res://Interface/Cursors/Art/Hint.PNG"),
                ToolTip = "Launch a bolt imbued with sunlight at the target! Range 5, Cost 3."
            }}
            },
            {SpellMode.SolarBlast, new List<SpellEffect>(){ new SpellEffect(){
                Name = "Solar Blast",
                RangeSquares = 4,
                DurationRounds = 0,
                Magnitude = 2,
                AreaSquares = 1,
                ManaCost = 8f,
                TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.Health},
                Target = SpellEffect.TargetMode.Area,
                ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/SolarBlastEffect.tscn"),
                IconTex = GD.Load<Texture>("res://Interface/Cursors/Art/Hint.PNG"),
                ToolTip = "Launch a monstrous blast of sunlight at the target area! Can hit allies! Range 4, Cost 8, Area 9."
            }}
            },
            {SpellMode.ComingForthByDay, new List<SpellEffect>(){ new SpellEffect(){
                Name = "Coming Forth By Day",
                RangeSquares = 20,
                DurationRounds = 3,
                Magnitude = 6,
                AreaSquares = 0,
                ManaCost = 2f,
                TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.TotalHealth, BattleUnitData.DerivedStat.Health, BattleUnitData.DerivedStat.PhysicalDamage},
                Target = SpellEffect.TargetMode.Ally,
                ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn"),
                IconTex = GD.Load<Texture>("res://Interface/Cursors/Art/Hint.PNG"),
                ToolTip = "Boost the vigour of an ally for 3 rounds! Cost 2."
            }}
            },
            {SpellMode.Preservation, new List<SpellEffect>(){ new SpellEffect(){
                Name = "Preservation",
                RangeSquares = 20,
                DurationRounds = 3,
                Magnitude = 1,
                AreaSquares = 0,
                ManaCost = 2f,
                TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.HealthRegen, BattleUnitData.DerivedStat.ManaRegen, BattleUnitData.DerivedStat.MagicResist},
                Target = SpellEffect.TargetMode.Ally,
                ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn"),
                IconTex = GD.Load<Texture>("res://Interface/Cursors/Art/Hint.PNG"),
                ToolTip = "Boost the resilience of an ally for 3 rounds! Cost 2."
            }}
            },
            {SpellMode.WeighingOfTheHeart, new List<SpellEffect>(){ new SpellEffect(){
                Name = "Weighing of the Heart",
                RangeSquares = 20,
                DurationRounds = 3,
                Magnitude = -2,
                AreaSquares = 0,
                ManaCost = 2f,
                TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.Dodge, BattleUnitData.DerivedStat.Speed, BattleUnitData.DerivedStat.Initiative, BattleUnitData.DerivedStat.CurrentAP},
                Target = SpellEffect.TargetMode.Hostile,
                ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn"),
                IconTex = GD.Load<Texture>("res://Interface/Cursors/Art/Hint.PNG"),
                ToolTip = "Reduce the swiftness of a foe for 3 rounds! Cost 2."
            }}
            },
            {SpellMode.GazeOfTheDead, new List<SpellEffect>(){ // duration effects first
                new SpellEffect() {
                    Name = "Gaze of the Dead",
                    RangeSquares = 3,
                    DurationRounds = 2,
                    Magnitude = -20000,
                    AreaSquares = 0,
                    ManaCost = 4f,
                    TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.Speed, BattleUnitData.DerivedStat.Initiative, BattleUnitData.DerivedStat.CurrentAP},
                    Target = SpellEffect.TargetMode.Hostile,
                    ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn"),
                    IconTex = GD.Load<Texture>("res://Interface/Cursors/Art/Hint.PNG"),
                    ToolTip = "Paralyse a foe, dealing damage and causing them to lose their next turn! Range 3, Cost 4."
                },
                new SpellEffect(){
                    Name = "Gaze of the Dead",
                    RangeSquares = 3,
                    DurationRounds = 0,
                    Magnitude = 3,
                    AreaSquares = 0,
                    ManaCost = 4f,
                    TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.Health},
                    Target = SpellEffect.TargetMode.Hostile,
                    ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/SolarBoltEffect.tscn")
                }}
            },
            {SpellMode.Teleport, new List<SpellEffect>(){ new SpellEffect(){
                Name = "Teleport",
                RangeSquares = 20,
                DurationRounds = 0,
                Magnitude = 0,
                AreaSquares = 0,
                ManaCost = 2f,
                TargetStats = new List<BattleUnitData.DerivedStat> {},//BattleUnitData.DerivedStat.Dodge, BattleUnitData.DerivedStat.Speed, BattleUnitData.DerivedStat.Initiative, BattleUnitData.DerivedStat.CurrentAP},
                Target = SpellEffect.TargetMode.Empty,
                ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn"),
                IconTex = GD.Load<Texture>("res://Interface/Cursors/Art/Hint.PNG"),
                ToolTip = "Teleport to an unoccupied space on the battlefield. Cost 2."
            }}
            },
            {SpellMode.LunarBlast, new List<SpellEffect>(){ // duration effects first
                new SpellEffect() {
                    Name = "Lunar Blast",
                    RangeSquares = 6,
                    DurationRounds = 0,
                    Magnitude = 6,
                    AreaSquares = 1,
                    ManaCost = 3f,
                    TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.Health},
                    Target = SpellEffect.TargetMode.Area,
                    ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/SolarBlastEffect.tscn"),
                IconTex = GD.Load<Texture>("res://Interface/Cursors/Art/Hint.PNG"),
                ToolTip = "Launch a bolt imbued with moonlight at the target area! Damages foes and heals allies! Range 6, Cost 6, Area 9."
                },
                new SpellEffect(){
                    Name = "Lunar Blast",
                    RangeSquares = 6,
                    DurationRounds = 0,
                    Magnitude = 3,
                    AreaSquares = 1,
                    ManaCost = 3f,
                    TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.Health},
                    Target = SpellEffect.TargetMode.Area,
                    ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn")
                }}
            },
            {SpellMode.HymnOfTheUnderworld, new List<SpellEffect>(){ 
                new SpellEffect(){ // teleport
                    Name = "Hymn of the Underworld",
                    RangeSquares = 6,
                    DurationRounds = 0,
                    Magnitude = 0,
                    AreaSquares = 1,
                    ManaCost = 2f,
                    TargetStats = new List<BattleUnitData.DerivedStat> {},//BattleUnitData.DerivedStat.Dodge, BattleUnitData.DerivedStat.Speed, BattleUnitData.DerivedStat.Initiative, BattleUnitData.DerivedStat.CurrentAP},
                    Target = SpellEffect.TargetMode.Empty,
                    ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn"),
                    IconTex = GD.Load<Texture>("res://Interface/Cursors/Art/Hint.PNG"),
                    ToolTip = "Teleport to the target unoccupied space, dealing damage around you before returning! Range 6, Cost 8, Area 8."
                },
                new SpellEffect(){ // do damage
                    Name = "Hymn of the Underworld",
                    RangeSquares = 6,
                    DurationRounds = 0,
                    Magnitude = 5,
                    AreaSquares = 1,
                    ManaCost = 4f,
                    TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.Health},
                    Target = SpellEffect.TargetMode.Empty,
                    ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/SolarBlastEffect.tscn")
                },
                new SpellEffect(){ // teleport back
                    Name = "Hymn of the Underworld",
                    RangeSquares = 6,
                    DurationRounds = 0,
                    Magnitude = 0,
                    AreaSquares = 0,
                    ManaCost = 2f,
                    TargetStats = new List<BattleUnitData.DerivedStat> {},//BattleUnitData.DerivedStat.Dodge, BattleUnitData.DerivedStat.Speed, BattleUnitData.DerivedStat.Initiative, BattleUnitData.DerivedStat.CurrentAP},
                    Target = SpellEffect.TargetMode.Empty,
                    ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn")
                },
            }
            },
            {SpellMode.PerilOfOsiris, new List<SpellEffect>(){ 
                new SpellEffect(){
                Name = "Peril of Osiris",
                RangeSquares = 5,
                DurationRounds = 0,
                Magnitude = 0,
                AreaSquares = 0,
                ManaCost = 3f,
                TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.Health},
                Target = SpellEffect.TargetMode.Hostile,
                ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/SolarBoltEffect.tscn"),
                IconTex = GD.Load<Texture>("res://Interface/Cursors/Art/Hint.PNG"),
                ToolTip = "Throw an enchanted poison dart at the opponent, dealing damage over 3 rounds! Range 5, Cost 6."
            }, 
                new SpellEffect(){
                Name = "Peril of Osiris",
                RangeSquares = 5,
                DurationRounds = 3,
                Magnitude = 0,
                AreaSquares = 0,
                ManaCost = 3f,
                TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.Health},
                Target = SpellEffect.TargetMode.Hostile,
                ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn")
            },}
            },
            {SpellMode.CharismaPotion, new List<SpellEffect>(){ new SpellEffect(){
                Name = "Charisma Potion",
                RangeSquares = 0,
                DurationRounds = 3,
                Magnitude = 0,
                AreaSquares = 0,
                ManaCost = 0f,
                TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.Leadership},//, BattleUnitData.DerivedStat.Speed, BattleUnitData.DerivedStat.Initiative, BattleUnitData.DerivedStat.CurrentAP},
                Target = SpellEffect.TargetMode.Ally,
                ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn")
            }}
            },
            {SpellMode.HealthPotion, new List<SpellEffect>(){ new SpellEffect(){
                Name = "Health Potion",
                RangeSquares = 0,
                DurationRounds = 0,
                Magnitude = 0,
                AreaSquares = 0,
                ManaCost = 0f,
                TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.Health},//, BattleUnitData.DerivedStat.Speed, BattleUnitData.DerivedStat.Initiative, BattleUnitData.DerivedStat.CurrentAP},
                Target = SpellEffect.TargetMode.Ally,
                ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn")
            }}
            },
            {SpellMode.IntellectPotion, new List<SpellEffect>(){ new SpellEffect(){
                Name = "Intellect Potion",
                RangeSquares = 0,
                DurationRounds = 3,
                Magnitude = 0,
                AreaSquares = 0,
                ManaCost = 0f,
                TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.TotalMana, BattleUnitData.DerivedStat.SpellDamage, BattleUnitData.DerivedStat.ManaRegen},//, BattleUnitData.DerivedStat.Speed, BattleUnitData.DerivedStat.Initiative, BattleUnitData.DerivedStat.CurrentAP},
                Target = SpellEffect.TargetMode.Ally,
                ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn")
            }}
            },
            {SpellMode.LuckPotion, new List<SpellEffect>(){ new SpellEffect(){
                Name = "Luck Potion",
                RangeSquares = 0,
                DurationRounds = 3,
                Magnitude = 0,
                AreaSquares = 0,
                ManaCost = 0f,
                TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.CriticalChance,BattleUnitData.DerivedStat.Dodge,},//, BattleUnitData.DerivedStat.Speed, BattleUnitData.DerivedStat.Initiative, BattleUnitData.DerivedStat.CurrentAP},
                Target = SpellEffect.TargetMode.Ally,
                ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn")
            }}
            },
            {SpellMode.ManaPotion, new List<SpellEffect>(){ new SpellEffect(){
                Name = "Mana Potion",
                RangeSquares = 0,
                DurationRounds = 0,
                Magnitude = 0,
                AreaSquares = 0,
                ManaCost = 0f,
                TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.Mana},//, BattleUnitData.DerivedStat.Speed, BattleUnitData.DerivedStat.Initiative, BattleUnitData.DerivedStat.CurrentAP},
                Target = SpellEffect.TargetMode.Ally,
                ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn")
            }}
            },
            {SpellMode.ResiliencePotion, new List<SpellEffect>(){ new SpellEffect(){
                Name = "Resilience Potion",
                RangeSquares = 0,
                DurationRounds = 3,
                Magnitude = 0,
                AreaSquares = 0,
                ManaCost = 0f,
                TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.HealthRegen,BattleUnitData.DerivedStat.ManaRegen,BattleUnitData.DerivedStat.MagicResist},//, BattleUnitData.DerivedStat.Speed, BattleUnitData.DerivedStat.Initiative, BattleUnitData.DerivedStat.CurrentAP},
                Target = SpellEffect.TargetMode.Ally,
                ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn")
            }}
            },
            {SpellMode.SwiftnessPotion, new List<SpellEffect>(){ new SpellEffect(){
                Name = "Swiftness Potion",
                RangeSquares = 0,
                DurationRounds = 3,
                Magnitude = 0,
                AreaSquares = 0,
                ManaCost = 0f,
                TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.Dodge,BattleUnitData.DerivedStat.Speed,BattleUnitData.DerivedStat.Initiative },//, BattleUnitData.DerivedStat.Speed, BattleUnitData.DerivedStat.Initiative, BattleUnitData.DerivedStat.CurrentAP},
                Target = SpellEffect.TargetMode.Ally,
                ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn")
            }}
            },
            {SpellMode.VigourPotion, new List<SpellEffect>(){ new SpellEffect(){
                Name = "Vigour Potion",
                RangeSquares = 0,
                DurationRounds = 3,
                Magnitude = 0,
                AreaSquares = 0,
                ManaCost = 0f,
                TargetStats = new List<BattleUnitData.DerivedStat> {BattleUnitData.DerivedStat.PhysicalDamage,BattleUnitData.DerivedStat.TotalHealth},//, BattleUnitData.DerivedStat.Speed, BattleUnitData.DerivedStat.Initiative, BattleUnitData.DerivedStat.CurrentAP},
                Target = SpellEffect.TargetMode.Ally,
                ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn")
            }}
            },
            {SpellMode.LeadershipBonus, new List<SpellEffect>(){ new SpellEffect(){
                Name = "Leadership Bonus",
                RangeSquares = 0,
                DurationRounds = 1,
                Magnitude = 0,
                AreaSquares = 0,
                ManaCost = 0f,
                TargetStats = new List<BattleUnitData.DerivedStat> {
                    BattleUnitData.DerivedStat.CriticalChance,
                    BattleUnitData.DerivedStat.Dodge,
                    BattleUnitData.DerivedStat.Initiative,
                    BattleUnitData.DerivedStat.HealthRegen,
                    // BattleUnitData.DerivedStat.Leadership,
                    BattleUnitData.DerivedStat.MagicResist,
                    BattleUnitData.DerivedStat.ManaRegen,
                    BattleUnitData.DerivedStat.PhysicalDamage,
                    BattleUnitData.DerivedStat.PhysicalResist,
                    // BattleUnitData.DerivedStat.Speed,
                    BattleUnitData.DerivedStat.SpellDamage,
                    // BattleUnitData.DerivedStat.TotalHealth,
                    // BattleUnitData.DerivedStat.TotalMana
                    },
                Target = SpellEffect.TargetMode.Ally,
                ArtEffectScn = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn")
            }}
            },
        };
    }
    public SpellEffectManager(BattleInteractionHandler battleInteractionHandler, Node2D spellEffectContainer, BattleGrid battleGrid)
    {
        _battleInteractionHandler = battleInteractionHandler;
        _spellEffectContainer = spellEffectContainer;
        _battleGrid = battleGrid;
        SetSpellEffects();

        SpellMethods = new Dictionary<SpellMode, Action<BattleUnit,BattleUnit, List<BattleUnit>, Vector2>> {
            {SpellMode.SolarBolt, SolarBolt},
            {SpellMode.SolarBlast, SolarBlast},
            {SpellMode.ComingForthByDay, ComingForthByDay},
            {SpellMode.Preservation, Preservation},
            {SpellMode.WeighingOfTheHeart, WeighingOfTheHeart},
            {SpellMode.GazeOfTheDead, GazeOfTheDead},
            {SpellMode.Teleport, Teleport},
            {SpellMode.LunarBlast, LunarBlast},
            {SpellMode.HymnOfTheUnderworld, HymnOfTheUnderworld},
            {SpellMode.PerilOfOsiris, PerilOfOsiris},
        };
    }


    public void SolarBolt(BattleUnit origin, BattleUnit target, List<BattleUnit> unitsAtArea, Vector2 targetWorldPos)
    {
        SpellMissile(origin, target, unitsAtArea, targetWorldPos, SpellMode.SolarBolt, SpellEffects[SpellMode.SolarBolt][0]);
    }
    public void ComingForthByDay(BattleUnit origin, BattleUnit target, List<BattleUnit> unitsAtArea, Vector2 targetWorldPos)
    {
        ApplyBuffDebuff(origin, target, unitsAtArea, targetWorldPos, SpellMode.ComingForthByDay, SpellEffects[SpellMode.ComingForthByDay][0]);
    }
    public void Preservation(BattleUnit origin, BattleUnit target, List<BattleUnit> unitsAtArea, Vector2 targetWorldPos)
    {
        ApplyBuffDebuff(origin, target, unitsAtArea, targetWorldPos, SpellMode.Preservation, SpellEffects[SpellMode.Preservation][0]);
    }
    public void WeighingOfTheHeart(BattleUnit origin, BattleUnit target, List<BattleUnit> unitsAtArea, Vector2 targetWorldPos)
    {
        ApplyBuffDebuff(origin, target, unitsAtArea, targetWorldPos, SpellMode.WeighingOfTheHeart, SpellEffects[SpellMode.WeighingOfTheHeart][0]);
    }
    public async void GazeOfTheDead(BattleUnit origin, BattleUnit target, List<BattleUnit> unitsAtArea, Vector2 targetWorldPos)
    {
        string los = GetLineOfSightPenalty(origin.GlobalPosition, target.GlobalPosition) != 1 ? " Reduced damage due to line of sight penalty." : "";
        EmitSignal(nameof(AnnouncingSpell), String.Format("{0} casts {2} on {1}. {1} is paralysed!{3}",
                origin.CurrentBattleUnitData.Name, target.CurrentBattleUnitData.Name, SpellEffects[SpellMode.GazeOfTheDead][0].Name, los));

        _effectsOngoing = 2;
        SpellMissile(origin, target, unitsAtArea, targetWorldPos, SpellMode.GazeOfTheDead, SpellEffects[SpellMode.GazeOfTheDead][1], multiEffect:true);
        ApplyBuffDebuff(origin, target, unitsAtArea, targetWorldPos, SpellMode.GazeOfTheDead, SpellEffects[SpellMode.GazeOfTheDead][0], multiEffect:true);
        
        await ToSignal(this, nameof(MultiSpellEffectFinished));
        foreach (SpellEffect effect in SpellEffects[SpellMode.LunarBlast])
        {
            origin.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Mana] -= effect.ManaCost;
        }
        origin.UpdateHealthManaBars();

        EmitSignal(nameof(SpellEffectFinished), 
            String.Format("{0} casts {2} on {1}. {1} is paralysed!",
                origin.CurrentBattleUnitData.Name, target.CurrentBattleUnitData.Name, SpellEffects[SpellMode.GazeOfTheDead][0].Name), 
            target, unitsAtArea);
    }

    public async void LunarBlast(BattleUnit origin, BattleUnit target, List<BattleUnit> unitsAtArea, Vector2 targetWorldPos)
    {

        string los = GetLineOfSightPenalty(origin.GlobalPosition, targetWorldPos) != 1 ? " Reduced damage due to line of sight penalty." : "";
        EmitSignal(nameof(AnnouncingSpell), String.Format("{0} casts {2}! {1} creatures are affected!{3}",
                origin.CurrentBattleUnitData.Name, unitsAtArea.Count, SpellEffects[SpellMode.LunarBlast][0].Name, los));
        // first do the missile effect
        SetToCastingState(origin, targetWorldPos);
        _effectsOngoing = 1;
        SpellArea(origin, target, unitsAtArea, targetWorldPos, SpellMode.LunarBlast, SpellEffects[SpellMode.LunarBlast][0], multiEffect:true, hostileOnly:true);
        await ToSignal(this, nameof(MultiSpellEffectFinished));

        foreach (SpellEffect effect in SpellEffects[SpellMode.LunarBlast])
        {
            origin.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Mana] -= effect.ManaCost;
        }
        origin.UpdateHealthManaBars();

        // then do the health buff
        foreach (BattleUnit unit in unitsAtArea)
        {
            if (unit.CurrentBattleUnitData.PlayerFaction == origin.CurrentBattleUnitData.PlayerFaction)
            {
                _effectsOngoing += 1;
            }
        }
        bool furtherCastingToHappen = _effectsOngoing > 0;
        foreach (BattleUnit unit in unitsAtArea)
        {
            if (unit.CurrentBattleUnitData.PlayerFaction == origin.CurrentBattleUnitData.PlayerFaction)
            {
                ApplyBuffDebuff(origin, unit, unitsAtArea, unit.GlobalPosition, SpellMode.LunarBlast, 
                    SpellEffects[SpellMode.LunarBlast][1], multiEffect:true);
            }
        }
        // if no creatures to heal, then don't need to wait        
        if (furtherCastingToHappen)
        {
            await ToSignal(this, nameof(MultiSpellEffectFinished));
        }

        // wait for caster to finish their animation
        if (origin.CurrentActionStateMode != BattleUnit.ActionStateMode.Idle)
        {
            await ToSignal(origin, nameof(BattleUnit.CurrentActionCompleted));
        }

        EmitSignal(nameof(SpellEffectFinished), 
            String.Format("{0} casts {2}! {1} creatures are affected!",
                origin.CurrentBattleUnitData.Name, unitsAtArea.Count, SpellEffects[SpellMode.LunarBlast][0].Name), 
            target, unitsAtArea);
    }
    public async void HymnOfTheUnderworld(BattleUnit origin, BattleUnit target, List<BattleUnit> unitsAtArea, Vector2 targetWorldPos)
    {
        string announceText =  String.Format("{0} casts {1}.{2}",
            origin.CurrentBattleUnitData.Name, SpellEffects[SpellMode.HymnOfTheUnderworld][0].Name,
            unitsAtArea.FindAll(x => x.CurrentBattleUnitData.PlayerFaction != origin.CurrentBattleUnitData.PlayerFaction).Count > 0 
                ? String.Format(" {0} creatures affected!", unitsAtArea.FindAll(x => x.CurrentBattleUnitData.PlayerFaction != origin.CurrentBattleUnitData.PlayerFaction).Count) : "");
        EmitSignal(nameof(AnnouncingSpell), announceText);
        // set the caster to Casting state
        SetToCastingState(origin, targetWorldPos);
        Vector2 startingPos = origin.GlobalPosition;

        // do the first effect
        // play the teleport animation
        PlayStaticAnim(SpellEffects[SpellMode.HymnOfTheUnderworld][0], startingPos);
        PlayStaticAnim(SpellEffects[SpellMode.HymnOfTheUnderworld][0], origin.GlobalPosition);
        origin.GlobalPosition = targetWorldPos;

        _effectsOngoing = 1;
        // do the second effect
        SpellArea(origin, target, unitsAtArea, targetWorldPos, SpellMode.HymnOfTheUnderworld, SpellEffects[SpellMode.HymnOfTheUnderworld][1], multiEffect:true, hostileOnly:true);
        
        AnimationPlayer anim = PlayStaticAnim(SpellEffects[SpellMode.HymnOfTheUnderworld][1], targetWorldPos);
        await ToSignal(anim, "animation_finished");
        if (_effectsOngoing > 0)
        {
            await ToSignal(this, nameof(MultiSpellEffectFinished));
        }

        // do the third effect
        // play the teleport animation
        PlayStaticAnim(SpellEffects[SpellMode.HymnOfTheUnderworld][2], targetWorldPos);
        PlayStaticAnim(SpellEffects[SpellMode.HymnOfTheUnderworld][2], startingPos);
        origin.GlobalPosition = startingPos;

        // wait for caster to finish their animation
        if (origin.CurrentActionStateMode != BattleUnit.ActionStateMode.Idle)
        {
            await ToSignal(origin, nameof(BattleUnit.CurrentActionCompleted));
        }

        // make some lines for the log to display
        // string announceText =  String.Format("{0} casts {1}.{2}",
        //     origin.CurrentBattleUnitData.Name, SpellEffects[SpellMode.HymnOfTheUnderworld][0].Name,
        //     unitsAtArea.FindAll(x => x.CurrentBattleUnitData.PlayerFaction != origin.CurrentBattleUnitData.PlayerFaction).Count > 0 
        //         ? String.Format(" {0} creatures affected!", unitsAtArea.FindAll(x => x.CurrentBattleUnitData.PlayerFaction != origin.CurrentBattleUnitData.PlayerFaction).Count) : "");
        
        foreach (SpellEffect effect in SpellEffects[SpellMode.PerilOfOsiris])
        {
            origin.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Mana] -= effect.ManaCost;
        }
        origin.UpdateHealthManaBars();
        EmitSignal(nameof(SpellEffectFinished), announceText, target, unitsAtArea);
    }

    // [Signal]
    // public delegate void StaticSpellAnimFinished();
    
    public async void PerilOfOsiris(BattleUnit origin, BattleUnit target, List<BattleUnit> unitsAtArea, Vector2 targetWorldPos)
    {
        EmitSignal(nameof(AnnouncingSpell), String.Format("{0} casts {2} on {1}. {1} is poisoned!",
                origin.CurrentBattleUnitData.Name, target.CurrentBattleUnitData.Name, SpellEffects[SpellMode.PerilOfOsiris][0].Name));
        // _effectsOngoing = 1;
        // SpellMissile(origin, target, unitsAtArea, targetWorldPos, SpellMode.PerilOfOsiris, SpellEffects[SpellMode.PerilOfOsiris][0], multiEffect:true);
        // await ToSignal(this, nameof(MultiSpellEffectFinished));
        // generate the missile and send to target
        GenerateMissile(SpellEffects[SpellMode.PerilOfOsiris][0], origin.GlobalPosition, target.GlobalPosition);
        await ToSignal(_currentMissileMoveTween, "tween_all_completed");
        _effectsOngoing = 1;
        ApplyBuffDebuff(origin, target, unitsAtArea, targetWorldPos, SpellMode.PerilOfOsiris, SpellEffects[SpellMode.PerilOfOsiris][1], multiEffect:true);
        // then set the target to hit state or dying state
        SetToHitOrDyingState(target, origin.GlobalPosition);
        await ToSignal(this, nameof(MultiSpellEffectFinished));

        // wait for both caster and target to finish their animations
        if (target.CurrentActionStateMode != BattleUnit.ActionStateMode.Idle)
        {
            await ToSignal(target, nameof(BattleUnit.CurrentActionCompleted));
        }
        // if (origin.CurrentActionStateMode != BattleUnit.ActionStateMode.Idle)
        // {
        //     await ToSignal(origin, nameof(BattleUnit.CurrentActionCompleted));
        // }

        foreach (SpellEffect effect in SpellEffects[SpellMode.PerilOfOsiris])
        {
            origin.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Mana] -= effect.ManaCost;
        }
        origin.UpdateHealthManaBars();

        EmitSignal(nameof(SpellEffectFinished), 
            String.Format("{0} casts {2} on {1}. {1} is poisoned!",
                origin.CurrentBattleUnitData.Name, target.CurrentBattleUnitData.Name, SpellEffects[SpellMode.PerilOfOsiris][0].Name), 
            target, unitsAtArea);

        // SpellMissile(origin, target, unitsAtArea, targetWorldPos, SpellMode.PerilOfOsiris, SpellEffects[SpellMode.PerilOfOsiris][0]);
    }

    public async void Teleport(BattleUnit origin, BattleUnit target, List<BattleUnit> unitsAtArea, Vector2 targetWorldPos)
    {
        string announceText =  String.Format("{0} casts {1}.",
            origin.CurrentBattleUnitData.Name, SpellEffects[SpellMode.Teleport][0].Name);
        EmitSignal(nameof(AnnouncingSpell), announceText);
        // set the caster to Casting state
        SetToCastingState(origin, targetWorldPos);

        // play the animation
        PlayStaticAnim(SpellEffects[SpellMode.Teleport][0], origin.GlobalPosition);
        // do the spell effect
        origin.GlobalPosition = targetWorldPos;
        AnimationPlayer anim = PlayStaticAnim(SpellEffects[SpellMode.Teleport][0], targetWorldPos);
        // update mana
        origin.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Mana] -= SpellEffects[SpellMode.Teleport][0].ManaCost;
        origin.UpdateHealthManaBars();
        await ToSignal(anim, "animation_finished");


        // wait for caster to finish their animation
        if (origin.CurrentActionStateMode != BattleUnit.ActionStateMode.Idle)
        {
            await ToSignal(origin, nameof(BattleUnit.CurrentActionCompleted));
        }

        // make some lines for the log to display
        
        EmitSignal(nameof(SpellEffectFinished), announceText, target, unitsAtArea);
    }

    public void SolarBlast(BattleUnit origin, BattleUnit target, List<BattleUnit> unitsAtArea, Vector2 targetWorldPos)
    {
        SpellArea(origin, target, unitsAtArea, targetWorldPos, SpellMode.SolarBlast, SpellEffects[SpellMode.SolarBlast][0]);
    }

    public async void SpellArea(BattleUnit origin, BattleUnit target, List<BattleUnit> unitsAtArea, Vector2 targetWorldPos, SpellMode spell, SpellEffect effect, bool multiEffect = false, bool hostileOnly = false)
    {
        // make some lines for the log to display
        string announceText = String.Format("{0} casts {2}, raining fury upon {1} creatures!",
            origin.CurrentBattleUnitData.Name, unitsAtArea.Count, effect.Name);
        string los = GetLineOfSightPenalty(origin.GlobalPosition, targetWorldPos) != 1 ? " Reduced damage due to line of sight penalty." : "";
        announceText += los;
        if (!multiEffect)
        {
            EmitSignal(nameof(AnnouncingSpell), announceText);
            // set the caster to Casting state
            SetToCastingState(origin, targetWorldPos);
        }

        // generate the missile and send to target
        GenerateMissile(effect, origin.GlobalPosition, targetWorldPos);
        await ToSignal(_currentMissileMoveTween, "tween_all_completed");
        
        // on missile reaching target, do damage calculation for each target
        foreach (BattleUnit tarBattleUnit in unitsAtArea)
        {
            if (hostileOnly)
            {
                if (tarBattleUnit.CurrentBattleUnitData.PlayerFaction == origin.CurrentBattleUnitData.PlayerFaction)
                {
                    continue;
                }
            }
            _battleInteractionHandler.CalculateSpell(effect, origin.CurrentBattleUnitData, tarBattleUnit.CurrentBattleUnitData, spell == SpellMode.HymnOfTheUnderworld ? 1 : GetLineOfSightPenalty(origin.GlobalPosition, targetWorldPos));

            
            
            tarBattleUnit.UpdateHealthManaBars();
            // then set the target to hit state or dying state
            SetToHitOrDyingState(tarBattleUnit, origin.GlobalPosition);
        }

        if (!multiEffect)
        {
            origin.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Mana] -= effect.ManaCost;
            origin.UpdateHealthManaBars();
        }
        // wait for both caster and target to finish their animations
        foreach (BattleUnit tarBattleUnit in unitsAtArea)
        {
            if (tarBattleUnit.CurrentActionStateMode != BattleUnit.ActionStateMode.Idle)
            {
                await ToSignal(tarBattleUnit, nameof(BattleUnit.CurrentActionCompleted));
            }
        }
        if (origin.CurrentActionStateMode != BattleUnit.ActionStateMode.Idle && !multiEffect)
        {
            await ToSignal(origin, nameof(BattleUnit.CurrentActionCompleted));
        }

            
        if (multiEffect)
        {
            _effectsOngoing -= 1;
            if (_effectsOngoing == 0)
            {
                EmitSignal(nameof(MultiSpellEffectFinished));
            }
        }
        else
        {
            EmitSignal(nameof(SpellEffectFinished), announceText, target, unitsAtArea);
        }
    }

    public float GetLineOfSightPenalty(Vector2 originWorldPos, Vector2 targetWorldPos)
    {
        if (_battleGrid.IsPathStraight(_battleGrid.GetCorrectedGridPosition(originWorldPos),
            _battleGrid.GetCorrectedGridPosition(targetWorldPos)))
        {
            return 1;
        }
        return 0.6f;
    }

    public async void SpellMissile(BattleUnit origin, BattleUnit target, List<BattleUnit> unitsAtArea, Vector2 targetWorldPos, SpellMode spell, SpellEffect effect, bool multiEffect = false)
    {
        // set the caster to Casting state
        SetToCastingState(origin, targetWorldPos);
        
        // generate the missile and send to target
        GenerateMissile(effect, origin.GlobalPosition, target.GlobalPosition);
        await ToSignal(_currentMissileMoveTween, "tween_all_completed");

        // on missile reaching target, do damage calculation in battleInteractionHandler
        float[] result = _battleInteractionHandler.CalculateSpell(effect, origin.CurrentBattleUnitData, target.CurrentBattleUnitData,
            GetLineOfSightPenalty(origin.GlobalPosition, target.GlobalPosition));

        // make some lines for the log to display
        string announceText =  String.Format("{0} takes {2} damage from {1}.{3}{4}{6}{5}",
            target.CurrentBattleUnitData.Name, origin.CurrentBattleUnitData.Name, Math.Round(result[2], 1),
            (result[0] == 2 ? " Double damage from critical hit!" : ""),
            (result[1] == 2 ? " Damage halved due to dodge!" : ""),
            target.Dead ? " " + target.CurrentBattleUnitData.Name + " perishes!" : "",
            GetLineOfSightPenalty(origin.GlobalPosition, target.GlobalPosition) != 1 ? " Reduced damage due to line of sight penalty." : "");

        if (!multiEffect)
        {
            EmitSignal(nameof(AnnouncingSpell), announceText);
            origin.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Mana] -= effect.ManaCost;
            origin.UpdateHealthManaBars();
        }
        origin.UpdateHealthManaBars();
        target.UpdateHealthManaBars();
        // then set the target to hit state or dying state
        SetToHitOrDyingState(target, origin.GlobalPosition);

        // wait for both caster and target to finish their animations
        if (target.CurrentActionStateMode != BattleUnit.ActionStateMode.Idle)
        {
            await ToSignal(target, nameof(BattleUnit.CurrentActionCompleted));
        }
        if (origin.CurrentActionStateMode != BattleUnit.ActionStateMode.Idle)
        {
            await ToSignal(origin, nameof(BattleUnit.CurrentActionCompleted));
        }


        // tell the CntBattle.cs we are finished and give it the text to pass to the hud
        // also it needs to know the target to remove from turnlist if it died
        if (multiEffect)
        {
            _effectsOngoing -= 1;
            if (_effectsOngoing == 0)
            {
                EmitSignal(nameof(MultiSpellEffectFinished));
            }
        }
        else
        {
            EmitSignal(nameof(SpellEffectFinished), announceText, target, unitsAtArea);
        }
    }

    private AnimationPlayer PlayStaticAnim(SpellEffect effect, Vector2 targetWorldPos)
    {
        Node2D art = (Node2D) effect.ArtEffectScn.Instance();
        _spellEffectContainer.AddChild(art);
        art.GlobalPosition = targetWorldPos;
        art.GetNode<AnimationPlayer>("Anim").Play("Die"); // also queue_frees
        return art.GetNode<AnimationPlayer>("Anim");
        // EmitSignal(nameof(StaticSpellAnimFinished));
    }

    public async void ApplyPotionEffect(BattleUnit origin, PotionEffect potionEffect) // the effect used is from above!
    {
        // make some lines for the log to display
        string announceText =  String.Format("{0} uses {1}!",
            origin.CurrentBattleUnitData.Name, potionEffect.Name);
        SpellEffect effect = SpellEffects[potionEffect.SpellEffect][0];
        // ApplyBuffDebuff(origin, origin, new List<BattleUnit>(), origin.GlobalPosition, potionEffect.SpellEffect, effect, multiEffect:true);
        EmitSignal(nameof(AnnouncingSpell), announceText);
        SetToCastingState(origin, origin.GlobalPosition);
        PlayStaticAnim(effect, origin.GlobalPosition);
        if (!origin.CurrentBattleUnitData.CurrentStatusEffects.ContainsKey(potionEffect.SpellEffect))
        {
            foreach (BattleUnitData.DerivedStat stat in effect.TargetStats)
            {
                origin.CurrentBattleUnitData.Stats[stat] = origin.CurrentBattleUnitData.Stats[stat] + potionEffect.Magnitude;// += finalMagnitude;
            }
            if (effect.DurationRounds > 0)
            {
                origin.CurrentBattleUnitData.CurrentStatusEffects.Add(potionEffect.SpellEffect, 
                    new Tuple<int,float>(effect.DurationRounds,potionEffect.Magnitude));
            }
        }
        origin.UpdateHealthManaBars();
        // wait for caster to finish their animation
        if (origin.CurrentActionStateMode != BattleUnit.ActionStateMode.Idle)
        {
            await ToSignal(origin, nameof(BattleUnit.CurrentActionCompleted));
        }


        EmitSignal(nameof(SpellEffectFinished), announceText, null, null);
    }

    public async void ApplyBuffDebuff(BattleUnit origin, BattleUnit target, List<BattleUnit> unitsAtArea, Vector2 targetWorldPos, SpellMode spell, SpellEffect effect, bool multiEffect = false)
    {
        // set the caster to Casting state
        SetToCastingState(origin, targetWorldPos);
        
        // make some lines for the log to display
        string announceText =  String.Format("{0} casts {2} on {1}.",
            origin.CurrentBattleUnitData.Name, target.CurrentBattleUnitData.Name, effect.Name);
        
        if (!multiEffect)
        {
            EmitSignal(nameof(AnnouncingSpell), announceText);
        }


        // play the animation
        PlayStaticAnim(effect, targetWorldPos);
        
        // do the spell effect
        if (!target.CurrentBattleUnitData.CurrentStatusEffects.ContainsKey(spell))
        {
            bool casterIsAlly = origin.CurrentBattleUnitData.PlayerFaction == target.CurrentBattleUnitData.PlayerFaction;
            float finalMagnitude = effect.Magnitude + 
                    ((float) Math.Floor(origin.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.SpellDamage]/2) 
                        * (casterIsAlly ? 1 : -1));
            // GD.Print("effect magnitude ", effect.Magnitude);
            // GD.Print("spelldamage help ", ((float) Math.Floor(origin.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.SpellDamage]/2) 
                        // * (casterIsAlly ? 1 : -1)));
            // GD.Print("finalMagnitude magnitude ", finalMagnitude);
            foreach (BattleUnitData.DerivedStat stat in effect.TargetStats)
            {
                target.CurrentBattleUnitData.Stats[stat] = target.CurrentBattleUnitData.Stats[stat] + finalMagnitude;// += finalMagnitude;
            }
            if (effect.DurationRounds > 0)
            {
                target.CurrentBattleUnitData.CurrentStatusEffects.Add(spell, 
                    new Tuple<int,float>(effect.DurationRounds,finalMagnitude));
            }
            
            target.UpdateHealthManaBars();
        }
        if (!multiEffect)
        {
            origin.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Mana] -= effect.ManaCost;
            origin.UpdateHealthManaBars();
        }
        // wait for caster to finish their animation
        if (origin.CurrentActionStateMode != BattleUnit.ActionStateMode.Idle)
        {
            await ToSignal(origin, nameof(BattleUnit.CurrentActionCompleted));
        }


        // tell the CntBattle.cs we are finished and give it the text to pass to the hud
        // also it needs to know the target to remove from turnlist if it died
        if (multiEffect)
        {
            _effectsOngoing -= 1;
            if (_effectsOngoing == 0)
            {
                EmitSignal(nameof(MultiSpellEffectFinished));
            }
        }
        else
        {
            EmitSignal(nameof(SpellEffectFinished), announceText, target, unitsAtArea);
        }

    }

    public void ReverseEffect(BattleUnit battleUnit, SpellMode spell, float magnitude)
    {
        foreach (BattleUnitData.DerivedStat stat in SpellEffects[spell][0].TargetStats)
        {
            // GD.Print(spell);
            // don't reverse health otherwise units will randomly die
            if (stat != BattleUnitData.DerivedStat.Health && stat != BattleUnitData.DerivedStat.Mana && stat != BattleUnitData.DerivedStat.CurrentAP)
            {
                battleUnit.CurrentBattleUnitData.Stats[stat] -= magnitude;
            }
        }
    }

    private void SetToCastingState(BattleUnit origin, Vector2 targetWorldPos)
    {
        // if (origin.GlobalPosition != targetWorldPos)
        // {
            origin.TargetWorldPos = targetWorldPos;
        // }
        origin.SetActionState(BattleUnit.ActionStateMode.Casting);
    }

    private void SetToHitOrDyingState(BattleUnit target, Vector2 originWorldPos)
    {
        target.TargetWorldPos = originWorldPos;
        target.SetActionState(target.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Health] < 0.1f 
                ? BattleUnit.ActionStateMode.Dying : BattleUnit.ActionStateMode.Hit);
    }

    private async void GenerateMissile(SpellEffect effect, Vector2 originPos, Vector2 targetPos)
    {
        // it is a missile spell, so generate the missile and move it towards the target
        Node2D missileArt = (Node2D) effect.ArtEffectScn.Instance();
        _spellEffectContainer.AddChild(missileArt);
        Tween moveTween = missileArt.GetNode<Tween>("MoveTween");
        missileArt.GlobalPosition = originPos;
        missileArt.LookAt(targetPos);
        float distance = originPos.DistanceTo(targetPos);
        float delta = missileArt.GetTree().Root.GetPhysicsProcessDeltaTime();
        float speed = 15f*AnimSpeed;
        moveTween.InterpolateProperty(missileArt, "global_position", null, targetPos, (distance/speed)*delta, Tween.TransitionType.Linear);
        moveTween.Start();
        _currentMissileMoveTween = moveTween;
        // kill the missile when it reaches the target
        await ToSignal(moveTween, "tween_all_completed");
        missileArt.GetNode<AnimationPlayer>("Anim").Play("Die"); // also queue_frees
    }
}

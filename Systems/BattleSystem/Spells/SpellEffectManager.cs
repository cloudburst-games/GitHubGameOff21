using Godot;
using System;
using System.Collections.Generic;

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
// - "Teleport": move target ally (including self) or enemy from square A to square B. Cheap.
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
    public delegate void SpellEffectFinished();
    public int AnimSpeed {get; set;} = 2;
    private BattleInteractionHandler _battleInteractionHandler;
    private Node2D _spellEffectContainer;

    public enum SpellMode { SolarBolt, SolarBlast, ComingForthByDay, Preservation, WeighingOfTheHeart, GazeOfTheDead,
        Teleport, LunarBolt, HymnOfTheUnderworld, PerilOfOsiris }

    public Dictionary<SpellMode, SpellEffect> SpellEffects;
    public Dictionary<SpellMode, Action<BattleUnit,BattleUnit>> SpellMethods;
    public Dictionary<SpellMode, PackedScene> SpellMissileArt = new Dictionary<SpellMode, PackedScene>() {
        {SpellMode.SolarBolt, GD.Load<PackedScene>("res://Effects/SpellEffects/SolarBoltEffect.tscn")}
    };

    public SpellEffectManager()
    {
        GD.Print("wronginitialiser -SpellEffectManager.cs");
        throw new Exception();
    }
    public SpellEffectManager(BattleInteractionHandler battleInteractionHandler, Node2D spellEffectContainer)
    {
        _battleInteractionHandler = battleInteractionHandler;
        _spellEffectContainer = spellEffectContainer;
        SpellEffects = new Dictionary<SpellMode, SpellEffect> {
            {SpellMode.SolarBolt, new SpellEffect() {
                RangeSquares = 5,
                DurationRounds = 0,
                Magnitude = 5,
                AreaSquares = 1,
                ManaCost = 5f,
                TargetStat = BattleUnitData.DerivedStat.Health,
                Target = SpellEffect.TargetMode.Hostile
            }},

        };
        SpellMethods = new Dictionary<SpellMode, Action<BattleUnit,BattleUnit>> {
            {SpellMode.SolarBolt, SolarBolt}
        };
    }
    public async void SolarBolt(BattleUnit origin, BattleUnit target)
    {

        // do spell particle effect (missile)
        // GD.Print("booom");
        Node2D missileArt = (Node2D) SpellMissileArt[SpellMode.SolarBolt].Instance();
        _spellEffectContainer.AddChild(missileArt);
        Tween moveTween = missileArt.GetNode<Tween>("MoveTween");
        missileArt.GlobalPosition = origin.GlobalPosition;
        missileArt.LookAt(target.GlobalPosition);
        float distance = origin.GlobalPosition.DistanceTo(target.GlobalPosition);
        float delta = missileArt.GetTree().Root.GetPhysicsProcessDeltaTime();
        float speed = 15f*AnimSpeed;
        moveTween.InterpolateProperty(missileArt, "global_position", null, target.GlobalPosition, (distance/speed)*delta, Tween.TransitionType.Linear);
        moveTween.Start();

        await ToSignal(moveTween, "tween_all_completed");

        // on missile reaching target, do damage calculation in battleInteractionHandler

        // hit or dying state for target

        // done
        EmitSignal(nameof(SpellEffectFinished));
        GD.Print("SpellEffectFinished turn signal emitted");
    }
}

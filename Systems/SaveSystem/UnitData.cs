using Godot;
using System;
using System.Collections.Generic;

[Serializable()]
public class UnitData : IStoreable
{
    public string PortraitPath {get; set;} = "";
    public string PortraitPathSmall {get; set;} = "";
    public string ID {get; set;} = "";
    private string _name = "";
    public string Name {
        get {
            return _name;
        }
        set{
            _name = value;
            CurrentBattleUnitData.Name = value;
        }
    }
    public Vector2 NPCPosition {get; set;}
    private bool _companion = false;
    public bool Companion {
        get {
            return _companion;
        }
        set {
            _companion = value;
            if (_companion)
            {
                Behaviour = AIUnitControlState.AIBehaviour.Follow;
            }
        }
    }
    public bool Modified {get; set;} = false;
    public float PhysicalDamageRange {get; set;} = 3f;
    
    public enum Attribute { Vigour, Resilience, Intellect, Swiftness, Charisma, Luck}
    
    public Dictionary<Attribute, int> Attributes {get; set;} = new Dictionary<Attribute, int>()
    {
        {Attribute.Vigour, 0},
        {Attribute.Resilience, 0},
        {Attribute.Intellect, 0},
        {Attribute.Swiftness, 0},
        {Attribute.Charisma, 0},
        {Attribute.Luck, 0}
    };

    public int AttributePoints {get; set;} = 0;
    // public bool NPC {get; set;} = false;
    // public Dictionary<BattleUnitData.Attribute, int> GetAttributes()
    // {
    //     return MainCombatant.Attributes;
    // }
    // public void SetAttribute(BattleUnitData.Attribute attribute, int num)
    // {
    //     MainCombatant.Attributes[attribute] = num;
    // }

    public bool Hostile {get; set;} = false;
    public bool Player {get; set;} = false;
    public List<UnitData> Companions {get; set;} = new List<UnitData>();
    public AIUnitControlState.AIBehaviour Behaviour = AIUnitControlState.AIBehaviour.Stationary;
    public List<Vector2> PatrolPoints {get; set;} = new List<Vector2>();

    // change to battle unit Data
    public BattleUnitData CurrentBattleUnitData {get; set;} = new BattleUnitData();
    public List<BattleUnitData> Minions {get; set;} = new List<BattleUnitData>();
    // public Dictionary<BattleUnitData, int> Minions {get; set;} = new Dictionary<BattleUnitData, int>();

    //

    public ExperienceManager ExperienceManager = new ExperienceManager();
    public DialogueData CurrentDialogueData = new DialogueData();

    public void StopCompanion()
    {
        Companion = false;
        Behaviour = AIUnitControlState.AIBehaviour.Stationary;
    }

    // NEEDS BALANCING. consider applying similar formula that is used for armour, if flat increases don't work out. or maybe just for luck %
    public void UpdateDerivedStatsFromAttributes() // called pre-battle as well
    {
        UnitData unitData = this; // clean this up when get time
    // public Dictionary<DerivedStat, float> Stats {get; set;} = new Dictionary<DerivedStat, float>()
    // {
    //     {DerivedStat.Health, 10},
    //     {DerivedStat.TotalHealth, 10},
    //     {DerivedStat.Mana, 10},
    //     {DerivedStat.TotalMana, 10},
    //     {DerivedStat.HealthRegen, 1},
    //     {DerivedStat.ManaRegen, 1},
    //     {DerivedStat.MagicResist, 10},
    //     {DerivedStat.PhysicalResist, 10},
    //     {DerivedStat.Dodge, 5},
    //     {DerivedStat.PhysicalDamage, 5},
    //     {DerivedStat.PhysicalDamageRange, 3},
    //     {DerivedStat.SpellDamage, 5},
    //     {DerivedStat.Speed, 6},
    //     {DerivedStat.Initiative, 5},
    //     {DerivedStat.Leadership, 1},
    //     {DerivedStat.CriticalChance, 1},
    //     {DerivedStat.CurrentAP, 6},
    // };

        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Health] = unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.TotalHealth]
            = UpdateStat(unitData.Attributes[UnitData.Attribute.Vigour], 2f);
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.PhysicalDamage]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Vigour], 0.5f);// + GetWeaponDamage();

        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Mana] = unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.TotalMana]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Intellect], 2.5f);
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.ManaRegen]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Intellect], 0.25f);
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.SpellDamage]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Intellect], 0.75f);
        
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Dodge]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Swiftness], 0.2f);
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Speed] = unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.CurrentAP]
            = (float) Math.Floor(UpdateStat(unitData.Attributes[UnitData.Attribute.Swiftness], 0.6f));
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Initiative]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Swiftness], 1f);

        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Leadership]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Charisma], 1f);

        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.CriticalChance]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Luck], 0.3f);
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Dodge]
            *= Math.Max(1,UpdateStat(unitData.Attributes[UnitData.Attribute.Luck], 0.2f));

        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.HealthRegen]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Resilience], 0.2f);
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.MagicResist]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Resilience], 0.3f);
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.ManaRegen]
            *= Math.Max(1,UpdateStat(unitData.Attributes[UnitData.Attribute.Resilience], 0.2f));

        // GD.Print("\n" + unitData.Name + ": ");
        // if (unitData.ID == "khepri")
        // foreach (BattleUnitData.DerivedStat stat in unitData.CurrentBattleUnitData.Stats.Keys)
        // {
        //     GD.Print(stat + ": " + unitData.CurrentBattleUnitData.Stats[stat]);
        // }
    }

    private float UpdateStat(float att, float multiplier) // higher multiplier = higher result
    {
        return att*multiplier / (1 + (att * 0.025f));
        // multiple att at the divisor by a higher number for greater diminishing returns (reduced power at higher numbers) (start at 0.025)
    }

}

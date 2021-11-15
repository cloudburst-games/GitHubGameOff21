using Godot;
using System;
using System.Collections.Generic;

[Serializable()]
public class UnitData : IStoreable
{
    public string ID = "";
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

    public DialogueData CurrentDialogueData = new DialogueData();
}

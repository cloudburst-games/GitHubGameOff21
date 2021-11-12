using Godot;
using System;
using System.Collections.Generic;

[Serializable()]
public class UnitData : IStoreable
{
    public string ID = "";
    public string Name = "";
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
    
    public enum Attribute { Vigour, Resilience, Intellect, Swiftness, Charisma, Luck}
    
    public Dictionary<Attribute, int> Attributes {get; set;} = new Dictionary<Attribute, int>()
    {
        {Attribute.Vigour, 10},
        {Attribute.Resilience, 10},
        {Attribute.Intellect, 10},
        {Attribute.Swiftness, 10},
        {Attribute.Charisma, 10},
        {Attribute.Luck, 10}
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
    public BattleUnitData MainCombatant {get; set;} = new BattleUnitData();
    public List<BattleUnitData> Minions {get; set;} = new List<BattleUnitData>();
    // public Dictionary<BattleUnitData, int> Minions {get; set;} = new Dictionary<BattleUnitData, int>();

    //

    public DialogueData CurrentDialogueData = new DialogueData();
}

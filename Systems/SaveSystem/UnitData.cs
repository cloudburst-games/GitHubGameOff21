using Godot;
using System;
using System.Collections.Generic;

[Serializable()]
public class UnitData : IStoreable
{
    public string ID = "";
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
    // public bool NPC {get; set;} = false;
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

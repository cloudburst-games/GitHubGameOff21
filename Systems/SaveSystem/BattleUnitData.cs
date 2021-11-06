using Godot;
using System;
using System.Collections.Generic;

[Serializable()]
public class BattleUnitData : IStoreable
{
    public BattleUnit.Combatant Combatant = BattleUnit.Combatant.Noob;
    public int Level {get; set;} = 1;

}

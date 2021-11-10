using Godot;
using System;

public class BattleUnitActionState : Reference
{
    public BattleUnit BattleUnit {get; set;}

    public BattleUnitActionState()
    {

    }

    public BattleUnitActionState(BattleUnit battleUnit)
    {
        this.BattleUnit = battleUnit;
    }

    public virtual void Update(float delta)
    {

    }
}

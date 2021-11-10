using Godot;
using System;

public class MovingBattleUnitActionState : BattleUnitActionState
{
    public MovingBattleUnitActionState()
    {

    }

    public MovingBattleUnitActionState(BattleUnit battleUnit)
    {
        this.BattleUnit = battleUnit;
    }

    public override void Update(float delta)
    {

    }
}

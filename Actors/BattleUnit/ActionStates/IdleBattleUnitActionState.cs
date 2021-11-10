using Godot;
using System;

public class IdleBattleUnitActionState : BattleUnitActionState
{
    public IdleBattleUnitActionState()
    {

    }

    public IdleBattleUnitActionState(BattleUnit battleUnit)
    {
        this.BattleUnit = battleUnit;
        BattleUnit.PlayActionAnim("Idle");
    }

    public override void Update(float delta)
    {
    }
}

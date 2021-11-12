using Godot;
using System;

public class DyingBattleUnitActionState : BattleUnitActionState
{
    public DyingBattleUnitActionState()
    {

    }

    public DyingBattleUnitActionState(BattleUnit battleUnit)
    {
        this.BattleUnit = battleUnit;
        BattleUnit.Dead = true;
        CalculateDirection(BattleUnit.TargetWorldPos);
        SingleAnim("Die");
    }

    public override void Update(float delta)
    {

    }
}

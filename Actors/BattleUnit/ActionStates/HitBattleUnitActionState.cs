using Godot;
using System;

public class HitBattleUnitActionState : BattleUnitActionState
{
    public HitBattleUnitActionState()
    {

    }

    public HitBattleUnitActionState(BattleUnit battleUnit)
    {
        this.BattleUnit = battleUnit;
        CalculateDirection(BattleUnit.TargetWorldPos);
        SingleAnim("Hit");
    }

    public override void Update(float delta)
    {

    }
}

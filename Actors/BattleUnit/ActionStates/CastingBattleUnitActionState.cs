using Godot;
using System;

public class CastingBattleUnitActionState : BattleUnitActionState
{
    public CastingBattleUnitActionState()
    {
 
    }

    public CastingBattleUnitActionState(BattleUnit battleUnit)
    {
        this.BattleUnit = battleUnit;
        CalculateDirection(BattleUnit.TargetWorldPos);

        SingleAnim("Cast");

    }

    public override void Update(float delta)
    {
            // BattleUnit.EmitSignal(nameof(BattleUnit.CurrentActionCompleted));
            // BattleUnit.SetActionState(BattleUnit.ActionStateMode.Idle);
    }

    // public async void SingleCastAnim()
    // {
    //     BattleUnit.PlayActionAnim("Cast");
    //     await ToSignal(BattleUnit.ActionAnim, "animation_finished");
    //     BattleUnit.EmitSignal(nameof(BattleUnit.CurrentActionCompleted));
    //     BattleUnit.SetActionState(BattleUnit.ActionStateMode.Idle);

    // }
}

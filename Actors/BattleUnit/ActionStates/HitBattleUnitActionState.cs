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
        this.BattleUnit.PlaySoundEffect(GD.Load<AudioStreamSample>("res://Music/SFX_GHGO/WorldSFX/GiveHurt.wav"));
    }

    public override void Update(float delta)
    {

    }
}

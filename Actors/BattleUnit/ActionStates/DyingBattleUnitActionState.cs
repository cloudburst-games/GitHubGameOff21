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
        this.BattleUnit.PlaySoundEffect(GD.Load<AudioStreamSample>("res://Music/SFX_GHGO/WorldSFX/Die.wav"), AudioManager.AudioBus.Voice);
    }
    
    public override void Update(float delta)
    {

    }
}

using Godot;
using System.Linq;
using System;

public class MovingBattleUnitActionState : BattleUnitActionState
{
    private Tween _moveTween;

    public MovingBattleUnitActionState()
    {

    }

    public MovingBattleUnitActionState(BattleUnit battleUnit)
    {
        this.BattleUnit = battleUnit;
         _moveTween = BattleUnit.GetNode<Tween>("MoveTween");

         BattleUnit.CurrentPath.RemoveAt(0);
         TweenMovement();
        if (this.BattleUnit.HasNode("AudioDataMove"))
        {
            // if (!this.Unit.GetNode<AudioData>("AudioData").Playing())
            // {
            this.BattleUnit.GetNode<AudioData>("AudioDataMove").Loop = true;
            this.BattleUnit.GetNode<AudioData>("AudioDataMove").StartPlaying = true;
            // }
        }
    }

    public override void Update(float delta)
    {
//         if (BattleUnit.CurrentPath.Count > 0)
//         {
// //             Forward: 2.677945
// // Backward: -0.4636476
// // Right: -2.677945
// // Left: 0.4646476

//             // GD.Print(BattleUnit.GlobalPosition.AngleToPoint(BattleUnit.CurrentPath[0]));
//         }
        BattleUnit.PlayActionAnim("Walk");
    }


    private async void TweenMovement()
    {
        if (BattleUnit.CurrentPath.Count > 0)
        {
            CalculateDirection(BattleUnit.CurrentPath[0], true);
            _moveTween.InterpolateProperty(BattleUnit, "global_position", null, BattleUnit.CurrentPath[0], 1/BattleUnit.AnimSpeed, Tween.TransitionType.Linear);
            BattleUnit.CurrentPath.RemoveAt(0);
            _moveTween.SetActive(true);
            await ToSignal(_moveTween, "tween_all_completed");
            TweenMovement();
        }
        else
        {
            if (this.BattleUnit.HasNode("AudioDataMove"))
            {
                this.BattleUnit.GetNode<AudioData>("AudioDataMove").StopLastSoundPlayer();
            }
            BattleUnit.EmitSignal(nameof(BattleUnit.CurrentActionCompleted));
            BattleUnit.SetActionState(BattleUnit.ActionStateMode.Idle);
        }
    }

}

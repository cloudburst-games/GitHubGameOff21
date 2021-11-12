using Godot;
using System.Linq;
using System;

public class MovingBattleUnitActionState : BattleUnitActionState
{
    private Tween _moveTween;
    private float _moveTweenSpeed = 2f;
    private bool _firstPoint;

    public MovingBattleUnitActionState()
    {

    }

    public MovingBattleUnitActionState(BattleUnit battleUnit)
    {
        this.BattleUnit = battleUnit;
         _moveTween = BattleUnit.GetNode<Tween>("MoveTween");
         _firstPoint = true;

         BattleUnit.CurrentPath.RemoveAt(0);
         TweenMovement();
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

    private void CalculateDirection()
    {
        if (BattleUnit.GlobalPosition.AngleToPoint(BattleUnit.CurrentPath[0]) > 2.5f)
        {
            BattleUnit.Direction = BattleUnit.DirectionFacingMode.UpRight;
        }
        else if (BattleUnit.GlobalPosition.AngleToPoint(BattleUnit.CurrentPath[0]) > 0.4f)
        {
            BattleUnit.Direction = BattleUnit.DirectionFacingMode.UpLeft;
        }
        else if (BattleUnit.GlobalPosition.AngleToPoint(BattleUnit.CurrentPath[0]) > -0.54f)
        {
            BattleUnit.Direction = BattleUnit.DirectionFacingMode.DownLeft;
        }
        else// if (BattleUnit.GlobalPosition.AngleToPoint(BattleUnit.CurrentPath[0]) > -0.54f)
        {
            BattleUnit.Direction = BattleUnit.DirectionFacingMode.DownRight;
        }
    }

    private async void TweenMovement()
    {
        if (BattleUnit.CurrentPath.Count > 0)
        {
            CalculateDirection();
            _moveTween.InterpolateProperty(BattleUnit, "global_position", null, BattleUnit.CurrentPath[0], 1/_moveTweenSpeed, Tween.TransitionType.Linear);
            BattleUnit.CurrentPath.RemoveAt(0);
            _moveTween.SetActive(true);
            await ToSignal(_moveTween, "tween_all_completed");
            TweenMovement();
        }
        else
        {
            BattleUnit.EmitSignal(nameof(BattleUnit.CurrentActionCompleted));
            BattleUnit.SetActionState(BattleUnit.ActionStateMode.Idle);
        }
    }

}

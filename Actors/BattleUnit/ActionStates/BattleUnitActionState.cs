using Godot;
using System;

public class BattleUnitActionState : Reference
{
    public BattleUnit BattleUnit {get; set;}
    // private float _animSpeed = 2f;
    // public float AnimSpeed {
    //     get {
    //         return _animSpeed;
    //     }
    //     set {
    //         _animSpeed = value;
    //     }
    // }
    // private float _startAnimSpeed = 2f;

    public BattleUnitActionState()
    {

    }

    public async void SingleAnim(string animName)
    {
        BattleUnit.PlayActionAnim(animName);
        await ToSignal(BattleUnit.ActionAnim, "animation_finished");
        if (animName == "Die")
        {
            BattleUnit.Visible = false;
        }
        BattleUnit.EmitSignal(nameof(BattleUnit.CurrentActionCompleted));
        BattleUnit.SetActionState(BattleUnit.ActionStateMode.Idle);
    }
    public void CalculateDirection(Vector2 worldPos, bool move = false)
    {
        if (worldPos == BattleUnit.GlobalPosition)
        {
            return;
        }
        if (BattleUnit.GlobalPosition.AngleToPoint(worldPos) > 3.1f)
        {
            BattleUnit.Direction = BattleUnit.DirectionFacingMode.Right;
        }
        else if (BattleUnit.GlobalPosition.AngleToPoint(worldPos) > 2.6f)
        {
            BattleUnit.Direction = BattleUnit.DirectionFacingMode.UpRight;
        }
        else if (BattleUnit.GlobalPosition.AngleToPoint(worldPos) > 1.52f && !move)
        {
            BattleUnit.Direction = BattleUnit.DirectionFacingMode.Up;
        }
        else if (BattleUnit.GlobalPosition.AngleToPoint(worldPos) > 0.46f)
        {
            BattleUnit.Direction = BattleUnit.DirectionFacingMode.UpLeft;
        }
        else if (BattleUnit.GlobalPosition.AngleToPoint(worldPos) > -0.1f && !move)
        {
            BattleUnit.Direction = BattleUnit.DirectionFacingMode.Left;
        }
        else if (BattleUnit.GlobalPosition.AngleToPoint(worldPos) > -0.47f)
        {
            BattleUnit.Direction = BattleUnit.DirectionFacingMode.DownLeft;
        }
        else if (BattleUnit.GlobalPosition.AngleToPoint(worldPos) > -1.6f && !move)
        {
            BattleUnit.Direction = BattleUnit.DirectionFacingMode.Down;
        }
        else if (BattleUnit.GlobalPosition.AngleToPoint(worldPos) > -2.7f)
        {
            BattleUnit.Direction = BattleUnit.DirectionFacingMode.DownRight;
        }
        // GD.Print(BattleUnit.GlobalPosition.AngleToPoint(worldPos));
    }

    public BattleUnitActionState(BattleUnit battleUnit)
    {
        this.BattleUnit = battleUnit;
    }

    public virtual void Update(float delta)
    {
    }

    // public void Start()
    // {
    //     AnimSpeed = _startAnimSpeed;
    // }
}

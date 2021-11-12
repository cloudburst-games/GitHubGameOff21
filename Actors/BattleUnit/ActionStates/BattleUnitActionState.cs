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
        BattleUnit.EmitSignal(nameof(BattleUnit.CurrentActionCompleted));
        BattleUnit.SetActionState(BattleUnit.ActionStateMode.Idle);
    }
    public void CalculateDirection(Vector2 worldPos)
    {
        // GD.Print(BattleUnit.GlobalPosition.AngleToPoint(worldPos));
        if (BattleUnit.GlobalPosition.AngleToPoint(worldPos) > 2.5f)
        {
            BattleUnit.Direction = BattleUnit.DirectionFacingMode.UpRight;
        }
        else if (BattleUnit.GlobalPosition.AngleToPoint(worldPos) > 0.4f)
        {
            BattleUnit.Direction = BattleUnit.DirectionFacingMode.UpLeft;
        }
        else if (BattleUnit.GlobalPosition.AngleToPoint(worldPos) > -0.54f)
        {
            BattleUnit.Direction = BattleUnit.DirectionFacingMode.DownLeft;
        }
        else// if (BattleUnit.GlobalPosition.AngleToPoint(BattleUnit.CurrentPath[0]) > -0.54f)
        {
            BattleUnit.Direction = BattleUnit.DirectionFacingMode.DownRight;
        }
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

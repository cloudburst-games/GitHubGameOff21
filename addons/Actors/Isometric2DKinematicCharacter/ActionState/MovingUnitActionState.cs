using Godot;
using System;

public class MovingUnitActionState : UnitActionState
{
	
	public MovingUnitActionState(Unit unit)
	{
		this.Unit = unit;
		// GD.Print("entering moving action state");
	}
    
	public MovingUnitActionState()
	{
		GD.Print("use constructor with unit argument");
		throw new InvalidOperationException();
	}

	public override void Update(float delta)
    {
        base.Update(delta);
		this.Unit.SetActionAnim(this.Unit.WalkAnimationsByDirection[this.Unit.DirectionAnim]);

		if (this.Unit.CurrentVelocity.LengthSquared() <= 1)
		{
			this.Unit.SetActionState(Unit.ActionState.Idle);
		}
		this.Unit.MoveAndSlide(this.Unit.CurrentVelocity);
    }
}

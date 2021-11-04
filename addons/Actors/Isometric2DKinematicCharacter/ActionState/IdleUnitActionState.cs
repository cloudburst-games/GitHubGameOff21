using Godot;
using System;

public class IdleUnitActionState : UnitActionState
{
	public IdleUnitActionState(Unit unit)
	{
		this.Unit = unit;
		// GD.Print("entering idle action state");
	}
	
	public IdleUnitActionState()
	{
		GD.Print("use constructor with unit argument");
		throw new InvalidOperationException();
	}

	public override void Update(float delta)
	{
		base.Update(delta);
		this.Unit.SetActionAnim(this.Unit.IdleAnimationsByDirection[this.Unit.DirectionAnim]);

		if (this.Unit.CurrentVelocity.LengthSquared() > 1)
		{
			this.Unit.SetActionState(Unit.ActionState.Moving);
		}
		else
		{
			// GD.Print(this.Unit.CurrentVelocity.LengthSquared());
		}
	}
}

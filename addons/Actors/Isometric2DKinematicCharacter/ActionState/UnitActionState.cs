using Godot;
using System;

public class UnitActionState : Reference
{
	public Unit Unit {get; set;}

	public virtual void Update(float delta)
	{
		// GD.Print(this.Unit.CurrentVelocity.LengthSquared());
	}
}

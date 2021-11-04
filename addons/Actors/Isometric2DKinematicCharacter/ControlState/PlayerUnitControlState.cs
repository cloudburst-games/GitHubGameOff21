using Godot;
using System;

public class PlayerUnitControlState : UnitControlState
{
	public PlayerUnitControlState(Unit unit)
	{
		this.Unit = unit;
	}
    
	public PlayerUnitControlState()
	{
		GD.Print("use constructor with unit argument");
		throw new InvalidOperationException();
	}

    public override void Update(float delta)
    {
        base.Update(delta);

		bool up = Input.IsActionPressed("Move Up");
		bool down =  Input.IsActionPressed("Move Down");
		bool left =  Input.IsActionPressed("Move Left");
		bool right =  Input.IsActionPressed("Move Right");
		

		SetTargetAnimRotation(this.Unit.Position + this.Unit.CurrentVelocity);
		this.Unit.CurrentVelocity = new Vector2( left ? -1 : (right ? 1 : 0), up ? -1 : (down ? 1 : 0)).Normalized();
		this.Unit.CurrentVelocity *= this.Unit.Speed;
		this.Unit.AnimRotation = TargetAnimRotation;
    }
}

using Godot;
using System;

public class UnitActionState : Reference
{
	public Unit Unit {get; set;}

	public virtual void Update(float delta)
	{
        if (this.Unit.CurrentVelocity.LengthSquared() <= 1)
		{
            if (this.Unit.HasNode("AudioData"))
            {
                // if (this.Unit.GetNode<AudioData>("AudioData").Playing())
                // {
                    this.Unit.GetNode<AudioData>("AudioData").StopLastSoundPlayer();
                // }
            }
        }
        // if (this.Unit.GetControlState() != Unit.ControlState.Player)
		// GD.Print(this.Unit.CurrentVelocity.LengthSquared());
	}
}

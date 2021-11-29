using Godot;
using System;

public class MovingUnitActionState : UnitActionState
{
	
	public MovingUnitActionState(Unit unit)
	{
		this.Unit = unit;
		// GD.Print("entering moving action state");
        if (this.Unit.HasNode("AudioData"))
        {
            // if (!this.Unit.GetNode<AudioData>("AudioData").Playing())
            // {
            this.Unit.GetNode<AudioData>("AudioData").Loop = true;
            this.Unit.GetNode<AudioData>("AudioData").StartPlaying = true;
            // }
        }
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


		if (this.Unit.CurrentVelocity.LengthSquared() <= (this.Unit.GetControlState() == Unit.ControlState.Player ? 1 : 13000))
		{
            if (this.Unit.HasNode("AudioData"))
            {
                // if (this.Unit.GetNode<AudioData>("AudioData").Playing())
                // {
                    this.Unit.GetNode<AudioData>("AudioData").StopLastSoundPlayer();
                // }
            }
			this.Unit.SetActionState(Unit.ActionState.Idle);
		}
		this.Unit.MoveAndSlide(this.Unit.CurrentVelocity);
    }
}

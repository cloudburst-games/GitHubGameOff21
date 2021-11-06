using Godot;
using System;

public class FollowAIBehaviourState : AIBehaviourState
{

	private RandomNumberGenerator _rand = new RandomNumberGenerator();
	private Timer _followTimer;

	// private float _wanderRangeLower;
	// private float _wanderRangeUpper;

	public FollowAIBehaviourState()
	{
		throw new InvalidOperationException();
	}    
	public FollowAIBehaviourState(AIUnitControlState unitControlState)
	{
		this.AIControl = unitControlState;
		// _wanderRangeLower = _rand.RandfRange(-700,-400);
		// _wanderRangeUpper = _rand.RandfRange(400,700);
		_followTimer = new Timer();
		_followTimer.OneShot = true;
		unitControlState.Unit.AddChild(_followTimer);
	}

	public override void Update(float delta)
	{
		base.Update(delta);
		if (AIControl.CurrentPath.Count <= 1 && _followTimer.TimeLeft == 0)
		{
            // GD.Print("get a follow!");
			AIControl.EmitSignal(nameof(AIUnitControlState.FollowPathRequested),AIControl);
			_rand.Randomize();
			_followTimer.WaitTime = 0.2f;//_rand.RandfRange(1,5);
			_followTimer.Start();
		}

		// if (AIControl.CurrentPath.Count <= 1 && AIControl.Steering.IsAvoiding)
		// {
		// 	AIControl.CurrentPath.Clear();
		// }
	}

	public override void Die()
	{
		base.Die();
		_followTimer.QueueFree();
	}

}

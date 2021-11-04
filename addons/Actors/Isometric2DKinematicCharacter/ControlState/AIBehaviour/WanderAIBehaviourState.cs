using Godot;
using System;

public class WanderAIBehaviourState : AIBehaviourState
{

	private RandomNumberGenerator _rand = new RandomNumberGenerator();
	private Timer _wanderTimer;

	private float _wanderRangeLower;
	private float _wanderRangeUpper;

	public WanderAIBehaviourState()
	{
		throw new InvalidOperationException();
	}    
	public WanderAIBehaviourState(AIUnitControlState unitControlState)
	{
		this.AIControl = unitControlState;
		_wanderRangeLower = _rand.RandfRange(-700,-400);
		_wanderRangeUpper = _rand.RandfRange(400,700);
		_wanderTimer = new Timer();
		_wanderTimer.OneShot = true;
		unitControlState.Unit.AddChild(_wanderTimer);
	}

	public override void Update(float delta)
	{
		base.Update(delta);
		// GD.Print("test");
		if (AIControl.CurrentPath.Count <= 1 && _wanderTimer.TimeLeft == 0)
		{
			AIControl.EmitSignal(nameof(AIUnitControlState.PathRequested),AIControl,
			new Vector2(AIControl.StartPosition.x + _rand.RandfRange(_wanderRangeLower,_wanderRangeUpper), 
				AIControl.StartPosition.y + _rand.RandfRange(_wanderRangeLower,_wanderRangeUpper)));
			_rand.Randomize();
			_wanderTimer.WaitTime = _rand.RandfRange(1,5);
			_wanderTimer.Start();
		}

		if (AIControl.CurrentPath.Count <= 2 && AIControl.Steering.IsAvoiding)
		{
			AIControl.CurrentPath.Clear();
		}
	}

	public override void Die()
	{
		base.Die();
		_wanderTimer.QueueFree();
	}

}

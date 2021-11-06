using Godot;
using System;
using System.Collections.Generic;

public class PatrolAIBehaviourState : AIBehaviourState
{

	private RandomNumberGenerator _rand = new RandomNumberGenerator();
	private Timer _patrolTimer;


	public PatrolAIBehaviourState()
	{
		throw new InvalidOperationException();
	}    
	public PatrolAIBehaviourState(AIUnitControlState unitControlState)
	{
		this.AIControl = unitControlState;
		_patrolTimer = new Timer();
		_patrolTimer.OneShot = true;
		unitControlState.Unit.AddChild(_patrolTimer);
	}

	public override void Update(float delta)
	{
		base.Update(delta);
        
		if (AIControl.CurrentPath.Count <= 1 && _patrolTimer.TimeLeft == 0)
		{
            Vector2 point = AIControl.Unit.CurrentUnitData.PatrolPoints[0];
            AIControl.Unit.CurrentUnitData.PatrolPoints.Remove(point);
            AIControl.Unit.CurrentUnitData.PatrolPoints.Add(point);
            AIControl.EmitSignal(nameof(AIUnitControlState.PathRequested),AIControl, point);
			_rand.Randomize();
			_patrolTimer.WaitTime = _rand.RandfRange(5,10);
			_patrolTimer.Start();
		}

		if (AIControl.CurrentPath.Count <= 2 && AIControl.Steering.IsAvoiding)
		{
			AIControl.CurrentPath.Clear();
		}
	}


	public override void Die()
	{
		base.Die();
		_patrolTimer.QueueFree();
	}

}

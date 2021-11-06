using Godot;
using System;

public class StationaryAIBehaviourState : AIBehaviourState
{


	public StationaryAIBehaviourState()
	{
		throw new InvalidOperationException();
	}    
	public StationaryAIBehaviourState(AIUnitControlState unitControlState)
	{
		this.AIControl = unitControlState;
	}

	public override void Update(float delta)
	{
		base.Update(delta);
	}

	public override void Die()
	{
		base.Die();
	}

}

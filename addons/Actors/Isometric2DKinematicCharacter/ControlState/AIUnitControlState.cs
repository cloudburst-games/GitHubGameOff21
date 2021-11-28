using Godot;
using System;
using System.Collections.Generic;

public class AIUnitControlState : UnitControlState
{
	public Steering Steering {get; set;}
	private Area2D _unitDetectArea;
	private CollisionShape2D _shape;
	public List<Vector2> CurrentPath = new List<Vector2>();
	
	public enum AIBehaviour { Wander, Follow, Patrol, Stationary }
	public AIBehaviourState _currentAIBehaviourState;
	public Vector2 StartPosition {get; set;}
	public bool IsAvoiding {get; set;} = false;

	[Signal]
	public delegate void PathRequested(AIUnitControlState aIUnitControlState, Vector2 worldPosition);
	[Signal]
	public delegate void PathToPlayerRequested(AIUnitControlState aIUnitControlState);
	[Signal]
	public delegate void FollowPathRequested(AIUnitControlState aIUnitControlState);

	public AIUnitControlState(Unit unit)
	{
		this.Unit = unit;
		_unitDetectArea = this.Unit.GetNode<Area2D>("UnitDetectArea");
		_shape = this.Unit.GetNode<CollisionShape2D>("Shape");
		_unitDetectArea.Connect("body_entered", this, nameof(OnUnitDetectAreaEntered));
		_unitDetectArea.Connect("body_exited", this, nameof(OnUnitDetectAreaExited));
		StartPosition = this.Unit.Position;
		Steering = new Steering(maximumForce:75f, maximumSpeed:this.Unit.Speed, extents:((RectangleShape2D) _shape.Shape).Extents, separationFactor:2f);

		// Set default state here:
		// SetAIBehaviourState(AIBehaviour.Wander);
        SetAIBehaviourState(this.Unit.CurrentUnitData.Behaviour);
	}
	public AIUnitControlState()
	{
		GD.Print("use constructor with unit argument");
		throw new InvalidOperationException();
	}

	public void SetAIBehaviourState(AIBehaviour state)
	{
		if (_currentAIBehaviourState != null)
		{
			_currentAIBehaviourState.Die();
		}
		switch (state)
		{
			case AIBehaviour.Wander:
				_currentAIBehaviourState = new WanderAIBehaviourState(this);
				break;
			case AIBehaviour.Follow:
				_currentAIBehaviourState = new FollowAIBehaviourState(this);
				break;
			case AIBehaviour.Stationary:
				_currentAIBehaviourState = new StationaryAIBehaviourState(this);
				break;
			case AIBehaviour.Patrol:
				_currentAIBehaviourState = new PatrolAIBehaviourState(this);
				break;
			
		}
	}
	
	private void OnUnitDetectAreaEntered(PhysicsBody2D body)
	{
		if (! (body is StaticBody2D || body is KinematicBody2D || body is RigidBody2D))
		{
			return;
		}
		if (body is Unit unit)
		{
			if (!Steering.DetectedUnits.Contains(unit))
			{
				Steering.DetectedUnits.Add(unit);
			}
		}
		if (!Steering.DetectedPhysicsBodies.Contains(body))
		{
			Steering.DetectedPhysicsBodies.Add(body);
		}
	}


	private void OnUnitDetectAreaExited(PhysicsBody2D body)
	{
		if (! (body is StaticBody2D || body is KinematicBody2D || body is RigidBody2D))
		{
			return;
		}
		if (body is Unit unit)
		{
			if (Steering.DetectedUnits.Contains(unit))
			{
				Steering.DetectedUnits.Remove(unit);
			}
		}
		if (Steering.DetectedPhysicsBodies.Contains(body))
		{
			Steering.DetectedPhysicsBodies.Remove(body);
		}
	}

	private void RotateToTarget(Vector2 target, float delta)
	{
		if (Steering.MaximumForce == 75f) // at high enough steering force, rotate instantly
		{
			this.Unit.AnimRotation = TargetAnimRotation;
			return;
		}
		float lerpAngle = Mathf.LerpAngle(this.Unit.AnimRotation, TargetAnimRotation, Steering.MaximumForce/75f);
		this.Unit.AnimRotation = Mathf.PosMod(lerpAngle, 2*Mathf.Pi);
	}

	public override void Update(float delta)
	{
		base.Update(delta);

		_currentAIBehaviourState.Update(delta);
		if (CurrentPath.Count == 0)
		{
			this.Unit.CurrentVelocity = new Vector2(0,0);
            // if (_currentAIBehaviourState is PatrolAIBehaviourState)
            // {
            //     GD.Print(" test");
            // }
			return;
		}
		// GD.Print(CurrentPath.Count);
		// incorproate the steering stuff into AI controls
		Steering.Update(this.Unit.Position);
		// GD.Print(Unit.CurrentVelocity);
		// if (Unit.CurrentVelocity.x == Mathf.NaN)
		// {
		// 	Unit.CurrentVelocity = new Vector2();
		// }
		float areaExtents = (((RectangleShape2D) _shape.Shape).Extents.x * ((RectangleShape2D) _shape.Shape).Extents.y);
		// GD.Print(areaExtents);
		// If we have a path, move towards it, otherwise just stay still
		if (CurrentPath.Count > 0)
		{
			if (CurrentPath.Count > 1)
			{
				if (this.Unit.Position.DistanceSquaredTo(CurrentPath[1]) < 125*areaExtents)
				{
					CurrentPath.RemoveAt(0);//(this.Unit.Position.DistanceSquaredTo(CurrentPath[0]));
				}
			}
					// GD.Print("is it this1?");
			RotateToTarget(CurrentPath[0], delta);
				
			if (CurrentPath.Count == 1)
			{
					// GD.Print("is it this2?");
				if (this.Unit.Position.DistanceSquaredTo(CurrentPath[0]) < 125*areaExtents)
				{
					// this.Unit.CurrentVelocity = new Vector2(0,0);
					CurrentPath.RemoveAt(0);
					// Steering.IsAvoiding = false;
					// GD.Print("test");
				}
				else
				{
					RotateAndMove(CurrentPath[0],true, delta);
				}
			}
			else if (CurrentPath.Count <= 2)
			{
					// GD.Print("is it thi3s?");
				RotateAndMove(CurrentPath[1],true, delta);
			}
			else
			{
					// GD.Print("is it this4?");
				RotateAndMove(CurrentPath[1],false, delta);
			}
		}
		// else
		// {
		// 	this.Unit.CurrentVelocity = new Vector2(0,0);
		// }

		
	}

	public void RotateAndMove(Vector2 target, bool brake, float delta)
	{
		// Vector2 prevVelocity = this.Unit.CurrentVelocity;
		// MoveAndSlide( _steering.CalculateVelocity(target, brake));
		this.Unit.CurrentVelocity = Steering.CalculateVelocity(target, brake);

		SetTargetAnimRotation(target);
	}
}

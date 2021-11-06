using Godot;
using System;
using System.Collections.Generic;

public class Steering
{

	public float MaximumForce {get; set;}
	private float _maximumSpeed;
	private Vector2 _extents;
	private float _separationFactor;
	public bool IsAvoiding {get; set;} = false;

	public List<Unit> DetectedUnits {get; set;} = new List<Unit>();

	public List<PhysicsBody2D> DetectedPhysicsBodies {get; set;} = new List<PhysicsBody2D>();
	private Vector2 _position;
	public Vector2 CurrentVelocity {get; set;} = new Vector2();

	public Steering()
	{
		GD.Print("use the other constructor");
		throw new InvalidOperationException();
	}

	public Steering(float maximumForce, float maximumSpeed, Vector2 extents, float separationFactor)
	{
		MaximumForce = maximumForce;
		MaximumForce = Mathf.Clamp(MaximumForce, 1, 75);
		_maximumSpeed = maximumSpeed;
		_extents = extents;
		_separationFactor = separationFactor;
	}

	public void Update(Vector2 position)
	{
		_position = position;
	}

	// Returns a velocity towards the target location, reduced if close to the target, proportionally to distance from target
	private Vector2 GetArriveVelocity(Vector2 targetLocation, bool brake)
	{
		Vector2 arriveVelocity = GetDesiredVelocity(targetLocation).Normalized();
		float breakingDistanceSquared = CurrentVelocity.LengthSquared()*(1 + (8/MaximumForce));
		if (GetDesiredVelocity(targetLocation).LengthSquared() <= breakingDistanceSquared && brake)
		{
			float percent = GetDesiredVelocity(targetLocation).LengthSquared()/(breakingDistanceSquared);
			float percentOfMaximumSpeed = _maximumSpeed * percent;
			arriveVelocity *= percentOfMaximumSpeed;
		}
		else
		{
			arriveVelocity *= _maximumSpeed;
		}
		return arriveVelocity;
	}

	

	private Vector2 GetDesiredVelocity(Vector2 targetLocation)
	{
		return targetLocation - _position;
	}

	private Vector2 GetSteeringForce(Vector2 desiredVelocity)
	{
		return (desiredVelocity - CurrentVelocity).Clamped(MaximumForce);
	}

	private Vector2 GetSeparateVelocity()
	{
		float neighbourDistance = _extents.Length()*_separationFactor;
		int count = 0;
		Vector2 sum = new Vector2(0,0);
		foreach (PhysicsBody2D body in DetectedPhysicsBodies)
		{
			float rotateTo = Mathf.Pi/2;
			float distance = _position.DistanceTo(body.Position);
			if (distance < neighbourDistance)
			{
				Vector2 diff = (_position - body.Position).Rotated(rotateTo);
                if ((body is Unit unit))
                {
                    if (unit.CurrentUnitData.Companion)
                    {
                        // diff *= 0.1f;
                    }
                }
				count ++;
				sum += diff;
			}
		}
		if (count > 0)
		{
			sum /= count;
			sum = sum.Normalized();
			sum *= _maximumSpeed;
			return sum;
		}
		return new Vector2(0,0);
	}

	private Vector2 GetCohesionVelocity()
	{
		float neighbourDistance = _extents.Length()*(_separationFactor*2);
		int count = 0;
		Vector2 sum = new Vector2(0,0);
		foreach (Unit unit in DetectedUnits)
		{
			float distance = _position.DistanceSquaredTo(unit.Position);
			if (distance < Math.Pow(neighbourDistance,2))
			{
				count++;
				sum += unit.Position;
			}
		}
		if (count > 0)
		{
			sum /= count;
			return GetArriveVelocity(sum, true);
		}
		return new Vector2(0,0);
	}

	private Vector2 GetAlignmentVelocity()
	{
		float neighbourDistance = _extents.Length()*(_separationFactor*8);
		int count = 0;
		Vector2 sum = new Vector2(0,0);
		
		foreach (Unit unit in DetectedUnits)
		{
			float distance = _position.DistanceSquaredTo(unit.Position);
			if (distance < Math.Pow(neighbourDistance,2))
			{
				sum += unit.CurrentVelocity;
				count++;
			}
		}
		if (count > 0)
		{
			sum /= count;
			sum.Normalized();
			sum *= _maximumSpeed;
			return sum;
		}
		return new Vector2(0,0);
	}

	// this can be customised to adjust weights
	public Vector2 CalculateVelocity(Vector2 targetLocation, bool brake)
	{
		Vector2 separate = GetSeparateVelocity();
		Vector2 seek = GetArriveVelocity(targetLocation, brake);
		Vector2 cohesion = GetCohesionVelocity();
		Vector2 align = GetAlignmentVelocity();
		separate *= 2f;
		cohesion *= 0f;
		seek *= 1f;
		IsAvoiding = separate.Length() > 0;
		align *= 0f;
		// if (Mathf.IsNaN(cohesion.x) || Mathf.IsNaN(cohesion.y))
		// {
		// 	GD.Print("seek is NOT a number");
		// 	cohesion = new Vector2(0,0);
		// }
		// if (Mathf.IsNaN(separate.x) || Mathf.IsNaN(separate.y))
		// {
		// 	GD.Print("seek is NOT a number");
		// 	separate = new Vector2(0,0);
		// }
		Vector2 totalSteeringForce = GetSteeringForce(separate + cohesion + seek);
		// GD.Print(Math.Round(totalSteeringForce.Length(),1));
		// CurrentVelocity += totalSteeringForce;
		CurrentVelocity = (CurrentVelocity + totalSteeringForce).Clamped(_maximumSpeed);
		// GD.Print(CurrentVelocity.Length());
		// if (Mathf.IsNaN(CurrentVelocity.x) || Mathf.IsNaN(CurrentVelocity.y))
		// {
		// 	return new Vector2(0,0);
		// }
		return CurrentVelocity;
	}
}

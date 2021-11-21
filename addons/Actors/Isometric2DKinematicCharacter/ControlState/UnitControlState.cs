using Godot;
using System;

public class UnitControlState : Reference
{
	public Unit Unit {get; set;}
    public float TargetAnimRotation {get; set;} = 0;

    public virtual void Update(float delta)
    {

    }

    public virtual void UpdateInputEvents(InputEvent ev)
    {

    }

    public void SetTargetAnimRotation(Vector2 target)
    {

		Vector2 direction = (target-this.Unit.Position).Normalized();
		// todo -refactor:		
		if (direction.x > 0.8f && Math.Abs(direction.y) < 0.2f)
		{
			TargetAnimRotation = Mathf.Pi/2;
		}
		else if (direction.x > 0.2f && direction.y > 0.2f)
		{
			TargetAnimRotation = 3*(Mathf.Pi/4);
		}
		else if (Math.Abs(direction.x) <0.2f && direction.y > 0.8f)
		{
			TargetAnimRotation = Mathf.Pi;
		}
		else if (direction.x < -0.2f && direction.y > 0.2f)
		{
			TargetAnimRotation = 5*(Mathf.Pi/4);
		}
		else if (direction.x < -0.8f && Math.Abs(direction.y) < 0.2f)
		{
			TargetAnimRotation = 6*(Mathf.Pi/4);
		}
		else if (direction.x < -0.2f && direction.y < -0.2f)
		{
			TargetAnimRotation = 7*(Mathf.Pi/4);
		}
		else if (Math.Abs(direction.x) < 0.2f && direction.y < -0.8f)
		{
			TargetAnimRotation = Mathf.Pi*2;
		}
		else if (direction.x > 0.2f && direction.y < -0.2f)
		{
			TargetAnimRotation = Mathf.Pi/4;
		}
		// else
		// {
		// 	GD.Print(direction);
		// }
    }
}

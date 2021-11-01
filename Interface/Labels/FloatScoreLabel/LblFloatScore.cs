using Godot;
using System;

public class LblFloatScore : Label
{
	[Export]
	private float _speed = 50f;

	[Export]
	private float _fadeSpeed = 0.8f; //higher is faster
	// private float distance = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		this.RectGlobalPosition += new Vector2(0, -_speed*delta);
		// distance += _speed * delta;

		Modulate = new Color(1,1,1,Modulate.a-delta/(1/_fadeSpeed));
		if (Modulate.a <= 0)
		{
			Die();
		}
	}

	public void Die()
	{
		QueueFree();
	}
}

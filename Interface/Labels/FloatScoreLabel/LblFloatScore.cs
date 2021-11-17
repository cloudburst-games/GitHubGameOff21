using Godot;
using System;

public class LblFloatScore : Label
{
	[Export]
	private float _speed = 50f;

	[Export]
	public float FadeSpeed {get; set;} = 0.8f; //higher is faster

    private float _edgeBuffer = 50f;
	// private float distance = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetProcess(false);
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
        // GD.Print(RectGlobalPosition);
		this.RectGlobalPosition += new Vector2(0, -_speed*delta);
		// distance += _speed * delta;

		Modulate = new Color(1,1,1,Modulate.a-delta/(1/FadeSpeed));
		if (Modulate.a <= 0)
		{
			Die();
		}
	}

    public void Start(Vector2 startPos)
    {
        float x = Mathf.Clamp(RectGlobalPosition.x - RectSize.x, _edgeBuffer, GetViewportRect().Size.x - RectSize.x - _edgeBuffer);
        RectGlobalPosition = new Vector2(x, startPos.y);// startPos - new Vector2(RectSize.x/2f, 0);
        
        SetProcess(true);
    }

	public void Die()
	{
		QueueFree();
	}
}

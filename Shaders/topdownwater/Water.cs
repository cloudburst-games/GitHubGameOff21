using Godot;
using System;

[Tool]
public class Water : Sprite
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		OnZoomChanged();
	}
	private void OnZoomChanged()
	{
		((ShaderMaterial)Material).SetShaderParam("y_zoom", GetViewportTransform().y.y);
	}

	private void OnWaterItemRectChanged()
	{
		((ShaderMaterial)Material).SetShaderParam("scale", Scale);
	}

}


using Godot;
using System;

public class PnlPopMenu : Panel
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Visible = false;
		// GetNode<Panel>("PnlPopPlay").Visible = false;
		// GetNode<Panel>("PnlPopAbout").Visible = false;
	}

	private void OnBtnPlayPressed()
	{
		Visible = true;
		PopPlay();
	}

	private void OnBtnAboutPressed()
	{
		Visible = true;
		PopAbout();
	}

	public void PopPlay()
	{
		GetNode<Label>("LblTitle").Text = "Play!";
		GetNode<Panel>("PnlPopAbout").Visible = false;
		GetNode<Panel>("PnlPopPlay").Visible = true;
	}

	public void PopAbout()
	{
		GetNode<Label>("LblTitle").Text = "About!";
		GetNode<Panel>("PnlPopAbout").Visible = true;
		GetNode<Panel>("PnlPopPlay").Visible = false;
	}

	private void OnBtnBackPressed()
	{
		Visible = false;
	}

	public override void _Input(InputEvent ev)
	{
		if (Visible && ev is InputEventMouseButton evMouseButton && ev.IsPressed())
		{
			if (! (evMouseButton.Position.x > RectGlobalPosition.x && evMouseButton.Position.x < RectSize.x + RectGlobalPosition.x
			&& evMouseButton.Position.y > RectGlobalPosition.y && evMouseButton.Position.y < RectSize.y + RectGlobalPosition.y) )
			{
				GD.Print("CLICKED OUTSIDE MENU");
				OnBtnBackPressed();
			}
		}
	}

}


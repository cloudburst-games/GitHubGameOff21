using Godot;
using System;

public class SpriteButton : TextureButton
{

	[Export]
	private string _infoText = "";

	// private Light2D _light;
	private Sprite _sprite;
	private InfoPanel _infoPanel;
	
	public override void _Ready()
	{
		_sprite = GetNode<Sprite>("Sprite");
		// _light = GetNode<Light2D>("Light2D");
		_infoPanel = GetNode<InfoPanel>("InfoPanel");
		// _light.Visible = false;
		_infoPanel.Init(_infoText);
	}

	private void OnMouseEntered()
	{
		// _light.Visible = true;
		_sprite.Modulate = new Color(1.5f,1.5f,1.5f);
	}


	private void OnMouseExited()
	{
		// _light.Visible = false;
		_sprite.Modulate = new Color(1,1,1);
	}

	private void OnBtnSpriteGuiInput(InputEvent ev)
	{
		if (ev is InputEventMouseButton evb)
		{
			if (evb.Pressed)
			{
				if (evb.ButtonIndex == (int)ButtonList.Right)
				{
					_infoPanel.Init(_infoText);
					_infoPanel.Start();
					_infoPanel.Visible = true;
				}
			}
			else
			{
				_infoPanel.Visible = false;
			}
		}
	}

}

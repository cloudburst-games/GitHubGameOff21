using Godot;
using System;
using System.Collections.Generic;

public class PnlGraphics : Panel
{


	[Signal]
	public delegate void SettingsChanged();

	private void OnCBoxFullScreenToggled(bool btnPressed)
	{
		OS.WindowFullscreen = btnPressed;
		EmitSignal(nameof(SettingsChanged));
	}

	public void RefreshSettings()
	{
		GetNode<CheckBox>("CBoxFullScreen").Pressed = OS.WindowFullscreen;
		if (!GetNode<CheckBox>("CBoxFullScreen").IsConnected("toggled", this, nameof(OnCBoxFullScreenToggled)))
		{
			GetNode<CheckBox>("CBoxFullScreen").Connect("toggled", this, nameof(OnCBoxFullScreenToggled));
		}
	}

}

using Godot;
using System;

public class CntPanels : Control
{
	public override void _Ready()
	{
		// GetNode<Button>("VBoxBtns/BtnControls").Disabled = true;

		foreach (Panel p in GetChildren())
		{
			p.Visible = false;
		}
		// GetNode<Panel>("CntPanels/PnlControls").Visible = true;
	}

}

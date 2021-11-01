using Godot;
using System;
using System.Collections.Generic;

public class PnlControls : Panel
{
	public new bool Visible {
		get{
			return base.Visible;
		}
		set{
			base.Visible = value;
			GetNode<Label>("LblInfo").Text = "";
		}
	}
	public void RefreshBtnMapText()
	{
		foreach (Node n in GetChildren())
		{
			foreach (Node m in n.GetChildren())
			{
				BtnRemap btnRemap = m.GetNode<BtnRemap>("BtnRemap");
				btnRemap.DisplayCurrentKey();
			}
		}
		GetNode<Label>("LblInfo").Text = "";
	}

	public void OnOtherBtnRemapped(string oldAction, string newAction)
	{
		string oldEvText = BtnRemap.GetReadableStringFromActionEvent((InputEvent)InputMap.GetActionList(oldAction)[0]); // this is now what the new action used to be
		string newEvText = BtnRemap.GetReadableStringFromActionEvent((InputEvent)InputMap.GetActionList(newAction)[0]); // this is now the new input (what the old action used to be)
		
		GetNode<Label>("LblInfo").Text = string.Format("{0} now bound to {1}", oldAction, oldEvText);
	}


}

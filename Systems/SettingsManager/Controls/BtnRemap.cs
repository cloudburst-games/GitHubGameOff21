// TOdo
// if no jsonsetting, make a set of default settings - suggest exporting a default variable (shouldnt need dev to edit via script). experiment how it is with default settings first - mayenot need

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class BtnRemap : Button
{
	[Export]
	public string Action {get; set;} = (string)InputMap.GetActions()[0];

	[Signal]
	public delegate void SettingsChanged();
	[Signal]
	public delegate void OtherActionRemapped(string oldAction, string newAction);

	private bool _alt = false;
	private bool _control = false;
	private bool _shift = false;

	public override void _Ready()
	{
		if (!InputMap.HasAction(Action))
		{
			GD.Print("Action " + Action + " not found. Make sure it exists in the Input Map.");
			throw new Exception();
		}
		SetProcessInput(false);
		DisplayCurrentKey();
	}

	public void DisplayCurrentKey()
	{
		InputEvent ev = (InputEvent)InputMap.GetActionList(Action)[0];
		Text = GetReadableStringFromActionEvent(ev);
	}

	public static string GetReadableStringFromActionEvent(InputEvent ev)
	{
		string currentKey = ev.AsText();
		if (ev is InputEventMouseButton eventMouseButton)
		{
			currentKey = string.Format("Mouse: {0}", (ButtonList)eventMouseButton.ButtonIndex);
			if (eventMouseButton.ButtonIndex == (int) ButtonList.WheelUp)
			{
				currentKey = string.Format("Mouse: {0}", "WheelUp");
			}
		}
		if (ev is InputEventJoypadButton eventJoypadButton)
		{
			currentKey = string.Format("Joy {0}, Btn {1}", eventJoypadButton.Device, eventJoypadButton.ButtonIndex);
		}
		if (ev is InputEventJoypadMotion eventJoypadMotion)
		{
			currentKey = string.Format("Joy {0}, Axis {1}: {2}", eventJoypadMotion.Device, eventJoypadMotion.Axis, eventJoypadMotion.AxisValue < 0 ? -1 : 1);
		}
		return currentKey;
	}

	public override void _Toggled(bool buttonPressed)
	{
		SetProcessInput(buttonPressed);
		if (buttonPressed)
		{
			Text = "... Key";
		}
		else
		{
			DisplayCurrentKey();
		}
	}

	public override void _Input(InputEvent ev)
	{
		if (ev is InputEventMouseMotion)
		{
			return;
		}
		if (ev is InputEventKey evk)
		{
			if (evk.Scancode == (uint) KeyList.Alt || evk.Scancode == (uint) KeyList.Control || evk.Scancode == (uint) KeyList.Shift)
			{
				return;
			}
			evk.Alt = _alt;
			evk.Control = _control;
			evk.Shift = _shift;
		}		
		RemapActionTo(ev);
		Pressed = false;
	}

	public override void _Process(float delta)
	{
		_alt = Input.IsKeyPressed((int)KeyList.Alt);
		_control = Input.IsKeyPressed((int)KeyList.Control);
		_shift = Input.IsKeyPressed((int)KeyList.Shift);
	}

	private void RemapActionTo(InputEvent ev)
	{
		string evTakenBy = EventTakenBy(ev);
		
		if (evTakenBy != "" && evTakenBy != Action)
		{
			RemapAction(evTakenBy, GetInputFromAction(Action));
		}
		RemapAction(Action, ev);
		Text = ev.AsText();
		EmitSignal(nameof(SettingsChanged));
		if (evTakenBy != "" && evTakenBy != Action)
		{
			EmitSignal(nameof(OtherActionRemapped), evTakenBy, Action);
		}
	}

	private string EventTakenBy(InputEvent ev)
	{
		foreach (string action in InputMap.GetActions())
		{
			if (ev is InputEventKey currentEventKey && GetInputFromAction(action) is InputEventKey oldEventKey)
			{
				if (EventKeyTakenBy(currentEventKey, oldEventKey))
				{
					return action;
				}
			}
			if (ev is InputEventMouseButton currentEventMb && GetInputFromAction(action) is InputEventMouseButton oldEventMb)
			{
				if (EventMouseButtonTakenBy(currentEventMb, oldEventMb))
				{
					return action;
				}
			}
			if (ev is InputEventJoypadButton currentEventJb && GetInputFromAction(action) is InputEventJoypadButton oldEventJb)
			{
				if (EventJoypadButtonTakenBy(currentEventJb, oldEventJb))
				{
					return action;
				}
			}
			if (ev is InputEventJoypadMotion currentEventJm && GetInputFromAction(action) is InputEventJoypadMotion oldEventJm)
			{
				if (EventJoypadMotionTakenBy(currentEventJm, oldEventJm))
				{
					return action;
				}
			}
		}
		return "";
	}

	private bool EventKeyTakenBy(InputEventKey currentEv, InputEventKey oldEv)
	{
		return (currentEv.Scancode == oldEv.Scancode && currentEv.Shift == oldEv.Shift && currentEv.Control == oldEv.Control && currentEv.Alt == oldEv.Alt && currentEv.Command == oldEv.Command);
	}

	private bool EventMouseButtonTakenBy(InputEventMouseButton currentEv, InputEventMouseButton oldEv)
	{
		return currentEv.ButtonIndex == oldEv.ButtonIndex;
	}

	private bool EventJoypadButtonTakenBy(InputEventJoypadButton currentEv, InputEventJoypadButton oldEv)
	{
		return (currentEv.Device == oldEv.Device && currentEv.ButtonIndex == oldEv.ButtonIndex);
	}

	private bool EventJoypadMotionTakenBy(InputEventJoypadMotion currentEv, InputEventJoypadMotion oldEv)
	{
		int currentEvAxisValue = currentEv.AxisValue < 0 ? -1 : 1;
		int oldEvAxisValue = oldEv.AxisValue < 0 ? -1 : 1;
		return (currentEv.Device == oldEv.Device && currentEv.Axis == oldEv.Axis && currentEvAxisValue == oldEvAxisValue);
	}

	private void RemapAction(string action, InputEvent inputEvent)
	{
		InputMap.ActionEraseEvents(action);
		InputMap.ActionAddEvent(action, inputEvent);
	}

	private InputEvent GetInputFromAction(string action)
	{
		return (InputEvent)InputMap.GetActionList(action)[0];
	}
	
}

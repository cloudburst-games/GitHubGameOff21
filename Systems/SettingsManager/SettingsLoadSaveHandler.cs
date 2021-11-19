using Godot;
using System;
using System.Collections.Generic;

public class SettingsLoadSaveHandler
{

    public delegate void DifficultySelectedDelegate(int difficulty);
    public event DifficultySelectedDelegate DifficultySelected;

	public void SaveToFile(int difficulty)
	{
		SettingsData data = new SettingsData() {
			KeyActionMapDict = GetKeyActionMapDict(), MouseActionMapDict = GetMouseActionMapDict(), 
			JoypadButtonActionMapDict = GetJoypadButtonActionMapDict(), JoypadMotionActionMapDict = GetJoypadMotionActionMapDict(),
			AudioSettingsDict = GetAudioSettingsDict(), GraphicsFullScreen = OS.WindowFullscreen, Difficulty = difficulty
		};

		FileJSON.SaveToJSON(OS.GetUserDataDir() + "/Settings.json", data);
	}

	public bool LoadFromFile()
	{
		string path = OS.GetUserDataDir() + "/Settings.json";
		if (! System.IO.File.Exists(path))
		{
			GD.Print("File at " + path + " not found.");
			return false;
		}
		SettingsData data = FileJSON.LoadFromJSON<SettingsData>(path);
		LoadControls(data);
		LoadAudio(data.AudioSettingsDict);
		LoadGraphics(data.GraphicsFullScreen);
        LoadDifficulty(data.Difficulty);
		return true;
	}

    private void LoadDifficulty(int difficulty)
    {
        DifficultySelected?.Invoke(difficulty);
    }

	private void LoadGraphics(bool fullscreen)
	{
		OS.WindowFullscreen = fullscreen;
	}

	private void LoadAudio(Dictionary<string, float> audioSettingsDict)
	{
		if (audioSettingsDict == null)
		{
			return;
		}
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Voice"), audioSettingsDict["Voice"]);
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Effects"), audioSettingsDict["Effects"]);
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), audioSettingsDict["Music"]);
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), audioSettingsDict["Master"]);
	}

	private void LoadControls(SettingsData data)
	{
		SetKeyActionMapDict(data.KeyActionMapDict);
		SetMouseActionMapDict(data.MouseActionMapDict);
		SetJoypadButtonActionMapDict(data.JoypadButtonActionMapDict);
		SetJoypadMotionActionMapDict(data.JoypadMotionActionMapDict);
	}

	private Dictionary<string, float> GetAudioSettingsDict()
	{
		return new Dictionary<string, float>()
		{
			{"Voice", AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Voice"))},
			{"Effects", AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Effects"))},
			{"Music", AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Music"))},
			{"Master", AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master"))}
		};
	}

	private void SetKeyActionMapDict(Dictionary<string, Tuple<string, bool[]>> keyActionMapDict)
	{
		foreach (string action in keyActionMapDict.Keys)
		{
			InputEventKey ev = new InputEventKey(); // bools: shift/ctrl/alt/command
			ev.Scancode = (uint) OS.FindScancodeFromString(keyActionMapDict[action].Item1);
			ev.Shift = keyActionMapDict[action].Item2[0];
			ev.Control = keyActionMapDict[action].Item2[1];
			ev.Alt = keyActionMapDict[action].Item2[2];
			ev.Command = keyActionMapDict[action].Item2[3];
			RemapAction(action, ev);
		}
	}
	private void SetMouseActionMapDict(Dictionary<string, int> mouseActionMapDict)
	{
		foreach (string action in mouseActionMapDict.Keys)
		{
			InputEventMouseButton ev = new InputEventMouseButton();
			ev.ButtonIndex = mouseActionMapDict[action];
			RemapAction(action, ev);
		}
	}

	private void SetJoypadButtonActionMapDict(Dictionary<string,int[]> joypadBtnActionMapDict)
	{
		foreach (string action in joypadBtnActionMapDict.Keys)
		{
			InputEventJoypadButton ev = new InputEventJoypadButton(); // device, buttonindex
			ev.Device = joypadBtnActionMapDict[action][0];
			ev.ButtonIndex = joypadBtnActionMapDict[action][1];
			RemapAction(action, ev);
		}
	}

	private void SetJoypadMotionActionMapDict(Dictionary<string,int[]> joypadBtnMotionMapDict)
	{
		foreach (string action in joypadBtnMotionMapDict.Keys)
		{
			InputEventJoypadMotion ev = new InputEventJoypadMotion(); // device, axis, axisvalue
			ev.Device = joypadBtnMotionMapDict[action][0];
			ev.Axis = joypadBtnMotionMapDict[action][1];
			ev.AxisValue = joypadBtnMotionMapDict[action][2];
			RemapAction(action, ev);
		}
	}

	private void RemapAction(string action, InputEvent ev)
	{
		if (!InputMap.HasAction(action))
		{
			GD.Print("Action " + action + " not found. Make sure it exists in the Input Map.");
			throw new Exception();
		}
		InputMap.ActionEraseEvents(action);
		InputMap.ActionAddEvent(action, ev);
	}

	private Dictionary<string, Tuple<string, bool[]>> GetKeyActionMapDict()
	{
		Dictionary<string, Tuple<string, bool[]>> keyActionMapDict = new Dictionary<string, Tuple<string, bool[]>>();
		foreach (string action in InputMap.GetActions())
		{
			InputEvent ev = (InputEvent)InputMap.GetActionList(action)[0];
			if (ev is InputEventKey eventKey)
			{
				keyActionMapDict.Add(action, new Tuple<string, bool[]>(OS.GetScancodeString(eventKey.Scancode), new bool[4] { 
					eventKey.Shift, eventKey.Control, eventKey.Alt, eventKey.Command
				}));
			}
		}
		return keyActionMapDict;
	}
	private Dictionary<string, int> GetMouseActionMapDict()
	{
		Dictionary<string, int> mouseActionMapDict = new Dictionary<string, int>();
		foreach (string action in InputMap.GetActions())
		{
			InputEvent ev = (InputEvent)InputMap.GetActionList(action)[0];
			if (ev is InputEventMouseButton eventKey)
			{
				mouseActionMapDict.Add(action,eventKey.ButtonIndex);
			}
		}
		return mouseActionMapDict;
	}
	private Dictionary<string, int[]> GetJoypadButtonActionMapDict()
	{
		Dictionary<string, int[]> joypadButtonActionMapDict = new Dictionary<string, int[]>();
		foreach (string action in InputMap.GetActions())
		{
			InputEvent ev = (InputEvent)InputMap.GetActionList(action)[0];
			if (ev is InputEventJoypadButton eventKey)
			{
				joypadButtonActionMapDict.Add(action, new int[2] {eventKey.Device, eventKey.ButtonIndex});
			}
		}
		return joypadButtonActionMapDict;
	}
	private Dictionary<string, int[]> GetJoypadMotionActionMapDict()
	{
		Dictionary<string, int[]> joypadMotionActionMapDict = new Dictionary<string, int[]>();
		foreach (string action in InputMap.GetActions())
		{
			InputEvent ev = (InputEvent)InputMap.GetActionList(action)[0];
			if (ev is InputEventJoypadMotion eventKey)
			{
				joypadMotionActionMapDict.Add(action, new int[3] {eventKey.Device, eventKey.Axis, eventKey.AxisValue < 0 ? -1 : 1});
			}
		}
		return joypadMotionActionMapDict;
	}

    public void OnDie()
    {
        DifficultySelected = null;
    }

}

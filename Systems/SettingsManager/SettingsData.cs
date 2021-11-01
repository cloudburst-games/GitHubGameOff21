using Godot;
using System;
using System.Collections.Generic;

public class SettingsData : IJSONSaveable
{
	public Dictionary<string, Tuple<string, bool[]>> KeyActionMapDict {get; set;}
	public Dictionary<string, int[]> JoypadButtonActionMapDict {get; set;}
	public Dictionary<string, int[]> JoypadMotionActionMapDict {get; set;} 
	public Dictionary<string, int> MouseActionMapDict {get; set;}
	public Dictionary<string, float> AudioSettingsDict {get; set;}
	public bool GraphicsFullScreen {get; set;}
}

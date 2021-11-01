// FileJSON: helper class for saving and loading conveniently in JSON format.

using System;
using Godot;
using Newtonsoft.Json;

public static class FileJSON
{
	public static void SaveToJSON(string path, IJSONSaveable input)
	{
		string jsonStr = JsonConvert.SerializeObject(input);
		string dir = new System.IO.FileInfo(path).Directory.FullName;
		System.IO.Directory.CreateDirectory(dir);
		System.IO.File.WriteAllText(path, jsonStr);

	}

	// Load data of type T from JSON format
	public static T LoadFromJSON<T>(string path)
	{
		if (! System.IO.File.Exists(path)){
			GD.Print("ERROR: file at " + path + " does not exist!");
			throw new Exception();
		}

		string loaded = System.IO.File.ReadAllText(path);
		T deserialized = JsonConvert.DeserializeObject<T>(loaded);
		if (! (deserialized is IJSONSaveable))
			GD.Print("WARNING, accessing object without interface + ", nameof(IJSONSaveable));
		return deserialized;
	}


	// Example use:
	// We wrap all our data in a convenient object to serialise/deserialize json
	
	// public class GameSettingsData : IJSONSaveable
	// {
	//     public Dictionary<int, string> PlayerNames;
	//     public Dictionary<string, float> SoundVolume;
	//     public Dictionary<GameSettings.Controls, string> InputEvents;
	//     public Dictionary<int, PlayerState.InputType> PlayerControls;
	// }
	//
	// public void SaveToJSON()
	// {
	//     GameSettingsData data = new GameSettingsData() {
	//         PlayerNames = this.PlayerNames,
	//         SoundVolume = this.SoundVolume,
	//         InputEvents = this.InputEvents,
	//     };

	//     File.SaveToJSON(FILEPATH, data);
	// }

	// public void LoadFromJSON()
	// {
	//     if (! System.IO.File.Exists(FILEPATH))
	//         return;
	//     GameSettingsData data = Utils.File.LoadFromJSON<GameSettingsData>(FILEPATH);
	//     this.PlayerNames = data.PlayerNames;
	//     this.SoundVolume = data.SoundVolume;
	//     this.InputEvents = data.InputEvents;
	// }
}


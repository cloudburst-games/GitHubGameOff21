using Godot;
using System;
using System.Collections.Generic;

public class BtnBase : Button
{
	private AudioStreamPlayer _soundPlayer;
	// Todo - redo the button sound stuff
	// private Dictionary<AudioData.ButtonSound, AudioStream> _btnSounds = AudioData.LoadedSounds<AudioData.ButtonSound>(AudioData.ButtonSoundPaths);
	public override void _Ready()
	{
		_soundPlayer = GetNode<AudioStreamPlayer>("SoundPlayer");
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

	private void OnBtnMouseEntered()
	{
		if (Disabled)
		{
			return;
		}
		// AudioHandler.PlaySound(_soundPlayer,_btnSounds[AudioData.ButtonSound.Hover], AudioData.SoundBus.Effects);
	}

	private void OnBtnPressed()
	{
		// AudioHandler.PlaySound(_soundPlayer,_btnSounds[AudioData.ButtonSound.Click], AudioData.SoundBus.Effects);
	}

}


using Godot;
using System;

public class PnlAudio : Panel
{
	//https://godotengine.org/qa/40911/best-way-to-create-a-volume-slider

	[Export]
	private AudioStream  _voiceSample;
	[Export]
	private AudioStream  _effectsSample;
	[Export]
	private AudioStream  _musicSample;
	
	[Signal]
	public delegate void SettingsChanged();

	private AudioStreamPlayer _soundPlayer;

	public override void _Ready()
	{
		_soundPlayer = GetNode<AudioStreamPlayer>("SoundPlayer");
		foreach (Node n in GetChildren())
		{
			if (n is HSlider slider)
			{
				slider.MinValue = 0.0001f;
				slider.MaxValue = 1f;
			}
		}
	}

	bool _refreshing = false;

	public void RefreshHSlidValues()
	{
		_refreshing = true;
		foreach (Node n in GetChildren())
		{
			if (n is HSlider slider)
			{
				if (slider.Name == "HSlidVoice")
				{
					slider.Value = Mathf.Exp((AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Voice"))/20f));
					// slider.Connect("value_changed", this, nameof(OnHSlidVoiceValueChanged));
					ConnectSliderSignal(slider, nameof(OnHSlidVoiceValueChanged));
				}
				if (slider.Name == "HSlidEffects")
				{
					slider.Value = Mathf.Exp((AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Effects"))/20f));
					// slider.Connect("value_changed", this, nameof(OnHSlidEffectsValueChanged));
					ConnectSliderSignal(slider, nameof(OnHSlidEffectsValueChanged));
				}
				if (slider.Name == "HSlidMusic")
				{
					slider.Value = Mathf.Exp((AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Music"))/20f));
					// slider.Connect("value_changed", this, nameof(OnHSlidMusicValueChanged));
					ConnectSliderSignal(slider, nameof(OnHSlidMusicValueChanged));
				}
				if (slider.Name == "HSlidMaster")
				{
					slider.Value = Mathf.Exp((AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master"))/20f));
					// slider.Connect("value_changed", this, nameof(OnHSlidMasterValueChanged));
					ConnectSliderSignal(slider, nameof(OnHSlidMasterValueChanged));
				}
			}
		}
		_refreshing = false;
	}

	private void ConnectSliderSignal(HSlider slider, string method)
	{
		if (slider.IsConnected("value_changed", this, method))
		{
			return;
		}
		slider.Connect("value_changed", this, method);
	}

	private void OnHSlidVoiceValueChanged(float value)
	{
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Voice"), Mathf.Log((float)GetNode<HSlider>("HSlidVoice").Value)*20);
		EmitSignal(nameof(SettingsChanged));
		if (_refreshing)
		{
			return;
		}
		_soundPlayer.Stream = _voiceSample;
		_soundPlayer.Bus = "Voice";
		_soundPlayer.Play();
	}


	private void OnHSlidEffectsValueChanged(float value)
	{
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Effects"), Mathf.Log((float)GetNode<HSlider>("HSlidEffects").Value)*20);
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("BadHeart"), Mathf.Log((float)GetNode<HSlider>("HSlidEffects").Value)*20);
		EmitSignal(nameof(SettingsChanged));
		if (_refreshing)
		{
			return;
		}
		_soundPlayer.Stream = _effectsSample;
		_soundPlayer.Bus = "Effects";
		_soundPlayer.Play();
	}


	private void OnHSlidMusicValueChanged(float value)
	{
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), Mathf.Log((float)GetNode<HSlider>("HSlidMusic").Value)*20);
		EmitSignal(nameof(SettingsChanged));
		if (_refreshing)
		{
			return;
		}
		_soundPlayer.Stream = _musicSample;
		_soundPlayer.Bus = "Music";
		_soundPlayer.Play();
	}


	private void OnHSlidMasterValueChanged(float value)
	{
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), Mathf.Log((float)GetNode<HSlider>("HSlidMaster").Value)*20);
		EmitSignal(nameof(SettingsChanged));
	}

}


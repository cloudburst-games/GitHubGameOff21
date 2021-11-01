using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class AudioData : Node
{

	[Export]
	public List<AudioStream> Streams = new List<AudioStream>();

	[Export]
	public AudioManager.AudioBus Bus = AudioManager.AudioBus.Effects;

	[Export]
	public AudioManager.SoundType SoundType = AudioManager.SoundType.Positional2D;

	[Export]
	public NodePath SoundParent;

	// Note for non3D, the maximum is 24.
	[Export(PropertyHint.Range, "-80, 80, 0")]
	public float VolumeDb {get; set;} = 0;
	[Export]
	public bool StartPlaying {get; set;} = false;
	[Export]
	public bool PauseAndPlayMusic {get; set;} = false;

	// Emitted when the FIRST INSTANCE of the sound being played finishes. E.g. if we have lots of gun sounds, the first one that finishes will trigger this signal.
	[Signal]
	public delegate void Finished();
	private Node _lastSoundPlayer {get; set;}
	private AudioManager _audioManager;
	private Random _rand = new Random();
	
	public override void _Ready()
	{
		_audioManager = GetNode<AudioManager>("/root/AudioManager");
		// Disable looping, in case we forget to change the import settings
		foreach (AudioStream stream in Streams)
		{
			if (stream is AudioStreamOGGVorbis ogg)
			{
				ogg.Loop = false;
			}
			if (stream is AudioStreamMP3 mp3)
			{
				mp3.Loop = false;
			}
			if (stream is AudioStreamSample wav)
			{
				wav.LoopMode = AudioStreamSample.LoopModeEnum.Disabled;
			}
		}
		if (SoundType != AudioManager.SoundType.Positional3D)
		{
			VolumeDb = Math.Min(VolumeDb, 24);
		}
	}

	public void OnFinished()
	{
		EmitSignal(nameof(Finished));
	}

	public void StopLastSoundPlayer()
	{
		if (_lastSoundPlayer == null)
		{
			return;
		}
		if (_lastSoundPlayer is AudioStreamPlayer audio)
		{
			audio.Stop();
		}
		if (_lastSoundPlayer is AudioStreamPlayer2D audioStreamPlayer2D)
		{
			audioStreamPlayer2D.Stop();
		}
		if (_lastSoundPlayer is AudioStreamPlayer3D audioStreamPlayer3D)
		{
			audioStreamPlayer3D.Stop();
		}
		_lastSoundPlayer.QueueFree();
		_lastSoundPlayer = null;
	}

	public bool Playing()
	{
		if (_lastSoundPlayer == null)
		{
			return false;
		}
		if (_lastSoundPlayer is AudioStreamPlayer audio)
		{
			if (audio.Playing)
			{
				return true;
			}
		}
		if (_lastSoundPlayer is AudioStreamPlayer2D audioStreamPlayer2D)
		{
			if (audioStreamPlayer2D.Playing)
			{
				return true;
			}
		}
		if (_lastSoundPlayer is AudioStreamPlayer3D audioStreamPlayer3D)
		{
			if (audioStreamPlayer3D.Playing)
			{
				return true;
			}
		}
		return false;
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		if (StartPlaying)
		{
			// Randomise if more than one stream
			if (SoundType != AudioManager.SoundType.Music && Streams.Count > 1)
			{
				Streams = Streams.OrderBy(a => _rand.Next()).ToList();
			}
			_lastSoundPlayer = _audioManager.PlaySound(this);
			StartPlaying = false;
			return;
		}
		if (PauseAndPlayMusic)
		{
			_audioManager.MusicPauseAndPlayNext(this);
			PauseAndPlayMusic = false;
		}
	}

}

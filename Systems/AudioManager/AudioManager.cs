using Godot;
using System;
using System.Collections.Generic;

public class AudioManager : Node
{

	public enum AudioBus {Voice, Effects, Music}
	public enum SoundType {Positional2D, Positional3D, NonPositional, Music}
	private Dictionary<AudioBus, string> _soundBusStrings = new Dictionary<AudioBus, string>()
	{
		{AudioBus.Voice, "Voice"},
		{AudioBus.Effects, "Effects"},
		{AudioBus.Music, "Music"}
	};

	private MusicPlayer _currentMusicPlayer;

	private Node MakePlayer(AudioData audioData)
	{
		Node soundPlayer;
		if (audioData.SoundType == SoundType.Music)
		{
			MakeMusicPlayer(audioData);
			soundPlayer = _currentMusicPlayer;
		}
		else if (audioData.SoundType == SoundType.NonPositional)
		{
			soundPlayer = new AudioStreamPlayer();
			((AudioStreamPlayer)soundPlayer).Bus = _soundBusStrings[audioData.Bus];
			((AudioStreamPlayer)soundPlayer).VolumeDb = audioData.VolumeDb;
		}
		else if (audioData.SoundType == SoundType.Positional2D)
		{
			soundPlayer = new AudioStreamPlayer2D();
			((AudioStreamPlayer2D)soundPlayer).Bus = _soundBusStrings[audioData.Bus];
			((AudioStreamPlayer2D)soundPlayer).VolumeDb = audioData.VolumeDb;
		}
		else
		{
			soundPlayer = new AudioStreamPlayer3D();
			((AudioStreamPlayer3D)soundPlayer).Bus = _soundBusStrings[audioData.Bus];
			((AudioStreamPlayer3D)soundPlayer).UnitDb = audioData.VolumeDb;
		}
		if (audioData.SoundParent == null || ! audioData.HasNode(audioData.SoundParent))
		{
			GD.Print("No sound parent, or path to specified parent not found. Adding as child of AudioManager.");
			AddChild(soundPlayer);
		}
		else
		{
			audioData.GetNode(audioData.SoundParent).AddChild(soundPlayer);
		}
		soundPlayer.Connect("finished", audioData, nameof(AudioData.OnFinished));
		return soundPlayer;
	}


	public Node PlaySound(AudioData audioData)
	{
		// Error checking
		if (audioData.Streams.Count == 0)
		{
			GD.Print("No streams in the AudioData. Aborting sound player.");
			return null;
		}

		Node soundPlayer = MakePlayer(audioData);

		if (soundPlayer is MusicPlayer musicPlayer)
		{
			StartPlayMusic(musicPlayer, audioData);
		}
		else
		{
			if (soundPlayer is AudioStreamPlayer nonPositionalPlayer)
			{
				nonPositionalPlayer.Stream = audioData.Streams[0];
				nonPositionalPlayer.Play();
			}
			else if (soundPlayer is AudioStreamPlayer2D positional2DPlayer)
			{
				positional2DPlayer.Stream = audioData.Streams[0];
				positional2DPlayer.Play();
			}
			else if (soundPlayer is AudioStreamPlayer3D positional3DPlayer)
			{
				positional3DPlayer.Stream = audioData.Streams[0];
				positional3DPlayer.Play();
			}
			soundPlayer.Connect("finished", soundPlayer, "queue_free");
		}
		return soundPlayer;		
	}

	private void StartPlayMusic(MusicPlayer musicPlayer, AudioData audioData)
	{
		if (audioData.Streams.Count > 1)
		{
			musicPlayer.StartPlaylist(audioData.Streams);
		}
		else
		{
			musicPlayer.Stream = audioData.Streams[0];
			musicPlayer.Play();
		}
	}

	private void MakeMusicPlayer(AudioData audioData)
	{
		if (_currentMusicPlayer != null)
		{
			_currentMusicPlayer.QueueFree();
		}
		_currentMusicPlayer = new MusicPlayer();
		_currentMusicPlayer.Connect("finished", _currentMusicPlayer, nameof(MusicPlayer.OnMusicPlayerFinished));
		_currentMusicPlayer.Bus = _soundBusStrings[audioData.Bus];
		_currentMusicPlayer.VolumeDb = audioData.VolumeDb;
		_currentMusicPlayer.StartingVolumeDb = audioData.VolumeDb;
	}

	public void MusicPauseAndPlayNext(AudioData audioData)
	{
		if (audioData.SoundType != SoundType.Music)
		{
			GD.Print("Error: audio data is not of Music type.");
			return;
		}
		if (_currentMusicPlayer == null)
		{
			// MakeMusicPlayer(audioData);
			MakePlayer(audioData);
			_currentMusicPlayer.Stream = audioData.Streams[0];
			_currentMusicPlayer.Play();
			return;
		}
        // if (!_currentMusicPlayer.Playing)
        // {
		// 	_currentMusicPlayer.Stream = audioData.Streams[0];
		// 	_currentMusicPlayer.Play();
        //     GD.Print("here 1");
        //     return;
        // }
		_currentMusicPlayer.PauseAndPlayNext(audioData.Streams[0]);
	}

	public void FadeAndStopMusic()
	{
		_currentMusicPlayer.FadeThenStop(true);
	}
}

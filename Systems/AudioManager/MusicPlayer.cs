using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class MusicPlayer : AudioStreamPlayer
{
	private Tween _musicTween;
	public float StartingVolumeDb {get; set;} = 0;

	public float FadeDuration {get; set;}= 1f;

	private Random _rand = new Random();

	private List<AudioStream> _currentPlaylist;

	private List<AudioStream> _finishedPlaylist = new List<AudioStream>();

	private bool _playlistActive = false;

	private bool _stop = false;

	public Dictionary<AudioStream, float> PausedMusic = new Dictionary<AudioStream, float>();

	public override void _Ready()
	{
		Tween t = new Tween();
		AddChild(t);
		_musicTween = t;
	}

	public override void _Process(float delta)
	{
		if (Stream == null)
		{
			return;
		}
		float timeLeft = Stream.GetLength() - GetPlaybackPosition();
		if (timeLeft <= FadeDuration && timeLeft > 0.1f && !_musicTween.IsActive())
		{
			FadeOut();
		}
	}

	// this works without playlist - if you want to manually control music being played e.g. based on entering an area
	public async void PauseAndPlayNext(AudioStream newStream)
	{
		
		if (Stream != null)
		{
			if (PausedMusic.ContainsKey(Stream))
			{
				PausedMusic.Remove(Stream);
			}
			PausedMusic[Stream] = GetPlaybackPosition();
		}
		
		FadeOut();
		await ToSignal(_musicTween, "tween_completed");
		Stop();
		Stream = newStream;		
		if (PausedMusic.ContainsKey(newStream))
		{
			FadeIn();
			this.Play(PausedMusic[newStream]);
		}
		else
		{
			this.Play();
		}
	}

	public void FadeIn()
	{
		_musicTween.InterpolateProperty(this, "volume_db", -50, StartingVolumeDb, FadeDuration, Tween.TransitionType.Linear, Tween.EaseType.Out);
		_musicTween.Start();
	}

	public void FadeOut(float volDB = -50)
	{
		_musicTween.InterpolateProperty(this, "volume_db", StartingVolumeDb, volDB, FadeDuration, Tween.TransitionType.Linear, Tween.EaseType.In);
		_musicTween.Start();
	}

	public async void FadeThenStop(bool freePlayer = false)
	{
		_stop = true;
		_playlistActive = false;
		FadeOut();
		await ToSignal(_musicTween, "tween_completed");
		Stop();
		if (freePlayer)
		{
			QueueFree();
		}
	}


	public new void Play(float from = 0)
	{
		_stop = false;
		_musicTween.StopAll();
		if (from == 0)
		{
			VolumeDb = StartingVolumeDb;
		}
		base.Play(from);
	}

	public void StartPlaylist(List<AudioStream> streams)
	{
		_playlistActive = true;
		_currentPlaylist = streams;
		// consider adding a shuffle here if we want to make it random each time
		PlayNext();
	}

	private void PlayNext()
	{
		GD.Print("playing next");
		Stream = _currentPlaylist[0];
		
		Play();

		_finishedPlaylist.Add(Stream);
		_currentPlaylist.Remove(Stream);

		if (_currentPlaylist.Count == 0)
		{
			GD.Print("finished");
			_currentPlaylist = _finishedPlaylist.ToList();
			_finishedPlaylist.Clear();
		}
	}

	public void StopPlaylist()
	{
		_playlistActive = false;
		_finishedPlaylist.Clear();
		_currentPlaylist.Clear();
	}

	public void OnMusicPlayerFinished()
	{
		if (_stop)
		{
			return;
		}
		if (_playlistActive)
		{
			PlayNext();
			return;
		}
		Play();
	}

}

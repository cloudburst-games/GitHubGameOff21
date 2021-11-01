// ** PictureStory class to provide narrative in "slide show" format with picture and text **
//	// Usage is:
//	// Reference the node
//	PictureStory pictureStory = GetNode<PictureStory>("PathToNode");
//	// Subscribe to the finished event:
//	pictureStory.PictureStoryFinished+=this.MyMethod;
//	// (Optional) Can set to stay paused on completing the picture story:
//	_pictureStory.StayPausedOnFinish = true;
//	// (Optional) Can set to NOT fade out on completing the picture story, e.g. if you want to transition to a new stage straight away:
//	_pictureStory.FadeOutOnFinish = false;
//	// (Optional) Can set to remain visible on finish - if you want it to stay black whilst transitioning onto a new stage
//	_pictureStory.VisibleOnFinish = true;
//	// Add screens
//	_pictureStory.AddScreen("pathToBackground", "text");
//	_pictureStory.AddScreen("pathToBackground2", "text2");
// // Can also add video instead, don't forget to pause/stop any music if the video has music you want to play
// 	_pictureStory.AddScreenVideo("res://movie.ogv");
//	// Start the picture story
//	_pictureStory.Start();
// // If we want to re-use this node, we need to call the Reset method
// 	_pictureStory.Reset();

using Godot;
using System;
using System.Collections.Generic;

public class PictureStory : Control
{	

	public delegate void PictureStoryFinishedDelegate();
	public event PictureStoryFinishedDelegate PictureStoryFinished;

	private TextureRect _background;
	private VideoPlayer _video;
	private Label _lblText;
	private Control _prev;
	private Control _current;
	private TextureRect _prevBackground;
	private VideoPlayer _prevVideo;
	private Label _prevLblText;
	// Enable this if we want it to continue being paused afterwards, e.g. at the end of the game
	public bool StayPausedOnFinish {get; set;} = false;
	// Disable this if we do NOT want to fade out at the end
	public bool FadeOutOnFinish {get; set;} = true;
	public bool VisibleOnFinish {get; set;} = false;

	private AnimationPlayer _anim;
	private AudioStreamPlayer _soundPlayer;
	private List<PictureStoryUnit> _screens = new List<PictureStoryUnit>();

	public override void _Ready()
	{
		// Testing (when testing this node in isolation, uncomment the below)
		// AddScreen("res://Common/UI/Textures/PlaceholderBG.png", "Wallowing in penury, you come across the famed Leprechaun Elixir.");
		// AddScreen("res://icon.png", "This is your last chance to escape destitution and find your fortune!");
		// AddScreen("res://Common/UI/Textures/PlaceholderBG.png", "With only a brief moment of hesitation, you gulp down the bitter-tasting potion and soon find yourself in a strange yet wonderous land.");
		
		_background = GetNode<TextureRect>("Current/Background");
		_video = GetNode<VideoPlayer>("Current/Video");
		_lblText = GetNode<Label>("Current/LblText");
		_prev = GetNode<Control>("Prev");
		_current = GetNode<Control>("Current");
		_prevBackground = GetNode<TextureRect>("Prev/Background");
		_prevVideo = GetNode<VideoPlayer>("Prev/Video");
		_prevLblText = GetNode<Label>("Prev/LblText");
		_anim = GetNode<AnimationPlayer>("Anim");
		_soundPlayer = GetNode<AudioStreamPlayer>("SoundPlayer");
		// GD.Print(_lblText.RectSize);
		// By default, when inactive, the PictureStory node should not accept input
		SetProcessInput(false);

		// Testing (when testing this node in isolation, uncomment the below)
		// Start();

	}

	public void AddScreen(string pathToBackground, string text, AudioStream audio = null, Label.VAlign vAlign = Label.VAlign.Bottom, Label.AlignEnum hAlign = Label.AlignEnum.Center, float[] regionRect = null)
	{
		// GD.Print(_lblText.RectSize);
		_screens.Add(new PictureStoryUnit() {Background = (Texture)GD.Load(pathToBackground), Text = text, Audio = audio, Video = null, TextVAlign = vAlign, TextHAlign = hAlign, RegionRect = regionRect});
		// GD.Print(_lblText.RectSize);
	}

	// overrides the color for the last screen in the list
	public void OverrideColor(Color fontColor, Color borderColor)
	{
		// add_color_override("font_color", Color(1,1,1,1))
		_screens[_screens.Count-1].FontColorOverride = fontColor;
		_screens[_screens.Count-1].FontBorderColorOverride = borderColor;
	}

	// private bool _isVideo = false;

	public void AddScreenVideo(string pathToVideo)
	{
		// _isVideo = true;
		_screens.Add(new PictureStoryUnit() {Background = null, Text = null, Audio = null, Video = (VideoStream)GD.Load(pathToVideo)});
	}

	public void ClearScreens()
	{
		// GD.Print(_lblText.RectSize);
		_screens.Clear();
		// GD.Print(_lblText.RectSize);
	}

	// Reset everything so we can use this picturestory node again
	public void Reset()
	{
		StayPausedOnFinish = false;;
		FadeOutOnFinish = true;
		ClearFinishedEvents();
		_background.Texture = null;
		_lblText.Text = null;
		Modulate = new Color(1,1,1,1);
		ClearScreens();
		// GD.Print(_lblText.RectSize);
	}

	public void Start()
	{
		// In case we hid this node in the engine, make sure it is visible.
		Visible = true;
		SetProcessInput(true);
		ShowNext();
		_anim.Play("FadeIn");
		GetTree().Paused = true;
	}
	public override void _Input(InputEvent ev)
	{
		if (ev.IsPressed() && !_anim.IsPlaying())
		{
			ShowNext();
		}
	}

	private void ShowNext()
	{
		// after each slide, you need to reset the label parameters, as they can be distorted by the previous set of text
		ResetLabel(_lblText);
		ResetLabel(_prevLblText);

		_prevVideo.Stream = _video.Stream;
		_prevBackground.Texture = _background.Texture;
		_prevLblText.Text = _lblText.Text;
		_prev.Show();
		_prev.Modulate = new Color(_prev.Modulate.r, _prev.Modulate.g, _prev.Modulate.b, 1);
		_current.Modulate = new Color(_current.Modulate.r, _current.Modulate.g, _current.Modulate.b, 0);

		if (_screens.Count > 0)
		{
			if (_screens[0].Video != null)
			{
				PlayNextVideo();
			}
			else
			{
				_lblText.AddColorOverride("font_color", _screens[0].FontColorOverride);
				_lblText.AddColorOverride("font_outline_modulate", _screens[0].FontBorderColorOverride);
				_background.Texture = _screens[0].Background;
				_lblText.Text = _screens[0].Text;
				_lblText.Valign = _screens[0].TextVAlign;
				_lblText.Align = _screens[0].TextHAlign;
				if (_screens[0].RegionRect != null)
				{
					_background.Texture = new AtlasTexture();
					((AtlasTexture)_background.Texture).Atlas = _screens[0].Background;
					((AtlasTexture) _background.Texture).Region = new Rect2(new Vector2(_screens[0].RegionRect[0], _screens[0].RegionRect[1]),
					new Vector2(_screens[0].RegionRect[2], _screens[0].RegionRect[3]));
					// _background.StretchMode = TextureRect.StretchModeEnum.ScaleOnExpand;
				}
				// else
				// {
					// _background.StretchMode = TextureRect.StretchModeEnum.ScaleOnExpand;
				// }
				if (_screens[0].Audio != null)
				{
					// AudioHandler.PlaySound(_soundPlayer, _screens[0].Audio, AudioData.SoundBus.Voice);
					// TODO - redo how sound is handled for picturestory
				
				}
			}
			_screens.RemoveAt(0);
			_anim.Play("FadeInOut");
		}
		else
		{
			Finish();
		}
	}

	private async void PlayNextVideo()
	{
		_video.Stream = _screens[0].Video;
		_video.Play();

		await ToSignal(_video, "finished");

		ShowNext();
	}

	private void ResetLabel(Label lbl)
	{
		lbl.Text = "";
		lbl.MarginBottom = 523;
		lbl.MarginRight = 947;
		lbl.MarginTop = 10;
		lbl.MarginLeft = 10;
		lbl.RectPosition = new Vector2(10,10);
		lbl.RectSize = new Vector2(GetViewportRect().Size.x-23, GetViewportRect().Size.y-27);
	}

	private async void Finish()
	{
		
		SetProcessInput(false);
		GetTree().Paused = StayPausedOnFinish; // false by default

		if (FadeOutOnFinish)
		{
			_anim.Play("FadeOut");
			await ToSignal (_anim, "animation_finished");
		}
		Visible = VisibleOnFinish;
		PictureStoryFinished?.Invoke();
	}

	public void ClearFinishedEvents()
	{
		PictureStoryFinished = null;
	}

}

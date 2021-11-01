// Stage menu: simple menu script connecting local and networked game signals to our scene manager.
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class StageMainMenu : Stage
{
	private PictureStory _pictureStory;
	private Panel _popAbout;
	private PnlSettings _pnlSettings;
	private Button _firstLevelButton;

	private bool _gameDataExists = false;

	public override void _Ready()
	{
		base._Ready();
		_popAbout = GetNode<Panel>("PopAbout");
		_pictureStory = GetNode<PictureStory>("PictureStory");
		_pnlSettings = GetNode<PnlSettings>("PnlSettings");
		_pnlSettings.Visible = false;

		if (OS.HasFeature("HTML5"))
		{
			GetNode<Button>("HBoxContainer/BtnQuit1").Visible = false;
			GetNode<Button>("HBoxContainer/BtnToggleMute").Visible = true;
			_pnlSettings.GetNode<CheckBox>("CntPanels/PnlGraphics/CBoxFullScreen").Visible = false;
			_pnlSettings.GetNode<Label>("CntPanels/PnlGraphics/LblTitle").Text = "Not available in HTML5 version.";
		}
				
		_popAbout.Visible = false;
		GetNode<Button>("HBoxContainer/BtnToggleMute").Text = AudioServer.IsBusMute(AudioServer.GetBusIndex("Master")) ? "Unmute" : "Mute";
		
	}



	private void OnBtnNewPressed()
	{
		
		SceneManager.SimpleChangeScene(SceneData.Stage.World);
	}

	private void Intro() // this is the new game intro
	{
		_pictureStory.Reset();
		_pictureStory.PictureStoryFinished+=this.OnIntroFinished;
		_pictureStory.FadeOutOnFinish = false;
		_pictureStory.VisibleOnFinish = true;
		_pictureStory.AddScreen("res://icon.png", 
		@"intro.");
		_pictureStory.OverrideColor(new Color(0,0,0), new Color(0,0,0));
		_pictureStory.Start();
	}

	private void OnIntroFinished()
	{
		SceneManager.SimpleChangeScene(SceneData.Stage.World);
	}


	private void OnBtnAboutPressed()
	{
		GetNode<Panel>("PopAbout").Visible = true;
	}

	private void OnBtnBackPressed()
	{
		GetNode<Panel>("PopAbout").Visible = false;
	}

	public override void _Input(InputEvent ev)
	{
		if (_popAbout.Visible && ev is InputEventMouseButton evMouseButton && ev.IsPressed())
		{
			if (! (evMouseButton.Position.x > _popAbout.RectGlobalPosition.x && evMouseButton.Position.x < _popAbout.RectSize.x + _popAbout.RectGlobalPosition.x
			&& evMouseButton.Position.y > _popAbout.RectGlobalPosition.y && evMouseButton.Position.y < _popAbout.RectSize.y + _popAbout.RectGlobalPosition.y) )
			{
				OnBtnBackPressed();
			}
		}
	}

	private void _on_BtnQuit_pressed()
	{
		GetTree().Quit();
	}

	private void ToggleMute()
	{
		bool muted = AudioServer.IsBusMute(AudioServer.GetBusIndex("Master"));
		AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), !muted);
		string label = muted ? "Mute" : "Unmute";
		GetNode<Button>("HBoxContainer/BtnToggleMute").Text = label;
	}

	private void OnGameLinkButtonPressed()
	{
		OS.ShellOpen("https://sage7.itch.io/");
	}


	private void OnBtnSettingsPressed()
	{
		_pnlSettings.Visible = true;
	}

	private void OnBtnTexBackgroundPressed()
	{
		_pnlSettings.OnBtnClosePressed();
		_popAbout.Visible = false;
	}

}


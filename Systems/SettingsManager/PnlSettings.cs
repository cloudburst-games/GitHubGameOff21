using Godot;
using System;
using System.Collections.Generic;

// TODO:
// Store the input maps in 3 separate dicts (see below) in JSON
// Retrive the input maps from the JSON file
// Write the readme and integrate this into shmup

public class PnlSettings : Panel
{
	private SettingsLoadSaveHandler _loadSaveHandler = new SettingsLoadSaveHandler();
	private bool _changesSaved = true;
	public bool ChangesSaved {
		get{
			return _changesSaved;
		}
		set{
			_changesSaved = value;
			GetNode<Button>("BtnApply").Disabled = value;
		}
	}

	[Signal]
	public delegate void FinalClosed();

	public override void _Ready()
	{
        //
		OnBtnGamePressed();
        //
		GetNode<Panel>("PnlConfirmExit").Visible = false;
		ConnectControlsSignals();
		ConnectAudioSignals();
		ConnectGraphicsSignals();
		ConnectGameSignals();
		ChangesSaved = true;

		LoadAndRefreshSettings();
		
	}

	private void LoadAndRefreshSettings()
	{
		if (!_loadSaveHandler.LoadFromFile())
		{
			OS.WindowFullscreen = true;
            GetNode<OptionButton>("CntPanels/PnlGame/HBoxContainer/BtnDifficulty").Selected = 1; // default
		}
        else
        {
            // GetNode<PnlSettingsGame>("CntPanels/PnlGame").RefreshSettings();
        }
		GetNode<PnlControls>("CntPanels/PnlControls").RefreshBtnMapText();
		GetNode<PnlAudio>("CntPanels/PnlAudio").RefreshHSlidValues();
		GetNode<PnlGraphics>("CntPanels/PnlGraphics").RefreshSettings();

		// 
	}

	private void ConnectControlsSignals()
	{
		foreach (Node n in GetNode("CntPanels/PnlControls").GetChildren())
		{
			foreach (Node m in n.GetChildren())
			{
				BtnRemap btnRemap = m.GetNode<BtnRemap>("BtnRemap");
				btnRemap.Connect(nameof(BtnRemap.SettingsChanged),this,nameof(OnSettingsChanged));
				btnRemap.Connect(nameof(BtnRemap.SettingsChanged),GetNode<PnlControls>("CntPanels/PnlControls"),nameof(PnlControls.RefreshBtnMapText));
				btnRemap.Connect(nameof(BtnRemap.OtherActionRemapped), GetNode<PnlControls>("CntPanels/PnlControls"), nameof(PnlControls.OnOtherBtnRemapped));
			}
		}
	}

	private void ConnectGraphicsSignals()
	{
		GetNode<PnlGraphics>("CntPanels/PnlGraphics").Connect(nameof(PnlGraphics.SettingsChanged), this, nameof(OnSettingsChanged));
	}

	private void ConnectAudioSignals()
	{
		GetNode<PnlAudio>("CntPanels/PnlAudio").Connect(nameof(PnlAudio.SettingsChanged), this, nameof(OnSettingsChanged));
	}
	private void ConnectGameSignals()
	{
		GetNode<PnlSettingsGame>("CntPanels/PnlGame").Connect(nameof(PnlSettingsGame.SettingsChanged), this, nameof(OnSettingsChanged));
        // Or difficulty setting selected
        // GetNode<PnlSettingsGame>("CntPanels/PnlGame").Connect(nameof(PnlSettingsGame.DifficultySelected), this, nameof(OnDifficultySelected));
        // Difficulty setting loaded
        _loadSaveHandler.DifficultySelected+=this.OnDifficultySelected;
	}

    private void OnDifficultySelected(int difficulty)
    {
        GetNode<OptionButton>("CntPanels/PnlGame/HBoxContainer/BtnDifficulty").Selected = difficulty;
    }

	private void OnSettingsChanged()
	{
		ChangesSaved = false;
	}

	private void OnBtnControlsPressed()
	{
		EnableSingleSettingsPanel(GetNode<Panel>("CntPanels/PnlControls"));
		EnableAllExceptOneButton(GetNode<Button>("VBoxBtns/BtnControls"));

	}
	private void OnBtnGamePressed()
	{
		EnableSingleSettingsPanel(GetNode<Panel>("CntPanels/PnlGame"));
		EnableAllExceptOneButton(GetNode<Button>("VBoxBtns/BtnGame"));

	}
	private void OnBtnAudioPressed()
	{
		EnableSingleSettingsPanel(GetNode<Panel>("CntPanels/PnlAudio"));
		EnableAllExceptOneButton(GetNode<Button>("VBoxBtns/BtnAudio"));
	}


    public bool CursorInsidePanel()
    {
        return GetGlobalMousePosition().x > RectGlobalPosition.x && GetGlobalMousePosition().x < RectGlobalPosition.x + RectSize.x
            && GetGlobalMousePosition().y > RectGlobalPosition.y && GetGlobalMousePosition().y < RectGlobalPosition.y + RectSize.y;
    }
    
	private void OnBtnGraphicsPressed()
	{
		EnableSingleSettingsPanel(GetNode<Panel>("CntPanels/PnlGraphics"));
		EnableAllExceptOneButton(GetNode<Button>("VBoxBtns/BtnGraphics"));
	}

	private void EnableSingleSettingsPanel(Panel p)
	{
		foreach (Panel panel in GetNode<Node>("CntPanels").GetChildren())
		{
			if (panel is PnlControls pnlControls)
			{
				pnlControls.Visible = false;
				continue;
			}
			panel.Visible = false;
		}
		p.Visible = true;
	}

	private void EnableAllExceptOneButton(Button button)
	{
		foreach (Button b in GetNode<VBoxContainer>("VBoxBtns").GetChildren())
		{
			b.Disabled = false;
		}
		button.Disabled = true;
	}

	private void OnBtnApplyPressed()
	{
		_loadSaveHandler.SaveToFile(GetNode<OptionButton>("CntPanels/PnlGame/HBoxContainer/BtnDifficulty").Selected);
		ChangesSaved = true;
	}


	public void OnBtnClosePressed()
	{
		if (ChangesSaved)
		{
			Visible = false;
			EmitSignal(nameof(PnlSettings.FinalClosed));
			return;
		}
		GetNode<Panel>("PnlConfirmExit").Visible = true;
		
	}

	private void OnBtnConfirmClosePressed()
	{
		LoadAndRefreshSettings();
		ChangesSaved = true;
		Visible = false;
		EmitSignal(nameof(PnlSettings.FinalClosed));
		GetNode<Panel>("PnlConfirmExit").Visible = false;
	}


	private void OnBtnCancelPressed()
	{
		GetNode<Panel>("PnlConfirmExit").Visible = false;
	}

    public void OnDie()
    {
        _loadSaveHandler.OnDie();
    }
}


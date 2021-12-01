using Godot;
using System;
using System.Collections.Generic;

public class HUD : CanvasLayer
{
    public Dictionary<LevelManager.Level, string> LevelNames {get; set;} = new Dictionary<LevelManager.Level, string>()
    {
        {LevelManager.Level.Level1, "level 1"},
        {LevelManager.Level.Level2, "level 2"},
        {LevelManager.Level.Level3, "level 3"},
        {LevelManager.Level.Level4, "level 4"},
        {LevelManager.Level.Level5, "level 5"},
    };
    public bool Pausable {get; set;} = true;
    public override void _Ready()
    {
        GetNode<Panel>("CtrlTheme/PnlMenu").Visible = GetNode<ColorRect>("CtrlTheme/PauseRect").Visible = 
            GetNode<Control>("CtrlTheme/ProgressAnim").Visible = GetNode<Control>("CtrlTheme/DialogueControl").Visible =
            GetNode<Panel>("CtrlTheme/PnlDefeat").Visible = GetNode<Panel>("CtrlTheme/PnlEventsBig").Visible = false;

        GetNode<DialogueControl>("CtrlTheme/DialogueControl").Connect(nameof(DialogueControl.DialogueEnded), this, nameof(OnDialogueEnded));
        GetNode<DialogueControl>("CtrlTheme/DialogueControl").Connect(nameof(DialogueControl.JournalUpdatedSignal), this, nameof(OnJournalUpdated));
        GetNode<DialogueControl>("CtrlTheme/DialogueControl").Connect(nameof(DialogueControl.GameEnded), this, nameof(OnGameEnded));
        GetNode<Journal>("CtrlTheme/DialogueControl/Journal").Connect(nameof(Journal.ClosedJournal), this, nameof(OnBtnJournalClosePressed));
        GetNode<DialogueControl>("CtrlTheme/DialogueControl").Connect(nameof(DialogueControl.MainQuestChanged), this, nameof(OnMainQuestChanged));
        // fix position bugs
        GetNode<DialogueControl>("CtrlTheme/DialogueControl").RectPosition = new Vector2(-960,540);
        GetNode<Journal>("CtrlTheme/DialogueControl/Journal").RectGlobalPosition = new Vector2(0,0);
        // connect journal updated to below
        GetNode<Control>("CtrlTheme/CntBtnUnspentPoints").Visible = false;
        GetNode<Panel>("CtrlTheme/PnlVictory").Visible = false;
        GetNode<Label>("CtrlTheme/LblShowLevelName").Visible = false;
    }
    private void OnJournalUpdated()
    {
        LogEntry("Your journal has been updated.");
        GetNode<AudioData>("CtrlTheme/DialogueControl/AudioData").StartPlaying = true;
    }

    public void OnMainQuestChanged(string mainQuest)
    {
        // GetNode<AnimationPlayer>("CtrlTheme/LblMainQuest/Anim").Play("FadeOut");
        // await ToSignal(GetNode<AnimationPlayer>("CtrlTheme/LblMainQuest/Anim"), "animation_finished");

        GetNode<Label>("CtrlTheme/LblMainQuest").Text = mainQuest;
        // GD.Print("main quest: ", mainQuest);

        GetNode<AnimationPlayer>("CtrlTheme/LblMainQuest/Anim").Play("FadeIn");

    }

    public async void OnGameEnded(bool joinMahef)
    {
        PauseCommon(true);
        Pausable = false;
        GetNode<Label>("CtrlTheme/PnlVictory/LblBody0").Visible = joinMahef;
        GetNode<Label>("CtrlTheme/PnlVictory/LblBody1").Visible = !joinMahef;
        GetNode<AnimationPlayer>("CtrlTheme/PnlVictory/Anim").Play("Start");

        Timer timer = new Timer();
        timer.WaitTime = 0.1f;
        AddChild(timer);
        timer.Start();
        await ToSignal(timer, "timeout");
        timer.QueueFree();
        PauseCommon(true);
        Pausable = false;
    }

    private void OnBtnJournalClosePressed()
    {
        Pausable = true;
        GetNode<DialogueControl>("CtrlTheme/DialogueControl").Visible = false;
        PauseCommon(false);
    }

    public void OnAttributePointsUnspent()
    {
        GetNode<AnimationPlayer>("CtrlTheme/CntBtnUnspentPoints/BtnUnspentPoints/AnimAppear").Play("FadeIn");
    }

    public void OnSpentAttributePoints()
    {
        GetNode<AnimationPlayer>("CtrlTheme/CntBtnUnspentPoints/BtnUnspentPoints/AnimAppear").Play("FadeOut");
    }

    public void OnBtnEventsPressed()
    {
        PauseCommon(true);
        Pausable = false;
        GetNode<Panel>("CtrlTheme/PnlEventsBig").Visible = true;
    }

    public void OnBtnCloseEventsPressed()
    {
        PauseCommon(false);
        Pausable = true;
        GetNode<Panel>("CtrlTheme/PnlEventsBig").Visible = false;
    }

    public void OnBtnResumePressed()
    {
        TogglePauseMenu(false);
    }

    public void OnBtnMenuPressed()
    {
        TogglePauseMenu(true);
    }

    public void PlayProgressAnim(string text)
    {
        GetNode<Label>("CtrlTheme/ProgressAnim/LblProgress").Text = text;
        GetNode<Control>("CtrlTheme/ProgressAnim").Visible = true;
        GetNode<AnimationPlayer>("CtrlTheme/ProgressAnim/Anim").Play("Load");
    }

    public void StopProgressAnim()
    {
        GetNode<Control>("CtrlTheme/ProgressAnim").Visible = false;
        GetNode<AnimationPlayer>("CtrlTheme/ProgressAnim/Anim").Stop();
    }

    public void LogEntry(string text, bool record=true)
    {
        if (record)
        {
            GetNode<PnlEvents>("CtrlTheme/PnlUIBar/PnlEvents").LogEntry(text);
            GetNode<RichTextLabel>("CtrlTheme/PnlEventsBig/RichTextLabel").Text += text + "\n";
        }
        else
        {
            GetNode<PnlEvents>("CtrlTheme/PnlUIBar/PnlEvents").LogEntry(text);
        }
    }


    public void OnShopBtnClosePressed()
    {
        PauseCommon(false);
        Pausable = true;
        GetNode<PnlShopScreen>("CtrlTheme/PnlShopScreen").Visible = false;
    }

    public void TogglePauseMenu(bool pause)
    {
        GetNode<Panel>("CtrlTheme/PnlMenu").Visible = pause;
        PauseCommon(pause);
    }
    
    public void PauseCommon(bool pause)
    {
        GetTree().Paused = pause;
        GetNode<ColorRect>("CtrlTheme/PauseRect").Visible = pause;
        foreach (Button btn in GetNode("CtrlTheme/PnlUIBar/HBoxBtns").GetChildren())
        {
            btn.Disabled = pause;
        }
        GetNode<Button>("CtrlTheme/PnlUIBar/PnlEvents/HBoxContainer/BtnEvents").Disabled = pause;
    }

    public void ShowDefeatMenu()
    {
        PauseCommon(true);
        GetNode<Panel>("CtrlTheme/PnlDefeat").Visible = true;
    }

    public void StartDialogue(UnitData unitData, UnitData khepriUnitData)
    {
        // GetNode<AudioData>("CtrlTheme/DialogueControl/AudioDataInitiateDialogue").StartPlaying = true;
        PauseCommon(true);
        Pausable = false;
        GetNode<DialogueControl>("CtrlTheme/DialogueControl").Visible = true;
        GetNode<DialogueControl>("CtrlTheme/DialogueControl").Start(unitData, khepriUnitData);
    }
    public void OnNPCRightClicked(Unit npc)
    {
        int difficulty = GetNode<OptionButton>("CtrlTheme/CanvasLayer/PnlSettings/CntPanels/PnlGame/HBoxContainer/BtnDifficulty").Selected;
        
        GetNode<NPCInfoPanel>("CtrlTheme/NPCInfoPanel").Activate(npc.CurrentUnitData, difficulty);
    }
    public void OnDialogueEnded()
    {
        PauseCommon(false);
        Pausable = true;
        GetNode<DialogueControl>("CtrlTheme/DialogueControl").Visible = false;
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("Pause") && !IsAnyWindowVisible())
        {
            TogglePauseMenu(!GetTree().Paused);
        }
        else
        {
            if (!IsAnyWindowVisible())
            {
                if (Input.IsActionJustPressed("Event Log") && !GetNode<Panel>("CtrlTheme/PnlEventsBig").Visible)
                {
                    OnBtnEventsPressed();
                }
                else if (Input.IsActionJustPressed("Map") && !GetNode<Map>("CtrlTheme/Map").Visible)
                {
                    EmitSignal(nameof(RequestingMap));
                }
                else if (Input.IsActionJustPressed("Journal") && !GetNode<Journal>("CtrlTheme/DialogueControl/Journal").Visible)
                {
                    EmitSignal(nameof(RequestingJournal));
                }
            }
            else
            {
                if (Input.IsActionJustPressed("Pause") && GetNode<Panel>("CtrlTheme/PnlEventsBig").Visible)
                {
                    OnBtnCloseEventsPressed();
                }
                else if (Input.IsActionJustPressed("Pause") && GetNode<PnlCharacterManager>("CtrlTheme/PnlCharacterManager").Visible)
                {
                    GetNode<PnlCharacterManager>("CtrlTheme/PnlCharacterManager").OnBtnClosePressed();
                }
                else if (Input.IsActionJustPressed("Pause") && GetNode<Map>("CtrlTheme/Map").Visible)
                {
                    OnBtnMapClosePressed();
                }
                else if (Input.IsActionJustPressed("Pause") && GetNode<Journal>("CtrlTheme/DialogueControl/Journal").Visible)
                {
                    GetNode<Journal>("CtrlTheme/DialogueControl/Journal").OnExitButtonPressed();
                }
            }
        }
        
    }

    [Signal]
    public delegate void RequestingMap();
    [Signal]
    public delegate void RequestingJournal();

    public bool IsAnyWindowVisible()
    {
        return GetNode<Panel>("CtrlTheme/PnlEventsBig").Visible || GetNode<PnlCharacterManager>("CtrlTheme/PnlCharacterManager").Visible || 
            GetNode<Map>("CtrlTheme/Map").Visible || GetNode<Journal>("CtrlTheme/DialogueControl/Journal").Visible || 
            GetNode<FileDialog>("CtrlTheme/FileDialog").Visible || GetNode<Panel>("CtrlTheme/PnlBattleVictory").Visible ||
            GetNode<Panel>("CtrlTheme/PnlDefeat").Visible || GetNode<DialogueControl>("CtrlTheme/DialogueControl").Visible || 
            GetNode<Panel>("CtrlTheme/PnlVictory").Visible || GetNode<PnlPreBattle>("CtrlTheme/PnlPreBattle").Visible ||
            GetNode<Panel>("CtrlTheme/PnlVictory").Visible || GetNode<PnlSettings>("CtrlTheme/CanvasLayer/PnlSettings").Visible;
    }

    public override void _Input(InputEvent ev)
    {
        base._Input(ev);
        // else if (Input.IsActionJustPressed("Pause") && GetNode<PnlShopScreen>("CtrlTheme/PnlShopScreen").Visible)
        // {
        //     OnShopBtnClosePressed();
        // }
        // if (Input.IsActionJustPressed("Event Log") && GetNode<Panel>("CtrlTheme/PnlEventsBig").Visible)
        // {
        //     OnBtnCloseEventsPressed();
        // }
        // else if (Input.IsActionJustPressed("Pause") && GetNode<Journal>("CtrlTheme/DialogueControl/Journal").Visible)
        // {
        //     GetNode<Journal>("CtrlTheme/DialogueControl/Journal").OnExitButtonPressed();
        // }
        // else if (Input.IsActionJustPressed("Pause") && GetNode<Map>("CtrlTheme/Map").Visible)
        // {
        //     OnBtnMapClosePressed();
        // }
        
        if (ev is InputEventMouseButton btn)// && !(ev.IsEcho()))
        {
            if (btn.ButtonIndex == (int) ButtonList.Right)
            {
                if (!btn.Pressed)
                {
                    GetNode<NPCInfoPanel>("CtrlTheme/NPCInfoPanel").Visible = false;
                }
            }
        }
    }

    public void OnBtnMapClosePressed()
    {
        GetNode<Map>("CtrlTheme/Map").Visible = false;
        // Pausable = true;
    }

    public void OnBtnSavePressed()
    {
        GetNode<FileDialogFixed>("CtrlTheme/FileDialog").Mode = FileDialog.ModeEnum.SaveFile;
        GetNode<FileDialogFixed>("CtrlTheme/FileDialog").Access = FileDialog.AccessEnum.Userdata;
        GetNode<FileDialogFixed>("CtrlTheme/FileDialog").PopupCentered();
        GetNode<FileDialogFixed>("CtrlTheme/FileDialog").Refresh();
    }
    public void OnBtnLoadPressed()
    {
        GetNode<FileDialogFixed>("CtrlTheme/FileDialog").Mode = FileDialog.ModeEnum.OpenFile;
        GetNode<FileDialogFixed>("CtrlTheme/FileDialog").Access = FileDialog.AccessEnum.Userdata;
        GetNode<FileDialogFixed>("CtrlTheme/FileDialog").PopupCentered();
        GetNode<FileDialogFixed>("CtrlTheme/FileDialog").Refresh();
    }
}

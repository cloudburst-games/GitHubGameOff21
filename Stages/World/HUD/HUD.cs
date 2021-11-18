using Godot;
using System;
using System.Collections.Generic;

public class HUD : CanvasLayer
{
    public bool Pausable {get; set;} = true;
    public override void _Ready()
    {
        GetNode<Panel>("CtrlTheme/PnlMenu").Visible = GetNode<ColorRect>("CtrlTheme/PauseRect").Visible = 
            GetNode<Control>("CtrlTheme/ProgressAnim").Visible = GetNode<Control>("CtrlTheme/DialogueControl").Visible =
            GetNode<Panel>("CtrlTheme/PnlDefeat").Visible = GetNode<Panel>("CtrlTheme/PnlEventsBig").Visible = false;

        GetNode<DialogueControl>("CtrlTheme/DialogueControl").Connect(nameof(DialogueControl.DialogueEnded), this, nameof(OnDialogueEnded));
        GetNode<Journal>("CtrlTheme/DialogueControl/Journal").Connect(nameof(Journal.ClosedJournal), this, nameof(OnBtnJournalClosePressed));
        // fix position bugs
        GetNode<DialogueControl>("CtrlTheme/DialogueControl").RectPosition = new Vector2(-960,540);
        GetNode<Journal>("CtrlTheme/DialogueControl/Journal").RectGlobalPosition = new Vector2(0,0);
    }

    private void OnBtnJournalClosePressed()
    {
        Pausable = true;
        GetNode<DialogueControl>("CtrlTheme/DialogueControl").Visible = false;
        PauseCommon(false);
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
    }

    public void ShowDefeatMenu()
    {
        PauseCommon(true);
        GetNode<Panel>("CtrlTheme/PnlDefeat").Visible = true;
    }

    public void StartDialogue(UnitData unitData, UnitData khepriUnitData)
    {
        PauseCommon(true);
        Pausable = false;
        GetNode<DialogueControl>("CtrlTheme/DialogueControl").Visible = true;
        GetNode<DialogueControl>("CtrlTheme/DialogueControl").Start(unitData, khepriUnitData);
    }
    public void OnNPCRightClicked(Unit npc)
    {
        GetNode<NPCInfoPanel>("CtrlTheme/NPCInfoPanel").Activate(npc.CurrentUnitData);
    }
    public void OnDialogueEnded()
    {
        PauseCommon(false);
        Pausable = true;
        GetNode<DialogueControl>("CtrlTheme/DialogueControl").Visible = false;
    }

    public override void _Input(InputEvent ev)
    {
        base._Input(ev);
        if (Input.IsActionJustPressed("Pause") && Pausable)
        {
            TogglePauseMenu(!GetTree().Paused);
        }
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

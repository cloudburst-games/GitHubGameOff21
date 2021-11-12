using Godot;
using System;
using System.Collections.Generic;

public class HUD : CanvasLayer
{
    public override void _Ready()
    {
        GetNode<Panel>("CtrlTheme/PnlMenu").Visible = GetNode<ColorRect>("CtrlTheme/PauseRect").Visible = 
            GetNode<Control>("CtrlTheme/ProgressAnim").Visible = GetNode<Control>("CtrlTheme/DialogueControl").Visible = false;

        GetNode<DialogueControl>("CtrlTheme/DialogueControl").Connect(nameof(DialogueControl.DialogueEnded), this, nameof(OnDialogueEnded));
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

    public void LogEntry(string text)
    {
        GetNode<Label>("CtrlTheme/LblLog").Text = text;
        GetNode<AnimationPlayer>("CtrlTheme/LblLog/Anim").Play("LogEntry");
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

    public void StartDialogue(UnitData unitData)
    {
        PauseCommon(true);
        GetNode<DialogueControl>("CtrlTheme/DialogueControl").Visible = true;
        GetNode<DialogueControl>("CtrlTheme/DialogueControl").Start(unitData);
    }
    public void OnNPCRightClicked(Unit npc)
    {
        GetNode<NPCInfoPanel>("CtrlTheme/NPCInfoPanel").Activate(npc.CurrentUnitData);
    }
    public void OnDialogueEnded()
    {
        PauseCommon(false);
        GetNode<DialogueControl>("CtrlTheme/DialogueControl").Visible = false;
    }

    public override void _Input(InputEvent ev)
    {
        base._Input(ev);
        if (Input.IsActionJustPressed("Pause"))
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

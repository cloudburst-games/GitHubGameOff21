using Godot;
using System;

public class HUD : CanvasLayer
{
    public override void _Ready()
    {
        GetNode<Panel>("CtrlTheme/PnlMenu").Visible = GetNode<ColorRect>("CtrlTheme/PauseRect").Visible = 
            GetNode<Control>("CtrlTheme/ProgressAnim").Visible = false;
    }

    public void OnBtnResumePressed()
    {
        TogglePause(false);
    }

    public void OnBtnMenuPressed()
    {
        TogglePause(true);
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

    public void TogglePause(bool pause)
    {
        GetTree().Paused = pause;
        GetNode<Panel>("CtrlTheme/PnlMenu").Visible = GetNode<ColorRect>("CtrlTheme/PauseRect").Visible = pause;
        foreach (Button btn in GetNode("CtrlTheme/PnlUIBar/HBoxBtns").GetChildren())
        {
            btn.Disabled = pause;
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (Input.IsActionJustPressed("Pause"))
        {
            TogglePause(!GetTree().Paused);
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

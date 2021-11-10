using Godot;
using System;

public class BattleHUD : CanvasLayer
{
    private Label _lblLog;
    public override void _Ready()
    {
        _lblLog = GetNode<Label>("CtrlTheme/PnlUI/LblLog");
    }

    public void LogEntry(string text)
    {
        _lblLog.Text = text;
    }

}

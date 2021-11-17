using Godot;
using System;
// using System.Collections.Generic;

public class PnlEvents : Panel
{
    // private List<string> _logEntries = new List<string>();
    public override void _Ready()
    {
        
    }

    public void LogEntry(string text)
    {
        GetNode<Label>("HBoxContainer/LblEvents").Text = text;
    }

    // public void LogAndRecordEntry(string text)
    // {
    //     LogEntry(text);
    //     // _logEntries.Add(text);
    //     // foreach (string s in _logEntries)
    //     // {
            
    //     // }
    // }
}

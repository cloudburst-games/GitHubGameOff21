using Godot;
using System;
using System.Collections.Generic;

public class PnlLog : Panel
{
    
    public override void _Ready()
    {
        Visible= false;
    }

    public bool CursorInsidePanel()
    {
        return GetGlobalMousePosition().x > RectGlobalPosition.x && GetGlobalMousePosition().x < RectGlobalPosition.x + RectSize.x
            && GetGlobalMousePosition().y > RectGlobalPosition.y && GetGlobalMousePosition().y < RectGlobalPosition.y + RectSize.y;
    }

    public void Show(List<string> logEntries)
    {
        GetNode<RichTextLabel>("RichTextLabel").Clear();
        for (int i = 0; i < logEntries.Count; i++)
        {
            GetNode<RichTextLabel>("RichTextLabel").AddText(logEntries[i]);
            if (i != logEntries.Count-1)
            {
                GetNode<RichTextLabel>("RichTextLabel").AddText("\n");
            }
        }
        Visible = true;
    }
    
}

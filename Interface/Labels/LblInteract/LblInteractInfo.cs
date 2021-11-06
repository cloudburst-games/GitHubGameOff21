using Godot;
using System;

public class LblInteractInfo : Label
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        string interact = ((InputEvent)InputMap.GetActionList("Interact")[0]).AsText();

        Text = String.Format("'{0}' to interact", interact);
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

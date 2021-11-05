using Godot;
using System;

public class FileDialogFixed : FileDialog
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    // var dir = Directory.new()
    // dir.open("user://")
    // dir.make_dir("sad")

        var dir = new Directory();
        dir.Open("user://");
        dir.MakeDir("Saves");
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public void Refresh()
    {
        _Draw();
    }

    public override void _Draw()
    {
        base._Draw();
		// if (OS.HasFeature("HTML5"))
        // {
        //     GD.Print("HTML");
        //     CurrentDir = "user://userfs";
        //     CurrentPath = "user://userfs";
        // }
        // else
        {
            CurrentDir = "user://Saves";
            // CurrentPath = "user://Saves";
        }
    }
}

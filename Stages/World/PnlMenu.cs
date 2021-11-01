using Godot;
using System;

public class PnlMenu : Panel
{
//     // Declare member variables here. Examples:
//     // private int a = 2;
//     // private string b = "text";

//     // Called when the node enters the scene tree for the first time.
//     public override void _Ready()
//     {
        
//     }

// //  // Called every frame. 'delta' is the elapsed time since the previous frame.
// //  public override void _Process(float delta)
// //  {
// //      
// //  }

//     private bool _gameover = false;

//     public override void _Input(InputEvent @event)
//     {
//         base._Input(@event);
//         if (_gameover)
//         {
//             return;
//         }
//         if (Visible && Input.IsActionJustPressed("Pause"))
//         {
//             PauseMenu(false);
//         }
//         else if (!Visible && Input.IsActionJustPressed("Pause"))
//         {
//             PauseMenu(true);
//         }
//     }

//     public void OnBtnResumePressed()
//     {
//         PauseMenu(false);
//     }

//     public void PauseMenu(bool pause, bool gameover = false, bool victory = false)
//     {
//         _gameover = gameover;
//         Visible = GetNode<ColorRect>("PauseRect").Visible = GetTree().Paused = pause;
//         if (gameover)
//         {//HUD/PnlEnd
//             GetNode<Label>("LblTitle").Text = "Game Over";
//             GetNode<Button>("VBoxBtns/BtnResume").Visible = false;
//         }
//         else if (victory)
//         {
//             GetNode<Label>("LblTitle").Text = "Victory!";
//             GetNode<Button>("VBoxBtns/BtnResume").Visible = false;
//         }
//         else
//         {
//             GetNode<Label>("LblTitle").Text = "Paused";
//             GetNode<Button>("VBoxBtns/BtnResume").Visible = true;
//         }
//     }
}

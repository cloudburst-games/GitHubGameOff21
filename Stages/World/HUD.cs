using Godot;
using System;

public class HUD : CanvasLayer
{
    public override void _Ready()
    {
        GetNode<Panel>("PnlMenu").Visible = GetNode<Panel>("PnlEnd").Visible = 
            GetNode<ColorRect>("PauseRect").Visible = GetNode<Panel>("PnlHint").Visible = false;
    }

    private bool _gameover = false;
    private bool _victory = false;

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (_gameover || _victory)
        {
            return;
        }
        if (GetNode<PnlSettings>("PnlSettings").Visible)
        {
            return;
        }
        if (GetNode<Panel>("PnlMenu").Visible && Input.IsActionJustPressed("Pause"))
        {
            PauseMenu(false);
        }
        else if (!GetNode<Panel>("PnlMenu").Visible && Input.IsActionJustPressed("Pause"))
        {
            PauseMenu(true);
        }
    }

    public void OnBtnResumePressed()
    {
        PauseMenu(false);
    }

    public void OnBtnHintsPressed()
    {
        GetNode<ColorRect>("PauseRect").Visible = GetTree().Paused = GetNode<Panel>("PnlHint").Visible = true;
    }

    public void OnHintBtnOkPressed()
    {
        GetNode<ColorRect>("PauseRect").Visible = GetTree().Paused = GetNode<Panel>("PnlHint").Visible = false;
    }

    public void PauseMenu(bool pause, bool gameover = false, bool victory = false)
    {
        _gameover = gameover;
        GetNode<ColorRect>("PauseRect").Visible = GetTree().Paused = pause;
        if (gameover)
        {//HUD/PnlEnd
            GetNode<Panel>("PnlEnd").Visible = true;
            GetNode<AnimationPlayer>("PnlEnd/Anim").Play("Defeat");
            // GetNode<Label>("LblTitle").Text = "Game Over";
            // GetNode<Button>("VBoxBtns/BtnResume").Visible = false;
        }
        else if (victory)
        {
            GetNode<Panel>("PnlEnd").Visible = true;
            GetNode<AnimationPlayer>("PnlEnd/Anim").Play("Victory");
            // GetNode<Label>("LblTitle").Text = "Victory!";
            // GetNode<Button>("VBoxBtns/BtnResume").Visible = false;
        }
        else
        {
            GetNode<Panel>("PnlMenu").Visible = pause;
            GetNode<Label>("PnlMenu/LblTitle").Text = "Paused";
            GetNode<Button>("PnlMenu/VBoxBtns/BtnResume").Visible = true;
        }
    }
}

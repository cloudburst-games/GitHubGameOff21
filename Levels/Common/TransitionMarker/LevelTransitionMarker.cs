using Godot;
using System;

public class LevelTransitionMarker : Node2D
{

    [Export]
    public string ButtonLabel = "Transition to somewhere";

    [Export]
    public LevelManager.Level DestinationLevel = LevelManager.Level.Level1;

    [Signal]
    public delegate void TriedToTransitionTo(LevelManager.Level dest);

    public override void _Ready()
    {
        Visible = false;
        GetNode<Button>("BtnTransition").Text = ButtonLabel;
    }

    public void OnAreaPlayerDetectBodyEntered(Godot.Object body)
    {
        if (body is Unit unit)
        {
            if (unit.GetControlState() == Unit.ControlState.Player)
            {
                Visible = true;
            }
        }
    }
    public void OnAreaPlayerDetectBodyExited(Godot.Object body)
    {
        if (body is Unit unit)
        {
            if (unit.GetControlState() == Unit.ControlState.Player)
            {
                Visible = false;
            }
        }
    }

    public void OnBtnTransitionPressed()
    {
        EmitSignal(nameof(TriedToTransitionTo), DestinationLevel);
    }
}

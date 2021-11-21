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
        // string interact = ((InputEvent)InputMap.GetActionList("Interact")[0]).AsText();
        GetNode<Label>("Sprite/Panel/Label").Text = ButtonLabel;// String.Format("'{0}': {1}", interact, ButtonLabel);
    }

    public void OnAreaPlayerDetectAreaEntered(Godot.Object area)
    {
        if (area is Area2D a)
        {
            if (a.Name == "NPCEnableInteractionArea")
            {
                if (a.GetParent() is Unit unit)
                {
                    if (unit.CurrentControlState is PlayerUnitControlState)
                    {
                        Visible = true;
                    }
                }
            }
        }
    }
    public void OnAreaPlayerDetectAreaExited(Godot.Object area)
    {
       if (area is Area2D a)
        {
            if (a.Name == "NPCEnableInteractionArea")
            {
                if (a.GetParent() is Unit unit)
                {
                    if (unit.CurrentControlState is PlayerUnitControlState)
                    {
                        Visible = false;
                    }
                }
            }
        }
    }

    public override void _Input(InputEvent ev)
    {
        base._Input(ev);
        if (ev.IsActionPressed("Interact") && Visible && !ev.IsEcho())
        {
            EmitSignal(nameof(TriedToTransitionTo), DestinationLevel);
        }
        
        if (ev is InputEventMouseButton btn)
        {
            if (btn.Pressed && !ev.IsEcho() && btn.ButtonIndex == (int) ButtonList.Left)
            {
                if (Visible)
                {
                    if (GetGlobalMousePosition().DistanceTo(GlobalPosition) < 0.5f*(GetNode<Sprite>("Sprite").Texture.GetSize().x*GetNode<Sprite>("Sprite").Scale.x))
                    EmitSignal(nameof(TriedToTransitionTo), DestinationLevel);
                }
            }
        }
    }

    public void OnBtnTransitionPressed()
    {
        // EmitSignal(nameof(TriedToTransitionTo), DestinationLevel);
    }
}

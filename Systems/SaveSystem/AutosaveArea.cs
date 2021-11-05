using Godot;
using System;

public class AutosaveArea : Area2D
{
    [Export]
    private bool _oneShot = true;

    [Signal]
    public delegate void AutosaveAreaPlayerEntered();

    public bool Active {get; set;} = true;

    public override void _Ready()
    {
        
    }

    public void OnAutosaveAreaBodyEntered(Godot.Object body)
    {
        if (body is Unit unit)
        {
            if (unit.GetControlState() is Unit.ControlState.Player)
            {
                if (!Active)
                {
                    return;
                }
                if (_oneShot)
                {
                    Active = false;
                }
                EmitSignal(nameof(AutosaveAreaPlayerEntered));
            }
        }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

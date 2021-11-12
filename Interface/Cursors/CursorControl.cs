using Godot;
using System;

public class CursorControl : Node
{
        // Load the custom images for the mouse cursor.
        private Resource _select = ResourceLoader.Load("res://Interface/Cursors/Art/Select.PNG");
        private Resource _attack = ResourceLoader.Load("res://Interface/Cursors/Art/Attack.PNG");
        private Resource _spell = ResourceLoader.Load("res://Interface/Cursors/Art/Spell.PNG");
        private Resource _move = ResourceLoader.Load("res://Interface/Cursors/Art/Move.PNG");
        private Resource _invalid = ResourceLoader.Load("res://Interface/Cursors/Art/Invalid.PNG");
        private Resource _wait = ResourceLoader.Load("res://Interface/Cursors/Art/Wait.PNG");

        public enum CursorMode {Select, Attack, Spell, Move, Invalid, Wait}
    public override void _Ready()
    {
        
        SetCursor(CursorMode.Move);
        // Changes a specific shape of the cursor (here, the I-beam shape).
        // Input.SetCustomMouseCursor(beam, Input.CursorShape.);
    }

    public void SetCursor(CursorMode cursorMode)
    {
        switch (cursorMode)
        {
            case CursorMode.Select:
                Input.SetCustomMouseCursor(_select, hotspot: new Vector2(16,0));
                break;
            case CursorMode.Attack:
                Input.SetCustomMouseCursor(_attack, hotspot: new Vector2(32,0));
                break;
            case CursorMode.Spell:
                Input.SetCustomMouseCursor(_spell, hotspot: new Vector2(16,16));
                break;
            case CursorMode.Invalid:
                Input.SetCustomMouseCursor(_invalid, hotspot: new Vector2(16,16));
                break;
            case CursorMode.Move:
                Input.SetCustomMouseCursor(_move, hotspot: new Vector2(16,16));
                break;
            case CursorMode.Wait:
                Input.SetCustomMouseCursor(_wait, hotspot: new Vector2(16,16));
                break;
        }
    }
}
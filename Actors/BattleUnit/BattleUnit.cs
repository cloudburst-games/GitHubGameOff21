using Godot;
using System.Collections.Generic;
using System;

public class BattleUnit : Node2D
{
    public enum Combatant { Beetle, Wasp, Noob }
    
    public enum ActionStateMode { Idle, Moving, Casting, Hit, Dying}
    public ActionStateMode CurrentActionStateMode {get; set;}
    private BattleUnitActionState _actionState;

    private ShaderMaterial _outlineShader = GD.Load<ShaderMaterial>("res://Shaders/Outline/OutlineShader.tres");
    private AnimationPlayer _actionAnim;
    private Sprite _sprite;

    public BattleUnitData CurrentBattleUnitData {get; set;}

    public enum DirectionFacingMode {UpRight, DownRight, DownLeft, UpLeft} // { 45, 135, 225, 315 }
    public DirectionFacingMode Direction {get; set;} = DirectionFacingMode.UpRight;
    
    

    public override void _Ready()
    {
         _actionAnim = GetNode<AnimationPlayer>("ActionAnim");
         _sprite = GetNode<Sprite>("Sprite");
        SetActionState(ActionStateMode.Idle);
    }

    // eventually swap out the sprites with this method
    public void SetSprite(Combatant combatantMode)
    {
        switch (combatantMode)
        {
            case Combatant.Beetle:
                // Modulate = new Color(1,1,1);
                break;
            case Combatant.Wasp:
                // Modulate = new Color (1,0,0);
                break;
            case Combatant.Noob:
                // Modulate = new Color(0,0,1);
                break;
        }
    }

    public void SetActionState(ActionStateMode actionStateMode)
    {
        CurrentActionStateMode = actionStateMode;
        switch (actionStateMode)
        {
            case ActionStateMode.Idle:
                _actionState = new IdleBattleUnitActionState(this);
                break;
            case ActionStateMode.Moving:
                _actionState = new MovingBattleUnitActionState(this);
                break;
            case ActionStateMode.Casting:
                _actionState = new CastingBattleUnitActionState(this);
                break;
            case ActionStateMode.Hit:
                _actionState = new HitBattleUnitActionState(this);
                break;
            case ActionStateMode.Dying:
                _actionState = new DyingBattleUnitActionState(this);
                break;
        }
    }
    
    public override void _PhysicsProcess(float delta)
    {
        if (_actionState != null)
        {
            _actionState.Update(delta);
        }
    }

    public void PlayActionAnim(string animState)
    {
        string anim = animState;
        if (_actionAnim.IsPlaying() && _actionAnim.CurrentAnimation == anim)
        {
            return;
        }

        switch (Direction)
        {
            case DirectionFacingMode.UpRight:
                anim += "UpRight";
                _sprite.FlipH = false;
                break;
            case DirectionFacingMode.UpLeft:
                anim += "UpRight";
                _sprite.FlipH = true;
                break;
            case DirectionFacingMode.DownRight:
                anim += "DownRight";
                _sprite.FlipH = false;
                break;
            case DirectionFacingMode.DownLeft:
                anim += "DownRight";
                _sprite.FlipH = true;
                break;
        }
        _actionAnim.Play(anim);

    }

    public void SetOutlineShader(float[] colour)
    {
        if (colour == null)
        {
            _sprite.Material = null;
            return;
        }
        if (colour.Length < 3)
        {
            _sprite.Material = null;
            return;
        }
        _sprite.Material = (ShaderMaterial) _outlineShader.Duplicate(true);
        ((ShaderMaterial)_sprite.Material).SetShaderParam("outline_color_origin", new Color(colour[0], colour[1], colour[2]));
    }

    public void MoveAlongPoints(List<Vector2> worldPoints)
    {

    }

    // public bool IsMouseOver()
    // {
    //     Vector2 clickPos = GetViewport().GetMousePosition();
    //     Vector2 canvasPos = _sprite.GetGlobalTransformWithCanvas().origin;
    //     Vector2 size = _sprite.Texture.GetSize() * _sprite.Scale;
    //     Vector2 topLeft = canvasPos - size / 2f;
    //     Rect2 area = new Rect2(topLeft, size);
        
    //     return area.HasPoint(clickPos);
    // }
}

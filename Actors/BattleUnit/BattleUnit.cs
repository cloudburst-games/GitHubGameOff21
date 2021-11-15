using Godot;
using System.Collections.Generic;
using System;

public class BattleUnit : Node2D
{
    public enum Combatant { Beetle } // change the name to whatever we are calling generic fighters
    
    public enum ActionStateMode { Idle, Moving, Casting, Hit, Dying}
    public ActionStateMode CurrentActionStateMode {get; set;}
    private BattleUnitActionState _actionState;

    private ShaderMaterial _outlineShader = GD.Load<ShaderMaterial>("res://Shaders/Outline/OutlineShader.tres");
    public AnimationPlayer ActionAnim {get; set;}
    private Sprite _sprite;
    public float AnimSpeed {get; set;} = 2f;

    public bool Dead {get; set;} = false;
    
    [Signal]
    public delegate void CurrentActionCompleted();
    public BattleUnitData CurrentBattleUnitData {get; set;}

    public enum DirectionFacingMode {UpRight, DownRight, DownLeft, UpLeft} // { 45, 135, 225, 315 }
    public DirectionFacingMode Direction {get; set;} = DirectionFacingMode.UpRight;

    public List<Vector2> CurrentPath {get; set;}
    public Vector2 TargetWorldPos {get; set;}

    [Signal]
    public delegate void ReachedNextMovePoint();
    
    

    public override void _Ready()
    {
         ActionAnim = GetNode<AnimationPlayer>("ActionAnim");
         _sprite = GetNode<Sprite>("Sprite");
        SetActionState(ActionStateMode.Idle);
    }

    public void UpdateHealthManaBars()
    {
        // GD.Print("mana ", CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Mana]);
        GetNode<PnlInfo>("PnlInfo").Update(CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Health],
                CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.TotalHealth],
                CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Mana],
                CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.TotalMana]);
    }

    public void SetFactionPanel()
    {
        GetNode<PnlInfo>("PnlInfo").SetFaction(CurrentBattleUnitData.PlayerFaction);
    }

    // eventually swap out the sprites with this method
    public void SetSprite(Combatant combatantMode)
    {
        switch (combatantMode)
        {
            case Combatant.Beetle:
                // Modulate = new Color(1,1,1);
                break;
            // case Combatant.Wasp:
            //     // Modulate = new Color (1,0,0);
            //     break;
            // case Combatant.Noob:
            //     // Modulate = new Color(0,0,1);
            //     break;
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
    
    [Signal]
    public delegate void ReachedHalfwayAnimation();

    public bool DetectingHalfway {get; set;} = true;

    public override void _PhysicsProcess(float delta)
    {
        if (_actionState != null)
        {
            _actionState.Update(delta);
        }

        if (ActionAnim.CurrentAnimationPosition >= ActionAnim.CurrentAnimationLength/2f && DetectingHalfway)
        {
            EmitSignal(nameof(ReachedHalfwayAnimation));
            DetectingHalfway = false;
        }
    }

    public void PlayActionAnim(string animState)//, bool replay=false)
    {
        string anim = animState;
        if (ActionAnim.IsPlaying() && ActionAnim.CurrentAnimation == anim)// && !replay)
        {
            return;
        }
        // if (!replay)
        // {
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
        // }
        // GD.Print(anim + ", flipped: " + _sprite.FlipH);
        ActionAnim.Play(anim, customSpeed:AnimSpeed/2f);

    }

    public void SetAnimSpeed(float speed=2f)
    {
        AnimSpeed = speed;
        // PlayActionAnim(ActionAnim.CurrentAnimation, true);
        // if (CurrentActionStateMode == ActionStateMode.Moving)
        // {
        //     SetActionState(ActionStateMode.Moving);
        // }
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
        CurrentPath = worldPoints;
        SetActionState(ActionStateMode.Moving);
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

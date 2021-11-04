using Godot;
using System;
using System.Collections.Generic;

public class Unit : KinematicBody2D
{
	public float Speed {get; set;} = 200f;

	private AnimationPlayer _actionAnim;
	private Sprite _sprite;
	private float _animRotation = 0;
	public float AnimRotation {
		get{
			return _animRotation;
		}
		set{
			_animRotation = value;
			if (DirectionsByRotation.ContainsKey((float)Math.Round(AnimRotation,1)))
			{
				DirectionAnim = DirectionsByRotation[(float)Math.Round(AnimRotation,1)];
			}			
		}
	}
	public enum ControlState { Player, AI }
	public UnitControlState CurrentControlState;

	public enum ActionState { Idle, Moving, MeleeAttacking }
	private UnitActionState _currentActionState;
	public enum FacingDirection {Up,UpRight,Right,DownRight,Down,DownLeft,Left,UpLeft}
	public FacingDirection DirectionAnim {get; set;} = FacingDirection.Up;
	public Dictionary<float,Unit.FacingDirection> DirectionsByRotation {get; set;} = new Dictionary<float, Unit.FacingDirection>()
	{
		{1.6f, Unit.FacingDirection.Right},
		{2.4f, Unit.FacingDirection.DownRight},
		{3.1f, Unit.FacingDirection.Down},
		{3.9f, Unit.FacingDirection.DownLeft},
		{4.7f, Unit.FacingDirection.Left},
		{5.5f, Unit.FacingDirection.UpLeft},
		{6.3f, Unit.FacingDirection.Up},
		{0, Unit.FacingDirection.Up},
		{0.8f, Unit.FacingDirection.UpRight}
	};
	public Dictionary<Unit.FacingDirection, string> IdleAnimationsByDirection {get; set;} = new Dictionary<FacingDirection, string>()
	{
		{Unit.FacingDirection.Up, "IdleUp"},
		{Unit.FacingDirection.UpRight, "IdleUpRight"},
		{Unit.FacingDirection.Right, "IdleRight"},
		{Unit.FacingDirection.DownRight, "IdleDownRight"},
		{Unit.FacingDirection.Down, "IdleDown"},
		{Unit.FacingDirection.DownLeft, "IdleDownRight"},
		{Unit.FacingDirection.Left, "IdleRight"},
		{Unit.FacingDirection.UpLeft, "IdleUpRight"}
	};
	public Dictionary<Unit.FacingDirection, string> WalkAnimationsByDirection {get; set;} = new Dictionary<FacingDirection, string>()
	{
		{Unit.FacingDirection.Up, "WalkUp"},
		{Unit.FacingDirection.UpRight, "WalkUpRight"},
		{Unit.FacingDirection.Right, "WalkRight"},
		{Unit.FacingDirection.DownRight, "WalkDownRight"},
		{Unit.FacingDirection.Down, "WalkDown"},
		{Unit.FacingDirection.DownLeft, "WalkDownRight"},
		{Unit.FacingDirection.Left, "WalkRight"},
		{Unit.FacingDirection.UpLeft, "WalkUpRight"}
	};

	[Export]
	private ControlState _controlState = ControlState.AI;
	private ActionState _actionState = ActionState.Idle;

	public override void _Ready()
	{
		base._Ready();
		_actionAnim = GetNode<AnimationPlayer>("ActionAnim");
		_sprite = GetNode<Sprite>("Sprite");
		SetControlState(_controlState);
		SetActionState(_actionState);
	}

	public void SetControlState(ControlState state)
	{
		switch (state)
		{
			case ControlState.AI:
				CurrentControlState = new AIUnitControlState(this);
				break;
			case ControlState.Player:
				CurrentControlState = new PlayerUnitControlState(this);
				break;
		}
	}

	public void SetActionState(ActionState state)
	{
		switch (state)
		{
			case ActionState.Idle:
				_currentActionState = new IdleUnitActionState(this);
				break;
			case ActionState.Moving:
				_currentActionState = new MovingUnitActionState(this);
				break;
			case ActionState.MeleeAttacking:
				_currentActionState = new MeleeAttackingUnitActionState(this);
				break;	
		}
	}

	public ControlState GetControlState()
	{
		return _controlState;
	}

	public void SetActionAnim(string animation)
	{
		if (_actionAnim.IsPlaying() && _actionAnim.CurrentAnimation == animation)
		{
			return;
		}
		_sprite.FlipH = (DirectionAnim == FacingDirection.Left || DirectionAnim == FacingDirection.DownLeft || DirectionAnim == FacingDirection.UpLeft);
		_actionAnim.Play(animation);
	}

	public Vector2 CurrentVelocity {get; set;}

	public override void _PhysicsProcess(float delta)
	{
		if (CurrentControlState == null || _currentActionState == null)
		{
			return;
		}
		CurrentControlState.Update(delta);
		_currentActionState.Update(delta);

		// temp
		
	}

}

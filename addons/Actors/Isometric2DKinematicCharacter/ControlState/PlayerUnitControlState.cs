using Godot;
using System;
using System.Collections.Generic;

public class PlayerUnitControlState : UnitControlState
{
	[Signal]
	public delegate void PathRequested(PlayerUnitControlState controlState, Vector2 worldPosition);


	public PlayerUnitControlState(Unit unit)
	{
		this.Unit = unit;

		_unitDetectArea = this.Unit.GetNode<Area2D>("UnitDetectArea");
		_shape = this.Unit.GetNode<CollisionShape2D>("Shape");
		_unitDetectArea.Connect("body_entered", this, nameof(OnUnitDetectAreaEntered));
		_unitDetectArea.Connect("body_exited", this, nameof(OnUnitDetectAreaExited));
		StartPosition = this.Unit.Position;
		Steering = new Steering(maximumForce:75f, maximumSpeed:this.Unit.Speed, extents:((RectangleShape2D) _shape.Shape).Extents, separationFactor:2f);
	}
    
	public PlayerUnitControlState()
	{
		GD.Print("use constructor with unit argument");
		throw new InvalidOperationException();
	}

    public void OnAttributePointsUnspentVisible()
    {

    }

    public void OnInteractNPC(Unit unit)
    {
        this.Unit.EmitSignal(nameof(Unit.DialogueStarted), unit);
    }

    public void ClearPath()
    {
        CurrentPath.Clear();
        this.Unit.EmitSignal(nameof(Unit.PlayerPathCleared));
    }

    public void SetPath(List<Vector2> path)
    {
        CurrentPath = path;
        if (CurrentPath.Count > 0)
        {
            this.Unit.EmitSignal(nameof(Unit.PlayerPathSet), path[path.Count-1]);
        }
    }

    public override void Update(float delta)
    {
        base.Update(delta);

		bool up = Input.IsActionPressed("Move Up");
		bool down =  Input.IsActionPressed("Move Down");
		bool left =  Input.IsActionPressed("Move Left");
		bool right =  Input.IsActionPressed("Move Right");
		

		if (up || down || left || right)
        {
            Unit.GetNode<CollisionShape2D>("Shape").Disabled = false;
            SetTargetAnimRotation(this.Unit.Position + this.Unit.CurrentVelocity);
        
            this.Unit.CurrentVelocity = new Vector2( left ? -1 : (right ? 1 : 0), up ? -1 : (down ? 1 : 0)).Normalized();
            this.Unit.CurrentVelocity *= this.Unit.Speed;
            this.Unit.AnimRotation = TargetAnimRotation;
        }

        // Battle / npc starting dialogue
        foreach (Node n in Unit.GetNode<Area2D>("NPCInteractArea").GetOverlappingBodies())
        {
            if (n is Unit unit)
            {
                if (!unit.CurrentUnitData.Active)
                {
                    continue;
                }
                if (!unit.CurrentUnitData.Companion && !unit.CurrentUnitData.Player && unit.CurrentUnitData.Hostile)
                {
                    this.Unit.EmitSignal(nameof(Unit.BattleStarted), unit, unit.CurrentUnitData.CustomBattleText);
                    return;
                }
                else if (!unit.CurrentUnitData.Companion && !unit.CurrentUnitData.Player && unit.CurrentUnitData.InitiatesDialogue)
                {
                    this.Unit.EmitSignal(nameof(Unit.NPCStartingDialogue), unit);
                    return;
                }
            }
        }
        // Interaction
        if (Input.IsActionJustPressed("Interact") || _talkOnArrive)
        {
            foreach (Node n in Unit.GetNode<Area2D>("NPCInteractArea").GetOverlappingBodies())
            {

                if (n is Unit unit)
                {
                    if (!unit.CurrentUnitData.Active)
                    {
                        continue;
                    }
                    if (!unit.CurrentUnitData.Companion && !unit.CurrentUnitData.Player && !unit.CurrentUnitData.Hostile)
                    {
                        float distance = ((CircleShape2D)unit.GetNode<CollisionShape2D>("NPCInteractArea/Shape").Shape).Radius * 0.8f;
                        if (_talkOnArrive && _talkToHere.DistanceTo(unit.GlobalPosition) < distance && Unit.GlobalPosition.DistanceTo(unit.GlobalPosition) < distance || !_talkOnArrive)
                        {
                            ClearPath();
                            OnInteractNPC(unit);
                            _talkOnArrive = false;
                            return;
                        }
                    }
                }
            }            
            foreach (Node n in Unit.GetNode<Area2D>("NPCInteractArea").GetOverlappingAreas())
            {
                if (n is ShopInteractableArea shopEntranceArea)
                {                        
                    float distance = ((CircleShape2D)shopEntranceArea.GetNode<CollisionShape2D>("Shape").Shape).Radius*1.35f;
                    if (_talkOnArrive && _talkToHere.DistanceTo(shopEntranceArea.GlobalPosition) < distance && Unit.GlobalPosition.DistanceTo(shopEntranceArea.GlobalPosition) < distance || !_talkOnArrive)
                    {
                        ClearPath();
                        // shopEntranceArea.CurrentShop.Start(Unit.CurrentUnitData);
                        this.Unit.EmitSignal(nameof(Unit.ShopAccessed), new ShopDataSignalWrapper() {CurrentShopData = shopEntranceArea.CurrentShop.CurrentShopData});
                        _talkOnArrive = false;
                        return;
                    }
                    
                }
            }
            // foreach (Node n in Unit.GetNode<Area2D>("NPCInteractArea").GetOverlappingBodies())
            // {

            //     if (n is Unit unit)
            //     {
            //         if (!unit.CurrentUnitData.Active)
            //         {
            //             continue;
            //         }
            //         if (!unit.CurrentUnitData.Companion && !unit.CurrentUnitData.Player && !unit.CurrentUnitData.Hostile)
            //         {
            //             if (_talkToHere.DistanceTo(unit.GlobalPosition) < 50)
            //             {
            //                 OnInteractNPC(unit);
            //                 return;
            //             }
            //         }
            //     }
            // }

            // foreach (Node n in Unit.))
        }


        // if using keyboard controls, forget about our path
        if (up || down || left || right || Input.IsActionJustPressed("Interact"))
        {
            _talkOnArrive = false;
            ClearPath();
            return;
        }


		if (CurrentPath.Count == 0)
		{
            ClearPath();
			this.Unit.CurrentVelocity = new Vector2(0,0);
            
            // if (_currentAIBehaviourState is PatrolAIBehaviourState)
            // {
            //     GD.Print(" test");
            // }
			return;
		}
		// GD.Print(CurrentPath.Count);
		// incorproate the steering stuff into AI controls
		Steering.Update(this.Unit.Position);
		// GD.Print(Unit.CurrentVelocity);
		// if (Unit.CurrentVelocity.x == Mathf.NaN)
		// {
		// 	Unit.CurrentVelocity = new Vector2();
		// }
		float areaExtents = (((RectangleShape2D) _shape.Shape).Extents.x * ((RectangleShape2D) _shape.Shape).Extents.y);
		// GD.Print(areaExtents);
		// If we have a path, move towards it, otherwise just stay still
		if (CurrentPath.Count > 0)
		{
            Unit.GetNode<CollisionShape2D>("Shape").Disabled = true;
			if (CurrentPath.Count > 1)
			{
				if (this.Unit.Position.DistanceSquaredTo(CurrentPath[1]) < 5*areaExtents)
				{
					CurrentPath.RemoveAt(0);//(this.Unit.Position.DistanceSquaredTo(CurrentPath[0]));
				}
			}
					// GD.Print("is it this1?");
			RotateToTarget(CurrentPath[0], delta);
				
			if (CurrentPath.Count == 1)
			{
					// GD.Print("is it this2?");
				if (this.Unit.Position.DistanceSquaredTo(CurrentPath[0]) < 5*areaExtents)
				{
					// this.Unit.CurrentVelocity = new Vector2(0,0);
					CurrentPath.RemoveAt(0);
					// Steering.IsAvoiding = false;
					// GD.Print("test");
				}
				else
				{
					RotateAndMove(CurrentPath[0],true, delta);
				}
			}
			else if (CurrentPath.Count <= 2)
			{
					// GD.Print("is it thi3s?");
				RotateAndMove(CurrentPath[1],true, delta);
			}
			else
			{
					// GD.Print("is it this4?");
				RotateAndMove(CurrentPath[1],false, delta);
			}
		}
    }

    public override void UpdateInputEvents(InputEvent ev)
    {
        if (ev is InputEventMouseButton btn)
        {
            // if (Unit.GetViewport().GetMousePosition().y > 1080-48)
            // {
            //     return;
            // }
            // if (btn.ButtonIndex == (int) ButtonList.Left && btn.Pressed && !ev.IsEcho())
            // {
            //     foreach (Node n in Unit.GetNode<Area2D>("NPCInteractArea").GetOverlappingBodies())
            //     {
            //         if (n is Unit unit)
            //         {GD.Print(unit.GetGlobalMousePosition().DistanceTo(unit.GlobalPosition));
            //             if (unit.GetGlobalMousePosition().DistanceTo(unit.GlobalPosition) > 50)
            //             {
                            
            //                 continue;
            //             }
            //             if (!unit.CurrentUnitData.Active)
            //             {
            //                 continue;
            //             }
            //             if (!unit.CurrentUnitData.Companion && !unit.CurrentUnitData.Player && !unit.CurrentUnitData.Hostile)
            //             {
            //                 OnInteractNPC(unit);
            //                 return;
            //             }
            //         }
            //     }
            // }

            if (btn.ButtonIndex == (int) ButtonList.Left && btn.Pressed && !ev.IsEcho())
            {
                EmitSignal(nameof(PathRequested), this, Unit.GetGlobalMousePosition());
                if (CurrentPath.Count > 0)
                {
                    _talkToHere = Unit.GetGlobalMousePosition();
                    _talkOnArrive = true;
                }
            }
        }

        
    }

    private Vector2 _talkToHere = new Vector2();
    private bool _talkOnArrive = false;

	public Steering Steering {get; set;}
	private Area2D _unitDetectArea;
	private CollisionShape2D _shape;
	public List<Vector2> CurrentPath = new List<Vector2>();
	
	public AIBehaviourState _currentAIBehaviourState;
	public Vector2 StartPosition {get; set;}
	public bool IsAvoiding {get; set;} = false;

	
	private void OnUnitDetectAreaEntered(PhysicsBody2D body)
	{
		if (! (body is StaticBody2D || body is KinematicBody2D || body is RigidBody2D))
		{
			return;
		}
		if (body is Unit unit)
		{
			if (!Steering.DetectedUnits.Contains(unit))
			{
				Steering.DetectedUnits.Add(unit);
			}
		}
		if (!Steering.DetectedPhysicsBodies.Contains(body))
		{
			Steering.DetectedPhysicsBodies.Add(body);
		}
	}


	private void OnUnitDetectAreaExited(PhysicsBody2D body)
	{
		if (! (body is StaticBody2D || body is KinematicBody2D || body is RigidBody2D))
		{
			return;
		}
		if (body is Unit unit)
		{
			if (Steering.DetectedUnits.Contains(unit))
			{
				Steering.DetectedUnits.Remove(unit);
			}
		}
		if (Steering.DetectedPhysicsBodies.Contains(body))
		{
			Steering.DetectedPhysicsBodies.Remove(body);
		}
	}

	private void RotateToTarget(Vector2 target, float delta)
	{
		if (Steering.MaximumForce == 75f) // at high enough steering force, rotate instantly
		{
			this.Unit.AnimRotation = TargetAnimRotation;
			return;
		}
		float lerpAngle = Mathf.LerpAngle(this.Unit.AnimRotation, TargetAnimRotation, Steering.MaximumForce/75f);
		this.Unit.AnimRotation = Mathf.PosMod(lerpAngle, 2*Mathf.Pi);
	}

	public void RotateAndMove(Vector2 target, bool brake, float delta)
	{
		// Vector2 prevVelocity = this.Unit.CurrentVelocity;
		// MoveAndSlide( _steering.CalculateVelocity(target, brake));
		this.Unit.CurrentVelocity = Steering.CalculateVelocity(target, brake);

		SetTargetAnimRotation(target);
	}
}

using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class Unit : KinematicBody2D
{
    [Signal]
    public delegate void PlayerPathSet(Vector2 finalWorldPosition);

    [Signal]
    public delegate void PlayerPathCleared();
    
    [Signal]
    public delegate void DialogueStarted(Unit target);
    [Signal]
    public delegate void BattleStarted(Unit target, string customBattleText);
    [Signal]
    public delegate void NPCStartingDialogue(Unit target);
    [Signal]
    public delegate void RightClicked(Unit target);
    [Signal]
    public delegate void ShopAccessed(ShopDataSignalWrapper wrappedShopData);

    [Export]
    public string ID = "";
    [Export]
    public string UnitName = "";
    [Export]
    private string _customBattleText {get; set;} = "";
    [Export]
    private string _defeatMessage {get; set;} = "You have defeated your foe!";
    [Export]
    public Texture PortraitPath = GD.Load<Texture>("res://Interface/Cursors/Art/Hint.PNG");
    [Export]
    public Texture PortraitPathSmall = GD.Load<Texture>("res://Interface/Cursors/Art/Hint.PNG");
    
    [Export]
    private PackedScene _body = GD.Load<PackedScene>("res://Actors/NPC/Bodies/NPCBody.tscn");

    [Export]
    private Unit.FacingDirection _startDirection;

    public float Speed {get; set;} = 200f;
    private Random _rand = new Random();

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

    ///
    [Export]
    private bool _active = true;
    [Export]
    private Dictionary<string, bool> _startingBools = new Dictionary<string, bool>() {
        {"Companion", false},
        {"InitiatesDialogue", false},
        {"Hostile", false}
    };
    [Export]
    private AIUnitControlState.AIBehaviour _startingBehaviour = AIUnitControlState.AIBehaviour.Stationary;

    // battle
    [Export]
    private BattleUnit.Combatant _mainCombatant = BattleUnit.Combatant.Beetle;
    [Export]
    private int _combatLevel = 1;
    [Export] // minions are set to combat level -1. use all of the data to generate battleunits
    private Dictionary<BattleUnit.Combatant, int> _minions = new Dictionary<BattleUnit.Combatant, int>() {
        {BattleUnit.Combatant.Beetle, 0}
    };
    [Export]
    private List<UnitData.Attribute> _favouredAttributes = new List<UnitData.Attribute>();
    [Export]
    private float _physicalDamageRange = 3f;
    [Export]
    private SpellEffectManager.SpellMode _startingSpell1 = SpellEffectManager.SpellMode.Empty;
    [Export]
    private SpellEffectManager.SpellMode _startingSpell2 = SpellEffectManager.SpellMode.Empty;
    [Export]
    private SpellEffectManager.SpellMode _spellGainedAtHigherLevel = SpellEffectManager.SpellMode.Empty;
    
    [Export]
    private int _startingGold = 0;
    [Export]
    private Godot.Collections.Array<PnlInventory.ItemMode> _itemsHeld = new Godot.Collections.Array<PnlInventory.ItemMode>();
    [Export]
    private PnlInventory.ItemMode _potionEquipped1 = PnlInventory.ItemMode.Empty;
    [Export]
    private PnlInventory.ItemMode _potionEquipped2 = PnlInventory.ItemMode.Empty;
    [Export]
    private PnlInventory.ItemMode _potionEquipped3 = PnlInventory.ItemMode.Empty;
    [Export]
    private PnlInventory.ItemMode _weaponEquipped = PnlInventory.ItemMode.Empty;
    [Export]
    private PnlInventory.ItemMode _armourEquipped = PnlInventory.ItemMode.Empty;
    [Export]
    private PnlInventory.ItemMode _amuletEquipped = PnlInventory.ItemMode.Empty;
    [Export]
    private bool _weak = false;
    //

    public UnitData CurrentUnitData {get; set;} = new UnitData();

    public IStoreable PackAndGetData() // update any data before storing
    {
        CurrentUnitData.NPCPosition = GlobalPosition; // only used for NPCs, player position needs to be stored Level level
        CurrentUnitData.Modified = true;
        return CurrentUnitData;
    }

    // public void UpdateBattleUnitData()
    // {
    //     // what can feasibly change is 
    // }

    private void SetStartingData()
    {
        if (CurrentUnitData.Modified)
        {
            return;
        }
        CurrentUnitData.ID = this.ID;
        CurrentUnitData.Name = this.UnitName;
        CurrentUnitData.BasePhysicalDamageRange = this._physicalDamageRange;
        CurrentUnitData.PortraitPath = this.PortraitPath.ResourcePath;
        CurrentUnitData.PortraitPathSmall = this.PortraitPathSmall.ResourcePath;
        CurrentUnitData.BodyPath = this._body.ResourcePath;
        CurrentUnitData.Gold = _startingGold;
        CurrentUnitData.Active = _active;
        CurrentUnitData.CurrentBattleUnitData = new BattleUnitData() {
            Combatant = _mainCombatant,
            Level = _combatLevel,
            Name = CurrentUnitData.Name,
            ItemsHeld = _itemsHeld.ToList(),
            WeaponEquipped = _weaponEquipped,
            AmuletEquipped = _amuletEquipped,
            ArmourEquipped = _armourEquipped,
            Spell1 = _startingSpell1,
            Spell2 = _startingSpell2,
            SpellGainedAtHigherLevel = _spellGainedAtHigherLevel
        };
        CurrentUnitData.CurrentBattleUnitData.BattlePortraitPath = this.PortraitPathSmall.ResourcePath;
        CurrentUnitData.EquipAmulet(_amuletEquipped);
        CurrentUnitData.EquipArmour(_armourEquipped);
        CurrentUnitData.EquipWeapon(_weaponEquipped);
        CurrentUnitData.CurrentBattleUnitData.PotionsEquipped[0] = _potionEquipped1;
        CurrentUnitData.CurrentBattleUnitData.PotionsEquipped[1] = _potionEquipped2;
        CurrentUnitData.CurrentBattleUnitData.PotionsEquipped[2] = _potionEquipped3;
        CurrentUnitData.CurrentBattleUnitData.BodyPath = CurrentUnitData.BodyPath;
        // ExperienceManager xpman = new ExperienceManager();

        // GD.Print("level: " + CurrentUnitData.CurrentBattleUnitData.Level + ", xp: " + xpman.GetExperienceNeeded(CurrentUnitData.CurrentBattleUnitData.Level));

        CurrentUnitData.CurrentBattleUnitData.Experience = CurrentUnitData.ExperienceManager.GetTotalExperienceValueOfLevel(CurrentUnitData.CurrentBattleUnitData.Level);
        CurrentUnitData.Minions = new List<BattleUnitData>();
        foreach (BattleUnit.Combatant combatant in _minions.Keys)
        {
            for (int i = 0; i < _minions[combatant]; i++)
            {
                UnitData minionUnitData = new UnitData();
                minionUnitData.CurrentBattleUnitData = new BattleUnitData();
                minionUnitData.CurrentBattleUnitData.Combatant = combatant;
                minionUnitData.CurrentBattleUnitData.Level = CurrentUnitData.CurrentBattleUnitData.Level - 1;
                minionUnitData.BodyPath = CurrentUnitData.BodyPath;
                minionUnitData.CurrentBattleUnitData.BodyPath = CurrentUnitData.BodyPath;
                minionUnitData.CurrentBattleUnitData.BattlePortraitPath = "res://Actors/PortraitPlaceholders/Small/NPC.PNG";
                minionUnitData.CurrentBattleUnitData.Weak = _weak;
                minionUnitData.SetAttributesByLevel(_favouredAttributes);
                minionUnitData.UpdateDerivedStatsFromAttributes();
                CurrentUnitData.Minions.Add(minionUnitData.CurrentBattleUnitData);
            } 
        }
        CurrentUnitData.Hostile = _startingBools["Hostile"];
        CurrentUnitData.InitiatesDialogue = _startingBools["InitiatesDialogue"];
        CurrentUnitData.Behaviour = _startingBehaviour;
        CurrentUnitData.CustomBattleText = _customBattleText;
        CurrentUnitData.DefeatMessage = _defeatMessage;
        CurrentUnitData.Companion = _startingBools["Companion"];
        CurrentUnitData.CurrentBattleUnitData.Weak = _weak;
        
        CurrentUnitData.SetAttributesByLevel(_favouredAttributes);
        
        foreach (Node n in GetChildren())
        {
            if (n is Position2D point)
            {
                // should add the point before the NPC moves - so the start point.
                // only works if only stays in patrol state
                CurrentUnitData.PatrolPoints.Add(point.GlobalPosition); 
            }
        }
        if (CurrentUnitData.PatrolPoints.Count < 2 && CurrentUnitData.Behaviour == AIUnitControlState.AIBehaviour.Patrol)
        {
            CurrentUnitData.Behaviour = AIUnitControlState.AIBehaviour.Stationary;
        }
        Visible = CurrentUnitData.Active;
        BodySwap();
        CurrentUnitData.StartDirectionFacing = _startDirection;
        DirectionAnim = CurrentUnitData.StartDirectionFacing;
    }




    // private float GetWeaponDamage()
    // {
    //     return 0f;
    // }

    // public void UpdateArmourEffects(BattleUnitData battleUnitData, float armourValue)
    // {
    //     // y=\frac{\left(x\cdot2\right)}{\left(1+\left(x\cdot0.05\right)\right)} // https://www.desmos.com/calculator/3fisjexbvp
    //     battleUnitData.Stats[BattleUnitData.DerivedStat.PhysicalResist] = armourValue*2 / (1 + (armourValue * 0.05f));
    // }

    private void OnNPCInteractAreaBodyEntered(Godot.Object body)
    {
        // if (!CurrentUnitData.Active)
        // {
        //     return;
        // }
        // if (body is Unit unit)
        // {
        //     if (unit.CurrentUnitData.Player)
        //     {
        //         if (CurrentUnitData.Hostile)
        //         {
        //             GD.Print("initiate battle");
        //         }
        //         else if (! CurrentUnitData.Companion)
        //         {
                    
        //         }
        //     }
        // }
    }

    public void SetHighlight(bool enable)
    {
        if (enable)
        {
            if (_sprite.Material == null)
            {
                ShaderMaterial shaderMaterial = (ShaderMaterial) GD.Load<ShaderMaterial>("res://Shaders/Flash/FlashShader.tres").Duplicate();
                shaderMaterial.SetShaderParam("speed", 12f);
                shaderMaterial.SetShaderParam("flash_colour_original", new Color(1f,1f,1f));
                shaderMaterial.SetShaderParam("flash_depth", 0.4f);
                _sprite.Material = shaderMaterial;
            }
        }
        else
        {
            // make sure the player is not overlapping first
            if (GetNode<Area2D>("NPCInteractArea").GetOverlappingAreas().Count > 0)
            {
                foreach (Godot.Object area in GetNode<Area2D>("NPCInteractArea").GetOverlappingAreas())
                {
                    if (area is Area2D a)
                    {
                        if (a.Name == "NPCEnableInteractionArea")
                        {
                            if (a.GetParent() is Unit unit)
                            {
                                if (unit.CurrentControlState is PlayerUnitControlState && ! CurrentUnitData.Companion)
                                {
                                    return;
                                    // GetNode<Panel>("PnlInfo").Visible = true;
                                }
                            }
                        }
                    }
                }
            }
            _sprite.Material = null;
        }
    }

    private void OnNPCInteractAreaAreaEntered(Godot.Object area)
    {
        if (area is Area2D a)
        {
            if (a.Name == "NPCEnableInteractionArea")
            {
                if (a.GetParent() is Unit unit)
                {
                    if (unit.CurrentControlState is PlayerUnitControlState && ! CurrentUnitData.Companion)
                    {
                        SetHighlight(true);
                        // GetNode<Panel>("PnlInfo").Visible = true;
                    }
                }
            }
        }
    }

    private void OnNPCInteractAreaAreaExited(Godot.Object area)
    {
        if (area is Area2D a)
        {
            if (a.Name == "NPCEnableInteractionArea")
            {
                if (a.GetParent() is Unit unit)
                {
                    if (unit.CurrentControlState is PlayerUnitControlState && ! CurrentUnitData.Companion)
                    {
                        _sprite.Material = null;
                    }
                }
            }
        }
    }
    
    private void OnNPCInteractAreaBodyExited(Godot.Object body)
    {   
        // if (!CurrentUnitData.Active)
        // {
        //     return;
        // }  
        // if (body is Unit unit)
        // {
        //     if (unit.CurrentUnitData.Player)
        //     {
        //         if (CurrentUnitData.Hostile)
        //         {
        //             // GD.Print("initiate battle");
        //         }
        //         else if (! CurrentUnitData.Companion)
        //         {

        //         }
        //     }
        // }
    }

    public void UpdateFromUnitData()
    {
        if (CurrentControlState is AIUnitControlState aIUnitControlState)
        {
            aIUnitControlState.SetAIBehaviourState(CurrentUnitData.Behaviour);
        }
        Visible = CurrentUnitData.Active;
        GetNode<Panel>("PnlInfo").Visible = false;
        BodySwap();
        DirectionAnim = CurrentUnitData.StartDirectionFacing;
    }

    private bool _bodySwapped = false;

    private void BodySwap()
    {
        if (_bodySwapped)
        {
            return;
        }
        _bodySwapped = true;
        Sprite oldSprite = GetNode<Sprite>("Sprite");
        oldSprite.Name = "SpriteOld";
        CollisionShape2D oldshape = GetNode<CollisionShape2D>("Shape");
        oldshape.Name = "ShapeOld";
        AnimationPlayer oldAnim = GetNode<AnimationPlayer>("ActionAnim");
        oldAnim.Name = "ActionAnimOld";
        Node npcBody = GD.Load<PackedScene>(CurrentUnitData.BodyPath).Instance();
        Sprite sprite = npcBody.GetNode<Sprite>("Sprite");
        CollisionShape2D shape = npcBody.GetNode<CollisionShape2D>("Shape");
        AnimationPlayer anim = npcBody.GetNode<AnimationPlayer>("ActionAnim");
        npcBody.RemoveChild(sprite);
        npcBody.RemoveChild(shape);
        npcBody.RemoveChild(anim);
        AddChild(sprite);
        AddChild(shape);
        AddChild(anim);
        oldSprite.QueueFree();
        oldshape.QueueFree();
        oldAnim.QueueFree();
        npcBody.QueueFree();

    }

    public override void _UnhandledInput(InputEvent ev)
    {
        if (!CurrentUnitData.Active)
        {
            return;
        }
        if (ev is InputEventMouseButton btn)// && !(ev.IsEcho()))
        {
            if (btn.ButtonIndex == (int) ButtonList.Right)
            {
                if (btn.Pressed)
                {
                    Vector2 clickPos = GetViewport().GetMousePosition();
                    Vector2 canvasPos = GetNode<Sprite>("Sprite").GetGlobalTransformWithCanvas().origin;
                    Vector2 size = GetNode<Sprite>("Sprite").Texture.GetSize() * GetNode<Sprite>("Sprite").Scale;
                    Vector2 topLeft = canvasPos - size / 2f;
                    Rect2 area = new Rect2(topLeft, size);
                    if (area.HasPoint(clickPos))
                    {
                        EmitSignal(nameof(RightClicked), this);
                    }                    
                }
            }
        }
        CurrentControlState.UpdateInputEvents(ev);
    }
    
    ///


	public enum ControlState { Player, AI, PlayerMouse }
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
        SetStartingData();
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

    // public override void _UnhandledInput(InputEvent @event)
    // {
    //     base._UnhandledInput(@event);
    // }

}

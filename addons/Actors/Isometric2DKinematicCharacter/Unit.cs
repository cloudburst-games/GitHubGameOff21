using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class Unit : KinematicBody2D
{
    [Signal]
    public delegate void DialogueStarted(Unit target);
    [Signal]
    public delegate void BattleStarted(Unit target);
    [Signal]
    public delegate void RightClicked(Unit target);

    [Export]
    public string ID = "";
    [Export]
    public string UnitName = "";
    [Export]
    public Texture PortraitPath = GD.Load<Texture>("res://Interface/Cursors/Art/Hint.PNG");
    [Export]
    public Texture PortraitPathSmall = GD.Load<Texture>("res://Interface/Cursors/Art/Hint.PNG");
    
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
    private Dictionary<string, bool> _startingBools = new Dictionary<string, bool>() {
        {"Companion", false},
        {"Hostile", false},
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
        {BattleUnit.Combatant.Beetle, 1}
    };
    [Export]
    private List<UnitData.Attribute> _favouredAttributes = new List<UnitData.Attribute>();
    [Export]
    private float _physicalDamageRange = 3f;
    [Export]
    private SpellEffectManager.SpellMode[] _startingSpells = new SpellEffectManager.SpellMode[2] {
        SpellEffectManager.SpellMode.Empty, SpellEffectManager.SpellMode.Empty
    };
    [Export]
    private SpellEffectManager.SpellMode _spellGainedAtHigherLevel = SpellEffectManager.SpellMode.Empty;

    [Export]
    private List<PotionEffect.PotionMode> _potions = new  List<PotionEffect.PotionMode>(); // max 9
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
        CurrentUnitData.PhysicalDamageRange = this._physicalDamageRange;
        CurrentUnitData.PortraitPath = this.PortraitPath.ResourcePath;
        CurrentUnitData.PortraitPathSmall = this.PortraitPathSmall.ResourcePath;

        CurrentUnitData.CurrentBattleUnitData = new BattleUnitData() {
            Combatant = _mainCombatant,
            Level = _combatLevel,
            Name = CurrentUnitData.Name,
            Potions = _potions,
            Spell1 = _startingSpells[0],
            Spell2 = _startingSpells[1],
            SpellGainedAtHigherLevel = _spellGainedAtHigherLevel
        };
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
                SetAttributesByLevel(minionUnitData);
                minionUnitData.UpdateDerivedStatsFromAttributes();
                CurrentUnitData.Minions.Add(minionUnitData.CurrentBattleUnitData);
            } 
        }
        CurrentUnitData.Hostile = _startingBools["Hostile"];
        CurrentUnitData.Behaviour = _startingBehaviour;
        CurrentUnitData.Companion = _startingBools["Companion"];
        
        SetAttributesByLevel(CurrentUnitData);
        
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
    }


    public void SetAttributesByLevel(UnitData unitData)
    {
        int pool = 60;// CurrentUnitData.CurrentBattleUnitData.Level * 60;
        if (unitData.CurrentBattleUnitData.Level >= 2)
        {
            for (int i = 2; i <= unitData.CurrentBattleUnitData.Level; i++)
            {
                pool += 5 + Convert.ToInt32(Math.Floor(i/10f));
            }
        }
        // GD.Print("\nsetting attributes for " + unitData.Name + "from pool of " + pool + " who is level " + unitData.CurrentBattleUnitData.Level);
        while (pool > 0)
        {
            List<UnitData.Attribute> atts = unitData.Attributes.Keys.ToList();
            for (int i = 0; i < atts.Count; i++)
            {
                UnitData.Attribute att = atts[_rand.Next(0, atts.Count)];
                int numToAllocate = Math.Min(pool, _rand.Next(0, _favouredAttributes.Contains(att) ? 4 : 2));
                unitData.Attributes[att] += numToAllocate;
                pool -= numToAllocate;
                atts.Remove(att);
            }

        }
        // foreach (UnitData.Attribute att in unitData.Attributes.Keys)
        // {
        //     GD.Print(att + ": " + unitData.Attributes[att]);
        // }

        unitData.UpdateDerivedStatsFromAttributes();
    }

    private float GetWeaponDamage()
    {
        return 0f;
    }

    public void UpdateArmourEffects(BattleUnitData battleUnitData, float armourValue)
    {
        // y=\frac{\left(x\cdot2\right)}{\left(1+\left(x\cdot0.05\right)\right)} // https://www.desmos.com/calculator/3fisjexbvp
        battleUnitData.Stats[BattleUnitData.DerivedStat.PhysicalResist] = armourValue*2 / (1 + (armourValue * 0.05f));
    }

    private void OnNPCInteractAreaBodyEntered(Godot.Object body)
    {
        if (body is Unit unit)
        {
            if (unit.CurrentUnitData.Player)
            {
                if (CurrentUnitData.Hostile)
                {
                    GD.Print("initiate battle");
                }
                else if (! CurrentUnitData.Companion)
                {
                    GetNode<Panel>("PnlInfo").Visible = GetNode<Label>("PnlInfo/LblInteractInfo").Visible = true;
                }
            }
        }
    }
    
    private void OnNPCInteractAreaBodyExited(Godot.Object body)
    {      
        if (body is Unit unit)
        {
            if (unit.CurrentUnitData.Player)
            {
                if (CurrentUnitData.Hostile)
                {
                    // GD.Print("initiate battle");
                }
                else if (! CurrentUnitData.Companion)
                {
                    GetNode<Panel>("PnlInfo").Visible = false;
                    foreach (Label l in GetNode<Panel>("PnlInfo").GetChildren())
                    {
                        l.Visible = false;
                    }
                }
            }
        }
    }

    public void UpdateFromUnitData()
    {
        if (CurrentControlState is AIUnitControlState aIUnitControlState)
        {
            aIUnitControlState.SetAIBehaviourState(CurrentUnitData.Behaviour);
        }
    }

    public override void _UnhandledInput(InputEvent ev)
    {
        
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
    }
    ///


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

}

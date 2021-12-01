using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class WorldInteractable : StaticBody2D
{

    [Signal]
    public delegate void Interacted(WorldInteractableDataSignalWrapper wrappedWorldInteractableData);

    public WorldInteractableData CurrentWorldInteractableData = new WorldInteractableData();

    [Export]
    private PackedScene _bodyScn = GD.Load<PackedScene>("res://Props/WorldInteractable/Bodies/ChestBody.tscn");
    [Export]
    private float _experience = 0;
    [Export]
    private int _gold = 0;
    [Export]
    private Godot.Collections.Array<PnlInventory.ItemMode> _startingItems = new Godot.Collections.Array<PnlInventory.ItemMode>();
    [Export]
    private bool _dieOnActivate = false;
    [Export]
    private string _flavourText = "";
    [Export]
    private string _eventText = "";

    private bool _interactable = false;

    public bool Loaded {get; set;} = false;

    public override void _Ready()
    {
        if (Loaded)
        {
            return;
        }

        CurrentWorldInteractableData.Active = true;
        CurrentWorldInteractableData.WorldPosition = GlobalPosition;
        CurrentWorldInteractableData.BodyPath = _bodyScn.ResourcePath;
        CurrentWorldInteractableData.Experience = _experience;
        CurrentWorldInteractableData.Gold = _gold;
        CurrentWorldInteractableData.Items = _startingItems.ToList();
        CurrentWorldInteractableData.DieOnActivate = _dieOnActivate;
        CurrentWorldInteractableData.FlavourText = _flavourText;
        CurrentWorldInteractableData.EventText = _eventText;

        SetBody();
    }

    public void LoadStartingData(WorldInteractableData worldInteractableData)
    {
        CurrentWorldInteractableData = worldInteractableData;
        GlobalPosition = CurrentWorldInteractableData.WorldPosition;
        SetBody();
    }

    public void SetBody()
    {
        Node body = GD.Load<PackedScene>(CurrentWorldInteractableData.BodyPath).Instance();
        Sprite sprite = body.GetNode<Sprite>("Sprite");
        CollisionPolygon2D shape = body.GetNode<CollisionPolygon2D>("CollisionPolygon2D");
        Area2D area = body.GetNode<Area2D>("Area2D");
        AnimationPlayer anim = body.GetNode<AnimationPlayer>("AnimationPlayer");
        body.RemoveChild(sprite);
        body.RemoveChild(shape);
        body.RemoveChild(area);
        body.RemoveChild(anim);
        AddChild(sprite);
        sprite.GlobalPosition = this.GlobalPosition;
        AddChild(shape);
        AddChild(area);
        AddChild(anim);
        body.QueueFree();

        area.Connect("area_entered", this, nameof(OnAreaPlayerDetectAreaEntered));
        area.Connect("area_exited", this, nameof(OnAreaPlayerDetectAreaExited));

    }

    public void OnAreaPlayerDetectAreaEntered(Godot.Object area)
    {
        GD.Print("test");
        if (!CurrentWorldInteractableData.Active)
        {
            return;
        }
        if (area is Area2D a)
        {
            if (a.Name == "NPCEnableInteractionArea")
            {
                if (a.GetParent() is Unit unit)
                {
                    if (unit.CurrentControlState is PlayerUnitControlState)
                    {
                        _interactable = true;
                        SetSpriteShader(true);
                    }
                }
            }
        }
    }

    private void SetSpriteShader(bool enable)
    {
        if (!HasNode("Sprite"))
        {
            return;
        }

        if (!enable)
        {
            GetNode<Sprite>("Sprite").Material = null;
            return;
        }

        if (GetNode<Sprite>("Sprite").Material == null)
        {
            ShaderMaterial shaderMaterial = (ShaderMaterial) GD.Load<ShaderMaterial>("res://Shaders/Flash/FlashShader.tres").Duplicate();
            shaderMaterial.SetShaderParam("speed", 12f);
            shaderMaterial.SetShaderParam("flash_colour_original", new Color(1f,1f,1f));
            shaderMaterial.SetShaderParam("flash_depth", 0.4f);
            GetNode<Sprite>("Sprite").Material = shaderMaterial;
        }
    }

    public void OnAreaPlayerDetectAreaExited(Godot.Object area)
    {
        if (!CurrentWorldInteractableData.Active)
        {
            return;
        }
       if (area is Area2D a)
        {
            if (a.Name == "NPCEnableInteractionArea")
            {
                if (a.GetParent() is Unit unit)
                {
                    if (unit.CurrentControlState is PlayerUnitControlState)
                    {
                        _interactable = false;
                        SetSpriteShader(false);
                    }
                }
            }
        }
    }

    public override void _Input(InputEvent ev)
    {
        base._Input(ev);
        if (ev.IsActionPressed("Interact") && _interactable && CurrentWorldInteractableData.Active && !ev.IsEcho())
        {
            Activate();
        }
        
        // if (ev is InputEventMouseButton btn)
        // {
        //     if (btn.Pressed && !ev.IsEcho() && btn.ButtonIndex == (int) ButtonList.Left)
        //     {
        //         if (_interactable && CurrentWorldInteractableData.Active)
        //         {
        //             if (GetGlobalMousePosition().DistanceTo(GlobalPosition) < 0.5f*(GetNode<Sprite>("Sprite").Texture.GetSize().x*GetNode<Sprite>("Sprite").Scale.x))
        //             {
        //                 Activate();
        //             }
        //         }
        //     }
        // }
    }

    public void ClickedToMove()
    {
        if (_interactable && CurrentWorldInteractableData.Active)
        {
            Activate();
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (!CurrentWorldInteractableData.Active)
        {
            return;
        }
        if (GetGlobalMousePosition().DistanceTo(GlobalPosition) < 0.5f*(GetNode<Sprite>("Sprite").Texture.GetSize().x*GetNode<Sprite>("Sprite").Scale.x))
        {
            SetSpriteShader(true);
        }
        else
        {
            if (!_interactable)
            {
                SetSpriteShader(false);
            }
        }
    }

    public async void Activate()
    {
        if (!CurrentWorldInteractableData.Active)
        {
            return;
        }
        CurrentWorldInteractableData.Active = false;

        GetNode<AnimationPlayer>("AnimationPlayer").Play("Activate");
        SetSpriteShader(false);

        EmitSignal(nameof(Interacted), new WorldInteractableDataSignalWrapper() {CurrentWorldInteractableData = this.CurrentWorldInteractableData});

        if (_dieOnActivate)
        {
            GetNode<CollisionPolygon2D>("CollisionPolygon2D").Disabled = true;
            await ToSignal(GetNode<AnimationPlayer>("AnimationPlayer"), "animation_finished");
            QueueFree();
        }


    }


}

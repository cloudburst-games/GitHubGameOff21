using Godot;
using System;
using System.Collections.Generic;

public class Mixable : Area2D
{

    public enum MixableType { BlueHerb, RedHerb, YellowHerb, EmptyPot, YellowPot, BluePot, RedPot, GreenPot, PurplePot, OrangePot, BrownPot }

    public MixableType MixType {get; set;} = MixableType.BlueHerb;

    public Vector2 TopLeftBorder {get; set;} = new Vector2(); // to be set when instancing
    public Vector2 BotRightBorder {get; set;} = new Vector2();

    public Rect2 ReceptacleArea {get; set;} = new Rect2();
    public Rect2 PotionArea {get; set;} = new Rect2();

    private float _minDistance = 75f;
    private bool _beingDragged = false;
    private Vector2 _originalPos = new Vector2();
    private Vector2 _currentDragDifference = new Vector2();
    private float _shakeAtTime = 7f;

    private Dictionary<Mixable.MixableType, Rect2> _textureDict = new Dictionary<MixableType, Rect2>()
    {
        {Mixable.MixableType.BlueHerb, new Rect2(new Vector2(0,0), new Vector2(500,500))},
        {Mixable.MixableType.RedHerb, new Rect2(new Vector2(0,0), new Vector2(500,500))},
        {Mixable.MixableType.YellowHerb, new Rect2(new Vector2(0,0), new Vector2(500,500))},
        {Mixable.MixableType.BluePot, new Rect2(new Vector2(20,560), new Vector2(500,500))},
        {Mixable.MixableType.BrownPot, new Rect2(new Vector2(560,560), new Vector2(500,500))},
        {Mixable.MixableType.GreenPot, new Rect2(new Vector2(1100,560), new Vector2(500,500))},
        {Mixable.MixableType.EmptyPot, new Rect2(new Vector2(1100,20), new Vector2(500,500))},
        {Mixable.MixableType.OrangePot, new Rect2(new Vector2(20,20), new Vector2(500,500))},
        {Mixable.MixableType.PurplePot, new Rect2(new Vector2(560,20), new Vector2(500,500))},
        {Mixable.MixableType.RedPot, new Rect2(new Vector2(560,1100), new Vector2(500,500))},
        {Mixable.MixableType.YellowPot, new Rect2(new Vector2(1100,1100), new Vector2(500,500))},

    };

    private Dictionary<Mixable.MixableType, string> _nameDict = new Dictionary<MixableType, string>()
    {
        {Mixable.MixableType.BlueHerb, "Blue bellflower"},
        {Mixable.MixableType.RedHerb, "Red mushroom"},
        {Mixable.MixableType.YellowHerb, "Yellow cactus"},
        {Mixable.MixableType.BluePot, "Blue potion"},
        {Mixable.MixableType.BrownPot, "Brown potion"},
        {Mixable.MixableType.GreenPot, "Green potion"},
        {Mixable.MixableType.EmptyPot, "Empty bottle"},
        {Mixable.MixableType.OrangePot, "Orange potion"},
        {Mixable.MixableType.PurplePot,  "Purple potion"},
        {Mixable.MixableType.RedPot,  "Red potion"},
        {Mixable.MixableType.YellowPot, "Yellow potion"}
    };

    [Signal]
    public delegate void StartedDragging(Mixable mixable);

    private Random _rand = new Random();

    public override void _Ready()
    {
        //test
        // TopLeftBorder = new Vector2(569,30);
        // BotRightBorder = new Vector2(1828,730);
        // ReceptacleArea = new Rect2(new Vector2(1655, 806), new Vector2(251, 205));
        if (HasNode("ExplodeTimer"))
        {
            GetNode("ExplodeTimer").Connect("timeout", this, nameof(OnExplode));
        }
        Visible = false;
    }

    public void InitMixable(Mixable.MixableType mixType, bool isHerb)
    {
        MixType = mixType;
        GetNode<Label>("UI/PnlName/LblName").Text = _nameDict[mixType];
        if (! isHerb)
        {
            GetNode<Sprite>("CntSprite/Sprite").RegionRect = _textureDict[mixType];
        }

        if (mixType == Mixable.MixableType.EmptyPot || isHerb)
        {
            GetNode<Control>("UI/PnlTime").Visible = false;
        }
        else
        {
            GetNode<Timer>("ExplodeTimer").WaitTime = _rand.Next(7,15);
            GetNode<Timer>("ExplodeTimer").Start();
            GetNode<Control>("UI/PnlTime").Visible = true;
        }

        GetNode<AnimationPlayer>("Anim").Play("Spawn");
    }


    public void OnCntSpriteGUIInput(InputEvent ev)
    {
        if (ev is InputEventMouseButton btn)
        {
            if (btn.ButtonIndex == (int)ButtonList.Left)
            {
                if (ev.IsPressed())
                {
                    _beingDragged = true;
                    _originalPos = GlobalPosition;
                    _currentDragDifference = GetGlobalMousePosition() - GlobalPosition;
                    EmitSignal(nameof(Mixable.StartedDragging), this);
                }
                else
                {
                    _beingDragged = false;
                    OnStoppedDragging();
                }
            }
        }
    }

    public override void _Process(float delta)
    {
        if (_beingDragged)
        {
            GlobalPosition = GetGlobalMousePosition() - _currentDragDifference;
            // GlobalPosition = new Vector2(
            //     Mathf.Clamp(GlobalPosition.x, TopLeftBorder.x, BotRightBorder.x),
            //     Mathf.Clamp(GlobalPosition.y, TopLeftBorder.y, BotRightBorder.y)
            // );
        }
        if (HasNode("ExplodeTimer"))
        {
            GetNode<Label>("UI/PnlTime/LblTime").Text = Math.Floor(GetNode<Timer>("ExplodeTimer").TimeLeft).ToString();
            if (GetNode<Timer>("ExplodeTimer").TimeLeft < _shakeAtTime && GetNode<Timer>("ExplodeTimer").TimeLeft > 0)
            {
                if (!GetNode<Tween>("ShakeTween").IsActive())
                {
                    GetNode<Tween>("ShakeTween").InterpolateProperty(GetNode<Node2D>("CntSprite"), "rotation_degrees", RotationDegrees, _rand.Next(-5,5), 0.05f);
                    GetNode<Tween>("ShakeTween").Start();
                }
            }
        }
    }

    // [Signal]
    // public delegate void Exploding(Mixable mixable);

    private void OnExplode()
    {
        Die();
    }

    [Signal]
    public delegate void DraggedOverReceptacle(MixableType mixType);


    private void OnStoppedDragging()
    {
        // first detect if over the silhouette. if not then snap back to originalpos
        if (MixType != MixableType.EmptyPot && MixType != MixableType.BlueHerb && MixType != MixableType.RedHerb && MixType != MixableType.YellowHerb)
        {
            if (GlobalPosition.x > ReceptacleArea.Position.x && GlobalPosition.y > ReceptacleArea.Position.y &&
                GlobalPosition.x < ReceptacleArea.Position.x + ReceptacleArea.Size.x && GlobalPosition.y < ReceptacleArea.Position.x + ReceptacleArea.Size.x)
            {
                EmitSignal(nameof(DraggedOverReceptacle), MixType);
                return;
            }
        }

        // if (GlobalPosition.x < TopLeftBorder.x || GlobalPosition.y < TopLeftBorder.y ||
        //  GlobalPosition.y > BotRightBorder.y || GlobalPosition.x > BotRightBorder.x)
        // {
        //     GlobalPosition = _originalPos;
        // }
        if (GlobalPosition.x < PotionArea.Position.x || GlobalPosition.y < PotionArea.Position.y ||
         GlobalPosition.y > PotionArea.Position.y + PotionArea.Size.y || GlobalPosition.x > PotionArea.Position.x + PotionArea.Size.x)
        {
            GlobalPosition = _originalPos;
        }
        if (GetOverlappingAreas().Count > 0)
        {
            foreach (Area2D area in GetOverlappingAreas())
            {
                if (area is Mixable mixable)
                {
                    // GD.Print((GlobalPosition - mixable.GlobalPosition).Length());
                    if ((GlobalPosition - mixable.GlobalPosition).Length() < _minDistance)
                    {
                        OnFoundTargetMixable(mixable);
                        break;
                    }
                }

            }
        }
    }

    public void StopExplodeTimer()
    {
        if (HasNode("ExplodeTimer"))
        {
            GetNode<Timer>("ExplodeTimer").Stop();
        }
    }

    public async void Die()
    {
        GetNode<AnimationPlayer>("Anim").Play("Die");
        await ToSignal(GetNode<AnimationPlayer>("Anim"), "animation_finished");
        QueueFree();
    }

    [Signal]
    public delegate void TryCombineWith(Mixable draggedMixable, Mixable targetMixable);

    private void OnFoundTargetMixable(Mixable target)
    {
        EmitSignal(nameof(TryCombineWith), this, target);
    }
}

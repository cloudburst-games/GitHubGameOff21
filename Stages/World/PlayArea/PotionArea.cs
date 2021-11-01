using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class PotionArea : TextureRect
{
    private List<Vector2> _positionsUsed = new List<Vector2>();
    private List<Vector2> _allPositions = new List<Vector2>();
    private Random _rand = new Random();

    public Rect2 ReceptacleArea {get; set;} = new Rect2();

    public Mixable.MixableType CurrentWinMixable = Mixable.MixableType.BlueHerb;

    public override void _Ready()
    {
        
        foreach (Node n in GetNode("SpawnPositions").GetChildren())
        {
            if (n is Position2D pos2D)
            {
                _allPositions.Add(pos2D.GlobalPosition);
            }
        }
    }

    [Signal]
    public delegate void ExplodingPotion();

    [Signal]
    public delegate void CorrectPotionOverReceptacle();

    public void GenerateMixables()
    {
       RemoveAllMixables();

        List<Vector2> shuffledPositions = _allPositions.OrderBy(x => _rand.Next()).ToList();


        for (int i = 0; i < 2; i++)
        {
            MakeNewMixable(Mixable.MixableType.BlueHerb, shuffledPositions[i]);
        }
        for (int i = 2; i < 4; i++)
        {
            MakeNewMixable(Mixable.MixableType.RedHerb, shuffledPositions[i]);
        }
        for (int i = 4; i < 6; i++)
        {
            MakeNewMixable(Mixable.MixableType.YellowHerb, shuffledPositions[i]);
        }
        for (int i = 6; i < 12; i++)
        {
            MakeNewMixable(Mixable.MixableType.EmptyPot, shuffledPositions[i]);
        }
    }

    public void RemoveAllMixables()
    {
        foreach (Node n in GetNode("Mixables").GetChildren())
        {
            n.QueueFree();
        }
    }

    private void MakeNewMixable(Mixable.MixableType mixType, Vector2 pos)
    {
        PackedScene mixableScn = (IsHerb(mixType) ? 
            ( 
                mixType == Mixable.MixableType.YellowHerb ? GD.Load<PackedScene>("res://Props/Mixable/Herbs/HerbYellow.tscn")
                : (mixType == Mixable.MixableType.RedHerb ? GD.Load<PackedScene>("res://Props/Mixable/Herbs/HerbRed.tscn")
                : GD.Load<PackedScene>("res://Props/Mixable/Herbs/HerbBlue.tscn")) 
            )
            : mixableScn = GD.Load<PackedScene>("res://Props/Mixable/Potions/Potion.tscn"));

        Mixable mixable = (Mixable) mixableScn.Instance();
        GetNode("Mixables").AddChild(mixable);
        mixable.ReceptacleArea = ReceptacleArea;
        mixable.PotionArea = new Rect2(RectGlobalPosition, RectSize);
        mixable.GlobalPosition = pos;
        mixable.Connect(nameof(Mixable.StartedDragging), this, nameof(OnStartedDragging));
        mixable.Connect(nameof(Mixable.TryCombineWith), this, nameof(OnTryCombineWith));
        mixable.Connect(nameof(Mixable.DraggedOverReceptacle), this, nameof(OnDraggedOverReceptacle));
        if (mixable.HasNode("ExplodeTimer"))
        {
            mixable.GetNode("ExplodeTimer").Connect("timeout", this, nameof(OnExplodeTimerTimeout));
        }
        mixable.InitMixable(mixType, IsHerb(mixType));
    }

    public bool IsHerb(Mixable.MixableType mixType)
    {
        if (mixType == Mixable.MixableType.BlueHerb || mixType == Mixable.MixableType.YellowHerb || mixType == Mixable.MixableType.RedHerb)
        {
            return true;
        }
        return false;
    }

    public void OnStartedDragging(Mixable mixable)
    {
        GetNode("Mixables").MoveChild(mixable, GetNode("Mixables").GetChildCount()-1);
    }

    public void OnDraggedOverReceptacle(Mixable.MixableType mixType)
    {
        foreach (Node n in GetNode("Mixables").GetChildren())
        {
            if (n is Mixable mixable)
            {
                if (!IsHerb(mixable.MixType) && mixable.MixType != Mixable.MixableType.EmptyPot)
                {
                    mixable.StopExplodeTimer();
                    mixable.Die();
                }
            }
            n.QueueFree();
        }
        if (mixType == CurrentWinMixable)
        {
            EmitSignal(nameof(CorrectPotionOverReceptacle));
        }
        else
        {
            GD.Print("bad onv receptacle");
            EmitSignal(nameof(ExplodingPotion));
        }
    }

    public void OnTryCombineWith(Mixable draggedMixable, Mixable targetMixable)
    {
        if (draggedMixable.MixType == targetMixable.MixType)
        {
            // GD.Print("cant combine same type");
            targetMixable.GetNode<AudioData>("AudioMixFailed").StartPlaying = true;
            return;
        }
        if (IsHerb(draggedMixable.MixType) && IsHerb(targetMixable.MixType))
        {
            // GD.Print("cant combine 2 herbs");
            targetMixable.GetNode<AudioData>("AudioMixFailed").StartPlaying = true;
            return;
        }
        // check for correct combinations
        foreach (Tuple<Mixable.MixableType, Mixable.MixableType> comb in _combinations.Keys)
        {
            if (comb.Item1 == draggedMixable.MixType || comb.Item2 == draggedMixable.MixType)
            {
                if (comb.Item1 == targetMixable.MixType || comb.Item2 == targetMixable.MixType)
                {
                    // GD.Print("TARGET MIX is: " + _combinations[comb].ToString());
                    draggedMixable.Die();
                    targetMixable.Die();
                    MakeNewMixable(_combinations[comb], targetMixable.GlobalPosition);
                    GetNode<AudioData>("AudioMixSuccess").StartPlaying = true;
                    // make the new pot at the target location
                    return;
                }
            }
        }

        // check for explosions
        foreach (Tuple<Mixable.MixableType, Mixable.MixableType> expComb in _explodeCombinations)
        {
            if (expComb.Item1 == draggedMixable.MixType || expComb.Item2 == draggedMixable.MixType)
            {
                if (expComb.Item1 == targetMixable.MixType || expComb.Item2 == targetMixable.MixType)
                {
                    draggedMixable.Die();
                    targetMixable.Die();
                    EmitSignal(nameof(ExplodingPotion));
                    return;
                }
            }
        }
        // brown + anything = explosion
        if (!IsHerb(draggedMixable.MixType) && !IsHerb(targetMixable.MixType))
        {
            if (draggedMixable.MixType == Mixable.MixableType.BrownPot || targetMixable.MixType == Mixable.MixableType.BrownPot)
            {

                draggedMixable.Die();
                targetMixable.Die();
                EmitSignal(nameof(ExplodingPotion));
                return;
            }
        }
    }

    public void OnExplodeTimerTimeout()
    {
        EmitSignal(nameof(ExplodingPotion));
    }


    private Dictionary<Tuple<Mixable.MixableType, Mixable.MixableType>, Mixable.MixableType> _combinations = new Dictionary<Tuple<Mixable.MixableType, Mixable.MixableType>, Mixable.MixableType>()
    {
        {Tuple.Create(Mixable.MixableType.BlueHerb, Mixable.MixableType.EmptyPot), Mixable.MixableType.BluePot},
        {Tuple.Create(Mixable.MixableType.YellowHerb, Mixable.MixableType.EmptyPot), Mixable.MixableType.YellowPot},
        {Tuple.Create(Mixable.MixableType.RedHerb, Mixable.MixableType.EmptyPot), Mixable.MixableType.RedPot},
        {Tuple.Create(Mixable.MixableType.RedPot, Mixable.MixableType.YellowPot), Mixable.MixableType.OrangePot},
        {Tuple.Create(Mixable.MixableType.RedPot, Mixable.MixableType.BluePot), Mixable.MixableType.PurplePot},
        {Tuple.Create(Mixable.MixableType.YellowPot, Mixable.MixableType.BluePot), Mixable.MixableType.GreenPot},
        {Tuple.Create(Mixable.MixableType.RedPot, Mixable.MixableType.GreenPot), Mixable.MixableType.BrownPot},
        {Tuple.Create(Mixable.MixableType.OrangePot, Mixable.MixableType.BluePot), Mixable.MixableType.BrownPot},
        {Tuple.Create(Mixable.MixableType.PurplePot, Mixable.MixableType.YellowPot), Mixable.MixableType.BrownPot},

    };
    private List<Tuple<Mixable.MixableType, Mixable.MixableType>> _explodeCombinations = new List<Tuple<Mixable.MixableType, Mixable.MixableType>>()
    {
        Tuple.Create(Mixable.MixableType.PurplePot, Mixable.MixableType.GreenPot),
        Tuple.Create(Mixable.MixableType.OrangePot, Mixable.MixableType.RedPot),
        Tuple.Create(Mixable.MixableType.OrangePot, Mixable.MixableType.YellowPot),
        Tuple.Create(Mixable.MixableType.GreenPot, Mixable.MixableType.YellowPot),
        Tuple.Create(Mixable.MixableType.GreenPot, Mixable.MixableType.BluePot),
        Tuple.Create(Mixable.MixableType.PurplePot, Mixable.MixableType.RedPot),
        Tuple.Create(Mixable.MixableType.PurplePot, Mixable.MixableType.BluePot)
    };
}

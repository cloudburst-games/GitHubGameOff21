using Godot;
using System;
using System.Collections.Generic;

public class LevelNPCManager : YSort
{
    [Signal]
    public delegate void AIPathRequested(AIUnitControlState aIUnitControlState, Vector2 worldPos);
    [Signal]
    public delegate void AIPathToPlayerRequested(AIUnitControlState aIUnitControlState);
    [Signal]
    public delegate void AIFollowPathRequested(AIUnitControlState aIUnitControlState);

    // private List<Unit> _companions = new List<Unit>();
    public override void _Ready()
    {
    }

    public void InitNPCs()
    {
        ConnectSignals();
    }

    // Call this whenever companions change. Currently called at the end of initialising level
    public void ConnectSignals()
    {
        // _companions.Clear();
        foreach (Unit unit in GetChildren())
        {
            // if (unit.CurrentUnitData.Companion)
            // {
                // _companions.Add(unit);
                if (unit.CurrentControlState is AIUnitControlState aIUnitControlState)
                {
                    // aIUnitControlState.SetAIBehaviourState(AIUnitControlState.AIBehaviour.Follow);
                    if (! aIUnitControlState.IsConnected(nameof(AIUnitControlState.PathRequested), this, nameof(OnAIPathRequested)))
                    {
                        aIUnitControlState.Connect(nameof(AIUnitControlState.PathRequested), this, nameof(OnAIPathRequested));
                    }
                    if (! aIUnitControlState.IsConnected(nameof(AIUnitControlState.PathToPlayerRequested), this, nameof(OnAIPathToPlayerRequested)))
                    {
                        aIUnitControlState.Connect(nameof(AIUnitControlState.PathToPlayerRequested), this, nameof(OnAIPathToPlayerRequested));
                    }
                    if (! aIUnitControlState.IsConnected(nameof(AIUnitControlState.FollowPathRequested), this, nameof(OnAIFollowPathRequested)))
                    {
                        aIUnitControlState.Connect(nameof(AIUnitControlState.FollowPathRequested), this, nameof(OnAIFollowPathRequested));
                    }
			    }
            // if (unit.CurrentUnitData.Hostile)
            // {
            //     unit.CurrentUnitData.MainCombatant.PlayerFaction = false;
            // }
            // }
        }
    }


    public void OnAIPathRequested(AIUnitControlState aIUnitControlState, Vector2 worldPos)
    {
        // GD.Print("levlenpcmanager path" + worldPos);
        EmitSignal(nameof(AIPathRequested), aIUnitControlState, worldPos);
    }
    public void OnAIPathToPlayerRequested(AIUnitControlState aIUnitControlState)
    {
        EmitSignal(nameof(AIPathToPlayerRequested), aIUnitControlState);
    }
    public void OnAIFollowPathRequested(AIUnitControlState aIUnitControlState)
    {
        List<Unit> companions = new List<Unit>();
        foreach (Unit unit in GetChildren())
        {
            if (unit.CurrentUnitData.Companion)
            {
                companions.Add(unit);
            }
        }
        if (companions.Count == 0)
        {
            OnAIPathToPlayerRequested(aIUnitControlState);
            return;
        }
        if (aIUnitControlState.Unit == companions[0])
        {
            // GD.Print("trying to follow player");
            // GD.Print("is companion ", aIUnitControlState.Unit.CurrentUnitData.Companion);
            OnAIPathToPlayerRequested(aIUnitControlState);
        }
        else
        {
            int index = companions.FindIndex(a => a == aIUnitControlState.Unit);
            // GD.Print("comnpanion index ", index);
            EmitSignal(nameof(AIPathRequested), aIUnitControlState, companions[index-1].GlobalPosition);
        }
    }
}
//
    //   int index = myList.FindIndex(a => a.Contains("Tennis"));
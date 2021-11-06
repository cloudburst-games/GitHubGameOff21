using Godot;
using System;
using System.Collections.Generic;
public class LevelManager : Node2D
{
    [Signal]
    public delegate void AutosaveAreaEntered();

    [Signal]
    public delegate void Announced(string message);
    [Signal]
    public delegate void NPCRightClicked(Unit npc);

    public enum Level {
        Level1, Level2
    }

    private Dictionary<Level, PackedScene> _levelSceneDict = new Dictionary<Level, PackedScene>() {
        {Level.Level1, GD.Load<PackedScene>("res://Levels/Level1/Level1.tscn")},
        {Level.Level2, GD.Load<PackedScene>("res://Levels/Level2/Level2.tscn")}
    };

    // Store all the level data whilst playing. when saving need to save all of this.
    public Dictionary<Level, LevelData> CurrentLevelData = new Dictionary<Level, LevelData>() {

    };
    public Level CurrentLevel;
    private Random _rand = new Random();

    public override void _Ready()
    {
        base._Ready();
    }

    // private void OnCurrentLevelExitedTree()
    // {
    //     EmitSignal(nameof(CurrentLevelExitedTree));
    // }

    public void InitialiseLevel(Level dest, Unit player) // this is called on transitioning, loading, and making new world
    {
        CurrentLevel = dest;
        LevelLocation newLevelLocation = (LevelLocation) _levelSceneDict[dest].Instance();
        newLevelLocation.Level = dest;
        AddChild(newLevelLocation);
        MoveChild(newLevelLocation, 0);
        if (CurrentLevelData.ContainsKey(dest))
        {
            newLevelLocation.SetFromData(CurrentLevelData[dest]);
        }
        // newLevelLocation.Connect("tree_exited", this, nameof(OnCurrentLevelExitedTree));
        // player.GlobalPosition = newLevelLocation.GetNode<Position2D>("All/PositionMarkers/PlayerPositionMarker").GlobalPosition;
        UnpackLevelData(dest, player);
        newLevelLocation.GetNode("All/Units").AddChild(player);
        GeneratePlayerCompanions(player);
        // 
        InitialiseNPCsAfterGen();
        ConnectLevelSignals(newLevelLocation);
        SetNavigation();
    }

    private void InitialiseNPCsAfterGen()
    {
        GetLevelInTree().GetNode<LevelNPCManager>("All/Units/LevelNPCManager").InitNPCs();
        foreach (Node n in GetLevelInTree().GetNode<LevelNPCManager>("All/Units/LevelNPCManager").GetChildren())
        {
            if (n is Unit npc)
            {
                npc.Connect(nameof(Unit.RightClicked), this, nameof(OnNPCRightClicked));
            }
        }
        
    }

    public void GeneratePlayerCompanions(Unit player)
    {
        for (int i = 0; i < player.CurrentUnitData.Companions.Count; i++)
        {
            player.CurrentUnitData.Companions[i].NPCPosition = GetLevelInTree().GetNode<Position2D>("All/PositionMarkers/CompanionPositionMarker" + (i+1)).GlobalPosition;
            GenerateNPC(player.CurrentUnitData.Companions[i]);
        }
    }

    private void ConnectLevelSignals(LevelLocation levelLocation)
    {  
        foreach (Node n in levelLocation.GetNode("All/TransitionMarkers").GetChildren())
        {
            if (n is LevelTransitionMarker marker)
            {
                marker.Connect(nameof(LevelTransitionMarker.TriedToTransitionTo), this, nameof(OnTriedToTransitionTo));
            }
        }
        foreach (Node n in levelLocation.GetNode("All/AutosaveAreas").GetChildren())
        {
            if (n is AutosaveArea autosaveArea)
            {
                autosaveArea.Connect(nameof(AutosaveArea.AutosaveAreaPlayerEntered), this, nameof(OnAutosaveAreaPlayerEntered));
            }
        }
    }

    private void OnAutosaveAreaPlayerEntered()
    {
        EmitSignal(nameof(AutosaveAreaEntered));
    }

    public void FreeCurrentLevel()
    { 
        LevelLocation oldLevel = (LevelLocation) GetChild(0);

        if (oldLevel.GetNode("All/Units").HasNode("Player"))
        {
            oldLevel.GetNode<Camera2D>("All/Units/Player/Camera2D").Current = false;
        }
        oldLevel.Name = "OldLevel";
        oldLevel.QueueFree();
    }

    private bool AreCompanionsWithPlayer()
    {
        int companionCount = 0;
        foreach (Node n in GetNPCManagerInTree().GetChildren())
        {
            if (n is Unit unit)
            {
                if (unit.CurrentUnitData.Companion)
                {
                    companionCount += 1;
                }
            }
        }
        int overlappingCount = 0;
        foreach (Node n in GetPlayerInTree().GetNode<Area2D>("NPCDetectArea").GetOverlappingBodies())
        {
            if (n is Unit unit)
            {
                if (unit.CurrentUnitData.Companion)
                {
                    overlappingCount += 1;
                }
            }
        }
        return overlappingCount == companionCount;
    }

    public async void OnTriedToTransitionTo(Level dest)
    {
        // do any checks needed (i.e. are we allowed to transition?)
        if (! AreCompanionsWithPlayer())
        {
            EmitSignal(nameof(Announced), "You must gather your party before venturing forth.");
            return;
        }
        
        // fade to black or do loading screen
        LoadingScreen loadingScreen = (LoadingScreen) GD.Load<PackedScene>("res://Interface/Transitions/LoadingScreen.tscn").Instance();
        AddChild(loadingScreen);
        loadingScreen.FadeIn();

        await ToSignal(loadingScreen.GetNode("FadeAnim"), "animation_finished");

        // store a ref to the player, as we are removing player from the tree before freeing the level
        Unit player = GetPlayerInTree();

        // save level data for source level
        PackLevelData(player);//GetPlayerInTree().GlobalPosition);

        // hold player node to carry between levels
        GetChild(0).GetNode("All/Units").RemoveChild(player);

        // free the current level
        FreeCurrentLevel();

        // add the level and add the player as child of destination level
        InitialiseLevel(dest, player); // player added back to tree
        GetPlayerInTree().GetNode<Camera2D>("Camera2D").Current = true;

        // load stored leveldata for destination level
        // UnpackLevelData(dest, player);
        
        // fade from black
        loadingScreen.FadeOut();        
    }

    public IStoreable PackAndGetData()
    {
        PackLevelData(GetPlayerInTree());
        return new LevelManagerData() {
            CurrentLevel = ((LevelLocation) GetChild(0)).Level,
            AllLevelData = CurrentLevelData
        };
    }

    public void UnpackAllData(LevelManagerData data)
    {
        CurrentLevelData = data.AllLevelData;
        CurrentLevel = data.CurrentLevel;
    }

    public Unit GetPlayerInTree()
    {
        return GetChild(0).GetNode<Unit>("All/Units/Player");
    }

    public LevelNPCManager GetNPCManagerInTree()
    {
        return GetLevelInTree().GetNode<LevelNPCManager>("All/Units/LevelNPCManager");
    }

    private LevelLocation GetLevelInTree()
    {
        if (GetChild(0).Name == "OldLevel")
        {
            GD.Print("Incorrect level from tree");
            throw new Exception();
        }
        return (LevelLocation)GetChild(0);
    }

    private void PackLevelData(Unit player)
    {
        LevelData sourceLevelData = new LevelData() {
            PlayerPosition = player.GlobalPosition,
            AutosaveAreaDatas = new List<Tuple<Vector2, bool>>()
        };

        // pack NPC data
        foreach (Node n in GetNPCManagerInTree().GetChildren())
        {
            if (n is Unit npc)
            {
                // if (npc.GetControlState() == Unit.ControlState.Player)
                // {
                //     continue;
                // }
                if (npc.CurrentUnitData.Companion)
                {
                    if (!player.CurrentUnitData.Companions.Contains(npc.CurrentUnitData))
                    {
                        player.CurrentUnitData.Companions.Add((UnitData)npc.PackAndGetData());
                    }
                    sourceLevelData.NPCPositions[(UnitData)npc.PackAndGetData()] = npc.CurrentUnitData.NPCPosition;
                }
                else
                {
                    sourceLevelData.NPCDatas.Add((UnitData)npc.PackAndGetData());
                }
            }
        }

        // pack autosave areas
        // consider replacing tuple with a separate data class
        foreach (Node n in GetLevelInTree().GetNode("All/AutosaveAreas").GetChildren())
        {
            if (n is AutosaveArea area)
            {
                sourceLevelData.AutosaveAreaDatas.Add(new Tuple<Vector2, bool>(area.GlobalPosition, area.Active));
            }
        }

        CurrentLevelData[((LevelLocation)GetChild(0)).Level] = sourceLevelData;
    }

    private void UnpackLevelData(Level dest, Unit player)
    {
        if (!CurrentLevelData.ContainsKey(dest))
        {
            GD.Print("Key missing for destination level. Will use defaults.");
            SetLevelDefaults(player);
            return;
        }
        player.GlobalPosition = CurrentLevelData[dest].PlayerPosition;
        foreach (Node n in GetLevelInTree().GetNode("All/AutosaveAreas").GetChildren())
        {
            n.QueueFree();
        }
        foreach (Tuple<Vector2, bool> autosaveAreaData in CurrentLevelData[dest].AutosaveAreaDatas)
        {
            AutosaveArea newAutosaveArea = (AutosaveArea) GD.Load<PackedScene>("res://Systems/SaveSystem/AutosaveArea.tscn").Instance();
            newAutosaveArea.Active = autosaveAreaData.Item2;
            newAutosaveArea.GlobalPosition = autosaveAreaData.Item1;
            GetLevelInTree().GetNode("All/AutosaveAreas").AddChild(newAutosaveArea);

        }
        foreach (Node n in GetNPCManagerInTree().GetChildren())
        {
            n.QueueFree();
        }
        foreach (UnitData unitData in CurrentLevelData[dest].NPCDatas)
        {
            GenerateNPC(unitData);
        }
    }

    private void GenerateNPC(UnitData unitData)
    {
        Unit npc = (Unit)GD.Load<PackedScene>("res://Actors/NPC/NPC.tscn").Instance();
        GetNPCManagerInTree().AddChild(npc);
        npc.CurrentUnitData = unitData;
        npc.UpdateFromUnitData();
        //Position
        if (!CurrentLevelData.ContainsKey(CurrentLevel))
        {
            npc.GlobalPosition = unitData.NPCPosition;
            return;
        }
        if (CurrentLevelData[CurrentLevel].NPCPositions.ContainsKey(unitData))
        {
            npc.GlobalPosition = CurrentLevelData[CurrentLevel].NPCPositions[unitData];
        }
        else
        {
            npc.GlobalPosition = unitData.NPCPosition;
        }
        //
    }

    public void OnNPCRightClicked(Unit npc)
    {
        EmitSignal(nameof(NPCRightClicked), npc);
    }

    private void SetLevelDefaults(Unit player)
    {        
        player.GlobalPosition = GetLevelInTree().GetNode<Position2D>("All/PositionMarkers/PlayerPositionMarker").GlobalPosition;
    }

	private void SetNavigation()
	{
		foreach (Node n in GetLevelInTree().GetNode("All/Obstacles").GetChildren())
		{
			if (n is StaticBody2D body)
			{
				foreach (Node bodyChild in body.GetChildren())
				{
					if (bodyChild is CollisionPolygon2D poly)
					{
						GetLevelInTree().GetNode<WorldNavigation>("WorldNavigation").SingleUse(poly, CurrentLevel);
					}
				}
			}
		}
		GetLevelInTree().GetNode<WorldNavigation>("WorldNavigation").Finalise();

        GetLevelInTree().GetNode<LevelNPCManager>("All/Units/LevelNPCManager").Connect
            (nameof(LevelNPCManager.AIPathRequested), GetLevelInTree().GetNode<WorldNavigation>("WorldNavigation"), 
            nameof(WorldNavigation.OnAIPathRequested));
        GetLevelInTree().GetNode<LevelNPCManager>("All/Units/LevelNPCManager").Connect
            (nameof(LevelNPCManager.AIPathToPlayerRequested), GetLevelInTree().GetNode<WorldNavigation>("WorldNavigation"), 
            nameof(WorldNavigation.OnAIPathToPlayerRequested), new Godot.Collections.Array {GetPlayerInTree()});
	}
}

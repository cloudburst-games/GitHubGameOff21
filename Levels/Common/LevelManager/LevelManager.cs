using Godot;
using System;
using System.Collections.Generic;
public class LevelManager : Node2D
{
    [Signal]
    public delegate void AutosaveAreaEntered();

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

    public override void _Ready()
    {
        base._Ready();
    }

    // private void OnCurrentLevelExitedTree()
    // {
    //     EmitSignal(nameof(CurrentLevelExitedTree));
    // }

    public void InitialiseLevel(Level dest, Unit player)
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

        ConnectLevelSignals(newLevelLocation);
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

    public async void OnTriedToTransitionTo(Level dest)
    {
        // do any checks needed (i.e. are we allowed to transition?)
        //
        
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

    public LevelManagerData PackAndGetData()
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

    private Unit GetPlayerInTree()
    {
        return GetChild(0).GetNode<Unit>("All/Units/Player");
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
            AutosaveAreas = new List<Tuple<Vector2, bool>>()
        };

        // pack autosave areas
        foreach (Node n in GetLevelInTree().GetNode("All/AutosaveAreas").GetChildren())
        {
            if (n is AutosaveArea area)
            {
                sourceLevelData.AutosaveAreas.Add(new Tuple<Vector2, bool>(area.GlobalPosition, area.Active));
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
        foreach (Tuple<Vector2, bool> autosaveAreaData in CurrentLevelData[dest].AutosaveAreas)
        {
            AutosaveArea newAutosaveArea = (AutosaveArea) GD.Load<PackedScene>("res://Systems/SaveSystem/AutosaveArea.tscn").Instance();
            newAutosaveArea.Active = autosaveAreaData.Item2;
            newAutosaveArea.GlobalPosition = autosaveAreaData.Item1;
            GetLevelInTree().GetNode("All/AutosaveAreas").AddChild(newAutosaveArea);

        }
    }

    private void SetLevelDefaults(Unit player)
    {        
        player.GlobalPosition = GetLevelInTree().GetNode<Position2D>("All/PositionMarkers/PlayerPositionMarker").GlobalPosition;
    }
}

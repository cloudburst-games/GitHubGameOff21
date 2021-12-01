using Godot;
using System;
using System.Collections.Generic;
public class LevelManager : Node2D
{
    [Signal]
    public delegate void AutosaveAreaEntered();

    [Signal]
    public delegate void Announced(string message, bool record);
    [Signal]
    public delegate void NPCRightClicked(Unit npc);
    [Signal]
    public delegate void NPCGenerated();

    [Signal]
    public delegate void LevelGenerated(Node2D terrainTilemaps);

    [Signal]
    public delegate void WorldInteractableInteracted(WorldInteractableDataSignalWrapper wrappedWorldInteractableData);

    public enum Level {
        Level1, Level2, Level3, Level4, Level5
    }

    private Dictionary<Level, PackedScene> _levelSceneDict = new Dictionary<Level, PackedScene>() {
        {Level.Level1, GD.Load<PackedScene>("res://Levels/Level1/Level1.tscn")},
        {Level.Level2, GD.Load<PackedScene>("res://Levels/Level2/Level2.tscn")},
        {Level.Level3, GD.Load<PackedScene>("res://Levels/Level3/Level3.tscn")},
        {Level.Level4, GD.Load<PackedScene>("res://Levels/Level4/Level4.tscn")},
        {Level.Level5, GD.Load<PackedScene>("res://Levels/Level5/Level5.tscn")}
    };

    // Store all the level data whilst playing. when saving need to save all of this.
    public Dictionary<Level, LevelData> CurrentLevelData = new Dictionary<Level, LevelData>() {

    };
    public Level CurrentLevel;
    private Random _rand = new Random();
    private Sprite _playerPathSprite;

    public override void _Ready()
    {
        base._Ready();
    }

    public UnitData GetNPCUnitDataByIDFromPackedLevels(string npcID)
    {
        foreach (KeyValuePair<Level, LevelData> kv in CurrentLevelData)
        {
            foreach (UnitData unitData in kv.Value.NPCDatas)
            {
                if (unitData.ID == npcID)
                {
                    return unitData;
                }
            }
        }
        return null;
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
        _playerPathSprite = (Sprite) GD.Load<PackedScene>("res://Interface/Markers/PlayerPathSprite.tscn").Instance();
        newLevelLocation.GetNode("Terrain").AddChild(_playerPathSprite);
        _playerPathSprite.Visible = false;
        GeneratePlayerCompanions(player);
        // 
        InitialiseNPCsAfterGen();
        ConnectLevelSignals(newLevelLocation);
        SetNavigation();


        EmitSignal(nameof(LevelGenerated));    
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

        foreach (Node n in levelLocation.GetNode<YSort>("All/WorldInteractables").GetChildren())
        {
            if (n is WorldInteractable worldInteractable)
            {
                worldInteractable.Connect(nameof(WorldInteractable.Interacted), this, nameof(OnWorldInteractableInteracted));
            }
        }
    }

    private void OnAutosaveAreaPlayerEntered()
    {
        EmitSignal(nameof(AutosaveAreaEntered));
    }

    public void FreeCurrentLevel()
    { 
        if (GetChildCount() == 0)
        {
            return;
        }
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

    [Signal]
    public delegate void StartedTransition();
    [Signal]
    public delegate void CompletedTransition();

    public async void OnTriedToTransitionTo(Level dest)
    {
        // do any checks needed (i.e. are we allowed to transition?)
        if (! AreCompanionsWithPlayer())
        {
            EmitSignal(nameof(Announced), "You must await your companions before travelling.", false);
            return;
        }

        EmitSignal(nameof(StartedTransition));
        
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

        EmitSignal(nameof(CompletedTransition));
    
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

    public LevelLocation GetLevelInTree()
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

        // pack shop data
        foreach (Node n in GetLevelInTree().GetNode<YSort>("All/Shops").GetChildren())
        {
            if (n is Shop shop)
            {
                sourceLevelData.ShopDatas.Add(shop.CurrentShopData);// GetShopData());
            }
        }

        // pack worldinteractable data
        foreach (Node n in GetLevelInTree().GetNode<YSort>("All/WorldInteractables").GetChildren())
        {
            if (n is WorldInteractable worldInteractable)
            {
                sourceLevelData.WorldInteractableDatas.Add(worldInteractable.CurrentWorldInteractableData);// GetShopData());
            }
        }

        CurrentLevelData[((LevelLocation)GetChild(0)).Level] = sourceLevelData;
    }

    public List<Shop> GetShops()
    {
        List<Shop> result = new List<Shop>();
        foreach (Node n in GetLevelInTree().GetNode<YSort>("All/Shops").GetChildren())
        {
            if (n is Shop shop)
            {
                result.Add(shop);
            }
        }
        return result;
    }
    public List<Tuple<Vector2, string>> GetLevelTransitionPositions()
    {
        List<Tuple<Vector2, string>> result = new List<Tuple<Vector2, string>>();
        foreach (Node n in GetLevelInTree().GetNode("All/TransitionMarkers").GetChildren())
        {
            if (n is LevelTransitionMarker marker)
            {
                result.Add(new Tuple<Vector2, string>(marker.Position, marker.ButtonLabel));
            }
        }
        return result;
    }

    public List<StaticBody2D> GetObstacles()
    {
        List<StaticBody2D> result = new List<StaticBody2D>();
        foreach (Node n in GetLevelInTree().GetNode("All/Obstacles").GetChildren())
        {
            if (n is StaticBody2D obstacle)
            {
                result.Add(obstacle);
            }
        }
        return result;
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
        foreach (Node n in GetLevelInTree().GetNode<YSort>("All/Shops").GetChildren())
        {
            n.QueueFree();
        }
        foreach (ShopData shopData in CurrentLevelData[dest].ShopDatas)
        {
            GenerateShop(shopData);
        }
        foreach (Node n in GetLevelInTree().GetNode<YSort>("All/WorldInteractables").GetChildren())
        {
            n.QueueFree();
        }
        foreach (WorldInteractableData worldInteractableData in CurrentLevelData[dest].WorldInteractableDatas)
        {
            GenerateWorldInteractable(worldInteractableData);
        }
    }

    private void GenerateNPC(UnitData unitData)
    {
        Unit npc = (Unit)GD.Load<PackedScene>("res://Actors/NPC/NPC.tscn").Instance();
        npc.CurrentUnitData = unitData;
        npc.UpdateFromUnitData();
        GetNPCManagerInTree().AddChild(npc);
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
        EmitSignal(nameof(NPCGenerated));
    }

    private void GenerateShop(ShopData shopData)
    {
        Shop shop = (Shop)GD.Load<PackedScene>("res://Props/Buildings/Shop/Shop.tscn").Instance();
        GetLevelInTree().GetNode<YSort>("All/Shops").AddChild(shop);
        shop.LoadStartingData(shopData);        
    }

    private void GenerateWorldInteractable(WorldInteractableData worldInteractableData)
    {
        WorldInteractable worldInteractable = (WorldInteractable)GD.Load<PackedScene>("res://Props/WorldInteractable/WorldInteractable.tscn").Instance();
        worldInteractable.Loaded = true;
        GetLevelInTree().GetNode<YSort>("All/WorldInteractables").AddChild(worldInteractable);
        worldInteractable.LoadStartingData(worldInteractableData);
    }

    public void OnWorldInteractableInteracted(WorldInteractableDataSignalWrapper wrappedWorldInteractableData)
    {
        EmitSignal(nameof(WorldInteractableInteracted), wrappedWorldInteractableData);
    }

    public void OnNPCRightClicked(Unit npc)
    {
        EmitSignal(nameof(NPCRightClicked), npc);
    }

    private void SetLevelDefaults(Unit player)
    {        
        player.GlobalPosition = GetLevelInTree().GetNode<Position2D>("All/PositionMarkers/PlayerPositionMarker").Position;
    }

	private void SetNavigation()
	{
        List<CollisionPolygon2D> collisionPolys = new List<CollisionPolygon2D>();
		foreach (Node n in GetLevelInTree().GetNode("All/Obstacles").GetChildren())
		{
			if (n is StaticBody2D body)
			{
				foreach (Node bodyChild in body.GetChildren())
				{
					if (bodyChild is CollisionPolygon2D poly)
					{
                        collisionPolys.Add(poly);
						// GetLevelInTree().GetNode<WorldNavigation>("WorldNavigation").SingleUse(poly, CurrentLevel);
					}
				}
			}
		}		
        foreach (Node n in GetLevelInTree().GetNode("All/Shops").GetChildren())
		{
			if (n is StaticBody2D body)
			{
				foreach (Node bodyChild in body.GetChildren())
				{
					if (bodyChild is CollisionPolygon2D poly)
					{
                        collisionPolys.Add(poly);
						// GetLevelInTree().GetNode<WorldNavigation>("WorldNavigation").SingleUse(poly, CurrentLevel);
					}
				}
			}
		}
        foreach (Node n in GetLevelInTree().GetNode("All/WorldInteractables").GetChildren())
		{
			if (n is StaticBody2D body)
			{
				foreach (Node bodyChild in body.GetChildren())
				{
					if (bodyChild is CollisionPolygon2D poly)
					{
                        collisionPolys.Add(poly);
						// GetLevelInTree().GetNode<WorldNavigation>("WorldNavigation").SingleUse(poly, CurrentLevel);
					}
				}
			}
		}
        GetLevelInTree().GetNode<WorldNavigation>("WorldNavigation").SingleUse(collisionPolys, CurrentLevel);
		GetLevelInTree().GetNode<WorldNavigation>("WorldNavigation").Finalise();

        GetLevelInTree().GetNode<LevelNPCManager>("All/Units/LevelNPCManager").Connect
            (nameof(LevelNPCManager.AIPathRequested), GetLevelInTree().GetNode<WorldNavigation>("WorldNavigation"), 
            nameof(WorldNavigation.OnAIPathRequested));
        GetLevelInTree().GetNode<LevelNPCManager>("All/Units/LevelNPCManager").Connect
            (nameof(LevelNPCManager.AIPathToPlayerRequested), GetLevelInTree().GetNode<WorldNavigation>("WorldNavigation"), 
            nameof(WorldNavigation.OnAIPathToPlayerRequested), new Godot.Collections.Array {GetPlayerInTree()});
        

        if (GetPlayerInTree().CurrentControlState is PlayerUnitControlState controlState)
        {
            controlState.Connect(nameof(PlayerUnitControlState.PathRequested), GetLevelInTree().GetNode<WorldNavigation>("WorldNavigation"), nameof(WorldNavigation.OnPlayerPathRequested));
        }
	}

    public void OnPlayerPathSet(Vector2 finalWorldPosition)
    {
        _playerPathSprite.GlobalPosition = finalWorldPosition;
        _playerPathSprite.Visible = true;
    }

    public void OnPlayerPathCleared()
    {
        _playerPathSprite.Visible = false;
    }
}

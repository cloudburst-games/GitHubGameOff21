// Terrain script
// Paint terrain with calculated tile transitions


// Also code in Grid.cs, GridVisualiser.cs, TextureAtlasData.cs, TilesetGenerator.cs
//TODO
// v0.1
// - Add instructions / guide
// - Add support for duplicates (randomly pick tile) (use a/b/c not 1-2-3)

// Ideally finish v0.1 before November jam starts

// v0.2
// - Add support for animations

// v0.3
// - Add elevation tiles and transitions

// v0.4
// - Add multiple brush sizes

// ..


using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Terrain : Node2D
{
    // determines what happens after FileDialog confirmation
    private int _saveMode = 0; // 0 = tscn, 1 = save terrainData, 2 = load terrainData
    
    // Set in ready, the size of each cell.
	private Vector2 _tileSize;
    // Start off with 0 (water) - can be changed via UI
	private int _selectedTerrainTile = 0;

    // Tilemaps are generated according to the tileset.
    [Export]
    private TileSet _currentTileset;// = GD.Load<TileSet>("res://Test3.tres");

	[Export]
	private int[] _gridSize = new int[2] {10,10};

    // Links each terrain type to the appropriate Grid tilemap (e.g. water 0 -> Level1, shore 1 -> Level1, earth 2 -> Level2) 
    private Dictionary<int, Grid> _terrainGridDict = new Dictionary<int, Grid>();

    public override void _Ready()
    {
        _tileSize = GetNode<Grid>("Tilemaps/Level1").CellSize;
        GetNode<Grid>("Tilemaps/Level1").TileSet = _currentTileset;
        // First generate the Grids (tilemaps) from each tile type in the tileset.
        GenerateTilemaps();
        // Set the scroll boundaries of the camera according to the grid size and tile size.
		FindAndSetCameraBoundaries();
        // Shows the grid using GridVisualiser based on tile and grid size.
		RenderGrid();
        // GetPackedTerrainData();
    }


    // Something like this code in wherever I need to make a grid with all the terrains (probably the "play" area)
    // private void GenerateUnifiedGrid()
    // {
    //     UnifiedGrid unifiedGrid = new UnifiedGrid();
    //     List<TileMap> tileMaps = new List<TileMap>();
    //     foreach(TileMap tileMap in GetNode("Tilemaps").GetChildren())
    //     {
    //         tileMaps.Add(tileMap);
    //     }
    //     unifiedGrid.GenGrid(tileMaps);
    // }

    // Generates each Grid (tilemap) from each tile type in the tileset.
    // First, generate a grid and set the terrain type for our starting grid (Level1)
    // Then loop through all the tile IDs, and get the name of each tile
    // (by convention, 4 digits, with 0 = water/empty and other number = terrain)
    // Duplicate the starting Grid to make a new Grid from Level2, set the name and terrain type appropriately,
    // add as a child and generate the grid.
    // Connect the signal that tells us when surrounding shore needs to be generated.
    // Finally, add an entry to our tiles-grid dictionary to link the new terrain type to the correct Grid.
    private void GenerateTilemaps()
    {
        GetNode<Grid>("Tilemaps/Level1").InitGrid(_gridSize);
        GetNode<Grid>("Tilemaps/Level1").GridTerrain = 1;
        _terrainGridDict[0] = GetNode<Grid>("Tilemaps/Level1");
        _terrainGridDict[1] = GetNode<Grid>("Tilemaps/Level1");

        // Start from level 2
        int n = 2;
        foreach (int id in _currentTileset.GetTilesIds())
        {
                if (_currentTileset.TileGetName(id).Contains(n.ToString()))
                {
                    
                    Grid newTileMap = (Grid) GetNode<Grid>("Tilemaps/Level1").Duplicate();
                    newTileMap.Name = "Level" + n;
                    newTileMap.GridTerrain = n;
                    GetNode("Tilemaps").AddChild(newTileMap);
                    newTileMap.InitGrid(_gridSize);
                    newTileMap.Connect(nameof(Grid.AtTerrainBorder), this, nameof(OnGridTileAtTerrainBorder));
                    _terrainGridDict[n] = GetNode<Grid>("Tilemaps/Level" + n);
                    n+=1;
                }
        }
        // By default, all tiles are set to 0 (water in Level1, empty in higher levels)
        // Render the tiles in Level1 to show the water at the beginning.
        // It may be that we need to render all levels if we start opening and editing terrains.
        GetNode<Grid>("Tilemaps/Level1").SetAllTileTextures();
    }

    // private void LoadTilemaps(List<Grid> grids)
    // {
    //     foreach (Node n in GetNode("Tilemaps").GetChildren())
    //     {
    //         n.Free();
    //     }
    //     for (int i = 0; i < grids.Count; i++)
    //     {
    //         Grid newTileMap = (Grid) GetNode<Grid
    //     }
    // }

    // Set the camera boundaries according to the grid size
    // Note, we may need to re-set them when the camera zooms in and out (ideally adjust this within the camera code)
	private void FindAndSetCameraBoundaries()
	{
		int topBound = 0;
		int rightBound = (int) GetNode<TileMap>("Tilemaps/Level1").MapToWorld(new Vector2(GetMainGridSize().x, 0)).x;
		int botBound = (int) GetNode<TileMap>("Tilemaps/Level1").MapToWorld(GetMainGridSize()).y;
		int leftBound = (int) GetNode<TileMap>("Tilemaps/Level1").MapToWorld(new Vector2(0, GetMainGridSize().y)).x;

		Vector2 centre = GetNode<TileMap>("Tilemaps/Level1").MapToWorld(GetMainGridSize()/2);
		GetNode<Cam2DRTS>("Cam2DRTS").SetBoundaries(topBound+450, rightBound, leftBound+900, botBound);
		GetNode<Cam2DRTS>("Cam2DRTS").Current = true;
		GetNode<Cam2DRTS>("Cam2DRTS").Position = centre;
	}

    // Draw the grid according to grid and tile sizes
	public void RenderGrid()
	{
		GetNode<GridVisualiser>("GridVisualiser").SetGrid(_tileSize, GetMainGridSize());
	}

	private Vector2 GetMainGridSize()
	{
		return new Vector2(GetNode<Grid>("Tilemaps/Level1").MainGrid.Count, GetNode<Grid>("Tilemaps/Level1").MainGrid[0].Count);
	}




    /*
    old transition rules (heroes 3 style):
// This method sets surrounding tiles according to transition rules.
	private void CalcTerrainBorders(Vector2 gridPosition, Vector2[] calcList)
	{
		int x = (int)gridPosition.x;
		int y = (int)gridPosition.y;

		// Cycle through surrounding tiles and compare current tile with surrounding tiles. 
		for (int i = 0; i <= calcList.GetUpperBound(0); i++)
		{
			int borderX = (int)calcList [i].x;
			int borderY = (int)calcList [i].y;

			// Only calculate if the border grid contains the tile
			if (BorderGridContains(calcList[i])) {
				// If current tile is water: 
				if (_borderGrid[x] [y] == 0)
				{
					// if ANY surrounding tiles are not water or not shore [if > 0], make them shore (1)
					if (_borderGrid [borderX] [borderY] > 1)
						_borderGrid [borderX] [borderY] = 1;
				}
				// If current tile is not water
				else
				{
                    
					// if ANY surrounding tiles are water [if == 0], make them shore.
					if (_borderGrid [borderX] [borderY] == 0) {
						_borderGrid [borderX] [borderY] = 1;
					}
					//If ANY surrounding tiles are not the same as current tile [if != currentTile] and are land, make them earth (2)
					else if (_borderGrid [borderX] [borderY] > 1 && _borderGrid [borderX] [borderY] != _borderGrid[x][y])
						_borderGrid [borderX] [borderY] = 2;						
				}
			}
		}

	}
    */




	// testing - for picking different terrain tiles
	private void OnOptionButtonItemSelected(int index)
	{
		_selectedTerrainTile = index;
	}

	public override void _Process(float delta)
	{
		// Crude placeholder code to disallow input if the mouse is over the UI. Adjust if we change the UI
		if (GetViewport().GetMousePosition().y < 40 || GetNode<OptionButton>("HUD/OptionButton").Pressed
			|| GetNode<FileDialog>("HUD/SaveDialog").Visible)
		{
			return;
		}
        
		if (Input.IsActionPressed("Editor Paint Terrain"))
		{
            SetTerrain();
		}
        if (Input.IsActionJustPressed("Editor Hide Grid"))
        {
            GetNode<GridVisualiser>("GridVisualiser").Visible = !GetNode<GridVisualiser>("GridVisualiser").Visible;
        }
	}

    // Set the terrain to selected when we click. Loops through all the Grids higher than the selected tile.
    public void SetTerrain()
    {
        Vector2 worldPos = GetGlobalMousePosition() - Position;
        // Loop through all of the different terrain tiles in descending order (from highest to lowest), e.g. snow (3), earth (2)...
        foreach (int terrainTile in _terrainGridDict.Keys.OrderByDescending(x => x))
        {
            // If the selected terrain is greater than or equal to the tile we are setting, then we need to make changes to that tilemap
            // E.g. if we are setting earth (2), we will adjust snow and earth tilemaps
            // We tell the Grid script at each tilemap at or above the selected terrain (_tilesGridDict[terrainTile]) that we..
            // .. want to change the terrain to _selectedTerrainTile at worldPos
            if (terrainTile >= _selectedTerrainTile)
            {
                _terrainGridDict[terrainTile].SetGridTerrain(worldPos, _selectedTerrainTile);
                
            }
        }
    }

    // This is called after a signal is emitted to say that a tile is drawn, and needs shore painted around/at it.
    private void OnGridTileAtTerrainBorder(Vector2 worldPos)
    {
        foreach (int terrainTile in _terrainGridDict.Keys.OrderByDescending(x => x))
        {
            GetNode<Grid>("Tilemaps/Level1").SetGridTerrain(worldPos, 1);
        }
    }


	private void OnSaveDialogConfirmed()
	{
        string suffix = _saveMode == 0 ? ".tscn" : ".tdat";
		string path = GetNode<FileDialog>("HUD/SaveDialog").CurrentPath;
        if (_saveMode == 2)
        {
            UnpackTerrainData(ProjectSettings.GlobalizePath(path));
            return;
        }
		if (!path.EndsWith(suffix))
		{
			path += suffix;
		}
        if (_saveMode == 0)
        {
		    SaveTscn(path);
        }
        else
        {
            SavePackedTerrainData(ProjectSettings.GlobalizePath(path));
        }
	}

    // Save the Terrain node, with child Tilemaps node, and all of its children
	private void SaveTscn(string path)
	{
		PackedScene packedScene = new PackedScene();
		Node terrain = this.Duplicate(0);
        foreach (Node n in terrain.GetChildren())
        {
            n.Free();
        }
        Node tilemaps = GetNode("Tilemaps").Duplicate(0);
        terrain.AddChild(tilemaps);
        tilemaps.Name = "Tilemaps";
        tilemaps.Owner = terrain;

        foreach (Node n in tilemaps.GetChildren())
        {
            n.Free();
        }

        for (int i = 0; i < GetNode("Tilemaps").GetChildCount(); i++)
        {
            Node grid = GetNode("Tilemaps").GetChild(i).Duplicate(0);
            tilemaps.AddChild(grid);
            grid.Name = GetNode("Tilemaps").GetChild(i).Name;
            grid.Owner = terrain;
        }

        // Node unifiedGrid = GetNode<UnifiedGrid>("UnifiedGrid").Duplicate()

		packedScene.Pack(terrain);
		ResourceSaver.Save(path, packedScene);
	}


    // Packs terrain data into a binary file
    private TerrainData SavePackedTerrainData(string path)
    {
        TerrainData terrainData = new TerrainData();
        foreach (Grid grid in GetNode("Tilemaps").GetChildren())
        {
            terrainData.GridDatas.Add(grid.GetPackedGridData());
        }
        
        Dictionary<string, object> dataDict = new Dictionary<string, object>();
        dataDict["terrain"] = terrainData;

        DataBinary dataBinary = new DataBinary();
        dataBinary.SaveBinary(dataDict, path);

        return terrainData;
    }

    // Unpacks terrain data from a binary file
    private void UnpackTerrainData(string fileName)
    {
        PackedScene gridScn = GD.Load<PackedScene>("res://Utils/LevelBuilder/Grid.tscn");
		DataBinary dataBinary = FileBinary.LoadFromFile(fileName);
        TerrainData terrainData = (TerrainData) dataBinary.Data["terrain"];

        // kill all the current tilemap children
        foreach (Grid grid in GetNode("Tilemaps").GetChildren())
        {
            grid.Free();
        }

        // Loop through each GridData (containing MainGrid and BorderGrid) in the Terrain Data
        // Make the new Grid tilemap from each one and populate the variables accordingly
        for (int i = 0; i < terrainData.GridDatas.Count; i++)
        {
            Grid newGrid = (Grid) gridScn.Instance();
            GetNode("Tilemaps").AddChild(newGrid);
            newGrid.Name = "Level" + (i+1);
            GridData gridData = terrainData.GridDatas[i];
            newGrid.MainGrid = gridData.MainGrid;
            newGrid.BorderGrid = gridData.BorderGrid;
            newGrid.SetGridSize(new int[] {gridData.MainGrid.Count, gridData.MainGrid[0].Count});
            newGrid.GridTerrain = i+1;
            newGrid.Connect(nameof(Grid.AtTerrainBorder), this, nameof(OnGridTileAtTerrainBorder));

            // link terrain appropriately to each grid
            if (i >= 2)
            {
                _terrainGridDict[i] = GetNode<Grid>("Tilemaps/Level" + i);
            }
        }

        // level 1 is water
        _terrainGridDict[0] = GetNode<Grid>("Tilemaps/Level1");
        _terrainGridDict[1] = GetNode<Grid>("Tilemaps/Level1");
        _gridSize = new int[] {GetNode<Grid>("Tilemaps/Level1").MainGrid.Count, GetNode<Grid>("Tilemaps/Level1").MainGrid[0].Count};

        // Paint all the tilemaps according to grid
        foreach (Grid grid in GetNode("Tilemaps").GetChildren())
        {
            grid.SetAllTileTextures();
        }
        // Reset camera and the grid lines
		FindAndSetCameraBoundaries();
		RenderGrid();
    }

    private void OnBtnSaveDataPressed()
    {
        _saveMode = 1;
        GetNode<FileDialog>("HUD/SaveDialog").Mode = FileDialog.ModeEnum.SaveFile;
        GetNode<FileDialog>("HUD/SaveDialog").WindowTitle = "Save Terrain Data";
        GetNode<FileDialog>("HUD/SaveDialog").PopupCentered();
    }
	private void OnBtnSavePressed()
	{
        _saveMode = 0;
        GetNode<FileDialog>("HUD/SaveDialog").Mode = FileDialog.ModeEnum.SaveFile;
        GetNode<FileDialog>("HUD/SaveDialog").WindowTitle = "Save Scene Data";
		GetNode<FileDialog>("HUD/SaveDialog").PopupCentered();
	}
	private void OnBtnLoadDataPressed()
	{
        _saveMode = 2;
        GetNode<FileDialog>("HUD/SaveDialog").Mode = FileDialog.ModeEnum.OpenFile;
        GetNode<FileDialog>("HUD/SaveDialog").WindowTitle = "Load Terrain Data";
		GetNode<FileDialog>("HUD/SaveDialog").PopupCentered();
	}

	private void OnBtnQuitPressed()
	{
		GetTree().Quit();
	}
}

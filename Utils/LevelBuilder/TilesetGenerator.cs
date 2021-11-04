using Godot;
using System;
using Newtonsoft.Json;

public class TilesetGenerator : Node2D
{
	private TextureAtlasData _tileAtlasData = new TextureAtlasData();
    [Export]
	private Resource _tileAtlas = ResourceLoader.Load("res://Utils/LevelBuilder/Tilesheet.png");

    // [Export]
    // private File _JSONPath;

    private string _tileData;

    public override void _Ready()
    {
        string JSONpath = ProjectSettings.GlobalizePath(_tileAtlas.ResourcePath);
        JSONpath = JSONpath.Substring(0,JSONpath.Length-3);
        JSONpath += "json";
        // GD.Print(JSONpath);// JSONpath);
		// // Read and convert the JSON file holding terrain atlas data
		_tileData = System.IO.File.ReadAllText (JSONpath);
		JsonConvert.PopulateObject(_tileData,_tileAtlasData);
        // MakeTileSet();
        GetNode<FileDialog>("FileDialog").PopupCentered();
    }

	// This method generates the terrain tiles from the json file. don't really need it here, was used only for gamedev
	// It can be generalised and go into the utility class
	public void MakeTileSet(string path)
	{
		// PackedScene packedScene = new PackedScene ();
		// Node node = new Node ();
		// Vector2 regionSize = new Vector2 (256, 128);
		// Vector2 regionPos = new Vector2 (0, 0);
		// foreach (TextureAtlasData.Frame fr in _tileAtlasData.frames)
		// {
		// 	string name = fr.filename.Split ('.') [0];
		// 	Sprite sprite = new Sprite ();
        //     sprite.Name = name;
        //     sprite.Texture = (Texture) _tileAtlas;
        //     sprite.RegionEnabled = true;
		// 	regionPos = new Vector2 (fr.frame ["x"], fr.frame ["y"]);
		// 	sprite.RegionRect = new Rect2 (regionPos, regionSize);
		// 	node.AddChild (sprite);
		// 	sprite.Owner = node;
		// }

		// packedScene.Pack (node);
		// ResourceSaver.Save ("res://SampleTerrainTiles2.tscn", packedScene);

        
        // streamline above by just generating the tileset...

        // TileSet tileset = new TileSet();
        // for (int i = 0; i < 10000; i++)
        // {
        //     tileset.CreateTile(i);
        //     tileset.TileSetTexture(i, ...)
        // }
        // TileSet blah = new TileSet();
        // blah.CreateTile(0);
        TileSet tileset = new TileSet();
		Vector2 regionSize = new Vector2 (256, 128);
		Vector2 regionPos = new Vector2 (0, 0);
		foreach (TextureAtlasData.Frame fr in _tileAtlasData.frames)
		{
            int id = tileset.GetLastUnusedTileId();
			string name = fr.filename.Split ('.') [0];

            tileset.CreateTile(id);
            tileset.TileSetName(id, name);
            tileset.TileSetTexture(id, (Texture)_tileAtlas);
            
			regionPos = new Vector2 (fr.frame ["x"], fr.frame ["y"]);
			tileset.TileSetRegion(id, new Rect2 (regionPos, regionSize));
		}

		ResourceSaver.Save (path + ".tres", tileset);

        GetNode<Button>("BtnDone").Visible = true;
	}

    public void OnFileDialogConfirmed()
    {
        string path = GetNode<FileDialog>("FileDialog").CurrentPath;
        MakeTileSet(path);
    }

    public void OnBtnDonePressed()
    {
        GetTree().Quit();
    }
}

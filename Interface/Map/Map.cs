using Godot;
using System;
using System.Collections.Generic;

public class Map : Control
{
    private Cam2DRTS _camera;
    private ViewportContainer _viewportContainer;
    private Control _allContainer;
    private Texture _playerSymbol;
    private Texture _NPCSymbol;
    private Texture _levelTransitionSymbol;
    private Vector2 _playerPos = new Vector2();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _camera = GetNode<Cam2DRTS>("Panel/ViewportContainer/Viewport/Terrain/Cam2DRTS");
        _viewportContainer = GetNode<ViewportContainer>("Panel/ViewportContainer");
        _allContainer = GetNode<Control>("Panel/ViewportContainer/Viewport/Terrain/All");
        _playerSymbol = GD.Load<Texture>("res://Interface/Map/Art/GHGO21MapSymbols/PlayerMarker.PNG");
        _NPCSymbol = GD.Load<Texture>("res://Interface/Map/Art/GHGO21MapSymbols/NPCMarker.PNG");
        _levelTransitionSymbol = GD.Load<Texture>("res://Interface/Map/Art/GHGO21MapSymbols/LevelTransition.PNG");
        Visible = false;
        //TEST

        // Start(new Vector2(500,1000));

    }

    public void UpdateCamera()
    {
        Vector2 topGridPos = new Vector2(GetCurrentTileMap().GetUsedRect().Position.x,
            GetCurrentTileMap().GetUsedRect().Position.y);
        Vector2 botGridPos = new Vector2(
            GetCurrentTileMap().GetUsedRect().Position.x + GetCurrentTileMap().GetUsedRect().Size.x,
            GetCurrentTileMap().GetUsedRect().Position.y + GetCurrentTileMap().GetUsedRect().Size.y
        );
        Vector2 leftGridPos = new Vector2(GetCurrentTileMap().GetUsedRect().Position.x, GetCurrentTileMap().GetUsedRect().Position.y + GetCurrentTileMap().GetUsedRect().Size.y);
        Vector2 rightGridPos = new Vector2(GetCurrentTileMap().GetUsedRect().Position.x + GetCurrentTileMap().GetUsedRect().Size.x, GetCurrentTileMap().GetUsedRect().Position.y);
        Vector2 topWorldPos = GetCurrentTileMap().MapToWorld(topGridPos);
        Vector2 botWorldPos = GetCurrentTileMap().MapToWorld(botGridPos);
        Vector2 leftWorldPos = GetCurrentTileMap().MapToWorld(leftGridPos);
        Vector2 rightWorldPos = GetCurrentTileMap().MapToWorld(rightGridPos);
        _camera.SetNewBoundaries(leftWorldPos, topWorldPos, rightWorldPos, botWorldPos);
        _camera.DragOnly = true;
        _camera.Current = true;
        _camera.Zoom = new Vector2(2,2);
    }

    public TileMap GetCurrentTileMap()
    {
        return GetNode<TileMap>("Panel/ViewportContainer/Viewport/Terrain/Tilemaps/Level1");
    }

    public void Start()
    {
        UpdateCamera();
        _camera.Active = true;        
    }

    public void Show(Vector2 playerPos)
    {
        Visible = true;
        _camera.Position = playerPos;
    }
    
    public override void _Input(InputEvent ev)
    {
        base._Input(ev);

        // if (ev.IsActionPressed("Pause") && !ev.IsEcho() && Visible)
        // {
        //     OnMapBtnClosePressed();
        // }
    }

    // if performance is an issue consider object pooling (currently making up to 6000 objects which are reference counted and removed on free)
    public void Update(Vector2 playerPos, Unit.FacingDirection dir, List<Vector2> npcPositions, List<Shop> shops, List<StaticBody2D> obstacles,
        List<Vector2> transitionPositions)
    {
        _playerPos = playerPos;
        ClearAll();
        GenerateObstacleSprites(obstacles);
        GenerateNPCSymbols(npcPositions);
        GenerateShopSprites(shops);
        GenerateLevelTransitionSymbols(transitionPositions);
        GeneratePlayerSymbol(playerPos, dir);
    }

    private void ClearAll()
    {
        foreach (Node n in _allContainer.GetChildren())
        {
            n.QueueFree();
        }
    }
	public Dictionary<Unit.FacingDirection,float> DirectionsByRotation {get; set;} = new Dictionary<Unit.FacingDirection,float>()
	{
		{Unit.FacingDirection.Right, 1.6f},
		{Unit.FacingDirection.DownRight, 2.4f},
		{Unit.FacingDirection.Down,3.1f },
		{Unit.FacingDirection.DownLeft, 3.9f},
		{Unit.FacingDirection.Left, 4.7f},
		{Unit.FacingDirection.UpLeft, 5.5f},
		{Unit.FacingDirection.Up, 0},
		{Unit.FacingDirection.UpRight, 0.8f}
	};

    private void GeneratePlayerSymbol(Vector2 playerPos, Unit.FacingDirection dir)
    {
        TextureRect texRect = new TextureRect();
        texRect.RectPosition = playerPos - new Vector2(32,32);
        texRect.Texture = _playerSymbol;
        texRect.RectRotation = Mathf.Rad2Deg(DirectionsByRotation[dir]);
        texRect.RectPivotOffset = new Vector2(32,32);
        texRect.RectSize = new Vector2(64,64);
        texRect.Expand = true;
        // texRect.HintTooltip = "Player";
        ShaderMaterial shaderMaterial = (ShaderMaterial) GD.Load<ShaderMaterial>("res://Shaders/Flash/FlashShader.tres").Duplicate();
        shaderMaterial.SetShaderParam("speed", 10f);
        shaderMaterial.SetShaderParam("flash_colour_original", new Color(1,1,1));
        shaderMaterial.SetShaderParam("flash_depth", 0.5f);
        texRect.Material = shaderMaterial;
        _allContainer.AddChild(texRect);
        texRect.Connect("mouse_entered", this, nameof(OnMouseEnteredSymbol), new Godot.Collections.Array {"Player"});
    }

    private void OnMouseEnteredSymbol(string hintToolTip)
    {
        GetNode<Label>("Panel/PnlStatus/LblStatus").Text = hintToolTip;
    }

    private void GenerateNPCSymbols(List<Vector2> npcPositions)
    {
        foreach (Vector2 pos in npcPositions)
        {
            TextureRect texRect = new TextureRect();
            texRect.RectPosition = pos - new Vector2(32,32);
            texRect.Texture = _NPCSymbol;
            texRect.RectSize = new Vector2(64,64);
            texRect.Expand = true;
            ShaderMaterial shaderMaterial = (ShaderMaterial) GD.Load<ShaderMaterial>("res://Shaders/Flash/FlashShader.tres").Duplicate();
            shaderMaterial.SetShaderParam("speed", 5);
            shaderMaterial.SetShaderParam("flash_colour_original", new Color(1,1,1));
            shaderMaterial.SetShaderParam("flash_depth", 0.25f);
            texRect.Material = shaderMaterial;
            _allContainer.AddChild(texRect);
            texRect.Connect("mouse_entered", this, nameof(OnMouseEnteredSymbol), new Godot.Collections.Array {"NPC"});
        }
    }

    private void GenerateObstacleSprites(List<StaticBody2D> obstacles)
    {
        foreach (StaticBody2D obstacle in obstacles)
        {
            Sprite sprite = (Sprite) obstacle.GetNode<Sprite>("Sprite").Duplicate();
            sprite.Texture = obstacle.GetNode<Sprite>("Sprite").Texture;
            // sprite.Scale;
            sprite.Centered = true;
            sprite.Position = obstacle.Position;// + new Vector2(32,32);
            _allContainer.AddChild(sprite);
        }
    }

    private void GenerateShopSprites(List<Shop> shops)
    {
        foreach (Shop shop in shops)
        {
            TextureRect texRect = new TextureRect();
            texRect.RectScale = shop.GetNode<Sprite>("Sprite").Scale;
            texRect.RectPosition = shop.Position;
            texRect.RectPosition += shop.GetNode<Sprite>("Sprite").Offset * shop.GetNode<Sprite>("Sprite").Scale * 0.5f;
            texRect.Texture = shop.GetNode<Sprite>("Sprite").Texture;
            texRect.RectPosition -= texRect.Texture.GetSize()*0.5f*texRect.RectScale;
            ShaderMaterial shaderMaterial = (ShaderMaterial) GD.Load<ShaderMaterial>("res://Shaders/Flash/FlashShader.tres").Duplicate();
            shaderMaterial.SetShaderParam("speed", 5);
            shaderMaterial.SetShaderParam("flash_colour_original", new Color(1,1,1));
            shaderMaterial.SetShaderParam("flash_depth", 0.25f);
            texRect.Material = shaderMaterial;
            // texRect.RectPivotOffset = (texRect.Texture.GetSize()*RectScale)/2f ;
            _allContainer.AddChild(texRect);
            texRect.Connect("mouse_entered", this, nameof(OnMouseEnteredSymbol), new Godot.Collections.Array {"Shop"});
        }
    }
    
    private void GenerateLevelTransitionSymbols(List<Vector2> transitionPositions)
    {
        foreach (Vector2 pos in transitionPositions)
        {
            TextureRect texRect = new TextureRect();
            texRect.RectPosition = pos - new Vector2(32,32);
            texRect.Texture = _levelTransitionSymbol;
            texRect.RectSize = new Vector2(64,64);
            texRect.Expand = true;
            ShaderMaterial shaderMaterial = (ShaderMaterial) GD.Load<ShaderMaterial>("res://Shaders/Flash/FlashShader.tres").Duplicate();
            shaderMaterial.SetShaderParam("speed", 5);
            shaderMaterial.SetShaderParam("flash_colour_original", new Color(1,1,1));
            shaderMaterial.SetShaderParam("flash_depth", 0.25f);
            texRect.Material = shaderMaterial;
            _allContainer.AddChild(texRect);
            texRect.Connect("mouse_entered", this, nameof(OnMouseEnteredSymbol), new Godot.Collections.Array {"Level Transition"});
        }
    }

    public void OnClose()
    {
        _camera.Active = false;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    // public void UpdateCameraPosition(
    // {

    // }
}

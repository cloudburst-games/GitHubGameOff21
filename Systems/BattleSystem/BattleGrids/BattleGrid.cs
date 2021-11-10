using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public class BattleGrid : Control
{
    private TileMap _overallTileMap;
    private AStar2D _aStar = new AStar2D();
    private Vector2 _cellSize = new Vector2(256,128);
    private Vector2 _halfCellSize;
    public Godot.Collections.Array TraversableCells {get; set;}
    public Godot.Collections.Array ObstacleCells {get; set;}
    // private Godot.Collections.Array _obstacles;
    // private Rect2 _usedRect;
    private Dictionary<Vector2, int> _pointIDs = new Dictionary<Vector2, int>();

    // private List<Sprite> _movePoints = new List<Sprite>();
    // private List<Sprite> _attackAPPoints = new List<Sprite>();

    private Texture[] _movePointTextures = new Texture[4] {
        GD.Load<Texture>("res://Systems/BattleSystem/BattleGrids/ArrowBLYellow.png"),
        GD.Load<Texture>("res://Systems/BattleSystem/BattleGrids/ArrowBRYellow.png"),
        GD.Load<Texture>("res://Systems/BattleSystem/BattleGrids/ArrowTRYellow.png"),
        GD.Load<Texture>("res://Systems/BattleSystem/BattleGrids/ArrowTLYellow.png"),
    };
    private Texture _movePointTex = GD.Load<Texture>("res://Systems/BattleSystem/GridMovePoint.png");
    private Texture _attackPointTex = GD.Load<Texture>("res://Systems/BattleSystem/GridAttackAPPoint.png");

    public override void _Ready()
    {
        _overallTileMap = GetNode<TileMap>("TileMapAll");
        _halfCellSize = _cellSize/2f;
        TraversableCells = _overallTileMap.GetUsedCellsById(0);
        ObstacleCells = _overallTileMap.GetUsedCellsById(1);
        // _obstacles = _overallTileMap.GetUsedCellsById(1);
        // _usedRect = _overallTileMap.GetUsedRect();

        SetPointIDs();
        AddCells();
        ConnectCells();
    }

    public Vector2[] CalculatePath(Vector2 mapStart, Vector2 mapEnd)
    {
        if (!TraversableCells.Contains(mapStart) || ! TraversableCells.Contains(mapEnd))
        {
            return new Vector2[0];
        }
        int startIndex = _pointIDs[mapStart];
        int endIndex = _pointIDs[mapEnd];
        return _aStar.GetPointPath(startIndex, endIndex);
    }

    private void SetPointIDs()
    {
        int pIDCount = 0;
        foreach (Vector2 point in TraversableCells)
        {
            _pointIDs.Add(point, pIDCount);
            pIDCount += 1;
        }
    }

    private void AddCells()
    {
        foreach (Vector2 point in TraversableCells)
        {
            _aStar.AddPoint(_pointIDs[point], point);
        }
    }

    private void ConnectCells()
    {
        foreach (Vector2 point in TraversableCells)
        {
            foreach (Vector2 neighbour in GetHorizontalNeighbours(point))
            {
                if (!TraversableCells.Contains(neighbour))
                {
                    continue;
                }
                _aStar.ConnectPoints(_pointIDs[point], _pointIDs[neighbour], true);
            }
        }
    }

    private List<Vector2> GetHorizontalNeighbours(Vector2 cell) // commented out diagonal cells
    {
        return new List<Vector2>() {
            new Vector2(cell.x - 1, cell.y),
            new Vector2(cell.x +1, cell.y),
            // new Vector2(cell.x - 1, cell.y -1),
            new Vector2(cell.x, cell.y - 1),
            // new Vector2(cell.x + 1, cell.y - 1),
            // new Vector2(cell.x - 1, cell.y + 1),
            new Vector2(cell.x, cell.y + 1),
            // new Vector2(cell.x + 1, cell.y + 1)
        };
    }   
    
    private List<Vector2> GetHexNeighbours(Vector2 cell) // commented out diagonal cells
    {
        return new List<Vector2>() {
            new Vector2(cell.x, cell.y-1),
            new Vector2(cell.x +1, cell.y),
            new Vector2(cell.x + 1, cell.y +1),
            new Vector2(cell.x, cell.y + 1),
            new Vector2(cell.x - 1, cell.y + 1),
            // new Vector2(cell.x - 1, cell.y + 1),
            new Vector2(cell.x -1, cell.y ),
            // new Vector2(cell.x + 1, cell.y + 1)
        };
    }
    private List<Vector2> GetAllNeighbours(Vector2 cell)
    {
        return new List<Vector2>() {
            new Vector2(cell.x - 1, cell.y),
            new Vector2(cell.x +1, cell.y),
            new Vector2(cell.x - 1, cell.y -1),
            new Vector2(cell.x, cell.y - 1),
            new Vector2(cell.x + 1, cell.y - 1),
            new Vector2(cell.x - 1, cell.y + 1),
            new Vector2(cell.x, cell.y + 1),
            new Vector2(cell.x + 1, cell.y + 1)
        };
    }

    public bool IsTraversable(Vector2 mapPos)
    {
        if (TraversableCells.Contains(mapPos))
        {
            return true;
        }
        return false;
    }

    private void GenerateMovePointSprite(Vector2 mapPos, Vector2 nextmapPos)
    {
        return;
        var sprite = new Sprite() {
            // Texture = _movePointTex
        };
        GetNode("BattleGridUI/MovePoints").AddChild(sprite);
        sprite.Position = new Vector2(_overallTileMap.MapToWorld(mapPos).x,  _overallTileMap.MapToWorld(mapPos).y + 1.5f*_halfCellSize.y) ;
        if (nextmapPos != mapPos)
        {
            float angle = mapPos.AngleToPoint(nextmapPos) - Mathf.Pi/2f;
            switch (angle)
            {
                case 0:
                    sprite.Texture = _movePointTextures[2];
                    break;
                case -Mathf.Pi/2f:
                    sprite.Texture = _movePointTextures[3];
                    break;
                case -Mathf.Pi:
                    sprite.Texture = _movePointTextures[0];
                    break;
                case Mathf.Pi/2f:
                    sprite.Texture = _movePointTextures[1];
                    break;

            }
        }

        // make the sprite red - if we want to show AP for instance
        sprite.Modulate = new Color(1,0,0);
    }

    private void ClearAllPointSprites()
    {
        foreach (Node n in GetNode("BattleGridUI/MovePoints").GetChildren())
        {
            n.QueueFree();
        }
        foreach (Node n in GetNode("BattleGridUI/AttackAPPoints").GetChildren())
        {
            n.QueueFree();
        }
    }

    public void DrawMovePoints(Vector2[] points)
    {
        ClearAllPointSprites();
        for (int i = 1; i < points.Count(); i++)
        {
            GenerateMovePointSprite(points[i], i+1 < points.Count() ? points[i+1] : points[i]);
        }
        // foreach (Vector2 vec in points)
        // {
        //     GenerateMovePointSprite(vec);
        // }
    }

    // For testing:
    private void PrintPathToConsole(Vector2[] path)
    {
        string s = "";
        foreach (Vector2 vec in path)
        {
            s += vec.ToString();
            s += vec == path[path.Count()-1] ? "." : ", ";
        }
        GD.Print(s);
    }

    public override void _Input(InputEvent ev)
    {
        if (TraversableCells.Contains(_overallTileMap.WorldToMap(GetGlobalMousePosition())))
        {
            // DrawMovePoints(CalculatePath(new Vector2(7, 6), _overallTileMap.WorldToMap(GetGlobalMousePosition())));
        }
        if (ev is InputEventMouseButton btn)
        {
            if (btn.ButtonIndex == (int) ButtonList.Left)
            {
                if (ev.IsPressed() && !ev.IsEcho())
                {
                    // GD.Print(GetCorrectedGridPosition(GetGlobalMousePosition()));
                    // GD.Print(_overallTileMap.WorldToMap(GetGlobalMousePosition()));
                }
            }
        }
    }

    public int GetDistanceToPoint(Vector2 gridStartPos, Vector2 gridEndPos)
    {
        return CalculatePath(gridStartPos, gridEndPos).Count()-1;
    }

    public Vector2 GetTileCentreFromPosition(Vector2 position)
    {
        return GetCorrectedWorldPosition(GetCorrectedGridPosition(position));// MapToWorld(_overallTileMap.WorldToMap(position)) + new Vector2(0, _halfCellSize.y);
    }

    public Vector2 GetCorrectedGridPosition(Vector2 position)
    {
        return _overallTileMap.WorldToMap(new Vector2(position.x-_overallTileMap.GlobalPosition.x, position.y-_overallTileMap.GlobalPosition.y));
    }
    public Vector2 GetCorrectedWorldPosition(Vector2 gridPosition)
    {
        return _overallTileMap.MapToWorld(new Vector2(gridPosition.x, gridPosition.y)) + _overallTileMap.GlobalPosition;
    }
}

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class BattleGrid : Control
{
    private TileMap _overallTileMap;
    private AStar2D _aStar = new AStar2D();
    private Vector2 _cellSize = new Vector2(256,128);
    private Vector2 _halfCellSize;
    public Godot.Collections.Array TraversableCells {get; set;}
    public Godot.Collections.Array ObstacleCells {get; set;}
    private Dictionary<Vector2, int> _pointIDs = new Dictionary<Vector2, int>();

    public override void _Ready()
    {
        _overallTileMap = GetNode<TileMap>("TileMapAll");
        _halfCellSize = _cellSize/2f;

        GetNode<TileMap>("TileMapShadedTiles").Clear();
        GetNode<TileMap>("TileMapShadedTilesLong").Clear();
        GetNode<TileMap>("TileMapShadedTilesPath").Clear();
        GetNode<TileMap>("TileMapShadedTilesAOE").Clear();

    }

    public Vector2[] CalculatePath(Vector2 mapStart, Vector2 mapEnd)
    {
        if (!TraversableCells.Contains(mapStart) || ! TraversableCells.Contains(mapEnd))
        {
            return new Vector2[0];
        }
        int startIndex = _pointIDs[mapStart];
        int endIndex = _pointIDs[mapEnd];
        // GD.Print("*");
        // foreach (Vector2 point in _aStar.GetPointPath(startIndex, endIndex))
        // {
        //     GD.Print(point);
        // }
        // GD.Print("*");

        // float xDiff = Math.Abs(mapStart.x - mapEnd.x);
        // float yDiff = Math.Abs(mapStart.y - mapEnd.y);
        // bool diagonal = Math.Abs(xDiff - yDiff) <= 1;
        // GD.Print("diagonal: ", diagonal);
        // GD.Print(IsPathStraight(mapStart, mapEnd));
        return _aStar.GetPointPath(startIndex, endIndex);
    }

    int _pIDCount = 0;
    private void SetPointIDs()
    {
        _pIDCount = 0;
        foreach (Vector2 point in TraversableCells)
        {
            _pointIDs.Add(point, _pIDCount);
            _pIDCount += 1;
        }
    }

    private List<Vector2> _lastAdditionalObstacles = new List<Vector2>();

    public void RecalculateAStarMap(List<Vector2> additionalObstacles, bool includeObstacles = true)
    {
        _aStar.Clear();
        _pointIDs.Clear();
        if (additionalObstacles != null)
        {
            _lastAdditionalObstacles = additionalObstacles;
        }
        TraversableCells = _overallTileMap.GetUsedCellsById(0);
        ObstacleCells = _overallTileMap.GetUsedCellsById(1);
        if (additionalObstacles != null)
        {
            foreach (Vector2 obstaclePoint in additionalObstacles)
            {
                ObstacleCells.Add(obstaclePoint);
            }
        }
        if (!includeObstacles)
        {
            ObstacleCells.Clear();
            foreach (Vector2 cell in _overallTileMap.GetUsedCellsById(1))
            {
                TraversableCells.Add(cell);
            }
        }
        foreach (Vector2 point in ObstacleCells)
        {
            if (TraversableCells.Contains(point))
            {
                TraversableCells.Remove(point);
            }
        }
        SetPointIDs();
        AddCells();
        ConnectCells();
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

    public List<Vector2> GetAllCells()
    {
        List<Vector2> result = new List<Vector2>();
        foreach (Vector2 point in TraversableCells)
        {
            result.Add(point);
        }
        foreach (Vector2 point in ObstacleCells)
        {
            result.Add(point);
        }
        return result;
    }

    public List<Vector2> GetHorizontalNeighbours(Vector2 cell) // commented out diagonal cells
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
        if (TraversableCells == null)
        {
            // RecalculateAStarMap(new List<Vector2>());
            return;
        }
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
    // public int GetDistanceToPointNoUnitObstacles(Vector2 gridStartPos, Vector2 gridEndPos)
    // {
    //     return CalculatePathNoUnitObstacles(gridStartPos, gridEndPos).Count()-1;
    // }

    public int GetDistanceToPointNoTargetObstacle(Vector2 gridStartPos, Vector2 gridEndPos)
    {
        AddSinglePoint(gridEndPos);
        TraversableCells.Add(gridEndPos);
        
        int distance = CalculatePath(gridStartPos, gridEndPos).Count()-1;
        TraversableCells.Remove(gridEndPos);
        RemoveSinglePoint(gridEndPos);
        return distance;
    }

    public bool IsDistanceEqualWithOrWithoutObstacles(Vector2 gridStartPos, Vector2 gridEndPos)
    {
        if (GetDistanceToPointNoTargetObstacle(gridStartPos, gridEndPos) == 
            GetDistanceToPointNoAnyObstacles(gridStartPos, gridEndPos))
        {
            return true;
        }
        return false;
    }

    // public bool IsPathStraight
    public bool IsPathStraight(Vector2 gridStartPos, Vector2 gridEndPos)
    {
        // check if diagonal
        float xDiff = Math.Abs(gridStartPos.x - gridEndPos.x);
        float yDiff = Math.Abs(gridStartPos.y - gridEndPos.y);
        bool diagonal = Math.Abs(xDiff - yDiff) <= 2;
        GD.Print("diagonal: ", diagonal);
        // check if straight
        bool straight = gridStartPos.x == gridEndPos.x || gridStartPos.y == gridEndPos.y;
        GD.Print("straight: ", straight);
        // check if distance equal
        bool equal = IsDistanceEqualWithOrWithoutObstacles(gridStartPos, gridEndPos);
        GD.Print("equal: ", equal);
        GD.Print("overall..:");
        return (diagonal || straight ) && equal;
    }

    public int GetDistanceToPointNoAnyObstacles(Vector2 gridStartPos, Vector2 gridEndPos)
    {
        RecalculateAStarMap(null, false);
        int distance = CalculatePath(gridStartPos, gridEndPos).Count()-1;
        RecalculateAStarMap(_lastAdditionalObstacles, true);
        return distance;
    }

    private void AddSinglePoint(Vector2 point)
    {
        if (_pointIDs.ContainsKey(point))
        {
            return;
        }
        _pointIDs.Add(point, _pIDCount);
        _pIDCount += 1;
        _aStar.AddPoint(_pointIDs[point], point);
        foreach (Vector2 neighbour in GetHorizontalNeighbours(point))
        {
            if (!TraversableCells.Contains(neighbour))
            {
                continue;
            }
            _aStar.ConnectPoints(_pointIDs[point], _pointIDs[neighbour], true);
        }
    }

    private void RemoveSinglePoint(Vector2 point)
    {
        if (!_pointIDs.ContainsKey(point))
        {
            return;
        }
        foreach (Vector2 neighbour in GetHorizontalNeighbours(point))
        {
            if (!_pointIDs.ContainsKey(neighbour))
            {
                continue;
            }
            if (_aStar.ArePointsConnected(_pointIDs[point], _pointIDs[neighbour]))
            {
                _aStar.DisconnectPoints(_pointIDs[point], _pointIDs[neighbour]);
            }
        }
        _aStar.RemovePoint(_pointIDs[point]);
        _pointIDs.Remove(point);
    }

    public Vector2 GetCentredWorldPosFromWorldPos(Vector2 position)
    {
        return GetCorrectedWorldPosition(GetCorrectedGridPosition(position));// MapToWorld(_overallTileMap.WorldToMap(position)) + new Vector2(0, _halfCellSize.y);
    }

    public Vector2 GetCorrectedGridPosition(Vector2 position)
    {
        return _overallTileMap.WorldToMap(new Vector2(position.x-_overallTileMap.Position.x, position.y-_overallTileMap.Position.y));
    }
    public Vector2 GetCorrectedWorldPosition(Vector2 gridPosition)
    {
        return _overallTileMap.MapToWorld(new Vector2(gridPosition.x, gridPosition.y)) + _overallTileMap.Position;
    }
}

using Godot;
using System;

public class LevelLocation : Node2D
{
    public LevelManager.Level Level {get; set;}

    public void SetFromData(LevelData levelData)
    {
        GetNode<Position2D>("All/PositionMarkers/PlayerPositionMarker").GlobalPosition = levelData.PlayerPosition;
    }

    [Export]
    public string LevelName {get; set;} = "Level Name";
}

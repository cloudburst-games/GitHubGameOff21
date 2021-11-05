using Godot;
using System;
using System.Collections.Generic;

[Serializable()]
public class LevelData : IStoreable
{
    public Vector2 PlayerPosition {get; set;}
    public List<Tuple<Vector2, bool>> AutosaveAreas;
}

using Godot;
using System;
using System.Collections.Generic;

[Serializable()]
public class LevelData : IStoreable
{
    public Vector2 PlayerPosition {get; set;}
    public Dictionary<UnitData, Vector2> NPCPositions {get; set;} = new Dictionary<UnitData, Vector2>();
    public List<Tuple<Vector2, bool>> AutosaveAreaDatas {get; set;}
    public List<UnitData> NPCDatas {get; set;} = new List<UnitData>();
    public List<ShopData> ShopDatas {get; set;} = new List<ShopData>();
    public List<WorldInteractableData> WorldInteractableDatas {get; set;} = new List<WorldInteractableData>();
}

using Godot;
using System;
using System.Collections.Generic;

[Serializable()]
public class LevelManagerData : IStoreable
{
    public LevelManager.Level CurrentLevel;
    public Dictionary<LevelManager.Level, LevelData> AllLevelData;
}

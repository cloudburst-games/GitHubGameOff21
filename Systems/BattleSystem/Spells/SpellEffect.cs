using Godot;
using System;
using System.Collections.Generic;

public class SpellEffect : Reference
{
    public enum TargetMode { Hostile, Ally, Area, Empty}
    public string Name = "";
    public int RangeSquares = 1;
    public int DurationRounds = 0;
    public float Magnitude = 5f;
    public int AreaSquares = 1;
    public float ManaCost = 5f;
    public List<BattleUnitData.DerivedStat> TargetStats {get; set;} = new List<BattleUnitData.DerivedStat>() 
        {BattleUnitData.DerivedStat.Health};
    public TargetMode Target = TargetMode.Hostile;

    public PackedScene ArtEffectScn {get; set;} = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn");
    public Texture IconTex {get; set;} = GD.Load<Texture>("res://Interface/Cursors/Art/Attack.PNG");
    public string ToolTip {get; set;} = "note to self. write a tooltip for this SpellEffect object";

    public SpellEffect()
    {
    }

}

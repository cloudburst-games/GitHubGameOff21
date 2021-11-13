using Godot;
using System;

public class SpellEffect : Reference
{
    public enum TargetMode { Hostile, Ally, Area}
    public int RangeSquares = 1;
    public int DurationRounds = 0;
    public float Magnitude = 5f;
    public int AreaSquares = 1;
    public float ManaCost = 5f;
    public BattleUnitData.DerivedStat TargetStat = BattleUnitData.DerivedStat.Health;
    public TargetMode Target = TargetMode.Hostile;

    public SpellEffect()
    {
    }

}

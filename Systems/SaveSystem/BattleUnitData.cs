using Godot;
using System;
using System.Collections.Generic;

[Serializable()]
public class BattleUnitData : IStoreable
{
    public BattleUnit.Combatant Combatant = BattleUnit.Combatant.Noob;
    public int Level {get; set;} = 1;

    public enum DerivedStat { Health, TotalHealth, Mana, TotalMana, HealthRegen, ManaRegen, MagicResist,
        PhysicalResist, Dodge, PhysicalDamage, SpellDamage, Speed, Initiative, Leadership, CriticalChance, CurrentAP}
    
    public Dictionary<DerivedStat, float> Stats {get; set;} = new Dictionary<DerivedStat, float>()
    {
        {DerivedStat.Health, 10},
        {DerivedStat.TotalHealth, 10},
        {DerivedStat.Mana, 10},
        {DerivedStat.TotalMana, 10},
        {DerivedStat.HealthRegen, 1},
        {DerivedStat.ManaRegen, 1},
        {DerivedStat.MagicResist, 10},
        {DerivedStat.PhysicalResist, 10},
        {DerivedStat.Dodge, 5},
        {DerivedStat.PhysicalDamage, 5},
        {DerivedStat.SpellDamage, 10},
        {DerivedStat.Speed, 6},
        {DerivedStat.Initiative, 5},
        {DerivedStat.Leadership, 1},
        {DerivedStat.CriticalChance, 1},
        {DerivedStat.CurrentAP, 6},
    };

}

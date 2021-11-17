using Godot;
using System;
using System.Collections.Generic;

[Serializable()]
public class BattleUnitData : IStoreable
{
    public BattleUnit.Combatant Combatant = BattleUnit.Combatant.Beetle;
    public string Name {get; set;} = "";
    public int Level {get; set;} = 1;
    public float Experience {get; set;} = 0;
    public bool PlayerFaction {get; set;} = false;

    public enum DerivedStat { Health, TotalHealth, Mana, TotalMana, HealthRegen, ManaRegen, MagicResist,
        PhysicalResist, Dodge, PhysicalDamage, PhysicalDamageRange, SpellDamage, Speed, Initiative, Leadership, CriticalChance, CurrentAP}

    public Dictionary<DerivedStat, float> Stats {get; set;} = new Dictionary<DerivedStat, float>()
    {
        {DerivedStat.Health, 10},
        {DerivedStat.TotalHealth, 10},
        {DerivedStat.Mana, 10},
        {DerivedStat.TotalMana, 10},
        {DerivedStat.HealthRegen, 1},
        {DerivedStat.ManaRegen, 1},
        {DerivedStat.MagicResist, 10},
        {DerivedStat.PhysicalResist, 0},
        {DerivedStat.Dodge, 5},
        {DerivedStat.PhysicalDamage, 5},
        {DerivedStat.PhysicalDamageRange, 1},
        {DerivedStat.SpellDamage, 5},
        {DerivedStat.Speed, 6},
        {DerivedStat.Initiative, 5},
        {DerivedStat.Leadership, 1},
        {DerivedStat.CriticalChance, 1},
        {DerivedStat.CurrentAP, 6},
    };

    public List<PotionEffect.PotionMode> Potions = new List<PotionEffect.PotionMode>() {
        PotionEffect.PotionMode.Health, PotionEffect.PotionMode.Mana, PotionEffect.PotionMode.Resilience,
        PotionEffect.PotionMode.Charisma, PotionEffect.PotionMode.Vigour, PotionEffect.PotionMode.Swiftness
    };

    public AITurnHandler.AITurnStateMode CurrentAITurnStateMode = AITurnHandler.AITurnStateMode.Aggressive;

    // durationLeft, magnitude
    public Dictionary<SpellEffectManager.SpellMode, Tuple<int, float>> CurrentStatusEffects = new Dictionary<SpellEffectManager.SpellMode, Tuple<int, float>>();

    public SpellEffectManager.SpellMode Spell1 = SpellEffectManager.SpellMode.Empty;
    public SpellEffectManager.SpellMode Spell2 = SpellEffectManager.SpellMode.Empty;
    public SpellEffectManager.SpellMode SpellGainedAtHigherLevel = SpellEffectManager.SpellMode.Empty;

}

using Godot;
using System;

public class BattleInteractionHandler : Reference
{
    private Random _rand = new Random();
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    //     public Dictionary<DerivedStat, float> Stats {get; set;} = new Dictionary<DerivedStat, float>()
    // {
    //     {DerivedStat.Health, 10},
    //     {DerivedStat.TotalHealth, 10},
    //     {DerivedStat.Mana, 10},
    //     {DerivedStat.TotalMana, 10},
    //     {DerivedStat.HealthRegen, 1},
    //     {DerivedStat.ManaRegen, 1},
    //     {DerivedStat.MagicResist, 10},
    //     {DerivedStat.PhysicalResist, 10},
    //     {DerivedStat.Dodge, 5},
    //     {DerivedStat.PhysicalDamage, 5},
    //     {DerivedStat.SpellDamage, 10},
    //     {DerivedStat.Speed, 6},
    //     {DerivedStat.Initiative, 5},
    //     {DerivedStat.Leadership, 1},
    //     {DerivedStat.CriticalChance, 1},
    //     {DerivedStat.CurrentAP, 6},
    // };
    public BattleInteractionHandler()
    {

    }

    public float[] CalculateMelee(BattleUnitData aggressor, BattleUnitData defender)
    {
        // randomise
        _rng.Randomize();
        // get physical damage
        float damage = aggressor.Stats[BattleUnitData.DerivedStat.PhysicalDamage] + 
            _rng.RandfRange(-aggressor.Stats[BattleUnitData.DerivedStat.PhysicalDamageRange], 
            aggressor.Stats[BattleUnitData.DerivedStat.PhysicalDamageRange]);
        damage = Math.Max(damage, 0); // dont do negative damage
        // apply critical chance for double damage
        int critical = _rng.RandiRange(0,100) < aggressor.Stats[BattleUnitData.DerivedStat.CriticalChance] ? 2 : 1;
        damage *= critical;
        // apply dodge reduction
        int dodge = _rng.RandiRange(1,100) < defender.Stats[BattleUnitData.DerivedStat.Dodge] ? 2 : 1;
        damage /= dodge;
        // apply flat reduction by physical resist
        float reductionMultiplier = 1 - defender.Stats[BattleUnitData.DerivedStat.PhysicalResist] / 100;
        // damage = (float)Math.Ceiling((1 - defender.Stats[BattleUnitData.DerivedStat.PhysicalResist] / 100f));
        damage *= reductionMultiplier;
        // reduce health by final damage
        defender.Stats[BattleUnitData.DerivedStat.Health] -= damage;

        return new float[3] {critical, dodge, damage};
    }


    public float[] CalculateSpell(SpellEffect spellEffect, BattleUnitData aggressor, BattleUnitData defender)
    {
        // randomise
        _rng.Randomize();
        // get spell damage
        float damage = spellEffect.Magnitude + aggressor.Stats[BattleUnitData.DerivedStat.SpellDamage];
        // apply critical chance for double damage
        int critical = _rng.RandiRange(0,100) < aggressor.Stats[BattleUnitData.DerivedStat.CriticalChance] ? 2 : 1;
        damage *= critical;
        // apply dodge reduction
        int dodge = _rng.RandiRange(1,100) < defender.Stats[BattleUnitData.DerivedStat.Dodge] ? 2 : 1;
        damage /= dodge;
        // apply flat reduction by magic resist
        float reductionMultiplier = 1 - defender.Stats[BattleUnitData.DerivedStat.MagicResist] / 100;
        damage *= reductionMultiplier;
        // reduce health by final damage
        defender.Stats[BattleUnitData.DerivedStat.Health] -= damage;
        // // reduce mana by mana cost
        // aggressor.Stats[BattleUnitData.DerivedStat.Mana] -= spellEffect.ManaCost;
        // GD.Print(damage);
        return new float[3] {critical, dodge, damage};
    }
}

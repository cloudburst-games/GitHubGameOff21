using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable()]
public class BattleUnitData : IStoreable
{
    public BattleUnit.Combatant Combatant = BattleUnit.Combatant.Beetle;
    public string BattlePortraitPath {get; set;} = "";
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


    public void ModulateStats(int increment)
    {
        foreach (BattleUnitData.DerivedStat stat in Stats.Keys.ToList())
        {
            if (stat == DerivedStat.Health || stat == DerivedStat.TotalHealth || stat == DerivedStat.Speed || stat == DerivedStat.CurrentAP 
                || stat == DerivedStat.PhysicalDamage|| stat == DerivedStat.PhysicalDamageRange)
                {
                    continue;
                }
            Stats[stat] = Math.Max(0, Stats[stat] + increment);
        }
    }

    public List<PnlInventory.ItemMode> ItemsHeld {get; set;} = new List<PnlInventory.ItemMode>() // all inventory items
    {
        PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.ManaPot
    };

    public PnlInventory.ItemMode[] PotionsEquipped {get; set;} = new PnlInventory.ItemMode[3] { // in potion slots
        PnlInventory.ItemMode.Empty, PnlInventory.ItemMode.Empty, PnlInventory.ItemMode.Empty
    };

    public PnlInventory.ItemMode AmuletEquipped {get; set;} = PnlInventory.ItemMode.Empty; // in amulet slot
    public PnlInventory.ItemMode WeaponEquipped {get; set;} = PnlInventory.ItemMode.Empty; // in weapon slot
    public PnlInventory.ItemMode ArmourEquipped {get; set;} = PnlInventory.ItemMode.Empty; // in armour slot

    public void DeleteEquippedPotion(PnlInventory.ItemMode potion)
    {
        foreach (PnlInventory.ItemMode pot in PotionsEquipped)
        {
            if (pot == potion)
            {
                int index = PotionsEquipped.ToList().IndexOf(pot);
                PotionsEquipped[index] = PnlInventory.ItemMode.Empty;
            }
        }
    }

    public List<PnlInventory.ItemMode> GetPotionsEquipped() //can be better
    {
        List<PnlInventory.ItemMode> potionsEquipped = new List<PnlInventory.ItemMode>();
        foreach (PnlInventory.ItemMode item in PotionsEquipped)
        {
            if ( item != PnlInventory.ItemMode.Empty)
            {
                potionsEquipped.Add(item);
            }
        }
        return potionsEquipped;
    }

    public AITurnHandler.AITurnStateMode CurrentAITurnStateMode = AITurnHandler.AITurnStateMode.Aggressive;

    // durationLeft, magnitude
    public Dictionary<SpellEffectManager.SpellMode, Tuple<int, float>> CurrentStatusEffects = new Dictionary<SpellEffectManager.SpellMode, Tuple<int, float>>();

    public SpellEffectManager.SpellMode Spell1 = SpellEffectManager.SpellMode.Empty;
    public SpellEffectManager.SpellMode Spell2 = SpellEffectManager.SpellMode.Empty;
    public SpellEffectManager.SpellMode SpellGainedAtHigherLevel = SpellEffectManager.SpellMode.Empty;

}

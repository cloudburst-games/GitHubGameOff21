using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class PnlCharacterManagementStats : Panel
{
    private Dictionary<SpellEffectManager.SpellMode, string> _spellNames = new Dictionary<SpellEffectManager.SpellMode, string>();

    public override void _Ready()
    {
        base._Ready();
        SpellEffectManager spellEffectManager = new SpellEffectManager();
        foreach (SpellEffectManager.SpellMode spell in spellEffectManager.SpellEffects.Keys)
        {
            if (spell == SpellEffectManager.SpellMode.Empty)
            {
                continue;
            }
            _spellNames.Add(spell, spellEffectManager.SpellEffects[spell][0].Name);
        }

    }
    public void UpdateStatDisplay(Dictionary<BattleUnitData.DerivedStat, float> stats)
    {
        Dictionary<BattleUnitData.DerivedStat, float> readableStats = stats.ToDictionary(x => x.Key, x => x.Value);
        foreach(BattleUnitData.DerivedStat key in readableStats.Keys.ToList())
        {
            readableStats[key] = Math.Max(0, readableStats[key]);
        }

        GetNode<Label>("VBoxContainer/PnlStats/HBoxStats/LblStats1").Text = 
            String.Format("Health: {0}/{1}\nMana: {2}/{3}\nHealth Regen: {4}\nMana Regen: {5}\nMagic Resist: {6}%\nPhysical Resist: {7}%\nDodge: {8}%",
                Math.Round(readableStats[BattleUnitData.DerivedStat.Health],1), Math.Round(readableStats[BattleUnitData.DerivedStat.TotalHealth],1),
                Math.Round(readableStats[BattleUnitData.DerivedStat.Mana],1), Math.Round(readableStats[BattleUnitData.DerivedStat.TotalMana],1),
                Math.Round(readableStats[BattleUnitData.DerivedStat.HealthRegen],1), Math.Round(readableStats[BattleUnitData.DerivedStat.ManaRegen],1),
                Math.Round(readableStats[BattleUnitData.DerivedStat.MagicResist],1), Math.Round(readableStats[BattleUnitData.DerivedStat.PhysicalResist],1),
                Math.Round(readableStats[BattleUnitData.DerivedStat.Dodge],1)
                );
        GetNode<Label>("VBoxContainer/PnlStats/HBoxStats/LblStats2").Text = 
            String.Format("Action Points: {0}/{1}\nPhysical Damage: {2}-{3}\nSpell Damage: {4}\nCritical Chance: {5}\nMove Speed: {6}\nLeadership: {7}\nInitiative: {8}",
                Math.Round(readableStats[BattleUnitData.DerivedStat.CurrentAP],1), Math.Round(readableStats[BattleUnitData.DerivedStat.Speed],1),
                Math.Round(Math.Max(readableStats[BattleUnitData.DerivedStat.PhysicalDamage] - readableStats[BattleUnitData.DerivedStat.PhysicalDamageRange], 0),1),
                Math.Round(readableStats[BattleUnitData.DerivedStat.PhysicalDamage] + readableStats[BattleUnitData.DerivedStat.PhysicalDamageRange],1),
                Math.Round(readableStats[BattleUnitData.DerivedStat.SpellDamage],1), Math.Round(readableStats[BattleUnitData.DerivedStat.CriticalChance],1),
                Math.Round(readableStats[BattleUnitData.DerivedStat.Speed],1), Math.Round(readableStats[BattleUnitData.DerivedStat.Leadership],1),
                Math.Round(readableStats[BattleUnitData.DerivedStat.Initiative],1)
                );
    }
    public void Start(Dictionary<BattleUnitData.DerivedStat, float> stats, SpellEffectManager.SpellMode[] spells)
    {
        UpdateStatDisplay(stats);
        GetNode<Label>("VBoxContainer/PnlSpells/VBox/LblTitle").Text = spells.ToList()
            .FindAll(x => x != SpellEffectManager.SpellMode.Empty)
            .Count != 0 ? "Known Spells" : "No Known Spells";
        GetNode<Label>("VBoxContainer/PnlSpells/VBox/LblSpellsLearned").Text = "";
        foreach (SpellEffectManager.SpellMode spell in spells)
        {
            if (spell == SpellEffectManager.SpellMode.Empty)
            {
                continue;
            }
            GetNode<Label>("VBoxContainer/PnlSpells/VBox/LblSpellsLearned").Text += _spellNames[spell] + "\n";
        }
        
    }
}

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class BattleUnitInfoPanel : Panel
{
    private Label _stats1;
    private Label _stats2;
    private Label _name;
    private Label _effects;
    private Label _lblSpellsLearned;
    public override void _Ready()
    {
        _stats1 = GetNode<Label>("VBoxLabels/HBoxStats/LblStats");
        _stats2 = GetNode<Label>("VBoxLabels/HBoxStats/LblStats2");
        _name = GetNode<Label>("VBoxLabels/LblName");
        _effects = GetNode<Label>("VBoxLabels/LblEffects");
        _lblSpellsLearned = GetNode<Label>("VBoxLabels/LblSpellsLearned");
        Visible = false;
    }

    public void Update(string name, Dictionary<BattleUnitData.DerivedStat, float> derivedStats, Dictionary<SpellEffectManager.SpellMode, Tuple<int, float>> currentEffects, Dictionary<SpellEffectManager.SpellMode, string> effectNames, List<string> spellsLearned)
    {
        _name.Text = name;
        Dictionary<BattleUnitData.DerivedStat, float> readableStats = derivedStats.ToDictionary(x => x.Key, x => x.Value);
        foreach(BattleUnitData.DerivedStat key in readableStats.Keys.ToList())
        {
            readableStats[key] = Math.Max(0, readableStats[key]);
        }
        _stats1.Text = String.Format("Health: {0}/{1}\nMana: {2}/{3}\nHealth Regen: {4}\nMana Regen: {5}\nPhysical Resist: {6}\nMagic Resist: {7}\nDodge: {8}",
            Math.Round(readableStats[BattleUnitData.DerivedStat.Health],1), Math.Round(readableStats[BattleUnitData.DerivedStat.TotalHealth],1),
            Math.Round(readableStats[BattleUnitData.DerivedStat.Mana],1), Math.Round(readableStats[BattleUnitData.DerivedStat.TotalMana],1),
            Math.Round(readableStats[BattleUnitData.DerivedStat.HealthRegen],1), Math.Round(readableStats[BattleUnitData.DerivedStat.ManaRegen],1),
            Math.Round(readableStats[BattleUnitData.DerivedStat.PhysicalResist],1), Math.Round(readableStats[BattleUnitData.DerivedStat.MagicResist],1),
            Math.Round(readableStats[BattleUnitData.DerivedStat.Dodge],1)
            );        
        _stats2.Text = String.Format("Action Points: {0}/{1}\nPhysical Damage: {2}-{3}\nSpell Damage: {4}\nCritical Chance: {5}\nMove Speed: {6}\nLeadership: {7}\nInitiative: {8}",
            Math.Round(readableStats[BattleUnitData.DerivedStat.CurrentAP],1), Math.Round(readableStats[BattleUnitData.DerivedStat.Speed],1),
            Math.Round(Math.Max(readableStats[BattleUnitData.DerivedStat.PhysicalDamage] - readableStats[BattleUnitData.DerivedStat.PhysicalDamageRange], 0),1),
            Math.Round(readableStats[BattleUnitData.DerivedStat.PhysicalDamage] + readableStats[BattleUnitData.DerivedStat.PhysicalDamageRange],1),
            Math.Round(readableStats[BattleUnitData.DerivedStat.SpellDamage],1), Math.Round(readableStats[BattleUnitData.DerivedStat.CriticalChance],1),
            Math.Round(readableStats[BattleUnitData.DerivedStat.Speed],1), Math.Round(readableStats[BattleUnitData.DerivedStat.Leadership],1),
            Math.Round(readableStats[BattleUnitData.DerivedStat.Initiative],1)
            );
        if (currentEffects.Keys.Count == 0)
        {
            _effects.Text = "No active effects.";
        }
        else
        {
            string currentEffectsStr = "Effects: ";
            foreach (SpellEffectManager.SpellMode spell in currentEffects.Keys)
            {
                currentEffectsStr += effectNames[spell] + ", ";
            }
            // currentEffectsStr.TrimEnd(new char[] {',', ' '});
            // currentEffectsStr.Remove(currentEffectsStr.Length-4, 2);
            _effects.Text = currentEffectsStr.Substring(0, currentEffectsStr.Length-2);
        }
        if (spellsLearned.Count == 0)
        {
            _lblSpellsLearned.Text = "No known spells.";
        }
        else
        {
            string spellsLearnedStr = "Spells known: ";
            foreach (string spell in spellsLearned)
            {
                spellsLearnedStr += spell + ", ";
            }
            // currentEffectsStr.TrimEnd(new char[] {',', ' '});
            // currentEffectsStr.Remove(currentEffectsStr.Length-4, 2);
            _lblSpellsLearned.Text = spellsLearnedStr.Substring(0, spellsLearnedStr.Length-2);
        }
    }

    public void Activate()
    {
        RectGlobalPosition = new Vector2(Mathf.Clamp(GetGlobalMousePosition().x, 0, GetViewportRect().Size.x - RectSize.x), 
            Mathf.Clamp(GetGlobalMousePosition().y, 0, GetViewportRect().Size.y - RectSize.y));
        Visible = true;
    }

}
// Health: 10/10
// Mana: 10/10
// Health Regen: 1
// Mana Regen: 1
// Magic Resist: 10%
// Physical Resist: 10%
// Dodge: 5%

// Action Points: 6/6
// Physical Damage: 5
// Spell Damage: 10
// Critical Chance: 1
// Move Speed: 6
// Leadership: 1
// Initiative: 1
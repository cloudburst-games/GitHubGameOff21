using Godot;
using System;
using System.Collections.Generic;

public class BattleUnitInfoPanel : Panel
{
    private Label _stats1;
    private Label _stats2;
    private Label _name;
    public override void _Ready()
    {
        _stats1 = GetNode<Label>("VBoxLabels/HBoxStats/LblStats");
        _stats2 = GetNode<Label>("VBoxLabels/HBoxStats/LblStats2");
        _name = GetNode<Label>("VBoxLabels/LblName");
        Visible = false;
    }

    public void Update(string name, Dictionary<BattleUnitData.DerivedStat, float> derivedStats)
    {
        _name.Text = name;
        _stats1.Text = String.Format("Health: {0}/{1}\nMana: {2}/{3}\nHealth Regen: {4}\nMana Regen: {5}\nPhysical Resist: {6}\nMagic Resist: {7}\nDodge: {8}",
            Math.Round(derivedStats[BattleUnitData.DerivedStat.Health],1), Math.Round(derivedStats[BattleUnitData.DerivedStat.TotalHealth],1),
            Math.Round(derivedStats[BattleUnitData.DerivedStat.Mana],1), Math.Round(derivedStats[BattleUnitData.DerivedStat.TotalMana],1),
            Math.Round(derivedStats[BattleUnitData.DerivedStat.HealthRegen],1), Math.Round(derivedStats[BattleUnitData.DerivedStat.ManaRegen],1),
            Math.Round(derivedStats[BattleUnitData.DerivedStat.PhysicalResist],1), Math.Round(derivedStats[BattleUnitData.DerivedStat.MagicResist],1),
            Math.Round(derivedStats[BattleUnitData.DerivedStat.Dodge],1)
            );        
        _stats2.Text = String.Format("Action Points: {0}/{1}\nPhysical Damage: {2}-{3}\nSpell Damage: {4}\nCritical Chance: {5}\nMove Speed: {6}\nLeadership: {7}\nInitiative: {8}",
            Math.Round(derivedStats[BattleUnitData.DerivedStat.CurrentAP],1), Math.Round(derivedStats[BattleUnitData.DerivedStat.Speed],1),
            Math.Round(derivedStats[BattleUnitData.DerivedStat.PhysicalDamage] - derivedStats[BattleUnitData.DerivedStat.PhysicalDamageRange],1),
            Math.Round(derivedStats[BattleUnitData.DerivedStat.PhysicalDamage] + derivedStats[BattleUnitData.DerivedStat.PhysicalDamageRange],1),
            Math.Round(derivedStats[BattleUnitData.DerivedStat.SpellDamage],1), Math.Round(derivedStats[BattleUnitData.DerivedStat.CriticalChance],1),
            Math.Round(derivedStats[BattleUnitData.DerivedStat.Speed],1), Math.Round(derivedStats[BattleUnitData.DerivedStat.Leadership],1),
            Math.Round(derivedStats[BattleUnitData.DerivedStat.Initiative],1)
            );
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
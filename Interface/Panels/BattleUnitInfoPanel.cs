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
    private ItemBuilder _itemBuilder = new ItemBuilder();
    public override void _Ready()
    {
        _stats1 = GetNode<Label>("VBoxLabels/HBoxStats/LblStats");
        _stats2 = GetNode<Label>("VBoxLabels/HBoxStats/LblStats2");
        _name = GetNode<Label>("VBoxLabels/Panel/LblName");
        _effects = GetNode<Label>("VBoxLabels/LblEffects");
        _lblSpellsLearned = GetNode<Label>("VBoxLabels/LblSpellsLearned");
        Visible = false;

        // TESTING. please keep this commented when doing release version
        // if (GetParent() == GetTree().Root && ProjectSettings.GetSetting("application/run/main_scene") != Filename)
        // {
        //     Test();
        // }
    }

    public void Test()
    {
        BattleUnitData playerData = new BattleUnitData() {
            Name = "Khepri sun",
            Combatant = BattleUnit.Combatant.Beetle,
            Level = 3,
            Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                {BattleUnitData.DerivedStat.Health, 10},
                {BattleUnitData.DerivedStat.TotalHealth, 10},
                {BattleUnitData.DerivedStat.Mana, 10},
                {BattleUnitData.DerivedStat.TotalMana, 10},
                {BattleUnitData.DerivedStat.HealthRegen, 1},
                {BattleUnitData.DerivedStat.ManaRegen, 1},
                {BattleUnitData.DerivedStat.MagicResist, 10},
                {BattleUnitData.DerivedStat.PhysicalResist, 10},
                {BattleUnitData.DerivedStat.Dodge, 5},
                {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 3},
                {BattleUnitData.DerivedStat.SpellDamage, 5},
                {BattleUnitData.DerivedStat.Speed, 6},
                {BattleUnitData.DerivedStat.Initiative, 5},
                {BattleUnitData.DerivedStat.Leadership, 1},
                {BattleUnitData.DerivedStat.CriticalChance, 1},
                {BattleUnitData.DerivedStat.CurrentAP, 6}
            },
            Spell1 = SpellEffectManager.SpellMode.SolarBolt,
            Spell2 = SpellEffectManager.SpellMode.HymnOfTheUnderworld,
            BattlePortraitPath = "res://Effects/SpellEffects/Art/WhiteSphericalParticle.png"
        };
        SpellEffectManager fxManager = new SpellEffectManager();
        Dictionary<SpellEffectManager.SpellMode, string> spellNames = new Dictionary<SpellEffectManager.SpellMode, string>();
        foreach (SpellEffectManager.SpellMode spell in fxManager.SpellEffects.Keys)
        {
            if (spell == SpellEffectManager.SpellMode.Empty)
            {
                continue;
            }
            spellNames.Add(spell, fxManager.SpellEffects[spell][0].Name);
        }

        List<string> spellNameList = new List<string>();
        
        if (playerData.Spell1 != SpellEffectManager.SpellMode.Empty)
        {
            spellNameList.Add(spellNames[playerData.Spell1]);
        }
        if (playerData.Spell2 != SpellEffectManager.SpellMode.Empty)
        {
            spellNameList.Add(spellNames[playerData.Spell2]);
        }



        Update(playerData.Name, playerData.Stats, new Dictionary<SpellEffectManager.SpellMode, Tuple<int, float>>() {{SpellEffectManager.SpellMode.LeadershipBonus, new Tuple<int, float>(3,2)}},
            spellNames, spellNameList, playerData.WeaponEquipped, playerData.AmuletEquipped, playerData.ArmourEquipped, playerData.PotionsEquipped, playerData.BattlePortraitPath);
        
        Activate();
    }

    public void Update(string name, Dictionary<BattleUnitData.DerivedStat, float> derivedStats, Dictionary<SpellEffectManager.SpellMode, Tuple<int, float>> currentEffects, Dictionary<SpellEffectManager.SpellMode, string> effectNames, List<string> spellsLearned,
        PnlInventory.ItemMode weaponEquipped, PnlInventory.ItemMode amuletEquipped, PnlInventory.ItemMode armourEquipped, PnlInventory.ItemMode[] potionsEquipped, string portraitPath)
    {
        _name.Text = name;
        Dictionary<BattleUnitData.DerivedStat, float> readableStats = derivedStats.ToDictionary(x => x.Key, x => x.Value);
        foreach(BattleUnitData.DerivedStat key in readableStats.Keys.ToList())
        {
            readableStats[key] = Math.Max(0, readableStats[key]);
        }
        _stats1.Text = String.Format("Health: {0}/{1}\nMana: {2}/{3}\nHealth Regen: {4}\nMana Regen: {5}\nMagic Resist: {6}%\nPhysical Resist: {7}%\nDodge: {8}%",
            Math.Round(readableStats[BattleUnitData.DerivedStat.Health],1), Math.Round(readableStats[BattleUnitData.DerivedStat.TotalHealth],1),
            Math.Round(readableStats[BattleUnitData.DerivedStat.Mana],1), Math.Round(readableStats[BattleUnitData.DerivedStat.TotalMana],1),
            Math.Round(readableStats[BattleUnitData.DerivedStat.HealthRegen],1), Math.Round(readableStats[BattleUnitData.DerivedStat.ManaRegen],1),
            Math.Round(readableStats[BattleUnitData.DerivedStat.MagicResist],1), Math.Round(readableStats[BattleUnitData.DerivedStat.PhysicalResist],1),
            Math.Round(readableStats[BattleUnitData.DerivedStat.Dodge],1)
            );       
        _stats2.Text = String.Format("Action Points: {0}/{1}\nPhysical Damage: {2}-{3}\nSpell Power: {4}\nCritical Chance: {5}%\nMove Speed: {6}\nLeadership: {7}\nInitiative: {8}",
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

        List<string> equipmentNames = new List<string>();
        equipmentNames.Add(_itemBuilder.BuildAnyItem(weaponEquipped).Name);
        equipmentNames.Add(_itemBuilder.BuildAnyItem(armourEquipped).Name);
        equipmentNames.Add(_itemBuilder.BuildAnyItem(amuletEquipped).Name);
        foreach (PnlInventory.ItemMode item in potionsEquipped)
        {
            equipmentNames.Add(_itemBuilder.BuildAnyItem(item).Name);
        }
        foreach (string s in equipmentNames.ToList())
        {
            if (s == "Empty")
            {
                equipmentNames.Remove(s);
            }
        }
        string equippedString = "Equipped: ";
        foreach (string eName in equipmentNames)
        {
            equippedString += eName + ", ";
        }
        GetNode<Label>("VBoxLabels/LblEquipment").Text = equipmentNames.Count != 0 ? equippedString.Substring(0, equippedString.Length-2)
            : "Nothing equipped.";

        if (portraitPath == "" || portraitPath == null)
        {
            GetNode<TextureRect>("VBoxLabels/Panel/TexRectPortrait").Visible = true;
            GetNode<TextureRect>("VBoxLabels/Panel/TexRectPortrait").Texture = GD.Load<Texture>("res://Actors/PortraitPlaceholders/Small/NPC.PNG");
        }
        else
        {
            GD.Print(portraitPath);
            GetNode<TextureRect>("VBoxLabels/Panel/TexRectPortrait").Visible = true;
            GetNode<TextureRect>("VBoxLabels/Panel/TexRectPortrait").Texture = GD.Load<Texture>(portraitPath);
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
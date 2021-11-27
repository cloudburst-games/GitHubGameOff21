using Godot;
using System;

public class IntellectPotionEffect : PotionEffect
{

    public IntellectPotionEffect()
    {
        CurrentItemMode = PnlInventory.ItemMode.IntellectPot;
        StatsAffected.Add(BattleUnitData.DerivedStat.Health);
        Magnitude = 8f;
        IconTexture = GD.Load<Texture>("res://Interface/Icons/PotionBlue.png");
        Name = "Intellect Potion";
        SpellEffect = SpellEffectManager.SpellMode.IntellectPotion;
        Tooltip = "Boosts intellect, improving mana capacity, mana regen, and spell power!";
        Cost = 5;
    }
}

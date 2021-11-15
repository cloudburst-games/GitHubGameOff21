using Godot;
using System;

public class IntellectPotionEffect : PotionEffect
{

    public IntellectPotionEffect()
    {
        CurrentPotionMode = PotionEffect.PotionMode.Intellect;
        StatsAffected.Add(BattleUnitData.DerivedStat.Health);
        Magnitude = 4f;
        IconTexture = GD.Load<Texture>("res://Interface/Icons/PotionBlue.png");
        Name = "Intellect Potion";
        SpellEffect = SpellEffectManager.SpellMode.IntellectPotion;
        Tooltip = "Boosts intellect, improving mana capacity, mana regen, and spell damage!";
    }
}

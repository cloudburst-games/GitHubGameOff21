using Godot;
using System;

public class CharismaPotionEffect : PotionEffect
{

    public CharismaPotionEffect()
    {
        CurrentPotionMode = PotionEffect.PotionMode.Charisma;
        StatsAffected.Add(BattleUnitData.DerivedStat.Health);
        Magnitude = 5f;
        IconTexture = GD.Load<Texture>("res://Interface/Icons/PotionPurp.png");
        Name = "Charisma Potion";
        SpellEffect = SpellEffectManager.SpellMode.CharismaPotion;
        Tooltip = "Boosts leadership, providing a bonus to nearby companions!";
    }
}

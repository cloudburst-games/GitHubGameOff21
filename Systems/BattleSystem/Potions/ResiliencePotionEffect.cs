using Godot;
using System;

public class ResiliencePotionEffect : PotionEffect
{

    public ResiliencePotionEffect()
    {
        CurrentPotionMode = PotionEffect.PotionMode.Resilience;
        StatsAffected.Add(BattleUnitData.DerivedStat.Health);
        Magnitude = 5f;
        IconTexture = GD.Load<Texture>("res://Interface/Icons/PotionGreen.png");
        Name = "Resilience Potion";
        SpellEffect = SpellEffectManager.SpellMode.ResiliencePotion;
        Tooltip = "Boosts resilience, providing a bonus to health and mana regeneration, and magic resist!";
    }
}

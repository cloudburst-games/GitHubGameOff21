using Godot;
using System;

public class HealthPotionEffect : PotionEffect
{

    public HealthPotionEffect()
    {
        CurrentPotionMode = PotionEffect.PotionMode.Health;
        StatsAffected.Add(BattleUnitData.DerivedStat.Health);
        Magnitude = 6f;
        IconTexture = GD.Load<Texture>("res://Interface/Icons/Food.png");
        Name = "Food";
        SpellEffect = SpellEffectManager.SpellMode.HealthPotion;
        Tooltip = "Restore " + Magnitude + " health.";
    }
}

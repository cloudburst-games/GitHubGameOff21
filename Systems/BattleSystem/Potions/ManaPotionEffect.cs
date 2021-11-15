using Godot;
using System;

public class ManaPotionEffect : PotionEffect
{

    public ManaPotionEffect()
    {
        CurrentPotionMode = PotionEffect.PotionMode.Mana;
        StatsAffected.Add(BattleUnitData.DerivedStat.Mana);
        Magnitude = 3f;
        IconTexture = GD.Load<Texture>("res://Interface/Icons/Water.png");
        Name = "Water";
        SpellEffect = SpellEffectManager.SpellMode.ManaPotion;
        Tooltip = "Restore " + Magnitude + " mana.";
    }
}

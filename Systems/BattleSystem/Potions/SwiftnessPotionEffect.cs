using Godot;
using System;

public class SwiftnessPotionEffect : PotionEffect
{

    public SwiftnessPotionEffect()
    {
        CurrentItemMode = PnlInventory.ItemMode.SwiftnessPot;
        StatsAffected.Add(BattleUnitData.DerivedStat.Health);
        Magnitude = 8f;
        IconTexture = GD.Load<Texture>("res://Interface/Icons/PotionAqua.png");
        Name = "Swiftness Potion";
        SpellEffect = SpellEffectManager.SpellMode.SwiftnessPotion;
        Tooltip = "Boosts swiftness, providing a bonus to dodge, speed, and initiative!";
        Cost = 5;
    }
}

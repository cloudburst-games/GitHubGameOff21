using Godot;
using System;

public class CharismaPotionEffect : PotionEffect
{

    public CharismaPotionEffect()
    {
        CurrentItemMode = PnlInventory.ItemMode.CharismaPot;
        StatsAffected.Add(BattleUnitData.DerivedStat.Health);
        Magnitude = 10f;
        IconTexture = GD.Load<Texture>("res://Interface/Icons/PotionPurp.png");
        Name = "Charisma Potion";
        SpellEffect = SpellEffectManager.SpellMode.CharismaPotion;
        Tooltip = "Boosts leadership, providing a bonus to nearby companions!";
        Cost = 5;
    }
}

using Godot;
using System;

public class LuckPotionEffect : PotionEffect
{

    public LuckPotionEffect()
    {
        CurrentItemMode = PnlInventory.ItemMode.LuckPot;
        StatsAffected.Add(BattleUnitData.DerivedStat.Health);
        Magnitude = 10f;
        IconTexture = GD.Load<Texture>("res://Interface/Icons/PotionYellow.png");
        Name = "Luck Potion";
        SpellEffect = SpellEffectManager.SpellMode.LuckPotion;
        Tooltip = "Boosts luck, providing a bonus to critical hit, dodge, and magic resist!";
    }
}

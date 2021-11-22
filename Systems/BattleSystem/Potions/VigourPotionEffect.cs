using Godot;
using System;

public class VigourPotionEffect : PotionEffect
{

    public VigourPotionEffect()
    {
        CurrentItemMode = PnlInventory.ItemMode.VigourPot;
        StatsAffected.Add(BattleUnitData.DerivedStat.Health);
        Magnitude = 8f;
        IconTexture = GD.Load<Texture>("res://Interface/Icons/PotionRed.png");
        Name = "Vigour Potion";
        SpellEffect = SpellEffectManager.SpellMode.VigourPotion;
        Tooltip = "Boosts vigour, providing a bonus to physical damage and health!";
        Cost = 5;
    }
}

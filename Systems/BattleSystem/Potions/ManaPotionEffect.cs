using Godot;
using System;

public class ManaPotionEffect : PotionEffect
{

    public ManaPotionEffect()
    {
        CurrentItemMode = PnlInventory.ItemMode.ManaPot;
        StatsAffected.Add(BattleUnitData.DerivedStat.Mana);
        Magnitude = 20f;
        IconTexture = GD.Load<Texture>("res://Interface/Icons/Water.png");
        Name = "Water";
        SpellEffect = SpellEffectManager.SpellMode.ManaPotion;
        Tooltip = "Restore " + Magnitude + " mana.";
        Cost = 2;
    }
}

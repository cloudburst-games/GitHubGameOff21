using Godot;
using System;
using System.Collections.Generic;

// [Serializable()]
public class PotionEffect : Reference, IInventoryPlaceable
{
    public enum PotionMode {Charisma, Health, Intellect, Luck, Mana, Resilience, Swiftness, Vigour}
    public PotionMode CurrentPotionMode = PotionMode.Health;
    public List<BattleUnitData.DerivedStat> StatsAffected = new List<BattleUnitData.DerivedStat>();
    public SpellEffectManager.SpellMode SpellEffect {get; set;} = SpellEffectManager.SpellMode.HealthPotion;
    public float Magnitude {get; set;} = 0;

    public string Name {get; set;} = "Generic Potion";
    public string Tooltip {get; set;} = "Potion does nothing";
    public Texture IconTexture {get; set;} = GD.Load<Texture>("res://Interface/Icons/PotionAqua.png");
    public TextureRect TexRect {get; set;} = null;
    // public PackedScene PotionEffectScn {get; set;} = GD.Load<PackedScene>("res://Effects/SpellEffects/ComingForthByDayEffect.tscn");
}

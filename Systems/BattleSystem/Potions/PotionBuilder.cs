using Godot;
using System;
using System.Collections.Generic;

// [Serializable()]
public class PotionBuilder
{
    public PotionEffect BuildPotion(PotionEffect.PotionMode potionMode)
    {
        switch (potionMode)
        {
            case PotionEffect.PotionMode.Charisma:
                return new CharismaPotionEffect();
            case PotionEffect.PotionMode.Health:
                return new HealthPotionEffect();
            case PotionEffect.PotionMode.Intellect:
                return new IntellectPotionEffect();
            case PotionEffect.PotionMode.Luck:
                return new LuckPotionEffect();
            case PotionEffect.PotionMode.Mana:
                return new ManaPotionEffect();
            case PotionEffect.PotionMode.Resilience:
                return new ResiliencePotionEffect();
            case PotionEffect.PotionMode.Swiftness:
                return new SwiftnessPotionEffect();
            case PotionEffect.PotionMode.Vigour:
                return new VigourPotionEffect();
            default:
                return new PotionEffect();
            
        }
    }
}

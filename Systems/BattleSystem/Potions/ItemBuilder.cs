using Godot;
using System;
using System.Collections.Generic;

// [Serializable()]
public class ItemBuilder
{
    public PotionEffect BuildPotion(PnlInventory.ItemMode potionMode)
    {
        switch (potionMode)
        {
            case PnlInventory.ItemMode.CharismaPot:
                return new CharismaPotionEffect();
            case PnlInventory.ItemMode.HealthPot:
                return new HealthPotionEffect();
            case PnlInventory.ItemMode.IntellectPot:
                return new IntellectPotionEffect();
            case PnlInventory.ItemMode.LuckPot:
                return new LuckPotionEffect();
            case PnlInventory.ItemMode.ManaPot:
                return new ManaPotionEffect();
            case PnlInventory.ItemMode.ResiliencePot:
                return new ResiliencePotionEffect();
            case PnlInventory.ItemMode.SwiftnessPot:
                return new SwiftnessPotionEffect();
            case PnlInventory.ItemMode.VigourPot:
                return new VigourPotionEffect();
            default:
                return new PotionEffect();
            
        }
    }

    public WeaponItem BuildWeapon(PnlInventory.ItemMode weaponMode)
    {
        switch (weaponMode)
        {
            case PnlInventory.ItemMode.RustedMace:
                return new RustedMaceWeaponItem();
            case PnlInventory.ItemMode.SilverMace:
                return new SilverMaceWeaponItem();
            default:
                return new WeaponItem();
        }
    }
    public ArmourItem BuildArmour(PnlInventory.ItemMode armourMode)
    {
        switch (armourMode)
        {
            case PnlInventory.ItemMode.RustedArmour:
                return new RustedArmourArmourItem();
            case PnlInventory.ItemMode.ObsidianPlate:
                return new ObsidianPlateArmourItem();
            default:
                return new ArmourItem();
        }
    }
    public AmuletItem BuildAmulet(PnlInventory.ItemMode amuletMode)
    {
        switch (amuletMode)
        {
            case PnlInventory.ItemMode.ScarabAmulet:
                return new ScarabAmuletAmuletItem();
            default:
                return new AmuletItem();
        }
    }
}

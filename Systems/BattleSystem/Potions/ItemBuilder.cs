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
            case PnlInventory.ItemMode.AshenMace:
                return new AshenMaceWeaponItem();
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
            case PnlInventory.ItemMode.PhantomArmour:
                return new PhantomArmourItem();
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

    public IInventoryPlaceable BuildAnyItem(PnlInventory.ItemMode itemMode)
    {
        if (IsPotion(itemMode))
        {
            return BuildPotion(itemMode);
        }
        else if (IsWeapon(itemMode))
        {
            return BuildWeapon(itemMode);
        }
        else if (IsArmour(itemMode))
        {
            return BuildArmour(itemMode);
        }
        else if (IsAmulet(itemMode))
        {
            return BuildAmulet(itemMode);
        }
        else
        {
            return new InventoryItemEmpty();
        }
    }

    public bool IsPotion(PnlInventory.ItemMode itemMode)
    {
        return itemMode == PnlInventory.ItemMode.CharismaPot || itemMode == PnlInventory.ItemMode.HealthPot || itemMode == PnlInventory.ItemMode.IntellectPot
             || itemMode == PnlInventory.ItemMode.LuckPot || itemMode == PnlInventory.ItemMode.ManaPot || itemMode == PnlInventory.ItemMode.ResiliencePot
             || itemMode == PnlInventory.ItemMode.SwiftnessPot || itemMode == PnlInventory.ItemMode.VigourPot;
    }
    public bool IsWeapon(PnlInventory.ItemMode itemMode)
    {
        return itemMode == PnlInventory.ItemMode.RustedMace || itemMode == PnlInventory.ItemMode.SilverMace || itemMode == PnlInventory.ItemMode.AshenMace;
    }
    public bool IsArmour(PnlInventory.ItemMode itemMode)
    {
        return itemMode == PnlInventory.ItemMode.RustedArmour || itemMode == PnlInventory.ItemMode.ObsidianPlate || itemMode == PnlInventory.ItemMode.PhantomArmour;
    }
    public bool IsAmulet(PnlInventory.ItemMode itemMode)
    {
        return itemMode == PnlInventory.ItemMode.ScarabAmulet;
    }
}

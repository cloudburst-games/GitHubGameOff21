using Godot;
using System;
using System.Collections.Generic;

public class SilverMaceWeaponItem : WeaponItem
{   
    public SilverMaceWeaponItem()
    {
        CurrentItemMode = PnlInventory.ItemMode.SilverMace;
        // AttributesAffected.Add(UnitData.Attribute.Vigour);
        // AttributesAffectedMagnitude = 3f;
        WeaponDamage = 3f;
        DamageRange = 2f;
        IconTexture = GD.Load<Texture>("res://Props/GHGO21Plceholders/EnchantedMace.PNG");
        Name = "Enchanted Mace";
        Tooltip = String.Format("The powerful enchantments surrounding this mace are in contrast to its mundane appearance. Deals {0}-{1} damage.", Convert.ToInt32(WeaponDamage-DamageRange), Convert.ToInt32(WeaponDamage+DamageRange));
        Cost = 50;
    }
}

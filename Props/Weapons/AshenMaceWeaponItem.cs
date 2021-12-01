using Godot;
using System;
using System.Collections.Generic;

public class AshenMaceWeaponItem : WeaponItem
{   
    public AshenMaceWeaponItem()
    {
        CurrentItemMode = PnlInventory.ItemMode.AshenMace;
        // AttributesAffected.Add(UnitData.Attribute.Vigour);
        // AttributesAffectedMagnitude = 3f;
        WeaponDamage = 8f;
        DamageRange = 3f;
        IconTexture = GD.Load<Texture>("res://Props/GHGO21Plceholders/EnchantedMace.PNG");
        Name = "Ashen Mace";
        Tooltip = String.Format("An ice-cold mace haunted by the souls of the lost. Deals {0}-{1} damage.", Convert.ToInt32(WeaponDamage-DamageRange), Convert.ToInt32(WeaponDamage+DamageRange));
        Cost = 120;
    }
}

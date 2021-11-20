using Godot;
using System;
using System.Collections.Generic;

public class RustedMaceWeaponItem : WeaponItem
{   
    public RustedMaceWeaponItem()
    {
        CurrentItemMode = PnlInventory.ItemMode.RustedMace;
        // AttributesAffected.Add(UnitData.Attribute.Vigour);
        // AttributesAffectedMagnitude = 3f;
        WeaponDamage = 2f;
        DamageRange = 1f;
        IconTexture = GD.Load<Texture>("res://Props/GHGO21Plceholders/RustedMace.PNG");
        Name = "Rusted Mace";
        Tooltip = String.Format("A rusted mace. Deals {0}-{1} damage.", Convert.ToInt32(WeaponDamage-DamageRange), Convert.ToInt32(WeaponDamage+DamageRange));
        Cost = 25;

    }
}

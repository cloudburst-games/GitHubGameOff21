using Godot;
using System;
using System.Collections.Generic;

public class PhantomArmourItem : ArmourItem
{   
    public PhantomArmourItem()
    {
        CurrentItemMode = PnlInventory.ItemMode.PhantomArmour;
        // AttributesAffected.Add(UnitData.Attribute.Vigour);
        // AttributesAffectedMagnitude = 3f;
        ArmourBonus = 20f;
        IconTexture = GD.Load<Texture>("res://Props/GHGO21Plceholders/PhantomArmour.PNG");
        Name = "Phantom Plate";
        Tooltip = String.Format("This magical piece from the Underworld is constantly shifting and changing.  Provides {0} armour bonus.", ArmourBonus);
        Cost = 60;
    }
}

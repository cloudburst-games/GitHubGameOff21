using Godot;
using System;
using System.Collections.Generic;

public class RustedArmourArmourItem : ArmourItem
{   
    public RustedArmourArmourItem()
    {
        CurrentItemMode = PnlInventory.ItemMode.RustedArmour;
        // AttributesAffected.Add(UnitData.Attribute.Vigour);
        // AttributesAffectedMagnitude = 3f;
        ArmourBonus = 5f;
        IconTexture = GD.Load<Texture>("res://Props/GHGO21Plceholders/AncientArmour.PNG");
        Name = "Ancient Armour";
        Tooltip = String.Format("This ancient piece of armour has not seen much use lately. Provides {0} armour bonus.", ArmourBonus);
        Cost = 15;
    }
}

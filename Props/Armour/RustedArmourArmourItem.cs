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
        ArmourBonus = 2f;
        IconTexture = GD.Load<Texture>("res://Interface/Cursors/Art/Spell.PNG");
        Name = "Rusted Armour";
        Tooltip = String.Format("A rusted slab of iron. Provides {0} armour bonus.", ArmourBonus);
        Cost = 15;
    }
}

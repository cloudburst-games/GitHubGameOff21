using Godot;
using System;
using System.Collections.Generic;

public class ObsidianPlateArmourItem : ArmourItem
{   
    public ObsidianPlateArmourItem()
    {
        CurrentItemMode = PnlInventory.ItemMode.ObsidianPlate;
        // AttributesAffected.Add(UnitData.Attribute.Vigour);
        // AttributesAffectedMagnitude = 3f;
        ArmourBonus = 4f;
        IconTexture = GD.Load<Texture>("res://Interface/Cursors/Art/Spell.PNG");
        Name = "Obsidian Plate";
        Tooltip = String.Format("This midnight black armour gleams with an unnatural glow. Provides {0} armour bonus.", ArmourBonus);
    }
}

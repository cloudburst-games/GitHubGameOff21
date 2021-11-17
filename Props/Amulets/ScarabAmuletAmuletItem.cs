using Godot;
using System;
using System.Collections.Generic;

public class ScarabAmuletAmuletItem : AmuletItem
{   
    public ScarabAmuletAmuletItem()
    {
        CurrentItemMode = PnlInventory.ItemMode.ScarabAmulet;
        AttributesAffected.Add(UnitData.Attribute.Resilience);
        AttributesAffectedMagnitude = 5;
        IconTexture = GD.Load<Texture>("res://Interface/Cursors/Art/Spell.PNG");
        Name = "Scarab Amulet";
        Tooltip = String.Format("A divine amulet. Provides {0} to resilience.", AttributesAffectedMagnitude);
    }
}

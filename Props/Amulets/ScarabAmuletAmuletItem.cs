using Godot;
using System;
using System.Collections.Generic;

public class ScarabAmuletAmuletItem : AmuletItem
{   
    public ScarabAmuletAmuletItem()
    {
        CurrentItemMode = PnlInventory.ItemMode.ScarabAmulet;
        AttributesAffected.Add(UnitData.Attribute.Resilience);
        AttributesAffectedMagnitude = 10;
        IconTexture = GD.Load<Texture>("res://Props/GHGO21Plceholders/ScarabAmulet.PNG");
        Name = "Scarab Amulet";
        Tooltip = String.Format("A divine amulet. Provides {0} to resilience.", AttributesAffectedMagnitude);
        Cost = 100;
    }
}

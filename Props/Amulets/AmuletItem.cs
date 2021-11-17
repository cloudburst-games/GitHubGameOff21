using Godot;
using System;
using System.Collections.Generic;

public class AmuletItem : IInventoryPlaceable
{
    public PnlInventory.ItemMode CurrentItemMode {get; set;} = PnlInventory.ItemMode.ScarabAmulet;
    public List<UnitData.Attribute> AttributesAffected = new List<UnitData.Attribute>();
    public int AttributesAffectedMagnitude {get; set;} = 0;

    public string Name {get; set;} = "Generic armour";
    public string Tooltip {get; set;} = "You shouldn't be able to see this";
    public Texture IconTexture {get; set;} = GD.Load<Texture>("res://Interface/Cursors/Art/Hint.PNG");
    public TextureRect TexRect {get; set;} = null;
}

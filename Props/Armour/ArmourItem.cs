using Godot;
using System;
using System.Collections.Generic;

public class ArmourItem : IInventoryPlaceable
{
    public PnlInventory.ItemMode CurrentItemMode  {get; set;} = PnlInventory.ItemMode.RustedArmour;
    public List<UnitData.Attribute> AttributesAffected = new List<UnitData.Attribute>();
    public float ArmourBonus {get; set;} = 2f;
    public int AttributesAffectedMagnitude {get; set;} = 0;

    public string Name {get; set;} = "Generic armour";
    public string Tooltip {get; set;} = "You shouldn't be able to see this";
    public Texture IconTexture {get; set;} = GD.Load<Texture>("res://Interface/Cursors/Art/Hint.PNG");
    public TextureRect TexRect {get; set;} = null;
    public int Cost {get; set;} = 15;

}

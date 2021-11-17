using Godot;
using System;
using System.Collections.Generic;

public class WeaponItem : IInventoryPlaceable
{
    public PnlInventory.ItemMode CurrentItemMode  {get; set;} = PnlInventory.ItemMode.RustedMace;
    public List<UnitData.Attribute> AttributesAffected = new List<UnitData.Attribute>();
    public float DamageRange {get; set;} = 2f;
    public float WeaponDamage {get; set;} = 1f;
    public int AttributesAffectedMagnitude {get; set;} = 0;

    public string Name {get; set;} = "Generic weapon";
    public string Tooltip {get; set;} = "You shouldn't be able to see this";
    public Texture IconTexture {get; set;} = GD.Load<Texture>("res://Interface/Cursors/Art/Attack.PNG");
    public TextureRect TexRect {get; set;} = null;
}

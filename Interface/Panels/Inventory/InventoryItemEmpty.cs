using Godot;
using System;

public class InventoryItemEmpty : Reference, IInventoryPlaceable
{
    public Texture IconTexture {get; set;} = null;
    public TextureRect TexRect {get; set;} = null;
    public string Name {get; set;} = "Empty";
    public string Tooltip {get; set;} = "Useless";
}

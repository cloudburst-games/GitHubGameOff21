using Godot;
using System;

public interface IInventoryPlaceable
{
    PnlInventory.ItemMode CurrentItemMode {
        get;
        set;
    }

    Texture IconTexture {
        get;
        set;
    }

    TextureRect TexRect {
        get;
        set;
    }

    string Name {
        get; set;
    }
    string Tooltip {
        get; set;
    }

    int Cost{
        get; set;
    }
}

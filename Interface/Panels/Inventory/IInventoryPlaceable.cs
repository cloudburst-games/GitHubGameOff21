using Godot;
using System;

public interface IInventoryPlaceable
{
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
}

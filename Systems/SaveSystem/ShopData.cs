using Godot;
using System;
using System.Collections.Generic;

[Serializable()]
public class ShopData : IStoreable
{
    public bool Modified {get; set;} = false;
    public Vector2 GlobalStartPosition {get; set;}
    public string ShopBackgroundTexturePath {get; set;}
    public string ShopTitle {get; set;}
    public List<PnlInventory.ItemMode> ItemsStocked {get; set;} = new List<PnlInventory.ItemMode>();
    public string IntroText {get; set;} = "Welcome, my lord! Please, peruse my humble wares.";
    public float Stinginess {get; set;} = 2;
}

using Godot;
using System;
using System.Collections.Generic;

[Serializable()]
public class WorldInteractableData : IStoreable
{
    public bool Active {get; set;} = true;
    public Vector2 WorldPosition {get; set;}
    public string BodyPath {get; set;}
    public float Experience {get; set;} = 0;
    public List<PnlInventory.ItemMode> Items {get; set;} = new List<PnlInventory.ItemMode>();
    public int Gold {get; set;} = 0;
    public int AttributePoints {get; set;} = 0;
    public bool DieOnActivate {get; set;} = false;
    public string FlavourText {get; set;} = "";
    public string EventText {get; set;} = "";
}

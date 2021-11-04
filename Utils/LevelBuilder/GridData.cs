using Godot;
using System;
using System.Collections.Generic;

[Serializable()]
public class GridData
{
    
	public List<List<Dictionary<string,object>>> MainGrid {get; set;} = new List<List<Dictionary<string, object>>>();
	public List<List<byte>> BorderGrid {get; set;} = new List<List<byte>>();


}

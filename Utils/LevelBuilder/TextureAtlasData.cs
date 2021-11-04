using Godot;
using System;
using System.Collections.Generic;

// Holds texture atlas data from JSON file 
public class TextureAtlasData : Reference
{

	public List<Frame> frames {get; set;}
	public class Frame
	{
		public string filename {get; set;}
		public Dictionary<string,int> frame {get;set;}
		public bool rotated;
		public bool trimmed;
		public Dictionary<string,int> spriteSourceSize {get;set;}
		public Dictionary<string,int> sourceSize {get;set;}
		public Dictionary<string,double> pivot {get;set;}
	}
}
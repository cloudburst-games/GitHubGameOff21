using Godot;
using System;

public class PictureStoryUnit
{

	public Texture Background {get; set;}
	public float[] RegionRect {get; set;}
	public string Text {get; set;}

	public AudioStream Audio {get; set;}
	public VideoStream Video {get; set;}
	public Label.VAlign TextVAlign {get; set;} = Label.VAlign.Bottom;
	public Label.AlignEnum TextHAlign {get; set;} = Label.AlignEnum.Center;
	public Color FontColorOverride {get; set;} = new Color(0,0,0);
	public Color FontBorderColorOverride {get; set;} = new Color(1,1,1);
	
}

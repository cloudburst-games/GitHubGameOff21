using Godot;
using System;

public class ExperienceManager
{
    public float GetExperienceNeeded(int level)
	{
		return (float) Math.Round((5* Math.Pow(level, 3)) / 4);
	}
}

// Basic stage script: acts as a foundation from which all stage scripts are inherited. 
// In this project, 'Stage' is used to refer to the main scenes that are shown as screens to the user, e.g. 'OverworldStage', 'BattleStage', 
// 'MainMenuStage'. Conversely, scenes that act as components within these stages may be referred to differnetly, mainly as 'Entities'.

// The purpose of this script is to contain member variables and methods common to all stages.
// For instance, all stages require access to the global SceneManager scene, to a SharedData dict that contains data shared between stages
// (useful for transitioning between stages - reducing reliance on potentially harmful singletons/globals), methods to hide and show children.

using Godot;
using System;
using System.Collections.Generic;

public class Stage : Node
{

	// Dictionary that contains data shared between stages.
	public Dictionary<string, object> SharedData {get; set;} = null;
	public SceneManager SceneManager {get; private set;}

	// NOTE in child classes we always have to run base._Ready() if we override _Ready
	public override void _Ready()
	{
		SceneManager = (SceneManager)GetNode("/root/SceneManager");
	}

	// Any startup code that should run when stage is shown (not added to tree as in _Ready)
	public virtual void Init()
	{

	}

	// Any exit code that needs to run when stage is freed
	public virtual void Exit()
	{
		
	}
}

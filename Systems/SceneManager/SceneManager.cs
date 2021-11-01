// Scene manager script: stages access this via the global SceneManager scene for stage transitions.
// This script utilises the SimpleSceneChanger and AnimatedSceneChanger scripts depending on the transition type desired.
// If we need to transition to an existing (hidden) stage, a SimpleChangeSceneExisting method exists which shows the required stage scene.

using Godot;
using System.Collections.Generic;
using System;

public sealed class SceneManager : CanvasLayer
{

	private TextureRect texBlack;
	private TextureRect texLoad;
	private AnimationPlayer anim;
	private Stage currentScene;
	private SceneData sceneData = new SceneData();
	private SceneChangeHelper sceneChangeHelper;
	private SimpleSceneChanger simpleSceneChanger;
	private AnimatedSceneChanger animatedSceneChanger;

	public override void _Ready()
	{
		texBlack = (TextureRect)GetNode("TexBlack");
		texLoad = (TextureRect)GetNode("TexLoad");
		anim = (AnimationPlayer)GetNode("Anim");

		if (GetTree().GetCurrentScene() is Stage)
			currentScene = (Stage)GetTree().GetCurrentScene();
		else
			GD.Print("WARNING: current scene does not inherit from stage");

		sceneChangeHelper = new SceneChangeHelper(texBlack, anim, sceneData, currentScene);
		simpleSceneChanger = new SimpleSceneChanger(sceneChangeHelper);
		animatedSceneChanger = new AnimatedSceneChanger(sceneChangeHelper, texLoad);
		AddChild(sceneChangeHelper); // Must be within the scenetree to access it via GetTree
		AddChild(simpleSceneChanger);
		AddChild(animatedSceneChanger);
		sceneChangeHelper.Name = "SceneChangeHelper"; // Setting the names aren't needed but look good in the scene tree (in remote view)
		simpleSceneChanger.Name = "SimpleSceneChanger"; // Alternatively we could do this all via the editor
		animatedSceneChanger.Name = "AnimatedSceneChanger"; // And use ready as the constructor within these nodes
	}
	

	// Arguments: 
	// SceneData.Stage (a non-existing stage scene that needs to be loaded), 
	// notepath (an existing stage scene that needs to be loaded), 
	// sharedData (pass along any shared date to the new stage), 
	// persist (whether we want the current stage to persist).

	public void SimpleChangeScene(SceneData.Stage scene, Dictionary<string, object> sharedData = null, bool persist = false, Node parent = null)
	{
		simpleSceneChanger.ChangeScene(scene, sharedData, persist, parent);
	}

	public void SimpleChangeSceneExisting(string nodepath, Dictionary<string, object> sharedData = null, bool persist = false)
	{
		simpleSceneChanger.ChangeSceneExisting(nodepath, sharedData, persist);
	}

	public void AnimatedChangeScene(SceneData.Stage scene, Dictionary<string, object> sharedData = null, bool persist = false)
	{
		animatedSceneChanger.ChangeScene(scene, sharedData, persist);
	}
	
}

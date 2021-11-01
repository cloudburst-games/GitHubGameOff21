// Simple scene changer script - used for simple stage transitions (i.e. there is just fade to and from black animations)

using Godot;
using System.Collections.Generic;
using System;

public class SimpleSceneChanger : Node
{

	private Dictionary<string, object> sharedData;
	private SceneChangeHelper sceneChangeHelper;

	private Node removedNode;

	public SimpleSceneChanger(SceneChangeHelper sceneChangeHelper)
	{
		this.sceneChangeHelper = sceneChangeHelper;
	}

	public void ChangeScene(SceneData.Stage scene, Dictionary<string, object> sharedData, bool persist, Node parent)
	{
		this.sharedData = sharedData;
		// Method called only when idle (so if scene is busy, it can finish)
		CallDeferred(nameof(DeferredSimpleChangeScene), scene, persist, parent);
	}

	public void ChangeSceneExisting(string nodepath, Dictionary<string, object> sharedData, bool persist)
	{
		this.sharedData = sharedData;
		// Method called only when idle (so if scene is busy, it can finish)
		CallDeferred(nameof(DeferredExistingChangeScene), nodepath, persist);
	}

	// Simple fade to black then load and show scene then fade from black - for scenes that are 'instant load'
	private async void DeferredSimpleChangeScene(SceneData.Stage scene, bool persist = false, Node parent = null)
	{
		// Prevent us from doing anything and prevent the scene from starting without us!
		GetTree().Paused = true;
		await sceneChangeHelper.FadeToBlack();

		FreeOrHideScene(persist);

		// Load new scene
		PackedScene nextScene = (PackedScene) GD.Load(sceneChangeHelper.sceneData.Stages[scene]);
		sceneChangeHelper.currentScene = (Stage)nextScene.Instance();
		sceneChangeHelper.currentScene.SharedData = sharedData;
		GetTree().GetRoot().AddChild(sceneChangeHelper.currentScene);
		GetTree().SetCurrentScene(sceneChangeHelper.currentScene);

		// Reparent new scene if specified
		if (parent != null)
		{
			// CallDeferred(nameof(BaseProject.Utils.Node.Reparent), sceneChangeHelper.currentScene, parent);
			BaseProject.Utils.Node.Reparent(sceneChangeHelper.currentScene, parent);
		}
		GetTree().Paused = false;
		sceneChangeHelper.currentScene.Init();
		this.sharedData = null;
		await sceneChangeHelper.FadeFromBlack();
		//**
		
	}

	// Change to existing scene - nameof scene
	public async void DeferredExistingChangeScene(string nodepath, bool persist = false)
	{
		// Prevent us from doing anything and prevent the scene from starting without us!
		GetTree().Paused = true;
		await sceneChangeHelper.FadeToBlack();

		FreeOrHideScene(persist);

		// Show existing scene
		if (HasNode(nodepath)){
			if (!((Node)GetNode(nodepath) is Stage)){
				GD.Print("Unable to change to scene. Not of Stage type.");
				throw new Exception();
			}
			Stage stage = (Stage)GetNode(nodepath);
			sceneChangeHelper.currentScene = (Stage)stage;
			sceneChangeHelper.currentScene.SharedData = sharedData;
			BaseProject.Utils.Node.RecursiveModVisibility(stage, show:true);
			GetTree().Paused = false;
			stage.Init();
			this.sharedData = null;
			await sceneChangeHelper.FadeFromBlack();
		}
		else
			GD.Print("INVALID NODE PATH IN CHANGESCENEEXISTING");

	}

	private void FreeOrHideScene(bool persist)
	{
		if (sceneChangeHelper.currentScene != null){
			// If persist is false then free the current scene
			if (!persist){
				sceneChangeHelper.currentScene.Exit();
				sceneChangeHelper.currentScene.Free();
			} else { // Otherwise just hide it
				BaseProject.Utils.Node.RecursiveModVisibility(sceneChangeHelper.currentScene, show:false);
			}
		}
	}
}

// Animated scene changer script - used for animated stage transitions (i.e. there is a loading screen in between stages).
// Note: this may not be fully implemented as extra features were added to simple scene changer including shared data and
// persisting of stages (instead of Freeing the stage scene we Hide it)

using Godot;
using System;
using System.Collections.Generic;

public class AnimatedSceneChanger : Node
{
    private Dictionary<string, object> sharedData;
    private SceneChangeHelper sceneChangeHelper;
    private TextureRect texLoad;
    private int waitFrames;
    private ResourceInteractiveLoader loader;

    public AnimatedSceneChanger(SceneChangeHelper sceneChangeHelper, TextureRect texLoad)
    {
        this.sceneChangeHelper = sceneChangeHelper;
        this.texLoad = texLoad;
    }

    public async void ChangeScene(SceneData.Stage scene, Dictionary<string, object> sharedData, bool persist)
    {
        this.sharedData = sharedData;
        await sceneChangeHelper.FadeToBlack();
        // Show loading texture
        texLoad.Show();
        await sceneChangeHelper.FadeFromBlack();
        sceneChangeHelper.anim.SetCurrentAnimation("Loading");
        LoadScene(sceneChangeHelper.sceneData.Stages[scene], persist);

    }

    // See godot documentation - background loading
    private void LoadScene(string path, bool persist)
    {
        loader = ResourceLoader.LoadInteractive(path);
        if (loader == null)
            {
                GD.Print("ERROR loading scene");
                return;
            }
        SetProcess(true);

        if (sceneChangeHelper.currentScene != null){
            // If persist is false then Qfree the current scene
            if (!persist){
                sceneChangeHelper.currentScene.Exit();
                sceneChangeHelper.currentScene.QueueFree();
            } else { // Otherwise just hide it
                BaseProject.Utils.Node.RecursiveModVisibility(sceneChangeHelper.currentScene, false);
            }
        }
        
        texLoad.Show();
        waitFrames = 1;
    }

    public override void _Process(float delta)
    {
        if (loader == null)
        {
            SetProcess(false);
            return;
        }
        if (waitFrames > 0)
        {
            waitFrames -= 1;
            return;
        }
        while (true)
        {
            Error err = loader.Poll();

            if (err == Error.FileEof)
            {
                sceneChangeHelper.anim.Seek(1, true); // show the last frame of the animation if we finished loading
                SetNewScene(loader.GetResource());
                loader = null;
                break;
            }
            else if (err == Error.Ok)
                UpdateProgress();
            else
            {
                GD.Print("ERROR loading scene");
                loader = null;
                break;
            }
        }
    }

    private async void SetNewScene(Resource sceneResource)
    {
        await sceneChangeHelper.FadeToBlack();

        texLoad.Hide(); 
        if (sceneResource == null)
        {
            GD.Print("ERROR loading scene");
            return;
        }   
        PackedScene scn = (PackedScene) sceneResource;
        sceneChangeHelper.currentScene = (Stage)scn.Instance();
        sceneChangeHelper.currentScene.SharedData = sharedData;
        GetTree().GetRoot().AddChild(sceneChangeHelper.currentScene);
        GetTree().SetCurrentScene(sceneChangeHelper.currentScene);
        this.sharedData = null;

        await sceneChangeHelper.FadeFromBlack();
    }

    private void UpdateProgress()
    {      
        float progress = (float)loader.GetStage() / (float)loader.GetStageCount();
        float length = sceneChangeHelper.anim.GetCurrentAnimationLength();
        sceneChangeHelper.anim.Seek(progress * length, true);
    }
}
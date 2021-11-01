// Scene Change Helper: helper methods used by both animated and simple scene changer scripts, e.g. fading to and from black.

using Godot;
using System;
using System.Threading.Tasks;

public class SceneChangeHelper : Node
{

    private TextureRect texBlack;
    public AnimationPlayer anim {get; private set;}
    public SceneData sceneData {get; private set;}
    public Stage currentScene {get; set;}

    public SceneChangeHelper(TextureRect texBlack, AnimationPlayer anim, SceneData sceneData, Stage currentScene)
    {
        this.texBlack = texBlack;
        this.anim = anim;
        this.sceneData = sceneData;
        this.currentScene = currentScene;
    }

    public async Task FadeToBlack()
    {
        texBlack.Show();
        anim.Play("FadeToBlack");
        await ToSignal(anim, "animation_finished");
    }

    public async Task FadeFromBlack()
    {
        anim.Play("FadeFromBlack");
        await ToSignal(anim, "animation_finished");
        texBlack.Hide();
    }
}
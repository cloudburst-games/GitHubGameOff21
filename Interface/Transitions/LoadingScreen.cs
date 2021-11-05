using Godot;
using System;

public class LoadingScreen : CanvasLayer
{
    public override void _Ready()
    {
        base._Ready();
    }
    
    public void FadeIn()
    {
        GetNode<AnimationPlayer>("FadeAnim").Play("FadeIn");
    }

    public void FadeOut()
    {
        GetNode<AnimationPlayer>("FadeAnim").Play("FadeOut"); // animation frees this node at 0.5sec!
    }
}

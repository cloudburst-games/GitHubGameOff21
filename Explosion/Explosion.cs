using Godot;
using System;

public class Explosion : Node2D
{    
    [Signal] public delegate void FinishedExploding();
    AnimationPlayer Anim;
    public override void _Ready()
    {
        Anim = GetNode<AnimationPlayer>("Anim");
        Start();
    }

    public void Start()
    {
        Anim.Play("Explode");
    }

    public void OnExplosionFinished(string _animName)
    {
        if (_animName == "Explode")
        {
          EmitSignal(nameof(FinishedExploding));
          QueueFree();
        }
    }

}

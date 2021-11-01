using Godot;
using System;

public class ExplosionManager : Node2D
{
    private PackedScene _explosionScn;
    public override void _Ready()
    {
        _explosionScn = GD.Load<PackedScene>("res://Explosion/Explosion.tscn");
    }

    public void OnButtonPressed()
    {
        Explosion explosionInstance = (Explosion)_explosionScn.Instance();
        AddChild(explosionInstance);
        explosionInstance.Position = new Vector2(1920/2, 1080/2);
    }
}

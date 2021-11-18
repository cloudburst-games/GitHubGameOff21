using Godot;
using System;

public class PortraitButton : Button
{
    [Signal]
    public delegate void InsideButton(PortraitButton btn, bool inside);
    private bool _inside = false;
    public string CurrentID {get; set;} = "";
    public override void _Process(float delta)
    {
        base._Process(delta);
        // on mouse entered/exited doesnt seem to work when the mouse button is pressed, so we do this instead
        Vector2 correctedMousePos = (GetLocalMousePosition() + RectPosition)*((Control) GetParent()).RectScale;
        
        if (correctedMousePos.x > 0 && correctedMousePos.x < RectSize.x * ((Control) GetParent()).RectScale.x && 
            correctedMousePos.y > 0 && correctedMousePos.y < RectSize.y * ((Control) GetParent()).RectScale.y && Visible)
        {
            if (_inside)
            {
                return;
            }
            _inside = true;
            EmitSignal(nameof(InsideButton), this, true);
            ShaderMaterial shaderMaterial = (ShaderMaterial) GD.Load<ShaderMaterial>("res://Shaders/Flash/FlashShader.tres").Duplicate();
            shaderMaterial.SetShaderParam("speed", 10f);
            shaderMaterial.SetShaderParam("flash_colour_original", new Color(1,1,1));
            shaderMaterial.SetShaderParam("flash_depth", 0.4f);
            GetNode<TextureRect>("TexRect").Material = shaderMaterial;
        }
        else
        {
            if (_inside)
            {
                EmitSignal(nameof(InsideButton), this, false);
            }
            _inside = false;
            if (GetNode<TextureRect>("TexRect").Material != null)
            {
                GetNode<TextureRect>("TexRect").Material = null;
            }
        }
        
    }
}

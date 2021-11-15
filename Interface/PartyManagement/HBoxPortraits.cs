using Godot;
using System;
using System.Linq;

public class HBoxPortraits : Control
{
    [Signal]
    public delegate void PopupPressed(int id, int unitPortrait);

    private Vector2[] _pBtnPositions = new Vector2[3] { new Vector2(10,0), new Vector2(65,0), new Vector2(120,0) };
    private Button[] _pBtns;
    private int _indexMouseOver = -1;
    public override void _Ready()
    {
        _pBtns = new Button[3] {GetNode<Button>("PBtnPlayer"), GetNode<Button>("PBtnCompanion1"), GetNode<Button>("PBtnCompanion2")};
        foreach (Button btn in _pBtns)
        {
            btn.Connect("mouse_entered", this, nameof(OnPBtnMouseEntered), new Godot.Collections.Array {btn});
            btn.Connect("mouse_exited", this, nameof(OnPBtnMouseExited), new Godot.Collections.Array {btn});
        }
    }

    public void SetPBtnVisible(int index, bool visible)
    {
        _pBtns[index].Visible = visible;
        UpdatePBtnPositions();
    }
    
    private void UpdatePBtnPositions()
    {
        if (_pBtns[1].Visible)
        {
            _pBtns[1].RectPosition = _pBtnPositions[1];
        }
        else
        {
            _pBtns[2].RectPosition = _pBtnPositions[1];
        }
    }

    private void OnPBtnMouseEntered(Button btn)
    {
        ShaderMaterial shaderMaterial = (ShaderMaterial) GD.Load<ShaderMaterial>("res://Shaders/Flash/FlashShader.tres").Duplicate();
        shaderMaterial.SetShaderParam("speed", 6f);
        shaderMaterial.SetShaderParam("flash_colour_original", new Color(1,1,1));
        shaderMaterial.SetShaderParam("flash_depth", 0.2f);
        btn.GetNode<TextureRect>("TexRect").Material = shaderMaterial;
        _indexMouseOver = _pBtns.ToList().IndexOf(btn);
    }

    public override void _Input(InputEvent ev)
    {
        base._Input(ev);

        if (ev is InputEventMouseButton btn && !ev.IsEcho())
        {
            if (btn.ButtonIndex == (int) ButtonList.Right)
            {
                if (btn.Pressed)
                {
                    if (_indexMouseOver == -1)
                    {
                        return;
                    }
                    GetNode<PopupMenu>("PopupMenu").SetItemDisabled(2, _indexMouseOver == 0);
                    GetNode<PopupMenu>("PopupMenu").RectGlobalPosition = _pBtns[_indexMouseOver].RectGlobalPosition;
                    GetNode<PopupMenu>("PopupMenu").Popup_();
                }
            }
        }
    }

    private void OnPBtnMouseExited(Button btn)
    {
        _indexMouseOver = -1;
        btn.GetNode<TextureRect>("TexRect").Material = null;
    }

    public void UpdatePortrait(int index, Texture tex)
    {
        _pBtns[index].GetNode<TextureRect>("TexRect").Texture = tex;
    }

    public void OnPopupMenuIDPressed(int id)
    {
        EmitSignal(nameof(PopupPressed), id, _indexMouseOver);
    }
}

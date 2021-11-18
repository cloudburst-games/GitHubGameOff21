using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class HBoxPortraits : Control
{
    [Signal]
    public delegate void PopupPressed(int id, string unitID);//int unitPortrait);

    [Signal]
    public delegate void PortraitPressed(string unitID);// int unitPortrait);

    [Signal]
    public delegate void InsideButtonOfCharacter(string id, bool inside);

    private Vector2[] _pBtnPositions = new Vector2[3] { new Vector2(0,0), new Vector2(128,0), new Vector2(256,0) };
    private PortraitButton[] _pBtns;
    private Dictionary<string, PortraitButton> _unitBtnsByID = new Dictionary<string, PortraitButton>();
    private string _idOver = null;
    public bool InCharacterManager {get; set;} = false;
    private string _IDPopUpSelected = null;

    public override void _Ready()
    {
        _pBtns = new PortraitButton[3] {GetNode<PortraitButton>("PBtnPlayer"), GetNode<PortraitButton>("PBtnCompanion1"), GetNode<PortraitButton>("PBtnCompanion2")};
        foreach (PortraitButton btn in _pBtns)
        {   // the mouse_entered and mouse_exited signals don't seem to be reliable, so we made our own which works better
            // i dare not remove the first two though, things break so easily
            btn.Connect("mouse_entered", this, nameof(OnPBtnMouseEntered), new Godot.Collections.Array {btn});
            btn.Connect("mouse_exited", this, nameof(OnPBtnMouseExited), new Godot.Collections.Array {btn});
            btn.Connect(nameof(PortraitButton.InsideButton), this, nameof(OnInsidePortraitButton));
        }
    }

    // from the signal in each PortraitButton
    public void OnInsidePortraitButton(PortraitButton btn, bool inside)
    {
        string id = _unitBtnsByID.FirstOrDefault(x => x.Value == btn).Key;
        if (id != null)
        {
            EmitSignal(nameof(InsideButtonOfCharacter), id, inside);
        }
    }

    public void ResetPositions()
    {
        SetPBtnVisible(1, false);
        SetPBtnVisible(2, false);
        _unitBtnsByID.Clear();
    }

    // called on levelup to really make the player take notice
    public void SetToFlashIntensely(string id, LblFloatScore lvlUpFloatLbl)
    {
        if (_unitBtnsByID.ContainsKey(id))
        {
            Timer timer = new Timer();
            timer.WaitTime = 6f;
            timer.OneShot = true;
            timer.Connect("timeout", this, nameof(OnFlashTimerTimeout), new Godot.Collections.Array {timer, id});
            AddChild(timer);
            timer.Start();
            ShaderMaterial shaderMaterial = (ShaderMaterial) GD.Load<ShaderMaterial>("res://Shaders/Flash/FlashShader.tres").Duplicate();
            shaderMaterial.SetShaderParam("speed", 12f);
            shaderMaterial.SetShaderParam("flash_colour_original", new Color(.5f,.5f,.5f));
            shaderMaterial.SetShaderParam("flash_depth", 1f);
            _unitBtnsByID[id].Material = shaderMaterial;
            _unitBtnsByID[id].AddChild(lvlUpFloatLbl);
            lvlUpFloatLbl.Start(_unitBtnsByID[id].RectGlobalPosition);
        }
    }

    private void OnFlashTimerTimeout(Timer timer, string id)
    {
        timer.QueueFree();
        if (_unitBtnsByID.ContainsKey(id))
        {
            _unitBtnsByID[id].Material = null;
        }
    }

    // this is where it all begins (player and companion IDs are passed into here at start and whenever companion changes)
    public void SetSingleUnitBtnByID(int index, string companionID)
    {
        _unitBtnsByID[companionID] = _pBtns[index];
        _unitBtnsByID[companionID].CurrentID = companionID;
        if (_unitBtnsByID[companionID].IsConnected("pressed", this, nameof(OnPortraitButtonPressed)))
        {
            _unitBtnsByID[companionID].Disconnect("pressed", this, nameof(OnPortraitButtonPressed));
        }
        _unitBtnsByID[companionID].Connect("pressed", this, nameof(OnPortraitButtonPressed), new Godot.Collections.Array {_unitBtnsByID[companionID].CurrentID});
        SetPBtnVisible(index, true);
    }

    // this is also called with above, at start and whenever companion changes
    public void SetPortrait(string unitID, Texture tex)
    {
        if (unitID != "")
        {
            _unitBtnsByID[unitID].GetNode<TextureRect>("TexRect").Texture = tex;
        }
    }

    // this is probably not needed but we use this to know which portrait we are over to bring up the menu on click
    // may be better just to use the signal we made for portrait button but we did that afterwards oh well
    private void OnPBtnMouseEntered(Button btn)
    {
        _idOver = _unitBtnsByID.FirstOrDefault(x => x.Value == btn).Key;
    }
    private void OnPBtnMouseExited(Button btn)
    {
        _idOver = null;
    }


    
    // detecting left or right btn clicks to bring up the popupmenu (only for the bototm part)
    // rather than using InCharacterManager bool would be better to have 2 diff states and set state depending on 
    // where we are
    public override void _Input(InputEvent ev)
    {
        base._Input(ev);

        if (ev is InputEventMouseButton btn && !ev.IsEcho())
        {
            if (!InCharacterManager)
            {
                if (btn.ButtonIndex == (int) ButtonList.Right || btn.ButtonIndex == (int) ButtonList.Left)
                {
                    if (btn.Pressed)
                    {
                        if (_idOver == null)
                        {
                            return;
                        }
                        PortraitButton btnSelected = _unitBtnsByID[_idOver];
                        int indexOfBtnSelected = _pBtns.ToList().IndexOf(btnSelected);
                        GetNode<PopupMenu>("PopupMenu").SetItemDisabled(2, indexOfBtnSelected == 0);
                        GetNode<PopupMenu>("PopupMenu").RectGlobalPosition = _pBtns[indexOfBtnSelected].RectGlobalPosition;
                        GetNode<PopupMenu>("PopupMenu").Popup_();
                        _IDPopUpSelected = _idOver;
                    }
                }
            }
        }
    }

    public void OnPortraitButtonPressed(string ID)
    {
        if (InCharacterManager)
        {
            DisableOnePortraitButtonByID(ID);
            EmitSignal(nameof(PortraitPressed), ID);
        }
    }

    public void DisableOnePortraitButtonByID(string id)
    {
        foreach (string s in _unitBtnsByID.Keys)
        {
            _unitBtnsByID[s].Disabled = false;
            if (s == id)
            {
                _unitBtnsByID[id].Disabled = true;
            }
            
        }
    }


    // handling popupmenu clicks
    public void OnPopupMenuIDPressed(int id)
    {
        EmitSignal(nameof(PopupPressed), id, _IDPopUpSelected);
    }

    private void SetPBtnVisible(int index, bool visible)
    {
        _pBtns[index].Visible = visible;
        UpdatePBtnPositions();
    }
    
    // TODO -REWRITE THIS MONSTROSITY
    private void UpdatePBtnPositions()
    {
        // 100 110 101 111. crude..
        if (! _pBtns[1].Visible && ! _pBtns[2].Visible)
        {
            _pBtns[1].RectPosition = _pBtnPositions[1];
            _pBtns[2].RectPosition = _pBtnPositions[2];
        }
        else if (_pBtns[1].Visible && ! _pBtns[2].Visible)
        {
            _pBtns[1].RectPosition = _pBtnPositions[1];
        }
        else if (!_pBtns[1].Visible && _pBtns[2].Visible)
        {
            _pBtns[2].RectPosition = _pBtnPositions[1];
        }
        else if (_pBtns[1].Visible && _pBtns[2].Visible)
        {
            _pBtns[1].RectPosition = _pBtnPositions[1];
            _pBtns[2].RectPosition = _pBtnPositions[2];
        }
    }
 
}

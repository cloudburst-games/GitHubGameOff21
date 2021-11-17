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

    private Vector2[] _pBtnPositions = new Vector2[3] { new Vector2(10,0), new Vector2(65,0), new Vector2(120,0) };
    private Button[] _pBtns;
    private Dictionary<string, Button> _unitBtnsByID = new Dictionary<string, Button>();
    // private int _indexMouseOver = -1;
    private string _idOver = null;
    // private int _portraitSelected = -1;

    public bool InCharacterManager {get; set;} = false;

    public override void _Ready()
    {
        _pBtns = new Button[3] {GetNode<Button>("PBtnPlayer"), GetNode<Button>("PBtnCompanion1"), GetNode<Button>("PBtnCompanion2")};
        foreach (Button btn in _pBtns)
        {
            btn.Connect("mouse_entered", this, nameof(OnPBtnMouseEntered), new Godot.Collections.Array {btn});
            btn.Connect("mouse_exited", this, nameof(OnPBtnMouseExited), new Godot.Collections.Array {btn});
        }
    //     SetPBtnVisible(1, false);
    //     SetPBtnVisible(2, false);
    //     _unitBtnsByID["khepri"] = _pBtns[0];
    }

    public void ResetPositions()
    {
        SetPBtnVisible(1, false);
        SetPBtnVisible(2, false);
    }

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

    // initialising.
    // call this to begin with
    // public void SetAllBtnsByID(string playerID, string[] companionIDs)
    // {
    //     SetPlayerUnitBtn(playerID);
    //     SetUnitBtnsByID(companionIDs);
    // }

    // set this whenever companion is updated
    // public void SetUnitBtnsByID(string[] companionIDs)
    // {
    //     if (companionIDs.Length != 2)
    //     {
    //         GD.Print("need to pass 2 strings to HBoxPortraits.cs SetUnitBtnsByID");
    //         return;
    //     }
    //     for (int i = 0; i < companionIDs.Length; i++)
    //     {
    //         if (companionIDs[i] != "")
    //         {
    //             _unitBtnsByID[companionIDs[i]] = _pBtns[i+1];
    //         }
    //     }
    // }
    public void SetSingleUnitBtnByID(int index, string companionID)
    {
        // if (companionIDs.Length != 2)
        // {
        //     GD.Print("need to pass 2 strings to HBoxPortraits.cs SetUnitBtnsByID");
        //     return;
        // }
        // for (int i = 0; i < companionIDs.Length; i++)
        // {
            // if (companionIDs[i] != "")
            // {
        _unitBtnsByID[companionID] = _pBtns[index];
        SetPBtnVisible(index, true);
            // }
        // }
    }

    // public void SetPlayerUnitBtn(string playerID)
    // {
    //     _unitBtnsByID[playerID] = _pBtns[0];
    // }

    // also call this to set the portraits whenever a companion is updated
    public void SetPortrait(string unitID, Texture tex)
    {
        if (unitID != "")
        {
            _unitBtnsByID[unitID].GetNode<TextureRect>("TexRect").Texture = tex;
        }
    }

    // Flash shader stuff
    private void OnPBtnMouseEntered(Button btn)
    {
        ShaderMaterial shaderMaterial = (ShaderMaterial) GD.Load<ShaderMaterial>("res://Shaders/Flash/FlashShader.tres").Duplicate();
        shaderMaterial.SetShaderParam("speed", 6f);
        shaderMaterial.SetShaderParam("flash_colour_original", new Color(1,1,1));
        shaderMaterial.SetShaderParam("flash_depth", 0.2f);
        btn.GetNode<TextureRect>("TexRect").Material = shaderMaterial;
        // _indexMouseOver = _pBtns.ToList().IndexOf(btn); /// tells us which button its over
        _idOver = _unitBtnsByID.FirstOrDefault(x => x.Value == btn).Key;
        GD.Print(_idOver);
    }

    private void OnPBtnMouseExited(Button btn)
    {
        // _indexMouseOver = -1;
        btn.GetNode<TextureRect>("TexRect").Material = null;
        _idOver = null;
    }

    private string _IDPopUpSelected = null;
    
    // detecting left or right btn clicks
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
                        Button btnSelected = _unitBtnsByID[_idOver];
                        int indexOfBtnSelected = _pBtns.ToList().IndexOf(btnSelected);
                        GetNode<PopupMenu>("PopupMenu").SetItemDisabled(2, indexOfBtnSelected == 0);
                        GetNode<PopupMenu>("PopupMenu").RectGlobalPosition = _pBtns[indexOfBtnSelected].RectGlobalPosition;
                        GetNode<PopupMenu>("PopupMenu").Popup_();
                        _IDPopUpSelected = _idOver;
                    }
                }
            }
            else
            {
                if (btn.ButtonIndex == (int) ButtonList.Left && btn.Pressed && _idOver != null)// _indexMouseOver != -1)
                {
                    // Button portraitBtn = _unitBtnsByID[_idOver];
                    EmitSignal(nameof(PortraitPressed), _idOver);// _unitBtnsByID
                        // .FirstOrDefault(x => x.Value == portraitBtn).Key); // _indexMouseOver);
                    // var myKey = types.FirstOrDefault(x => x.Value == "one").Key;
                }
            }
        }
    }   
    // handling popupmenu clicks
    public void OnPopupMenuIDPressed(int id)
    {
        // Button portraitBtn = _unitBtnsByID[_IDPopUpSelected];
        EmitSignal(nameof(PopupPressed), id, _IDPopUpSelected);
        // EmitSignal(nameof(PopupPressed), id, _portraitSelected);
    }

    private void SetPBtnVisible(int index, bool visible)
    {
        _pBtns[index].Visible = visible;
        UpdatePBtnPositions();
    }
    
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





    // // public void SetPortrait(int index, Texture tex)
    // // {
    // //     _pBtns[index].GetNode<TextureRect>("TexRect").Texture = tex;
    // // }

    // private void SetPortrait(UnitData unitData)
    // {
    //     _unitBtnsByID[unitData.ID].GetNode<TextureRect>("TexRect").Texture = GD.Load<Texture>(unitData.PortraitPathSmall);
    // }

    // public void UpdateCharacters(List<UnitData> unitDatas)
    // {
    //     if (unitDatas.Count > 2)
    //     {
    //         for (int i = 0; i < 100; i++)
    //             GD.Print("error somewhere (see HBoxPortraits.cs UpdateCharacters) - should not have more than 2 companions");
    //         // throw new Exception("error somewhere (see HBoxPortraits.cs UpdateCharacters) - should not have more than 2 companions");
    //     }
    //     for (int i = 0; i < 2; i++)
    //     {
    //         if (unitDatas.Count < i+1)
    //         {
    //             SetPBtnVisible(i+1, false);
    //             continue;
    //         }
    //         _unitBtnsByID[unitDatas[i].ID] = _pBtns[i];
    //         SetPortrait(unitDatas[i]);
    //         SetPBtnVisible(i+1, true);
    //     }
        

    //     // TODO. call this for BOTH HBoxPortraits when need to change companion etc.
    // }

 
}

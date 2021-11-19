using Godot;
using System;
using System.Collections.Generic;

public class BattleHUD : CanvasLayer
{
    public Dictionary<CntBattle.ActionMode, Button> ActionButtons {get; set;}
    private Label _lblLog;
    private BattleUnitInfoPanel _battleUnitInfoPanel;
    private List<string> _logEntries = new List<string>();
    public override void _Ready()
    {
        _lblLog = GetNode<Label>("CtrlTheme/PnlUI/LblLog");
        _battleUnitInfoPanel = GetNode<BattleUnitInfoPanel>("CtrlTheme/BattleUnitInfoPanel");
        ActionButtons = new Dictionary<CntBattle.ActionMode, Button>()
        {
            {CntBattle.ActionMode.Move, GetNode<Button>("CtrlTheme/PnlUI/HBoxActions/BtnMove")},
            {CntBattle.ActionMode.Melee, GetNode<Button>("CtrlTheme/PnlUI/HBoxActions/BtnMelee")},
            {CntBattle.ActionMode.Spell1, GetNode<Button>("CtrlTheme/PnlUI/HBoxActions/BtnSpell1")},
            {CntBattle.ActionMode.Spell2, GetNode<Button>("CtrlTheme/PnlUI/HBoxActions/BtnSpell2")},
            {CntBattle.ActionMode.Wait, GetNode<Button>("CtrlTheme/PnlUI/HBoxActions/BtnEndTurn")},
            {CntBattle.ActionMode.Potion, GetNode<Button>("CtrlTheme/PnlUI/HBoxActions/BtnPotion")},
        };
        GetNode<Panel>("CtrlTheme/PnlUI").Visible = true;

    }

    public void ClearLog()
    {
        _logEntries.Clear();
    }

    public void LogEntry(string text)
    {
        _lblLog.Text = text;
        _logEntries.Add(text);
    }

    public void ShowButDoNotLog(string text)
    {
        _lblLog.Text = text;
    }

    public void OnMouseEnteredActionButton(Button btn)
    {
        ShowButDoNotLog(btn.HintTooltip);
    }

    public void OnMouseExitedActionButton(Button btn)
    {
        ShowButDoNotLog("");
    }

    public void ShowLog()
    {
        GetNode<PnlLog>("CtrlTheme/PnlLog").Show(_logEntries);
    }

    public void LogMeleeEntry(string aggressorName, string defenderName, float[] result, bool death) // crit, dodge, damage
    {
        LogEntry(String.Format("{0} strikes {1} for {2} damage.{3}{4}{5}", aggressorName, defenderName, Math.Round(result[2], 1),
            (result[0] == 2 ? " Double damage from critical hit!" : ""),
            (result[1] == 2 ? " Damage halved due to dodge!" : ""),
            death ? " " + defenderName + " perishes!" : "")
            );
    }

    public void SetDisableAllActionButtons(bool disable)
    {
        foreach (Node n in GetNode("CtrlTheme/PnlUI/HBoxActions").GetChildren())
        {
            if (n is Button btn)
            {
                SetDisableSingleButton(btn, disable);
            }
        }
        GetNode<Button>("CtrlTheme/PnlUI/BtnMenu").Disabled = disable;
    }
    public void SetDisableAllAnimSpeedButtons(bool disable)
    {
        foreach (Node n in GetNode("CtrlTheme/PnlUI/HBoxAnimSpeed").GetChildren())
        {
            if (n is Button btn)
            {
                SetDisableSingleButton(btn, disable);
            }
        }
    }

    public void SetDisableSingleButton(Button btn, bool disable)
    {
        btn.Disabled = disable;
        foreach (Node n in btn.GetChildren())
        {
            if (n is Sprite s)
            {
                s.Modulate = disable ? new Color(0.2f, 0.2f, 0.2f, 0.5f) : new Color(1,1,1,1);
            }
        }
    }

    public void SetSpellUI(SpellEffect effect1, SpellEffect effect2)
    {
        // if (effect1 == null)
        // {
        //     GetNode<Button>("CtrlTheme/PnlUI/HBoxActions/BtnSpell1").Disabled = true;
        // }
        GetNode<Sprite>("CtrlTheme/PnlUI/HBoxActions/BtnSpell1/Spell").Texture = effect1.IconTex;
        GetNode<Button>("CtrlTheme/PnlUI/HBoxActions/BtnSpell1").HintTooltip = effect1.Name + ": " + effect1.ToolTip;
        
        // if (effect2 == null)
        // {
        //     GetNode<Button>("CtrlTheme/PnlUI/HBoxActions/BtnSpell2").Disabled = true;
        // }
        GetNode<Sprite>("CtrlTheme/PnlUI/HBoxActions/BtnSpell2/Spell2").Texture = effect2.IconTex;
        GetNode<Button>("CtrlTheme/PnlUI/HBoxActions/BtnSpell2").HintTooltip = effect2.Name + ": " + effect2.ToolTip;
        
    }

    // public void SetDisableSpellUI(bool spell1, bool spell2)
    // {
    //     GetNode<Button>("CtrlTheme/PnlUI/HBoxActions/BtnSpell1").Visible = !spell1;
    //     GetNode<Button>("CtrlTheme/PnlUI/HBoxActions/BtnSpell2").Visible = !spell2;
    // }


    public override void _Input(InputEvent ev)
    {
        base._Input(ev);
        if (ev is InputEventMouseButton btn)// && !(ev.IsEcho()))
        {
            // if (btn.ButtonIndex == (int) ButtonList.Right)
            // {
                if (!btn.Pressed)
                {
                    _battleUnitInfoPanel.Visible = false;
                }
            // }
        }
    }

    public void UpdateAndShowUnitInfoPanel(string name, Dictionary<BattleUnitData.DerivedStat, float> derivedStats, Dictionary<SpellEffectManager.SpellMode, Tuple<int, float>> currentEffects, Dictionary<SpellEffectManager.SpellMode, string> effectNames, List<string> spellsLearned, PnlInventory.ItemMode weaponEquipped, PnlInventory.ItemMode amuletEquipped, PnlInventory.ItemMode armourEquipped, PnlInventory.ItemMode[] potionsEquipped, string portraitPath)
    {
        _battleUnitInfoPanel.Update(name, derivedStats, currentEffects, effectNames, spellsLearned, weaponEquipped, amuletEquipped, armourEquipped, potionsEquipped, portraitPath);
        _battleUnitInfoPanel.Activate();
    }
}

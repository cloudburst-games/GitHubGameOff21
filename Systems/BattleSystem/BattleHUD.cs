using Godot;
using System;
using System.Collections.Generic;

public class BattleHUD : CanvasLayer
{
    public Dictionary<CntBattle.ActionMode, Button> ActionButtons {get; set;}
    private Label _lblLog;
    private BattleUnitInfoPanel _battleUnitInfoPanel;
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
    };

    }

    public void LogEntry(string text)
    {
        _lblLog.Text = text;
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

    public override void _Input(InputEvent ev)
    {
        base._Input(ev);
        if (ev is InputEventMouseButton btn)// && !(ev.IsEcho()))
        {
            if (btn.ButtonIndex == (int) ButtonList.Right)
            {
                if (!btn.Pressed)
                {
                    _battleUnitInfoPanel.Visible = false;
                }
            }
        }
    }

    public void UpdateAndShowUnitInfoPanel(string name, Dictionary<BattleUnitData.DerivedStat, float> derivedStats)
    {
        _battleUnitInfoPanel.Update(name, derivedStats);
        _battleUnitInfoPanel.Activate();
    }
}

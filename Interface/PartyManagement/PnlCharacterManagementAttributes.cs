using Godot;
using System;
// using System.Linq;
using System.Collections.Generic;

public class PnlCharacterManagementAttributes : Panel
{
    [Signal]
    public delegate void AttributePointSpent(Dictionary<BattleUnitData.DerivedStat, float> stats);
    [Signal]
    public delegate void AllAttributePointsSpent();
    private Dictionary<UnitData.Attribute, Panel> _attributePanels;
    private Dictionary<UnitData.Attribute, Button> _attributeButtons;
    private Dictionary<UnitData.Attribute, string> _attributeExplanations = new Dictionary<UnitData.Attribute, string>()
    {
        {UnitData.Attribute.Vigour, "Physical prowess. Determines physical damage and health."},
        {UnitData.Attribute.Resilience, "Shrug off magical attacks and outlast the opponent. Increases health and mana regeneration, and magic resistance."},
        {UnitData.Attribute.Intellect, "A great mind lends itself to even greater magical power. Increases mana capacity, mana regeneration, and spell power."},
        {UnitData.Attribute.Swiftness, "Outmaneuver the opponent. Increases initiative (strike first), speed (move fast), and dodge (chance to reduce incoming damage)."},
        {UnitData.Attribute.Charisma, "Improves leadership (provides a bonus in battle to nearby companions) and used in dialogue."},
        {UnitData.Attribute.Luck, "Shifts encounters into the character's favour. Improves chance to critically hit, and chance to dodge an attack."}
    };
    public override void _Ready()
    {
        base._Ready();
        _attributePanels = new Dictionary<UnitData.Attribute, Panel>() {
            {UnitData.Attribute.Vigour, GetNode<Panel>("VBoxAttributes/PnlVigour")},
            {UnitData.Attribute.Resilience, GetNode<Panel>("VBoxAttributes/PnlResilience")},
            {UnitData.Attribute.Swiftness, GetNode<Panel>("VBoxAttributes/PnlSwiftness")},
            {UnitData.Attribute.Luck, GetNode<Panel>("VBoxAttributes/PnlLuck")},
            {UnitData.Attribute.Charisma, GetNode<Panel>("VBoxAttributes/PnlCharisma")},
            {UnitData.Attribute.Intellect, GetNode<Panel>("VBoxAttributes/PnlIntellect")}
        };
        _attributeButtons = new Dictionary<UnitData.Attribute, Button>() {
            {UnitData.Attribute.Vigour, GetNode<Button>("VBoxAttributes/PnlVigour/HBox/BtnIncrease")},
            {UnitData.Attribute.Resilience, GetNode<Button>("VBoxAttributes/PnlResilience/HBox/BtnIncrease")},
            {UnitData.Attribute.Swiftness, GetNode<Button>("VBoxAttributes/PnlSwiftness/HBox/BtnIncrease")},
            {UnitData.Attribute.Luck, GetNode<Button>("VBoxAttributes/PnlLuck/HBox/BtnIncrease")},
            {UnitData.Attribute.Charisma, GetNode<Button>("VBoxAttributes/PnlCharisma/HBox/BtnIncrease")},
            {UnitData.Attribute.Intellect, GetNode<Button>("VBoxAttributes/PnlIntellect/HBox/BtnIncrease")}
        };

        foreach (UnitData.Attribute att in _attributePanels.Keys)
        {
            _attributePanels[att].Connect("mouse_entered", this, nameof(OnAttributePanelMouseEntered), new Godot.Collections.Array {att});
            _attributePanels[att].Connect("mouse_exited", this, nameof(OnAttributePanelMouseExited));
        }
        foreach (UnitData.Attribute att in _attributeButtons.Keys)
        {
            _attributeButtons[att].Connect("mouse_entered", this, nameof(OnAttributePanelMouseEntered), new Godot.Collections.Array {att});
            _attributeButtons[att].Connect("mouse_exited", this, nameof(OnAttributePanelMouseExited));
        }

        SetStatusText("");
    }
    public void SetStatusText(string text)
    {
        GetNode<Label>("PnlStatus/LblStatus").Text = text;
    }

    public void SpendPointOnAttribute(UnitData.Attribute att, UnitDataSignalWrapper unitDataSignalWrapper)
    {
        unitDataSignalWrapper.CurrentUnitData.AttributePoints -= 1;
        unitDataSignalWrapper.CurrentUnitData.Attributes[att] += 1;
        UpdatePointsLeftDisplay(unitDataSignalWrapper.CurrentUnitData.AttributePoints);
        UpdateAttributesDisplay(unitDataSignalWrapper.CurrentUnitData);
        unitDataSignalWrapper.CurrentUnitData.UpdateDerivedStatsFromAttributes();
        EmitSignal(nameof(AttributePointSpent), unitDataSignalWrapper.CurrentUnitData.CurrentBattleUnitData.Stats);
    }

    private void UpdatePointsLeftDisplay(int pointsRemaining)
    {
        GetNode<Label>("LblSkillPoints").Text = pointsRemaining > 0 ? "Points remaining: " + pointsRemaining : "";
        foreach (Panel p in _attributePanels.Values)
        {
            p.GetNode<Button>("HBox/BtnIncrease").Visible = pointsRemaining > 0;
        }
        if (pointsRemaining == 0)
        {
            EmitSignal(nameof(AllAttributePointsSpent));
        }
    }

    public void OnAttributePanelMouseEntered(UnitData.Attribute att)
    {
        SetStatusText(_attributeExplanations[att]);
    }   
    public void OnAttributePanelMouseExited()
    {
        SetStatusText("");
    }

    private void ConnectAttributePointBtns(UnitData unitData)
    {
        foreach (UnitData.Attribute a in unitData.Attributes.Keys)
        {
            if (_attributePanels[a].GetNode<Button>("HBox/BtnIncrease").IsConnected("pressed", this, nameof(SpendPointOnAttribute)))
            {
                _attributePanels[a].GetNode<Button>("HBox/BtnIncrease").Disconnect("pressed", this, nameof(SpendPointOnAttribute));
            }
            _attributePanels[a].GetNode<Button>("HBox/BtnIncrease").Connect("pressed", this, nameof(SpendPointOnAttribute), new Godot.Collections.Array {a, 
            new UnitDataSignalWrapper() {CurrentUnitData = unitData} });
        }
    }

    private void UpdateAttributesDisplay(UnitData unitData)
    {
        foreach (UnitData.Attribute a in unitData.Attributes.Keys)
        {
            _attributePanels[a].GetNode<Label>("HBox/LblNum").Text = unitData.Attributes[a].ToString();
        }
        GetNode<Label>("LblArmour").Text = String.Format("Armour: {0}", unitData.GetArmourBonus());
    }
    public void OnAttributesChanged(Dictionary<UnitData.Attribute, int> atts, float armour)
    {
        foreach (UnitData.Attribute a in atts.Keys)
        {
            _attributePanels[a].GetNode<Label>("HBox/LblNum").Text = atts[a].ToString();
        }
        GetNode<Label>("LblArmour").Text = String.Format("Armour: {0}", armour);
    }
    public void Start(UnitData unitData)
    {
        UpdateAttributesDisplay(unitData);
        UpdatePointsLeftDisplay(unitData.AttributePoints);
        ConnectAttributePointBtns(unitData);
    }
}

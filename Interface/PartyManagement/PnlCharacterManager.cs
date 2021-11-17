using Godot;
using System;

public class PnlCharacterManager : Panel
{

    [Signal]
    public delegate void RequestedPause(bool pauseEnable);
    private PnlCharacterManagementAttributes _pnlAttributes;
    private PnlCharacterManagementStats _pnlStats;
    public override void _Ready()
    {
        _pnlAttributes = GetNode<PnlCharacterManagementAttributes>("TabContainer/Character/TabContainer/Attributes");
        _pnlStats = GetNode<PnlCharacterManagementStats>("TabContainer/Character/TabContainer/Derived Stats (Advanced)");
        _pnlAttributes.Connect(nameof(PnlCharacterManagementAttributes.AttributePointSpent), _pnlStats, nameof(PnlCharacterManagementStats.UpdateStatDisplay));
        GetNode<HBoxPortraits>("HBoxPortraits").InCharacterManager = true;
        Visible = false;
    }

    public void Start(UnitData unitData, int mode) // 0 = character, 1 = inventory
    {
        // load unit data
        GetNode<Label>("LblCharacterName").Text = unitData.Name;
        GetNode<TextureRect>("TabContainer/Character/TexRectPortrait").Texture = GD.Load<Texture>(unitData.PortraitPath);
        GetNode<Label>("TabContainer/Character/LblLevelExperience").Text = String.Format("Level: {0}\nExperience {1}",
            unitData.CurrentBattleUnitData.Level, unitData.CurrentBattleUnitData.Experience);
        _pnlAttributes.Start(unitData);
        _pnlStats.Start(unitData.CurrentBattleUnitData.Stats, new SpellEffectManager.SpellMode[2] 
            {unitData.CurrentBattleUnitData.Spell1, unitData.CurrentBattleUnitData.Spell2});
        // show relevant tab
        Visible = true;
        GetNode<TabContainer>("TabContainer").CurrentTab = mode;
    }

    private void OnBtnClosePressed()
    {
        // set sub-tabs to defaults
        GetNode<TabContainer>("TabContainer/Character/TabContainer").CurrentTab = 0;
        EmitSignal(nameof(RequestedPause), false);
        Visible = false;
    }

    
}

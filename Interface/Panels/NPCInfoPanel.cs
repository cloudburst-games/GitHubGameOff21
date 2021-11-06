using Godot;
using System;
using System.Collections.Generic;

public class NPCInfoPanel : Panel
{
    public override void _Ready()
    {
        Visible = false;

        // Test();
    }

    private void Test()
    {
        // UnitData unitData = new UnitData();
        // unitData.MainCombatant = BattleUnit.Combatant.Wasp;
        // unitData.Minions = new System.Collections.Generic.Dictionary<BattleUnit.Combatant, int>()
        // {
        //     {BattleUnit.Combatant.Beetle, 3},
        //     {BattleUnit.Combatant.Noob, 4},
        //     {BattleUnit.Combatant.Wasp, 1}
        // };
        // unitData.Hostile = true;
        // Activate(unitData);
    }

    public void Activate(UnitData unitData)
    {
        RectGlobalPosition = new Vector2(Mathf.Clamp(GetGlobalMousePosition().x, 0, GetViewportRect().Size.x - RectSize.x), 
            Mathf.Clamp(GetGlobalMousePosition().y, 0, GetViewportRect().Size.y - RectSize.y));
        // int value = GetValueFromDb();
        // var enumDisplayStatus = (EnumDisplayStatus)value;
        // string stringValue = enumDisplayStatus.ToString();

        GetNode<Label>("VBoxLabels/LblMainCombatant").Text = "Leader: " + Enum.GetName(typeof(BattleUnit.Combatant), unitData.MainCombatant.Combatant);
        GetNode<Label>("VBoxLabels/LblMinions").Text = unitData.Minions.Count == 0 ? "" : "Minions:";

        var combatantNumbers = new Dictionary<BattleUnit.Combatant, int>() {
            {BattleUnit.Combatant.Beetle, 0},
            {BattleUnit.Combatant.Noob, 0},
            {BattleUnit.Combatant.Wasp, 0}
        };
        foreach (BattleUnitData battleUnitData in unitData.Minions)
        {
            combatantNumbers[battleUnitData.Combatant] += 1;
        }

        foreach (BattleUnit.Combatant minion in combatantNumbers.Keys)
        {
            GetNode<Label>("VBoxLabels/LblMinions").Text += GetFormattedCombatants(minion,combatantNumbers[minion]);
        }
        GetNode<Label>("VBoxLabels/LblHostileStatus").Text = String.Format(unitData.Hostile? "Status: Hostile" : "Status: Friendly");
        GetNode<Label>("VBoxLabels/LblHostileStatus").AddColorOverride("font_color", unitData.Hostile? new Color(1,0,0) : new Color(0,1,0));
        Visible = true;

    }

    private string GetFormattedCombatants(BattleUnit.Combatant combatant, int num)
    {
        return String.Format("\n{0} {1}{2}", num, Enum.GetName(typeof(BattleUnit.Combatant), combatant), num > 1 ? "s" : "");
    }

}

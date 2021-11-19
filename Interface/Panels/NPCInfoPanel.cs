using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class NPCInfoPanel : Panel
{
    private ItemBuilder _itemBuilder = new ItemBuilder();
    public override void _Ready()
    {
        Visible = false;

        //TESTING. please keep this commented when doing release version
        // if (GetParent() == GetTree().Root && ProjectSettings.GetSetting("application/run/main_scene") != Filename)
        // {
        //     Test();
        // }
    }

    private void Test()
    {
        UnitData unitData = new UnitData();
        unitData.CurrentBattleUnitData.Combatant = BattleUnit.Combatant.Beetle;
        unitData.CurrentBattleUnitData.Level = 3;
        unitData.PortraitPathSmall = "res://Interface/Icons/PotionPurp.png";
        for (int i = 0; i < 4; i++)
        {
            UnitData minionUnitData = new UnitData();
            minionUnitData.CurrentBattleUnitData = new BattleUnitData();
            minionUnitData.CurrentBattleUnitData.Combatant = unitData.CurrentBattleUnitData.Combatant;
            minionUnitData.CurrentBattleUnitData.Level = unitData.CurrentBattleUnitData.Level - 1;
            minionUnitData.SetAttributesByLevel(new List<UnitData.Attribute>());
            minionUnitData.UpdateDerivedStatsFromAttributes();
            unitData.Minions.Add(minionUnitData.CurrentBattleUnitData);
        }
        unitData.CurrentBattleUnitData.WeaponEquipped = PnlInventory.ItemMode.RustedMace;
        unitData.CurrentBattleUnitData.AmuletEquipped = PnlInventory.ItemMode.ScarabAmulet;
        unitData.CurrentBattleUnitData.ArmourEquipped = PnlInventory.ItemMode.ObsidianPlate;
        unitData.CurrentBattleUnitData.PotionsEquipped = new PnlInventory.ItemMode[3] {PnlInventory.ItemMode.CharismaPot, PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.ResiliencePot};
        unitData.Hostile = true;
        Activate(unitData);
    }

    public void Activate(UnitData unitData)
    {
        RectGlobalPosition = new Vector2(Mathf.Clamp(GetGlobalMousePosition().x, 0, GetViewportRect().Size.x - RectSize.x), 
            Mathf.Clamp(GetGlobalMousePosition().y, 0, GetViewportRect().Size.y - RectSize.y));
        // int value = GetValueFromDb();
        // var enumDisplayStatus = (EnumDisplayStatus)value;
        // string stringValue = enumDisplayStatus.ToString();
        List<string> equipmentNames = new List<string>();
        equipmentNames.Add(_itemBuilder.BuildAnyItem(unitData.CurrentBattleUnitData.WeaponEquipped).Name);
        equipmentNames.Add(_itemBuilder.BuildAnyItem(unitData.CurrentBattleUnitData.ArmourEquipped).Name);
        equipmentNames.Add(_itemBuilder.BuildAnyItem(unitData.CurrentBattleUnitData.AmuletEquipped).Name);
        foreach (PnlInventory.ItemMode item in unitData.CurrentBattleUnitData.PotionsEquipped)
        {
            equipmentNames.Add(_itemBuilder.BuildAnyItem(item).Name);
        }
        foreach (string s in equipmentNames.ToList())
        {
            if (s == "Empty")
            {
                equipmentNames.Remove(s);
            }
        }
        string equippedString = "Equipped: ";
        foreach (string name in equipmentNames)
        {
            equippedString += name + ", ";
        }
        GetNode<Label>("VBoxLabels/LblEquipment").Text = equipmentNames.Count != 0 ? equippedString.Substring(0, equippedString.Length-2)
            : "Nothing equipped.";

        GetNode<Label>("VBoxLabels/PnlTitle/LblMainCombatant").Text = unitData.Name != "" ? unitData.Name :
            Enum.GetName(typeof(BattleUnit.Combatant), unitData.CurrentBattleUnitData.Combatant);
        GetNode<TextureRect>("VBoxLabels/PnlTitle/TexRectPortrait").Texture = GD.Load<Texture>(unitData.PortraitPathSmall);
        GetNode<Label>("VBoxLabels/LblMinions").Text = unitData.Minions.Count == 0 ? "No minions." : "Minions:";


        GetNode<Label>("VBoxLabels/LblLevel").Text = "Level: " + unitData.CurrentBattleUnitData.Level;

        var combatantNumbers = new Dictionary<BattleUnit.Combatant, int>() {
            {BattleUnit.Combatant.Beetle, 0},
            // {BattleUnit.Combatant.Noob, 0},
            // {BattleUnit.Combatant.Wasp, 0}
        };
        foreach (BattleUnitData battleUnitData in unitData.Minions)
        {
            combatantNumbers[battleUnitData.Combatant] += 1;
        }

        foreach (BattleUnit.Combatant minion in combatantNumbers.Keys)
        {
            if (combatantNumbers[minion] != 0)
            {
                GetNode<Label>("VBoxLabels/LblMinions").Text += GetFormattedCombatants(minion,combatantNumbers[minion]);
            }
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

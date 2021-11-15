using Godot;
using System;

public class PnlPreBattle : Panel
{
    [Signal]
    public delegate void BattleConfirmed();

    public override void _Ready()
    {
        Visible = false;
    }

    public void Start(UnitData unitData)// BattleUnit.Combatant combatant)
    {
        // GD.Print("start");
        Visible = true;
        GetNode<Label>("LblAttackMsg").Text = String.Format(
            "{0} attacks!\n\nPrepare for battle...",
            unitData.Name == "" ? "A " + Enum.GetName(typeof(BattleUnit.Combatant), unitData.CurrentBattleUnitData.Combatant) : unitData.Name);
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public void OnBtnContinuePressed()
    {
        Visible = false;
        EmitSignal(nameof(BattleConfirmed));
    }
}

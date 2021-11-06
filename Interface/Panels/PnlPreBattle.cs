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

    public void Start(BattleUnit.Combatant combatant)
    {
        // GD.Print("start");
        Visible = true;
        GetNode<Label>("LblAttackMsg").Text = String.Format(
            "Look! A {0}!\n\nThey and their minions challenge you! This insult will not go unanswered.\n\nTo battle!",
            Enum.GetName(typeof(BattleUnit.Combatant), combatant));
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

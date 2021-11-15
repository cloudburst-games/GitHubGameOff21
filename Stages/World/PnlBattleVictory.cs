// manage the outcome of battles here

using Godot;
using System;
using System.Collections.Generic;

public class PnlBattleVictory : Panel
{
    [Signal]
    public delegate void RequestedPause(bool pauseEnable);

    private Dictionary<string, Action<Unit>> _defeatNPCOutcomes;
    public override void _Ready()
    {
        Visible = false;
        _defeatNPCOutcomes = new Dictionary<string, Action<Unit>>() {
           // {"enemyjill1", OnEnemyJillDefeated}
        };
    }

    public void Start(Unit npcDefeated)
    {
        EmitSignal(nameof(RequestedPause), true);
        Visible = true;
        if (_defeatNPCOutcomes.ContainsKey(npcDefeated.CurrentUnitData.ID))
        {
            _defeatNPCOutcomes[npcDefeated.CurrentUnitData.ID](npcDefeated);
        }
        else
        {
            OnDefaultEnemyDefeated(npcDefeated);
        }

    }

    public void OnDefaultEnemyDefeated(Unit npcDefeated)
    {
        // delete the NPC from the game
        npcDefeated.CurrentUnitData.Hostile = false;
        npcDefeated.QueueFree();
    }


    public void OnEnemyJillDefeated(Unit npcDefeated)
    {
        // do something special
        // lets try making her our companion
        GetNode<Label>("LblDefeatMessage").Text = "Jill is super impressed by your silly face! She decided to join LOL!";
        npcDefeated.CurrentUnitData.Name = "Reformed Jill";
        npcDefeated.CurrentUnitData.Hostile = false;
        npcDefeated.CurrentUnitData.Companion = true;
        npcDefeated.UpdateFromUnitData();
    }

    public void OnBtnContinuePressed()
    {
        EmitSignal(nameof(RequestedPause), false);
        Visible = false;
    }
}

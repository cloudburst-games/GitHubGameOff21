using Godot;
using System;

public class AITurnHandler
{
    // private bool _roundEnd = false;
    public enum AITurnStateMode { Helper, Aggressive}
    private AITurnState _AITurnState;
    public AITurnStateMode CurrentAITurnStateMode {get; set;} = AITurnStateMode.Aggressive;

    public void SetAITurnState(AITurnStateMode stateMode)
    {
        CurrentAITurnStateMode = stateMode;
        switch (stateMode)
        {
            case AITurnStateMode.Aggressive:
                _AITurnState = new AggressiveAITurnState(this);
                break;
            case AITurnStateMode.Helper:
                _AITurnState = new HelperAITurnState(this);
                break;
        }
    }

    public void OnAITurn(CntBattle cntBattle)
    {
        // if no state, set default
        if (_AITurnState == null)
        {
            SetAITurnState(CurrentAITurnStateMode);
        }

        _AITurnState.DoAITurn(cntBattle);
    }
}

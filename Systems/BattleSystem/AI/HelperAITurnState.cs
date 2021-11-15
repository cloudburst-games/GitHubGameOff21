using Godot;
using System;

public class HelperAITurnState : AITurnState
{
    public HelperAITurnState()
    {
        
    }
    public HelperAITurnState(AITurnHandler aITurnHandler)
    {
        this.AITurnHandler = aITurnHandler;
    }
}

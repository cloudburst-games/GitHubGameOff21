using Godot;
using System;

public class AIBehaviourState : Reference
{
    public AIUnitControlState AIControl {get; set;}

    public virtual void Update(float delta)
    {
        // NPC code here for now?
        // foreach (Godot.Object body in AIControl.Unit.GetNode<Area>("UnitDetectArea").GetOverlappingBodies())
        // {
        //     if (body is Unit unit)
        //     {
        //         if (unit.CurrentUnitData.Player)
        //         {
        //             OnPlayerInRange(unit);
        //             return;
        //         }
        //     }
        // }
    }

    // private void OnPlayerInRange(Unit player)
    // {
    //     if (AIControl.Unit.CurrentUnitData.Hostile)
    //     {
    //         GD.Print("initiate battle");
    //     }
    //     else
    //     {
    //         GD.Print("initiate dialog");
    //     }
    // }

    public virtual void Die()
    {
        
    }
}

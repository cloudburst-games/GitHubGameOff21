using Godot;
using System;

public class AIBehaviourState : Reference
{
    public AIUnitControlState AIControl {get; set;}

    public virtual void Update(float delta)
    {

    }

    public virtual void Die()
    {
        
    }
}

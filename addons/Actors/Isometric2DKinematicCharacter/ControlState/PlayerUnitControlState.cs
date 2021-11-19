using Godot;
using System;

public class PlayerUnitControlState : UnitControlState
{

	public PlayerUnitControlState(Unit unit)
	{
		this.Unit = unit;
	}
    
	public PlayerUnitControlState()
	{
		GD.Print("use constructor with unit argument");
		throw new InvalidOperationException();
	}

    public override void Update(float delta)
    {
        base.Update(delta);

		bool up = Input.IsActionPressed("Move Up");
		bool down =  Input.IsActionPressed("Move Down");
		bool left =  Input.IsActionPressed("Move Left");
		bool right =  Input.IsActionPressed("Move Right");
		

		SetTargetAnimRotation(this.Unit.Position + this.Unit.CurrentVelocity);
		this.Unit.CurrentVelocity = new Vector2( left ? -1 : (right ? 1 : 0), up ? -1 : (down ? 1 : 0)).Normalized();
		this.Unit.CurrentVelocity *= this.Unit.Speed;
		this.Unit.AnimRotation = TargetAnimRotation;

        // Battle / npc starting dialogue
        foreach (Node n in Unit.GetNode<Area2D>("NPCInteractArea").GetOverlappingBodies())
        {
            if (n is Unit unit)
            {
                if (!unit.CurrentUnitData.Active)
                {
                    continue;
                }
                if (!unit.CurrentUnitData.Companion && !unit.CurrentUnitData.Player && unit.CurrentUnitData.Hostile)
                {
                    this.Unit.EmitSignal(nameof(Unit.BattleStarted), unit, unit.CurrentUnitData.CustomBattleText);
                    return;
                }
                else if (!unit.CurrentUnitData.Companion && !unit.CurrentUnitData.Player && unit.CurrentUnitData.InitiatesDialogue)
                {
                    this.Unit.EmitSignal(nameof(Unit.NPCStartingDialogue), unit);
                    return;
                }
            }
        }
        // Interaction
        if (Input.IsActionJustPressed("Interact"))
        {
            foreach (Node n in Unit.GetNode<Area2D>("NPCInteractArea").GetOverlappingBodies())
            {

                if (n is Unit unit)
                {
                    if (!unit.CurrentUnitData.Active)
                    {
                        continue;
                    }
                    if (!unit.CurrentUnitData.Companion && !unit.CurrentUnitData.Player && !unit.CurrentUnitData.Hostile)
                    {
                        this.Unit.EmitSignal(nameof(Unit.DialogueStarted), unit);
                        return;
                    }
                }
            }
            foreach (Node n in Unit.GetNode<Area2D>("NPCInteractArea").GetOverlappingAreas())
            {
                if (n is ShopInteractableArea shopEntranceArea)
                {
                    // shopEntranceArea.CurrentShop.Start(Unit.CurrentUnitData);
                    this.Unit.EmitSignal(nameof(Unit.ShopAccessed), new ShopDataSignalWrapper() {CurrentShopData = shopEntranceArea.CurrentShop.CurrentShopData});
                    return;
                    
                }
            }
            // foreach (Node n in Unit.))
        }
    }
}

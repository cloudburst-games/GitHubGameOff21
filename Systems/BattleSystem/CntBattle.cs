using Godot;
using System;
using System.Collections.Generic;

public class CntBattle : Control
{
    [Signal]
    public delegate void BattleEnded();
    private BattleUnitData _playerData;
    private BattleUnitData _enemyCommanderData;
    private List<BattleUnitData> _friendliesData;
    private List<BattleUnitData> _hostilesData;
    public override void _Ready()
    {
        Visible = false;
    }

    public void Start(BattleUnitData playerData, BattleUnitData enemyCommanderData, List<BattleUnitData> friendliesData, List<BattleUnitData> hostilesData)
    {
        Visible = true;
        // Save references locally
        _playerData = playerData;
        _enemyCommanderData = enemyCommanderData;
        _friendliesData = friendliesData;
        _hostilesData = hostilesData;

        // Generate playing board

        // Generate each board piece from data and place on either side

        // Initiate turns by initiative

        // Battle loop

        // If all friendlies die, lose

        // If all enemies die, win and resurrect all if dead (box to say by divine will you and your allies are revitalised)

        // Calculate and show experience and level changes
        
        // emit signal battle over and back to adventure map, 
        // passing playerData and friendliesData to update battle unit data (do i need to..? cant i update from here)

        // player can adjust and see battleunit stats from adventure map
    }
    
    public void OnBtnEndTestPressed()
    {
        OnBattleEnded();
    }
    public void OnBattleEnded()
    {
        GD.Print("battle ended signal");
        EmitSignal(nameof(BattleEnded));
    }
}

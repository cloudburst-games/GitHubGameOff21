// manage the outcome of battles here

using Godot;
using System;
using System.Collections.Generic;

public class PnlBattleVictory : Panel
{
    [Signal]
    public delegate void RequestedPause(bool pauseEnable);

    [Signal]
    public delegate void TestCompanionJoining(UnitDataSignalWrapper unitDataSignalWrapper);
    [Signal]
    public delegate void ExperienceGained(UnitDataSignalWrapper unitDataSignalWrapper);
    [Signal]
    public delegate void FoundGold(int goldAmount);

    private Dictionary<string, Action<Unit>> _defeatNPCOutcomes;
    public override void _Ready()
    {
        Visible = false;
        _defeatNPCOutcomes = new Dictionary<string, Action<Unit>>() {
           {"enemyjill1", OnEnemyJillDefeated},
           {"NPC0", OnEnemyJillDefeated},
           {"NPC1", OnEnemyJillDefeated}
        };
    }

    public void Start(Unit npcDefeated, UnitData playerData, List<UnitData> companionDatas)
    {
        EmitSignal(nameof(RequestedPause), true);
        Visible = true;

        float xpPerMember = GetExperienceRewardPerMember(npcDefeated, playerData);
        int goldReward = 5; // TODO - set starting gold for each npc, and randomise based on this for each minion. then access this after battle to distribute.
        EmitSignal(nameof(FoundGold), goldReward);

        string rewardMessage = String.Format("Each party member gains {0} experience!\nYou find {1} gold!{2}",
            xpPerMember.ToString(), goldReward.ToString(), CanOneMemberLevelUp(playerData, companionDatas, xpPerMember) ? "\n\nOne of your party members has gained a level!" : "");
        
        DoExperienceOutcome(xpPerMember, playerData, companionDatas);

        
        GetNode<Label>("LblXPGoldMessage").Text = rewardMessage;
        
        if (_defeatNPCOutcomes.ContainsKey(npcDefeated.CurrentUnitData.ID))
        {
            _defeatNPCOutcomes[npcDefeated.CurrentUnitData.ID](npcDefeated);
        }
        else
        {
            OnDefaultEnemyDefeated(npcDefeated);
        }
    }

    private bool CanOneMemberLevelUp(UnitData playerData, List<UnitData> companionDatas, float xpPerMember)
    {
        if (playerData.ExperienceManager.CanLevelUp(playerData.CurrentBattleUnitData.Level, playerData.CurrentBattleUnitData.Experience + xpPerMember))
        {
            return true;
        }
        foreach (UnitData unitData in companionDatas)
        {
            if (unitData.ExperienceManager.CanLevelUp(unitData.CurrentBattleUnitData.Level, unitData.CurrentBattleUnitData.Experience + xpPerMember))
            {
                return true;
            }
        }
        return false;
    }

    private void DoExperienceOutcome(float xpPerMember, UnitData playerData, List<UnitData> companionDatas)
    {
        playerData.CurrentBattleUnitData.Experience += xpPerMember;
        // if (playerData.ExperienceManager.CanLevelUp(playerData.CurrentBattleUnitData.Level, playerData.CurrentBattleUnitData.Experience))
        // {
            EmitSignal(nameof(ExperienceGained), new UnitDataSignalWrapper() {CurrentUnitData = playerData});
        // }
        foreach (UnitData unitData in companionDatas)
        {
            unitData.CurrentBattleUnitData.Experience += xpPerMember;
            // if (unitData.ExperienceManager.CanLevelUp(unitData.CurrentBattleUnitData.Level, unitData.CurrentBattleUnitData.Experience))
            // {
                EmitSignal(nameof(ExperienceGained), new UnitDataSignalWrapper() {CurrentUnitData = unitData}); // should lead to an alert that x levelled up
            // }
        }

    }

    private void OnDefaultEnemyDefeated(Unit npcDefeated)
    {
        // delete the NPC from the game
        npcDefeated.CurrentUnitData.Hostile = false;
        npcDefeated.QueueFree();
    }

    private float GetExperienceRewardPerMember(Unit npcDefeated, UnitData playerData)
    {
        float experienceValueOfDefeated = 0;
        foreach (BattleUnitData battleUnitData in npcDefeated.CurrentUnitData.Minions)
        {
            experienceValueOfDefeated += battleUnitData.Experience;
        }
        experienceValueOfDefeated += npcDefeated.CurrentUnitData.CurrentBattleUnitData.Experience;
        return playerData.ExperienceManager.GetTotalExperienceFromVictory(experienceValueOfDefeated, npcDefeated.CurrentUnitData.Minions.Count + 1, playerData.Minions.Count) / (1 + playerData.Minions.Count);
    }


    private void OnEnemyJillDefeated(Unit npcDefeated)
    {
        // do something special
        // lets try making her our companion
        npcDefeated.CurrentUnitData.Hostile = false;
        GetNode<Label>("LblDefeatMessage").Text = "Jill is super impressed by your silly face! She decided to join LOL!";
        EmitSignal(nameof(TestCompanionJoining), new UnitDataSignalWrapper() {CurrentUnitData = npcDefeated.CurrentUnitData} );
        // npcDefeated.CurrentUnitData.Name = "Reformed Jill";
        // npcDefeated.CurrentUnitData.Hostile = false;
        // npcDefeated.CurrentUnitData.Companion = true;
        // npcDefeated.UpdateFromUnitData();
    }

    private void OnBtnContinuePressed()
    {
        EmitSignal(nameof(RequestedPause), false);
        Visible = false;
    }
}

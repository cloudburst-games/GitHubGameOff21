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
    public delegate void ExperienceGained(UnitDataSignalWrapper unitDataSignalWrapper, int xpPerMember);
    [Signal]
    public delegate void FoundGold(int goldAmount);
    [Signal]
    public delegate void FoundItems(Godot.Collections.Array<PnlInventory.ItemMode> items);

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

        int goldReward = npcDefeated.CurrentUnitData.Gold;
        EmitSignal(nameof(FoundGold), goldReward);
        
        Godot.Collections.Array<PnlInventory.ItemMode> convertedItems = new Godot.Collections.Array<PnlInventory.ItemMode>();
        foreach (PnlInventory.ItemMode item in npcDefeated.CurrentUnitData.CurrentBattleUnitData.ItemsHeld)
        {
            convertedItems.Add(item);
            
        }
        EmitSignal(nameof(FoundItems), convertedItems);

        string rewardMessage = String.Format("Each party member gains {0} experience!\nYou find {1} gold!{3}{2}",
            xpPerMember.ToString(), goldReward.ToString(), CanOneMemberLevelUp(playerData, companionDatas, xpPerMember) ? "\n\nOne of your party members has gained a level!" : "",
            npcDefeated.CurrentUnitData.CurrentBattleUnitData.ItemsHeld.Count > 0 ?"\n\nYou find treasure!" : "");
        
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
            EmitSignal(nameof(ExperienceGained), new UnitDataSignalWrapper() {CurrentUnitData = playerData}, xpPerMember);
        // }
        foreach (UnitData unitData in companionDatas)
        {
            unitData.CurrentBattleUnitData.Experience += xpPerMember;
            // if (unitData.ExperienceManager.CanLevelUp(unitData.CurrentBattleUnitData.Level, unitData.CurrentBattleUnitData.Experience))
            // {
                EmitSignal(nameof(ExperienceGained), new UnitDataSignalWrapper() {CurrentUnitData = unitData}, xpPerMember); // should lead to an alert that x levelled up
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
        return playerData.ExperienceManager.GetTotalExperienceFromVictory(experienceValueOfDefeated, npcDefeated.CurrentUnitData.Minions.Count + 1, playerData.Companions.Count);
    }


    private void OnEnemyJillDefeated(Unit npcDefeated)
    {
        // do something special
        // lets try just making her neutral
        npcDefeated.CurrentUnitData.Hostile = false;

        // do some custom text
        GetNode<Label>("LblDefeatMessage").Text = "blah blah u win ok!";

        // can make companion if we wish, but i dont think we will be using this to add companions
        // EmitSignal(nameof(TestCompanionJoining), new UnitDataSignalWrapper() {CurrentUnitData = npcDefeated.CurrentUnitData} );

        // can also give custom item reward after battles
        EmitSignal(nameof(FoundItems), new Godot.Collections.Array<PnlInventory.ItemMode>() {
            PnlInventory.ItemMode.CharismaPot, PnlInventory.ItemMode.VigourPot
        });

        // and custom gold
        // EmitSignal(nameof(FoundGold), 34);
    }

    // private List<IInventoryPlaceableSignalWrapper> GetItemsFromEnemy(UnitData defeatedNPCUnitData)
    // {
    //     List<IInventoryPlaceableSignalWrapper> wrappedItems = new List<IInventoryPlaceableSignalWrapper>();

    //     //dummy list
    //     List<IInventoryPlaceable> items = new List<IInventoryPlaceable>() {
    //         new VigourPotionEffect(),
    //         new LuckPotionEffect(),
    //         new SwiftnessPotionEffect()
    //     }; //defeatedNPCUnitData.ItemsHeld;
    //     //

    //     foreach (IInventoryPlaceable item in items)
    //     {
    //         wrappedItems.Add(new IInventoryPlaceableSignalWrapper() {CurrentIInventoryPlaceable = item});
    //     }
    //     return wrappedItems;
    // }

    private void OnBtnContinuePressed()
    {
        EmitSignal(nameof(RequestedPause), false);
        Visible = false;
    }
}

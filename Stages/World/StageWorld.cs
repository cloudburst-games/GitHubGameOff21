using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class StageWorld : Stage
{
    private ItemBuilder _itemBuilder = new ItemBuilder();
    private int _difficulty = 1;
    private bool _transitioningLevel = false;


    
    public override void _Input(InputEvent ev)
    {
        base._Input(ev);
        if (GetNode<Panel>("HUD/CtrlTheme/PnlMenu").Visible)
        {
            return;
        }
        // wait for lvl to be loaded first
        if (GetNode<LevelManager>("LevelManager").GetChildCount() == 0)
        {
            return;
        }
        if (GetNode<LevelManager>("LevelManager").GetChildCount() > 0)
        {
            if (GetNode<LevelManager>("LevelManager").GetChild(0).Name == "OldLevel")
            {
                return;
            }
        }
        //
        if (GetNode<HUD>("HUD").IsAnyWindowVisible() || GetNode<PopupMenu>("HUD/CtrlTheme/PnlUIBar/HBoxPortraits/PopupMenu").Visible)
        {
            GetNode<LevelManager>("LevelManager").GetPlayerInTree().SetProcessInput(false);
        }
        
        if (ev.IsActionPressed("Party 1") && !ev.IsEcho())
        {
            OnCharacterManagerPortraitPressed(GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.ID);
        }
        else if (ev.IsActionPressed("Party 2") && !ev.IsEcho())
        {
            if (GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Companions.Count >= 1)
            {
                OnCharacterManagerPortraitPressed(GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Companions[0].ID);
            }
        }
        else if (ev.IsActionPressed("Party 3") && !ev.IsEcho())
        {
            if (GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Companions.Count >= 2)
            {
                OnCharacterManagerPortraitPressed(GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Companions[1].ID);
            }
        }
        // else if (ev.IsActionPressed("Event Log") && !ev.IsEcho())
        // {
        //     GetNode<HUD>("HUD").OnBtnEventsPressed();
        // }
        else if (ev.IsActionPressed("Inventory") && !ev.IsEcho())
        {
            OnPopupMenuIDPressed(1, GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.ID);
        }


        // if (ev.IsActionPressed("Battle Anim Speed 1"))
        // {
        //     OnCompanionJoining(new UnitDataSignalWrapper() {CurrentUnitData = GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNode<Unit>("Child0").CurrentUnitData});

        //     // GetNode<Label>("HUD/CtrlTheme/Blah/Label2").Text = "I HAVE NOW SET BLAH FROM FALSE TO TRUE";
        //     // GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.CurrentDialogueData.Blah = true;
        // }

        if (ev is InputEventMouseMotion)
        {
            foreach (Unit npc in GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetActiveNonCompanionNPCs())
            {
                float distance = ((CircleShape2D)npc.GetNode<CollisionShape2D>("NPCInteractArea/Shape").Shape).Radius;
                if (npc.GetGlobalMousePosition().DistanceTo(npc.GlobalPosition) < distance)
                {
                    npc.SetHighlight(true);
                }
                else
                {
                    npc.SetHighlight(false);
                }
            }            
            
            foreach (Shop shop in GetNode<LevelManager>("LevelManager").GetLevelInTree().GetNode<YSort>("All/Shops").GetChildren())
            {
                float distance = ((CircleShape2D)shop.GetNode<CollisionShape2D>("InteractableArea/Shape").Shape).Radius;
                if (shop.GetGlobalMousePosition().DistanceTo(shop.GetNode<Area2D>("InteractableArea").GlobalPosition) < distance)
                {
                    shop.SetHighlight(true);
                }
                else
                {
                    shop.SetHighlight(false);
                }
            }
        }
        // if (ev.IsActionPressed("Interact"))
        // {
        //     GetNode<Label>("HUD/CtrlTheme/Blah/Label").Text = "status of blah: " + GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.CurrentDialogueData.Blah;
        // }


        
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if (GetNode<Map>("HUD/CtrlTheme/Map").Visible)
        {
            GetNode<Map>("HUD/CtrlTheme/Map").Update(GetNode<LevelManager>("LevelManager").GetPlayerInTree().Position,
                GetNode<LevelManager>("LevelManager").GetPlayerInTree().DirectionAnim,
                GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNPCPositions(),
                GetNode<LevelManager>("LevelManager").GetShops(),
                GetNode<LevelManager>("LevelManager").GetObstacles(),
                GetNode<LevelManager>("LevelManager").GetLevelTransitionPositions());
            
        }

        
    }

    public override void _Ready()
    {
        base._Ready();
        _difficulty = GetNode<OptionButton>("HUD/CtrlTheme/CanvasLayer/PnlSettings/CntPanels/PnlGame/HBoxContainer/BtnDifficulty").Selected;
        GetNode<PnlSettings>("HUD/CtrlTheme/CanvasLayer/PnlSettings").Visible = false;
        ConnectSignals();
        // GetNode<Control>("CntBattle").Visible = false;
        if (SharedData != null)
        {
            if (SharedData.ContainsKey("Load"))
            {
                if ((bool)SharedData["Load"] == true)
                {
                    DataBinary dataBinary = ((DataBinary)SharedData["Data"]);
                    Dictionary<string, IStoreable> unpackedData = UnpackDataOnLoad(dataBinary);
                    LoadWorldGen(unpackedData);
                    return;
                }
            }
            else if (SharedData.ContainsKey("Victory"))
            {
                DataBinary dataBinary = ((DataBinary)SharedData["Data"]);
                Dictionary<string, IStoreable> unpackedData = UnpackDataOnLoad(dataBinary);
                BattleUnitData enemyCommanderData = (BattleUnitData) SharedData["EnemyCommanderData"];
                bool victory = (bool) SharedData["Victory"];
                GetNode<RichTextLabel>("HUD/CtrlTheme/PnlEventsBig/RichTextLabel").Text = (string) SharedData["Events"];
                GetNode<HUD>("HUD").OnMainQuestChanged((string) SharedData["MainQuestText"]);
                LoadWorldGen(unpackedData, true, enemyCommanderData, victory);

                return;
            }
        }

        NewWorldGen();
    }

    public void OnPnlSettingsFinalClosed()
    {
        _difficulty = GetNode<OptionButton>("HUD/CtrlTheme/CanvasLayer/PnlSettings/CntPanels/PnlGame/HBoxContainer/BtnDifficulty").Selected;
    }

    public void ConnectSignals()
    {
        GetNode<PnlSettings>("HUD/CtrlTheme/CanvasLayer/PnlSettings").Connect(nameof(PnlSettings.FinalClosed), this, nameof(OnPnlSettingsFinalClosed));
        GetNode<LevelManager>("LevelManager").Connect(nameof(LevelManager.AutosaveAreaEntered), this, nameof(OnAutosaveTriggered));
        GetNode<LevelManager>("LevelManager").Connect(nameof(LevelManager.Announced), GetNode<HUD>("HUD"), nameof(HUD.LogEntry));
        GetNode<LevelManager>("LevelManager").Connect(nameof(LevelManager.LevelGenerated), this, nameof(OnLevelGenerated));
        GetNode<LevelManager>("LevelManager").Connect(nameof(LevelManager.StartedTransition), this, nameof(OnStartedLevelTransition));
        GetNode<LevelManager>("LevelManager").Connect(nameof(LevelManager.CompletedTransition), this, nameof(OnCompletedLevelTransition));
        GetNode<LevelManager>("LevelManager").Connect(nameof(LevelManager.NPCRightClicked), GetNode<HUD>("HUD"), nameof(HUD.OnNPCRightClicked));
        // // GetNode<LevelManager>("LevelManager").Connect(nameof(LevelManager.NPCGenerated), this, nameof(OnCompanionChanged));
        // GetNode<CntBattle>("HUD/CtrlTheme/CntBattle").Connect(nameof(CntBattle.BattleEnded), this, nameof(OnBattleEnded));
        GetNode<HUD>("HUD").Connect(nameof(HUD.RequestingMap), this, nameof(OnBtnMapPressed));
        GetNode<HUD>("HUD").Connect(nameof(HUD.RequestingJournal), this, nameof(OnBtnJournalPressed));
        // GetNode<CntBattle>("HUD/CtrlTheme/CntBattle").GetNode<Button>("Panel/BattleHUD/CtrlTheme/PnlMenu/VBox/BtnSettings").Connect("pressed", this, nameof(OnBtnSettingsPressed));
        GetNode<PnlBattleVictory>("HUD/CtrlTheme/PnlBattleVictory").Connect(nameof(PnlBattleVictory.RequestedPause), this, nameof(OnPauseRequested));
        GetNode<PnlCharacterManager>("HUD/CtrlTheme/PnlCharacterManager").Connect(nameof(PnlCharacterManager.RequestedPause), this, nameof(OnPauseRequested));
        GetNode<HBoxPortraits>("HUD/CtrlTheme/PnlUIBar/HBoxPortraits").Connect(nameof(HBoxPortraits.PopupPressed), this, nameof(OnPopupMenuIDPressed));
        GetNode<HBoxPortraits>("HUD/CtrlTheme/PnlCharacterManager/HBoxPortraits").Connect(nameof(HBoxPortraits.PortraitPressed), this, nameof(OnCharacterManagerPortraitPressed));
        GetNode<CharacterInventory>("HUD/CtrlTheme/PnlCharacterManager/TabContainer/Inventory").Connect(nameof(CharacterInventory.GivingItemToAnotherCharacter), this, nameof(OnGivingItemToAnotherCharacter));
        GetNode<PnlBattleVictory>("HUD/CtrlTheme/PnlBattleVictory").Connect(nameof(PnlBattleVictory.TestCompanionJoining), this, nameof(OnCompanionJoining));
        GetNode<PnlBattleVictory>("HUD/CtrlTheme/PnlBattleVictory").Connect(nameof(PnlBattleVictory.ExperienceGained), this, nameof(OnExperienceGainedWrapped));
        GetNode<PnlBattleVictory>("HUD/CtrlTheme/PnlBattleVictory").Connect(nameof(PnlBattleVictory.FoundGold), this, nameof(OnFoundGold));
        GetNode<PnlBattleVictory>("HUD/CtrlTheme/PnlBattleVictory").Connect(nameof(PnlBattleVictory.FoundItems), this, nameof(OnFoundItems));
        GetNode<PnlBattleVictory>("HUD/CtrlTheme/PnlBattleVictory").Connect(nameof(PnlBattleVictory.SunStolen), this, nameof(OnSunStolen));
        GetNode<PnlBattleVictory>("HUD/CtrlTheme/PnlBattleVictory").Connect(nameof(PnlBattleVictory.MahefKilled), GetNode<HUD>("HUD"), nameof(HUD.OnGameEnded));
        GetNode<DialogueControl>("HUD/CtrlTheme/DialogueControl").Connect(nameof(DialogueControl.CompanionJoining), this, nameof(OnCompanionJoining));
        GetNode<DialogueControl>("HUD/CtrlTheme/DialogueControl").Connect(nameof(DialogueControl.CompanionLeaving), this, nameof(OnCompanionLeaving));
        GetNode<DialogueControl>("HUD/CtrlTheme/DialogueControl").Connect(nameof(DialogueControl.CompletedQuest), this, nameof(OnCompletedQuest));
        GetNode<DialogueControl>("HUD/CtrlTheme/DialogueControl").Connect(nameof(DialogueControl.NPCUnitDataRequested), this, nameof(OnNPCUnitDataRequested));
        GetNode<DialogueControl>("HUD/CtrlTheme/DialogueControl").Connect(nameof(DialogueControl.CreatedAmbush), this, nameof(OnCreatedAmbush));
        GetNode<PnlCharacterManagementAttributes>("HUD/CtrlTheme/PnlCharacterManager/TabContainer/Character/TabContainer/Attributes").Connect(
            nameof(PnlCharacterManagementAttributes.AllAttributePointsSpent), GetNode<HUD>("HUD"), nameof(HUD.OnSpentAttributePoints)
        );

// GetNode<PnlPreBattle>("HUD/CtrlTheme/PnlPreBattle").Connect(nameof(PnlPreBattle.BattleConfirmed), this, nameof(OnBattleConfirmed));
    }

    public void OnSunStolen()
    {
        GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.DayNightCycle = "Night";
        GetNode<AnimationPlayer>("CanvasLayer/AnimDayNight").Stop();        
        GetNode<AnimationPlayer>("CanvasLayer/AnimDayNight").Play(GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.DayNightCycle);
        GetNode<AnimationPlayer>("CanvasLayer/AnimDayNight").Seek(GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Time, true);
    }
    public void OnStartedLevelTransition()
    {
        SetProcessInput(false);
        _transitioningLevel = true;

    }

    public void OnCompletedLevelTransition()
    {
       GetNode<HUD>("HUD").LogEntry(String.Format("Entered {0}.", GetNode<LevelManager>("LevelManager").GetLevelInTree().LevelName));
       GetNode<Label>("HUD/CtrlTheme/LblShowLevelName").Text = GetNode<LevelManager>("LevelManager").GetLevelInTree().LevelName;
       GetNode<Label>("HUD/CtrlTheme/LblShowLevelName").Visible = true;
       GetNode<AnimationPlayer>("HUD/CtrlTheme/LblShowLevelName/Anim").Play("Start");
       _transitioningLevel = false;
    }

    private void OnBtnMapPressed()
    {
        GetNode<Map>("HUD/CtrlTheme/Map").Show(GetNode<LevelManager>("LevelManager").GetPlayerInTree().Position);
        // GetNode<HUD>("HUD").Pausable = false;

    }

    public void OnLevelGenerated()
    {
        SetProcessInput(true);
        if (GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentControlState is PlayerUnitControlState p)
        {
            p.ClearPath();
        }
       Node2D tilemaps = (Node2D) GetNode<LevelManager>("LevelManager").GetLevelInTree().GetNode("Terrain/Tilemaps").Duplicate();
       GetNode("HUD/CtrlTheme/Map/Panel/ViewportContainer/Viewport/Terrain/Tilemaps").Name = "OldTilemaps";
       GetNode("HUD/CtrlTheme/Map/Panel/ViewportContainer/Viewport/Terrain/OldTilemaps").QueueFree();
       GetNode("HUD/CtrlTheme/Map/Panel/ViewportContainer/Viewport/Terrain").AddChild(tilemaps);
       GetNode("HUD/CtrlTheme/Map/Panel/ViewportContainer/Viewport/Terrain").MoveChild(tilemaps, 0);
       GetNode<Map>("HUD/CtrlTheme/Map").Start();
    }

    public void OnCreatedAmbush(string npcID)
    {
        if (GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNPCFromUnitDataID(npcID) == null)
        {
            GD.Print("tried to create an ambush with an invalid NPC ID and failed. The NPC with this ID must exist and be on the same level as the player.");
            return;
        }
        OnBattleStarted(
            GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNPCFromUnitDataID(npcID),
            GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNPCFromUnitDataID(npcID).CurrentUnitData.CustomBattleText
        );
    }

    public void OnNPCUnitDataRequested(string npcID)
    {
        UnitData unitData;
        // try from current level first:
        if (GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNPCFromUnitDataID(npcID) != null)
        {
            unitData = GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNPCFromUnitDataID(npcID).CurrentUnitData;
        }
        else
        {
            unitData = GetNode<LevelManager>("LevelManager").GetNPCUnitDataByIDFromPackedLevels(npcID);
        }
        if (unitData != null)
        {
            GetNode<DialogueControl>("HUD/CtrlTheme/DialogueControl").OnNPCUnitDataRequestSuccessful(unitData);
        }
        else
        {
            GD.Print("unit data request unsuccessful due to invalid ID. either the ID does not exist or the level has not been visited before.");
        }
    }

    public void OnGivingItemToAnotherCharacter(string id, PnlInventory.ItemMode itemMode)
    {
        GD.Print(id);
        if (id == GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.ID)
        {
            GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.CurrentBattleUnitData.ItemsHeld.Add(itemMode);
            return;
        }
        GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNPCFromUnitDataID(id).CurrentUnitData.CurrentBattleUnitData.ItemsHeld.Add(itemMode);
    }

    public void OnExperienceGainedWrapped(UnitDataSignalWrapper unitDataSignalWrapper, int xpPerMember)
    {        
        if (unitDataSignalWrapper.CurrentUnitData == GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData)
        {
            GetNode<HUD>("HUD").LogEntry(String.Format("Each party member gains {0} experience.", xpPerMember));
        }

        OnExperienceGained(unitDataSignalWrapper.CurrentUnitData);

    }

    public void OnFoundGold(int goldAmount)
    {    
        GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Gold += goldAmount;
        GetNode<HUD>("HUD").LogEntry(String.Format("Found {0} gold.", goldAmount));

    }

    private void OnExperienceGained(UnitData unitData)
    {
        bool levelledup = false;
        bool learnedNewSpell = false;
        if (unitData.ExperienceManager.CanLevelUp(unitData.CurrentBattleUnitData.Level, unitData.CurrentBattleUnitData.Experience))
        {
            levelledup = true;
        }
        
        // GETTING ATTRIBUTE POINTS AND LEVELS BASED ON XP
        while (unitData.ExperienceManager.CanLevelUp(
            unitData.CurrentBattleUnitData.Level, unitData.CurrentBattleUnitData.Experience))
        {
            unitData.CurrentBattleUnitData.Level += 1;
            unitData.AttributePoints += 5 + Convert.ToInt32(Math.Floor(unitData.CurrentBattleUnitData.Level/10f));
        }

        // IF REACH CERTAIN LEVEL, UNLOCK THE 2ND SPELL
        if (unitData.CurrentBattleUnitData.Level >= 3 && 
            unitData.CurrentBattleUnitData.Spell2 == SpellEffectManager.SpellMode.Empty && 
            unitData.CurrentBattleUnitData.SpellGainedAtHigherLevel != SpellEffectManager.SpellMode.Empty)
        {
            unitData.CurrentBattleUnitData.Spell2 = unitData.CurrentBattleUnitData.SpellGainedAtHigherLevel;
            learnedNewSpell = true;
            GetNode<HUD>("HUD").LogEntry(String.Format("{0} has learned {1}.", unitData.Name, "a new spell"));
        }

        // UI FEEDBACK
        if (levelledup)
        {
            LblFloatScore lvlUpFloatLbl = (LblFloatScore) GD.Load<PackedScene>("res://Interface/Labels/FloatScoreLabel/LblFloatScore.tscn").Instance();
            lvlUpFloatLbl.FadeSpeed = 0.1f;
            lvlUpFloatLbl.Text = 
                String.Format("{0} has gained a level{1}",
                    unitData.Name, learnedNewSpell ? " and learned a new spell!" : "!");
            GetNode<HBoxPortraits>("HUD/CtrlTheme/PnlUIBar/HBoxPortraits").SetToFlashIntensely(unitData.ID, lvlUpFloatLbl);
            GetNode<HUD>("HUD").LogEntry(String.Format("{0} gains a new level.", unitData.Name));
            GetNode<HUD>("HUD").OnAttributePointsUnspent();
        }

        // Update stats
        UpdatePlayerBattleCompanions();

    }
    

    private void OnFoundItems(Godot.Collections.Array<PnlInventory.ItemMode> items)
    {
        foreach (PnlInventory.ItemMode item in items)
        {
            GD.Print(Enum.GetName(typeof(PnlInventory.ItemMode), item));
            if (item != PnlInventory.ItemMode.Empty)
            {
                GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.CurrentBattleUnitData.ItemsHeld.Add(item);
                GetNode<HUD>("HUD").LogEntry(String.Format("The party has picked up {0}.", _itemBuilder.BuildAnyItem(item).Name));
            }
        }
    }

    private void OnCompletedQuest(int questDifficulty, Godot.Collections.Array<PnlInventory.ItemMode> itemrewards, int goldReward) // this is the level the quest is targeted for
    {
        float xpReward = 10 + GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.ExperienceManager.GetTotalExperienceValueOfLevel(questDifficulty);
        for (int i = 0; i < GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Companions.Count; i++)
        {
            xpReward *= 1.1f; // party bonus
        }

        // divide by number of companions + player
        xpReward /= GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Companions.Count + 1;

        foreach (UnitData unitData in GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Companions)
        {
            unitData.CurrentBattleUnitData.Experience += xpReward;
            OnExperienceGained(unitData);
        }
        GetNode<HUD>("HUD").LogEntry(String.Format("Quest complete! Each party member gains {0} experience.", xpReward));
        GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.CurrentBattleUnitData.Experience += xpReward;

        if (goldReward > 0)
        {
            OnFoundGold(goldReward);
        }
        if (itemrewards.Count > 0)
        {
            OnFoundItems(itemrewards);
        }

        OnExperienceGained(GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData);
    }

    private void OnBtnJournalPressed()
    {
        GetNode<HUD>("HUD").PauseCommon(true);
        GetNode<HUD>("HUD").Pausable = false;
        GetNode<Journal>("HUD/CtrlTheme/DialogueControl/Journal").ShowJournal(GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData);
        GetNode<DialogueControl>("HUD/CtrlTheme/DialogueControl").Visible = true;
    }

    public void OnCompanionChanged()
    {
        HBoxPortraits[] portraitControls = new HBoxPortraits[3] {GetNode<HBoxPortraits>("HUD/CtrlTheme/PnlShopScreen/HBoxPortraits"), GetNode<HBoxPortraits>("HUD/CtrlTheme/PnlUIBar/HBoxPortraits"), GetNode<HBoxPortraits>("HUD/CtrlTheme/PnlCharacterManager/HBoxPortraits")};
        string playerID = GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.ID;
        
        // GD.Print("TEASTAST 1");
        foreach (HBoxPortraits portraitControl in portraitControls)
        {
            portraitControl.ResetPositions();
            portraitControl.SetSingleUnitBtnByID(0, playerID);
            portraitControl.SetPortrait(
                playerID, 
                GD.Load<Texture>(GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.PortraitPathSmall));
        }

        // GD.Print("TEASTAST 2");
        List<UnitData> unitDatas = GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Companions;
        if (unitDatas.Count == 0 || unitDatas.Count > 2)
        {
            GD.Print("no comanions or 2 many");
            return;
        }

        // GD.Print("TEASTAST 3");
        foreach (HBoxPortraits portraitControl in portraitControls)
        {
            for (int i = 0; i < unitDatas.Count; i++)
            {
                string ID = unitDatas[i].ID;
                portraitControl.SetSingleUnitBtnByID(i+1, ID);
                portraitControl.SetPortrait(ID, GD.Load<Texture>(unitDatas[i].PortraitPathSmall));
            }
        }
        // GD.Print("TEASTAST 4");
    }

    public void OnCompanionJoining(UnitDataSignalWrapper unitDataSignalWrapper)
    {
        // in dialoguecontrol, need to check whether the player has too many companions, and if too many do dialogue option of "too much"
        // otherwise, DialogueControl emits a wrapped signal which is connected to this method, leading to the companion joining us
        if (GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Companions.Count >= 2)
        {
            GD.Print("2 many minions! something went wrong! this companion shouldntbe trying to join! StageWorld.cs OnCompanionJoin");
            return;
        }

        unitDataSignalWrapper.CurrentUnitData.Hostile = false;
        unitDataSignalWrapper.CurrentUnitData.Companion = true;
        GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNPCFromUnitDataID(unitDataSignalWrapper.CurrentUnitData.ID).UpdateFromUnitData();
        GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNPCFromUnitDataID(unitDataSignalWrapper.CurrentUnitData.ID).SetHighlight(false);
        GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Companions.Add(unitDataSignalWrapper.CurrentUnitData);
        GetNode<HUD>("HUD").LogEntry(String.Format("{0} has joined the party.", unitDataSignalWrapper.CurrentUnitData.Name));
        // UpdatePlayerBattleCompanions();
        OnCompanionChanged();
    }

    public void OnCompanionLeaving(UnitDataSignalWrapper unitDataSignalWrapper)
    {
        // in dialoguecontrol, upon selecting the dismiss companion option, dialoguecontrol emits a wrapped signal which is connected to this method, dismissing the companion
        if (!unitDataSignalWrapper.CurrentUnitData.Companion)
        {
            GD.Print("error in StageWorld.cs OnDismissCompanion: is not a companion");
            return;
        }
        unitDataSignalWrapper.CurrentUnitData.StopCompanion();
        GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNPCFromUnitDataID(unitDataSignalWrapper.CurrentUnitData.ID).UpdateFromUnitData();
        GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Companions.Remove(unitDataSignalWrapper.CurrentUnitData);
        GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNPCFromUnitDataID(unitDataSignalWrapper.CurrentUnitData.ID).SetHighlight(true);
        GetNode<HUD>("HUD").LogEntry(String.Format("{0} has left the party.", unitDataSignalWrapper.CurrentUnitData.Name));
        // UpdatePlayerBattleCompanions();
        OnCompanionChanged();
    }

    public void OnCharacterManagerPortraitPressed(string unitID)//int portraitIndex)
    {
        // GD.Print(unitID);
        OnPopupMenuIDPressed(-1, unitID);
    }

    public void OnBtnUnspentPointsPressed()
    {
        if (GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.AttributePoints > 0)
        {
            OnPopupMenuIDPressed(0, GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.ID);
        }
        else
        {
            foreach (UnitData companion in GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Companions)
            {
                if (companion.AttributePoints > 0)
                {
                    OnPopupMenuIDPressed(0, companion.ID);
                    return;
                }
            }
        }
    }

    public void OnPopupMenuIDPressed(int id, string unitID)// int portraitIndex)
    {
        GetNode<LevelManager>("LevelManager").GetPlayerInTree().SetProcessInput(true);
        // 0 sheet // 1 inv // 2  talk // -1 keep the same tab
        Unit unit = unitID == GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.ID
            ? GetNode<LevelManager>("LevelManager").GetPlayerInTree()
            : GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNPCFromUnitDataID(unitID);
        // GD.Print(unitID);
        // GD.Print(unit);
        GetNode<PnlCharacterManager>("HUD/CtrlTheme/PnlCharacterManager").SetGold(GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Gold);

        // if (GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetPlayerCompanions().Count >= portraitIndex)
        // {
            if (id == 0 || id == 1)
            {
                GetNode<HUD>("HUD").PauseCommon(true);
                GetNode<HUD>("HUD").Pausable = false;
                GetNode<PnlCharacterManager>("HUD/CtrlTheme/PnlCharacterManager").Start(unit.CurrentUnitData,
                    // portraitIndex == 0
                    //     ? GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData
                    //     : GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetPlayerCompanions()[portraitIndex - 1].CurrentUnitData,
                    id);
            }
            else if (id == 2)
            {
                // if (portraitIndex > 0)
                // {
                    OnDialogueStarted(unit);
                // }
            }
            else if (id == -1)
            {
                GetNode<HUD>("HUD").PauseCommon(true);
                GetNode<HUD>("HUD").Pausable = false;
                GetNode<PnlCharacterManager>("HUD/CtrlTheme/PnlCharacterManager").Start(unit.CurrentUnitData,
                    // portraitIndex == 0
                    //     ? GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData
                    //     : GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetPlayerCompanions()[portraitIndex - 1].CurrentUnitData,
                GetNode<TabContainer>("HUD/CtrlTheme/PnlCharacterManager/TabContainer").CurrentTab);
            }
        // }
    }

    public void OnBtnSettingsPressed()
    {
        GetNode<PnlSettings>("HUD/CtrlTheme/CanvasLayer/PnlSettings").Visible = true;
    }

    private void OnAutosaveTriggered()
    {
        FileInfo[] files = new DirectoryInfo(ProjectSettings.GlobalizePath("user://Saves"))
            .GetFiles("AUTOSAVE*.ksav")
            .OrderBy(f => f.LastWriteTime)
            .ToArray();

        int count = files.Count();
        if (count >=3)
        {
            OnSaveConfirmed(files[0].ToString());
        }
        else
        {
            OnSaveConfirmed(ProjectSettings.GlobalizePath("user://Saves/AUTOSAVE" + (count+1) + ".ksav"));
        }
    }

    public void OnDialogueStarted(Unit target)
    {
        target.CurrentUnitData.InitiatesDialogue = false;
        GetNode<HUD>("HUD").StartDialogue(target.CurrentUnitData, GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData);
    }
    public async void OnBattleStarted(Unit target, string customBattleText)
    {
        if (_transitioningLevel)
        {
            return;
        }
        if (customBattleText == "")
        {
             customBattleText = "{0} attacks!\n\nPrepare for battle.";
        }
        GetNode<Label>("HUD/CtrlTheme/LblMainQuest").Visible = false;
        GetNode<PnlPreBattle>("HUD/CtrlTheme/PnlPreBattle").Start(target.CurrentUnitData, String.Format(customBattleText, target.CurrentUnitData.Name));
        GetNode<HUD>("HUD").PauseCommon(true);
        GetNode<HUD>("HUD").Pausable = false;
        await ToSignal(GetNode<PnlPreBattle>("HUD/CtrlTheme/PnlPreBattle"), nameof(PnlPreBattle.BattleConfirmed));

        target.CurrentUnitData.UpdateDerivedStatsFromAttributes();
        GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.UpdateDerivedStatsFromAttributes();
        UpdatePlayerBattleCompanions();
        // GD.Print(_difficulty);
        GetNode<HUD>("HUD").LogEntry(String.Format("Commenced battle with {0} and their minions.", target.CurrentUnitData.Name));

        // GetNode<CntBattle>("HUD/CtrlTheme/CntBattle").Start(
        //     playerData:GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.CurrentBattleUnitData,
        //     enemyCommanderData:target.CurrentUnitData.CurrentBattleUnitData,
        //     friendliesData:GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Minions,
        //     hostilesData:target.CurrentUnitData.Minions, _difficulty
        // );

        // save data to load back in
        Dictionary<string, object> saveDict = PackDataPreSave();
        DataBinary dataBinary = new DataBinary();
        dataBinary.Data = saveDict;

        // GD.Print("minions count: ", target.CurrentUnitData.Minions.Count);

        SceneManager.SimpleChangeScene(SceneData.Stage.Battle, new Dictionary<string,object>() {
            {"Data", dataBinary},
            {"PlayerData", GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.CurrentBattleUnitData},
            {"EnemyCommanderData", target.CurrentUnitData.CurrentBattleUnitData},
            {"FriendliesData", GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Minions},
            {"HostilesData", target.CurrentUnitData.Minions},
            {"Difficulty", _difficulty},
            {"Events", GetNode<RichTextLabel>("HUD/CtrlTheme/PnlEventsBig/RichTextLabel").Text},
            {"MainQuestText", GetNode<Label>("HUD/CtrlTheme/LblMainQuest").Text}
        });

    }

    private void UpdatePlayerBattleCompanions()
    {
        GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Minions.Clear();
        GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.UpdateDerivedStatsFromAttributes();
        foreach (UnitData unitData in GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Companions)//  GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetPlayerCompanions())
        {
            unitData.UpdateDerivedStatsFromAttributes();
            GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Minions.Add(unitData.CurrentBattleUnitData);
        }
    }

    // public void OnBattleEnded(bool quitToMainMenu, bool victory, BattleUnitDataSignalWrapper wrappedEnemyCommanderData)
    // {
    //     if (quitToMainMenu)
    //     {
    //         OnBtnExitPressed();
    //         return;
    //     }
    //     GetNode<HUD>("HUD").PauseCommon(false);
    //     GetNode<CntBattle>("HUD/CtrlTheme/CntBattle").Visible = false;

    //     BattleUnitData enemyCommanderData = wrappedEnemyCommanderData.CurrentBattleUnitData;
    //     Unit enemyNPC = GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNPCFromBattleUnitData(enemyCommanderData);

    //     if (victory)
    //     {
    //         OnBattleVictory(enemyNPC);
    //     }
    //     else
    //     {
    //         OnDefeat();
    //     }

    //     // this is a hack.next time we should switch scenes entirely to avoid corruption. or actually maybe this is good...
    //     GetNode<CntBattle>("HUD/CtrlTheme/CntBattle").Name = "CntBattleOld";
    //     GetNode<CntBattle>("HUD/CtrlTheme/CntBattleOld").QueueFree();
    //     CntBattle cntBattleNext = (CntBattle) (GD.Load<PackedScene>("res://Systems/BattleSystem/CntBattle.tscn").Instance());
    //     cntBattleNext.Name = "CntBattle";
    //     GetNode("HUD/CtrlTheme").AddChild(cntBattleNext);
    //     GetNode("HUD/CtrlTheme").MoveChild(cntBattleNext, 5);
    //     cntBattleNext.RectGlobalPosition = new Vector2(0,0);

    //     // foreach (Node n in GetNode<CntBattle>("HUD/CtrlTheme/CntBattle").CurrentBattleGrid.GetNode("All/BattleUnits").GetChildren())
    //     // {
    //     //     n.QueueFree();
    //     // }
    // }

    public void OnPauseRequested(bool pauseEnable)
    {
        GetNode<HUD>("HUD").Pausable = !pauseEnable ? true : GetNode<HUD>("HUD").Pausable;
        GetNode<HUD>("HUD").PauseCommon(pauseEnable);
    }

    public void OnBattleVictory(Unit enemyDefeated)
    {
        // GD.Print("minions count: ", enemyDefeated.CurrentUnitData.Minions.Count);
        // GetNode<PnlBattleVictory>("HUD/CtrlTheme/PnlBattleVictory");
        // do victory stuff
        // GD.Print(enemyDefeated.CurrentUnitData.Name);

        GetNode<Label>("HUD/CtrlTheme/LblMainQuest").Visible = true;
        GetNode<HUD>("HUD").LogEntry(String.Format("Battle with {0} ended in victory.", enemyDefeated.CurrentUnitData.Name));
        GetNode<PnlBattleVictory>("HUD/CtrlTheme/PnlBattleVictory").Start(enemyDefeated,
            GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData,
            GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Companions
            );
    }

    public void OnDefeat()
    {
        GetNode<HUD>("HUD").ShowDefeatMenu();
    }

    // public void OnPortraitBtnPressed(int index)
    // {
    //     GD.Print(index);
    // }

    public void NewWorldGen()
    {
        Unit player = CommonPlayerGen();
        GetNode<LevelManager>("LevelManager").InitialiseLevel( // PLAYER IS ADDED AS CHILD HERE
            LevelManager.Level.Level1,
            player);
        // STARTING PLAYER DATA HERE
        player.CurrentUnitData = new UnitData() {
            Player = true,
            Name = "Khepri",
            ID = "khepri",
            BasePhysicalDamageRange = 1f, // ideally this would change depending on weapon equipped
            Modified = true,
            PortraitPath = "res://Actors/PortraitPlaceholders/Big/Khepri.PNG",
            PortraitPathSmall = "res://Actors/PortraitPlaceholders/Small/Khepri.PNG"
        };
        player.CurrentUnitData.Time = 50; // start at 5am
        player.CurrentUnitData.CurrentBattleUnitData.BattlePortraitPath = player.CurrentUnitData.PortraitPathSmall;
        player.CurrentUnitData.BodyPath = "res://Actors/NPC/Bodies/PlayerBody.tscn"; // todo - change this to PlayerBody when this is done
        player.CurrentUnitData.CurrentBattleUnitData.BodyPath = "res://Actors/NPC/Bodies/PlayerBody.tscn"; // todo - change this to PlayerBody when this is done
        // set starting attributes
        foreach (UnitData.Attribute att in player.CurrentUnitData.Attributes.Keys.ToList())
        {
            player.CurrentUnitData.Attributes[att] = 10;
        }
        // set starting spells
        player.CurrentUnitData.CurrentBattleUnitData.Spell1 = SpellEffectManager.SpellMode.SolarBolt;
        player.CurrentUnitData.CurrentBattleUnitData.Spell2 = SpellEffectManager.SpellMode.Empty;
        player.CurrentUnitData.CurrentBattleUnitData.SpellGainedAtHigherLevel = SpellEffectManager.SpellMode.SolarBlast;

        // set starting equipment
        // player.CurrentUnitData.EquipAmulet(PnlInventory.ItemMode.ScarabAmulet);
        // player.CurrentUnitData.EquipArmour(PnlInventory.ItemMode.RustedArmour);
        // player.CurrentUnitData.EquipWeapon(PnlInventory.ItemMode.RustedMace);
        player.CurrentUnitData.CurrentBattleUnitData.PotionsEquipped = new PnlInventory.ItemMode[3] {
            PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.Empty, PnlInventory.ItemMode.Empty
        };
        // player.CurrentUnitData.CurrentBattleUnitData.ItemsHeld = new List<PnlInventory.ItemMode>() {
        //     PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.ManaPot
        // };
        player.CurrentUnitData.UpdateDerivedStatsFromAttributes();

        
        string controlUp = ((InputEvent)InputMap.GetActionList("Move Up")[0]).AsText();
        string controlDown = ((InputEvent)InputMap.GetActionList("Move Down")[0]).AsText();
        string controlLeft = ((InputEvent)InputMap.GetActionList("Move Left")[0]).AsText();
        string controlRight = ((InputEvent)InputMap.GetActionList("Move Right")[0]).AsText();
        string controlInteract = ((InputEvent)InputMap.GetActionList("Interact")[0]).AsText();

        GetNode<HUD>("HUD").LogEntry(String.Format("New world generated."));
        GetNode<HUD>("HUD").LogEntry(String.Format("Hint: {0}/{1}/{2}/{3} or click to move. {4} or click to interact. Find someone to talk to.", controlUp,controlLeft, controlDown, controlRight, controlInteract));

        GetNode<AnimationPlayer>("CanvasLayer/AnimDayNight").Play(GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.DayNightCycle);
        GetNode<AnimationPlayer>("CanvasLayer/AnimDayNight").Seek(player.CurrentUnitData.Time, true);
        OnCompanionChanged();
    }

    public void OnShopAccessed(ShopDataSignalWrapper wrappedShopData)
    {

        GetNode<HUD>("HUD").PauseCommon(true);
        GetNode<HUD>("HUD").Pausable = false;
        GetNode<PnlShopScreen>("HUD/CtrlTheme/PnlShopScreen").Start(
            GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData,
            GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Companions,
            wrappedShopData.CurrentShopData
        );        
    }


    private Unit CommonPlayerGen()
    {
        Unit player = (Unit) GD.Load<PackedScene>("res://Actors/Player/Player.tscn").Instance();
        // AND STARTING PLAYER DATA HERE
        // player.ID = "khepri"; // may not be needed
        // player.UnitName = "Khepri";
        //
        player.Connect(nameof(Unit.DialogueStarted), this, nameof(OnDialogueStarted));
        player.Connect(nameof(Unit.BattleStarted), this, nameof(OnBattleStarted));
        player.Connect(nameof(Unit.ShopAccessed), this, nameof(OnShopAccessed));
        player.Connect(nameof(Unit.NPCStartingDialogue), this, nameof(OnDialogueStarted));
        player.Connect(nameof(Unit.PlayerPathCleared), GetNode<LevelManager>("LevelManager"), nameof(LevelManager.OnPlayerPathCleared));
        player.Connect(nameof(Unit.PlayerPathSet), GetNode<LevelManager>("LevelManager"), nameof(LevelManager.OnPlayerPathSet));
        

        return player;
    }
    /*

            (IStoreable)dataBinary.Data["LevelManagerData"],
            (IStoreable)dataBinary.Data["PlayerData"]
    */

    public async void LoadWorldGen(Dictionary<string, IStoreable> unpackedData, bool fromBattle = false, BattleUnitData enemyCommanderData = null, bool victory = false)
    {
        GetNode<AnimationPlayer>("CanvasLayer/AnimDayNight").Stop();
        // fade to black or do loading screen
        LoadingScreen loadingScreen = (LoadingScreen) GD.Load<PackedScene>("res://Interface/Transitions/LoadingScreen.tscn").Instance();
        AddChild(loadingScreen);
        loadingScreen.FadeIn();
        await ToSignal(loadingScreen.GetNode("FadeAnim"), "animation_finished");

        // delete current level data
        GetNode<LevelManager>("LevelManager").FreeCurrentLevel();

        // load new level
        GetNode<LevelManager>("LevelManager").UnpackAllData((LevelManagerData) unpackedData["LevelManagerData"]);

        Unit player = CommonPlayerGen();
        player.CurrentUnitData = (UnitData) unpackedData["PlayerData"];
        player.CurrentUnitData.BodyPath = "res://Actors/NPC/Bodies/PlayerBody.tscn"; // todo - change this to PlayerBody when this is done
        player.CurrentUnitData.CurrentBattleUnitData.BodyPath = "res://Actors/NPC/Bodies/PlayerBody.tscn"; // todo - change this to PlayerBody when this is done
        player.BodySwap();
        // when we save player data we will not make a new player rather load player data into this variable?
        GetNode<LevelManager>("LevelManager").InitialiseLevel(
            GetNode<LevelManager>("LevelManager").CurrentLevel,
            player);

        GetNode<DialogueControl>("HUD/CtrlTheme/DialogueControl").Load(null, player.CurrentUnitData);

        GetNode<AnimationPlayer>("CanvasLayer/AnimDayNight").Play(GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.DayNightCycle);
        // GD.Print(GetNode<AnimationPlayer>("CanvasLayer/AnimDayNight").CurrentAnimationPosition);
        // GD.Print("player tyime: ", player.CurrentUnitData.Time);
        GetNode<AnimationPlayer>("CanvasLayer/AnimDayNight").Seek(player.CurrentUnitData.Time, true);
        // GD.Print(GetNode<AnimationPlayer>("CanvasLayer/AnimDayNight").CurrentAnimationPosition);
        OnCompanionChanged();

        // fade out
        loadingScreen.FadeOut();        
        GetNode<HUD>("HUD").TogglePauseMenu(false);
        GetNode<HUD>("HUD").OnMainQuestChanged(player.CurrentUnitData.MainQuest);

        if (!fromBattle)
        {
            GetNode<HUD>("HUD").LogEntry("Game Loaded");
        }
        else
        {
            if (victory)
            {
                Unit enemyNPC = GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNPCFromBattleUnitData(enemyCommanderData);
                OnBattleVictory(enemyNPC);
                if (player.CurrentUnitData.AttributePoints > 0)
                {
                    GetNode<HUD>("HUD").OnAttributePointsUnspent();
                }
            }
            else
            {
                OnDefeat();
            }
        }
    }

    public void CommonWorldGen()
    {

    }

    public void OnFileDialogConfirmed(string path)
    {
        
        if (GetNode<FileDialog>("HUD/CtrlTheme/FileDialog").Mode == FileDialog.ModeEnum.SaveFile)
        {
            OnSaveConfirmed(path);
        }
        else
        {
            OnLoadConfirmed(path);
        }
    }

    private void OnSaveConfirmed(string path)
    {
        // show save animation - need to know how to detect progress of save first
        // GetNode<HUD>("HUD").PlayProgressAnim("Saving...");
        
        // update the time before saving
        GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Time = GetNode<AnimationPlayer>("CanvasLayer/AnimDayNight").CurrentAnimationPosition; // start at 8am
        
        // save current quest objective
        GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.MainQuest = GetNode<Label>("HUD/CtrlTheme/LblMainQuest").Text;
        
        // make the dict in which data will be saved - each object will pack and return data
        Dictionary<string, object> saveDict = PackDataPreSave();

        // save the binary to disk
        DataBinary dataBinary = new DataBinary();
        dataBinary.SaveBinary(saveDict, ProjectSettings.GlobalizePath(path));

        // GetNode<HUD>("HUD").StopProgressAnim();
        GetNode<HUD>("HUD").LogEntry(String.Format("Game {0} Saved", System.IO.Path.GetFileName(path)));
    }

    // ******************
    // **PACK DATA HERE**
    // ******************
    private Dictionary<string, object> PackDataPreSave()
    {
        return new Dictionary<string, object>() 
        {
            {"LevelManagerData", GetNode<LevelManager>("LevelManager").PackAndGetData()},
            {"PlayerData", GetNode<LevelManager>("LevelManager").GetPlayerInTree().PackAndGetData()}
        };
    }

    // ******************
    // **UNPACK DATA HERE**
    // ******************
    private Dictionary<string, IStoreable> UnpackDataOnLoad(DataBinary dataBinary)
    {
        // GetNode<LevelManager>("LevelManager").UnpackAllData((LevelManagerData) dataBinary.Data["LevelManagerData"]);
        return new Dictionary<string, IStoreable>() {
            {"LevelManagerData", (IStoreable)dataBinary.Data["LevelManagerData"]},
            {"PlayerData", (IStoreable)dataBinary.Data["PlayerData"]}
        };
    }
   
    public void OnLoadConfirmed(string path)
    {
        GetNode<Panel>("HUD/CtrlTheme/PnlDefeat").Visible = false;
        DataBinary dataBinary = FileBinary.LoadFromFile(ProjectSettings.GlobalizePath(path));
        Dictionary<string, IStoreable> unpackedData = UnpackDataOnLoad(dataBinary);
        LoadWorldGen(unpackedData);
        GetNode<HUD>("HUD").LogEntry(String.Format("Loading {0}", System.IO.Path.GetFileName(path)));
    }

    public void OnBtnExitPressed()
    {
        GetNode<PnlSettings>("HUD/CtrlTheme/CanvasLayer/PnlSettings").OnDie();
        // ?show a warning re unsaved data
        // GetNode<CntBattle>("HUD/CtrlTheme/CntBattle").Die();
        GetNode<PnlCharacterManager>("HUD/CtrlTheme/PnlCharacterManager").Die();
        GetNode<PnlShopScreen>("HUD/CtrlTheme/PnlShopScreen").Die();
        SceneManager.SimpleChangeScene(SceneData.Stage.MainMenu);
    }
}
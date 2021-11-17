using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class StageWorld : Stage
{
    public override void _Input(InputEvent ev)
    {
        base._Input(ev);

        // if (ev.IsActionPressed("Hide Grid"))
        // {
            // OnDismissCompanion(new UnitDataSignalWrapper(){
            //     CurrentUnitData = GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetPlayerCompanions().Select(x => x.CurrentUnitData).ToList()[0]
            // });
            // GetNode<Label>("HUD/CtrlTheme/Blah/Label2").Text = "I HAVE NOW SET BLAH FROM FALSE TO TRUE";
            // GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.CurrentDialogueData.Blah = true;
        // }
        // if (ev.IsActionPressed("Interact"))
        // {
        //     GetNode<Label>("HUD/CtrlTheme/Blah/Label").Text = "status of blah: " + GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.CurrentDialogueData.Blah;
        // }
        // if (ev.IsActionPressed("Test"))
        // {
        //     GetNode<Panel>("HUD/CtrlTheme/Blah").Visible = !GetNode<Panel>("HUD/CtrlTheme/Blah").Visible;
        // }
    }

    public override void _Ready()
    {
        base._Ready();
        GetNode<CntBattle>("HUD/CtrlTheme/CntBattle").PnlSettings = GetNode<PnlSettings>("HUD/CtrlTheme/CanvasLayer/PnlSettings");
        GetNode<PnlSettings>("HUD/CtrlTheme/CanvasLayer/PnlSettings").Visible = false;
        ConnectSignals();
        // GetNode<Control>("CntBattle").Visible = false;
        
        //testing:
        NewWorldGen();
    }

    public void ConnectSignals()
    {
        GetNode<LevelManager>("LevelManager").Connect(nameof(LevelManager.AutosaveAreaEntered), this, nameof(OnAutosaveTriggered));
        GetNode<LevelManager>("LevelManager").Connect(nameof(LevelManager.Announced), GetNode<HUD>("HUD"), nameof(HUD.LogEntry));
        GetNode<LevelManager>("LevelManager").Connect(nameof(LevelManager.NPCRightClicked), GetNode<HUD>("HUD"), nameof(HUD.OnNPCRightClicked));
        GetNode<LevelManager>("LevelManager").Connect(nameof(LevelManager.NPCGenerated), this, nameof(OnCompanionChanged));
        GetNode<CntBattle>("HUD/CtrlTheme/CntBattle").Connect(nameof(CntBattle.BattleEnded), this, nameof(OnBattleEnded));
        GetNode<CntBattle>("HUD/CtrlTheme/CntBattle").GetNode<Button>("Panel/BattleHUD/CtrlTheme/PnlMenu/VBox/BtnSettings").Connect("pressed", this, nameof(OnBtnSettingsPressed));
        GetNode<PnlBattleVictory>("HUD/CtrlTheme/PnlBattleVictory").Connect(nameof(PnlBattleVictory.RequestedPause), this, nameof(OnPauseRequested));
        GetNode<PnlCharacterManager>("HUD/CtrlTheme/PnlCharacterManager").Connect(nameof(PnlCharacterManager.RequestedPause), this, nameof(OnPauseRequested));
        GetNode<HBoxPortraits>("HUD/CtrlTheme/PnlUIBar/HBoxPortraits").Connect(nameof(HBoxPortraits.PopupPressed), this, nameof(OnPopupMenuIDPressed));
        GetNode<HBoxPortraits>("HUD/CtrlTheme/PnlCharacterManager/HBoxPortraits").Connect(nameof(HBoxPortraits.PortraitPressed), this, nameof(OnCharacterManagerPortraitPressed));
        GetNode<PnlBattleVictory>("HUD/CtrlTheme/PnlBattleVictory").Connect(nameof(PnlBattleVictory.TestCompanionJoining), this, nameof(OnCompanionJoin));
        GetNode<PnlBattleVictory>("HUD/CtrlTheme/PnlBattleVictory").Connect(nameof(PnlBattleVictory.ExperienceGained), this, nameof(OnExperienceGainedWrapped));

// GetNode<PnlPreBattle>("HUD/CtrlTheme/PnlPreBattle").Connect(nameof(PnlPreBattle.BattleConfirmed), this, nameof(OnBattleConfirmed));
    }

    public void OnExperienceGainedWrapped(UnitDataSignalWrapper unitDataSignalWrapper)
    {
        OnExperienceGained(unitDataSignalWrapper.CurrentUnitData);
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
        if (unitData.CurrentBattleUnitData.Level >= 5 && 
            unitData.CurrentBattleUnitData.Spell2 == SpellEffectManager.SpellMode.Empty && 
            unitData.CurrentBattleUnitData.SpellGainedAtHigherLevel != SpellEffectManager.SpellMode.Empty)
        {
            unitData.CurrentBattleUnitData.Spell2 = unitData.CurrentBattleUnitData.SpellGainedAtHigherLevel;
            learnedNewSpell = true;
        }

        // UI FEEDBACK
        if (levelledup)
        {
            LblFloatScore lvlUpFloatLbl = new LblFloatScore();
            lvlUpFloatLbl.FadeSpeed = 0.2f;
            lvlUpFloatLbl.Text = 
                String.Format("{0} has gained a level{1}",
                    unitData.Name, learnedNewSpell ? " and learned a new spell!" : "!");
            GetNode<HBoxPortraits>("HUD/CtrlTheme/PnlUIBar/HBoxPortraits").SetToFlashIntensely(unitData.ID, lvlUpFloatLbl);
        }
    }

    private void OnQuestCompleted(int questDifficulty) // this is the level the quest is targeted for
    {
        float xpReward = questDifficulty*10;
        for (int i = 0; i < GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetPlayerCompanions().Count; i++)
        {
            xpReward *= 1.1f; // party bonus
        }
        foreach (UnitData unitData in GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetPlayerCompanions().Select(x => x.CurrentUnitData).ToList())
        {
            unitData.CurrentBattleUnitData.Experience += xpReward;
            OnExperienceGained(unitData);
        }
        GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.CurrentBattleUnitData.Experience += xpReward;
        OnExperienceGained(GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData);
    }

    public void OnCompanionChanged()
    {
        HBoxPortraits[] portraitControls = new HBoxPortraits[2] {GetNode<HBoxPortraits>("HUD/CtrlTheme/PnlUIBar/HBoxPortraits"), GetNode<HBoxPortraits>("HUD/CtrlTheme/PnlCharacterManager/HBoxPortraits")};
        string playerID = GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.ID;
        
        foreach (HBoxPortraits portraitControl in portraitControls)
        {
            portraitControl.ResetPositions();
            portraitControl.SetSingleUnitBtnByID(0, playerID);
            portraitControl.SetPortrait(
                playerID, 
                GD.Load<Texture>(GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.PortraitPathSmall));
        }

        List<UnitData> unitDatas = GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetPlayerCompanions().Select(x => x.CurrentUnitData).ToList();
        if (unitDatas.Count == 0 || unitDatas.Count > 2)
        {
            GD.Print("no comanions or 2 many");
            return;
        }

        foreach (HBoxPortraits portraitControl in portraitControls)
        {
            for (int i = 0; i < unitDatas.Count; i++)
            {
                string ID = unitDatas[i].ID;
                portraitControl.SetSingleUnitBtnByID(i+1, ID);
                portraitControl.SetPortrait(ID, GD.Load<Texture>(unitDatas[i].PortraitPathSmall));
            }
        }
    }

    public void OnCompanionJoin(UnitDataSignalWrapper unitDataSignalWrapper)
    {
        // in dialoguecontrol, need to check whether the player has too many companions, and if too many do dialogue option of "too much"
        // otherwise, DialogueControl emits a wrapped signal which is connected to this method, leading to the companion joining us
        
        if (GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Minions.Count >= 2)
        {
            GD.Print("2 many minions! something went wrong! this companion shouldntbe trying to join! StageWorld.cs OnCompanionJoin");
            return;
        }

        unitDataSignalWrapper.CurrentUnitData.Hostile = false;
        unitDataSignalWrapper.CurrentUnitData.Companion = true;
        GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNPCFromUnitDataID(unitDataSignalWrapper.CurrentUnitData.ID).UpdateFromUnitData();
        
        UpdatePlayerBattleCompanions();
        OnCompanionChanged();
    }

    public void OnDismissCompanion(UnitDataSignalWrapper unitDataSignalWrapper)
    {
        // in dialoguecontrol, upon selecting the dismiss companion option, dialoguecontrol emits a wrapped signal which is connected to this method, dismissing the companion
        if (!unitDataSignalWrapper.CurrentUnitData.Companion)
        {
            GD.Print("error in StageWorld.cs OnDismissCompanion: is not a companion");
            return;
        }
        unitDataSignalWrapper.CurrentUnitData.StopCompanion();
        GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNPCFromUnitDataID(unitDataSignalWrapper.CurrentUnitData.ID).UpdateFromUnitData();
        UpdatePlayerBattleCompanions();
        OnCompanionChanged();
    }

    public void OnCharacterManagerPortraitPressed(string unitID)//int portraitIndex)
    {
        GD.Print(unitID);
        OnPopupMenuIDPressed(-1, unitID);
    }

    public void OnPopupMenuIDPressed(int id, string unitID)// int portraitIndex)
    {
        // 0 sheet // 1 inv // 2  talk // -1 keep the same tab
        Unit unit = unitID == GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.ID
            ? GetNode<LevelManager>("LevelManager").GetPlayerInTree()
            : GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNPCFromUnitDataID(unitID);
        // GD.Print(unitID);
        // GD.Print(unit);

        // if (GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetPlayerCompanions().Count >= portraitIndex)
        // {
            if (id == 0 || id == 1)
            {
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
        GetNode<HUD>("HUD").StartDialogue(target.CurrentUnitData, GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData);
    }
    public async void OnBattleStarted(Unit target)
    {
        GetNode<PnlPreBattle>("HUD/CtrlTheme/PnlPreBattle").Start(target.CurrentUnitData);
        GetNode<HUD>("HUD").PauseCommon(true);
        GetNode<HUD>("HUD").Pausable = false;
        await ToSignal(GetNode<PnlPreBattle>("HUD/CtrlTheme/PnlPreBattle"), nameof(PnlPreBattle.BattleConfirmed));

        GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.UpdateDerivedStatsFromAttributes();
        UpdatePlayerBattleCompanions();

        GetNode<CntBattle>("HUD/CtrlTheme/CntBattle").Start(
            playerData:GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.CurrentBattleUnitData,
            enemyCommanderData:target.CurrentUnitData.CurrentBattleUnitData,
            friendliesData:GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Minions,
            hostilesData:target.CurrentUnitData.Minions
        );
    }

    private void UpdatePlayerBattleCompanions()
    {
        GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Minions.Clear();
        foreach (Unit unit in GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetPlayerCompanions())
        {
            unit.CurrentUnitData.UpdateDerivedStatsFromAttributes();
            GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Minions.Add(unit.CurrentUnitData.CurrentBattleUnitData);
        }
    }

    public void OnBattleEnded(bool quitToMainMenu, bool victory, BattleUnitDataSignalWrapper wrappedEnemyCommanderData)
    {
        if (quitToMainMenu)
        {
            OnBtnExitPressed();
            return;
        }
        GetNode<HUD>("HUD").PauseCommon(false);
        GetNode<CntBattle>("HUD/CtrlTheme/CntBattle").Visible = false;

        BattleUnitData enemyCommanderData = wrappedEnemyCommanderData.CurrentBattleUnitData;
        Unit enemyNPC = GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetNPCFromBattleUnitData(enemyCommanderData);

        if (victory)
        {
            OnBattleVictory(enemyNPC);
        }
        else
        {
            OnDefeat();
        }
    }

    public void OnPauseRequested(bool pauseEnable)
    {
        GetNode<HUD>("HUD").Pausable = !pauseEnable ? true : GetNode<HUD>("HUD").Pausable;
        GetNode<HUD>("HUD").PauseCommon(pauseEnable);
    }

    public void OnBattleVictory(Unit enemyDefeated)
    {
        // GetNode<PnlBattleVictory>("HUD/CtrlTheme/PnlBattleVictory");
        // do victory stuff
        // GD.Print(enemyDefeated.CurrentUnitData.Name);
        GetNode<PnlBattleVictory>("HUD/CtrlTheme/PnlBattleVictory").Start(enemyDefeated,
            GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData,
            GetNode<LevelManager>("LevelManager").GetNPCManagerInTree().GetPlayerCompanions().Select(x => x.CurrentUnitData).ToList()
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
            PhysicalDamageRange = 1f, // ideally this would change depending on weapon equipped
            Modified = true,
            PortraitPath = "res://Systems/BattleSystem/GridAttackAPPoint.png",
            PortraitPathSmall = "res://Systems/BattleSystem/GridAttackAPPoint.png"
        };
        
        // set starting attributes
        foreach (UnitData.Attribute att in player.CurrentUnitData.Attributes.Keys.ToList())
        {
            player.CurrentUnitData.Attributes[att] = 10;
        }
        player.CurrentUnitData.UpdateDerivedStatsFromAttributes();
        // set starting spells
        player.CurrentUnitData.CurrentBattleUnitData.Spell1 = SpellEffectManager.SpellMode.SolarBolt;
        player.CurrentUnitData.CurrentBattleUnitData.Spell2 = SpellEffectManager.SpellMode.Empty;
        player.CurrentUnitData.CurrentBattleUnitData.SpellGainedAtHigherLevel = SpellEffectManager.SpellMode.SolarBlast;

        OnCompanionChanged();
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
        
        return player;
    }
    /*

            (IStoreable)dataBinary.Data["LevelManagerData"],
            (IStoreable)dataBinary.Data["PlayerData"]
    */

    public async void LoadWorldGen(Dictionary<string, IStoreable> unpackedData)
    {
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
        // when we save player data we will not make a new player rather load player data into this variable?
        GetNode<LevelManager>("LevelManager").InitialiseLevel(
            GetNode<LevelManager>("LevelManager").CurrentLevel,
            player);
        
        OnCompanionChanged();

        // fade out
        loadingScreen.FadeOut();        
        GetNode<HUD>("HUD").TogglePauseMenu(false);
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
        
        // make the dict in which data will be saved - each object will pack and return data
        Dictionary<string, object> saveDict = PackDataPreSave();

        // save the binary to disk
        DataBinary dataBinary = new DataBinary();
        dataBinary.SaveBinary(saveDict, ProjectSettings.GlobalizePath(path));

        // GetNode<HUD>("HUD").StopProgressAnim();
        GetNode<HUD>("HUD").LogEntry("Game Saved");
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
    }

    public void OnBtnExitPressed()
    {
        // ?show a warning re unsaved data
        GetNode<CntBattle>("HUD/CtrlTheme/CntBattle").Die();
        SceneManager.SimpleChangeScene(SceneData.Stage.MainMenu);
    }
}
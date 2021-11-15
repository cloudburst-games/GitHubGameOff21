using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class StageWorld : Stage
{


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
        GetNode<CntBattle>("HUD/CtrlTheme/CntBattle").Connect(nameof(CntBattle.BattleEnded), this, nameof(OnBattleEnded));
        GetNode<CntBattle>("HUD/CtrlTheme/CntBattle").GetNode<Button>("Panel/BattleHUD/CtrlTheme/PnlMenu/VBox/BtnSettings").Connect("pressed", this, nameof(OnBtnSettingsPressed));
        GetNode<PnlBattleVictory>("HUD/CtrlTheme/PnlBattleVictory").Connect(nameof(PnlBattleVictory.RequestedPause), this, nameof(OnPauseRequested));
        GetNode<HBoxPortraits>("HUD/CtrlTheme/PnlUIBar/HBoxPortraits").Connect(nameof(HBoxPortraits.PopupPressed), this, nameof(OnPopupMenuIDPressed));
        // GetNode<PnlPreBattle>("HUD/CtrlTheme/PnlPreBattle").Connect(nameof(PnlPreBattle.BattleConfirmed), this, nameof(OnBattleConfirmed));
    }

    public void OnPopupMenuIDPressed(int id, int portraitIndex)
    {
        
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

        GetNode<LevelManager>("LevelManager").GetPlayerInTree().UpdateDerivedStatsFromAttributes(
                GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData
        );
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
            unit.UpdateDerivedStatsFromAttributes(unit.CurrentUnitData);
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
        GetNode<PnlBattleVictory>("HUD/CtrlTheme/PnlBattleVictory").Start(enemyDefeated);
    }

    public void OnDefeat()
    {
        GetNode<HUD>("HUD").ShowDefeatMenu();
    }

    public void OnPortraitBtnPressed(int index)
    {
        GD.Print(index);
    }

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
            Modified = true
            
        };
        // set starting attributes
        foreach (UnitData.Attribute att in player.CurrentUnitData.Attributes.Keys.ToList())
        {
            player.CurrentUnitData.Attributes[att] = 10;
        }
        player.UpdateDerivedStatsFromAttributes(player.CurrentUnitData);
        // set starting spells
        player.CurrentUnitData.CurrentBattleUnitData.Spell1 = SpellEffectManager.SpellMode.SolarBolt;
        player.CurrentUnitData.CurrentBattleUnitData.Spell2 = SpellEffectManager.SpellMode.Empty;
    }

    private Unit CommonPlayerGen()
    {
        Unit player = (Unit) GD.Load<PackedScene>("res://Actors/Player/Player.tscn").Instance();
        // AND STARTING PLAYER DATA HERE
        player.ID = "khepri";
        player.UnitName = "Khepri";
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
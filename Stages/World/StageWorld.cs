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
        // GetNode<PnlPreBattle>("HUD/CtrlTheme/PnlPreBattle").Connect(nameof(PnlPreBattle.BattleConfirmed), this, nameof(OnBattleConfirmed));
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
        GetNode<HUD>("HUD").StartDialogue(target.CurrentUnitData);
    }
    public async void OnBattleStarted(Unit target)
    {
        GetNode<PnlPreBattle>("HUD/CtrlTheme/PnlPreBattle").Start(target.CurrentUnitData.MainCombatant.Combatant);
        GetNode<HUD>("HUD").PauseCommon(true);

        await ToSignal(GetNode<PnlPreBattle>("HUD/CtrlTheme/PnlPreBattle"), nameof(PnlPreBattle.BattleConfirmed));

        // GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.MainCombatant.PlayerFaction = true;

        GetNode<CntBattle>("HUD/CtrlTheme/CntBattle").Start(
            playerData:GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.MainCombatant,
            enemyCommanderData:target.CurrentUnitData.MainCombatant,
            friendliesData:GetNode<LevelManager>("LevelManager").GetPlayerInTree().CurrentUnitData.Minions,
            hostilesData:target.CurrentUnitData.Minions
        );

        // tmeporary - remove when battle done and enemy dies
        target.CurrentUnitData.Hostile = false;
    }

    public void OnBattleEnded()
    {
        GetNode<HUD>("HUD").PauseCommon(false);
        GetNode<CntBattle>("HUD/CtrlTheme/CntBattle").Visible = false;
    }

    public void NewWorldGen()
    {
        Unit player = CommonPlayerGen();
        player.CurrentUnitData = new UnitData() {
            Player = true
        };
        GetNode<LevelManager>("LevelManager").InitialiseLevel(
            LevelManager.Level.Level1,
            player);
    }

    private Unit CommonPlayerGen()
    {
        Unit player = (Unit) GD.Load<PackedScene>("res://Actors/Player/Player.tscn").Instance();
        player.ID = "khepri";
        player.UnitName = "Khepri";
        // player.SetControlState(Unit.ControlState.Player);
        // if (player.CurrentControlState is PlayerUnitControlState playerUnitControlState)
        // {
        // player.CurrentUnitData.MainCombatant.PlayerFaction = true;
        player.Connect(nameof(Unit.DialogueStarted), this, nameof(OnDialogueStarted));
        player.Connect(nameof(Unit.BattleStarted), this, nameof(OnBattleStarted));
        // }
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
        DataBinary dataBinary = FileBinary.LoadFromFile(ProjectSettings.GlobalizePath(path));
        Dictionary<string, IStoreable> unpackedData = UnpackDataOnLoad(dataBinary);
        LoadWorldGen(unpackedData);
    }

    public void OnBtnExitPressed()
    {
        // ?show a warning re unsaved data
        SceneManager.SimpleChangeScene(SceneData.Stage.MainMenu);
    }
}
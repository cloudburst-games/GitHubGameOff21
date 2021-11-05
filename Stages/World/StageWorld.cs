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
        
        //testing:
        NewWorldGen();
    }

    public void ConnectSignals()
    {
        GetNode<LevelManager>("LevelManager").Connect(nameof(LevelManager.AutosaveAreaEntered), this, nameof(OnAutosaveTriggered));
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

    public void NewWorldGen()
    {
        GetNode<LevelManager>("LevelManager").InitialiseLevel(
            LevelManager.Level.Level1,
            (Unit) GD.Load<PackedScene>("res://Actors/Player/Player.tscn").Instance());
    }

    public async void LoadWorldGen()
    {
        // fade to black or do loading screen
        LoadingScreen loadingScreen = (LoadingScreen) GD.Load<PackedScene>("res://Interface/Transitions/LoadingScreen.tscn").Instance();
        AddChild(loadingScreen);
        loadingScreen.FadeIn();
        await ToSignal(loadingScreen.GetNode("FadeAnim"), "animation_finished");

        // delete current level data
        GetNode<LevelManager>("LevelManager").FreeCurrentLevel();

        // load new level
        // when we save player data we will not make a new player rather load player data into this variable?
        GetNode<LevelManager>("LevelManager").InitialiseLevel(
            GetNode<LevelManager>("LevelManager").CurrentLevel,
            (Unit) GD.Load<PackedScene>("res://Actors/Player/Player.tscn").Instance());

        // fade out
        loadingScreen.FadeOut();        
        GetNode<HUD>("HUD").TogglePause(false);
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
        return new Dictionary<string, object>() {
            {"LevelManagerData", GetNode<LevelManager>("LevelManager").PackAndGetData()},

        };
    }

    // ******************
    // **UNPACK DATA HERE**
    // ******************
    private void UnpackDataOnLoad(DataBinary dataBinary)
    {
        GetNode<LevelManager>("LevelManager").UnpackAllData((LevelManagerData) dataBinary.Data["LevelManagerData"]);
    }
   
    public void OnLoadConfirmed(string path)
    {
        DataBinary dataBinary = FileBinary.LoadFromFile(ProjectSettings.GlobalizePath(path));
        UnpackDataOnLoad(dataBinary);
        LoadWorldGen();
    }

    public void OnBtnExitPressed()
    {
        // ?show a warning re unsaved data
        SceneManager.SimpleChangeScene(SceneData.Stage.MainMenu);
    }
}
using Godot;
using System;
using System.Collections.Generic;

public class StageWorld : Stage
{

    public override void _Ready()
    {
        base._Ready();
        GetNode<Panel>("HUD/PnlMenu").Visible = GetNode<ColorRect>("HUD/PauseRect").Visible = false;
        GetNode<PotionArea>("PlayArea/PotionArea").Connect(nameof(PotionArea.ExplodingPotion), this, nameof(OnExploding));
        GetNode<PotionArea>("PlayArea/PotionArea").Connect(nameof(PotionArea.CorrectPotionOverReceptacle), this, nameof(OnCorrectPotion));
        GetNode<PotionArea>("PlayArea/PotionArea").ReceptacleArea = new Rect2(GetNode<TextureRect>("PlayArea/Buttons/PotionReceptacle").RectGlobalPosition, 
            GetNode<TextureRect>("PlayArea/Buttons/PotionReceptacle").RectSize);
        
    }

    public async void OnExploding()
    {
        PackedScene explosionScn = GD.Load<PackedScene>("res://Explosion/Explosion.tscn");
        Explosion explosion = (Explosion) explosionScn.Instance();
        AddChild(explosion);
        explosion.Start();
        await ToSignal(explosion, nameof(Explosion.FinishedExploding));
        // play explosion
        // wait for explosion to finish then game over and popup box shows game over -> main menu / restart / settings
        // GD.Print("explosion ups");
        GameOver();
    }

    public void OnBtnMenuPressed()
    {
        GetNode<HUD>("HUD").PauseMenu(true);
    }

    public void OnBtnRestartPressed()
    {
        SceneManager.SimpleChangeScene(SceneData.Stage.World);
    }
    public void OnBtnSettingsPressed()
    {
        // SceneManager.SimpleChangeScene(SceneData.Stage.World);
        GetNode<PnlSettings>("HUD/PnlSettings").Visible = true;
		// GetNode<PnlSettings>("HUD/PnlSettings").OnBtnControlsPressed();
    }

    public void OnBtnMainMenuPressed()
    {
        SceneManager.SimpleChangeScene(SceneData.Stage.MainMenu);
    }

    public void GameOver()
    {
        GetNode<HUD>("HUD").PauseMenu(true, gameover:true);
    }

    private Random _rand = new Random();
    private List<Tuple<string, Mixable.MixableType>> _riddlesDone = new List<Tuple<string, Mixable.MixableType>>();
    private Tuple<string, Mixable.MixableType> _currentRiddle;
    // private int _maxNumber = 3;
    // private int _riddleCount = 0;

    private List<Tuple<string, Mixable.MixableType>> _riddlesRemaining = new List<Tuple<string, Mixable.MixableType>>
    {
        Tuple.Create("I want a fruity elixir.\nWhen colours streak the sky,\nThis one lies behind red.\nGet it wrong and you’re dead.", Mixable.MixableType.OrangePot),
        Tuple.Create("To make my potion,\nMelt the sun in the sky,\nI am a bottle of envy,\nBlend me badly and you will die", Mixable.MixableType.GreenPot),
        Tuple.Create("To concoct this potion,\nBlend blood with the ocean,\nMerge misery and hell,\nOr add spores to the bell.", Mixable.MixableType.PurplePot),
        Tuple.Create("Add the colour of rage,\nTo prickles and chimes,\nWhen three brews unite,\nThis potion will be right.", Mixable.MixableType.BrownPot),
        Tuple.Create("Make me a brew,\nThat could be of royal blood,\na musical hue,\nor an unsullied flood", Mixable.MixableType.BluePot),
        Tuple.Create("I want a potion that resembles,\njoy in the sky,\nterror in the belly,\na fever caused by a fly", Mixable.MixableType.YellowPot),
        Tuple.Create("I want an elixir,\nThe colour of a royal carpet,\nBeauty in the eyes of a demon,\nLeaves in the third season", Mixable.MixableType.RedPot)
    };

    public Mixable.MixableType GetAnswer(Tuple<string, Mixable.MixableType> riddle)
    {
        return riddle.Item2;
    }

    public async void OnCorrectPotion()
    {
        GetNode<AnimationPlayer>("PlayArea/Speech/Bubble/Anim").Play("WellDone");
        _riddlesRemaining.Remove(_currentRiddle);
        _riddlesDone.Add(_currentRiddle);
        await ToSignal(GetNode<AnimationPlayer>("PlayArea/Speech/Bubble/Anim"), "animation_finished");
        if (_riddlesRemaining.Count > 0)// < 3)
        {
            StartNextRiddle();
        }
        else
        {
            Victory();
        }
    }

    public async void OnBtnResetPressed()
    {
        GetNode<AnimationPlayer>("PlayArea/AnimStart").Play("RestartFadeOut");
        GetNode<AnimationPlayer>("PlayArea/Speech/Bubble/Anim").Play("EndRiddle");
        await ToSignal(GetNode<AnimationPlayer>("PlayArea/AnimStart"), "animation_finished");
        GetNode<PotionArea>("PlayArea/PotionArea").RemoveAllMixables();
        GetNode<AnimationPlayer>("PlayArea/Speech/Bubble/Anim").Play("NewRiddle");
        GetNode<AnimationPlayer>("PlayArea/AnimStart").Play("RestartFadeIn");
        await ToSignal(GetNode<AnimationPlayer>("PlayArea/AnimStart"), "animation_finished");
        GetNode<PotionArea>("PlayArea/PotionArea").GenerateMixables();
    }

    private void Victory()
    {
        GetNode<HUD>("HUD").PauseMenu(true, victory:true);
    }

    public async void OnBtnStartPressed()
    {
        GetNode<Button>("PlayArea/Buttons/BtnStart").Disabled = true;
        GetNode<AnimationPlayer>("PlayArea/AnimStart").Play("StartGame");

        await ToSignal(GetNode<AnimationPlayer>("PlayArea/AnimStart"), "animation_finished");
        StartNextRiddle();
    }

    private async void StartNextRiddle()
    {
        GetNode<AnimationPlayer>("PlayArea/Speech/Bubble/Anim").Play("EndRiddle");
        await ToSignal(GetNode<AnimationPlayer>("PlayArea/Speech/Bubble/Anim"), "animation_finished");
        SetNewRiddle();
        // _riddleCount += 1;

        GetNode<AnimationPlayer>("PlayArea/Speech/Bubble/Anim").Play("NewRiddle");

        await ToSignal(GetNode<AnimationPlayer>("PlayArea/Speech/Bubble/Anim"), "animation_finished");

        GetNode<PotionArea>("PlayArea/PotionArea").GenerateMixables();
        GetNode<PotionArea>("PlayArea/PotionArea").CurrentWinMixable = _currentRiddle.Item2;
    }

    private void SetNewRiddle()
    {
        if (_riddlesRemaining.Count > 0)
        {
            _currentRiddle = _riddlesRemaining[_rand.Next(_riddlesRemaining.Count)];
            GetNode<Label>("PlayArea/Speech/Bubble/LblRiddle").Text = _currentRiddle.Item1;
        }        
    }


    

}

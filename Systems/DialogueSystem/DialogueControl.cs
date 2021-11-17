using Godot;
using System;
using System.Collections.Generic;

public class DialogueControl : Control
{

    // Just emit this signal when you want to hide the dialog box
    [Signal]
    public delegate void DialogueEnded();

#region Ink code
    public InkStory InkStory;
    string KnotName;
	int DialogueIndex;
	public Dictionary<int, string> DialogueDict = new Dictionary <int, string>();
	public Dictionary<int, string> ChoiceDict = new Dictionary <int, string>();
    public Dictionary<string, PackedScene> QuestDict = new Dictionary<string, PackedScene>();
	public List<string> _journalList = new List<string>();
	public Dictionary <int,string> JournalDict = new Dictionary<int,string>();
	public List<string> QuestList = new List<string>();
    public int n;
	Label MainText;
	int DialogueSize;
	PackedScene scene = GD.Load<PackedScene>("res://Systems/DialogueSystem/ChoiceButton.tscn");
	PackedScene AmuletScene = GD.Load<PackedScene>("res://Systems/DialogueSystem/AmuletLabel.tscn");
	PackedScene CompanionScene = GD.Load<PackedScene>("res://Systems/DialogueSystem/CompanionLabel.tscn");
	PackedScene StolenScene = GD.Load<PackedScene>("res://Systems/DialogueSystem/StolenLabel.tscn");
	PackedScene NightGateScene = GD.Load<PackedScene>("res://Systems/DialogueSystem/NightGateLabel.tscn");
	PackedScene EscapeScene = GD.Load<PackedScene>("res://Systems/DialogueSystem/EscapeLabel.tscn");
    VBoxContainer ButtonContainer;
	public Tween Tween;
     Button ContinueButton;
    TextureRect Portrait;
    Label NameLabel;
	Color StartColour;
	Color EndColour;
	public string Quest = " ";
	public string QuestComplete = " ";
	public string JournalUpdated = " ";
	MarginContainer DialogueContainer;
	Journal Journal;
//	bool _nPC0Spoken;
	bool _nPC1Spoken;
	bool _nPC2Spoken;
	bool _nPC3Spoken;
	bool _nPC4Spoken;
	bool _lookingForAmulet;
	bool _amuletFound;
	bool _stolenSun;
	bool _nightGateConvo;
	bool _escapeConvo;
	string JournalUpdate;
#endregion

#region Dialogue/ink
    public override void _Ready()
    {
        DialogueContainer = GetNode<MarginContainer>("MarginContainer/DialogueContainer");
		Journal = GetNode<Journal>("Journal");
	    LoadInkStory();
		MainText = GetNode<Label>("MarginContainer/DialogueContainer/HBoxContainer/VBoxContainer2/MarginContainer2/MainText");
		ButtonContainer = GetNode<VBoxContainer>("MarginContainer/DialogueContainer/HBoxContainer/VBoxContainer2/MarginContainer3/ScrollContainer/ButtonContainer");
		DialogueContainer.Visible = false;
		GetTree().Paused = false;
		Tween = GetNode<Tween>("Tween");
		//Tween2 = GetNode<Tween>("Tween2");
		ContinueButton = GetNode<Button>("MarginContainer/DialogueContainer/HBoxContainer/VBoxContainer2/MarginContainer4/ContinueButton");
		Portrait = GetNode<TextureRect>("MarginContainer/DialogueContainer/HBoxContainer/VBoxContainer/MarginContainer/Portrait");
		NameLabel = GetNode<Label>("MarginContainer/DialogueContainer/HBoxContainer/VBoxContainer2/MarginContainer/NameLabel");
		//MakeQuestDict();
    }

    public void LoadInkStory()
    {
        InkStory = GetNode<InkStory>("Ink Story"); 
		InkStory.InkFile = GD.Load("res://Systems/DialogueSystem/KhepriStory.json");
		
		InkStory.LoadStory();
//Setting ink variables works here
        //InkStory.InkVariableChanged();
    }

    public void OnDialogueRefSignal(string DialogueReference) //not really necessary - can directly call startDialogue if you want.
	{
		KnotName = DialogueReference;
		GD.Print("KnotNameChanged " + KnotName);
		StartDialogue(KnotName);
	}

    public void StartDialogue(string NameOfKnot)
	{
        //disolve the box in with anim.
		if (InkStory.ChoosePathString(NameOfKnot))
		{
			//Tween.InterpolateProperty(DialogueContainer, "modulate:a", 0, 200, 100, Tween.TransitionType.Linear, Tween.EaseType.InOut);
			//Tween.Start();
			MakeDialogueDict(); //stores the strings so they can be displayed gradually
			//UpdateNamePortrait(NameOfKnot);
		}
	}

    private void MakeDialogueDict()
	{
		DialogueIndex = 0;
		DialogueDict.Clear();
		n = -1;
		while (InkStory.CanContinue)
		{
			n = n+1;
			string text = InkStory.Continue();
            GD.Print(text);
			DialogueDict.Add(n,text);
		}
		ShowText();	
	}

        private void OnContinueButtonPressed() 
	{
		//CheckForJournalUpdates();
		//SaveVariables();
 		if (Tween.IsActive())
		{
			Tween.StopAll();
			//Tween.Stop(MainText, "percent_visible");
			MainText.PercentVisible = 1;
			
		}
		else
		{
			ShowText();
		}

		Save();
		
	}

    private void ShowText()
	{
		ContinueButton.Visible = true;
		DialogueContainer.Visible = true;
		GetTree().Paused=true;
		DialogueSize = DialogueDict.Count;
		if(DialogueIndex < DialogueSize) //uses the dialogue size to show the dialogue line by line
		{
			//SetNameAndPortrait();
			MainText.Text = DialogueDict[DialogueIndex];
		//	TextPanel.BbcodeText = DialogueDisplayDict[DialogueIndex]; 
			MainText.PercentVisible = 0;
			float duration = MainText.Text.Length/10; //30 is a random number to control the reveal speed for different lengths of text. CHange "45" if speed needs to change.
			Tween.InterpolateProperty(MainText,"percent_visible", 0, 1, duration, Tween.TransitionType.Linear, Tween.EaseType.InOut);
			Tween.Start();
			DialogueIndex += 1;
		}
		else
		{
			if (InkStory.HasChoices)
			{
				ContinueButton.Visible = false;
				ChoiceDict.Clear();
				int ChoiceNum;
				ChoiceNum = -1;
				foreach (string choice in InkStory.CurrentChoices)
				{
					ChoiceNum ++;
					ChoiceDict.Add(ChoiceNum, choice);
					var instance = scene.Instance();
					ButtonContainer.AddChild(instance);
					GD.Print("Button Child added");
					Button Button = (Button)ButtonContainer.GetChild(ChoiceNum);
					Label Label = (Label)Button.GetChild(0); //new
					Label.Text = ChoiceDict[ChoiceNum];
				//	Button.Text = ChoiceDict[ChoiceNum];
				//	CheckForJournalUpdates();
				//	SaveVariables();
					Button.Connect("ChoiceButtonPressedSignal", this, nameof(OnChoiceButtonPressed));
				}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
			}
			else
			{
				//instead of quickly removing the visibility, perhaps play an anim here
				//	SaveVariables();
					DialogueContainer.Visible = false;
					GetTree().Paused = false;
                    EmitSignal(nameof(DialogueEnded)); //ENDS DIALOGUE ALTOGETHER
				
				
			}
		}
	}

/* 	public void SaveVariables() //save variables before conversation closes (to do)
	{
		//_nPC0Spoken = (bool)InkStory.GetVariable("npc0_spoken");
		//GD.Print("3. check 4 true - save variables method" + _nPC0Spoken); //this is true
		_nPC1Spoken = (bool)InkStory.GetVariable("npc1_spoken");
		_nPC2Spoken = (bool)InkStory.GetVariable("npc2_spoken");
		_nPC3Spoken = (bool)InkStory.GetVariable("npc3_spoken");
		//_NPC4Spoken = (bool)InkStory.GetVariable("npc0_spoken");
		_lookingForAmulet = (bool)InkStory.GetVariable("looking_for_amulet");
		_amuletFound = (bool)InkStory.GetVariable("amulet_found");
		_stolenSun = (bool)InkStory.GetVariable("stolen_sun");
		_nightGateConvo = (bool)InkStory.GetVariable("night_gate_convo");
		_escapeConvo = (bool)InkStory.GetVariable("escape_convo");
	} */

	public void UpdateInkVariables(string inkyVariable, bool savedVariable)
	{
		InkStory.SetVariable(inkyVariable, savedVariable);
		//GD.Print(inkyVariable + " " + savedVariable); //this is being transmitted as false
	}

     public void OnChoiceButtonPressed(int n)  
	{
		
		GD.Print(n + " Chocie button num dialoge");
		InkStory.ChooseChoiceIndex(n);
		foreach (Button b in ButtonContainer.GetChildren())
			{
				b.QueueFree();
				GD.Print("This method works");
			}
		while (InkStory.CanContinue)
		{
			GD.Print("Inkstory can continue");
			MakeDialogueDict();
			return;
		}
		if ((!InkStory.CanContinue) && (!InkStory.HasChoices))
		{
			//instead of quickly removing the visibility, perhaps play an anim here
			//SaveVariables();
			DialogueContainer.Visible = false;
			GetTree().Paused = false;
            EmitSignal(nameof(DialogueEnded));
		}
	}




/* 	public void MakeQuestDict()
	{
		QuestDict.Add("amulet", AmuletScene);
		QuestDict.Add("companion", CompanionScene);
		QuestDict.Add("stolen", StolenScene);
		QuestDict.Add("night_gate", NightGateScene);
		QuestDict.Add("escape", EscapeScene);
		GD.Print("QuestDictionaryUpdated");
		
	} */

	public void AddSceneToQuestList(PackedScene scene)
	{
		Journal.InstanceQuest(scene);
		GD.Print("added to quest list");
	}

	public void UpdateNameAndPortrait(string name, string portraitpath)
	{
		NameLabel.Text = name;
		Portrait.Texture = (Texture)GD.Load(portraitpath);
	}
//TO DO
/*1. yes just add a variable to the DIalogueData such as public bool TalkAfterBattle and set it to true, and add a string variable such as textafterbattle, this can be checked after the battle and then displayed on the post-battle screen. just remind me to implement it when im finished with my current tasks
2.  yes you can add a variable to the DialogueData class i gave you, such as public string PathToPortrait
2. you can then use this path to load the portrait of whatever npc you are talking to
everything in DialogueData will be saved between levels and on save/load*/

#endregion

    // Started when press E next to non-companion, non-hostile NPC
    // Passes in the interlocutor data (modify variables inside here as needed - will be stored on save and load)
    // within this can access DialogueData. Modify variables as needed in class below.

	public void Load(UnitData npcUnitData, UnitData khepriUnitData)
	{

		UpdateInkVariables("npc0_spoken", khepriUnitData.CurrentDialogueData.NPC0Spoken);
		UpdateInkVariables("npc1_spoken", khepriUnitData.CurrentDialogueData.NPC1Spoken);
		UpdateInkVariables("npc2_spoken", khepriUnitData.CurrentDialogueData.NPC2Spoken);
		UpdateInkVariables("npc3_spoken", khepriUnitData.CurrentDialogueData.NPC3Spoken);
		UpdateInkVariables("npc4_spoken", khepriUnitData.CurrentDialogueData.NPC4Spoken);
		UpdateInkVariables("looking_for_amulet", khepriUnitData.CurrentDialogueData.LookingForAmulet);
		UpdateInkVariables("amulet_found", khepriUnitData.CurrentDialogueData.AmuletFound);
		UpdateInkVariables("stolen_sun", khepriUnitData.CurrentDialogueData.StolenSun);
		UpdateInkVariables("night_gate_convo", khepriUnitData.CurrentDialogueData.NightGateConvo);
		UpdateInkVariables("escape_convo", khepriUnitData.CurrentDialogueData.EscapeConvo);
		JournalUpdate = khepriUnitData.CurrentDialogueData.JournalString;
		// OnDialogueRefSignal(npcUnitData.ID);
	}

	private UnitData _khepriUnitData;

    public void Start(UnitData npcUnitData, UnitData khepriUnitData)
    {
		_khepriUnitData = khepriUnitData;

		// if (khepriUnitData.Modified)
		// {
		Load(npcUnitData, khepriUnitData);




        // // REMEMBER: khepriUnitData will always be there with the player (as it is khepri) and so u can store things to always be accessible there
        // // npcUnitData is the data for the guy khepri is talking to!

        // // you can use KHEPRI'S "charisma" stat if you want to determine whether a dialogue option should be visible
        // int charisma = khepriUnitData.Attributes[UnitData.Attribute.Charisma];
        // if (charisma > 15)
        // {
        //     // show special dialogue option
        // }
        //ADDED
		OnDialogueRefSignal(npcUnitData.ID); //USE THIS WHEN INTEGRATED WITH STAGEWORLD

    }

	// run this and save stuff before exiting dialogue
	public void Save()
	{
		_khepriUnitData.CurrentDialogueData.NPC0Spoken = (bool)InkStory.GetVariable("npc0_spoken");
		_khepriUnitData.CurrentDialogueData.NPC1Spoken = (bool)InkStory.GetVariable("npc1_spoken");
		_khepriUnitData.CurrentDialogueData.NPC2Spoken = (bool)InkStory.GetVariable("npc2_spoken");
		_khepriUnitData.CurrentDialogueData.NPC3Spoken = (bool)InkStory.GetVariable("npc3_spoken");
		_khepriUnitData.CurrentDialogueData.NPC4Spoken = (bool)InkStory.GetVariable("npc4_spoken");
		_khepriUnitData.CurrentDialogueData.LookingForAmulet = (bool)InkStory.GetVariable("looking_for_amulet");
		_khepriUnitData.CurrentDialogueData.AmuletFound = (bool)InkStory.GetVariable("amulet_found");
		_khepriUnitData.CurrentDialogueData.StolenSun = (bool)InkStory.GetVariable("stolen_sun");
		_khepriUnitData.CurrentDialogueData.NightGateConvo = (bool)InkStory.GetVariable("night_gate_convo");
		_khepriUnitData.CurrentDialogueData.EscapeConvo = (bool)InkStory.GetVariable("escape_convo");
		_khepriUnitData.CurrentDialogueData.JournalString = (string)InkStory.GetVariable("journal");
	}

		//NPC4Spoken = (bool)InkStory.GetVariable("npc0_spoken");

  /*   public void OnContinueButtonPressed() //@Sarah: move emit signal to when the dialogue finally ends.
    {
       
    } */

	public override void _Process(float delta)
	{
		//GD.Print(_nPC0Spoken);
	}

}

// Use variables and modify as needed below
// Make any extra dialogue variables needed for dialogue that you want to be saved in here.
// This will be saved whenever the game is saved and will also persist throughout the playthrough and between levels.
[Serializable()]
public class DialogueData : IStoreable
{
    public bool Blah {get; set;} = false;
    public bool AmuletFound {get; set;} = false; 
	public bool NPC1Spoken {get; set;} = false; 
	public bool NPC0Spoken {get; set;} = false; 
	public bool NPC2Spoken {get; set;} = false;
	public bool NPC3Spoken {get; set;} = false;
	public bool NPC4Spoken {get; set;} = false;
	public bool LookingForAmulet {get; set;} = false;
	public bool StolenSun {get; set;} = false;
	public bool NightGateConvo {get; set;} = false;
	public bool EscapeConvo {get; set;} = false;
	public bool Modified {get; set;} = false;
	public string JournalString{get;set;}
	//public List <string> JournalList {get; set;}
}
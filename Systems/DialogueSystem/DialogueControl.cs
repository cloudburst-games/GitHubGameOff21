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
	bool NPC1Spoken = false;
	bool NPC2Spoken = false;
	bool NPC3Spoken = false;
	bool NPC4Spoken = false;
	bool LookingForAmulet = false;
	bool AmuletFound = false;
	bool StolenSun = false;
	bool NightGateConvo = false;
	bool EscapeConvo = false;
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
		MakeQuestDict();
    }

    public void LoadInkStory()
    {
        InkStory = GetNode<InkStory>("Ink Story"); 
		InkStory.InkFile = GD.Load("res://Systems/DialogueSystem/KhepriStory.json");
		InkStory.LoadStory();
		GD.Print("inkstory loaded");
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
		CheckForJournalUpdates();
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
					CheckForJournalUpdates();
					Button.Connect("ChoiceButtonPressedSignal", this, nameof(OnChoiceButtonPressed));
				}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
			}
			else
			{
				//instead of quickly removing the visibility, perhaps play an anim here

					DialogueContainer.Visible = false;
					GetTree().Paused = false;
                    EmitSignal(nameof(DialogueEnded)); //ENDS DIALOGUE ALTOGETHER
				
				
			}
		}
	}

	public void SaveVariables() //save variables before conversation closes (to do)
	{
		Quest = (string)InkStory.GetVariable("quest");
		NPC1Spoken = (bool)InkStory.GetVariable("npc1_spoken");
		NPC2Spoken = (bool)InkStory.GetVariable("npc2_spoken");
		NPC3Spoken = (bool)InkStory.GetVariable("npc3_spoken");
		//NPC4Spoken = (bool)InkStory.GetVariable("npc0_spoken");
		LookingForAmulet = (bool)InkStory.GetVariable("looking_for_amulet");
		AmuletFound = (bool)InkStory.GetVariable("amulet_found");
		StolenSun = (bool)InkStory.GetVariable("stolen_sun");
		NightGateConvo = (bool)InkStory.GetVariable("night_gate_convo");
		EscapeConvo = (bool)InkStory.GetVariable("escape_convo");
	}

	public void UpdateInkVariables()
	{
		//update the ink script with the saved variables (to do)
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
			
			DialogueContainer.Visible = false;
			GetTree().Paused = false;
            EmitSignal(nameof(DialogueEnded));
		}
	}

	public void CheckForJournalUpdates() //Checks for journal text updates and emits signals
	{
		if (Quest != (string)InkStory.GetVariable("quest")) //Check that the journal text variable is not the same as the ink journal text variable because this would have already been transmitted
		{
			Quest = (string)InkStory.GetVariable("quest"); //update the variable to the inkjournaltext variabke
			AddSceneToQuestList(QuestDict[Quest]);
			//make a dictionary that stores the location of the label scene

		}
		if (QuestComplete != (string)InkStory.GetVariable("quest_complete"))
		{
			QuestComplete = (string)InkStory.GetVariable("quest_complete");
			Journal.UpdateCompletedQuests(QuestComplete); //replace this with signal in futre - same for others
		}
		if (JournalUpdated != (string)InkStory.GetVariable("journal_updated"))
		{
			JournalUpdated = (string)InkStory.GetVariable("journal_updated");
			Journal.UpdateJournal(JournalUpdated);
		}
		return;
	}


	public void MakeQuestDict()
	{
		QuestDict.Add("amulet", AmuletScene);
		QuestDict.Add("companion", CompanionScene);
		QuestDict.Add("stolen", StolenScene);
		QuestDict.Add("night_gate", NightGateScene);
		QuestDict.Add("escape", EscapeScene);
		GD.Print("QuestDictionaryUpdated");
		
	}

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
    public void Start(UnitData unitData)
    {
        // Access one of your dialogue variables for this npc by doing unitData.CurrentDialogueData
        bool test = unitData.CurrentDialogueData.Blah;

        GD.Print("testing dialogue editable variables: ", test);

        string id = unitData.ID; // this is the unique string identifier of the unitData

        string name = unitData.Name; // this is the human readable name

        // you can use the "charisma" stat if you want to determine whether a dialogue option should be visible
        int charisma = unitData.Attributes[UnitData.Attribute.Charisma];
        if (charisma > 15)
        {
            // show special dialogue option
        }
        //ADDED
        OnDialogueRefSignal(unitData.ID); //USE THIS WHEN INTEGRATED WITH STAGEWORLD
		//UpdateNameAndPortrait(unitData.Portrait, unitData.Name); //USE THIS WHEN INTEGRATED WITH STAGE WORLD (ADD PORTRAIT TO UNIT PATH)
    }



  /*   public void OnContinueButtonPressed() //@Sarah: move emit signal to when the dialogue finally ends.
    {
       
    } */

}

// Use variables and modify as needed below
// Make any extra dialogue variables needed for dialogue that you want to be saved in here.
// This will be saved whenever the game is saved and will also persist throughout the playthrough and between levels.
[Serializable()]
public class DialogueData : IStoreable
{
    public bool Blah {get; set;} = false;
    //public bool AmuletFound {get; set;} = false; 
}

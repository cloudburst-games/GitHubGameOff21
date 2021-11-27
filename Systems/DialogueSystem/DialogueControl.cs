using Godot;
using System;
using System.Collections.Generic;

public class DialogueControl : Control
{
    [Signal]
    public delegate void GameEnded(bool joinMahef); // true or false
    // EmitSignal(nameof(DialogueControl.GameEnded), true) // or , false)
    [Signal]
    public delegate void MainQuestChanged(string mainQuest); // when you emit this, it will change what is displayed as the current objective
	[Signal] public delegate void JournalUpdatedSignal();
    [Signal]
    public delegate void CreatedAmbush(string idOfAmbusher);
    [Signal]
    public delegate void NPCUnitDataRequested(string npcID);
    // Just emit this signal when you want to hide the dialog box
    [Signal]
    public delegate void DialogueEnded();
    [Signal]
    public delegate void CompanionJoining(UnitDataSignalWrapper npcUnitData);
    [Signal]
    public delegate void CompanionLeaving(UnitDataSignalWrapper npcUnitData);
    [Signal]
    public delegate void CompletedQuest(int difficulty, Godot.Collections.Array<PnlInventory.ItemMode> wrappedItemRewards, int goldReward);
#region Ink code
    public InkStory InkStory;
    string KnotName;
	int DialogueIndex;
	public Dictionary<int, string> DialogueDict = new Dictionary <int, string>();
	public Dictionary<int, string> ChoiceDict = new Dictionary <int, string>();
    public Dictionary<string, PackedScene> QuestDict = new Dictionary<string, PackedScene>();
	//JournalUpublic List<string> _journalList = new List<string>();
	public Dictionary <string,string> _journalDict = new Dictionary<string,string>();
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
	public string pdated = " ";
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
	Label JournalLabel;
	bool _joinParty = false;
	bool _leaveParty = false;
	UnitData CurrentUnitData;
	UnitData CurrentKhepriUnitData;
    private UnitData _requestedUnitData;
	bool _partyFull;
	string _ammitPostFightText = " Whilst you celebrate your victory, followers of Apophis emerge from the sand dunes.\nThey chant a demonic melody and you are engulfed in smoke.\nIn an instant, the dark mist disappears... along with your sun!";

#endregion

#region Dialogue/ink
    public override void _Ready()
    {
        DialogueContainer = GetNode<MarginContainer>("MarginContainer/DialogueContainer");
		Journal = GetNode<Journal>("Journal");
        JournalLabel = GetNode<Label>("Journal/Panel/MarginContainer/VBoxContainer/Journal/ScrollContainer/JournalContainer/MarginContainer/Label");
		//JournalLabel = GetNode<Label>("Journal/Panel/MarginContainer/VBoxContainer/Journal/ScrollContainer/JournalContainer/Label");
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
        Journal.Connect(nameof(Journal.ClosedJournal), this, nameof(OnJournalClosed));
		//MakeQuestDict();
    }



	public void CheckForJoinPartyChanges()
	{
		if ((bool)InkStory.GetVariable("join_party") == _joinParty) //if the ink variable is the same as default, nothing has happened. If it is diffeerent it has changes
		{
			return;
		}
		CompanionJoinParty(CurrentUnitData, CurrentKhepriUnitData); //if the ink variable is different, trigger join party
		_joinParty = false;
		InkStory.SetVariable("join_party", false);
	}

	public void CheckForLeavePartyChanges()
	{
		if ((bool)InkStory.GetVariable("leave_party") == _leaveParty) //
		{
			return;
		}
		CompanionLeaveParty(CurrentUnitData, CurrentKhepriUnitData);
		_leaveParty = false;
		InkStory.SetVariable("leave_party", false);
	}

	public void CheckIfQuestComplete()
	{
		if ((string)InkStory.GetVariable("quest_complete") == "AMULET") //
		{
			CompleteQuest(1, itemRewards:new Godot.Collections.Array<PnlInventory.ItemMode> {PnlInventory.ItemMode.ScarabAmulet}, 10);
			InkStory.SetVariable("quest_complete", "");
		}
		if ((string)InkStory.GetVariable("quest_complete") == "TRAITOR") //
		{
			CompleteQuest(1, itemRewards:new Godot.Collections.Array<PnlInventory.ItemMode> {PnlInventory.ItemMode.RustedArmour}, 20);
			InkStory.SetVariable("quest_complete", "");
		}
		if ((string)InkStory.GetVariable("quest_complete") == "SPHYNX") //
		{
			CompleteQuest(1, itemRewards:new Godot.Collections.Array<PnlInventory.ItemMode> {PnlInventory.ItemMode.IntellectPot}, 10);
			InkStory.SetVariable("quest_complete", "");
		}
		if ((string)InkStory.GetVariable("quest_complete") == "CHARISMA") //
		{
			CompleteQuest(1, itemRewards:new Godot.Collections.Array<PnlInventory.ItemMode> {PnlInventory.ItemMode.LuckPot}, 50);
			InkStory.SetVariable("quest_complete", "");
		}
		if ((string)InkStory.GetVariable("quest_complete") == "MAHEF") //
		{
			CompleteQuest(1, itemRewards:new Godot.Collections.Array<PnlInventory.ItemMode> {PnlInventory.ItemMode.HealthPot}, 50);
			InkStory.SetVariable("quest_complete", "");
		}
		if ((bool)InkStory.GetVariable("ambush")==true)
		{
			CreateAmbush("Ambush");
			InkStory.SetVariable("ambush", false);
		}
		if ((bool)InkStory.GetVariable("hostile")==true)
		{
			CurrentUnitData.Hostile= (bool)InkStory.GetVariable("hostile");
			InkStory.SetVariable("hostile",false);
		}
		if ((bool)InkStory.GetVariable("journal_updated")==true)
		{
			Journal.UpdateJournal();
			InkStory.SetVariable("journal_updated",false);
		}
		if ((bool)InkStory.GetVariable("main_quest_updated")==true)
		{
			EmitSignal(nameof(MainQuestChanged), (string)(InkStory.GetVariable("main_quest")));
			//GD.Print("Main quest changed");
			InkStory.SetVariable("main_quest_updated",false);	
		}
		if ((bool)InkStory.GetVariable("game_ended")==true)
		{
			EmitSignal(nameof(GameEnded), InkStory.GetVariable("mahef_in_party"));
		}
	}

	public void CheckIfJournalUpdated()
	{
		if ((bool)InkStory.GetVariable("journal_updated"))
		{
			EmitSignal(nameof(JournalUpdatedSignal));
			InkStory.SetVariable("journal_updated",false);
		}
	}
	
    // YOU CAN ONLY REQUEST THE DATA OF AN NPC IN THE SAME LEVEL AS THE PLAYER!!! (or in a level you visited before - untested)
    public UnitData GetNPCUnitDataByID(string npcID)
    {
        EmitSignal(nameof(NPCUnitDataRequested), npcID);
        return _requestedUnitData;
    }

    public void OnNPCUnitDataRequestSuccessful(UnitData npcUnitData)
    {
        _requestedUnitData = npcUnitData;
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
	public override void _Input(InputEvent ev)
    {
        base._Input(ev);
        
        if (ev is InputEventKey key && !ev.IsEcho() && ContinueButton.Visible && Visible)
        {
          //  if (key.Scancode == (int) KeyList.Enter || key.Scancode == (int) KeyList.Space && key.Pressed)
            if (key.Scancode == (int) KeyList.Space && key.Pressed)

			{
                OnContinueButtonPressed();
            }
        }
	}

	public void ManageParty()
	{
		if (((string)InkStory.GetVariable("companion_leave"))=="")
		{
			return;
		}
		//CompanionLeaveParty(npcUnitData, )
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
			float duration = MainText.Text.Length/20; //30 is a random number to control the reveal speed for different lengths of text. CHange "45" if speed needs to change.
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
					Button.Connect("ChoiceButtonPressedSignal", this, nameof(OnChoiceButtonPressed));
				}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
			}
			else
			{
				//instead of quickly removing the visibility, perhaps play an anim here
				//	SaveVariables();
					DialogueContainer.Visible = false;
					CheckForJoinPartyChanges();
					CheckForLeavePartyChanges();
					CheckIfQuestComplete();
					GetTree().Paused = false;
                    EmitSignal(nameof(DialogueEnded)); //ENDS DIALOGUE ALTOGETHER
				
				
			}
		}
	}


	public void UpdateInkVariables(string inkyVariable, bool savedVariable)
	{
		InkStory.SetVariable(inkyVariable, savedVariable);
	}

     public void OnChoiceButtonPressed(int n)  
	{
		
		GD.Print(n + " Chocie button num dialoge");
		InkStory.ChooseChoiceIndex(n);
		foreach (Button b in ButtonContainer.GetChildren())
			{
				b.QueueFree();
			}
		while (InkStory.CanContinue)
		{
			MakeDialogueDict();
			return;
		}
		if ((!InkStory.CanContinue) && (!InkStory.HasChoices))
		{
			//instead of quickly removing the visibility, perhaps play an anim here
			//SaveVariables();
			DialogueContainer.Visible = false;
			CheckForJoinPartyChanges();
			CheckForLeavePartyChanges();
			CheckIfQuestComplete();
			GetTree().Paused = false;
            EmitSignal(nameof(DialogueEnded));
		}
	}

    // call this when khepri talks to an NPC, and accepts the NPC offer to join.
    private void CompanionJoinParty(UnitData npcUnitData, UnitData khepriUnitData)
    {
        // @Sarah - you should only show the option to join party if khepri has less than two companions - check this
        // by doing if (khepriUnitData.Companions.Count < 2). If khepri already has 2 or more companions, you need to show
        // dialogue option that party is full and come back if you need me something like that

        // error checking -check if khepri already has 2 companions. If this shows up, sarah fix your code.
        if (khepriUnitData.Companions.Count >= 2)
        {
            GD.Print("we shouldnt offer to join because party is full!");
            return;
        }
        EmitSignal(nameof(CompanionJoining), new UnitDataSignalWrapper() {CurrentUnitData = npcUnitData});
    }

    // call this for example, when khepri talks to the party member and says I don't want you anymore. The option should
    // only exist for party members!
    private void CompanionLeaveParty(UnitData npcUnitData, UnitData khepriUnitData)
    {
        
        // error checking - check if the npc is indeed one of khepri's crew. If this shows up, sarah fix your code.CompanionLeave
        if (!khepriUnitData.Companions.Contains(npcUnitData))
        {
            GD.Print("we shouldnt ask them to leave because they aren't in our party!");
            return;
        }
        EmitSignal(nameof(CompanionLeaving), new UnitDataSignalWrapper() {CurrentUnitData = npcUnitData});
    }

    // call this after the player hands in a quest to the NPC
    // pass in a difficulty integer, which is equivalent to the player level the quest is designed for.
    // e.g. a quest designed for level 3 character you would pass 3 as the difficulty variable
    // also pass in a gold reward (0 if you dont want to reward gold), and any items
    // e.g.:
    // CompleteQuest(
    //     difficulty:3, 
    //     itemRewards:new List<PnlInventory.ItemMode> { PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.ManaPot, PnlInventory.ItemMode.ResiliencePot },
    //     goldReward:31);
    // note you will also need to remove it from the player's journal
    private void CompleteQuest(int difficulty, Godot.Collections.Array<PnlInventory.ItemMode> itemRewards, int goldReward)
    {
        EmitSignal(nameof(CompletedQuest), difficulty, itemRewards, goldReward);
    }



    // public void OnCompletedAmuletQuest()
    // {
    //     CompleteQuest(
    //         difficulty:1, 
    //         itemRewards:new List<PnlInventory.ItemMode> { PnlInventory.ItemMode.ScarabAmulet },
    //         goldReward:13);
    // }

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


	public void Load(UnitData npcUnitData, UnitData khepriUnitData)
	{
		JournalLabel.Text = khepriUnitData.CurrentDialogueData.JournalText;
		GD.Print(khepriUnitData.CurrentDialogueData.JournalText);
		if (khepriUnitData.CurrentDialogueData.InkyString == "nil")
		{
			return;
			
		}
		InkStory.SetState(khepriUnitData.CurrentDialogueData.InkyString);
	}

	private UnitData _khepriUnitData;
	private UnitData _npcUnitData;

    public void CreateAmbush(string idOfAmbusher)
    {
        EmitSignal(nameof(CreatedAmbush), idOfAmbusher);
    }

    // Started when press E next to non-companion, non-hostile NPC
    // Passes in the interlocutor data (modify variables inside here as needed - will be stored on save and load)
    // within this can access DialogueData. Modify variables as needed in class below.
	//UnitData requestedUnitDataAmbush;
    public void Start(UnitData npcUnitData, UnitData khepriUnitData)
    {

        // ******** @Sarah: examples -: ********
        // YOU CAN GET THE ID OF ANY NPC AND ACCESS ITS UNITDATA BY DOING UnitData requestedUnitData = GetNPCUnitDataByID(id); E.g.:
        
        // UnitData requestedUnitData = GetNPCUnitDataByID("test3416");
        // GD.Print("name of test3416: ", requestedUnitData.Name);
        // requestedUnitData.Hostile = true; // make them hostile on sight
        // requestedUnitData.InitiatesDialogue = true; // alternatively, make them intiiate dialogue as soon as they are close to you
        // (do one or the other, not both)

        // You can create an ambush by entering the id of the ambusher NPC. this NPC would usually be set as Active = false in the editor. 
        // So the player cannot see them on the world map
        // E.g:
        // CreateAmbush("bob2");
        // If the NPC with this ID does not exist in the same level as the player, nothing will happen

        // ********    examples end.    ********

		_khepriUnitData = khepriUnitData;
		CurrentUnitData = npcUnitData;
		CurrentKhepriUnitData = khepriUnitData;
		//requestedUnitDataAmbush = GetNPCUnitDataByID("Ambush");
		//UnitData requestedUnitData = GetNPCUnitDataByID("Ammit");
		
		//requestedUnitData.DefeatMessage = _ammitPostFightText; THIS BROKE WHEN I DEFEATED AMIT AND SHE WAS NO LONGER THERE
		Load(npcUnitData, khepriUnitData);




        // // REMEMBER: khepriUnitData will always be there with the player (as it is khepri) and so u can store things to always be accessible there
        // // npcUnitData is the data for the guy khepri is talking to!

        // // you can use KHEPRI'S "charisma" stat if you want to determine whether a dialogue option should be visible
        int charisma = khepriUnitData.Attributes[UnitData.Attribute.Charisma];
		int companion_count = khepriUnitData.Companions.Count;
		if (companion_count>=2)
		{	
			_partyFull = true;
		}
		if (companion_count<2)
		{
			_partyFull = false;
		}
		InkStory.SetVariable("party_full", _partyFull);		
		if (charisma>=15)
		{	
			bool _charisma;
			_charisma = true;
			InkStory.SetVariable("charisma", _charisma);
		}
		UpdateNameAndPortrait(npcUnitData.Name, npcUnitData.PortraitPathSmall);
		OnDialogueRefSignal(npcUnitData.ID); //USE THIS WHEN INTEGRATED WITH STAGEWORLD

    }

	// run this and save stuff before exiting dialogue
	public void Save()
	{
		//_khepriUnitData.CurrentDialogueData.JournalDict = _journalDict;
		_khepriUnitData.CurrentDialogueData.JournalText = JournalLabel.Text;
		_khepriUnitData.CurrentDialogueData.InkyString = InkStory.GetState();
	}

    private void OnJournalClosed()
    {
        // here so we can hide the dialoguecontrol as well (GetParent() is bad and should never be used)
        Visible = false;
    }

	public override void _Process(float delta)
	{
	}

}

// Use variables and modify as needed below
// Make any extra dialogue variables needed for dialogue that you want to be saved in here.
// This will be saved whenever the game is saved and will also persist throughout the playthrough and between levels.
[Serializable()]
public class DialogueData : IStoreable
{
    /* public bool Blah {get; set;} = false;
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
 */	public bool Modified {get; set;} = false;
	public string JournalString{get;set;} = "";
	public string InkyString {get; set;} = "nil";
	public bool TalkAfterBattle = false;
	public string JournalText = "";
	//public List <string> JournalList {get; set;}
}
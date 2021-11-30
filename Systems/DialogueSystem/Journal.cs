using Godot;
using System;
using System.Collections.Generic;

public class Journal : MarginContainer
{
    VBoxContainer QuestContainer;
    DialogueControl DialogueControl;
   // Tween Tween;
    Label AmuletLabel;
    Label CompanionLabel;
    MarginContainer Amulet;
    MarginContainer Companion;
    MarginContainer Stolen;
    Label StolenLabel;
    MarginContainer NightGate;
    Label NightGateLabel;
    MarginContainer Escape;
    Label EscapeLabel;
    Panel JournalPanel;
    Panel QuestPanel;
  //  Label JournalLabel;
    PackedScene journalLabelScene = GD.Load<PackedScene>("res://Systems/DialogueSystem/JournalLabel.tscn");
    VBoxContainer JournalContainer;
    Button JournalButton;
    Button QuestButton;
    Label JournalLabel;
    
    [Signal]
    public delegate void ClosedJournal();

   //public List<string> JournalList = new List < string>();

    public override void _Ready()
    {
       DialogueControl = (DialogueControl)this.GetParent();
        QuestContainer = GetNode<VBoxContainer>("Panel/MarginContainer/VBoxContainer/Quests/ScrollContainer/QuestContainer");
       // Tween = GetNode<Tween>("Tween");
        JournalPanel = GetNode<Panel>("Panel/MarginContainer/VBoxContainer/Journal");
        QuestPanel = GetNode<Panel>("Panel/MarginContainer/VBoxContainer/Quests");
        JournalContainer = GetNode<VBoxContainer>("Panel/MarginContainer/VBoxContainer/Journal/ScrollContainer/JournalContainer");
        this.Visible = false;
        JournalButton = GetNode<Button>("Panel/MarginContainer/VBoxContainer/HBoxContainer/JournalButton");
        QuestButton = GetNode<Button>("Panel/MarginContainer/VBoxContainer/HBoxContainer/QuestButton");
       // JournalButton.AddColorOverride("font_color", new Color(0.99f, 0.96f, 0.9f, 1f));
       // QuestButton.AddColorOverride("font_color", new Color(0.96f, 0.64f, 0.38f, 1 ));
        JournalLabel = GetNode<Label>("Panel/MarginContainer/VBoxContainer/Journal/ScrollContainer/JournalContainer/MarginContainer/Label");
        /* DialogueControl.JournalList.Add("xxxxxxxxxxxxxxxxxxxxxx");
        DialogueControl.JournalList.Add("YYYYYYYYYYYYYYYYYYYYY"); */
    }

    public void ShowJournal(UnitData khepriUnitData)
    {
        // UPDATE JOURNAL WITH KHEPRI DATA IN HERE
       // UpdateJournal();
        //TEST
       // GD.Print(khepriUnitData.CurrentDialogueData.JournalString);
        //

        this.Visible = true; //replace with anim
    }

    public void UpdateJournal() //@ SARAH ADD ALL THE VARIABLES THAT YOU WANT RECORDED IN THE JOURNAL HERE
	{
		JournalLabel.Text += (string)DialogueControl.InkStory.GetVariable("journal_text");
        JournalLabel.Text += "\n";
        DialogueControl.InkStory.SetVariable("journal_text","");
        
	}


    public void InstanceQuest(PackedScene scene)
    {
        var instance = scene.Instance();
		QuestContainer.AddChild(instance);
        GD.Print("Questadded");

    }
    public void UpdateCompletedQuests(string completedquest)
	{
/*         if(completedquest == "amulet")
        {
            Amulet = QuestContainer.GetNode<MarginContainer>("AmuletLabel");
            AmuletLabel = QuestContainer.GetNode<Label>("AmuletLabel/Label");
            AmuletLabel.Text = "Amulet of the dead: Quest complete!";
            //AnimationPlayerRemove - move the position of the amulet to the side and quefree it.
        }
        if(completedquest == "companion")
        {
            Companion = QuestContainer.GetNode<MarginContainer>("CompanionLabel");
            CompanionLabel = QuestContainer.GetNode<Label>("CompanionLabel/Label");
            CompanionLabel.Text = "Amass an army: Quest complete!";
            //AnimationPlayerRemove - move the position of the amulet to the side and quefree it.
        }
        if(completedquest == "stolen")
        {
            Stolen = QuestContainer.GetNode<MarginContainer>("StolenLabel");
            StolenLabel = QuestContainer.GetNode<Label>("StolenLabel/Label");
            StolenLabel.Text = "u found the sun!";
        }
        if(completedquest == "night_gate")
        {
            NightGate = QuestContainer.GetNode<MarginContainer>("NightGateLabel");
            NightGateLabel = QuestContainer.GetNode<Label>("NightGateLabel/Label");
            NightGateLabel.Text = "u found the nmightgate!";
        }
        if(completedquest == "escape")
        {
            Escape = QuestContainer.GetNode<MarginContainer>("EscapeLabel");
            EscapeLabel = QuestContainer.GetNode<Label>("EscapeLabel/Label");
            EscapeLabel.Text = "u escaped!";
        } */
	}

 /*    public void UpdateJournal(string journalUpdate)
    {
        if (DialogueControl._journalList.Contains(journalUpdate)== false)
        {
            DialogueControl._journalList.Add(journalUpdate);
        /*     for(int i = 0; i < JournalList.Count; i++)
            { */
                /* var instance = journalLabelScene.Instance();
                JournalContainer.AddChild(instance);
                instance.GetNode<Label>("Label").Text = journalUpdate; */

                //JournalLabel = JournalContainer.GetNode<Label>("JournalLabel/PanelContainer/Panel/Label");
               // instance.GetNode<Label>("PanelContainer/Panel/Label").Text = JournalList[i];
          //  }
     //   } 
        
       
      // JournalContainer.AddChild(instance);
      // 
      // JournalLabel.Text = journalUpdate;
      //  GD.Print("journal update check" + journalUpdate);
  //  }

    public void OnExitButtonPressed()
    {
        this.Visible=false;
        EmitSignal(nameof(ClosedJournal)); // need to know when u are closing journal so can unpause stuff..
    }

/*     public void OnQuestButtonPressed()
    {
        JournalPanel.Visible=false;
        QuestPanel.Visible = true;
        QuestButton.AddColorOverride("font_color", new Color(0.99f, 0.96f, 0.9f, 1f));
        JournalButton.AddColorOverride("font_color", new Color(0.96f, 0.64f, 0.38f, 1 ));
    } */

    public void OnJournalButtonPressed() //when you press the journal button, 
    {
        /* this.Visible = true;
        JournalPanel.Visible=true;
       // QuestPanel.Visible = false;
        JournalButton.AddColorOverride("font_color", new Color(0.99f, 0.96f, 0.9f, 1f));
        JournalLabel.Text = (string)DialogueControl.InkStory.GetVariable("journal"); */
       // DialogueControl.UpdateJournal();

/*         for (int i = 0; i<DialogueControl._journalList.Count; i++)
        {
            JournalLabel.Text += DialogueControl._journalList[i];
        } */
        //QuestButton.AddColorOverride("font_color", new Color(0.96f, 0.64f, 0.38f, 1 ));
    }



    //for testing:
    //Connect to the emit conversation signal and print

    //To do:
    //Emit a signal every time an NPC wants to talk. Signal must include the name of the NPC
    
}

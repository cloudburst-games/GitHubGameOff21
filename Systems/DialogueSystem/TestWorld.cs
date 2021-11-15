using Godot;
using System;

public class TestWorld : Control
{
    DialogueControl DialogueControl;
    Journal Journal;
    string NPC1Tex;
    string NPC0Tex;
    string NPC2Tex;
    string NPC3Tex;
    
    public override void _Ready()
    {
        DialogueControl = GetNode<DialogueControl>("DialogueControl");
        Journal = GetNode<Journal>("DialogueControl/Journal");
        NPC1Tex = "res://Systems/DialogueSystem/RaKhepri1.png";
        NPC0Tex = "res://Systems/DialogueSystem/RaKhepri0.png";
        NPC2Tex = "res://Systems/DialogueSystem/RaKhepri2.png";
        NPC3Tex = "res://Systems/DialogueSystem/RaKhepri3.png";
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public void OnNPC1ButtonPressed()
    {
        DialogueControl.StartDialogue("NPC1");
        DialogueControl.UpdateNameAndPortrait("NPC1", NPC1Tex);
    }
    public void OnNPC2ButtonPressed()
    {
        DialogueControl.StartDialogue("NPC2");
        DialogueControl.UpdateNameAndPortrait("NPC2", NPC2Tex);
    }
    public void OnNPC0ButtonPressed()
    {
        DialogueControl.StartDialogue("NPC0");
        DialogueControl.UpdateNameAndPortrait("NPC0", NPC0Tex);
    }
    public void OnNPC3ButtonPressed()
    {
        DialogueControl.StartDialogue("NPC3");
        DialogueControl.UpdateNameAndPortrait("NPC3", NPC3Tex);
    }

    public void OnNPC4ButtonPressed()
    {
        DialogueControl.StartDialogue("NPC4");
        DialogueControl.UpdateNameAndPortrait("NPC4", NPC3Tex);
    }

    public void OnJournalButtonPressed()
    {
        Journal.ShowJournal();
    }
}

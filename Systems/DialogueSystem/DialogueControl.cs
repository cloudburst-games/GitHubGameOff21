using Godot;
using System;

public class DialogueControl : Control
{

    // Just emit this signal when you want to hide the dialog box
    [Signal]
    public delegate void DialogueEnded();

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
    }

    public void OnTestEndDialogueButtonPressed()
    {
        EmitSignal(nameof(DialogueEnded));
    }

}

// Use variables and modify as needed below
// Make any extra dialogue variables needed for dialogue that you want to be saved in here.
// This will be saved whenever the game is saved and will also persist throughout the playthrough and between levels.
[Serializable()]
public class DialogueData : IStoreable
{
    public bool Blah {get; set;} = false;
}

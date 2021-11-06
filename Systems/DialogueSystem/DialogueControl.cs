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
        bool test = unitData.CurrentDialogueData.Blah;

        GD.Print("testing dialogue editable variables: ", test);
    }

    public void OnTestEndDialogueButtonPressed()
    {
        EmitSignal(nameof(DialogueEnded));
    }

}

// Use variables and modify as needed below
[Serializable()]
public class DialogueData : IStoreable
{
    public bool Blah {get; set;} = false;
}

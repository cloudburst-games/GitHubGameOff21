using Godot;
using System;

public class ChoiceButton : Button
{
     [Export]
    public int Id {get; set;} = 0;
    [Signal] public delegate void ChoiceButtonPressedSignal(int Id); 
    Label Label;
    public override void _Ready()
    {
      // this.Connect("ChoiceButtonPressedSignal", this, nameof(OnChoiceButtonPressed));
       Label = GetNode<Label>("Label");
       Label.SetModulate(new Color (0.98f, 0.92f, 0.84f, 1.0f));
       //0.98, 0.92, 0.84, 1
    }
    public void OnButtonPressed()
    {
        Label.SetModulate(new Color(108.5f, 209.0f, 43.0f));
        Id = this.GetPositionInParent();
        EmitSignal("ChoiceButtonPressedSignal", Id);
    }

/*     public void OnChoiceButtonPressed(int Id)
    {
        GD.Print("pressed new" + Id);
    }
 */
	private void OnMouseEntered()
	{
		 Label.SetModulate(new Color(0.13f, 0.55f, 0.13f));	 
	}

	private void OnMouseExited()
	{
		Label.SetModulate(new Color(0.98f, 0.92f, 0.84f, 1.0f));
	}

    public override void _Process(float delta)
    {
        
    }


}



    

using Godot;
using System;

public class PnlSettingsGame : Panel
{
    [Signal]
    public delegate void DifficultySelected(int difficulty);
	
	[Signal]
	public delegate void SettingsChanged();

    public override void _Ready()
    {
    }
	public void RefreshSettings()
	{

	}

    public void SetDifficultyText(int index)
    {
        switch (index)
        {
            case 0:
                GetNode<Label>("LblDifficulty2").Text = "At the easiest difficulty, enemies are weaker and fewer in number.";
                break;
            case 1:
                GetNode<Label>("LblDifficulty2").Text = "At medium difficulty, enemies put up a fight, but can be readily defeated with the right strategy.";
                break;
            case 2:
                GetNode<Label>("LblDifficulty2").Text = "For veterans who do not shy away from a tactical challenge, enemies will appear in greater numbers, and can learn the spells of their commanders.";
                break;
            case 3:
                GetNode<Label>("LblDifficulty2").Text = "This difficulty is not for the faint of heart. Enemies are stronger and more numerous, and more likely to be armed with devastating magic beyond the capabilities of their commanders.";
                break;
        }
    }

    public void OnBtnDifficultyItemSelected(int index) // 0 easy 1 medium 2 hard 3 very hard
    {
        SetDifficultyText(index);
        EmitSignal(nameof(SettingsChanged));
    }
}

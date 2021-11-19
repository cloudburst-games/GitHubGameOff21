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

    public void OnBtnDifficultyItemSelected(int index) // 0 easy 1 medium 2 hard 3 very hard
    {
        EmitSignal(nameof(SettingsChanged));
    }
}

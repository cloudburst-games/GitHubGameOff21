using Godot;
using System;

public class PnlInfo : Panel
{
    private ProgressBar _healthBar;
    private ProgressBar _manaBar;
    public override void _Ready()
    {
        _healthBar = GetNode<ProgressBar>("VBox/HealthBar");
        _manaBar = GetNode<ProgressBar>("VBox/ManaBar");
    }

    public void Update(float health, float maxHealth, float mana, float maxMana)
    {
        _healthBar.Value = (health/maxHealth)*100;
        _manaBar.Value = (mana/maxMana)*100;
    }

    public void SetFaction(bool player)
    {
        GetNode<Panel>("PnlFaction").SelfModulate = player ? new Color(1,1,1) : new Color(0.6f,0,0);
    }
}

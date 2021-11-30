using Godot;
using System;
using System.Collections.Generic;

public class StageBattle : Stage
{
    private DataBinary _heldData;
    public override void _Ready()
    {
        base._Ready();

        GetNode<Button>("CntBattle/Panel/BattleHUD/CtrlTheme/PnlMenu/VBox/BtnSettings").Connect("pressed", this, nameof(OnBtnSettingsPressed));
        GetNode<OptionButton>("CanvasLayer/PnlSettings/CntPanels/PnlGame/HBoxContainer/BtnDifficulty").Disabled = true;
        if (SharedData != null)
        {
            // _heldData = (DataBinary) SharedData["Data"];
            GetNode<CntBattle>("CntBattle").Start(
                (BattleUnitData) SharedData["PlayerData"],
                (BattleUnitData) SharedData["EnemyCommanderData"],
                (List<BattleUnitData>) SharedData["FriendliesData"],
                (List<BattleUnitData>) SharedData["HostilesData"],
                (int) SharedData["Difficulty"]);
            

            GetNode<CntBattle>("CntBattle").Connect(nameof(CntBattle.BattleEnded), this, nameof(OnBattleEnded));
            return;
        }
        else
        {
            GD.Print("ERROR data not loaded into stage battle");
            Test();
        }
        
            

    }

    public void OnBtnSettingsPressed()
    {
        GetNode<PnlSettings>("CanvasLayer/PnlSettings").Visible = true;
    }

    public void OnBattleEnded(bool quitToMainMenu, bool victory, BattleUnitDataSignalWrapper wrappedEnemyCommanderData)
    {
        GetNode<CntBattle>("CntBattle").Die();
        if (quitToMainMenu)
        {
            SceneManager.SimpleChangeScene(SceneData.Stage.MainMenu);
            return;
        }

        BattleUnitData enemyCommanderData = wrappedEnemyCommanderData.CurrentBattleUnitData;

        SceneManager.SimpleChangeScene(SceneData.Stage.World, new Dictionary<string, object>() {
            ["Data"] = (DataBinary) SharedData["Data"],
            ["EnemyCommanderData"] = enemyCommanderData,
            ["Victory"] = victory,
            ["Events"] = (string) SharedData["Events"],
            ["MainQuestText"] = (string) SharedData["MainQuestText"]
        });



    }

    public void Test()
    {
        BattleUnitData playerData = new BattleUnitData() {
            Name = "Khepri sun",
            Combatant = BattleUnit.Combatant.Beetle,
            Level = 3,
            Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                {BattleUnitData.DerivedStat.Health, 10},
                {BattleUnitData.DerivedStat.TotalHealth, 10},
                {BattleUnitData.DerivedStat.Mana, 100},
                {BattleUnitData.DerivedStat.TotalMana, 100},
                {BattleUnitData.DerivedStat.HealthRegen, 1},
                {BattleUnitData.DerivedStat.ManaRegen, 1},
                {BattleUnitData.DerivedStat.MagicResist, 10},
                {BattleUnitData.DerivedStat.PhysicalResist, 10},
                {BattleUnitData.DerivedStat.Dodge, 5},
                {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 3},
                {BattleUnitData.DerivedStat.SpellDamage, 5},
                {BattleUnitData.DerivedStat.Speed, 6},
                {BattleUnitData.DerivedStat.Initiative, 5},
                {BattleUnitData.DerivedStat.Leadership, 1},
                {BattleUnitData.DerivedStat.CriticalChance, 1},
                {BattleUnitData.DerivedStat.CurrentAP, 6}
            },
            Spell1 = SpellEffectManager.SpellMode.LunarBlast,
            Spell2 = SpellEffectManager.SpellMode.LunarBlast,
            BodyPath = "res://Actors/NPC/Bodies/NPCBody.tscn"
            
        };
        playerData.PotionsEquipped = new PnlInventory.ItemMode[3] {PnlInventory.ItemMode.LuckPot, PnlInventory.ItemMode.HealthPot, PnlInventory.ItemMode.IntellectPot};
        BattleUnitData enemyCommanderData = new BattleUnitData() {
            Name = "Mr Commander",
            Combatant = BattleUnit.Combatant.Beetle,
            Level = 3,
            Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                {BattleUnitData.DerivedStat.Health, 10},
                {BattleUnitData.DerivedStat.TotalHealth, 10},
                {BattleUnitData.DerivedStat.Mana, 10},
                {BattleUnitData.DerivedStat.TotalMana, 10},
                {BattleUnitData.DerivedStat.HealthRegen, 1},
                {BattleUnitData.DerivedStat.ManaRegen, 1},
                {BattleUnitData.DerivedStat.MagicResist, 10},
                {BattleUnitData.DerivedStat.PhysicalResist, 10},
                {BattleUnitData.DerivedStat.Dodge, 5},
                {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 2},
                {BattleUnitData.DerivedStat.SpellDamage, 5},
                {BattleUnitData.DerivedStat.Speed, 6},
                {BattleUnitData.DerivedStat.Initiative, 4},
                {BattleUnitData.DerivedStat.Leadership, 15},
                {BattleUnitData.DerivedStat.CriticalChance, 1},
                {BattleUnitData.DerivedStat.CurrentAP, 6}
            },
            Spell1 = SpellEffectManager.SpellMode.LunarBlast,
            Spell2 = SpellEffectManager.SpellMode.LunarBlast,
            BodyPath = "res://Actors/NPC/Bodies/NPCBody.tscn"
        };
        // GD.Print("fact: ", enemyCommanderData.PlayerFaction);
        enemyCommanderData.Stats[BattleUnitData.DerivedStat.Initiative] = 5;
        List<BattleUnitData> friendliesData = new List<BattleUnitData>() {
            new BattleUnitData() {
                Combatant = BattleUnit.Combatant.Beetle,
                Level = 1,
                Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                    {BattleUnitData.DerivedStat.Health, 10},
                    {BattleUnitData.DerivedStat.TotalHealth, 10},
                    {BattleUnitData.DerivedStat.Mana, 100},
                    {BattleUnitData.DerivedStat.TotalMana, 100},
                    {BattleUnitData.DerivedStat.HealthRegen, 1},
                    {BattleUnitData.DerivedStat.ManaRegen, 1},
                    {BattleUnitData.DerivedStat.MagicResist, 10},
                    {BattleUnitData.DerivedStat.PhysicalResist, 10},
                    {BattleUnitData.DerivedStat.Dodge, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamageRange, 3},
                    {BattleUnitData.DerivedStat.SpellDamage, 5},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 3},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                },
                Spell1 = SpellEffectManager.SpellMode.LunarBlast,
                Spell2 = SpellEffectManager.SpellMode.LunarBlast,
            BodyPath = "res://Actors/NPC/Bodies/NPCBody.tscn"
            },
            new BattleUnitData() {
                Combatant = BattleUnit.Combatant.Beetle,
                Level = 2,
                Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                    {BattleUnitData.DerivedStat.Health, 10},
                    {BattleUnitData.DerivedStat.TotalHealth, 10},
                    {BattleUnitData.DerivedStat.Mana, 100},
                    {BattleUnitData.DerivedStat.TotalMana, 100},
                    {BattleUnitData.DerivedStat.HealthRegen, 1},
                    {BattleUnitData.DerivedStat.ManaRegen, 1},
                    {BattleUnitData.DerivedStat.MagicResist, 10},
                    {BattleUnitData.DerivedStat.PhysicalResist, 10},
                    {BattleUnitData.DerivedStat.Dodge, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 3},
                    {BattleUnitData.DerivedStat.SpellDamage, 5},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 3},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                },
                Spell1 = SpellEffectManager.SpellMode.LunarBlast,
                Spell2 = SpellEffectManager.SpellMode.LunarBlast,
            BodyPath = "res://Actors/NPC/Bodies/NPCBody.tscn"
            },
            new BattleUnitData() {
                Combatant = BattleUnit.Combatant.Beetle,
                Level = 1,
                Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                    {BattleUnitData.DerivedStat.Health, 10},
                    {BattleUnitData.DerivedStat.TotalHealth, 10},
                    {BattleUnitData.DerivedStat.Mana, 100},
                    {BattleUnitData.DerivedStat.TotalMana, 100},
                    {BattleUnitData.DerivedStat.HealthRegen, 1},
                    {BattleUnitData.DerivedStat.ManaRegen, 1},
                    {BattleUnitData.DerivedStat.MagicResist, 10},
                    {BattleUnitData.DerivedStat.PhysicalResist, 10},
                    {BattleUnitData.DerivedStat.Dodge, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamageRange, 4},
                    {BattleUnitData.DerivedStat.SpellDamage, 5},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 2},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                },
                Spell1 = SpellEffectManager.SpellMode.LunarBlast,
                Spell2 = SpellEffectManager.SpellMode.LunarBlast,
            BodyPath = "res://Actors/NPC/Bodies/NPCBody.tscn"
            }
        };
        List<BattleUnitData> hostilesData = new List<BattleUnitData>() {
            new BattleUnitData() {
                Combatant = BattleUnit.Combatant.Beetle,
                Level = 2,
                Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                    {BattleUnitData.DerivedStat.Health, 10},
                    {BattleUnitData.DerivedStat.TotalHealth, 10},
                    {BattleUnitData.DerivedStat.Mana, 100},
                    {BattleUnitData.DerivedStat.TotalMana, 100},
                    {BattleUnitData.DerivedStat.HealthRegen, 1},
                    {BattleUnitData.DerivedStat.ManaRegen, 1},
                    {BattleUnitData.DerivedStat.MagicResist, 10},
                    {BattleUnitData.DerivedStat.PhysicalResist, 10},
                    {BattleUnitData.DerivedStat.Dodge, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 3},
                    {BattleUnitData.DerivedStat.SpellDamage, 5},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 1},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                },
                Spell1 = SpellEffectManager.SpellMode.LunarBlast,
                Spell2 = SpellEffectManager.SpellMode.LunarBlast,
            BodyPath = "res://Actors/NPC/Bodies/NPCBody.tscn"
            },
            new BattleUnitData() {
                Combatant = BattleUnit.Combatant.Beetle,
                Level = 1,
                Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                    {BattleUnitData.DerivedStat.Health, 10},
                    {BattleUnitData.DerivedStat.TotalHealth, 10},
                    {BattleUnitData.DerivedStat.Mana, 100},
                    {BattleUnitData.DerivedStat.TotalMana, 100},
                    {BattleUnitData.DerivedStat.HealthRegen, 1},
                    {BattleUnitData.DerivedStat.ManaRegen, 1},
                    {BattleUnitData.DerivedStat.MagicResist, 10},
                    {BattleUnitData.DerivedStat.PhysicalResist, 10},
                    {BattleUnitData.DerivedStat.Dodge, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 3},
                    {BattleUnitData.DerivedStat.SpellDamage, 5},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 2},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                },
                Spell1 = SpellEffectManager.SpellMode.LunarBlast,
                Spell2 = SpellEffectManager.SpellMode.LunarBlast,
            BodyPath = "res://Actors/NPC/Bodies/NPCBody.tscn"
            },
            new BattleUnitData() {
                Combatant = BattleUnit.Combatant.Beetle,
                Level = 1,
                Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                    {BattleUnitData.DerivedStat.Health, 10},
                    {BattleUnitData.DerivedStat.TotalHealth, 10},
                    {BattleUnitData.DerivedStat.Mana, 100},
                    {BattleUnitData.DerivedStat.TotalMana, 100},
                    {BattleUnitData.DerivedStat.HealthRegen, 1},
                    {BattleUnitData.DerivedStat.ManaRegen, 1},
                    {BattleUnitData.DerivedStat.MagicResist, 10},
                    {BattleUnitData.DerivedStat.PhysicalResist, 10},
                    {BattleUnitData.DerivedStat.Dodge, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 3},
                    {BattleUnitData.DerivedStat.SpellDamage, 5},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 3},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                },
                Spell1 = SpellEffectManager.SpellMode.LunarBlast,
                Spell2 = SpellEffectManager.SpellMode.LunarBlast,
            BodyPath = "res://Actors/NPC/Bodies/NPCBody.tscn"
            },
            new BattleUnitData() {
                Combatant = BattleUnit.Combatant.Beetle,
                Level = 1,
                Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                    {BattleUnitData.DerivedStat.Health, 10},
                    {BattleUnitData.DerivedStat.TotalHealth, 10},
                    {BattleUnitData.DerivedStat.Mana, 100},
                    {BattleUnitData.DerivedStat.TotalMana, 100},
                    {BattleUnitData.DerivedStat.HealthRegen, 1},
                    {BattleUnitData.DerivedStat.ManaRegen, 1},
                    {BattleUnitData.DerivedStat.MagicResist, 10},
                    {BattleUnitData.DerivedStat.PhysicalResist, 10},
                    {BattleUnitData.DerivedStat.Dodge, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 1},
                    {BattleUnitData.DerivedStat.SpellDamage, 5},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 3},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                },
                Spell1 = SpellEffectManager.SpellMode.LunarBlast,
                Spell2 = SpellEffectManager.SpellMode.LunarBlast,
            BodyPath = "res://Actors/NPC/Bodies/NPCBody.tscn"
            },
            new BattleUnitData() {
                Combatant = BattleUnit.Combatant.Beetle,
                Level = 1,
                Stats = new Dictionary<BattleUnitData.DerivedStat, float>() {
                    {BattleUnitData.DerivedStat.Health, 10},
                    {BattleUnitData.DerivedStat.TotalHealth, 10},
                    {BattleUnitData.DerivedStat.Mana, 100},
                    {BattleUnitData.DerivedStat.TotalMana, 100},
                    {BattleUnitData.DerivedStat.HealthRegen, 1},
                    {BattleUnitData.DerivedStat.ManaRegen, 1},
                    {BattleUnitData.DerivedStat.MagicResist, 10},
                    {BattleUnitData.DerivedStat.PhysicalResist, 10},
                    {BattleUnitData.DerivedStat.Dodge, 5},
                    {BattleUnitData.DerivedStat.PhysicalDamage, 5},
                {BattleUnitData.DerivedStat.PhysicalDamageRange, 3},
                    {BattleUnitData.DerivedStat.SpellDamage, 5},
                    {BattleUnitData.DerivedStat.Speed, 6},
                    {BattleUnitData.DerivedStat.Initiative, 3},
                    {BattleUnitData.DerivedStat.Leadership, 1},
                    {BattleUnitData.DerivedStat.CriticalChance, 1},
                    {BattleUnitData.DerivedStat.CurrentAP, 6}
                },
                Spell1 = SpellEffectManager.SpellMode.LunarBlast,
                Spell2 = SpellEffectManager.SpellMode.LunarBlast,
            BodyPath = "res://Actors/NPC/Bodies/NPCBody.tscn"
            }
        };
        GetNode<CntBattle>("CntBattle").Start(playerData, enemyCommanderData, friendliesData, hostilesData, 1);
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

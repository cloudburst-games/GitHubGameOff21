using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable()]
public class UnitData : IStoreable
{
    public bool Active {get; set;} = true;
    public string PortraitPath {get; set;} = "";
    public string PortraitPathSmall {get; set;} = "";
    public string CustomBattleText {get; set;} = "";
    public string ID {get; set;} = "";
    private string _name = "";
    public string Name {
        get {
            return _name;
        }
        set{
            _name = value;
            CurrentBattleUnitData.Name = value;
        }
    }
    public Vector2 NPCPosition {get; set;}
    private bool _companion = false;
    public bool Companion {
        get {
            return _companion;
        }
        set {
            _companion = value;
            if (_companion)
            {
                Behaviour = AIUnitControlState.AIBehaviour.Follow;
            }
        }
    }
    public bool Modified {get; set;} = false;
    public float BasePhysicalDamageRange {get; set;} = 3f;
    public int Gold {get; set;} = 0;
    // public List<IInventoryPlaceable> ItemsHeld {get; set;} = new List<IInventoryPlaceable>();// TODO make a serializable thing instead
    
    public enum Attribute { Vigour, Resilience, Intellect, Swiftness, Charisma, Luck}
    
    public Dictionary<Attribute, int> Attributes {get; set;} = new Dictionary<Attribute, int>()
    {
        {Attribute.Vigour, 0},
        {Attribute.Resilience, 0},
        {Attribute.Intellect, 0},
        {Attribute.Swiftness, 0},
        {Attribute.Charisma, 0},
        {Attribute.Luck, 0}
    };

    public int AttributePoints {get; set;} = 0;
    // public bool NPC {get; set;} = false;
    // public Dictionary<BattleUnitData.Attribute, int> GetAttributes()
    // {
    //     return MainCombatant.Attributes;
    // }
    // public void SetAttribute(BattleUnitData.Attribute attribute, int num)
    // {
    //     MainCombatant.Attributes[attribute] = num;
    // }

    public bool Hostile {get; set;} = false;
    public bool InitiatesDialogue {get; set;} = false;
    public bool Player {get; set;} = false;
    public List<UnitData> Companions {get; set;} = new List<UnitData>();
    public AIUnitControlState.AIBehaviour Behaviour = AIUnitControlState.AIBehaviour.Stationary;
    public List<Vector2> PatrolPoints {get; set;} = new List<Vector2>();

    // change to battle unit Data
    public BattleUnitData CurrentBattleUnitData {get; set;} = new BattleUnitData();
    public List<BattleUnitData> Minions {get; set;} = new List<BattleUnitData>();
    // public Dictionary<BattleUnitData, int> Minions {get; set;} = new Dictionary<BattleUnitData, int>();

    //

    public ExperienceManager ExperienceManager = new ExperienceManager();
    public DialogueData CurrentDialogueData = new DialogueData();

    public void StopCompanion()
    {
        Companion = false;
        Behaviour = AIUnitControlState.AIBehaviour.Stationary;
    }

    public float GetWeaponDamage()
    {

        if (CurrentBattleUnitData.WeaponEquipped != PnlInventory.ItemMode.Empty)
        {
            ItemBuilder itemBuilder = new ItemBuilder();
            WeaponItem weapon = itemBuilder.BuildWeapon(CurrentBattleUnitData.WeaponEquipped);
            return weapon.WeaponDamage;
        }
        return 0;
    }

    public float GetDamageRange()
    {
        if (CurrentBattleUnitData.WeaponEquipped != PnlInventory.ItemMode.Empty)
        {
            ItemBuilder itemBuilder = new ItemBuilder();
            WeaponItem weapon = itemBuilder.BuildWeapon(CurrentBattleUnitData.WeaponEquipped);
            return weapon.DamageRange;
        }
        return BasePhysicalDamageRange;
    }

    public float GetArmourBonus()
    {
        if (CurrentBattleUnitData.ArmourEquipped != PnlInventory.ItemMode.Empty)
        {
            ItemBuilder itemBuilder = new ItemBuilder();
            ArmourItem armour = itemBuilder.BuildArmour(CurrentBattleUnitData.ArmourEquipped);
            return armour.ArmourBonus;
        }
        return 0;
    }

    public void RemoveEquippedWeapon()
    {
        PnlInventory.ItemMode oldWeapon = CurrentBattleUnitData.WeaponEquipped;
        if (oldWeapon != PnlInventory.ItemMode.Empty)
        {
            ItemBuilder itemBuilder = new ItemBuilder();
            WeaponItem old = itemBuilder.BuildWeapon(oldWeapon);
            foreach (Attribute att in old.AttributesAffected)
            {
                Attributes[att] -= old.AttributesAffectedMagnitude;
            }
            CurrentBattleUnitData.WeaponEquipped = PnlInventory.ItemMode.Empty;
            UpdateDerivedStatsFromAttributes();
        }
    }

    public void RemoveEquippedArmour()
    {
        PnlInventory.ItemMode oldArmour = CurrentBattleUnitData.ArmourEquipped;
        if (oldArmour != PnlInventory.ItemMode.Empty)
        {
            ItemBuilder itemBuilder = new ItemBuilder();
            ArmourItem old = itemBuilder.BuildArmour(oldArmour);
            foreach (Attribute att in old.AttributesAffected)
            {
                Attributes[att] -= old.AttributesAffectedMagnitude;
            }
            CurrentBattleUnitData.ArmourEquipped = PnlInventory.ItemMode.Empty;
            UpdateDerivedStatsFromAttributes();
        }
    }
    public void RemoveEquippedAmulet()
    {
        PnlInventory.ItemMode oldAmulet = CurrentBattleUnitData.AmuletEquipped;
        if (oldAmulet != PnlInventory.ItemMode.Empty)
        {
            ItemBuilder itemBuilder = new ItemBuilder();
            AmuletItem old = itemBuilder.BuildAmulet(oldAmulet);
            foreach (Attribute att in old.AttributesAffected)
            {
                Attributes[att] -= old.AttributesAffectedMagnitude;
            }
            CurrentBattleUnitData.AmuletEquipped = PnlInventory.ItemMode.Empty;
            UpdateDerivedStatsFromAttributes();
        }
    }

    public void EquipArmour(PnlInventory.ItemMode newArmourItemMode)
    {
        if (newArmourItemMode == PnlInventory.ItemMode.Empty)
        {
            return;
        }
        RemoveEquippedArmour();
        CurrentBattleUnitData.ArmourEquipped = newArmourItemMode;
        ItemBuilder itemBuilder = new ItemBuilder();
        ArmourItem newArmour = itemBuilder.BuildArmour(newArmourItemMode);
        foreach (Attribute att in newArmour.AttributesAffected)
        {
            Attributes[att] += newArmour.AttributesAffectedMagnitude;
        }
        UpdateDerivedStatsFromAttributes();
    }
    public void EquipWeapon(PnlInventory.ItemMode newWeaponItemMode)
    {
        if (newWeaponItemMode == PnlInventory.ItemMode.Empty)
        {
            return;
        }
        RemoveEquippedWeapon();
        CurrentBattleUnitData.WeaponEquipped = newWeaponItemMode;
        ItemBuilder itemBuilder = new ItemBuilder();
        WeaponItem newWeapon = itemBuilder.BuildWeapon(newWeaponItemMode);
        foreach (Attribute att in newWeapon.AttributesAffected)
        {
            Attributes[att] += newWeapon.AttributesAffectedMagnitude;
        }
        UpdateDerivedStatsFromAttributes();
    }

    public void EquipAmulet(PnlInventory.ItemMode newAmuletItemMode)
    {
        if (newAmuletItemMode == PnlInventory.ItemMode.Empty)
        {
            return;
        }
        RemoveEquippedAmulet();
        CurrentBattleUnitData.AmuletEquipped = newAmuletItemMode;
        ItemBuilder itemBuilder = new ItemBuilder();
        AmuletItem newAmulet = itemBuilder.BuildAmulet(newAmuletItemMode);
        foreach (Attribute att in newAmulet.AttributesAffected)
        {
            Attributes[att] += newAmulet.AttributesAffectedMagnitude;
        }
        UpdateDerivedStatsFromAttributes();
    }
    
    public void SetAttributesByLevel(List<UnitData.Attribute> favouredAttributes)
    {
        Random rand = new Random();
        UnitData unitData = this;
        int pool = 60;// CurrentUnitData.CurrentBattleUnitData.Level * 60;
        if (unitData.CurrentBattleUnitData.Level >= 2)
        {
            for (int i = 2; i <= unitData.CurrentBattleUnitData.Level; i++)
            {
                pool += 5 + Convert.ToInt32(Math.Floor(i/10f));
            }
        }
        // GD.Print("\nsetting attributes for " + unitData.Name + "from pool of " + pool + " who is level " + unitData.CurrentBattleUnitData.Level);
        while (pool > 0)
        {
            List<UnitData.Attribute> atts = unitData.Attributes.Keys.ToList();
            for (int i = 0; i < atts.Count; i++)
            {
                UnitData.Attribute att = atts[rand.Next(0, atts.Count)];
                int numToAllocate = Math.Min(pool, rand.Next(0, favouredAttributes.Contains(att) ? 4 : 2));
                unitData.Attributes[att] += numToAllocate;
                pool -= numToAllocate;
                atts.Remove(att);
            }

        }
        // foreach (UnitData.Attribute att in unitData.Attributes.Keys)
        // {
        //     GD.Print(att + ": " + unitData.Attributes[att]);
        // }

        unitData.UpdateDerivedStatsFromAttributes();
    }

    // NEEDS BALANCING. consider applying similar formula that is used for armour, if flat increases don't work out. or maybe just for luck %
    public void UpdateDerivedStatsFromAttributes() // called pre-battle as well
    {
        UnitData unitData = this; // clean this up when get time
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Health] = unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.TotalHealth]
            = UpdateStat(unitData.Attributes[UnitData.Attribute.Vigour], 2f);
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.PhysicalDamage]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Vigour], 0.5f) + GetWeaponDamage();
        
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.PhysicalDamageRange] = GetDamageRange();

        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Mana] = unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.TotalMana]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Intellect], 2.5f);
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.ManaRegen]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Intellect], 0.25f);
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.SpellDamage]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Intellect], 0.75f);
        
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Dodge]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Swiftness], 0.2f);
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Speed] = unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.CurrentAP]
            = (float) Math.Floor(UpdateStat(unitData.Attributes[UnitData.Attribute.Swiftness], 0.6f));
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Initiative]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Swiftness], 1f);

        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Leadership]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Charisma], 1f);

        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.CriticalChance]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Luck], 0.3f);
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.Dodge]
            *= Math.Max(1,UpdateStat(unitData.Attributes[UnitData.Attribute.Luck], 0.2f));

        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.HealthRegen]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Resilience], 0.2f);
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.MagicResist]
            =  UpdateStat(unitData.Attributes[UnitData.Attribute.Resilience], 0.3f);
        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.ManaRegen]
            *= Math.Max(1,UpdateStat(unitData.Attributes[UnitData.Attribute.Resilience], 0.2f));

        unitData.CurrentBattleUnitData.Stats[BattleUnitData.DerivedStat.PhysicalResist]
            =  UpdateStat(GetArmourBonus(), 1f);
        // GD.Print("\n" + unitData.Name + ": ");
        // if (unitData.ID == "khepri")
        // foreach (BattleUnitData.DerivedStat stat in unitData.CurrentBattleUnitData.Stats.Keys)
        // {
        //     GD.Print(stat + ": " + unitData.CurrentBattleUnitData.Stats[stat]);
        // }
    }



    private float UpdateStat(float att, float multiplier) // higher multiplier = higher result
    {
        return att*multiplier / (1 + (att * 0.025f));
        // multiple att at the divisor by a higher number for greater diminishing returns (reduced power at higher numbers) (start at 0.025)
    }

}

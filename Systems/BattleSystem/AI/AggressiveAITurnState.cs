using Godot;
using System.Collections.Generic;
using System.Linq;
using System;

public class AggressiveAITurnState : AITurnState
{
    
    public AggressiveAITurnState()
    {
        
    }
    public AggressiveAITurnState(AITurnHandler aITurnHandler)
    {
        this.AITurnHandler = aITurnHandler;
    }

    public async override void DoAITurn(CntBattle cntBattle)
    {
        base.DoAITurn(cntBattle);
        
        // first try casting a spell as this is most powerful -DONE
        if (SuccessCastingSpell(cntBattle))
        {
            // GD.Print("casted spell 1");
            return;
        }

        // then try attacking if someone is adjacent -DONE
        if (SuccessAttackingMelee(cntBattle))
        {
            return;
        }

        // then try walking halfway towards someone
        if (SuccessMoving(cntBattle, false))
        {
            while (!cntBattle.AreAllUnitsIdle())
            {
                await cntBattle.ToSignal(cntBattle.GetTree(), "idle_frame");
                // GD.Print("waiting mov half");
            }
            // then we have AP left, so try actions again
            if (SuccessCastingSpell(cntBattle)) //-DONE
            {
                return;
            }
            if (SuccessAttackingMelee(cntBattle)) // DONE
            {
                return;
            }
        }

        // try using a potion
        if (SuccessUsingPotion(cntBattle))
        {
            return;
        }

        // if we have AP left after moving.. then teleport if we have the spell
        if (SuccessCastingTeleport(cntBattle)) // DONE
        {
            return;
        }
        // otherwise, just walk the rest towards someone
        if (SuccessMoving(cntBattle, true))
        {
            // while (!cntBattle.AreAllUnitsIdle())
            // {
            //     await cntBattle.ToSignal(cntBattle.GetTree(), "idle_frame");
            //     // GD.Print("waiting mov half");
            return;
            // }
        }

        // if all else fails, skip turn:
        cntBattle.OnBtnEndTurnPressed();
    }

    private bool SuccessUsingPotion(CntBattle cntBattle)
    {
        BattleUnitData currentUnitData = cntBattle.GetActiveBattleUnit().CurrentBattleUnitData;
        List<PnlInventory.ItemMode> potions = currentUnitData.GetPotionsEquipped();
        // first prioritise health or mana if these are low
        foreach (PnlInventory.ItemMode pot in potions)
        {
            if (pot == PnlInventory.ItemMode.HealthPot && currentUnitData.Stats[BattleUnitData.DerivedStat.Health] < currentUnitData.Stats[BattleUnitData.DerivedStat.TotalHealth])
            {
                cntBattle.OnPnlPotionPotionSelected(pot);
                return true;
            }
            else if (pot == PnlInventory.ItemMode.ManaPot && currentUnitData.Stats[BattleUnitData.DerivedStat.Mana] < currentUnitData.Stats[BattleUnitData.DerivedStat.TotalMana])
            {
                cntBattle.OnPnlPotionPotionSelected(pot);
                return true;
            }
        }
        // then go through the rest and use whichever as long as not health or mana
        foreach (PnlInventory.ItemMode pot in potions)
        {
            GD.Print(Enum.GetName(typeof(PnlInventory.ItemMode),pot));
            if (pot != PnlInventory.ItemMode.HealthPot && pot != PnlInventory.ItemMode.ManaPot)
            {
                cntBattle.OnPnlPotionPotionSelected(pot);
                return true;
            }
        }
        return false;
    }

    private bool SuccessAttackingMelee(CntBattle cntBattle)
    {
        Vector2 activeUnitGridPos = cntBattle.CurrentBattleGrid.GetCorrectedGridPosition(cntBattle.GetActiveBattleUnit().GlobalPosition);
        List<BattleUnit> adjacentUnits = cntBattle.GetNeighbouringBattleUnits(activeUnitGridPos);
        foreach (BattleUnit battleUnit in adjacentUnits)
        {
            Vector2 targetUnitGridPos = cntBattle.CurrentBattleGrid.GetCorrectedGridPosition(battleUnit.GlobalPosition);
            // teleport next to human controlled unit
            if (battleUnit.CurrentBattleUnitData.PlayerFaction)
            {
                if (cntBattle.PermittedAttack(targetUnitGridPos))
                {
                    cntBattle.OnLeftClickMelee(targetUnitGridPos);
                    return true;
                }
            }
        }
        return false;
    }

    private bool SuccessMoving(CntBattle cntBattle, bool fullMovement)
    {
        Vector2 gridPos = cntBattle.CurrentBattleGrid.GetCorrectedGridPosition(cntBattle.GetActiveBattleUnit().GlobalPosition);
        List<Vector2> possibleGridDestinations = new List<Vector2>();
        // get list of possible destinations at half total AP
        foreach (Vector2 cell in cntBattle.CurrentBattleGrid.TraversableCells)
        {
            if (cntBattle.PermittedMove(gridPos, cell))
            {
                float abilAPCost = cntBattle.GetUnitSpeed(cntBattle.GetActiveBattleUnit())/2f;
                if (cntBattle.CurrentBattleGrid.GetDistanceToPoint(gridPos, cell) <= cntBattle.GetUnitAP(cntBattle.GetActiveBattleUnit())
                    - (fullMovement ? 0 : abilAPCost))
                {
                    possibleGridDestinations.Add(cell);
                }
            }
        }
        if (possibleGridDestinations.Count == 0)
        {
            return false; // stop if no possible destinations
        }
        // get closest enemy unit
        List<Vector2> enemyUnitPositions = new List<Vector2>();
        foreach (BattleUnit battleUnit in cntBattle.GetBattleUnits())
        {
            if (battleUnit.CurrentBattleUnitData.PlayerFaction)
            {
                // GD.Print(battleUnit.CurrentBattleUnitData.Name);
                enemyUnitPositions.Add(cntBattle.CurrentBattleGrid.GetCorrectedGridPosition(battleUnit.GlobalPosition));
            }
        }
        enemyUnitPositions = enemyUnitPositions.OrderBy(x => cntBattle.CurrentBattleGrid.GetDistanceToPointNoTargetObstacle(gridPos, x)).ToList();

        //
        // remove unreachable points
        foreach (Vector2 p in enemyUnitPositions.ToList())
        {
            if (cntBattle.CurrentBattleGrid.GetDistanceToPointNoTargetObstacle(gridPos, p) == -1)
            {
                enemyUnitPositions.Remove(p);
            }
            // GD.Print("distance to enemy: ", cntBattle.BattleGrid.GetDistanceToPointNoUnitObstacles(gridPos, p));
        }
        //

        if (enemyUnitPositions.Count == 0)
        {
            return false; // stop if no enemy positions
        }
        Vector2 closestEnemyPosition = enemyUnitPositions[0];
        // GD.Print(closestEnemyPosition);

        // get the closest grid destination to the closest enemy
        possibleGridDestinations = possibleGridDestinations.OrderBy(x => cntBattle.CurrentBattleGrid.GetDistanceToPointNoTargetObstacle(x, closestEnemyPosition)).ToList();
        // re-order by whichever is neighbouring
        possibleGridDestinations = possibleGridDestinations.OrderBy(x => !cntBattle.CurrentBattleGrid.GetHorizontalNeighbours(closestEnemyPosition).Contains(x)).ToList();

        Vector2 closestGridDestinationToEnemy = possibleGridDestinations[0];
        // GD.Print(closestGridDestinationToEnemy);


        if (cntBattle.PermittedMove(gridPos, closestGridDestinationToEnemy))
        {
            cntBattle.OnLeftClickMove(closestGridDestinationToEnemy, fullMovement);
            return true;
        }
        return false;
    }

    private bool SuccessMovingRemaining(CntBattle cntBattle)
    {
        return false;
    }

    // do this logic for when we have no targets for our spells, and we cant get close enough (half AP) to attack someone
    private bool SuccessCastingTeleport(CntBattle cntBattle)
    {
        SpellEffectManager.SpellMode[] spells = new SpellEffectManager.SpellMode[2] {
            cntBattle.GetActiveBattleUnit().CurrentBattleUnitData.Spell1, cntBattle.GetActiveBattleUnit().CurrentBattleUnitData.Spell2
        };
        foreach (SpellEffectManager.SpellMode spell in spells)
        {
            if (spell == SpellEffectManager.SpellMode.Teleport)
            {
                foreach (Vector2 cell in cntBattle.CurrentBattleGrid.GetAllCells())
                {
                    List<BattleUnit> adjacentUnits = cntBattle.GetNeighbouringBattleUnits(cell);

                    foreach (BattleUnit battleUnit in adjacentUnits)
                    {
                        // teleport next to human controlled unit
                        if (battleUnit.CurrentBattleUnitData.PlayerFaction)
                        {
                            if (cntBattle.PermittedSpell(spell, cell))
                            {
                                cntBattle.CurrentSpellEffectManager.SpellMethods[spell]
                                    (cntBattle.GetActiveBattleUnit(),
                                    cntBattle.GetBattleUnitAtGridPosition(cell),
                                    cntBattle.GetBattleUnitsAtArea(cntBattle.CurrentSpellEffectManager.SpellEffects[spell][0].AreaSquares, cell),
                                    cntBattle.CurrentBattleGrid.GetCorrectedWorldPosition(cell));
                                    return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    private bool SuccessCastingSpell(CntBattle cntBattle)
    {
        SpellEffectManager.SpellMode[] spells = new SpellEffectManager.SpellMode[2] {
            cntBattle.GetActiveBattleUnit().CurrentBattleUnitData.Spell1, cntBattle.GetActiveBattleUnit().CurrentBattleUnitData.Spell2
        };
        foreach (SpellEffectManager.SpellMode spell in spells)
        {
            // we sort out teleport elsewhere
            if (spell == SpellEffectManager.SpellMode.Teleport)
            {
                continue;
            }

            if (spell == SpellEffectManager.SpellMode.Empty) // if we dont have a spell, do something else
            {
                continue;
            }

            // AOE spells
            if (spell == SpellEffectManager.SpellMode.HymnOfTheUnderworld || cntBattle.CurrentSpellEffectManager.SpellEffects[spell][0].Target == SpellEffect.TargetMode.Area)
            {
                foreach (Vector2 cell in cntBattle.CurrentBattleGrid.GetAllCells())
                {
                    List<BattleUnit> battleUnitsAtArea = cntBattle.GetBattleUnitsAtArea(cntBattle.CurrentSpellEffectManager.SpellEffects[spell][0].AreaSquares, cell);
                    bool bad = false; // avoid hitting allies
                    foreach (BattleUnit battleUnit in battleUnitsAtArea)
                    {
                        if (!battleUnit.CurrentBattleUnitData.PlayerFaction)
                        {
                            bad = true;
                        }
                    }
                    // dont avoid if lunar blast as it heals allies
                    if (bad && spell != SpellEffectManager.SpellMode.LunarBlast)
                    {
                        continue;
                    }
                    foreach (BattleUnit battleUnit in battleUnitsAtArea)
                    {
                        if (battleUnit.CurrentBattleUnitData.PlayerFaction)
                        {
                            if (cntBattle.PermittedSpell(spell, cell))
                                {
                                    cntBattle.CurrentSpellEffectManager.SpellMethods[spell]
                                    (cntBattle.GetActiveBattleUnit(),
                                    cntBattle.GetBattleUnitAtGridPosition(cell),
                                    cntBattle.GetBattleUnitsAtArea(cntBattle.CurrentSpellEffectManager.SpellEffects[spell][0].AreaSquares, cell),
                                    cntBattle.CurrentBattleGrid.GetCorrectedWorldPosition(cell));
                                    return true;
                                }
                        }
                    }
                }
            }

            // for hostile
            if (cntBattle.CurrentSpellEffectManager.SpellEffects[spell][0].Target == SpellEffect.TargetMode.Hostile)
            {
                foreach (BattleUnit battleUnit in cntBattle.GetBattleUnits())
                {
                    if (battleUnit.CurrentBattleUnitData.PlayerFaction)
                    {
                        Vector2 gridPos = cntBattle.CurrentBattleGrid.GetCorrectedGridPosition(battleUnit.GlobalPosition);
                        if (!cntBattle.GetBattleUnitAtGridPosition(gridPos).CurrentBattleUnitData.CurrentStatusEffects.ContainsKey(spell))
                        {
                            if (cntBattle.PermittedSpell(spell, gridPos))
                            {
                                cntBattle.CurrentSpellEffectManager.SpellMethods[spell]
                                (cntBattle.GetActiveBattleUnit(),
                                cntBattle.GetBattleUnitAtGridPosition(gridPos),
                                cntBattle.GetBattleUnitsAtArea(cntBattle.CurrentSpellEffectManager.SpellEffects[spell][0].AreaSquares, gridPos),
                                cntBattle.CurrentBattleGrid.GetCorrectedWorldPosition(gridPos));
                                return true;
                            }
                        }
                    }
                }
            }
            // for buff spells
            if (cntBattle.CurrentSpellEffectManager.SpellEffects[spell][0].Target == SpellEffect.TargetMode.Ally)
            {
                foreach (BattleUnit battleUnit in cntBattle.GetBattleUnits())
                {
                    if (!battleUnit.CurrentBattleUnitData.PlayerFaction)
                    {
                        Vector2 gridPos = cntBattle.CurrentBattleGrid.GetCorrectedGridPosition(battleUnit.GlobalPosition);
                        if (!cntBattle.GetBattleUnitAtGridPosition(gridPos).CurrentBattleUnitData.CurrentStatusEffects.ContainsKey(spell))
                        {
                            if (cntBattle.PermittedSpell(spell, gridPos))
                            {
                                cntBattle.CurrentSpellEffectManager.SpellMethods[spell]
                                (cntBattle.GetActiveBattleUnit(),
                                cntBattle.GetBattleUnitAtGridPosition(gridPos),
                                cntBattle.GetBattleUnitsAtArea(cntBattle.CurrentSpellEffectManager.SpellEffects[spell][0].AreaSquares, gridPos),
                                cntBattle.CurrentBattleGrid.GetCorrectedWorldPosition(gridPos));
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }
}

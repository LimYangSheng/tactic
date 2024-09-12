using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitBase
{
    // runtime unit data
    private Vector3Int tilePosition;
    public Vector3Int TilePosition { get => tilePosition; set => tilePosition = value; }

    private int currentHealth;
    public int CurrentHealth { get => currentHealth; set => currentHealth = value; }

    private bool isEnemy;
    public bool IsEnemy { get => isEnemy; set => isEnemy = value; }

    private Dictionary<Vector3Int, int> tileRange = new Dictionary<Vector3Int, int>();
    public Dictionary<Vector3Int, int> TileRange { get => tileRange; set => tileRange = value; }

    private HashSet<Vector3Int> moveTileRange = new HashSet<Vector3Int>();
    public HashSet<Vector3Int> MoveTileRange { get => moveTileRange; set => moveTileRange = value; }

    private HashSet<Vector3Int> attackTileRange = new HashSet<Vector3Int>();
    public HashSet<Vector3Int> AttackTileRange { get => attackTileRange; set => attackTileRange = value; }

    private List<Action> allPossibleActions = new List<Action>();
    public List<Action> AllPossibleActions { get => allPossibleActions; set => allPossibleActions = value; }

    // unit stats
    private int moveRange = 3;
    public int MoveRange { get => moveRange; }

    private int attackRange = 2;
    public int AttackRange { get => attackRange; }

    private int maxHealth = 100;
    public int MaxHealth { get => maxHealth; }

    private int attackValue = 10;
    public int AttackValue { get => attackValue; }

    // AI related variables
    private List<ActionType> allActions = new List<ActionType>() {
        ActionType.PressForward,
        ActionType.MoveToSafety,
        ActionType.NormalAttack
    };
    public List<ActionType> AllActions { get => allActions; }

    public UnitBase() 
    { 
    
    }

    public UnitBase(UnitBase unitBase)
    {
        tilePosition = unitBase.TilePosition;
        currentHealth = unitBase.CurrentHealth;
        isEnemy = unitBase.IsEnemy;
        tileRange = unitBase.TileRange;
        moveTileRange = unitBase.MoveTileRange;
        attackTileRange = unitBase.AttackTileRange;
        allPossibleActions = unitBase.AllPossibleActions;
        moveRange = unitBase.MoveRange;
        attackRange = unitBase.AttackRange;
        maxHealth = unitBase.MaxHealth;
        attackValue = unitBase.AttackValue;
        allActions = unitBase.AllActions;
    }

    public void UpdatePosition(Vector3Int tilePos)
    {
        tilePosition = tilePos;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
    }

    public void UpdateAllTileRanges(Dictionary<Vector3Int, int> updatedTileRange)
    {
        tileRange = updatedTileRange;
        moveTileRange.Clear();
        attackTileRange.Clear();

        foreach (Vector3Int tilePos in tileRange.Keys) {
            if (tileRange[tilePos] - attackRange >= 0) {
                // move range
                moveTileRange.Add(tilePos);
            }
            else {
                // attack range
                attackTileRange.Add(tilePos);
            }
        }
    }

    public bool IsInUnitMoveRange(Vector3Int tilePos)
    {
        return moveTileRange.Contains(tilePos);
    }

    public bool IsInUnitAttackRange(Vector3Int tilePos)
    {
        return attackTileRange.Contains(tilePos) || moveTileRange.Contains(tilePos);
    }
}

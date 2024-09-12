using System;
using System.Collections.Generic;
using UnityEngine;
    
public class Unit : MonoBehaviour
{
    [SerializeField]
    public HealthBar healthBar;

    private UnitBase unitBase = new UnitBase();
    public UnitBase UnitBase { get => unitBase; }

    public Vector3Int TilePosition { get => unitBase.TilePosition; set => unitBase.TilePosition = value; }

    public int CurrentHealth { get => unitBase.CurrentHealth; set => unitBase.CurrentHealth = value; }

    public bool IsEnemy { get => unitBase.IsEnemy; set => unitBase.IsEnemy = value; }
    public Dictionary<Vector3Int, int> TileRange { get => unitBase.TileRange; set => unitBase.TileRange = value; }
    public HashSet<Vector3Int> MoveTileRange { get => unitBase.MoveTileRange; set => unitBase.MoveTileRange = value; }
    public HashSet<Vector3Int> AttackTileRange { get => unitBase.AttackTileRange; set => unitBase.AttackTileRange = value; }
    public List<Action> AllPossibleActions { get => unitBase.AllPossibleActions; set => unitBase.AllPossibleActions = value; }

    // unit stats
    public int MoveRange { get => unitBase.MoveRange; }

    public int AttackRange { get => unitBase.AttackRange; }

    public int MaxHealth { get => unitBase.MaxHealth; }

    public int AttackValue { get => unitBase.AttackValue; }

    public List<ActionType> AllActions { get => unitBase.AllActions; }

    private void Start()
    {
        CurrentHealth = MaxHealth;
        healthBar.Setup(MaxHealth);
    }

    // to reflect changes from board state computation
    public void UpdateUnitDisplay()
    {
        // update health bar
        healthBar.UpdateHealthBar(CurrentHealth);

        // update displayed position of unit
        Vector3 worldPos = TilemapManager.Instance.GetWorldPosFromTilePos(TilePosition);
        this.transform.position = worldPos;
    }

    public void UpdatePosition(Vector3Int tilePos, Vector3 worldPos) 
    {
        TilePosition = tilePos;
        this.transform.position = worldPos;
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        healthBar.UpdateHealthBar(CurrentHealth);
    }

    public void UpdateAllTileRanges(Dictionary<Vector3Int, int> updatedTileRange)
    { 
        TileRange = updatedTileRange;
        MoveTileRange.Clear();
        AttackTileRange.Clear();

        foreach (Vector3Int tilePos in TileRange.Keys) {
            if (TileRange[tilePos] - AttackRange >= 0) {
                // move range
                MoveTileRange.Add(tilePos);
            }
            else { 
                // attack range
                AttackTileRange.Add(tilePos);
            }
        }
    }

    public bool IsInUnitMoveRange(Vector3Int tilePos)
    {
        return MoveTileRange.Contains(tilePos);
    }

    public bool IsInUnitAttackRange(Vector3Int tilePos)
    {
        return AttackTileRange.Contains(tilePos) || MoveTileRange.Contains(tilePos);
    }
}

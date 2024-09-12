using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AIBoardState
{
    private List<UnitBase> playerUnits = new List<UnitBase>();
    public List<UnitBase> PlayerUnits { get => playerUnits; set => playerUnits = value; }

    private List<UnitBase> enemyUnits = new List<UnitBase>();
    public List<UnitBase> EnemyUnits { get => enemyUnits; set => enemyUnits = value; }

    public void CreateBoardStateRef(List<Unit> playerUnitsRef, List<Unit> enemyUnitsRef)
    {
        foreach (Unit unit in playerUnitsRef) {
            playerUnits.Add(unit.UnitBase);
        }
        foreach (Unit unit in enemyUnitsRef) {
            enemyUnits.Add(unit.UnitBase);
        }
    }

    public void CloneBoardState(AIBoardState aiboardStateRef)
    {
        foreach (UnitBase unitBase in aiboardStateRef.PlayerUnits) {
            UnitBase clone = new UnitBase(unitBase);
            playerUnits.Add(clone);
        }
        foreach (UnitBase unitBase in aiboardStateRef.EnemyUnits) {
            UnitBase clone = new UnitBase(unitBase);
            enemyUnits.Add(clone);
        }
    }
}

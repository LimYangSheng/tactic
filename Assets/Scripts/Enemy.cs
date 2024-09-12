using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private static Enemy instance;
    public static Enemy Instance { get => instance; }

    private List<Unit> enemyUnits = new List<Unit>();
    public List<Unit> EnemyUnits { get => enemyUnits; }

    void Awake()
    {
        if (instance == null) {
            instance = this;
        }
    }

    public void DeployUnit(Unit unit, Vector3Int tilePos, Vector3 worldPos) 
    {
        if (!enemyUnits.Contains(unit)) {
            enemyUnits.Add(unit);
            unit.UpdatePosition(tilePos, worldPos);
        }
    }

    public void UpdateAllUnitDisplay()
    {
        foreach (Unit unit in enemyUnits) {
            unit.UpdateUnitDisplay();
        }
    }
}

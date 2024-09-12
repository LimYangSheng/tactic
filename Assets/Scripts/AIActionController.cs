using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AIActionController
{
    private static AIActionController instance;
    public static AIActionController Instance {
        get {
            if (instance == null) {
                instance = new AIActionController();
            }
            return instance;
        }
    }

    private AIBoardState aiBoardState;
    public AIBoardState AIBoardState { get => aiBoardState; set => aiBoardState = value; }

    public void MoveUnit(int unitIdx, Vector3Int tilePos)
    {
        UnitBase enemyUnitBase = aiBoardState.EnemyUnits[unitIdx];
        enemyUnitBase.UpdatePosition(tilePos);
    }

    public void NormalAttack(int sourceIdx, int targetIdx)
    {
        UnitBase enemyUnit = aiBoardState.EnemyUnits[sourceIdx];
        UnitBase playerUnit = aiBoardState.PlayerUnits[targetIdx];
        playerUnit.TakeDamage(enemyUnit.AttackValue);
    }
}

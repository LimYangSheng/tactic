using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AIEnemy : MonoBehaviour
{
    private static AIEnemy instance;
    public static AIEnemy Instance { get => instance; }

    private AIBoardState aiBoardStateRef = new AIBoardState();

    private bool isComputing = false;
    public bool IsComputing { get => isComputing; }

    private float bestScore = float.NegativeInfinity;
    private List<Action> bestActions = new List<Action>();

    void Awake()
    {
        if (instance == null) {
            instance = this;
        }
    }

    public void ComputeAndExecuteEnemiesBestAction()
    {
        isComputing = true;
        aiBoardStateRef.CreateBoardStateRef(Player.Instance.PlayerUnits, Enemy.Instance.EnemyUnits);

        // get all possible actions by all enemies
        Debug.Log("Computing all possible actions");
        ComputeAllEnemiesPossibleActions();

        // permutate all possible actions and get best score
        RecurseSimulateEnemiesActions(0, new List<Action>());
        Debug.Log("Finish simulation");

        // clear all possible actions
        ClearEnemiesAllPossibleActions();

        // execute computed best actions
        ExecuteEnemiesBestAction();

        // reset ai enemy variables
        ResetAIEnemy();

        // signal completion of ai computation
        isComputing = false;
    }

    private void RecurseSimulateEnemiesActions(int currIdx, List<Action> currentActions)
    {
        // if all units have taken an action, compute score
        if (currIdx > aiBoardStateRef.EnemyUnits.Count - 1) {
            Debug.Log("Computing board score");
            float currentScore = ExecuteActionsAndComputeBoardScore(currentActions);
            Debug.Log("Board score is: " + currentScore);
            if (currentScore > bestScore) {
                bestScore = currentScore;
                bestActions = new List<Action>(currentActions);
            }
            return;
        }

        UnitBase enemy = aiBoardStateRef.EnemyUnits[currIdx];
        foreach (Action action in enemy.AllPossibleActions) {
            currentActions.Add(action);
            RecurseSimulateEnemiesActions(currIdx + 1, currentActions);
            currentActions.RemoveAt(currentActions.Count - 1);
        }
    }

    private float ExecuteActionsAndComputeBoardScore(List<Action> actions)
    {
        AIBoardState tempAIBoardStateCopy = new AIBoardState();
        tempAIBoardStateCopy.CloneBoardState(aiBoardStateRef);
        AIActionController.Instance.AIBoardState = tempAIBoardStateCopy;
        foreach (Action action in actions) {
            action.Invoke();
        }
        return ComputeBoardScore(tempAIBoardStateCopy);
    }

    private float ComputeBoardScore(AIBoardState aiBoardState)
    {
        float score = 0.0f;
        float totalEnemyPercentHP = 0.0f;
        foreach (UnitBase enemyUnit in aiBoardState.EnemyUnits) {
            totalEnemyPercentHP += (float)enemyUnit.CurrentHealth / (float)enemyUnit.MaxHealth;
        }
        float totalEnemyPercentHPMultiplier = 1.0f;

        float totalPlayerPercentHP = 0.0f;
        float totalPlayerPercentHPMultiplier = -2.0f;

        score = totalEnemyPercentHP * totalEnemyPercentHPMultiplier
            + totalPlayerPercentHP * totalPlayerPercentHPMultiplier;
        return score;
    }

    private void ClearEnemiesAllPossibleActions()
    {
        foreach (UnitBase enemyUnit in aiBoardStateRef.EnemyUnits) {
            enemyUnit.AllPossibleActions.Clear();
        }
    }

    private void ExecuteEnemiesBestAction()
    {
        AIActionController.Instance.AIBoardState = aiBoardStateRef;
        foreach (Action action in bestActions) {
            action.Invoke();
        }
    }

    private void ResetAIEnemy()
    {
        aiBoardStateRef = new AIBoardState();
        bestScore = float.NegativeInfinity;
        bestActions = new List<Action>();
    }

    #region Functions to generate possible actions 
    private void ComputeAllEnemiesPossibleActions()
    {
        // go through each enemy unit
        for (int enemyUnitIdx = 0; enemyUnitIdx < aiBoardStateRef.EnemyUnits.Count; enemyUnitIdx++) {
            // get all their possible actions
            UnitBase enemyUnit = aiBoardStateRef.EnemyUnits[enemyUnitIdx];
            foreach (ActionType actionType in enemyUnit.AllActions) {
                switch (actionType) {
                    case ActionType.PressForward:
                        ComputePressForward(enemyUnitIdx, enemyUnit, aiBoardStateRef.PlayerUnits);
                        break;
                    case ActionType.MoveToSafety:
                        ComputeMoveToSafety(enemyUnitIdx, enemyUnit, aiBoardStateRef.PlayerUnits);
                        break;
                    case ActionType.NormalAttack:
                        ComputeNormalAttack(enemyUnitIdx, enemyUnit, aiBoardStateRef.PlayerUnits);
                        break;
                }
            }
        }
    }

    private void ComputePressForward(int enemyUnitIdx, UnitBase enemyUnit, List<UnitBase> playerUnits)
    {
        // find closest enemy and move towards them
        Vector3Int bestTilePos = enemyUnit.TilePosition;
        float shortestDistance = 0.0f;
        float currentDistance = 0.0f;
        foreach (Vector3Int movableTiles in enemyUnit.MoveTileRange) {
            foreach (UnitBase playerUnit in playerUnits) {
                currentDistance = CalculateTileDistance(playerUnit.TilePosition, movableTiles);
            }
            if (currentDistance < shortestDistance) {
                shortestDistance = currentDistance;
                bestTilePos = movableTiles;
            }
        }

        if (bestTilePos != enemyUnit.TilePosition) {
            List<Action> possibleActions = new List<Action>();
            possibleActions.Add(() => {
                AIActionController.Instance.MoveUnit(enemyUnitIdx, bestTilePos);
            });
            enemyUnit.AllPossibleActions.AddRange(possibleActions);
        }
    }

    private void ComputeMoveToSafety(int enemyUnitIdx, UnitBase enemyUnit, List<UnitBase> playerUnits)
    {
        HashSet<Vector3Int> safeTiles = new HashSet<Vector3Int>(enemyUnit.TileRange.Keys);

        // remove all tiles that can be reached by player
        foreach (Vector3Int movableTiles in enemyUnit.MoveTileRange) {
            foreach (UnitBase playerUnit in playerUnits) {
                if (playerUnit.IsInUnitAttackRange(movableTiles)) {
                    safeTiles.Remove(movableTiles);
                }
            }
        }

        // find tile that is closest to all enemies
        Vector3Int bestTilePos = enemyUnit.TilePosition;
        float longestDistance = 0.0f;
        float totalDistance;
        foreach (Vector3Int safeTile in safeTiles) {
            totalDistance = 0.0f;
            foreach (UnitBase playerUnit in playerUnits) {
                totalDistance += CalculateTileDistance(playerUnit.TilePosition, safeTile);
            }
            if (totalDistance > longestDistance) {
                longestDistance = totalDistance;
                bestTilePos = safeTile;
            }
        }

        if (bestTilePos != enemyUnit.TilePosition) {
            List<Action> possibleActions = new List<Action>();
            possibleActions.Add(() => {
                AIActionController.Instance.MoveUnit(enemyUnitIdx, bestTilePos);
            });
            enemyUnit.AllPossibleActions.AddRange(possibleActions);
        }
    }

    private void ComputeNormalAttack(int enemyUnitIdx, UnitBase enemyUnit, List<UnitBase> playerUnits)
    {
        List<Action> possibleActions = new List<Action>(); 
        for (int playerUnitIdx = 0; playerUnitIdx < playerUnits.Count; playerUnitIdx++) {
            UnitBase playerUnit = playerUnits[playerUnitIdx];
            if (enemyUnit.IsInUnitAttackRange(playerUnit.TilePosition)) {
                int playerUnitIdxVal = playerUnitIdx;
                possibleActions.Add(() => {
                    AIActionController.Instance.NormalAttack(enemyUnitIdx, playerUnitIdxVal);
                });
            }
        }
        enemyUnit.AllPossibleActions.AddRange(possibleActions);
    }

    #endregion

    #region Utility
    private float CalculateTileDistance(Vector3Int tile1, Vector3Int tile2)
    {
        return Vector3Int.Distance(tile1, tile2);
    }
    #endregion
}

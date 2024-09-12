using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get => instance; }

    private GamePhase currentPhase = GamePhase.GenerateMap;
    public GamePhase CurrentPhase { get => currentPhase; }

    void Awake()
    {
        if (instance == null) {
            instance = this;
        }
    }

    // Update is called once per frame
    void Update()
	{
        switch (currentPhase) {
            case GamePhase.GenerateMap:
                MapGenerator.Instance.GenerateMap();
                currentPhase = GamePhase.PlayerPreparation;
                break;
            case GamePhase.PlayerPreparation:
                if (Input.GetKeyUp(KeyCode.Return)) {
                    currentPhase = GamePhase.UpdateBoardState;
                }
                break;
            case GamePhase.UpdateBoardState:
                TilemapManager.Instance.ComputeAllUnitsTileRange();
                currentPhase = GamePhase.PlayerTurn;
                Debug.Log("Player Turn");
                break;
            case GamePhase.PlayerTurn:
                if (Input.GetKeyUp(KeyCode.Return)) {
                    currentPhase = GamePhase.EnemyTurn;
                    Debug.Log("Enemy Turn");
                }
                break;
            case GamePhase.EnemyTurn:
                if (!AIEnemy.Instance.IsComputing) {
                    TilemapManager.Instance.ComputeAllUnitsTileRange();
                    AIEnemy.Instance.ComputeAndExecuteEnemiesBestAction();
                    Enemy.Instance.UpdateAllUnitDisplay();
                    currentPhase = GamePhase.PlayerTurn;
                }
                break;
            default:
                break;
        }
    }
}

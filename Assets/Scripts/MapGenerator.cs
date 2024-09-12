using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MapGenerator : MonoBehaviour
{
    private static MapGenerator instance;
    public static MapGenerator Instance { get => instance; }

    List<Vector3Int> spawnPoints = new List<Vector3Int>();

    void Awake()
    {
        if (instance == null) {
            instance = this;
        }
    }

    public void GenerateMap()
    {
        GenerateEnemySpawnPoints();
        GenerateEnemies();
    }

    private void GenerateEnemySpawnPoints()
    { 
        spawnPoints.Add(new Vector3Int(8, 1, 0));
        spawnPoints.Add(new Vector3Int(7, -2, 0));
    }

    private void GenerateEnemies()
    {
        foreach (Vector3Int tilePosition in spawnPoints) {
            Unit enemyUnit = Instantiate(Resources.Load("Slime", typeof(GameObject))).GetComponent<Unit>();
            enemyUnit.IsEnemy = true;
            Vector3 worldPosition = TilemapManager.Instance.GetWorldPosFromTilePos(tilePosition);
            Enemy.Instance.DeployUnit(enemyUnit, tilePosition, worldPosition);
        }
    }

    private List<T> GetRandomUniqueNFromList<T>(List<T> items, int n)
    {
        if (n >= items.Count) {
            return items;
        }

        List<T> randomItems = new List<T>();
        for (int i = 0; i < 2; i++) { 
            int randIdx = Random.Range(0, items.Count);
            randomItems.Add(items[randIdx]);
            items.Remove(items[randIdx]);
        }
        return randomItems;
    }
}

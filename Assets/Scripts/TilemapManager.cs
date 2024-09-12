using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour 
{
    enum ClickedType { 
        NoClick,
        PlayerUnit,
        EnemyUnit,
        Tile
    }

    private static TilemapManager instance;

    public static TilemapManager Instance { get => instance; }

    [SerializeField]
    private Tilemap floorTilemap;

    [SerializeField]
    private Tilemap uiTilemap;
    private Vector3Int lastHoverPosition;

    [SerializeField]
    Tile hoverTile;

    private ClickedType currentClickedType = ClickedType.NoClick;
    private Unit clickedUnit = null;

    // ui tile changes variables
    Color32 moveRangeColor = new Color32(204, 255, 255, 255);
    Color32 attackRangeColor = new Color32(255, 135, 135, 255);

    // ui tile changes memoization
    private Dictionary<Vector3Int, int> uiTileMemo = new Dictionary<Vector3Int, int>();
    private Queue<Vector3Int> uiTileQueue = new Queue<Vector3Int>();

    public void Awake() 
    {
        if (instance == null) {
            instance = this;
        }
    }

    public void Update() 
    {
        currentClickedType = ClickedType.NoClick;
        Vector3Int tilePosition = GetMouseTilePosition();
        Vector3 worldPosition = GetWorldPosFromTilePos(tilePosition);
        //Debug.Log(tilePosition);

        // update hover tile ui
        if (tilePosition != lastHoverPosition) {
            // update hover highlight
            uiTilemap.SetTile(lastHoverPosition, null);
            uiTilemap.SetTile(tilePosition, hoverTile);
            lastHoverPosition = tilePosition;
        }

        if (Input.GetMouseButtonDown(0)) {
            GameObject clickedObject = GetColliderObjectFromMouseClick(worldPosition);
            if (clickedObject != null) {
                if (clickedObject.GetComponent<Unit>() != null) {
                    clickedUnit = clickedObject.GetComponent<Unit>();
                    if (clickedUnit.IsEnemy) {
                        currentClickedType = ClickedType.EnemyUnit;
                    }
                    else {
                        currentClickedType = ClickedType.PlayerUnit;
                    }
                }
            }
            else {
                currentClickedType = ClickedType.Tile;
            }
        }

        if (GameManager.Instance.CurrentPhase == GamePhase.PlayerPreparation) {
            // on-click tile
            if (currentClickedType == ClickedType.Tile) {
                Unit unit = Instantiate(Resources.Load("Slime", typeof(GameObject))).GetComponent<Unit>();
                Player.Instance.DeployUnit(unit, tilePosition, worldPosition);
            }
        }

        if (GameManager.Instance.CurrentPhase == GamePhase.PlayerTurn) {
            // update ui to show unit range
            if (Player.Instance.IsUnitSelected()) {
                Unit selectedUnit = Player.Instance.SelectedUnit;
                UpdateUIUnitRange(selectedUnit);
            }

            // on-click player unit
            if (currentClickedType == ClickedType.PlayerUnit) {
                if (!Player.Instance.IsUnitSelected()) {
                    Player.Instance.SelectedUnit = clickedUnit;
                }
            }

            // on-click enemy unit
            if (currentClickedType == ClickedType.EnemyUnit) {
                if (Player.Instance.IsUnitSelected()) {
                    Unit playerSelectedUnit = Player.Instance.SelectedUnit;

                    if (playerSelectedUnit.IsInUnitAttackRange(tilePosition)) {
                        Unit enemyUnit = clickedUnit;
                        enemyUnit.TakeDamage(playerSelectedUnit.AttackValue);

                        // loop through and reset all tiles with ui changes
                        foreach (KeyValuePair<Vector3Int, int> entry in playerSelectedUnit.TileRange) {
                            floorTilemap.SetColor(entry.Key, Color.white);
                        }
                    }
                }
            }

            // on-click tile
            if (currentClickedType == ClickedType.Tile) {
                if (Player.Instance.IsUnitSelected()) {
                    Unit playerSelectedUnit = Player.Instance.SelectedUnit;

                    // update unit position only if it is reachable
                    if (playerSelectedUnit.IsInUnitMoveRange(tilePosition)) {
                        // loop through and reset all tiles with ui changes
                        foreach (KeyValuePair<Vector3Int, int> entry in playerSelectedUnit.TileRange) {
                            floorTilemap.SetColor(entry.Key, Color.white);
                        }

                        Player.Instance.UpdateSelectedUnitPosition(tilePosition, worldPosition);
                        UpdateUnitTileRange(playerSelectedUnit);
                    }
                }
            }
        }
    }

    private Vector3Int GetMouseTilePosition() 
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int tilePosition = floorTilemap.WorldToCell(mousePosition);
        return tilePosition;
    }

    public Vector3 GetWorldPosFromTilePos(Vector3Int tilePosition)
    {
        Vector3 worldPosition = floorTilemap.GetCellCenterWorld(tilePosition);
        return worldPosition;
    }

    private GameObject GetColliderObjectFromMouseClick(Vector3 worldPos)
    {
        RaycastHit2D hit;
        hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null) {
            GameObject clickedObject = hit.collider.gameObject;
            //Debug.Log("click name: " + clickedObject.name);
            return clickedObject;
        }
        return null;
    }

    #region Unit range functions
    public void ComputeAllUnitsTileRange()
    {
        // compute all player unit tile range
        foreach (Unit unit in Player.Instance.PlayerUnits) {
            UpdateUnitTileRange(unit);
        }

        // compute all enemy unit tile range
        foreach (Unit unit in Enemy.Instance.EnemyUnits) {
            UpdateUnitTileRange(unit);
        }
    }

    private void UpdateUnitTileRange(Unit unit)
    {
        ComputeUnitTileRange(unit);
        unit.UpdateAllTileRanges(new Dictionary<Vector3Int, int>(uiTileMemo));
        uiTileMemo.Clear();
    }

    private void ComputeUnitTileRange(Unit unit)
    {
        // add initial tile
        TryMemoAndQueueTile(unit.TilePosition, unit.MoveRange + unit.AttackRange);

        while (uiTileQueue.Count != 0) {
            Vector3Int currentTilePosition = uiTileQueue.Dequeue();
            int totalCostRemaining = uiTileMemo[currentTilePosition];

            TryMemoAndQueueTile(currentTilePosition + new Vector3Int(0, 1, 0), totalCostRemaining - 1);
            TryMemoAndQueueTile(currentTilePosition + new Vector3Int(1, 0, 0), totalCostRemaining - 1);
            TryMemoAndQueueTile(currentTilePosition + new Vector3Int(0, -1, 0), totalCostRemaining - 1);
            TryMemoAndQueueTile(currentTilePosition + new Vector3Int(-1, 0, 0), totalCostRemaining - 1);
        }
        return;
    }

    private void UpdateUIUnitRange(Unit unit)
    {
        foreach(KeyValuePair<Vector3Int, int> tileRangeInfo in unit.TileRange) {
            // update tile ui
            if (tileRangeInfo.Value - unit.AttackRange >= 0) {
                floorTilemap.SetColor(tileRangeInfo.Key, moveRangeColor);
            }
            else {
                floorTilemap.SetColor(tileRangeInfo.Key, attackRangeColor);
            }
        }
        return;
    }

    private void TryMemoAndQueueTile(Vector3Int currentTilePosition, int totalCostRemaining)
    {
        if (totalCostRemaining < 0) {
            return;
        }

        // add to memo table
        if (!uiTileMemo.ContainsKey(currentTilePosition)) {
            uiTileMemo.Add(currentTilePosition, totalCostRemaining);
            uiTileQueue.Enqueue(currentTilePosition);
        }
        else {
            // redo bfs on tile only if new value has larger cost
            if (totalCostRemaining > uiTileMemo[currentTilePosition]) {
                uiTileMemo[currentTilePosition] = totalCostRemaining;
                uiTileQueue.Enqueue(currentTilePosition);
            }
        }
        return;
    }
    #endregion
}

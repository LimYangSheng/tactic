using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class Player : MonoBehaviour 
{
    private static Player instance;
    public static Player Instance { get => instance; }

    private Unit selectedUnit = null;
    public Unit SelectedUnit { get => selectedUnit; set => selectedUnit = value; }

    private List<Unit> playerUnits = new List<Unit>();
    public List<Unit> PlayerUnits { get => playerUnits; }

    void Awake() 
    {
        if (instance == null) {
            instance = this;
        }
    }

    public void DeployUnit(Unit unit, Vector3Int tilePos, Vector3 worldPos) 
    {
        if (!playerUnits.Contains(unit)) {
            playerUnits.Add(unit);
            UpdateUnitPosition(unit, tilePos, worldPos);
        }
    }

    public void UpdateSelectedUnitPosition(Vector3Int tilePos, Vector3 worldPos) 
    {
        if (selectedUnit != null) {
            UpdateUnitPosition(selectedUnit, tilePos, worldPos);
            ResetSelectedUnit();
        }
    }

    private void UpdateUnitPosition(Unit unit, Vector3Int tilePos, Vector3 worldPos)
    {
        unit.UpdatePosition(tilePos, worldPos);
    }

    public void ResetSelectedUnit() 
    {
        selectedUnit = null;
    }

    public bool IsUnitSelected() 
    { 
        return selectedUnit != null;
    }
}

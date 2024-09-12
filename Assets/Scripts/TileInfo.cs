using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileInfo : TileBase 
{
    [SerializeField]
    private Sprite sprite;

    [SerializeField]
    private bool isTraversable;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) 
    {
        tileData.sprite = sprite;
    }

    #region Editor Menu

    [MenuItem("Assets/Create/TileInfo")]
    public static void CreateTileInformation() 
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Tile Info", "New Tile Info", "Asset", "Save Tile Info", "Assets/Tiles");
        if (path == "") {
            return;
        }
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<TileInfo>(), path);
    }
    #endregion
}

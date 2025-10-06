using System;
using System.Collections.Generic;
using UnityEngine;

public class Layer : MonoBehaviour
{
    [Header("References")]
    public Grid grid;
    public GridTileData gridTileData;

    [Header("Layer Settings")]
    public string Name = "New Layer";
    [HideInInspector] public float gridYPos = 0f;
    [HideInInspector] public float tileWidth = 1f;
    [HideInInspector] public bool settingsLocked = false;

    [Header("Properties")]
    [NonSerialized] public BrushMode currentBrushMode = BrushMode.Single;
    [NonSerialized] public Vector3 currentBrushPosition;
    [NonSerialized] public List<int> selectedIndices = new();
    [NonSerialized] public List<int> secondSelectedIndices = new();
    [NonSerialized] public List<Prefab> prefabsInUse_Unique = new();
    [NonSerialized] public List<LayerCellData> allPlacedPrefabs_Repeat = new();
}

public class LayerCellData
{
    public Vector3 position;
    public List<GameObject> placedPrefabs = new();
}

public class PlacedPrefabData
{
    public GameObject placedPrefab;
    public Prefab prefabData;
    public Vector3 positionOffset;
    public Quaternion rotation;
    public Vector3 scale;

    public PlacedPrefabData(GameObject placedPrefab, Prefab prefabData, Vector3 positionOffset, Quaternion rotation, Vector3 scale)
    {
        this.placedPrefab = placedPrefab;
        this.prefabData = prefabData;
        this.positionOffset = positionOffset;
        this.rotation = rotation;
        this.scale = scale;
    }

}
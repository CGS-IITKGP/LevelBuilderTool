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
    [HideInInspector] public float finalYPos = 0f;
    [HideInInspector] public float gridYIncrement = 1f;
    [HideInInspector] public float tileWidth = 1f;
    [HideInInspector] public bool settingsLocked = false;

    [Header("Editor Properties")]
    public int prefabTabIndex = 0;
    [NonSerialized] public BrushMode currentBrushMode = BrushMode.Single;
    [NonSerialized] public Vector3 currentBrushPosition;
    public int noOfIncrements;
    /*[NonSerialized] */  public Vector3 currentMousePosition;
    /*[NonSerialized] */  public List<int> selectedIndices = new();
    /*[NonSerialized] */  public List<int> secondSelectedIndices = new();
    /*[NonSerialized] */  public List<Prefab> allUniquePrefabsInUse = new();
    /*[NonSerialized] */  public List<LayerYLevel> layerData = new();






    public void RegisterPlacedPrefab(GameObject placed, Prefab prefab, Vector3 offsetPos, Quaternion rotation, Vector3 scale)
    {
        Vector3 pos = currentBrushPosition;
        if (!allUniquePrefabsInUse.Contains(prefab))
        {
            allUniquePrefabsInUse.Add(prefab);
        }

        PlacedPrefabData newPlaced = new PlacedPrefabData(placed, prefab, offsetPos, rotation, scale);

        AddPrefabToLayerData(newPlaced);

        void AddPrefabToLayerData(PlacedPrefabData newPlaced)
        {
            int yIndex = Mathf.RoundToInt((finalYPos - gridYPos) / gridYIncrement);
            for (int i = 0; i < layerData.Count; i++)
            {
                if (layerData[i].yIndex == yIndex)
                {
                    for (int j = 0; j < layerData[i].cells.Count; j++)
                    {
                        if (layerData[i].cells[j].position == pos)
                        {
                            layerData[i].cells[j].placedPrefabs.Add(newPlaced);
                            return;
                        }
                    }
                    LayerCellData cellData = new LayerCellData();
                    cellData.position = pos;
                    cellData.placedPrefabs.Add(newPlaced);
                    layerData[i].cells.Add(cellData);
                    return;
                }
            }
            LayerCellData newCellData = new LayerCellData();
            newCellData.position = pos;
            newCellData.placedPrefabs.Add(newPlaced);
            LayerYLevel newYLevel = new LayerYLevel();
            newYLevel.yIndex = yIndex;
            newYLevel.cells.Add(newCellData);
            layerData.Add(newYLevel);
        }
    }


    public void EraseSinglePrefab()
    {
        for (int i = 0; i < layerData[noOfIncrements].cells.Count; i++)
        {
            if (layerData[noOfIncrements].cells[i].position == currentBrushPosition)
            {
                if (layerData[noOfIncrements].cells[i].placedPrefabs.Count > 0)
                {
                    var placedPrefabs = layerData[noOfIncrements].cells[i].placedPrefabs;
                    for (int j = placedPrefabs.Count - 1; j >= 0; j--)
                    {
                        if (placedPrefabs[j].placedPrefab != null)
                        {
                            DestroyImmediate(placedPrefabs[j].placedPrefab);
                        }
                        placedPrefabs.RemoveAt(j);
                    }

                    if (layerData[noOfIncrements].cells[i].placedPrefabs.Count == 0)
                    {
                        layerData[noOfIncrements].cells.RemoveAt(i);
                    }
                }
                return;
            }
        }
    }
}

[System.Serializable]
public class LayerYLevel
{
    public int yIndex;
    public List<LayerCellData> cells = new();
}

[System.Serializable]
public class LayerCellData
{
    public Vector3 position;
    public List<PlacedPrefabData> placedPrefabs = new();
}

[System.Serializable]
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
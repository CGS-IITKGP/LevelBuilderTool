using System;
using System.Collections.Generic;
using UnityEngine;

public class Layer : MonoBehaviour
{
    [Header("References")]
    public Grid grid;
    public GridTileData gridTileData;
    public GameObject objectParentObject;

    [Header("Layer Settings")]
    public string Name = "New Layer";
    [HideInInspector] public float gridYPos = 0f;
    [HideInInspector] public float finalYPos_current = 0f;
    [HideInInspector] public float gridYIncrement = 1f;
    [HideInInspector] public float tileWidth = 1f;
    [HideInInspector] public bool settingsLocked = false;

    [Header("Editor Properties")]
    public int prefabTabIndex = 0;
    [NonSerialized] public BrushMode currentBrushMode = BrushMode.Single;
    [NonSerialized] public Vector3 currentBrushPosition;
    public int noOfIncrements;
    [NonSerialized]  public Vector3 currentMousePosition;
    [NonSerialized]  public List<int> selectedIndices = new();
    [NonSerialized]  public List<int> secondSelectedIndices = new();
    [NonSerialized]  public List<UniquePrefabData> allUniquePrefabsInUse = new();
    /*[NonSerialized] */[SerializeField]  public List<LayerYLevel> layerData = new();

    [NonSerialized] public List<LayerCellData> selectedCells = new();


    void OnEnable()
    {
        RebuildUniquePrefabList();
    }

    // Call this to regenerate the 'allUniquePrefabsInUse' list from scratch
    public void RebuildUniquePrefabList()
    {
        allUniquePrefabsInUse.Clear();

        foreach (var yLevel in layerData)
        {
            foreach (var cell in yLevel.cells)
            {
                foreach (var placedPrefab in cell.placedPrefabs)
                {
                    AddUniquePrefabData(placedPrefab.prefabData, cell);
                }
            }
        }
    }

    void AddUniquePrefabData(Prefab prefab, LayerCellData cellData)
    {
        var uniqueData = allUniquePrefabsInUse.Find(up => up.prefab == prefab);

        if (uniqueData != null)
        {
            uniqueData.count++;
            if (!uniqueData.cellsUsingThisPrefab.Contains(cellData))
            {
                uniqueData.cellsUsingThisPrefab.Add(cellData);
            }
        }
        else
        {
            UniquePrefabData newUnique = new UniquePrefabData(prefab, 1);
            newUnique.cellsUsingThisPrefab.Add(cellData);
            allUniquePrefabsInUse.Add(newUnique);
        }
    }



    public void RegisterPlacedPrefab(GameObject placed, Prefab prefab, Vector3 offsetPos, Quaternion rotation, Vector3 scale)
    {
        Vector3 pos = currentBrushPosition;

        PlacedPrefabData newPlaced = new PlacedPrefabData(placed, prefab, offsetPos, rotation, scale);

        LayerCellData cellData = AddPrefabToLayerData(newPlaced);

        AddUniquePrefabData(prefab, cellData);

        LayerCellData AddPrefabToLayerData(PlacedPrefabData newPlaced)
        {
            int yIndex = Mathf.RoundToInt((finalYPos_current - gridYPos) / gridYIncrement);
            for (int i = 0; i < layerData.Count; i++)
            {
                if (layerData[i].yIndex == yIndex)
                {
                    for (int j = 0; j < layerData[i].cells.Count; j++)
                    {
                        if (layerData[i].cells[j].position == pos)
                        {
                            layerData[i].cells[j].placedPrefabs.Add(newPlaced);
                            return layerData[i].cells[j];
                        }
                    }
                    LayerCellData cellData = new LayerCellData();
                    cellData.position = pos;
                    cellData.placedPrefabs.Add(newPlaced);
                    layerData[i].cells.Add(cellData);
                    return cellData;
                }
            }
            LayerCellData newCellData = new LayerCellData();
            newCellData.position = pos;
            newCellData.placedPrefabs.Add(newPlaced);
            LayerYLevel newYLevel = new LayerYLevel();
            newYLevel.yIndex = yIndex;
            newYLevel.cells.Add(newCellData);
            layerData.Add(newYLevel);
            return newCellData;
        }

        void AddUniquePrefabData(Prefab prefab, LayerCellData cellData)
        {
            for (int i = 0; i < allUniquePrefabsInUse.Count; i++)
            {
                if (allUniquePrefabsInUse[i].prefab == prefab)
                {
                    allUniquePrefabsInUse[i].count++;
                    if (!allUniquePrefabsInUse[i].cellsUsingThisPrefab.Contains(cellData))
                    {
                        allUniquePrefabsInUse[i].cellsUsingThisPrefab.Add(cellData);
                    }
                    return;
                }
            }
            UniquePrefabData newUnique = new UniquePrefabData(prefab, 1);
            newUnique.cellsUsingThisPrefab.Add(cellData);
            allUniquePrefabsInUse.Add(newUnique);
        }
    }


    public void EraseSinglePrefab(LayerCellData cellToDelete = null)
    {
        LayerYLevel YthLayer = null;

        for (int i = 0; i < layerData.Count; i++)
        {
            if (layerData[i].yIndex == cellToDelete.position.y)
            {
                YthLayer = layerData[i];
                break;
            }
        }


        var removingCell = YthLayer.cells.Find(cell => cell.position == currentBrushPosition);  // ===((suggested short form))
        if (cellToDelete != null)
        {
            removingCell = cellToDelete;
        }

        if (removingCell == null)
        {
            return;
        }

        if (removingCell.placedPrefabs.Count > 0)
        {
            var placedPrefabs = removingCell.placedPrefabs;
            for (int j = placedPrefabs.Count - 1; j >= 0; j--)
            {
                ReduceCountInUniquePrefabs(placedPrefabs[j]);
                if (placedPrefabs[j].placedPrefab != null)
                {
                    DestroyImmediate(placedPrefabs[j].placedPrefab);
                }
                placedPrefabs.RemoveAt(j);
            }

            if (removingCell.placedPrefabs.Count == 0)
            {
                YthLayer.cells.Remove(removingCell);
                Debug.Log("Removed cell at position: " + removingCell.position);
            }

            if (YthLayer.cells.Count == 0)
            {
                layerData.Remove(YthLayer);
            }
        }

        return;

        void ReduceCountInUniquePrefabs(PlacedPrefabData placedPrefabData)
        {
            var uniquePrefab = allUniquePrefabsInUse.Find(unique => unique.prefab == placedPrefabData.prefabData);
            
            if (uniquePrefab != null)
            {
                uniquePrefab.count--;
                if (removingCell.placedPrefabs.Find(p => p.prefabData == placedPrefabData.prefabData) == null)
                    uniquePrefab.cellsUsingThisPrefab.Remove(removingCell);
                if (uniquePrefab.count <= 0)
                {
                    allUniquePrefabsInUse.Remove(uniquePrefab);
                }
            }
        }
    }

    public void RefreshAllReferences()
    {
        for (int i = 0; i < layerData.Count; i++)
        {
            for (int j = 0; j < layerData[i].cells.Count; j++)
            {
                for (int k = layerData[i].cells[j].placedPrefabs.Count - 1; k >= 0; k--)
                {
                    if (layerData[i].cells[j].placedPrefabs[k].placedPrefab == null)
                    {
                        var prefabData = layerData[i].cells[j].placedPrefabs[k].prefabData;
                        var cellData = layerData[i].cells[j];
                        layerData[i].cells[j].placedPrefabs.RemoveAt(k);

                        var uniquePrefab = allUniquePrefabsInUse.Find(unique => unique.prefab == prefabData);
                        if (uniquePrefab != null)
                        {
                            uniquePrefab.count--;
                            if (cellData.placedPrefabs.Find(p => p.prefabData == prefabData) == null)
                                uniquePrefab.cellsUsingThisPrefab.Remove(cellData);

                            if (uniquePrefab.count <= 0)
                            {
                                allUniquePrefabsInUse.Remove(uniquePrefab);
                            }
                        }
                    }
                }
                if (layerData[i].cells[j].placedPrefabs.Count == 0)
                {
                    layerData[i].cells.RemoveAt(j);
                    j--;
                }
            }
            if (layerData[i].cells.Count == 0)
            {
                layerData.RemoveAt(i);
                i--;
            }
        }
    }
}

[System.Serializable]
public class LayerYLevel
{
    public int yIndex;
    public bool forceVisibility = false;
    public bool visibility = true;
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

[System.Serializable]
public class UniquePrefabData
{
    public Prefab prefab;
    public int count;
    public List<LayerCellData> cellsUsingThisPrefab = new();
    public UniquePrefabData(Prefab prefab, int count)
    {
        this.prefab = prefab;
        this.count = count;
    }
}
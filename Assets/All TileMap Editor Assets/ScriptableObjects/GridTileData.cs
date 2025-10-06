using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridTileData", menuName = "Scriptable Objects/GridTileData")]
public class GridTileData : ScriptableObject
{
    
    public Grid grid;

    public List<Prefab> AllPrefabs = new List<Prefab>();
    public List<GroupedPrefab> GroupedPrefabs = new List<GroupedPrefab>();

}

[System.Serializable]
public class GroupedPrefab
{
    public string groupName;
    public List<Prefab> prefabs;
    [Tooltip("How many Prefab to select from group")] 
    public int noOFDiffPrefabPerTile = 1;
    [Tooltip("Assuming 2 Prefab selected, Each one is to be placed 3 times, so total is 6 prefab of two type per tile, but this limit it by to a max value.")]
    public int maxNoOfAllPrefabsPerTile = 5;
    //public List<PrefabPropertiesInsideGroupPrefab> prefabPropertiesInsideGroupPrefabs;
}

//[System.Serializable]
//public class PrefabPropertiesInsideGroupPrefab
//{
//}

[System.Serializable]
public class Prefab
{
    public Prefab(Prefab prefabPrev)
    {
        this.prefab = prefabPrev.prefab;
        this.randomizePosition = prefabPrev.randomizePosition;
        this.specificPositions = new List<Vector3>(prefabPrev.specificPositions);
        this.positionRange = new List<Vector3>(prefabPrev.positionRange);
        this.randomizeRotation = prefabPrev.randomizeRotation;
        this.rangeRandomizationPosition = prefabPrev.rangeRandomizationPosition;
        this.specificRotations = new List<Vector3>(prefabPrev.specificRotations);
        this.rotationRange = new List<Vector3>(prefabPrev.rotationRange);
        this.rangeRandomizationRotation = prefabPrev.rangeRandomizationRotation;
        this.randomizeScale = prefabPrev.randomizeScale;
        this.scaleRange = new List<float>(prefabPrev.scaleRange);
        this.noOfPrefabPerTileMin = prefabPrev.noOfPrefabPerTileMin;
        this.noOfPrefabPerTileMax = prefabPrev.noOfPrefabPerTileMax;
    }

    public GameObject prefab;
    public bool randomizePosition = false;
    public bool rangeRandomizationPosition = false;
    public List<Vector3> specificPositions = new List<Vector3>();
    public List<Vector3> positionRange = new List<Vector3>(2);
    public bool randomizeRotation = false;
    public bool rangeRandomizationRotation = false;
    public List<Vector3> specificRotations = new List<Vector3>();
    public List<Vector3> rotationRange = new List<Vector3>(2);
    public bool randomizeScale = false;
    public List<float> scaleRange = new List<float>(2) { 1f, 1f };
    public int noOfPrefabPerTileMin = 1;
    public int noOfPrefabPerTileMax = 1;

    // only for grouping
    [Range(0f, 10f)]
    [Tooltip("Higher the value, higher the chance of selection | The value is relative to all the prefabs of only this group prefabs | Actual Chance = chance / Sum of all chance of all prefabs in group")]
    public float chanceOfSelection = 1f;

}


public enum BrushMode
{
    Single,
    Multi,
    Fill,
    Line,
    Rectangle,
    Erase
}
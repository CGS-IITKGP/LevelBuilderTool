using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridTileData", menuName = "Scriptable Objects/GridTileData")]
public class GridTileData : ScriptableObject
{
    public float GridYPos = 0f;
    public float TileWidth = 1f;

    public List<Prefab> AllPrefabs = new List<Prefab>();
    public List<GroupedPrefab> GroupedPrefabs = new List<GroupedPrefab>();

}

[System.Serializable]
public class GroupedPrefab
{
    public string groupName;
    public List<Prefab> prefabs;
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
        prefab = prefabPrev.prefab;
        randomizePos = prefabPrev.randomizePos;
        minX = prefabPrev.minX;
        maxX = prefabPrev.maxX;
        minY = prefabPrev.minY;
        maxY = prefabPrev.maxY;
        minZ = prefabPrev.minZ;
        maxZ = prefabPrev.maxZ;
        noOfPrefabPerTileMin = prefabPrev.noOfPrefabPerTileMin;
        noOfPrefabPerTileMax = prefabPrev.noOfPrefabPerTileMax;

        // only for grouping
        chanceOfSelection = prefabPrev.chanceOfSelection;
    }

    public GameObject prefab;
    public bool randomizePos = false;
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;
    public float minZ;
    public float maxZ;
    public int noOfPrefabPerTileMin = 1;
    public int noOfPrefabPerTileMax = 1;

    // only for grouping
    [Range(0f, 10f)]
    [Tooltip("Higher the value, higher the chance of selection | The value is relative to all the prefabs of only this group prefabs | Actual Chance = chance / Sum of all chance of all prefabs in group")]
    public float chanceOfSelection = 1f;

}
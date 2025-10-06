using System.Collections.Generic;
using UnityEngine;

public class Layer : MonoBehaviour
{
    public Grid grid;
    public GridTileData gridTileData;

    public string Name = "New Layer";
    [HideInInspector] public float gridYPos = 0f;
    [HideInInspector] public float tileWidth = 1f;

    [HideInInspector] public bool settingsLocked = false;
    [HideInInspector] public BrushMode currentBrushMode = BrushMode.Single;
    [HideInInspector] public Vector3 currentBrushPosition;
    public List<int> selectedIndices = new();
    public List<int> secondSelectedIndices = new();
}

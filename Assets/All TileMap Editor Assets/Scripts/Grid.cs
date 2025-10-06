using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public GridTileData gridTileData;
    public List<Layer> layers = new();
    public int selectedLayer = 0;

    //temp
    public int startX = -10;
    public int startZ = -10;
    public int endX = 10;
    public int endZ = 10;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        for (int i = startX; i <= endX; i++)
        {
            Vector3 from = new Vector3(i * layers[selectedLayer].tileWidth, layers[selectedLayer].gridYPos, startZ * layers[selectedLayer].tileWidth);
            Vector3 to = new Vector3(i * layers[selectedLayer].tileWidth, layers[selectedLayer].gridYPos, endZ * layers[selectedLayer].tileWidth);
            Gizmos.DrawLine(from, to);
        }

        for (int j = startZ; j <= endZ; j++)
        {
            Vector3 from = new Vector3(startX * layers[selectedLayer].tileWidth, layers[selectedLayer].gridYPos, j * layers[selectedLayer].tileWidth);
            Vector3 to = new Vector3(endX * layers[selectedLayer].tileWidth, layers[selectedLayer].gridYPos, j * layers[selectedLayer].tileWidth);
            Gizmos.DrawLine(from, to);
        }
    }

    public void RegisterPlacedPrefab(GameObject placed)
    {

    }
}

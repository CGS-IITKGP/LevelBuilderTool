using UnityEngine;

public class Grid : MonoBehaviour
{
    public GridTileData gridTileData;

    public int x_Length;
    public int y_Length;
    public int startX = -10;
    public int startZ = -10;
    public int endX = 10;
    public int endZ = 10;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        for (int i = startX; i <= endX; i++)
        {
            Vector3 from = new Vector3(i * gridTileData.TileWidth, gridTileData.GridYPos, startZ * gridTileData.TileWidth);
            Vector3 to = new Vector3(i * gridTileData.TileWidth, gridTileData.GridYPos, endZ * gridTileData.TileWidth);
            Gizmos.DrawLine(from, to);
        }

        for (int j = startZ; j <= endZ; j++)
        {
            Vector3 from = new Vector3(startX * gridTileData.TileWidth, gridTileData.GridYPos, j * gridTileData.TileWidth);
            Vector3 to = new Vector3(endX * gridTileData.TileWidth, gridTileData.GridYPos, j * gridTileData.TileWidth);
            Gizmos.DrawLine(from, to);
        }
    }
}

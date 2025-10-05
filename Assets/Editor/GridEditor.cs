using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Grid))]
public class GridEditor : Editor
{
    private void OnSceneGUI()
    {
        Grid grid = (Grid)target;

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, grid.gridTileData.GridYPos, 0));

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);

            int snappedX = Mathf.FloorToInt(hitPoint.x / grid.gridTileData.TileWidth);
            int snappedZ = Mathf.FloorToInt(hitPoint.z / grid.gridTileData.TileWidth);

            Vector3 snappedPosition = new Vector3(snappedX * grid.gridTileData.TileWidth + grid.gridTileData.TileWidth/2, grid.gridTileData.GridYPos + grid.gridTileData.TileWidth/2, snappedZ * grid.gridTileData.TileWidth + grid.gridTileData.TileWidth / 2);

            Handles.color = new Color(0, 1, 1, 0.2f);
            Handles.CubeHandleCap(0, snappedPosition, Quaternion.identity, grid.gridTileData.TileWidth, EventType.Repaint);

            SceneView.RepaintAll();
        }
    }
}
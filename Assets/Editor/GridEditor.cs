using UnityEngine;
using UnityEditor;

// This attribute tells Unity that this script is an editor for your Grid class.
[CustomEditor(typeof(Grid))]
public class GridEditor : Editor
{
    // This function is called whenever the scene view is updated.
    private void OnSceneGUI()
    {
        // Get a reference to the Grid script component on the selected object.
        Grid grid = (Grid)target;

        // 1. Create a ray from the mouse position into the scene.
        // We use HandleUtility to correctly get a ray from the Scene view camera.
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        // 2. Find where the ray hits the ground plane.
        // We define a mathematical plane at y=0, with a normal pointing up.
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, grid.gridTileData.GridYPos, 0));

        // Check if the ray intersects with our plane.
        if (groundPlane.Raycast(ray, out float distance))
        {
            // If it hits, get the 3D point of intersection.
            Vector3 hitPoint = ray.GetPoint(distance);

            // 3. Round the hit position to the nearest grid coordinate.
            int snappedX = Mathf.FloorToInt(hitPoint.x / grid.gridTileData.TileWidth);
            int snappedZ = Mathf.FloorToInt(hitPoint.z / grid.gridTileData.TileWidth);

            // Convert the snapped integer coordinates back to a 3D world position.
            Vector3 snappedPosition = new Vector3(snappedX * grid.gridTileData.TileWidth + grid.gridTileData.TileWidth/2, grid.gridTileData.GridYPos + grid.gridTileData.TileWidth/2, snappedZ * grid.gridTileData.TileWidth + grid.gridTileData.TileWidth / 2);

            // --- Visualization ---
            // Draw a semi-transparent cube at the snapped position to show where you are aiming.
            Handles.color = new Color(0, 1, 1, 0.2f); // Cyan with transparency
            Handles.CubeHandleCap(0, snappedPosition, Quaternion.identity, grid.gridTileData.TileWidth, EventType.Repaint);

            // This forces the scene view to repaint, making the preview feel responsive.
            SceneView.RepaintAll();
        }
    }
}
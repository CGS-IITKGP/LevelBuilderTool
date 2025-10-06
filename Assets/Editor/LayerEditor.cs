using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Layer))]
public class LayerEditor : Editor
{
    private void OnSceneGUI()
    {
        Event e = Event.current;
        Layer layer = (Layer)target;

        GET_GRID_POSITION(layer);

        HANDLE_CLICK(e, layer);
    }

    private void GET_GRID_POSITION(Layer layer)
    {

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, layer.gridYPos, 0));

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);

            int snappedX = Mathf.FloorToInt(hitPoint.x / layer.tileWidth);
            int snappedZ = Mathf.FloorToInt(hitPoint.z / layer.tileWidth);

            Vector3 snappedPosition = new(snappedX * layer.tileWidth + layer.tileWidth / 2, layer.gridYPos, snappedZ * layer.tileWidth + layer.tileWidth / 2);
            layer.currentBrushPosition = snappedPosition;

            Handles.color = new Color(0, 1, 1, 0.2f);
            Handles.CubeHandleCap(0, snappedPosition + new Vector3(0, layer.tileWidth / 2, 0), Quaternion.identity, layer.tileWidth, EventType.Repaint);

            SceneView.RepaintAll();
        }
    }
    private void HANDLE_CLICK(Event e, Layer layer)
    {
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            PlacePrefabAt(layer.currentBrushPosition, layer);
            e.Use();
        }

        if (e.type == EventType.MouseDown && e.button == 1)
        {
            RemovePrefabAt(layer.currentBrushPosition, layer);
            e.Use();
        }



        void PlacePrefabAt(Vector3 position, Layer layer)
        {
            if (layer.gridTileData == null || layer.gridTileData.AllPrefabs.Count == 0)
                return;

            var prefab = layer.gridTileData.AllPrefabs[layer.selectedIndices[0]].prefab; // Example: place first prefab

            if (prefab == null)
                return;

            GameObject placed = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            placed.transform.position = position;
            placed.transform.SetParent(layer.transform);

            //layer.RegisterPlacedPrefab(placed);

            Undo.RegisterCreatedObjectUndo(placed, "Placed Prefab");
        }

        void RemovePrefabAt(Vector3 position, Layer layer)
        {
            // Cast small overlap sphere to detect prefab at position
            Collider[] hits = Physics.OverlapSphere(position, layer.tileWidth / 3f);
            foreach (Collider hit in hits)
            {
                if (hit.transform.IsChildOf(layer.transform))
                {
                    Undo.DestroyObjectImmediate(hit.gameObject);
                    //grid.placedPrefabs.Remove(hit.gameObject);
                    break;
                }
            }
        }
    }
}
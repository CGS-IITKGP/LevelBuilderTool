using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Layer))]
public class LayerEditor : Editor
{
    Event e;
    Layer layer;
    Ray mouseRay;
    bool leftMouseHeldDown;
    private Vector3 lastBrushPosition = Vector3.positiveInfinity;










    private void GET_MOUSE_POSITION(Layer layer)
    {

        mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        layer.finalYPos = layer.gridYPos + layer.noOfIncrements * layer.gridYIncrement;
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, layer.finalYPos, 0));

        if (groundPlane.Raycast(mouseRay, out float distance))
        {
            Vector3 hitPoint = mouseRay.GetPoint(distance);
            layer.currentMousePosition = hitPoint;

            int snappedX = Mathf.FloorToInt(hitPoint.x / layer.tileWidth);
            int snappedZ = Mathf.FloorToInt(hitPoint.z / layer.tileWidth);

            Vector3 snappedPosition = new(snappedX * layer.tileWidth + layer.tileWidth / 2, layer.finalYPos, snappedZ * layer.tileWidth + layer.tileWidth / 2);
            layer.currentBrushPosition = snappedPosition;

            CheckOverlap();

            Handles.CubeHandleCap(0, snappedPosition + new Vector3(0, layer.tileWidth / 2, 0), Quaternion.identity, layer.tileWidth, EventType.Repaint);

            if (layer.currentBrushPosition != lastBrushPosition)
            {
                lastBrushPosition = layer.currentBrushPosition;
                SceneView.RepaintAll();
            }

        }
        void CheckOverlap()
        {
            Handles.color = new Color(0, 1, 1, 0.2f);
            var YthLayer = layer.layerData.Find(YthLayer => YthLayer.yIndex == layer.noOfIncrements);
            if (YthLayer != null)
            {
                LayerCellData cellData = YthLayer.cells.Find(cell => cell.position == layer.currentBrushPosition);
                if (cellData != null && cellData.placedPrefabs.Count > 0)
                {
                    Handles.color = new Color(1, 0, 0, 0.6f);
                    return;
                }
            }
        }

    }


    private void HANDLE_CLICK(Event e, Layer layer)
    {
        if (e.button == 0 && e.type == EventType.MouseDrag)
        {
            leftMouseHeldDown = true;
                e.Use();
        } else 
        {
            leftMouseHeldDown = false;
        }

        if (leftMouseHeldDown || (e.type == EventType.MouseDown && e.button == 0))
        {
            if (!layer.settingsLocked)
            {
                Debug.LogWarning("Layer Settings must be locked to paint.");
                return;
            }
            //  ========== BRUSH MODE SINGLE ==========//
            if (layer.currentBrushMode == BrushMode.Single)
            {
                PlaceSingleModePrefab();
                EditorWindow.FocusWindowIfItsOpen<GridWindow>();
            }
            //  ========== BRUSH MODE MULTI ==========//
            if (layer.currentBrushMode == BrushMode.Multi)
            {
                // PlacePrefabAt(layer.currentBrushPosition, layer);
            }
        }

        //if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.R)
        //{
        //    Debug.Log("Refreshing All References (R pressed)");
        //    layer.RefreshAllReferences();
        //    Event.current.Use();
        //    SceneView.RepaintAll();
        //}



        if (!layer.settingsLocked) return;

        if (e.type == EventType.ScrollWheel)
        {
            float scrollDelta = e.delta.y;

            if (scrollDelta < 0)
            {
                layer.noOfIncrements--;
            }
            else if (scrollDelta > 0)
            {
                layer.noOfIncrements++;
            }
            e.Use();
        }



        void PlaceSingleModePrefab()
        {
            if (layer.gridTileData == null || layer.gridTileData.AllPrefabs.Count == 0)
                return;

            if (layer.selectedIndices.Count == 0)
            {
                Debug.LogWarning("No Prefab Selected");
                return;
            }

            var YthLayer = layer.layerData.Find(YthLayer => YthLayer.yIndex == layer.noOfIncrements);
            if (YthLayer != null)
            {
                LayerCellData cellData = YthLayer.cells.Find(cell => cell.position == layer.currentBrushPosition);
                if (cellData != null && cellData.placedPrefabs.Count > 0)
                {
                    //Debug.LogWarning("A Prefab already exists in this cell. Use Erase tool to remove it first.");
                    return;
                }
            }


            Undo.RecordObject(layer, "Register Placed Prefab");

            Prefab allPrefab = null;
            GroupedPrefab groupedPrefab = null;
            bool isGrouped = false;
            if (layer.prefabTabIndex == 0)
            {
                allPrefab = layer.gridTileData.AllPrefabs[layer.selectedIndices[0]];
            }else if (layer.prefabTabIndex == 1)
            {
                groupedPrefab = layer.gridTileData.GroupedPrefabs[layer.selectedIndices[0]];
                isGrouped = true;
            }

            if (!isGrouped)
            {
                if (allPrefab == null)
                    return;

                ////========= All Prefab Settings =========//

                if (allPrefab.prefab == null)
                    return;

                int noOfPrefabPerTile = UnityEngine.Random.Range(allPrefab.noOfPrefabPerTileMin, allPrefab.noOfPrefabPerTileMax + 1);
                for (int i = 0; i < noOfPrefabPerTile; i++)
                {
                    InstantiatePrefab(allPrefab);
                }
            }
            else
            {
                if (isGrouped && (groupedPrefab == null || groupedPrefab.prefabs.Count == 0))
                    return;

                List<int> selectedPrefabsIndecies = GetWeightedRandomIndices(ref groupedPrefab);
                List<int> amountPerPrefab = new List<int>();

                int totalAmount = 0;

                for (int i = 0; i < selectedPrefabsIndecies.Count; i++)
                {
                    int amount = UnityEngine.Random.Range(groupedPrefab.prefabs[selectedPrefabsIndecies[i]].noOfPrefabPerTileMin, groupedPrefab.prefabs[selectedPrefabsIndecies[i]].noOfPrefabPerTileMax + 1);
                    amountPerPrefab.Add(amount);
                    totalAmount += amount;
                }

                ReduceToMaxAmountPerTile(ref amountPerPrefab, ref totalAmount, ref groupedPrefab);
                

                for (int i = 0; i < selectedPrefabsIndecies.Count; i++)
                {
                    Prefab prefab = groupedPrefab.prefabs[selectedPrefabsIndecies[i]];
                    if (prefab.prefab == null)
                        return;

                    for (int j = 0; j < amountPerPrefab[i]; j++)
                    {
                        InstantiatePrefab(prefab);
                    }
                }
            }

            

            

        }
    }

    void RightClickFuntionality()
    {
        if (!layer.settingsLocked)
        {
            Debug.LogWarning("Layer Settings must be locked to paint.");
            return;
        }

        Undo.RecordObject(layer, "Erase Placed Prefab");
        layer.EraseSinglePrefab();
    }

    List<int> GetWeightedRandomIndices(ref GroupedPrefab groupedPrefab)
    {
        int noOfDiffPrefabPerTile = groupedPrefab.noOFDiffPrefabPerTile;
        if (noOfDiffPrefabPerTile > groupedPrefab.prefabs.Count)
            noOfDiffPrefabPerTile = groupedPrefab.prefabs.Count;

        List<int> selectedPrefabsIndecies = new List<int>();
        List<int> allPrefabsIndecies = new List<int>();

        for (int i = 0; i < groupedPrefab.prefabs.Count; i++)
        {
            allPrefabsIndecies.Add(i);
        }

        float totalChance = 0;
        for (int i = 0; i < allPrefabsIndecies.Count; i++)
        {
            totalChance += groupedPrefab.prefabs[allPrefabsIndecies[i]].chanceOfSelection;
        }

        while (selectedPrefabsIndecies.Count < noOfDiffPrefabPerTile)
        {
            if (totalChance <= 0f)
            {
                int randomIndex = UnityEngine.Random.Range(0, allPrefabsIndecies.Count);
                selectedPrefabsIndecies.Add(allPrefabsIndecies[randomIndex]);
                allPrefabsIndecies.Remove(randomIndex);
                continue;
            }

            float randomPoint = UnityEngine.Random.value * totalChance;
            float cumulative = 0f;
            for (int i = 0; i < allPrefabsIndecies.Count; i++)
            {
                var prefab = groupedPrefab.prefabs[allPrefabsIndecies[i]];
                cumulative += prefab.chanceOfSelection;

                if (randomPoint <= cumulative)
                {
                    selectedPrefabsIndecies.Add(allPrefabsIndecies[i]);
                    totalChance -= prefab.chanceOfSelection;
                    allPrefabsIndecies.Remove(i);
                    break;
                }
            }
        }

        return selectedPrefabsIndecies;
    }

    void ReduceToMaxAmountPerTile(ref List<int> amountPerPrefab, ref int totalAmount, ref GroupedPrefab groupedPrefab)
    {

        if (totalAmount > groupedPrefab.maxNoOfAllPrefabsPerTile)
        {
            int reductionAmount = totalAmount - groupedPrefab.maxNoOfAllPrefabsPerTile;

            while (reductionAmount > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, totalAmount);
                int cumulative = 0;
                for (int i = 0; i < amountPerPrefab.Count; i++)
                {
                    cumulative += amountPerPrefab[i];
                    if (randomIndex <= cumulative)
                    {
                        amountPerPrefab[i]--;
                        reductionAmount--;
                        totalAmount--;
                        break;
                    }
                }
            }
        }
    }

    private void InstantiatePrefab(Prefab prefab)
    {
        // randomize position
        Vector3 pos = layer.currentBrushPosition;
        Vector3 offsetPos = Vector3.zero;
        if (prefab.randomizePosition)
        {
            if (prefab.rangeRandomizationPosition && prefab.positionRange.Count == 2)
            {
                float randomX = UnityEngine.Random.Range(prefab.positionRange[0].x, prefab.positionRange[1].x);
                float randomY = UnityEngine.Random.Range(prefab.positionRange[0].y, prefab.positionRange[1].y);
                float randomZ = UnityEngine.Random.Range(prefab.positionRange[0].z, prefab.positionRange[1].z);
                offsetPos = new Vector3(randomX, randomY, randomZ);
            }
            else if (prefab.specificPositions.Count > 0)
            {
                int randIndex = UnityEngine.Random.Range(0, prefab.specificPositions.Count);
                offsetPos = prefab.specificPositions[randIndex];
            }
            else
            {
                offsetPos = Vector3.zero;
            }
        }

        // randomize rot
        Quaternion rot = Quaternion.identity;
        if (prefab.randomizeRotation)
        {
            if (prefab.rangeRandomizationRotation && prefab.rotationRange.Count == 2)
            {
                float randomX = UnityEngine.Random.Range(prefab.rotationRange[0].x, prefab.rotationRange[1].x);
                float randomY = UnityEngine.Random.Range(prefab.rotationRange[0].y, prefab.rotationRange[1].y);
                float randomZ = UnityEngine.Random.Range(prefab.rotationRange[0].z, prefab.rotationRange[1].z);
                rot = Quaternion.Euler(randomX, randomY, randomZ);
            }
            else if (prefab.specificRotations.Count > 0)
            {
                int randIndex = UnityEngine.Random.Range(0, prefab.specificRotations.Count);
                rot = Quaternion.Euler(prefab.specificRotations[randIndex]);
            }
            else
            {
                rot = Quaternion.identity;
            }
        }

        // randomize scale
        float scale = 1;
        if (prefab.randomizeScale)
        {
            if (prefab.scaleRange.Count == 2)
            {
                scale = UnityEngine.Random.Range(prefab.scaleRange[0], prefab.scaleRange[1]);
            }
            else
            {
                scale = 1;
            }
        }



        GameObject placed = (GameObject)PrefabUtility.InstantiatePrefab(prefab.prefab);
        placed.transform.position = pos + offsetPos;
        placed.transform.rotation = rot;
        placed.transform.localScale = new Vector3(scale, scale, scale);
        placed.transform.SetParent(layer.transform);


        layer.RegisterPlacedPrefab(placed, prefab, offsetPos, rot, new Vector3(scale, scale, scale));
        Undo.RegisterCreatedObjectUndo(placed, "Placed Prefab");
    }

    private void OnEnable()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    private void OnDisable()
    {
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
    }

    private void OnHierarchyChanged()
    {
        if (layer == null || layer.gameObject == null)
            return;

        if (!layer.gameObject.scene.IsValid())
            return;

        foreach (Transform child in layer.transform)
        {
            if (child == null)
            {
                TriggerLayerRefresh("Child removed or deleted");
                return;
            }
        }

        int childCount = layer.transform.childCount;
        if (lastChildCount != childCount)
        {
            lastChildCount = childCount;
            TriggerLayerRefresh("Child count changed (added/removed prefab)");
        }
    }

    private int lastChildCount = -1;

    private void TriggerLayerRefresh(string reason)
    {
        //Debug.Log($"Hierarchy changed under Layer ({reason}) — refreshing references.");
        layer.RefreshAllReferences();
        SceneView.RepaintAll();
    }









    /// <summary>
    /// =====================CHAT GPT CODE TO KEEP FOCUS ON WINDOW AFTER NAVIGATION========================== ///
    /// </summary>
    private bool isRightMouseDown;
    private bool isMiddleMouseDown;
    private double rightClickDownTime;
    private bool sceneNavigating;

    private const float clickThreshold = 0.2f; // seconds to count as quick click

    [MenuItem("Window/Grid Window")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<GridWindow>("Grid Window");
    }

    private void OnSceneGUI()
    {
        e = Event.current;
        layer = (Layer)target;

        if (layer.grid.windowFocused == false)
            return;

        GET_MOUSE_POSITION(layer);

        HANDLE_CLICK(e, layer);

        // --- Mouse Down ---
        if (e.type == EventType.MouseDown)
        {
            if (e.button == 1)
            {
                isRightMouseDown = true;
                rightClickDownTime = EditorApplication.timeSinceStartup;
                sceneNavigating = false;
            }
            else if (e.button == 2)
            {
                isMiddleMouseDown = true;
                sceneNavigating = true;
            }
        }

        // --- Mouse Drag (camera navigation) ---
        if (e.type == EventType.MouseDrag && (isRightMouseDown || isMiddleMouseDown))
        {
            sceneNavigating = true;
        }

        // --- Mouse Up ---
        if (e.type == EventType.MouseUp)
        {
            // --- Right Click ---
            if (e.button == 1 && isRightMouseDown)
            {
                isRightMouseDown = false;

                double heldTime = EditorApplication.timeSinceStartup - rightClickDownTime;
                bool isQuickClick = heldTime < clickThreshold && !sceneNavigating;

                // Perform custom right-click action (short tap only)
                if (isQuickClick)
                {
                    PerformRightClickAction();
                }

                e.Use();

                // Refocus window after camera navigation ends
                FocusGridWindowAfterDelay();
            }

            // --- Middle Click ---
            else if (e.button == 2 && isMiddleMouseDown)
            {
                isMiddleMouseDown = false;
                FocusGridWindowAfterDelay();
            }
        }
    }

    private void PerformRightClickAction()
    {
        RightClickFuntionality();
        // Place your custom function here.
        // Example: Select tile under mouse, show context menu, erase prefab, etc.
    }

    private void FocusGridWindowAfterDelay()
    {
        // Wait a bit for Unity’s internal camera handling to finish
        EditorApplication.delayCall += () =>
        {
            GridWindow window = EditorWindow.GetWindow<GridWindow>();
            if (window != null)
            {
                window.Focus();
                window.Repaint();
            }
        };
    }

}
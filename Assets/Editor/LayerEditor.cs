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


    bool leftMouseDragging;
    bool leftClickCancelled = false;


    Vector3 mouseDragStartPos;
    Vector3 mouseDragEndPos;
    bool dragRegistered;





    private Vector3 lastBrushPosition = Vector3.positiveInfinity;






    private void GET_MOUSE_POSITION(Layer layer)
    {

        mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        layer.finalYPos_current = layer.gridYPos + layer.noOfIncrements * layer.gridYIncrement;
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, layer.finalYPos_current, 0));

        if (groundPlane.Raycast(mouseRay, out float distance))
        {
            Vector3 hitPoint = mouseRay.GetPoint(distance);
            layer.currentMousePosition = hitPoint;

            int snappedX = Mathf.FloorToInt(hitPoint.x / layer.tileWidth);
            int snappedZ = Mathf.FloorToInt(hitPoint.z / layer.tileWidth);

            Vector3 snappedPosition = new(snappedX * layer.tileWidth + layer.tileWidth / 2, layer.finalYPos_current, snappedZ * layer.tileWidth + layer.tileWidth / 2);
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
        {
            // ===== Left Mouse Button Clicking ======//
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                mouseDragStartPos = layer.currentBrushPosition;
                leftClickCancelled = false;
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                mouseDragEndPos = layer.currentBrushPosition;
            }

            // ===== Left Mouse Button Dragging ======//
            if (e.button == 0 && e.type == EventType.MouseDrag)
            {
                leftClickCancelled = true;
                leftMouseDragging = true;
                e.Use();
            }
            else if (e.type == EventType.MouseUp)
            {
                leftMouseDragging = false;
            }


        }
     
        
        //  ========== BRUSH MODE SINGLE ==========//
        if (layer.currentBrushMode == BrushMode.Single)
        {
            if (leftMouseDragging || (e.type == EventType.MouseDown && e.button == 0))
            {
                if (!layer.settingsLocked)
                {
                    Debug.LogWarning("Layer Settings must be locked to paint.");
                    return;
                }
                PlaceSingleModePrefab();
                //EditorWindow.FocusWindowIfItsOpen<GridWindow>();
                Selection.activeGameObject = layer.gameObject;
                e.Use();
            }
        }

        //  ========== BRUSH MODE MULTI ==========//
        if (layer.currentBrushMode == BrushMode.Multi)
        {
            if (leftMouseDragging)
            {
                DrawDragSelectionHologram();
            }
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                InitiateMultiModePlacement();
            }
            if (e.type == EventType.MouseUp && e.button == 0)
            {
                RegisterMultiModePlacement();

                Selection.activeGameObject = layer.gameObject;
                e.Use();
            }
        }

        // ========= BRUSH MODE SELECTION =============//
        if (layer.currentBrushMode == BrushMode.Select)
        {
            if (leftMouseDragging)
            {
                DrawDragSelectionHologram();
            }
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                InitiateSelectModePlacement();
            }
            if (e.type == EventType.MouseUp && e.button == 0)
            {
                RegisterSelectModePlacement();

                Selection.activeGameObject = layer.gameObject;
                e.Use();
            }

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete)
            {
                Debug.Log("Deleting Selected Cell Prefabs");
                DeleteSelectedCellPrefabs();
                e.Use();
                GUIUtility.hotControl = 0; // ensures SceneView regains control (Chat gpt)
                HandleUtility.Repaint();   // force SceneView update
            }
        }
        else
        {
            layer.selectedCells.Clear();
        }

        if (!layer.settingsLocked) return;

        // ========= SCROLL WHEEL TO CHANGE LAYER ==========//
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

        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.UpArrow)
            {
                layer.noOfIncrements++;
                e.Use();
            }
            else if (e.keyCode == KeyCode.DownArrow)
            {
                layer.noOfIncrements--;
                e.Use();
            }
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
            }
            else if (layer.prefabTabIndex == 1)
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

        void InitiateMultiModePlacement()
        {
            if (!dragRegistered)
            {
                dragRegistered = true;
            }
        }
        void RegisterMultiModePlacement()
        {
            if (!dragRegistered)
            {
                return;
            }
            dragRegistered = false;

            Vector3 start = mouseDragStartPos;
            Vector3 end = mouseDragEndPos;

            for (float x = Math.Min(start.x, end.x); x <= Math.Max(start.x, end.x); x += layer.tileWidth)
            {
                for (float z = Math.Min(start.z, end.z); z <= Math.Max(start.z, end.z); z += layer.tileWidth)
                {
                    layer.currentBrushPosition = new Vector3(x, layer.finalYPos_current, z);
                    PlaceSingleModePrefab();
                }
            }

            //EditorWindow.FocusWindowIfItsOpen<GridWindow>();
        }

        void InitiateSelectModePlacement()
        {
            if (!dragRegistered)
            {
                dragRegistered = true;
            }
        }
        void RegisterSelectModePlacement()
        {
            if (!dragRegistered)
            {
                return;
            }
            dragRegistered = false;

            Vector3 start = mouseDragStartPos;
            Vector3 end = mouseDragEndPos;

            bool firstSelection = (!e.control && !e.shift);

            for (float x = Math.Min(start.x, end.x); x <= Math.Max(start.x, end.x); x += layer.tileWidth)
            {
                for (float z = Math.Min(start.z, end.z); z <= Math.Max(start.z, end.z); z += layer.tileWidth)
                {
                    layer.currentBrushPosition = new Vector3(x, layer.finalYPos_current, z);
                    SelectCell(firstSelection);
                    firstSelection = false;
                }
            }

            //EditorWindow.FocusWindowIfItsOpen<GridWindow>();
        }


        void SelectCell(bool firstSelection)
        {
            if (firstSelection)
            {
                layer.selectedCells.Clear();
            }


            var YthLayer = layer.layerData.Find(YthLayer => YthLayer.yIndex == layer.noOfIncrements);
            if (YthLayer == null)
                return;
            
            LayerCellData cellData = YthLayer.cells.Find(cell => cell.position == layer.currentBrushPosition);

            if (cellData != null)
            {
                if (!layer.selectedCells.Contains(cellData))
                    layer.selectedCells.Add(cellData);
                else if (!leftClickCancelled)
                    layer.selectedCells.Remove(cellData);
            }
        }

        void DeleteSelectedCellPrefabs()
        {
            if (layer.selectedCells == null || layer.selectedCells.Count == 0)
                return;


            Undo.RecordObject(layer, "Delete Selected Cell Prefabs");

            foreach (var cell in layer.selectedCells)
            {
                foreach (var placedPrefab in cell.placedPrefabs)
                {
                    if (placedPrefab.placedPrefab != null)
                    {
                        Undo.RegisterFullObjectHierarchyUndo(placedPrefab.placedPrefab, "Delete Prefab");
                    }
                }
            }

            foreach (var cell in layer.selectedCells)
            {
                    layer.EraseSinglePrefab(cell);
            }
            layer.selectedCells.Clear();

            EditorUtility.SetDirty(layer);
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
        // ================== randomize position
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

        // =============== randomize rot
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

        // =========== randomize scale
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
        placed.transform.SetParent(layer.objectParentObject.transform);


        layer.RegisterPlacedPrefab(placed, prefab, offsetPos, rot, new Vector3(scale, scale, scale));
        Undo.RegisterCreatedObjectUndo(placed, "Placed Prefab");
    }

    void DrawSelectedCellOutlines()
    {
        if (layer.selectedCells == null || layer.selectedCells.Count == 0)
            return;

        Handles.color = new Color(0, 1, 0, 0.8f); 

        Vector3 cubeSize = Vector3.one * layer.tileWidth;

        foreach (var cell in layer.selectedCells)
        {
            if (cell != null)
            {
                Vector3 cubeCenter = cell.position + new Vector3(0, layer.tileWidth / 2, 0);

                Handles.DrawWireCube(cubeCenter, cubeSize);
            }
        }
    }
    void DrawDragSelectionHologram()
    {
        Handles.color = new Color(0.9f, 0.9f, 0.9f, 0.9f);

        Vector3 cubeSize = Vector3.one * layer.tileWidth;

        Vector3 start = mouseDragStartPos;
        Vector3 end = layer.currentBrushPosition;

        for (float x = Math.Min(start.x, end.x); x <= Math.Max(start.x, end.x); x += layer.tileWidth)
        {
            for (float z = Math.Min(start.z, end.z); z <= Math.Max(start.z, end.z); z += layer.tileWidth)
            {
                Vector3 pos = new Vector3(x, layer.finalYPos_current, z);
                Vector3 cubeCenter = pos + new Vector3(0, layer.tileWidth / 2, 0);

                Handles.DrawWireCube(cubeCenter, cubeSize);
            }
        }
    }
    void HandleLayerVisibility()
    {
        int y = layer.noOfIncrements;

        for (int i = 0; i < layer.layerData.Count; i++)
        {
            LayerYLevel layerY = layer.layerData[i];
            if (!layerY.forceVisibility)
            {
                if (layerY.yIndex <= y)
                {
                    SetLayerYVisibility(layerY, true);
                }
                else
                {
                    SetLayerYVisibility(layerY, false);
                }
            }else
            {
                SetLayerYVisibility(layerY, layerY.visibility);
            }
        }


        void SetLayerYVisibility(LayerYLevel layerY, bool vis)
        {
            for (int i = 0; i < layerY.cells.Count; i++)
            {
                LayerCellData cell = layerY.cells[i];
                for (int j = 0; j < cell.placedPrefabs.Count; j++)
                {
                    GameObject prefabInstance = cell.placedPrefabs[j].placedPrefab;
                    if (prefabInstance != null)
                    {
                        prefabInstance.SetActive(vis);
                    }
                }
            }
        }
    }

    // State tracking variables
    private bool isRightMouseDown = false;
    private bool isNavigating = false;
    private int lastChildCount = -1;

    #region Hierarchy Change Detection
    private void OnEnable()
    {
        // Subscribe to the hierarchy changed event to detect prefab deletion
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        layer = (Layer)target;
        if (layer != null && layer.transform != null)
        {
            lastChildCount = layer.transform.childCount;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
    }

    private void OnHierarchyChanged()
    {
        if (layer == null) return;

        // Check if the number of children has changed (e.g., Undo/Redo, manual deletion)
        if (layer.transform.childCount != lastChildCount)
        {
            layer.RefreshAllReferences(); // Call your refresh method
            lastChildCount = layer.transform.childCount;
            SceneView.RepaintAll();
        }
    }
    #endregion


    private void OnSceneGUI()
    {
        e = Event.current;
        layer = (Layer)target;

        // Draw the brush preview first, so it's always visible
        GET_MOUSE_POSITION(layer);
        HANDLE_CLICK(e, layer);
        DrawSelectedCellOutlines();

        HandleLayerVisibility();


        


        // Use the ID of a control to ensure our editor has GUI focus
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        // Handle the event based on its type
        switch (e.GetTypeForControl(controlID))
        {
            case EventType.MouseDown:
                if (e.button == 1) // Right mouse button pressed
                {
                    isRightMouseDown = true;
                    isNavigating = false;
                    // IMPORTANT: We DO NOT use e.Use() here.
                    // Unity needs this event to initiate its camera navigation.
                }
                break;

            case EventType.MouseDrag:
                // If we are holding the right mouse button and drag, it's navigation.
                if (isRightMouseDown)
                {
                    isNavigating = true;
                    // We don't use the event, letting Unity handle the camera.
                }
                break;

            case EventType.MouseUp:
                if (e.button == 1 && isRightMouseDown)
                {
                    // Check if this was a quick click (no dragging occurred)
                    if (!isNavigating)
                    {
                        // This was a tap. Perform our action.
                        RightClickFuntionality();

                        // IMPORTANT: We ONLY use e.Use() here.
                        // This consumes the event and stops Unity from showing the context menu.
                        e.Use();
                    }
                    // Reset state for the next click
                    isRightMouseDown = false;
                    isNavigating = false;
                }
                break;
        }

        // We don't need any special handling for middle-click (e.button == 2) or Alt+Click.
        // By not interfering, Unity's default navigation works perfectly.


    }



    [InitializeOnLoadMethod]
    private static void KeepGridWindowFocusAfterSceneNavigation()
    {
        SceneView.duringSceneGui += sceneView =>
        {
            Event e = Event.current;
            if (e != null && e.type == EventType.MouseUp && (e.button == 1 || e.button == 2))
            {
                EditorApplication.delayCall += () =>
                {
                    var window = Resources.FindObjectsOfTypeAll<GridWindow>();
                    if (window != null && window.Length > 0)
                        window[0].Focus();
                };
            }
        };
    }
}

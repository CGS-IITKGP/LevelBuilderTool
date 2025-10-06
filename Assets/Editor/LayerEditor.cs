using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Layer))]
public class LayerEditor : Editor
{
    Event e;
    Layer layer;

    private void OnSceneGUI()
    {
        e = Event.current;
        layer = (Layer)target;

        GET_MOUSE_POSITION(layer);

        HANDLE_CLICK(e, layer);
    }










    private void GET_MOUSE_POSITION(Layer layer)
    {

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, layer.gridYPos, 0));

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            layer.currentMousePosition = hitPoint;

            int snappedX = Mathf.FloorToInt(hitPoint.x / layer.tileWidth);
            int snappedZ = Mathf.FloorToInt(hitPoint.z / layer.tileWidth);

            Vector3 snappedPosition = new(snappedX * layer.tileWidth + layer.tileWidth / 2, layer.gridYPos, snappedZ * layer.tileWidth + layer.tileWidth / 2);
            layer.currentBrushPosition = snappedPosition;

            CheckOverlap();

            Handles.color = new Color(0, 1, 1, 0.2f);
            Handles.CubeHandleCap(0, snappedPosition + new Vector3(0, layer.tileWidth / 2, 0), Quaternion.identity, layer.tileWidth, EventType.Repaint);

            SceneView.RepaintAll();
        }
    }

    private void CheckOverlap()
    {
        for (int i = 0; i < layer.allPlacedPrefabs_Repeat.Count; i++)
        {
            if (layer.allPlacedPrefabs_Repeat[i].position == layer.currentBrushPosition)
            {
                Handles.color = new Color(1, 0, 0, 0.3f);
                Handles.CubeHandleCap(0, layer.currentBrushPosition + new Vector3(0, layer.tileWidth / 2, 0), Quaternion.identity, layer.tileWidth, EventType.Repaint);
                break;
            }
        }
    }

    private void HANDLE_CLICK(Event e, Layer layer)
    {
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            //  ========== BRUSH MODE SINGLE ==========//
            if (layer.currentBrushMode == BrushMode.Single)
            {
                PlaceSinglePrefab();
                e.Use();
            }
            //  ========== BRUSH MODE MULTI ==========//
            if (layer.currentBrushMode == BrushMode.Multi)
            {
                // PlacePrefabAt(layer.currentBrushPosition, layer);
                e.Use();
            }
        }

        if (e.type == EventType.MouseDown && e.button == 1)
        {
            if (layer.currentBrushMode == BrushMode.Single)
            {
                RemovePrefab();
                e.Use();
            }
        }



        void PlaceSinglePrefab()
        {
            if (layer.gridTileData == null || layer.gridTileData.AllPrefabs.Count == 0)
                return;

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
                    PlacePrefab(allPrefab);
                }
            }
            else
            {
                if (isGrouped && (groupedPrefab == null || groupedPrefab.prefabs.Count == 0))
                    return;

                List<int> selectedPrefabsIndecies = GetWeightedRandomIndices(groupedPrefab);
                List<int> amountPerPrefab = new List<int>();

                int totalAmount = 0;

                for (int i = 0; i < selectedPrefabsIndecies.Count; i++)
                {
                    int amount = UnityEngine.Random.Range(groupedPrefab.prefabs[selectedPrefabsIndecies[i]].noOfPrefabPerTileMin, groupedPrefab.prefabs[selectedPrefabsIndecies[i]].noOfPrefabPerTileMax + 1);
                    amountPerPrefab.Add(amount);
                    totalAmount += amount;
                }
                Debug.Log($"total amount: {totalAmount}");

                ReduceToMaxAmountPerTile(ref amountPerPrefab, ref totalAmount);
                Debug.Log($"selectedPrefabsIndecies: {selectedPrefabsIndecies.Count}");
                foreach (var item in amountPerPrefab)
                {
                    Debug.Log($"amountPerPrefab: {item}");
                }

                for (int i = 0; i < selectedPrefabsIndecies.Count; i++)
                {
                    Prefab prefab = groupedPrefab.prefabs[selectedPrefabsIndecies[i]];
                    if (prefab.prefab == null)
                        return;

                    for (int j = 0; j < amountPerPrefab[i]; j++)
                    {
                        PlacePrefab(prefab);
                    }
                }
            }

            List<int> GetWeightedRandomIndices(GroupedPrefab groupedPrefab)
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

            void ReduceToMaxAmountPerTile(ref List<int> amountPerPrefab, ref int totalAmount)
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

        }

        void RemovePrefab()
        {
            Collider[] hits = Physics.OverlapSphere(layer.currentBrushPosition, layer.tileWidth / 3f);
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

    private void PlacePrefab(Prefab prefab)
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
}
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GridWindow : EditorWindow
{
    [Header("References")]
    GridTileData gridTileData;
    Grid grid;

    [Header("References for Serialized Data")]
    SerializedObject serializedObject;
    SerializedObject gridSO;
    SerializedObject currentLayerSO;

    SerializedProperty gridProp;
    SerializedProperty layersProp;
    SerializedProperty allPrefabs;
    SerializedProperty groupedPrefabs;





    [Header("Properties For Navigating Editor")]
    private Vector2 mainScroll; // ============ Window Scrolllllll ============//

    [Header("Properties For Navigating Editor - PREFAB SECTION")]
    bool toShowPrefabSection = false;
    private int tabIndex = 0; // 0 = All Prefabs, 1 = Grouped Prefabs
    private string[] tabs = new string[] { "All Prefabs", "Grouped Prefabs" };
    public List<int> selectedIndices = new(); // ======= List of selected prefab in (all prefabs) and (grouped prefabs) =======//
    SerializedProperty currentPrefabList; // ====to keep track of which list is currently being edited (allPrefabs or groupedPrefabs)=====//
    private Vector2 scrollPos;
    
    [Header("Properties For Navigating Editor - ALL PREFABS")]
    
    [Header("Properties For Navigating Editor - GROUPED PREFABS")]
    List<int> secondSelectedIndices = new(); // ======= List of selected prefab in (grouped prefab) when editing a group =======//
    private Vector2 secondScrollPos;
    
    [Header("Properties For Navigating Editor - GROUPED PREFABS PROPERTIES")]
    bool toShowSecondLevelPrefabProperties = false;

    [Header("Properties For Navigating Editor - Layers")]

    [Header("Properties For Navigating Editor - Brush")]
    private Texture2D singleIcon;
    private Texture2D multiIcon;
    private Texture2D fillIcon;
    private Texture2D lineIcon;
    private Texture2D rectangleIcon;
    private Texture2D eraseIcon;



    [Header("Properties For Making Editor")]
    float prefabThumbnailSize = 70f;
    string newGroupName = "New Group";







    [MenuItem("Grid Tiles/Grid Window")]
    public static void ShowWindow()
    {
        GetWindow<GridWindow>("Grid Tile");
    }




    private void OnEnable()
    {
        gridTileData = AssetDatabase.LoadAssetAtPath<GridTileData>("Assets/All TileMap Editor Assets/ScriptableObjects/GridTileData.asset");
        if (gridTileData == null)
        {
            Debug.LogError("GridTileData asset not found at specified path.");
            return;
        }
        serializedObject = new SerializedObject(gridTileData);
        allPrefabs = serializedObject.FindProperty("AllPrefabs");
        groupedPrefabs = serializedObject.FindProperty("GroupedPrefabs");
        gridProp = serializedObject.FindProperty("grid");

        
        LoadBrushIcons();
    }
    void ReferenceGrid()
    {
        if (grid != null)
        {
            grid.gridTileData = gridTileData;
            gridTileData.grid = grid;
            gridProp = serializedObject.FindProperty("grid");
            gridSO = new SerializedObject(grid);
            layersProp = gridSO.FindProperty("layers");
            
            EditorUtility.SetDirty(grid);
        }
        serializedObject.ApplyModifiedProperties();
    }
    private void LoadBrushIcons()              //TODO
    {
        singleIcon = EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D;
        multiIcon = EditorGUIUtility.IconContent("GameObject Icon").image as Texture2D;
        fillIcon = EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;
        lineIcon = EditorGUIUtility.IconContent("d_UnityEditor.AnimationWindow").image as Texture2D;
        rectangleIcon = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow").image as Texture2D;
        eraseIcon = EditorGUIUtility.IconContent("TreeEditor.Trash").image as Texture2D;
    }








    private void OnGUI()
    {
        InitStyles();
        serializedObject.Update();
        mainScroll = EditorGUILayout.BeginScrollView(mainScroll);



        GUILayout.Space(20);
        DRAW_HEADER();
        GUILayout.Space(20);



        IF_NO_ASSET_FOUND_GUI();

        DATA_AND_REFERENCES_GUI();

        LAYER_MANAGEMENT_GUI();

        DRAW_SELECTED_LAYER_SETTINGS_GUI();

        BRUSH_TOOLS_GUI();

        PREFABS_TAB_GUI();

        SHOWING_SELECTED_PREFAB_OR_GROUP();

        



        EditorGUILayout.EndScrollView();
        serializedObject.ApplyModifiedProperties();
    }


    private void DRAW_HEADER()
    {
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.2f, 0.6f, 0.9f) }
        };
        GUILayout.Space(10);
        GUILayout.Label("GRID TILE EDITOR", headerStyle);
        GUILayout.Space(10);
    }
    private void IF_NO_ASSET_FOUND_GUI()
    {

        if (serializedObject == null || gridTileData == null)
        {
            EditorGUILayout.HelpBox("No GridTileData asset found!", MessageType.Warning);

            if (GUILayout.Button("Create Data Asset"))
            {
                gridTileData = ScriptableObject.CreateInstance<GridTileData>();
                AssetDatabase.CreateAsset(gridTileData, "Assets/All TileMap Editor Assets/ScriptableObjects/GridTileData.asset");
                AssetDatabase.SaveAssets();


                OnEnable(); // reload
            }
            gridTileData = (GridTileData)EditorGUILayout.ObjectField(gridTileData, typeof(GridTileData), true);
            if (gridTileData != null && gridProp != null)
            {
                OnEnable();
            }

            return;
        }
        if (serializedObject == null || gridTileData == null) return;
        

    }
    private void DATA_AND_REFERENCES_GUI()
    {
        if (gridTileData == null) return;

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        using (new EditorGUILayout.HorizontalScope())
        {
                GUILayout.Space(020);
            using (new EditorGUILayout.VerticalScope(GUILayout.MaxHeight(40)))
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Data", EditorStyles.largeLabel);
                GUILayout.FlexibleSpace();
            }
            using (new GUILayout.VerticalScope())
            {

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Asset", GUILayout.Width(40));
                    gridTileData = (GridTileData)EditorGUILayout.ObjectField(gridTileData, typeof(GridTileData), true);
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Grid", GUILayout.Width(40));
                    grid = (Grid)EditorGUILayout.ObjectField(grid, typeof(Grid), true);
                    ReferenceGrid();
                    serializedObject.ApplyModifiedProperties();
                }
                if (grid == null)
                {
                    if (GUILayout.Button("Create Grid GameObject"))
                    {
                        GameObject gridGO = new GameObject("TileGrid");
                        grid = gridGO.AddComponent<Grid>(); // Adds Unity's Grid Component
                        ReferenceGrid();
                        Selection.activeObject = gridGO;   // Select it in the hierarchy
                    }
                }
                GUILayout.Space(0);
            }
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }
    private void LAYER_MANAGEMENT_GUI()
    {
        if (grid == null || layersProp == null) return;

        DrawCenteredLabel("Layers", 18, FontStyle.Bold, new Color(0.8f, 0.7f, 0.3f));
        GUILayout.Space(10);

        int layerCount = layersProp.arraySize;
        string[] layerTabs = new string[layerCount + 1];

        for (int i = 0; i < layerCount; i++)
        {
            layerTabs[i] = $"Layer {i}";
        }
        layerTabs[layerCount] = "+"; 

        grid.selectedLayer = GUILayout.Toolbar(grid.selectedLayer, layerTabs);

        if (grid.selectedLayer == layerCount)
        {
            CreateNewLayer();
        }

        GUILayout.Space(5);


        void CreateNewLayer()
        {
            GameObject layerGO = new GameObject("New Layer");
            layerGO.transform.parent = grid.transform;
            Layer layer = layerGO.AddComponent<Layer>();

            layer.Name = "New Layer";
            layer.grid = grid;
            layer.gridTileData = gridTileData;
            
            grid.layers.Add(layer);
        }
    }
    private void DRAW_SELECTED_LAYER_SETTINGS_GUI()
    {
        if (grid == null || layersProp == null || layersProp.arraySize == 0) return;

        if (grid.selectedLayer >= layersProp.arraySize)
        {
            grid.selectedLayer = layersProp.arraySize - 1;
        }

        if (grid.layers[grid.selectedLayer] == null)
        {
            layersProp.DeleteArrayElementAtIndex(grid.selectedLayer);
            grid.layers.RemoveAt(grid.selectedLayer);
            grid.selectedLayer = Mathf.Clamp(grid.selectedLayer - 1, 0, layersProp.arraySize - 1);
            return;
        }

        if (grid.layers[grid.selectedLayer].gameObject.name != grid.layers[grid.selectedLayer].Name)
        {
            grid.layers[grid.selectedLayer].gameObject.name = grid.layers[grid.selectedLayer].Name;
        }

        if (EditorWindow.focusedWindow == this)
        {
            Selection.activeGameObject = grid.layers[grid.selectedLayer].gameObject;
            grid.windowFocused = true;
        }else
        {
            grid.windowFocused = false;
        }

            GUILayout.BeginVertical(boxStyle);

        currentLayerSO = new SerializedObject(grid.layers[grid.selectedLayer]);

        SerializedProperty layerNameProp = currentLayerSO.FindProperty("Name");
        SerializedProperty yPosProp = currentLayerSO.FindProperty("gridYPos");
        SerializedProperty yIncrement = currentLayerSO.FindProperty("gridYIncrement");
        SerializedProperty noOfIncrement = currentLayerSO.FindProperty("noOfIncrements");
        SerializedProperty widthProp = currentLayerSO.FindProperty("tileWidth");
        SerializedProperty settingsLocked = currentLayerSO.FindProperty("settingsLocked");

        if (yPosProp != null && widthProp != null)
        {
            EditorGUILayout.LabelField($"Settings for {layerNameProp.stringValue}", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(layerNameProp);
            EditorGUI.BeginDisabledGroup(currentLayerSO.FindProperty("settingsLocked").boolValue);
            EditorGUILayout.PropertyField(yPosProp, new GUIContent("Grid Y Position"));
            EditorGUILayout.PropertyField(yIncrement, new GUIContent("Y Increment"));
            EditorGUILayout.PropertyField(widthProp, new GUIContent("Tile Width")); 
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Apply", GUILayout.Width(60)))
            {
                settingsLocked.boolValue = true;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            if (!settingsLocked.boolValue)
                EditorGUILayout.HelpBox("You will not be able to Change after Applying this Setting", MessageType.Warning);
            EditorGUILayout.EndVertical();
            
            EditorGUI.EndDisabledGroup();
            if (settingsLocked.boolValue)
                EditorGUILayout.PropertyField(noOfIncrement, new GUIContent("No of Increments"));

        }
        else
        {
            EditorGUILayout.HelpBox("Could not find 'gridYPos' or 'tileWidth' properties in the Layer class.", MessageType.Error);
        }
        currentLayerSO.ApplyModifiedProperties();


        if (GUILayout.Button("Delete This Layer", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Delete Layer?", $"Are you sure you want to delete Layer {grid.selectedLayer}?", "Yes", "No"))
            {
                DestroyImmediate(grid.layers[grid.selectedLayer].gameObject);
                grid.layers.RemoveAt(grid.selectedLayer);
                if (grid.selectedLayer > 0)
                {
                    grid.selectedLayer--;
                }
            }
        }

        GUILayout.EndVertical();
    }
    private void BRUSH_TOOLS_GUI()
    {
        GUILayout.Space(10);
        DrawHorizontalLine(new Color(0.3f, 0.3f, 0.3f, 1f));
        GUILayout.Space(8);

        DrawCenteredLabel("Brush Tools", 18, FontStyle.Bold, new Color(0.334f, 0.545f, 0.375f));
        GUILayout.Space(8);

        using (new GUILayout.VerticalScope("box"))
        {
            GUILayout.Space(5);

            // Brush Mode Selection
            using (new GUILayout.HorizontalScope())
            {
                if (gridProp.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("Assign a Grid to enable brush tools.", MessageType.Info);
                    return;
                }

                GUILayout.FlexibleSpace();

                DrawBrushButton(singleIcon, BrushMode.Single, "Place one prefab");
                DrawBrushButton(multiIcon, BrushMode.Multi, "Place multiple prefabs");
                DrawBrushButton(fillIcon, BrushMode.Fill, "Fill selected area");
                DrawBrushButton(lineIcon, BrushMode.Line, "Draw line of prefabs");
                DrawBrushButton(rectangleIcon, BrushMode.Rectangle, "Draw rectangle");
                DrawBrushButton(eraseIcon, BrushMode.Erase, "Erase prefab");

                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(5);
        }

        GUILayout.Space(10);
        DrawHorizontalLine(new Color(0.3f, 0.3f, 0.3f, 1f));

        void DrawBrushButton(Texture2D icon, BrushMode mode, string tooltip)
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.fixedWidth = 50;
            style.fixedHeight = 50;
            style.imagePosition = ImagePosition.ImageAbove;

            if (grid.layers[grid.selectedLayer].currentBrushMode == mode)
            {
                style.normal.background = Texture2D.grayTexture; // highlight selected
            }

            if (GUILayout.Button(new GUIContent(icon, tooltip), style))
            {
                grid.layers[grid.selectedLayer].currentBrushMode = mode;
                //EditorUtility.SetDirty(grid);
            }
        }
    }
    private void PREFABS_TAB_GUI()
    {

        GUILayout.Space(10);
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            toShowPrefabSection = GUILayout.Toggle(toShowPrefabSection, " Show Prefab Section", "Button", GUILayout.Width(200));
            GUILayout.FlexibleSpace();
        }
        GUILayout.Space(10);


        if (toShowPrefabSection)
        {

            tabIndex = GUILayout.Toolbar(tabIndex, tabs);
            grid.layers[grid.selectedLayer].prefabTabIndex = tabIndex;
            GUILayout.BeginVertical(boxStyle);

            if (tabIndex == 0)
            {
                //GUILayout.Label("All Prefabs", EditorStyles.boldLabel);
                GUILayout.Space(15);
                DrawCenteredLabel("All Prefabs (Drag & Drop)", 14, FontStyle.Bold, new Color(0.455f, 0.345f, 0.546f));
                GUILayout.Space(15);
                DrawAllPrefabListElements(allPrefabs, prefabThumbnailSize, ref scrollPos, selectedIndices);
                currentPrefabList = allPrefabs;


                for (int i = 0; i < selectedIndices.Count; i++)
                {
                    selectedIndices.Sort();
                    if (selectedIndices[i] >= gridTileData.AllPrefabs.Count)
                    {
                        selectedIndices.RemoveAt(i);
                    }
                }
            }
            else if (tabIndex == 1)
            {
                //GUILayout.Label("Grouped Prefabs", EditorStyles.boldLabel);
                GUILayout.Space(15);
                DrawCenteredLabel("Grouped Prefabs (Select Indiv. & Create Group)", 14, FontStyle.Bold, new Color(0.2f, 0.4f, 0.3f));
                GUILayout.Space(15);
                DrawGroupedPrefabListElements(groupedPrefabs, prefabThumbnailSize + 50f);
                currentPrefabList = groupedPrefabs;

                for (int i = 0; i < selectedIndices.Count; i++)
                {
                    selectedIndices.Sort();
                    if (selectedIndices[i] >= gridTileData.GroupedPrefabs.Count)
                    {
                        selectedIndices.RemoveAt(i);
                    }
                }
            }

            //GUILayout.Space(5);
            DrawCenteredLabel($"Selected {selectedIndices.Count}", 8, FontStyle.Normal, new Color(0.7f, 0.7f, 0.7f));
            GUILayout.Space(5);

            using (new GUILayout.HorizontalScope())
            {

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Size", GUILayout.Width(30));
                    prefabThumbnailSize = EditorGUILayout.Slider(prefabThumbnailSize, 20f, 150f);
                }

                EditorGUI.BeginDisabledGroup(selectedIndices.Count <= 0);
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Deselect All", GUILayout.MinWidth(100)))
                    {
                        selectedIndices.Clear();
                    }
                    if (GUILayout.Button("Delete", GUILayout.MinWidth(100)))
                    {
                        DeleteSelectedFromList(currentPrefabList, ref selectedIndices);
                    }
                }
            }
            using (new GUILayout.HorizontalScope())
            {
                newGroupName = EditorGUILayout.TextField(newGroupName);
                if (GUILayout.Button("Create Group"))
                {
                    CreatePrefabGroup(newGroupName, ref selectedIndices);
                    newGroupName = "New Group";
                    tabIndex = 1;
                }
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();
        }
    }
    private void SHOWING_SELECTED_PREFAB_OR_GROUP()
    {
        if (selectedIndices != null && selectedIndices.Count > 0)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            if (tabIndex == 0)
            {
                ALL_PREFABS_PROPERTIES_GUI(allPrefabs, ref selectedIndices);
            }
            else if (tabIndex == 1)
            {
                GROUPED_PREFAB_PROPERTIES_GUI();
            }
        }
    }
    private void ALL_PREFABS_PROPERTIES_GUI(SerializedProperty prefabList, ref List<int> SelectedIndices, bool forceDraw = false)
    {
        if (currentPrefabList != allPrefabs && !forceDraw) return;


        if (SelectedIndices.Count > 0)
        {
            GUILayout.BeginVertical(boxStyle);
            GUILayout.Space(15);
            using (new EditorGUILayout.HorizontalScope())
            {
                Rect rect = GUILayoutUtility.GetRect(prefabThumbnailSize, prefabThumbnailSize, GUILayout.ExpandWidth(false));

                GameObject prefab = (GameObject)prefabList.GetArrayElementAtIndex(SelectedIndices[0]).FindPropertyRelative("prefab").objectReferenceValue;
                Texture2D preview = prefab ? AssetPreview.GetAssetPreview(prefab) : Texture2D.grayTexture;

                if (prefab != null && preview != null)
                {
                    GUI.Box(rect, GUIContent.none);
                    GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit);

                }

                using (new EditorGUILayout.VerticalScope())
                {
                    DrawCenteredLabel("Selected Prefab Properties", 20, FontStyle.Bold, new Color(0.145f, 0.346f, 0.655f));
                    if (prefab != null)
                    {
                        //Rect labelRect = new Rect(rect.x, rect.yMax - 16, rect.width, 16);
                        GUILayout.Label(prefab.name, EditorStyles.centeredGreyMiniLabel);
                    }
                }
            }
            GUILayout.Space(15);
            SerializedProperty firstElement = prefabList.GetArrayElementAtIndex(SelectedIndices[0]);

            using (new GUILayout.HorizontalScope())
            {
                //GUILayout.Label("Randomize Position", GUILayout.MinWidth(120));
                EditorGUILayout.PropertyField(firstElement.FindPropertyRelative("randomizePosition"));
            }

            if (firstElement.FindPropertyRelative("randomizePosition").boolValue)
            {
                GUILayout.BeginVertical(boxStyle);
                {
                    EditorGUILayout.PropertyField(firstElement.FindPropertyRelative("rangeRandomizationPosition"), new GUIContent("Range Randomization"));
                    if (firstElement.FindPropertyRelative("rangeRandomizationPosition").boolValue)
                    {
                        EditorGUILayout.PropertyField(firstElement.FindPropertyRelative("positionRange"));
                        if (firstElement.FindPropertyRelative("positionRange").arraySize < 2 || firstElement.FindPropertyRelative("positionRange").arraySize > 2)
                        {
                            firstElement.FindPropertyRelative("positionRange").arraySize = 2;
                        }
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(firstElement.FindPropertyRelative("specificPositions"));
                        if (firstElement.FindPropertyRelative("specificPositions").arraySize < 1)
                        {
                            firstElement.FindPropertyRelative("specificPositions").arraySize = 1;
                        }
                    }
                    // using (new GUILayout.HorizontalScope())
                    // {
                    //     GUILayout.Label("X :");
                    //     GUILayout.Label("MinX");
                    //     firstElement.FindPropertyRelative("minX").floatValue = EditorGUILayout.Slider(firstElement.FindPropertyRelative("minX").floatValue, -.5f, firstElement.FindPropertyRelative("maxX").floatValue);
                    //     GUILayout.Label("MaxX");
                    //     firstElement.FindPropertyRelative("maxX").floatValue = EditorGUILayout.Slider(firstElement.FindPropertyRelative("maxX").floatValue, firstElement.FindPropertyRelative("minX").floatValue, .5f);
                    // }
                    // using (new GUILayout.HorizontalScope())
                    // {
                    //     GUILayout.Label("Y :");
                    //     GUILayout.Label("MinY");
                    //     firstElement.FindPropertyRelative("minY").floatValue = EditorGUILayout.Slider(firstElement.FindPropertyRelative("minY").floatValue, -.5f, firstElement.FindPropertyRelative("maxY").floatValue);
                    //     GUILayout.Label("MaxY");
                    //     firstElement.FindPropertyRelative("maxY").floatValue = EditorGUILayout.Slider(firstElement.FindPropertyRelative("maxY").floatValue, firstElement.FindPropertyRelative("minY").floatValue, .5f);
                    // }
                    // using (new GUILayout.HorizontalScope())
                    // {
                    //     GUILayout.Label("Z :");
                    //     GUILayout.Label("MinZ");
                    //     firstElement.FindPropertyRelative("minZ").floatValue = EditorGUILayout.Slider(firstElement.FindPropertyRelative("minZ").floatValue, -.5f, firstElement.FindPropertyRelative("maxZ").floatValue);
                    //     GUILayout.Label("MaxZ");
                    //     firstElement.FindPropertyRelative("maxZ").floatValue = EditorGUILayout.Slider(firstElement.FindPropertyRelative("maxZ").floatValue, firstElement.FindPropertyRelative("minZ").floatValue, .5f);
                    // }
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.PropertyField(firstElement.FindPropertyRelative("randomizeRotation"));

            if (firstElement.FindPropertyRelative("randomizeRotation").boolValue)
            {
            GUILayout.BeginVertical(boxStyle);
                EditorGUILayout.PropertyField(firstElement.FindPropertyRelative("rangeRandomizationRotation"), new GUIContent("Range Randomization"));
                if (firstElement.FindPropertyRelative("rangeRandomizationRotation").boolValue)
                {
                    EditorGUILayout.PropertyField(firstElement.FindPropertyRelative("rotationRange"));
                    if (firstElement.FindPropertyRelative("rotationRange").arraySize < 2 || firstElement.FindPropertyRelative("rotationRange").arraySize > 2)
                    {
                        firstElement.FindPropertyRelative("rotationRange").arraySize = 2;
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(firstElement.FindPropertyRelative("specificRotations"));
                    if (firstElement.FindPropertyRelative("specificRotations").arraySize < 1)
                    {
                        firstElement.FindPropertyRelative("specificRotations").arraySize = 1;
                    }
                }   
            GUILayout.EndVertical();
            }

            EditorGUILayout.PropertyField(firstElement.FindPropertyRelative("randomizeScale"));

            if (firstElement.FindPropertyRelative("randomizeScale").boolValue)
            {
            GUILayout.BeginVertical(boxStyle);
                EditorGUILayout.PropertyField(firstElement.FindPropertyRelative("scaleRange"));
                if (firstElement.FindPropertyRelative("scaleRange").arraySize < 2 || firstElement.FindPropertyRelative("scaleRange").arraySize > 2)
                {
                    firstElement.FindPropertyRelative("scaleRange").arraySize = 2;
                }
            GUILayout.EndVertical();
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("No of Prefab to place per Tile");
                GUILayout.Label("Min :", GUILayout.Width(30));
                firstElement.FindPropertyRelative("noOfPrefabPerTileMin").intValue = EditorGUILayout.IntField(firstElement.FindPropertyRelative("noOfPrefabPerTileMin").intValue);
                firstElement.FindPropertyRelative("noOfPrefabPerTileMin").intValue = Mathf.Clamp(firstElement.FindPropertyRelative("noOfPrefabPerTileMin").intValue, 0, 100);
                GUILayout.Label("Max :", GUILayout.Width(30));
                firstElement.FindPropertyRelative("noOfPrefabPerTileMax").intValue = EditorGUILayout.IntField(firstElement.FindPropertyRelative("noOfPrefabPerTileMax").intValue);
                firstElement.FindPropertyRelative("noOfPrefabPerTileMax").intValue = Mathf.Clamp(firstElement.FindPropertyRelative("noOfPrefabPerTileMax").intValue, 0, 100);

                if (firstElement.FindPropertyRelative("noOfPrefabPerTileMin").intValue > firstElement.FindPropertyRelative("noOfPrefabPerTileMax").intValue)
                {
                    firstElement.FindPropertyRelative("noOfPrefabPerTileMax").intValue = firstElement.FindPropertyRelative("noOfPrefabPerTileMin").intValue;
                }
            }

            if (SelectedIndices == secondSelectedIndices)
            {
                SerializedProperty listOfPrefabs = groupedPrefabs.GetArrayElementAtIndex(selectedIndices[0]).FindPropertyRelative("prefabs");
                SerializedProperty chance = listOfPrefabs.GetArrayElementAtIndex(secondSelectedIndices[0]).FindPropertyRelative("chanceOfSelection");
                    

                EditorGUILayout.PropertyField(chance, GUILayout.Width(200));
            }

            GUILayout.EndVertical();
        }

    }
    private void GROUPED_PREFAB_PROPERTIES_GUI()
    {
        if (currentPrefabList != groupedPrefabs) return;
        if (selectedIndices.Count == 0) return; // no group selected

        // Get the selected group
        SerializedProperty selectedGroup = groupedPrefabs.GetArrayElementAtIndex(selectedIndices[0]);
        SerializedProperty groupName = selectedGroup.FindPropertyRelative("groupName");
        SerializedProperty subPrefabsArray = selectedGroup.FindPropertyRelative("prefabs");
        SerializedProperty noOfDiffPrefabPerTile = selectedGroup.FindPropertyRelative("noOFDiffPrefabPerTile");
        SerializedProperty maxNoOfAllPrefabsPerTile = selectedGroup.FindPropertyRelative("maxNoOfAllPrefabsPerTile");

        GUILayout.BeginVertical(boxStyle);


        GUILayout.Space(10);


        //groupName.stringValue = EditorGUILayout.TextField("Group Name", groupName.stringValue);
        DrawCenteredLabel("Selected Group", 20, FontStyle.Bold, new Color(0.455f, 0.545f, 0.246f));
        GUILayout.Space(10);
        groupName.stringValue = DrawCenteredTextField(groupName.stringValue, 40, 40, 16);
        EditorGUILayout.PropertyField(noOfDiffPrefabPerTile, new GUIContent("No of Different Prefab to select per Tile"));
        noOfDiffPrefabPerTile.intValue = Mathf.Clamp(noOfDiffPrefabPerTile.intValue, 1, subPrefabsArray.arraySize);
        EditorGUILayout.PropertyField(maxNoOfAllPrefabsPerTile, new GUIContent("Max No of All Prefab to place per Tile"));
        maxNoOfAllPrefabsPerTile.intValue = Mathf.Clamp(maxNoOfAllPrefabsPerTile.intValue, 1, int.MaxValue);

        GUILayout.Space(10);
        GUILayout.Label("Prefabs in Group", EditorStyles.boldLabel);

        DrawAllPrefabListElements(subPrefabsArray, prefabThumbnailSize, ref secondScrollPos, secondSelectedIndices); // reuse the existing drawer
        DrawCenteredLabel($"Selected {secondSelectedIndices.Count}", 8, FontStyle.Normal, new Color(0.7f, 0.7f, 0.7f));
        GUILayout.Space(5);


        EditorGUI.BeginDisabledGroup(secondSelectedIndices.Count <= 0);
        using (new GUILayout.HorizontalScope())
        {
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Deselect Sub Prefabs", GUILayout.MinWidth(100)))
                {
                    secondSelectedIndices.Clear();
                }
                if (GUILayout.Button("Remove Sub Prefabs", GUILayout.MinWidth(100)))
                {
                    DeleteSelectedFromList(subPrefabsArray, ref secondSelectedIndices);
                }
            }
        }
        using (new GUILayout.HorizontalScope())
        {
            newGroupName = EditorGUILayout.TextField(newGroupName);
            if (GUILayout.Button("Create Group From Selected Sub Prefabs"))
            {
                CreatePrefabGroup(newGroupName, ref secondSelectedIndices);
                newGroupName = "New Group";
                tabIndex = 1;
            }
        }

        GUILayout.Space(3);
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            toShowSecondLevelPrefabProperties = GUILayout.Toggle(toShowSecondLevelPrefabProperties, " Show Prefab Properties", "Button", GUILayout.Width(200));
            GUILayout.FlexibleSpace();
        }
        GUILayout.Space(3);

        if (toShowSecondLevelPrefabProperties)
        {
            for (int i = 0; i < secondSelectedIndices.Count; i++)
            {
                secondSelectedIndices.Sort();
                if (secondSelectedIndices[i] >= gridTileData.GroupedPrefabs[selectedIndices[0]].prefabs.Count)
                {
                    secondSelectedIndices.RemoveAt(i);
                }
            }
            ALL_PREFABS_PROPERTIES_GUI(subPrefabsArray, ref secondSelectedIndices, true); // force draw even if not in allPrefabs tab
        }

        EditorGUI.EndDisabledGroup();


        GUILayout.EndVertical();
    }




    private void CreatePrefabGroup(string name, ref List<int> SELECTEDINDEX)
    {
        List<Prefab> prefabs = new ();
        //List<PrefabPropertiesInsideGroupPrefab> prefabProperties = new ();

        for (int i = 0; i < SELECTEDINDEX.Count; i++)
        {
            if (currentPrefabList == allPrefabs)
                prefabs.Add(new Prefab(gridTileData.AllPrefabs[SELECTEDINDEX[i]]));
            else if (currentPrefabList == groupedPrefabs && SELECTEDINDEX == this.selectedIndices)
                CreateGroupFromGroup(SELECTEDINDEX[i]);
            else if (currentPrefabList == groupedPrefabs && SELECTEDINDEX == this.secondSelectedIndices)
            {
                prefabs.Add(new Prefab(gridTileData.GroupedPrefabs[selectedIndices[0]].prefabs[SELECTEDINDEX[i]]));
            }
        }
        GroupedPrefab groupedPrefab = new GroupedPrefab
        {
            groupName = name,
            prefabs = prefabs
        };

        gridTileData.GroupedPrefabs.Add(groupedPrefab);

        void CreateGroupFromGroup(int index)
        {
            for (int i = 0; i < gridTileData.GroupedPrefabs[index].prefabs.Count; i++)
            {
                prefabs.Add(new Prefab(gridTileData.GroupedPrefabs[index].prefabs[i]));
            }
        }
    }
    private void DrawAllPrefabListElements(SerializedProperty listProperty, float thumbnailSize, ref Vector2 scrollPos, List<int> selectedIndicesRef)
    {
        if (listProperty == null || !listProperty.isArray)
        {
            EditorGUILayout.HelpBox("Property is missing or not an array!", MessageType.Error);
            return;
        }

        float maxHeight = 200f; // maximum height of the scroll area

        int count = listProperty.arraySize + 1;
        int columns = Mathf.Max(1, (int)(EditorGUIUtility.currentViewWidth / (thumbnailSize + 10)));
        int rows = Mathf.CeilToInt((float)count / columns);

        float contentHeight = rows * (thumbnailSize + 10); // how tall it actually needs
        float viewHeight = Mathf.Min(contentHeight, maxHeight); // shrink-to-fit, but clamp

        // Reserve exactly the height we need (no fixed 200)
        Rect scrollRect = GUILayoutUtility.GetRect(0, viewHeight, GUILayout.ExpandWidth(true));

        GUIStyle scrollBg = new GUIStyle();
        scrollBg.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.7f));
        // Draw background box
        GUI.Box(scrollRect, GUIContent.none, scrollBg);


        // Handle drag & drop anywhere in this box
        HandlePrefabDragAndDrop(listProperty, scrollRect);

        // Begin scroll view inside that rect
        scrollPos = GUI.BeginScrollView(scrollRect, scrollPos, new Rect(0, 0, scrollRect.width - 20, (thumbnailSize + 10) * rows));

        float x = 5, y = 5;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int index = row * columns + col;
                if (index >= count) break;

                Rect rect = new Rect(x, y, thumbnailSize, thumbnailSize);

                if (index == listProperty.arraySize) /////===== for last element, the "+" button =====/////
                {
                    if (GUI.Button(rect, "+"))
                    {
                        listProperty.arraySize++;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                else
                {
                    SerializedProperty element = listProperty.GetArrayElementAtIndex(index);
                    GameObject prefab = element.FindPropertyRelative("prefab").objectReferenceValue as GameObject;
                    Texture2D preview = prefab ? AssetPreview.GetAssetPreview(prefab) : Texture2D.grayTexture;


                    if (preview != null)
                        GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit);
                    
                    HandleSelectDeselectMultiSelect(rect, index);

                    Label(prefab, index, rect);

                    Event evt = Event.current;
                    if (evt.type == EventType.KeyDown && (evt.keyCode == KeyCode.Delete)
                        && EditorWindow.focusedWindow == this) // only when this editor window is focused
                    {
                        if (DeleteSelectedFromList(listProperty, ref selectedIndicesRef))
                        {
                            evt.Use();
                            return;
                        }
                    }

                    if (rect.Contains(Event.current.mousePosition))
                    {
                        EditorGUI.DrawRect(rect, new Color(1f, 1f, 1f, 0.1f));
                    }

                }

                x += thumbnailSize + 10;
            }

            x = 5;
            y += thumbnailSize + 10;
        }

        GUI.EndScrollView();


        //GUILayout.Space(20);


        void Label(GameObject prefab, int index, Rect rect)
        {
            if (prefab != null && !selectedIndicesRef.Contains(index))
            {
                Rect labelRect = new Rect(rect.x, rect.yMax - 16, rect.width, 16);
                GUI.Label(labelRect, prefab.name, EditorStyles.miniLabel);
            }
            else if (prefab != null && selectedIndicesRef.Contains(index))
            {
                Rect labelRect = new Rect(rect.x, rect.yMax - 16, rect.width, 16);
                EditorGUI.DrawRect(rect, new Color(0.2f, 0.4f, 1f, 0.15f));

                GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);

                var style = new GUIStyle(EditorStyles.boldLabel);
                style.normal.textColor = Color.white;
                GUI.Label(labelRect, prefab.name, style);
            }

        }

        void HandleSelectDeselectMultiSelect(Rect rect, int index)
        {

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                if (!Event.current.control && !Event.current.shift)
                {
                    selectedIndicesRef.Clear();
                    if (selectedIndicesRef.Contains(index))
                        selectedIndicesRef.Remove(index);
                    else
                        selectedIndicesRef.Add(index);
                }
                if (Event.current.control && !Event.current.shift)
                {
                    if (selectedIndicesRef.Contains(index))
                        selectedIndicesRef.Remove(index);
                    else
                        selectedIndicesRef.Add(index);
                }
                if (Event.current.shift && selectedIndicesRef.Count > 0)
                {
                    int start = selectedIndicesRef[selectedIndicesRef.Count - 1];
                    int end = index;

                    int min = Mathf.Min(start, end);
                    int max = Mathf.Max(start, end);

                    for (int i = min; i <= max; i++)
                    {
                        if (!selectedIndicesRef.Contains(i))
                            selectedIndicesRef.Add(i);
                    }
                }
                Event.current.Use();
            }

            if (tabIndex == 0)
            {
                if (grid != null)
                {
                    grid.layers[grid.selectedLayer].selectedIndices = selectedIndicesRef;
                    //EditorUtility.SetDirty(grid);
                }
            }

        }

        void HandlePrefabDragAndDrop(SerializedProperty listProperty, Rect dropArea)       ///chatgpt///
        {
            Event evt = Event.current;

            if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is GameObject go)
                        {
                            int newIndex = listProperty.arraySize;
                            listProperty.arraySize++;
                            listProperty.GetArrayElementAtIndex(newIndex).FindPropertyRelative("prefab").objectReferenceValue = go;
                        }
                    }

                    serializedObject.ApplyModifiedProperties();
                }

                evt.Use();
            }
        }
    }
    private void DrawGroupedPrefabListElements(SerializedProperty groupedPrefabs, float thumbnailSize)
    {
        if (groupedPrefabs == null || !groupedPrefabs.isArray)
        {
            EditorGUILayout.HelpBox("Property is missing or not an array!", MessageType.Error);
            return;
        }

        float maxHeight = 200f; // maximum height of the scroll area

        int count = groupedPrefabs.arraySize;
        int columns = Mathf.Max(1, (int)(EditorGUIUtility.currentViewWidth / (thumbnailSize + 10)));
        int rows = Mathf.CeilToInt((float)count / columns);

        float contentHeight = rows * (thumbnailSize + 10); // how tall it actually needs
        float viewHeight = Mathf.Min(contentHeight, maxHeight); // shrink-to-fit, but clamp

        // Reserve exactly the height we need (no fixed 200)
        Rect scrollRect = GUILayoutUtility.GetRect(0, viewHeight, GUILayout.ExpandWidth(true));

        GUIStyle scrollBg = new GUIStyle();
        scrollBg.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.7f));
        // Draw background box
        GUI.Box(scrollRect, GUIContent.none, scrollBg);


        // Begin scroll view inside that rect
        scrollPos = GUI.BeginScrollView(scrollRect, scrollPos, new Rect(0, 0, scrollRect.width - 20, (thumbnailSize + 10) * rows));

        float x = 5, y = 5;


        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int index = row * columns + col;
                if (index >= count) break;


                Rect rect = new Rect(x, y, thumbnailSize, thumbnailSize);
                if (rect.Contains(Event.current.mousePosition)) ///======== highlight on hover =======///
                {
                    EditorGUI.DrawRect(rect, new Color(1f, 1f, 1f, 0.1f));
                }


                SerializedProperty element = groupedPrefabs.GetArrayElementAtIndex(index);
                SerializedProperty prefabsArray = element.FindPropertyRelative("prefabs");
                int totalPrefabsInGroup = prefabsArray.arraySize;

                GameObject prefab0 = prefabsArray.arraySize > 0 ? prefabsArray.GetArrayElementAtIndex(0).FindPropertyRelative("prefab").objectReferenceValue as GameObject : null;
                GameObject prefab1 = prefabsArray.arraySize > 1 ? prefabsArray.GetArrayElementAtIndex(1).FindPropertyRelative("prefab").objectReferenceValue as GameObject : null;
                GameObject prefab2 = prefabsArray.arraySize > 2 ? prefabsArray.GetArrayElementAtIndex(2).FindPropertyRelative("prefab").objectReferenceValue as GameObject : null;
                GameObject prefab3 = prefabsArray.arraySize > 3 ? prefabsArray.GetArrayElementAtIndex(3).FindPropertyRelative("prefab").objectReferenceValue as GameObject : null;

                Texture2D preview0 = prefab0 ? AssetPreview.GetAssetPreview(prefab0) : Texture2D.grayTexture;
                Texture2D preview1 = prefab1 ? AssetPreview.GetAssetPreview(prefab1) : Texture2D.blackTexture;
                Texture2D preview2 = prefab2 ? AssetPreview.GetAssetPreview(prefab2) : Texture2D.blackTexture;
                Texture2D preview3 = prefab3 ? AssetPreview.GetAssetPreview(prefab3) : null;

                // Divide rect into 4 quadrants
                float halfW = rect.width / 2f;
                float halfH = rect.height / 2f;

                Rect topLeft = new Rect(rect.x, rect.y, halfW, halfH);
                Rect topRight = new Rect(rect.x + halfW, rect.y, halfW, halfH);
                Rect bottomLeft = new Rect(rect.x, rect.y + halfH, halfW, halfH);
                Rect bottomRight = new Rect(rect.x + halfW, rect.y + halfH, halfW, halfH);
                Rect outline = new Rect(rect.x + 1, rect.y + 1, rect.width - 1 , rect.height - 1);

                // Draw previews
                if (preview0) GUI.DrawTexture(topLeft, preview0, ScaleMode.ScaleToFit);
                if (preview1) GUI.DrawTexture(topRight, preview1, ScaleMode.ScaleToFit);
                if (preview2) GUI.DrawTexture(bottomLeft, preview2, ScaleMode.ScaleToFit);

                // Bottom right: either 4th prefab preview or "+N" indicator
                if (preview3 && totalPrefabsInGroup == 4)
                {
                    GUI.DrawTexture(bottomRight, preview3, ScaleMode.ScaleToFit);
                }
                else if (prefabsArray.arraySize > 3) // more prefabs exist
                {
                    GUI.Box(bottomRight, $"+{prefabsArray.arraySize - 3}", EditorStyles.centeredGreyMiniLabel);
                }
                else
                {
                    GUI.Box(bottomRight, "^_^", EditorStyles.centeredGreyMiniLabel);
                }

                GUI.Box(outline, GUIContent.none, EditorStyles.helpBox);

                HandleSelectDeselectMultiSelect(rect, index);

                Label(index, rect);

                Event evt = Event.current;
                if (evt.type == EventType.KeyDown && (evt.keyCode == KeyCode.Delete)
                    && EditorWindow.focusedWindow == this) // only when this editor window is focused
                {
                    if (DeleteSelectedFromList(groupedPrefabs, ref selectedIndices))
                    {
                        evt.Use();
                        return;
                    }
                }

                

                x += thumbnailSize + 10;
            }
            x = 5;
            y += thumbnailSize + 10;
        }





        void HandleSelectDeselectMultiSelect(Rect rect, int index)
        {

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                if (!Event.current.control && !Event.current.shift)
                {
                    selectedIndices.Clear();
                    if (selectedIndices.Contains(index))
                        selectedIndices.Remove(index);
                    else
                        selectedIndices.Add(index);
                }
                if (Event.current.control && !Event.current.shift)
                {
                    if (selectedIndices.Contains(index))
                        selectedIndices.Remove(index);
                    else
                        selectedIndices.Add(index);
                }
                if (Event.current.shift && selectedIndices.Count > 0)
                {
                    int start = selectedIndices[selectedIndices.Count - 1];
                    int end = index;

                    int min = Mathf.Min(start, end);
                    int max = Mathf.Max(start, end);

                    for (int i = min; i <= max; i++)
                    {
                        if (!selectedIndices.Contains(i))
                            selectedIndices.Add(i);
                    }
                }
                Event.current.Use();
            }

            if (grid != null)
            {
                grid.layers[grid.selectedLayer].selectedIndices = selectedIndices;
                //EditorUtility.SetDirty(grid);
            }
        }

        void Label(int index, Rect rect)
        {
            if (!selectedIndices.Contains(index))
            {
                Rect labelRect = new Rect(rect.x, rect.yMax - 16, rect.width, 16);
                GUI.Label(labelRect, groupedPrefabs.GetArrayElementAtIndex(index).FindPropertyRelative("groupName").stringValue);
            }
            else if (selectedIndices.Contains(index))
            {
                Rect labelRect = new Rect(rect.x, rect.yMax - 16, rect.width, 16);
                EditorGUI.DrawRect(rect, new Color(0.2f, 0.4f, 1f, 0.15f));

                var style = new GUIStyle(EditorStyles.boldLabel);
                style.normal.textColor = Color.white;
                GUI.Label(labelRect, groupedPrefabs.GetArrayElementAtIndex(index).FindPropertyRelative("groupName").stringValue, style);
            }

        }

        GUI.EndScrollView();
    }
    bool DeleteSelectedFromList(SerializedProperty listProperty, ref List<int> selectedIndices)
    {
        if (selectedIndices.Count == 0) return false;

        // Sort descending to safely delete from array
        selectedIndices.Sort((a, b) => b.CompareTo(a));

        foreach (int idx in selectedIndices)
        {
            if (idx >= 0 && idx < listProperty.arraySize)
            {
                listProperty.DeleteArrayElementAtIndex(idx);
            }
        }

        selectedIndices.Clear();
        serializedObject.ApplyModifiedProperties();
        return true;
    }
    



    //============= Styling ==============//
    private void DrawCenteredLabel(string text, int fontSize = 12, FontStyle fontStyle = FontStyle.Normal, Color? color = null)
    {
        GUIStyle centeredStyle = new GUIStyle(EditorStyles.label);
        centeredStyle.alignment = TextAnchor.MiddleCenter;
        centeredStyle.fontSize = fontSize;
        centeredStyle.fontStyle = fontStyle;
        centeredStyle.normal.textColor = color ?? GUI.skin.label.normal.textColor;

        // Use horizontal flexible space to ensure exact centering
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(text, centeredStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private string DrawCenteredTextField(string text, float width = 200f, float height = 20, int fontSize = 12)
    {
        GUIStyle style = new GUIStyle(EditorStyles.textField);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = fontSize;

        Rect rect = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(true));
        return EditorGUI.TextField(rect, text, style);
    }

    private GUIStyle boxStyle;
    private bool stylesInitialized = false;
    private void InitStyles()
    {
        if (stylesInitialized) return; // only once
        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.padding = new RectOffset(10, 10, 10, 10);
        boxStyle.margin = new RectOffset(5, 5, 5, 5);
        boxStyle.normal.background = MakeTex(2, 2, new Color(0.15f, 0.15f, 0.15f, 0.8f));
        boxStyle.border = new RectOffset(2, 2, 2, 2);
        stylesInitialized = true;
    }
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
    private void DrawHorizontalLine(Color color, int thickness = 2, int padding = 5)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        EditorGUI.DrawRect(r, color);
    }

}




using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]

public class CustomTerrainEditor : Editor
{
    //properties
    SerializedProperty randomHeightRange;
    SerializedProperty heightMapScale;
    SerializedProperty heightMapImage;
    SerializedProperty perlinXScale;
    SerializedProperty perlinYScale;
    SerializedProperty perlinOffsetX;
    SerializedProperty perlinOffsetY;
    SerializedProperty perlinOctaves;
    SerializedProperty perlinPersistance;
    SerializedProperty perlinHeightScale;
    SerializedProperty resetTerrain;
    SerializedProperty voronoiFallOff;
    SerializedProperty voronoiDropOff;
    SerializedProperty voronoiMinHeight;
    SerializedProperty voronoiMaxHeight;
    SerializedProperty voronoiPeakCount;
    SerializedProperty voronoiType;
    SerializedProperty MPDheightMin;
    SerializedProperty MPDheightMax;
    SerializedProperty MPDroughness;
    SerializedProperty MPDheightDampenerPower;
    SerializedProperty smoothAmount;

    GUITableState splatMapTable;
    SerializedProperty splatHeights;

    GUITableState perlinParameterTable;
    SerializedProperty perlinParameters;

    GUITableState vegetationHeightMap;
    SerializedProperty Vegetation;
    SerializedProperty maxTrees;
    SerializedProperty treeSpacing;

    GUITableState detailMapTable;
    SerializedProperty details;
    SerializedProperty maxDetails;
    SerializedProperty detailSpacing;
    
    SerializedProperty waterHeight;
    SerializedProperty waterGO;
    SerializedProperty shoreLineMaterial;

    SerializedProperty erosionType;
    SerializedProperty erosionStrength;
    SerializedProperty springsPerRiver;
    SerializedProperty solubility;
    SerializedProperty droplets;
    SerializedProperty erosionSmoothAmount;
    SerializedProperty erosionAmount;


    //fold outs
    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlinNoise = false;
    bool showMultiplePerlin = false;
    bool showVoronoi = false;
    bool showMidPointDisplacement = false;
    bool showSmooth = false;
    bool showSplatMaps = false;
    bool showHeightMap = false;
    bool showVegetation = false;
    bool showDetail = false;
    bool showWater = false;
    bool showErosion = false;

    Texture2D hmTexture;

    void OnEnable() 
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        perlinXScale = serializedObject.FindProperty("perlinXScale");
        perlinYScale = serializedObject.FindProperty("perlinYScale");
        perlinOffsetX = serializedObject.FindProperty("perlinOffsetX");
        perlinOffsetY = serializedObject.FindProperty("perlinOffsetY");
        perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        perlinPersistance = serializedObject.FindProperty("perlinPersistance");
        perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");
        resetTerrain = serializedObject.FindProperty("resetTerrain");
        perlinParameterTable = new GUITableState("perlinParameters");
        perlinParameters = serializedObject.FindProperty("perlinParameters");
        voronoiFallOff = serializedObject.FindProperty("voronoiFallOff");
        voronoiDropOff = serializedObject.FindProperty("voronoiDropOff");
        voronoiMinHeight = serializedObject.FindProperty("voronoiMinHeight");
        voronoiMaxHeight = serializedObject.FindProperty("voronoiMaxHeight");
        voronoiPeakCount = serializedObject.FindProperty("voronoiPeakCount");
        voronoiType = serializedObject.FindProperty("voronoiType");
        MPDheightMin = serializedObject.FindProperty("MPDheightMin");
        MPDheightMax = serializedObject.FindProperty("MPDheightMax");
        MPDroughness = serializedObject.FindProperty("MPDroughness");
        MPDheightDampenerPower = serializedObject.FindProperty("MPDheightDampenerPower");
        smoothAmount = serializedObject.FindProperty("smoothAmount");
        splatHeights = serializedObject.FindProperty("splatHeights");
        splatMapTable = new GUITableState("splatMapTable");
        Vegetation = serializedObject.FindProperty("Vegetation");
        vegetationHeightMap = new GUITableState("vegetationHeightMap");
        maxTrees = serializedObject.FindProperty("maxTrees");
        treeSpacing = serializedObject.FindProperty("treeSpacing");
        detailMapTable = new GUITableState("detailMapTable");
        details = serializedObject.FindProperty("details");
        maxDetails = serializedObject.FindProperty("maxDetails");
        detailSpacing = serializedObject.FindProperty("detailSpacing");
        waterHeight = serializedObject.FindProperty("waterHeight");
        waterGO = serializedObject.FindProperty("waterGO");
        shoreLineMaterial = serializedObject.FindProperty("shoreLineMaterial");
        erosionType = serializedObject.FindProperty("erosionType");
        erosionStrength = serializedObject.FindProperty("erosionStrength");
        springsPerRiver = serializedObject.FindProperty("springsPerRiver");
        solubility = serializedObject.FindProperty("solubility");
        droplets = serializedObject.FindProperty("droplets");
        erosionSmoothAmount = serializedObject.FindProperty("erosionSmoothAmount");
        erosionAmount = serializedObject.FindProperty("erosionAmount");
        hmTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);
    }

    Vector2 scrollPos;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain) target;

        //Scrollbar Starting Code
        Rect r = EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(r.width), GUILayout.Height(r.height));
        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(resetTerrain);

        showRandom = EditorGUILayout.Foldout(showRandom, "Random");
        if(showRandom) 
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange); 
            if(GUILayout.Button("Random Heights"))
            {
                terrain.RandomTerrain();
            }

        }


        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "Load Heights");
        if(showLoadHeights)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load Heights From Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.PropertyField(heightMapScale);
            if(GUILayout.Button("Load Texture"))
            {
                terrain.LoadTexture();
            }

        }


        showPerlinNoise = EditorGUILayout.Foldout(showPerlinNoise, "Perlin Noise");
        if(showPerlinNoise)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Add Perlin Noise", EditorStyles.boldLabel);
            EditorGUILayout.Slider(perlinXScale, 0, 1, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinYScale, 0, 1, new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(perlinOffsetX, 0, 10000, new GUIContent("Offset X"));
            EditorGUILayout.IntSlider(perlinOffsetY, 0, 10000, new GUIContent("OffsetY"));
            EditorGUILayout.IntSlider(perlinOctaves, 1, 10, new GUIContent("Octaves"));
            EditorGUILayout.Slider(perlinPersistance, 0.1f, 10, new GUIContent("Persistance"));
            EditorGUILayout.Slider(perlinHeightScale, 0, 1, new GUIContent("Height Scale"));
            if(GUILayout.Button("Add Perlin Noise"))
            {
                terrain.Perlin();
            }
        }

        showMultiplePerlin = EditorGUILayout.Foldout(showMultiplePerlin, "Multiple Perlin Noise");
        if(showMultiplePerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Multiple Perlin Noise", EditorStyles.boldLabel);
            perlinParameterTable = GUITableLayout.DrawTable(perlinParameterTable, serializedObject.FindProperty("perlinParameters"));
            GUILayout.Space(20); 
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewPerlin();
            }
            if(GUILayout.Button("-"))
            {
                terrain.RemovePerlin();
            }
            EditorGUILayout.EndHorizontal();
            if(GUILayout.Button("Apply Multiple Perlin"))
            {
                terrain.MultiplePerlinTerrain();
            }
        }

        showVoronoi = EditorGUILayout.Foldout(showVoronoi, "Voronoi");
        if(showVoronoi)
        {
            EditorGUILayout.IntSlider(voronoiPeakCount, 1, 10, new GUIContent("Peak Count"));
            EditorGUILayout.Slider(voronoiFallOff, 0, 10, new GUIContent("Falloff"));
            EditorGUILayout.Slider(voronoiDropOff, 0, 10, new GUIContent("Dropoff"));
            EditorGUILayout.Slider(voronoiMinHeight, 0, 1, new GUIContent("Min Height"));
            EditorGUILayout.Slider(voronoiMaxHeight, 0, 1, new GUIContent("Max Height"));
            EditorGUILayout.PropertyField(voronoiType);
            if(GUILayout.Button("Voronoi"))
            {
                terrain.Voronoi();
            }
        }

        showMidPointDisplacement = EditorGUILayout.Foldout(showMidPointDisplacement, "Midpoint Displacement");
        if(showMidPointDisplacement)
        {
            EditorGUILayout.PropertyField(MPDheightMin);
            EditorGUILayout.PropertyField(MPDheightMax);
            EditorGUILayout.PropertyField(MPDroughness);
            EditorGUILayout.PropertyField(MPDheightDampenerPower);
            if(GUILayout.Button("MPD"))
            {
                terrain.MidPointDisplacement();
            }
        }

        showSplatMaps = EditorGUILayout.Foldout(showSplatMaps, "Splat Maps");
        if(showSplatMaps)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Splat Maps", EditorStyles.boldLabel);
            splatMapTable = GUITableLayout.DrawTable(splatMapTable, serializedObject.FindProperty("splatHeights"));
            GUILayout.Space(20); 
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewSlatHeigths();
            }
            if(GUILayout.Button("-"))
            {
                terrain.RemoveSplatHeigth();
            }
            EditorGUILayout.EndHorizontal();
            if(GUILayout.Button("Apply Splat Maps"))
            {
                terrain.SplatMaps();
            }
        }

        showVegetation = EditorGUILayout.Foldout(showVegetation, "Vegetation");
    	if(showVegetation)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Vegetation", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(maxTrees, 0, 10000, new GUIContent("Maximum Trees"));
            EditorGUILayout.IntSlider(treeSpacing, 2, 20, new GUIContent("Tree Spacing"));
            vegetationHeightMap = GUITableLayout.DrawTable(vegetationHeightMap, serializedObject.FindProperty("vegetationHeightMap"));
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("+"))
            {
                terrain.AddNewVegetation();
            }
            if(GUILayout.Button("-"))
            {
                terrain.RemoveVegetation();
            }
            EditorGUILayout.EndHorizontal();
            if(GUILayout.Button("Apply Vegetation"))
            {
                terrain.PlantVegetation();
            }
        }

        showDetail = EditorGUILayout.Foldout(showDetail, "Detail");
    	if(showDetail)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Details", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(maxDetails, 0, 10000, new GUIContent("Maximum Details"));
            EditorGUILayout.IntSlider(detailSpacing, 1, 20, new GUIContent("Detail Spacing"));
            detailMapTable = GUITableLayout.DrawTable(detailMapTable, serializedObject.FindProperty("details"));
            terrain.GetComponent<Terrain>().detailObjectDistance = maxDetails.intValue;
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("+"))
            {
                terrain.AddNewDetails();
            }
            if(GUILayout.Button("-"))
            {
                terrain.RemoveDetails();
            }
            EditorGUILayout.EndHorizontal();
            if(GUILayout.Button("Apply Details"))
            {
                terrain.Details(); 
            }
        }

        showWater = EditorGUILayout.Foldout(showWater, "Water");
    	if(showWater)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Water", EditorStyles.boldLabel);
            EditorGUILayout.Slider(waterHeight, 0, 1, new GUIContent("Water Height"));
            EditorGUILayout.PropertyField(waterGO);

            if(GUILayout.Button("Add Water"))
            {
                terrain.AddWater(); 
            }

            EditorGUILayout.PropertyField(shoreLineMaterial);
            if(GUILayout.Button("Add Shoreline"))
            {
                terrain.drawShoreLine(); 
            }
        }

        showErosion = EditorGUILayout.Foldout(showErosion, "Erosion");
    	if(showErosion)
        {
            EditorGUILayout.PropertyField(erosionType);
            EditorGUILayout.Slider(erosionStrength, 0, 1, new GUIContent("Erosion Strength"));
            EditorGUILayout.Slider(erosionAmount, 0, 1, new GUIContent("Erosion Amount"));
            EditorGUILayout.IntSlider(droplets, 0, 500, new GUIContent("Droplets"));
            EditorGUILayout.Slider(solubility, 0.001f, 1, new GUIContent("Solubility"));
            EditorGUILayout.IntSlider(springsPerRiver, 0, 20, new GUIContent("Spring per River"));
            EditorGUILayout.IntSlider(erosionSmoothAmount, 0, 10, new GUIContent("Smooth Amount"));

            if(GUILayout.Button("Erode"))
            {
                terrain.Erode(); 
            }
        }



        showSmooth = EditorGUILayout.Foldout(showSmooth, "Smooth");
        if(showSmooth)
        {
            EditorGUILayout.IntSlider(smoothAmount,1, 10, new GUIContent("smoothAmount"));
            if(GUILayout.Button("Smooth"))
            {
                terrain.Smooth();
            }
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.ResetTerrain();
        }

        showHeightMap = EditorGUILayout.Foldout(showHeightMap, "Height Map");
        if(showHeightMap)
        {
            int hmtSize = (int)(EditorGUIUtility.currentViewWidth - 100);
            GUILayout.Label(hmTexture, GUILayout.Width(hmtSize), GUILayout.Height(hmtSize));
            if(GUILayout.Button("Refresh", GUILayout.Width(hmtSize)))
            {
                float[,] heightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
                for (int y = 0; y < terrain.terrainData.heightmapHeight; y++)
                {
                    for (int x = 0; x < terrain.terrainData.heightmapHeight; x++)
                    {
                        hmTexture.SetPixel(x , y, new Color(heightMap[x, y], heightMap[x, y], heightMap[x, y], 1));
                    }
                }
                hmTexture.Apply();        
            }
        }

        //Scrollbar ending code
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties(); 
    }
}
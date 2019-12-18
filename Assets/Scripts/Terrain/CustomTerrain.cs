using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Random=UnityEngine.Random;


[ExecuteInEditMode]

public class CustomTerrain : MonoBehaviour 
{
    public Vector2 randomHeightRange = new Vector2(0, 0.1f);
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1, 1, 1);

    public bool resetTerrain = true;

    //PERLIN NOISE-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int perlinOffsetX = 0;
    public int perlinOffsetY = 0;
    public int perlinOctaves = 3;
    public float perlinPersistance = 8;
    public float perlinHeightScale = 0.09f;

    //MULTIPLE PERLIN-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [System.Serializable]
    public class PerlinParameters
    {
        public float mPerlinXScale = 0.01f;
        public float mPerlinYScale = 0.01f;
        public int mPerlinOctaves = 3;
        public float mPerlinPersistance = 8;
        public float mPerlinHeightScale = 0.09f;
        public int mPerlinOffsetX = 0;
        public int mPerlinOffsetY = 0;
        public bool remove = false;
    }

    public List<PerlinParameters> perlinParameters = new List<PerlinParameters>()
    {
        new PerlinParameters()
    };

    //VORONOI-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public float voronoiFallOff = 0.2f;
    public float voronoiDropOff = 0.6f;
    public float voronoiMaxHeight = 0.25f;
    public float voronoiMinHeight = 0.412f;
    public int voronoiPeakCount = 5;
    public enum VoronoiType {Linear = 0, Power = 1, Combiened = 2, SinPow = 3}
    public VoronoiType voronoiType = VoronoiType.Linear;

    //MPD-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public float MPDheightMin = -2f;
    public float MPDheightMax = 2.0f;
    public float MPDroughness = 2.0f;
    public float MPDheightDampenerPower = 2.0f;

    //Smooth-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public int smoothAmount = 1;

    //Splatmaps-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [System.Serializable]
    public class SplatHeights
    {
        public Texture2D texture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0;
        public float maxSlope = 1.5f;
        public Vector2 tileOffset = new Vector2 (0, 0);
        public Vector2 tileSize = new Vector2(50, 50);
        public float sNoiseXScale = 0.01f;
        public float sNoiseYScale = 0.01f;
        public float sNoiseScaler = 0.01f;
        public float sOffset = 0.01f;
        public bool remove = false;
    }

    public List<SplatHeights> splatHeights = new List<SplatHeights>()
    {
        new SplatHeights()
    };

    //Vegetation-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [System.Serializable]
    public class Vegetation
    {
        public GameObject mesh;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0f;
        public float maxSlope = 90f;

        public Color color1 = Color.white;
        public Color color2 = Color.white;
        public Color lightColor = Color.white;
        public float maxScale = 1f;
        public float minScale = 0.5f;
        public float minRotation = 0f;
        public float maxRotation = 360f;
        public float density = 0.5f;
        public bool remove = false;
    }

    public List<Vegetation> vegetationHeightMap = new List<Vegetation>()
    {
        new Vegetation()
    };

    public int maxTrees = 5000;
    public int treeSpacing = 5;

    //DETAILS-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [System.Serializable]
    public class Detail
    {
        public GameObject prototype = null;
        public Texture2D prototypeTexture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0;
        public float maxSlope = 1;
        public float overlap = 0.01f;
        public float feather = 0.05f;
        public float density = 0.5f;
        public float minHeightScale = 0.8f;
        public float maxHeightScale = 1f;
        public float minWidth = 0.8f;
        public float maxWidth = 1f;
        public float noiseSpread = 0.5f;
        public Color healthyColor = Color.white;
        public Color dryColor = Color.white;
        public bool remove = false;
    }

    public List<Detail> details = new List<Detail>()
    {
        new Detail()
    };

    public int maxDetails = 5000;
    public int detailSpacing = 5;

    //WATER------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public float waterHeight = 0.5f;
    public GameObject waterGO;
    public Material shoreLineMaterial;

    //EROSION----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public enum ErosionType {Rain = 0, Thermal = 1, Tidal = 2, River = 3, Wind = 4, Canyon = 5}
    public ErosionType erosionType = ErosionType.Rain;
    public float erosionStrength = 0.1f;
    public float erosionAmount = 0.01f;
    public int springsPerRiver = 5;
    public float solubility = 0.01f;
    public int droplets = 10;
    public int erosionSmoothAmount = 5;


    public Terrain terrain;
    public TerrainData terrainData; 

    float[,] GetHeightMap()
    {
        if (!resetTerrain)
        {
            return terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        }
        else
        {
            return new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        }
    }

    public void Perlin()
    {
        float[,] heightMap = GetHeightMap();
        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                heightMap[x, y] += Utils.fBM((x + perlinOffsetX) * perlinXScale, (y + perlinOffsetY) * perlinYScale, perlinOctaves, perlinPersistance) * perlinHeightScale;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void MultiplePerlinTerrain()
    {
        float[,] heightMap = GetHeightMap();
        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                foreach (PerlinParameters p in perlinParameters)
                {
                    heightMap[x, y] += Utils.fBM((x + p.mPerlinOffsetX)* p.mPerlinXScale, (y + p.mPerlinOffsetY) * p.mPerlinYScale, p.mPerlinOctaves, p.mPerlinPersistance) * p.mPerlinHeightScale;
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap); 
    }

    public void AddNewPerlin()
    {
        perlinParameters.Add(new PerlinParameters());
    }

    public void RemovePerlin()
    {
        List<PerlinParameters> keptPerlinParameters = new List<PerlinParameters>();
        for (int i = 0; i < perlinParameters.Count; i++)
        {
            if (!perlinParameters[i].remove)
            {
                keptPerlinParameters.Add(perlinParameters[i]);
            }
        } 

        if (keptPerlinParameters.Count == 0) //don't want to keep any
        {
            keptPerlinParameters.Add(perlinParameters[0]); // add at least 1
        }
        perlinParameters = keptPerlinParameters;
    }

    public void Voronoi()
    {
        float height = UnityEngine.Random.Range(voronoiMinHeight, voronoiMaxHeight);
        float[,] heightMap = GetHeightMap();
        for (int p = 0; p < voronoiPeakCount; p++)
        {
            Vector3 peak = new Vector3(UnityEngine.Random.Range(0, terrainData.heightmapWidth), height, UnityEngine.Random.Range(0, terrainData.heightmapHeight));
            if(heightMap[(int)peak.x, (int) peak.z] < peak.y)
            {
                heightMap[(int)peak.x, (int) peak.z] = peak.y;
            }
            else
            {
                continue;
            }

            Vector2 peakLocation = new Vector2(peak.x, peak.z);
            float maxDistance = Vector2.Distance(new Vector2(0, 0), new Vector2(terrainData.heightmapWidth, terrainData.heightmapHeight));
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                for (int x = 0; x < terrainData.heightmapWidth; x++)
                {
                    if (!(x == peak.x && y == peak.z))
                    {
                        float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y))/maxDistance;
                        float h;
                        if (voronoiType == VoronoiType.Combiened)
                        {
                            h = peak.y - distanceToPeak * voronoiFallOff -Mathf.Pow(distanceToPeak, voronoiDropOff); //combined
                        }
                        else if(voronoiType == VoronoiType.Power)
                        {
                            h = peak.y - Mathf.Pow(distanceToPeak, voronoiDropOff) * voronoiFallOff; //power
                        }
                        else if(voronoiType== VoronoiType.Linear)
                        {
                            h = peak.y - distanceToPeak * voronoiFallOff; //linear
                        }
                        else
                        {
                            h = peak.y - Mathf.Pow(distanceToPeak * 3, voronoiFallOff) - Mathf.Sin(distanceToPeak * 2 * Mathf.PI) / voronoiDropOff; //sinpow
                        }
                        if(heightMap[x, y] < h)
                        {
                            heightMap[x, y] = h;
                        }
                    }
                }
            } 
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    List<Vector2> GenerateNeighbours(Vector2 pos, int width, int height)
    {
        List<Vector2> neighbours = new List<Vector2>();
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if (!(x == 0 && y == 0))
                {
                    Vector2 nPos = new Vector2(Mathf.Clamp(pos.x + x, 0, width - 1), Mathf.Clamp(pos.y + y, 0, height - 1));
                    if (!neighbours.Contains(nPos))
                    {
                       neighbours.Add(nPos);
                    }
                }
            }
        }
        return neighbours;
    }

    public void Smooth()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        float smoothProgress = 0;
        EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress);
        for (int s = 0; s < smoothAmount; s++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                for (int x = 0; x < terrainData.heightmapWidth; x++)
                {
                    float avgHeight = heightMap[x, y];
                    List<Vector2> neighbours = GenerateNeighbours(new Vector2(x, y), terrainData.heightmapWidth, terrainData.heightmapHeight);
                    foreach (Vector2 n in neighbours)
                    {
                        avgHeight += heightMap[(int)n.x, (int)n.y];
                    }

                    heightMap[x, y] = avgHeight / ((float)neighbours.Count + 1);
                }
            }
            smoothProgress++;
            EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress/smoothAmount);
        }
        terrainData.SetHeights(0, 0, heightMap);
        EditorUtility.ClearProgressBar();
    }
   
    public void MidPointDisplacement()
    {
        float[,] heightMap = GetHeightMap();
        int width = terrainData.heightmapWidth - 1;
        int squareSize = width;
        float heightMax = MPDheightMin;
        float heightMin = MPDheightMax;
        float heightDampener = (float)Mathf.Pow(MPDheightDampenerPower, -1 * MPDroughness);

        int cornerX, cornerY;
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

        /*/heightMap[0, 0] = UnityEngine.Random.Range(0f, 0.2f);
        heightMap[0, terrainData.heightmapHeight - 2] = UnityEngine.Random.Range(0f, 0.2f);
        heightMap[terrainData.heightmapWidth - 2, 0] = UnityEngine.Random.Range(0f, 0.2f);
        heightMap[terrainData.heightmapWidth - 2, terrainData.heightmapHeight - 2] = UnityEngine.Random.Range(0f, 0.2f);*/

        while(squareSize > 0)
        {
             for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    heightMap[midX, midY] = (float)((heightMap[x, y] + heightMap[cornerX, y] + heightMap[x, cornerY] + heightMap[cornerX, cornerY]) / 4.0f + UnityEngine.Random.Range(heightMin, heightMax));
                }
            }

           for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {

                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    pmidXR = (int)(midX + squareSize);
                    pmidYU = (int)(midY + squareSize);
                    pmidXL = (int)(midX - squareSize);
                    pmidYD = (int)(midY - squareSize);

                    if (pmidXL <= 0 || pmidYD <= 0 || pmidXR >= width - 1 || pmidYU >= width - 1)
                    {
                        continue;
                    }

                    //Calculate the square value for the bottom side
                    heightMap[midX, y] = (float)((heightMap[midX, pmidYD] + heightMap[cornerX, y] + heightMap[midX, midY] + heightMap[x, y]) / 4.0f +  UnityEngine.Random.Range(heightMin, heightMax));
                    //Calculate the square value for the right side
                    heightMap[cornerX, midY] = (float)((heightMap[cornerX, y] + heightMap[pmidXR, midY] + heightMap[cornerX, cornerY] + heightMap[midX, midY]) / 4.0f +  UnityEngine.Random.Range(heightMin, heightMax));
                    //Calculate the square value for the top side
                    heightMap[midX, cornerY] = (float)((heightMap[cornerX, cornerY] + heightMap[midX, pmidYU] + heightMap[x, cornerY] + heightMap[midX, midY]) / 4.0f +  UnityEngine.Random.Range(heightMin, heightMax));
                    //Calculate the square value for the left side
                    heightMap[x, midY] = (float)((heightMap[x, cornerY] + heightMap[pmidXL, midY] + heightMap[x, y] + heightMap[midX, midY]) / 4.0f +  UnityEngine.Random.Range(heightMin, heightMax));
                }
            }
            squareSize = (int)(squareSize / 2.0f);
            heightMin *= heightDampener;
            heightMax *= heightDampener;
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void AddNewSlatHeigths()
    {
        splatHeights.Add(new SplatHeights());
    }

    public void RemoveSplatHeigth()
    {
        List<SplatHeights> keptSplatHeigths = new List<SplatHeights>();
        for (int i = 0; i < splatHeights.Count; i++)
        {
            if (!splatHeights[i].remove)
            {
                keptSplatHeigths.Add(splatHeights[i]);
            }
        } 

        if (keptSplatHeigths.Count == 0) //don't want to keep any
        {
            keptSplatHeigths.Add(splatHeights[0]); // add at least 1
        }
        splatHeights = keptSplatHeigths;
    }

    float GetSteepness (float[,] heightmap, int x, int y, int width, int heigth)
    {
        float h = heightmap[x, y];
        int nx = x + 1;
        int ny = y + 1;

        //if on the upper edge of the map find gradiend bby going backward.
        if (nx > width - 1)
        {
            nx = x - 1;
        }
        if(ny > heigth - 1)
        {
            ny = y -1;
        }

        float dx = heightmap[nx, y] -h;
        float dy = heightmap[x, ny] -h;
        Vector2 gradient = new Vector2(dx, dy);

        float steep = gradient.magnitude;

        return steep;
    }

    public void SplatMaps()
    {
        TerrainLayer[] newSplatPrototypes;
        newSplatPrototypes = new TerrainLayer[splatHeights.Count];
        int spindex = 0;
        foreach (SplatHeights sh in splatHeights)
        {
            newSplatPrototypes[spindex] = new TerrainLayer();
            newSplatPrototypes[spindex].diffuseTexture = sh.texture;
            newSplatPrototypes[spindex].tileOffset = sh.tileOffset;
            newSplatPrototypes[spindex].tileSize = sh.tileSize;
            newSplatPrototypes[spindex].diffuseTexture.Apply(true);
            spindex++;
        }
        terrainData.terrainLayers = newSplatPrototypes;

        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        float[,,] splatmapData = new float [terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float[] splat = new float[terrainData.alphamapLayers];
                for (int i = 0; i < splatHeights.Count; i++)
                {
                    float sOffset = splatHeights[i].sOffset;
                    float sNoiseScaler = splatHeights[i].sNoiseScaler;
                    float sNoiseXScale = splatHeights[i].sNoiseXScale;
                    float sNoiseYScale = splatHeights[i].sNoiseYScale;
                    float noise = Mathf.PerlinNoise(x * sNoiseXScale, y * sNoiseYScale) * sNoiseScaler;
                    float offset = sOffset + noise;
                    float thisHeightStart = splatHeights[i].minHeight - offset;
                    float thisHeightStop = splatHeights[i].maxHeight + offset;
                    //float steepness = GetSteepness(heightMap, x, y, terrainData.heightmapWidth, terrainData.heightmapHeight);
                    float steepness = terrainData.GetSteepness(y / (float)terrainData.alphamapHeight, x / (float)terrainData.alphamapWidth);
                    if ((heightMap[x, y] >= thisHeightStart && heightMap[x, y] <= thisHeightStop) && (steepness >= splatHeights[i].minSlope && steepness <= splatHeights[i].maxSlope))
                    {
                        splat[i] = 1;
                    }
                }
                NormalizeVector(splat);
                for (int j = 0; j < splatHeights.Count; j++)
                {
                    splatmapData[x, y, j] = splat[j];
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    void NormalizeVector(float[] v)
    {
        float total = 0;
        for (int i = 0; i < v.Length; i++)
        {
            total += v[i];
        }

        for (int i = 0; i < v.Length; i++)
        {
            v[i] /= total;
        }
    }

    public void PlantVegetation()
    {
        TreePrototype[] newTreePrototypes;
        newTreePrototypes = new TreePrototype[vegetationHeightMap.Count];
        int tindex = 0;
        foreach (Vegetation t in vegetationHeightMap)
        {
            newTreePrototypes[tindex] = new TreePrototype();
            newTreePrototypes[tindex].prefab = t.mesh;
            tindex++;
        }
        terrainData.treePrototypes = newTreePrototypes;

        List<TreeInstance> allVegetation = new List<TreeInstance>();
        for (int z = 0; z < terrainData.size.z; z += treeSpacing)
        {
            for (int x = 0; x < terrainData.size.x; x += treeSpacing)
            {
                for (int tp = 0; tp < terrainData.treePrototypes.Length; tp++)
                {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) > vegetationHeightMap[tp].density)
                    {
                        break;
                    }

                    float thisHeight = terrainData.GetHeight(x, z) / terrainData.size.y;
                    float thisHeightStart = vegetationHeightMap[tp].minHeight;
                    float thisHeightEnd = vegetationHeightMap[tp].maxHeight;

                    float steepness = terrainData.GetSteepness(x / (float)terrainData.size.x, z / (float)terrainData.size.z);

                    if ((thisHeight >= thisHeightStart && thisHeight <= thisHeightEnd) && (steepness >= vegetationHeightMap[tp].minSlope && steepness <= vegetationHeightMap[tp].maxSlope))
                    {
                        TreeInstance instance = new TreeInstance();
                        instance.position = new Vector3((x + UnityEngine.Random.Range(-5.0f, 5.0f)) / terrainData.size.x,terrainData.GetHeight(x, z) / terrainData.size.y, (z + UnityEngine.Random.Range(-5.0f, 5.0f)) /terrainData.size.z);

                        Vector3 treeWorldPos = new Vector3(instance.position.x * terrainData.size.x, instance.position.y * terrainData.size.y, instance.position.z * terrainData.size.z)+ this.transform.position;

                        RaycastHit hit;
                        int layerMask = 1 << terrainLayer;

                        if (Physics.Raycast(treeWorldPos + new Vector3(0, 10, 0), -Vector3.up, out hit, 100, layerMask) || Physics.Raycast(treeWorldPos - new Vector3(0, 10, 0), Vector3.up, out hit, 100, layerMask))
                        {
                            float treeHeight = (hit.point.y - this.transform.position.y) / terrainData.size.y;
                            instance.position = new Vector3(instance.position.x,treeHeight, instance.position.z);
                        
                            instance.rotation = UnityEngine.Random.Range(vegetationHeightMap[tp].minRotation, vegetationHeightMap[tp].maxRotation);
                            instance.prototypeIndex = tp;
                            instance.color = Color.Lerp(vegetationHeightMap[tp].color1, vegetationHeightMap[tp].color2, UnityEngine.Random.Range(0.0f,1.0f));
                            instance.lightmapColor = vegetationHeightMap[tp].lightColor;
                            float s = UnityEngine.Random.Range(vegetationHeightMap[tp].minScale, vegetationHeightMap[tp].maxScale);
                            instance.heightScale = s;
                            instance.widthScale = s;

                            allVegetation.Add(instance);
                            if (allVegetation.Count >= maxTrees) 
                            {
                                goto TREESDONE;
                            }
                        }


                    }
                }
            }
        }
    TREESDONE:
        terrainData.treeInstances = allVegetation.ToArray();

    }

    public void AddNewVegetation()
    {
        vegetationHeightMap.Add(new Vegetation());
    }

    public void RemoveVegetation()
    {
        List<Vegetation> keptVegetationParameters = new List<Vegetation	>();
        for (int i = 0; i < vegetationHeightMap.Count; i++)
        {
            if (!vegetationHeightMap[i].remove)
            {
                keptVegetationParameters.Add(vegetationHeightMap[i]);
            }
        } 

        if (keptVegetationParameters.Count == 0) //don't want to keep any
        {
            keptVegetationParameters.Add(vegetationHeightMap[0]); // add at least 1
        }
        vegetationHeightMap = keptVegetationParameters;
    }

    public void Erode()
    {
        if (erosionType == ErosionType.Rain)
        {
            Rain();
        }
        else if (erosionType == ErosionType.Tidal)
        {
            Tidal();
        }
        else if (erosionType == ErosionType.Thermal)
        {
            Thermal();
        }
        else if (erosionType == ErosionType.River)
        {
            River();
        }
        else if (erosionType == ErosionType.Wind)
        {
            Wind();
        }
        else if (erosionType == ErosionType.Canyon)
        {
            DigCanyon();
        }

        smoothAmount = erosionSmoothAmount;
        Smooth();
    }

    void Rain()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        for (int i = 0; i < droplets; i ++)
        {
            heightMap[UnityEngine.Random.Range(0, terrainData.heightmapWidth), UnityEngine.Random.Range(0, terrainData.heightmapHeight)] -= erosionStrength;
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    void Thermal()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                Vector2 thisLocation = new Vector2(x, y);
                List<Vector2> neighbours = GenerateNeighbours(thisLocation, terrainData.heightmapWidth, terrainData.heightmapHeight);
                foreach (Vector2 n in neighbours)
                {
                    if(heightMap[x, y] > heightMap[(int)n.x, (int)n.y] + erosionStrength)
                    {
                        float currentHeight = heightMap[x, y];
                        heightMap[x, y] -= currentHeight * 0.01f;
                        heightMap[(int)n.x, (int)n.y] += currentHeight * erosionAmount;
                    }
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    void Tidal()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                Vector2 thisLocation = new Vector2(x, y);
                List<Vector2> neighbours = GenerateNeighbours(thisLocation, terrainData.heightmapWidth, terrainData.heightmapHeight);
                foreach (Vector2 n in neighbours)
                {
                    //if(heightMap[x, y] > heightMap[(int)n.x, (int)n.y] + erosionStrength)
                    //{
                        if (heightMap[x, y] < waterHeight && heightMap[(int)n.x, (int)n.y] > waterHeight)
                        {
                            //float currentHeight = heightMap[x, y];
                            heightMap[x, y] = waterHeight;
                            heightMap[(int)n.x, (int)n.y] = waterHeight;
                        }
                    //}
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    void River()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        float[,] erosionMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];

        for (int i = 0; i < droplets; i++)
        {
            Vector2 dropletPosition = new Vector2(UnityEngine.Random.Range(0, terrainData.heightmapWidth), UnityEngine.Random.Range(0, terrainData.heightmapHeight));
            erosionMap[(int)dropletPosition.x, (int)dropletPosition.y] = erosionStrength;
            for (int j = 0; j < springsPerRiver; j++)
            {
                erosionMap = RunRiver(dropletPosition, heightMap, erosionMap, terrainData.heightmapWidth, terrainData.heightmapHeight);
            }
        }

        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                if(erosionMap[x, y] > 0)
                {
                    heightMap[x, y] -= erosionMap[x, y];
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    float[,] RunRiver (Vector3 dropletPosition, float[,] heightMap, float[,] erosionMap, int width, int height)
    {
        while (erosionMap[(int)dropletPosition.x, (int)dropletPosition.y] > 0)
        {
            List<Vector2> neighbours = GenerateNeighbours(dropletPosition, width, height);
            neighbours.Shuffle();
            bool foundLower = false;
            foreach (Vector2 n in neighbours)
            {
                if(heightMap[(int)n.x, (int)n.y] < heightMap[(int)dropletPosition.x, (int)dropletPosition.y])
                {
                    erosionMap[(int)n.x, (int)n.y] = erosionMap[(int)dropletPosition.x, (int)dropletPosition.y] - solubility;
                    dropletPosition = n;
                    foundLower = true;
                    break;
                }
            }
            if(!foundLower)
            {
                erosionMap[(int)dropletPosition.x, (int)dropletPosition.y] -= solubility;
            }
        }
        return erosionMap;
    }

    void Wind()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0,terrainData.heightmapWidth, terrainData.heightmapHeight);
        int width = terrainData.heightmapWidth;
        int height = terrainData.heightmapHeight;

        float WindDir = 70;
        float sinAngle = -Mathf.Sin(Mathf.Deg2Rad * WindDir);
        float cosAngle = Mathf.Cos(Mathf.Deg2Rad * WindDir);

        for (int y = -(height - 1) * 2; y <= height * 2; y += 10)
        {
            for (int x = -(width - 1) * 2; x <= width * 2; x += 1)
            {
                float thisNoise = (float)Mathf.PerlinNoise(x * 0.06f, y * 0.06f) * 20 * erosionStrength;
                int nx = (int)x;
                int digy = (int)y + (int)thisNoise;
                int ny = (int)y + 5 + (int)thisNoise;

                Vector2 digCoords = new Vector2(x * cosAngle - digy * sinAngle, digy * cosAngle + x * sinAngle);
                Vector2 pileCoords = new Vector2(nx * cosAngle - ny * sinAngle, ny * cosAngle + nx * sinAngle);

                if (!(pileCoords.x < 0 || pileCoords.x > (width - 1) || pileCoords.y < 0 || pileCoords.y > (height - 1) ||(int)digCoords.x < 0 || (int)digCoords.x > (width - 1) ||(int)digCoords.y < 0 || (int)digCoords.y > (height - 1)))
                {
                    heightMap[(int)digCoords.x, (int)digCoords.y] -= 0.001f;
                    heightMap[(int)pileCoords.x, (int)pileCoords.y] += 0.001f;
                }

            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    float[,] tempHeightMap;
    public void DigCanyon()
    {
        float digDepth = erosionStrength;
        float bankSlope = erosionAmount;
        float maxDepth = 0;
        tempHeightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        int cx = 1;
        int cy = UnityEngine.Random.Range(10, terrainData.heightmapHeight - 10);
        while (cy >= 0 && cy < terrainData.heightmapHeight && cx > 0 && cx < terrainData.heightmapWidth)
        {
            CanyonCrawler(cx, cy, tempHeightMap[cx, cy] - digDepth, bankSlope, maxDepth);
            cx = cx + UnityEngine.Random.Range(-1, 4);
            cy = cy + UnityEngine.Random.Range(-2, 3);
        }
        terrainData.SetHeights(0, 0, tempHeightMap);
    }

    void CanyonCrawler(int x, int y, float height, float slope, float maxDepth)
    {
        if (x < 0 || x >= terrainData.heightmapWidth) return; //off x range of map
        if (y < 0 || y >= terrainData.heightmapHeight) return; //off y range of map
        if (height <= maxDepth) return; //if hit lowest level
        if (tempHeightMap[x, y] <= height) return; //if run into lower elevation

        tempHeightMap[x, y] = height;

        CanyonCrawler(x + 1, y, height + UnityEngine.Random.Range(slope, slope + 0.01f), slope, maxDepth);
        CanyonCrawler(x - 1, y, height + UnityEngine.Random.Range(slope, slope + 0.01f), slope, maxDepth);
        CanyonCrawler(x + 1, y + 1, height + UnityEngine.Random.Range(slope, slope + 0.01f), slope, maxDepth);
        CanyonCrawler(x - 1, y + 1, height + UnityEngine.Random.Range(slope, slope + 0.01f), slope, maxDepth);
        CanyonCrawler(x, y - 1, height + UnityEngine.Random.Range(slope, slope + 0.01f), slope, maxDepth);
        CanyonCrawler(x, y + 1, height + UnityEngine.Random.Range(slope, slope + 0.01f), slope, maxDepth);
    }


    public void Details()
    {
        DetailPrototype[] newDetailPrototypes;
        newDetailPrototypes = new DetailPrototype[details.Count];
        int dindex = 0;
        foreach (Detail d in details)
        {
            newDetailPrototypes[dindex] = new DetailPrototype();
            newDetailPrototypes[dindex].prototype = d.prototype;
            newDetailPrototypes[dindex].prototypeTexture = d.prototypeTexture;
            newDetailPrototypes[dindex].healthyColor = d.healthyColor;
            newDetailPrototypes[dindex].dryColor = d.dryColor;
            newDetailPrototypes[dindex].minHeight = d.minHeightScale;
            newDetailPrototypes[dindex].maxHeight = d.maxHeightScale;
            newDetailPrototypes[dindex].minWidth = d.minWidth;
            newDetailPrototypes[dindex].maxWidth = d.maxWidth;
            newDetailPrototypes[dindex].noiseSpread = d.noiseSpread;
            if(newDetailPrototypes[dindex].prototype)
            {
                newDetailPrototypes[dindex].usePrototypeMesh = true;
                newDetailPrototypes[dindex].renderMode = DetailRenderMode.VertexLit;
            }
            else
            {
                newDetailPrototypes[dindex].usePrototypeMesh = false;
                newDetailPrototypes[dindex].renderMode = DetailRenderMode.GrassBillboard;
            }
            dindex++;
        }
        terrainData.detailPrototypes = newDetailPrototypes;

        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);

        List<DetailPrototype> Detail = new List<DetailPrototype>();
        for (int i = 0; i < terrainData.detailPrototypes.Length; i++)
        {
            int[,] detailMap = new int[terrainData.detailWidth, terrainData.detailHeight];

            for (int y = 0; y < terrainData.detailHeight; y += detailSpacing)
            {
                for (int x = 0; x < terrainData.detailWidth; x += detailSpacing)
                {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) > details[i].density)
                    {
                        continue;
                    }

                    int xHM = (int)(x / (float)terrainData.detailWidth * terrainData.heightmapWidth);
                    int yHM = (int)(y / (float)terrainData.detailHeight * terrainData.heightmapHeight);
  
                    float thisNoise = Utils.Map(Mathf.PerlinNoise(x * details[i].feather, y * details[i].feather), 0, 1, 0.5f, 1);
                    float thisHeightStart = details[i].minHeight * thisNoise - details[i].overlap * thisNoise;
                    float thisHeightEnd = details[i].maxHeight * thisNoise + details[i].overlap * thisNoise;

                    float thisHeight = heightMap[yHM, xHM];
                    float steepness = terrainData.GetSteepness(xHM / (float)terrainData.size.x, yHM / (float)terrainData.size.z);

                    
                    if((thisHeight >= thisHeightStart && thisHeight <= thisHeightEnd) && (steepness >= details[i].minSlope && steepness <= details[i].maxSlope))
                    {
                        detailMap[y, x] = 1;
                    }
                }
            }
            terrainData.SetDetailLayer(0, 0, i, detailMap);
        }
    }

    public void AddNewDetails()
    {
        details.Add(new Detail());
    }

    public void RemoveDetails()
    {
        List<Detail> keptDetails = new List<Detail>();
        for (int i = 0; i < details.Count; i++)
        {
            if (!details[i].remove)
            {
                keptDetails.Add(details[i]);
            }
        } 

        if (keptDetails.Count == 0) //don't want to keep any
        {
            keptDetails.Add(details[0]); // add at least 1
        }
        details = keptDetails; 
    }

    public void AddWater()
    {
        GameObject water = GameObject.Find("water");
        if(!water)
        {
            water = Instantiate(waterGO, this.transform.position, this.transform.rotation);
            water.name = "water";
        } 
        water.transform.position = this.transform.position + new Vector3(terrainData.size.x / 2, waterHeight * terrainData.size.y, terrainData.size.z / 2);
        water.transform.localScale = new Vector3(terrainData.size.x, 1, terrainData.size.z);
    }

    public void drawShoreLine()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);

        int quadCount = 0;
        //GameObject quads = new GameObject("QUADS");
        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                //find spot on shore
                Vector2 thisLocation = new Vector2(x, y);
                List<Vector2> neighbours = GenerateNeighbours(thisLocation, terrainData.heightmapWidth, terrainData.heightmapHeight);

                foreach (Vector2 n in neighbours)
                {
                    if (heightMap[x, y] < waterHeight && heightMap[(int)n.x, (int)n.y] > waterHeight)
                    {
                        //if(quadCount < 1000)
                        //{ 
                            quadCount++;
                            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                            go.transform.localScale *= 5.0f;

                            go.transform.position = this.transform.position + new Vector3(y / (float)terrainData.heightmapHeight * terrainData.size.z, waterHeight * terrainData.size.y, x / (float)terrainData.heightmapWidth * terrainData.size.x);
                            
                            go.transform.LookAt(new Vector3(n.y / (float)terrainData.heightmapHeight * terrainData.size.z, waterHeight * terrainData.size.y, n.x / (float)terrainData.heightmapWidth * terrainData.size.x));

                            go.transform.Rotate(90, 0, 0);

                            go.tag = "Shore";

                            //go.transform.parent = quads.transform;
                        //}
                    }
                }
            }            
        }

        GameObject[] shoreQuads = GameObject.FindGameObjectsWithTag("Shore");
        MeshFilter[] meshFilters = new MeshFilter[shoreQuads.Length];
        for (int m = 0; m < shoreQuads.Length; m++)
        {
            meshFilters[m] = shoreQuads[m].GetComponent<MeshFilter>();
        }
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }

        GameObject currentShoreLine = GameObject.Find("ShoreLine");
        if(currentShoreLine)
        {
            DestroyImmediate(currentShoreLine);
        }
        GameObject shoreLine = new GameObject();
        shoreLine.name = "ShoreLine";
        shoreLine.AddComponent<WaveAnimation>();
        shoreLine.transform.position = this.transform.position;
        shoreLine.transform.rotation = this.transform.rotation;
        MeshFilter thisMF = shoreLine.AddComponent<MeshFilter>();
        thisMF.mesh = new Mesh();
        shoreLine.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);

        MeshRenderer r = shoreLine.AddComponent<MeshRenderer>();
        r.sharedMaterial = shoreLineMaterial;

        for (int sQ = 0; sQ < shoreQuads.Length; sQ++)
        {
            DestroyImmediate(shoreQuads[sQ]);
        }
    }

    public void RandomTerrain()
    {
        float[,] heightMap = GetHeightMap();
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                heightMap[x, z] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void LoadTexture()
    {
        float[,] heightMap = GetHeightMap();
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                heightMap[x, z] += heightMapImage.GetPixel((int)(x * heightMapScale.x), (int)(z * heightMapScale.z)).grayscale * heightMapScale.y;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void ResetTerrain()
    {
        float[,] heightMap;
        heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                heightMap[x, z] = 500;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    void OnEnable()
    {
        Debug.Log("Initialising Terrain Data");
        terrain = this.GetComponent<Terrain>();
        terrainData = Terrain.activeTerrain.terrainData;
    }

    void Start() 
    {
        SplatMaps();    
    }

    public enum TagType { Tag = 0, Layer = 1}
    [SerializeField]
    int terrainLayer = -1;
    void Awake() 
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        SerializedObject tagManager = new SerializedObject(assets[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        tagManager.ApplyModifiedProperties();
        AddTag(tagsProp, "Terrain", TagType.Tag);
        AddTag(tagsProp, "Cloud", TagType.Tag);
        AddTag(tagsProp, "Shore", TagType.Tag);
 
        SerializedProperty layerProp = tagManager.FindProperty("layers");
        terrainLayer = AddTag(layerProp, "Terrain", TagType.Layer);
        tagManager.ApplyModifiedProperties();

        //take this object
        this.gameObject.tag = "Terrain";
        this.gameObject.layer = terrainLayer;
    } 

    int AddTag(SerializedProperty tagsProp, string newTag, TagType tType)
    {
        bool found = false;

        //ensure the tag doesn't already eist
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);

            if(t.stringValue.Equals(newTag)) {found = true; return i;}
        }
        //add your new tag
        if(!found && tType == TagType.Tag)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
        //add new layer
        else if(!found && tType == TagType.Layer)
        {
            for (int j = 8; j < tagsProp.arraySize; j++)
            {
                SerializedProperty newLayer = tagsProp.GetArrayElementAtIndex(j);
                //add layer in next empty slot
                if(newLayer.stringValue == "")
                {
                    newLayer.stringValue = newTag;
                    return j;
                }
            }    
        }
        return -1;
    }
}

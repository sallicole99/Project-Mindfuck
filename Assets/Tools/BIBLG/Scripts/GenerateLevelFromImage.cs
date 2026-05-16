using UnityEngine;
using UnityEditor;
using UnityEditor.AI;
using System.Collections.Generic;

namespace JohnsterSpaceTools
{
    public class GenerateLevelFromImage : EditorWindow
    {
        [System.Serializable]
        public class LevelObject
        {
            public LevelObject(Color c, GameObject g, TileType.TileVariant t)
            {
                tileColor = c;
                tilePrefab = g;
                tileType = t;
            }
            public Color tileColor;
            public GameObject tilePrefab;
            public TileType.TileVariant tileType;
        }

        [MenuItem("JSP's Tools/Baldi's Image Based Level Generator")]
        static void Init()
        {
            GenerateLevelFromImage window = (GenerateLevelFromImage)GetWindow(typeof(GenerateLevelFromImage), false, "BIB Level Generator");
            window.Show();
            window.minSize = new Vector2(300, 400);
        }

        private void OnInspectorUpdate()
        {
            Repaint();

            if (levelTiles.Count < 16)
            {
                Object[] subListObjects = Resources.LoadAll(levelTilesFolder, typeof(GameObject));
                foreach (GameObject subListObject in subListObjects)
                {
                    GameObject lo = subListObject;
                    levelTiles.Add(lo);
                }
            }
            else if (levelTiles.Count > 16)
            {
                levelTiles.Clear();
            }
        }

        private void OnGUI()
        {
            if (!levelImageMap)
            {
                levelImageMap = (Texture2D)EditorGUILayout.ObjectField("Level Image File", levelImageMap, typeof(Texture2D), false);

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Drag your level's layout image file into the box above! Accepted image formats: PNG.", MessageType.None, true);
                EditorGUILayout.Space();
            }
            else
            {
                GUILayout.Label(levelImageMap);

                parentName = EditorGUILayout.TextField("Level Name ", levelImageMap.name);

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("The name of your level shown above is taken from what the level's image file is named.", MessageType.None, true);
                EditorGUILayout.Space();

                levelTilesFolder = EditorGUILayout.TextField("Level Tiles Folder Name ", levelTilesFolder);

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("The name of the folder where the level tile prefabs are loaded from.", MessageType.None, true);
                EditorGUILayout.Space();
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty stringsProperty = so.FindProperty("levelTiles");
            EditorGUILayout.PropertyField(stringsProperty, true);
            so.ApplyModifiedProperties();

            EditorGUILayout.Space();

            if (levelImageMap)
            {
                if (levelObjects.Count < 1)
                {
                    ReadImage();
                }

                GUILayout.Label("=== TILE SETTINGS ===");
                EditorGUILayout.Space();

                GenerateFields();

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                GUILayout.Label("=== LEVEL MATERIALS ===");
                EditorGUILayout.Space();

                floorMat = (Material)EditorGUILayout.ObjectField("Floor Material", floorMat, typeof(Material), false);
                wallMat = (Material)EditorGUILayout.ObjectField("Wall Material", wallMat, typeof(Material), false);
                ceilingMat = (Material)EditorGUILayout.ObjectField("Ceiling Material", ceilingMat, typeof(Material), false);

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("This is where you can select what materials you want to use for the floors, walls, and ceilings of the tiles in your level. If you leave these empty, each tile will generate with default materials.", MessageType.None, true);
                EditorGUILayout.Space();

                GUILayout.Label("=== LEVEL SETTINGS ===");
                EditorGUILayout.Space();

                bakeNavMesh = EditorGUILayout.Toggle("Bake NavMesh?", bakeNavMesh);

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Determines whether a navmesh is baked for the level when it is generated.", MessageType.None, true);
                EditorGUILayout.Space();

                enterPlayMode = EditorGUILayout.Toggle("Enter Play Mode?", enterPlayMode);

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Determines whether play mode starts after the level has been generated.", MessageType.None, true);
                EditorGUILayout.Space();

                useTileLighting = EditorGUILayout.Toggle("Use Tile Lighting?", useTileLighting);

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Determines whether the tile lighting system is used in the level.", MessageType.None, true);
                EditorGUILayout.Space();

                if (useTileLighting)
                {
                    tileLightingColor = (TileLighting.TileLightColor)EditorGUILayout.EnumPopup("Tile Lighting Color", tileLightingColor);

                    if (tileLightingColor == TileLighting.TileLightColor.Custom)
                    {
                        EditorGUILayout.Space();
                        customTileColor = EditorGUILayout.ColorField("Custom Tile Color", customTileColor);
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("Determines what lighting color tiles will be in the level. If you selected Custom, then this will be the color you selected for the Custom Tile Color field.", MessageType.None, true);
                    EditorGUILayout.Space();

                    if (tileLightingColor != TileLighting.TileLightColor.Custom)
                    {
                        minTileLightStrength = EditorGUILayout.FloatField("Min Tile Light Strength", minTileLightStrength);
                        maxTileLightStrength = EditorGUILayout.FloatField("Max Tile Light Strength", maxTileLightStrength);
                    }

                    if (minTileLightStrength > 100f)
                    {
                        minTileLightStrength = 100f;
                    }
                    if (maxTileLightStrength > 100f)
                    {
                        maxTileLightStrength = 100f;
                    }

                    if (minTileLightStrength < 0f)
                    {
                        minTileLightStrength = 0f;
                    }
                    if (maxTileLightStrength < 0f)
                    {
                        maxTileLightStrength = 0f;
                    }

                    if (minTileLightStrength > maxTileLightStrength)
                    {
                        minTileLightStrength = maxTileLightStrength;
                    }

                    if (tileLightingColor != TileLighting.TileLightColor.Custom)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox("Sets the brightness of a tile to a random brightness between the minimum and maximum values. Higher numbers for brighter tiles and lower numbers for darker tiles. If you don't want the brightness of a tile to be randomized, set both numbers to the same value (like 100 and 100 for example).", MessageType.None, true);
                        EditorGUILayout.Space();
                    }
                }

                GUILayout.Label("=== LEVEL DECORATIONS ===");
                EditorGUILayout.Space();

                spawnDecor = EditorGUILayout.Toggle("Spawn Decorations?", spawnDecor);

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("If you want decorations to appear in your level, tick the box above.", MessageType.None, true);
                EditorGUILayout.Space();

                if (spawnDecor)
                {
                    GameObject plantObject = (GameObject)Resources.Load("DecorPlant");
                    if (plantObject != null)
                    {
                        plantPrefab = (GameObject)EditorGUILayout.ObjectField("Plant Prefab", plantObject, typeof(GameObject), false);
                    }
                    else
                    {
                        plantPrefab = (GameObject)EditorGUILayout.ObjectField("Plant Prefab", plantPrefab, typeof(GameObject), false);
                    }
                    EditorGUILayout.Space();

                    randomizePlantPos = EditorGUILayout.Toggle("Randomize Plant Pos?", randomizePlantPos);

                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("Determines whether the position of a plant generated on a tile with a type of Straight will be randomized when the level is started in play mode.", MessageType.None, true);
                    EditorGUILayout.Space();

                    decorSpawnChance = EditorGUILayout.IntField("Decor Spawn Chance", decorSpawnChance);

                    if (decorSpawnChance > 100)
                    {
                        decorSpawnChance = 100;
                    }
                    if (decorSpawnChance < 0)
                    {
                        decorSpawnChance = 0;
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("Determines the chance of decorations spawning on a Straight tile in your level, with 0 being a 0% chance and 100 being a 100% chance.", MessageType.None, true);
                    EditorGUILayout.Space();
                }

                if (GUILayout.Button("Generate Level"))
                {
                    InsertObjects();

                    GenerateLevelFromImage window = (GenerateLevelFromImage)GetWindow(typeof(GenerateLevelFromImage), false, "BIB Level Generator");
                    window.Close();
                }

                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();

            showDebugMessages = EditorGUILayout.Toggle("Show Debug Messages?", showDebugMessages);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Tick the box above to show debug messages in the unity editor console window.", MessageType.None, true);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Baldi's Image Based Level Generator V1.1\n©2022 JohnsterSpaceGames", MessageType.Info, true);
        }

        private void GenerateFields()
        {
            for (int i = 0; i < levelObjects.Count; i++)
            {
                EditorGUILayout.Space();
                levelObjects[i].tileColor = EditorGUILayout.ColorField("Tile Color", levelObjects[i].tileColor);
                levelObjects[i].tilePrefab = (GameObject)EditorGUILayout.ObjectField("Tile Prefab", levelObjects[i].tilePrefab, typeof(GameObject), false);
                levelObjects[i].tileType = (TileType.TileVariant)EditorGUILayout.EnumPopup("Tile Type", levelObjects[i].tileType);
                EditorGUILayout.Space();
            }
        }

        private void ReadImage()
        {
            List<Color> colors = new List<Color>();
            foreach (Color c in levelImageMap.GetPixels())
            {
                if (!colors.Contains(c) && c.a == 1)
                {
                    for (int i = 0; i < levelTiles.Count; i++)
                    {
                        if (levelTiles[i].GetComponentInChildren<Renderer>().sharedMaterial.color == c)
                        {
                            levelTileToApply = levelTiles[i];
                            levelTileTypeToApply = levelTiles[i].GetComponent<TileType>().tileType;
                        }
                    }

                    colors.Add(c);

                    if (levelTileToApply == null)
                    {
                        levelObjects.Add(new LevelObject(c, null, levelTileTypeToApply));
                        if (showDebugMessages)
                        {
                            Debug.Log("No level tile was found to apply to this color!");
                        }
                    }
                    else
                    {
                        levelObjects.Add(new LevelObject(c, levelTileToApply, levelTileTypeToApply));
                        levelTileToApply = null;
                    }
                }
            }
            colors = null;
        }

        private void InsertObjects()
        {
            if (bakeNavMesh)
            {
                NavMeshBuilder.ClearAllNavMeshes();
            }

            DestroyImmediate(GameObject.Find(parentName));
            GameObject parentObj = new GameObject(parentName);
            int h = levelImageMap.height;
            int w = levelImageMap.width;
            float tileSize = 10f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    foreach (LevelObject l in levelObjects)
                    {
                        if (levelImageMap.GetPixel(x, y).Equals(l.tileColor))
                        {
                            GameObject tile = Instantiate(l.tilePrefab, new Vector3(x * tileSize, 0f, y * tileSize), Quaternion.identity);
                            tile.transform.parent = parentObj.transform;

                            if (useTileLighting)
                            {
                                TileLighting tileLighting = tile.GetComponent<TileLighting>();

                                if (tileLighting != null)
                                {
                                    tileLighting.setOnStart = true;

                                    if (tileLightingColor == TileLighting.TileLightColor.Custom)
                                    {
                                        tileLighting.lightColor = customTileColor;
                                    }

                                    tileLighting.tileLightColor = tileLightingColor;
                                    tileLighting.minLightStrength = minTileLightStrength;
                                    tileLighting.maxLightStrength = maxTileLightStrength;
                                }
                            }
                            else
                            {
                                TileLighting tileLighting = tile.GetComponent<TileLighting>();
                                if (tileLighting != null)
                                {
                                    tileLighting.destroyLighting = true;
                                    DestroyImmediate(tileLighting);
                                }
                            }

                            foreach (MeshRenderer renderer in tile.GetComponentsInChildren<MeshRenderer>())
                            {
                                if (renderer.gameObject.name.Contains("Floor") && floorMat != null)
                                {
                                    renderer.sharedMaterial = floorMat;
                                }
                                else if (renderer.gameObject.name.Contains("Wall") && wallMat != null)
                                {
                                    renderer.sharedMaterial = wallMat;
                                }
                                else if (renderer.gameObject.name.Contains("Ceiling") && ceilingMat != null)
                                {
                                    renderer.sharedMaterial = ceilingMat;
                                }
                            }

                            if (spawnDecor)
                            {
                                TileType tt = tile.GetComponent<TileType>();
                                if (tt != null && tt.tileType == TileType.TileVariant.Straight)
                                {
                                    if (plantPrefab != null)
                                    {
                                        if (Random.Range(0, 100) < decorSpawnChance)
                                        {
                                            GameObject plantDecor = Instantiate(plantPrefab);
                                            plantDecor.transform.position = new Vector3(tile.transform.position.x, plantDecor.transform.position.y, tile.transform.position.z);
                                            plantDecor.transform.parent = tile.gameObject.transform;

                                            DecorPlantSetter dps = plantDecor.GetComponent<DecorPlantSetter>();
                                            if (dps != null)
                                            {
                                                dps.randomizePosition = randomizePlantPos;
                                            }
                                        }
                                    }
                                }
                            }

                            TileType tt2 = tile.GetComponent<TileType>();
                            if (tt2 != null && tt2.destroyScript)
                            {
                                DestroyImmediate(tt2);
                            }
                        }
                    }
                }
            }

            if (showDebugMessages)
            {
                Debug.Log("A level with a size of " + h + " by " + w + " has been generated!");
            }

            parentObj.transform.position -= new Vector3(5f, 0f, 5f);

            if (bakeNavMesh)
            {
                NavMeshBuilder.BuildNavMesh();
            }

            EditorApplication.isPlaying = enterPlayMode;
        }

        private string parentName;
        private Texture2D levelImageMap;
        private List<LevelObject> levelObjects = new List<LevelObject>();

        public List<GameObject> levelTiles = new List<GameObject>();
        private GameObject levelTileToApply;
        private string levelTilesFolder = "LevelTiles";

        private Material floorMat;
        private Material wallMat;
        private Material ceilingMat;

        private Vector2 scrollPosition;
        private bool showDebugMessages = false;
        private bool enterPlayMode = false;

        private bool bakeNavMesh = false;
        private bool useTileLighting = true;
        public TileLighting.TileLightColor tileLightingColor;

        public Color customTileColor = Color.white;
        private float minTileLightStrength = 75f;
        private float maxTileLightStrength = 100f;

        public GameObject plantPrefab;
        public bool spawnDecor = true;
        public bool randomizePlantPos = true;

        public int decorSpawnChance = 25;
        private TileType.TileVariant levelTileTypeToApply;
    }
}

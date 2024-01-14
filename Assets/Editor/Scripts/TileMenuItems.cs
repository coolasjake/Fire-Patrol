using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace FirePatrol
{
    public static class TileTester
    {
        const float TILE_SIZE = 30.0f;
        const int POINTS_PER_ROW = 10;
        const int TILES_PER_ROW = POINTS_PER_ROW - 1;
        const string CLEANED_PREFAB_SAVE_DIR = "Assets/Prefabs/Steve/Tiles/Cleaned";

        private static void LoadTile(Transform levelParent, TileTesterSettings settings, int row, int col, int type)
        {
            var pos = new Vector3(col * TILE_SIZE, 0, row * TILE_SIZE);

            TileInfo info;

            if (type > 15)
            {
                type -= 16;
                info = settings.SandTiles[type];
            }
            else
            {
                info = settings.GrassTiles[type];
            }

            var prefab = info.Prefab;
            Assert.IsNotNull(prefab);

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            instance.transform.position = pos;
            instance.transform.rotation = Quaternion.AngleAxis(info.Rotation, Vector3.up);

            instance.transform.SetParent(levelParent, false);
        }

        private static List<List<int>> GenerateRandomPointMap()
        {
            var result = new List<List<int>>();

            for (int i = 0; i < POINTS_PER_ROW; i++)
            {
                var row = new List<int>();

                for (int k = 0; k < POINTS_PER_ROW; k++)
                {
                    row.Add(Random.Range(1, 3));
                }

                result.Add(row);
            }

            return result;
        }

        private static void ChangeNeighbourWaterToSand(int i, int k, List<List<int>> map)
        {
            for (int r = i - 1; r <= i + 1; r++)
            {
                for (int c = k - 1; c <= k + 1; c++)
                {
                    if (r < 0 || r >= POINTS_PER_ROW || c < 0 || c >= POINTS_PER_ROW)
                    {
                        continue;
                    }

                    if (map[r][c] == 2)
                    {
                        map[r][c] = 0;
                    }
                }
            }
        }

        private static void ChangeNeighbourGrassToSand(int i, int k, List<List<int>> map)
        {
            for (int r = i - 1; r <= i + 1; r++)
            {
                for (int c = k - 1; c <= k + 1; c++)
                {
                    if (r < 0 || r >= POINTS_PER_ROW || c < 0 || c >= POINTS_PER_ROW)
                    {
                        continue;
                    }

                    if (map[r][c] == 1)
                    {
                        map[r][c] = 0;
                    }
                }
            }
        }

        private static List<List<int>> GeneratePointMap()
        {
            var map = GenerateRandomPointMap();

            Log.Info("[TileTester] Random point map: {0}", DebugPrint(map));

            for (int i = 0; i < POINTS_PER_ROW; i++)
            {
                for (int k = 0; k < POINTS_PER_ROW; k++)
                {
                    var type = map[i][k];

                    if (type == 1)
                    {
                        ChangeNeighbourWaterToSand(i, k, map);
                    }

                    if (type == 2)
                    {
                        ChangeNeighbourGrassToSand(i, k, map);
                    }
                }
            }

            Log.Info("[TileTester] Fixed point map: {0}", DebugPrint(map));
            return map;
        }

        // 0 = sand, 1 = grass, 2 = water
        private static int ClassifyTile(int[] points)
        {
            int type = 0;

            if (points.Contains(2))
            {
                Assert.That(!points.Contains(1));

                if (points[0] == 0) type += 1;
                if (points[1] == 0) type += 2;
                if (points[2] == 0) type += 4;
                if (points[3] == 0) type += 8;

                type += 16;
            }
            else
            {
                if (points[0] == 1) type += 1;
                if (points[1] == 1) type += 2;
                if (points[2] == 1) type += 4;
                if (points[3] == 1) type += 8;
            }

            return type;
        }

        private static string DebugPrint(List<List<int>> map)
        {
            var result = "";

            for (int i = 0; i < map.Count; i++)
            {
                result += "{ " + string.Join(", ", map[i].Select(x => x.ToString()).ToArray()) + " }, ";
            }

            return result;
        }

        private static List<List<int>> GenerateTileMap()
        {
            var points = GeneratePointMap();

            var tiles = new List<List<int>>();

            for (int i = 0; i < TILES_PER_ROW; i++)
            {
                var row = new List<int>();

                for (int k = 0; k < TILES_PER_ROW; k++)
                {
                    var p1 = points[i][k];
                    var p2 = points[i][k + 1];
                    var p3 = points[i + 1][k + 1];
                    var p4 = points[i + 1][k];

                    row.Add(ClassifyTile(new int[] { p1, p2, p3, p4 }));
                }

                tiles.Add(row);
            }

            return tiles;
        }

        [MenuItem("Svkj/Save Clean Version")]
        public static void SaveCleanVersion()
        {
            var meshMerger = new MAST.Tools.CombineMeshes();
            var sourceObj = Selection.activeGameObject;
            var tileName = sourceObj.name;

            Assert.That(tileName.StartsWith("Grass") || tileName.StartsWith("Sand"));

            var mergedGameObj = meshMerger.MergeMeshes(sourceObj);

            try
            {
                mergedGameObj.name = tileName + "_Merged";

                var meshFilter = mergedGameObj.GetComponentInChildren<MeshFilter>();

                var mesh = meshFilter.sharedMesh;
                var meshAssetPath = CLEANED_PREFAB_SAVE_DIR + $"/{tileName}.asset";
                AssetDatabase.CreateAsset(mesh, meshAssetPath);
                AssetDatabase.SaveAssets();

                meshFilter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshAssetPath);

                var prefabSavePath = CLEANED_PREFAB_SAVE_DIR + $"/{tileName}_Merged.prefab";
                PrefabUtility.SaveAsPrefabAssetAndConnect(mergedGameObj, prefabSavePath, InteractionMode.UserAction);

                Debug.Log($"Successfully saved clean version at {prefabSavePath}");
            }
            finally
            {
                GameObject.DestroyImmediate(mergedGameObj);
            }
        }

        [MenuItem("Svkj/Toggle Tile Test %&i")]
        public static void ToggleTiles()
        {
            LogInitializer.LazyInitialize();

            var scene = EditorSceneManager.GetActiveScene();
            
            Assert.That(scene.name == "scene2");

            foreach (var gameObject in scene.GetRootGameObjects())
            {
                GameObject.DestroyImmediate(gameObject);
            }

            var levelParent = new GameObject("Level");
            var map = GenerateTileMap();
            Log.Info("[TileTester] Tile map: {0}", DebugPrint(map));

            var settings = GameSettings.Instance.TileTester;
            var numTiles = 0;

            for (int r = 0; r < map.Count; r++)
            {
                var row = map[r];

                for (int c = 0; c < row.Count; c++)
                {
                    var type = row[c];
                    LoadTile(levelParent.transform, settings, r, c, type);
                    numTiles += 1;
                }
            }

            Log.Info("[TileTester] Added {0} tiles", numTiles);
        }
    }
}

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Linq;
using System.IO;
using System;

namespace FirePatrol
{
    public static class TileMenuItems
    {
        const string CLEANED_PREFAB_SAVE_DIR = "Prefabs/Steve/Tiles/Cleaned";

        static void AssignLayerRecursively(GameObject obj, string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            Assert.That(layer != -1);
            SetLayer(obj, layer);
        }

        static void SetLayer(GameObject obj, int layer)
        {
            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                SetLayer(child.gameObject, layer);
            }
        }

        [MenuItem("Svkj/Save Clean Version")]
        public static void SaveCleanVersion()
        {
            var meshMerger = new MAST.Tools.CombineMeshes();
            var sourceObj = Selection.activeGameObject;
            var tileName = sourceObj.name;

            Assert.That(tileName.StartsWith("Grass") || tileName.StartsWith("Sand"));

            var mergedGameObj = meshMerger.MergeMeshes(sourceObj);

            AssignLayerRecursively(mergedGameObj, "Terrain");

            try
            {
                mergedGameObj.name = tileName + "_Merged";

                var meshFilter = mergedGameObj.GetComponentInChildren<MeshFilter>();
                var mesh = meshFilter.sharedMesh;

                // What follows here is a ridiculous hack to workaround unity's flakiness with
                // serializing mesh references
                // For unknown reasons, updating the existing mesh asset at the existing path
                // does not work, even if the existing mesh is deleted first
                // Therefore we need to create a new mesh asset at a new asset path
                // and delete the old one
                var meshFileBaseName = $"{tileName}_Merged";
                var outputDir = Path.Combine(Application.dataPath, CLEANED_PREFAB_SAVE_DIR);
                var deletedExistingMesh = false;

                foreach (var path in Directory.GetFiles(outputDir))
                {
                    var fileName = Path.GetFileName(path);
                    if (fileName.StartsWith(meshFileBaseName) && fileName.EndsWith(".asset"))
                    {
                        Assert.That(!deletedExistingMesh);
                        File.Delete(path);
                        Log.Debug($"[TileMenuItems] Deleted existing mesh at path {path}");

                        var metaPath = path + ".meta";

                        if (File.Exists(metaPath))
                        {
                            File.Delete(metaPath);
                        }

                        deletedExistingMesh = true;
                    }
                }

                AssetDatabase.Refresh();

                var meshAssetPath = "Assets/" + CLEANED_PREFAB_SAVE_DIR + $"/{tileName}_Merged_{UnityEngine.Random.Range(1, 99999999)}.asset";
                AssetDatabase.CreateAsset(mesh, meshAssetPath);
                AssetDatabase.SaveAssets();

                meshFilter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshAssetPath);

                meshFilter.gameObject.AddComponent<MeshCollider>();

                var prefabSavePath = "Assets/" + CLEANED_PREFAB_SAVE_DIR + $"/{tileName}_Merged.prefab";
                PrefabUtility.SaveAsPrefabAssetAndConnect(mergedGameObj, prefabSavePath, InteractionMode.UserAction);

                Debug.Log($"Successfully saved clean version at {prefabSavePath}");
            }
            finally
            {
                GameObject.DestroyImmediate(mergedGameObj);
            }
        }

        const float SnapGridSize = 0.25f;

        private static Vector3 SnapToGrid(Vector3 pos)
        {
            pos.x = Mathf.Round(pos.x / SnapGridSize) * SnapGridSize;
            pos.y = Mathf.Round(pos.y / SnapGridSize) * SnapGridSize;
            pos.z = Mathf.Round(pos.z / SnapGridSize) * SnapGridSize;
            return pos;
        }

        private static List<Transform> GetRootTransforms()
        {
            return SceneManager.GetActiveScene().GetRootGameObjects().Select(x => x.transform).ToList();
        }

        [MenuItem("Svkj/SnapToGrid %&r")]
        public static void SnapToGrid()
        {
            var numUpdated = 0;

            foreach (var root in GetRootTransforms())
            {
                if (!root.gameObject.activeInHierarchy || root.transform.position.magnitude > 0)
                {
                    continue;
                }

                foreach (Transform child in root.transform)
                {
                    if (!child.gameObject.activeInHierarchy || child.transform.position.magnitude > 0)
                    {
                        continue;
                    }

                    foreach (Transform grandchild in child.transform)
                    {
                        if (!grandchild.gameObject.activeInHierarchy)
                        {
                            continue;
                        }

                        var desiredScale = new Vector3(Mathf.Round(grandchild.localScale.x), Mathf.Round(grandchild.localScale.y), Mathf.Round(grandchild.localScale.z));

                        if (desiredScale != grandchild.localScale)
                        {
                            grandchild.localScale = desiredScale;
                            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                            Debug.LogFormat("Upated grandchild scale '{0}' from {1} to {2}", grandchild.name, grandchild.localScale, desiredScale);
                            numUpdated = numUpdated + 1;
                        }

                        var snapPos = SnapToGrid(grandchild.position);

                        if (grandchild.position != snapPos)
                        {
                            grandchild.position = snapPos;
                            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                            Debug.LogFormat("Upated grandchild '{0}' from {1} to {2}", grandchild.name, grandchild.position, snapPos);
                            numUpdated = numUpdated + 1;
                        }

                        var expectedGrandchildScale = new Vector3(100, 100, 100);

                        foreach (Transform greatGrandChild in grandchild.transform)
                        {
                            var grandchildScale = greatGrandChild.localScale;

                            if ((grandchildScale - expectedGrandchildScale).magnitude > 0.01)
                            {
                                Debug.Log($"Expected greatGrandChild scale to be 100, 100, 100 but found {grandchildScale}", greatGrandChild);
                            }
                        }
                    }
                }
            }

            Debug.Log($"Snapped {numUpdated} properties to grid");
        }
    }
}

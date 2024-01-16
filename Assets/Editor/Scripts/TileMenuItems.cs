using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Linq;

namespace FirePatrol
{
    public static class TileMenuItems
    {
        const string CLEANED_PREFAB_SAVE_DIR = "Assets/Prefabs/Steve/Tiles/Cleaned";

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
                PrefabUtility.SaveAsPrefabAsset(mergedGameObj, prefabSavePath);

                Debug.Log($"Successfully saved clean version at {prefabSavePath}");
            }
            finally
            {
                GameObject.DestroyImmediate(mergedGameObj);
            }
        }

        const float SnapGridSize = 0.5f;

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

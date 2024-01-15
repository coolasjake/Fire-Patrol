using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace FirePatrol
{
    [Serializable]
    public class TileSettings
    {
        public float TreeDensity = 0;
        public int Variation = 0;
    }

    public class LevelEditorWindowImpl : IDisposable
    {
        enum BrushType { Water, Grass, Sand, Trees }

        BrushType? _currentBrush = null;
        Dictionary<BrushType, Texture2D> _brushCursors;
        Action<SceneView> _onSceneGuiHandler;

        private void LoadCursorTextures()
        {
            _brushCursors = new Dictionary<BrushType, Texture2D>();

            foreach (var brushType in Enum.GetValues(typeof(BrushType)).OfType<BrushType>())
            {
                var assetPath = $"Assets/Editor/Textures/Cursors/{brushType.ToString()}.png";
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

                if (texture == null)
                {
                    Log.Warn("[LevelEditorWindowImpl] Expected to find cursor image at {0} but none was found", assetPath);
                }
                else
                {
                    _brushCursors[brushType] = texture;
                }
            }
        }

        public void Initialize()
        {
            LogInitializer.LazyInitialize();
            LoadCursorTextures();

            _onSceneGuiHandler = (x) => { OnSceneGUI(x); };
            SceneView.duringSceneGui += _onSceneGuiHandler;
        }

        public void Dispose()
        {
            SceneView.duringSceneGui -= _onSceneGuiHandler;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (_currentBrush.HasValue && _brushCursors.TryGetValue(_currentBrush.Value, out var cursorTexture))
            {
                Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, sceneView.position.width, sceneView.position.height), MouseCursor.CustomCursor);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }

            var evt = Event.current;

            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);

                Vector3 rayOrigin = ray.origin;
                Vector3 rayDirection = ray.direction;

                if (Mathf.Abs(rayDirection.y) < 1e-5)
                {
                    return;
                }

                float distance = -rayOrigin.y / rayDirection.y;
                Vector3 intersectionPoint = rayOrigin + rayDirection * distance;

                if (distance >= 0)
                {
                    Log.Info("[LevelEditorWindowImpl] intersectionPoint = {0}", intersectionPoint);
                    evt.Use();
                }
            }
        }

        public void Update() { }

        public void OnGUI(Rect windowRect)
        {
            GUILayout.BeginArea(windowRect);
            GUILayout.Space(10);

            GUILayout.Label("Select Brush:", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();

            var activeButtonStyle = new GUIStyle(GUI.skin.button);
            var selectedColor = Color.green;
            activeButtonStyle.normal.textColor = selectedColor;
            activeButtonStyle.hover.textColor = selectedColor;
            activeButtonStyle.active.textColor = selectedColor;

            var regularButtonStyle = GUI.skin.button;

            if (GUILayout.Button("Water", _currentBrush == BrushType.Water ? activeButtonStyle : regularButtonStyle))
            {
                _currentBrush = BrushType.Water;
            }

            if (GUILayout.Button("Grass", _currentBrush == BrushType.Grass ? activeButtonStyle : regularButtonStyle))
            {
                _currentBrush = BrushType.Grass;
            }

            if (GUILayout.Button("Sand", _currentBrush == BrushType.Sand ? activeButtonStyle : regularButtonStyle))
            {
                _currentBrush = BrushType.Sand;
            }

            if (GUILayout.Button("Trees", _currentBrush == BrushType.Trees ? activeButtonStyle : regularButtonStyle))
            {
                _currentBrush = BrushType.Trees;
            }

            if (GUILayout.Button("None", _currentBrush == null ? activeButtonStyle : regularButtonStyle))
            {
                _currentBrush = null;
            }

            GUILayout.EndHorizontal();

            // // Tile Settings
            // if (currentTileSettings != null)
            // {
            //     GUILayout.Label("Tile Settings:", EditorStyles.boldLabel);
            //     currentTileSettings.TreeDensity = EditorGUILayout.FloatField("Tree Density", currentTileSettings.TreeDensity);
            //     currentTileSettings.Variation = EditorGUILayout.IntField("Variation", currentTileSettings.Variation);
            // }

            GUILayout.EndArea();
        }
    }
}

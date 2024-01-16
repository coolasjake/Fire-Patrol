using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Linq;

namespace FirePatrol
{
    public class LevelEditorWindowImpl : IDisposable
    {
        enum BrushType {
            Water,
            Grass,
            Sand,
            Trees,
            Rocks,
            GrassTufts,
            Flowers,
            Mushrooms,
            PropEraser,
        }

        static readonly Color SELECTED_COLOR = Color.green;
        const int TILE_TYPE_ALL_GRASS = 15;
        const float MIN_TILE_SCALE = 0.1f;

        BrushType? _currentBrush = null;
        Dictionary<BrushType, Texture2D> _brushCursors;
        Action<SceneView> _onSceneGuiHandler;
        GUIStyle _selectedButtonStyle;
        LevelEditorSettings _settings;
        TileData _highlightedTile;
        PointData _highlightedPoint;
        bool _isPainting;
        int _regeneratePointsPerRow = 0;
        float _regenerateTileScale = 0;

        void LoadCursorTextures()
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

        LevelData TryGetLevelData()
        {
            return EditorSceneManager.GetActiveScene().GetRootGameObjects().Select(x => x.GetComponent<LevelData>()).FirstOrDefault(x => x != null);
        }

        LevelData GetLevelData()
        {
            var levelData = TryGetLevelData();
            Assert.IsNotNull(levelData);
            return levelData;
        }

        bool CheckIsSceneValid()
        {
            return TryGetLevelData() != null;
        }

        public void Initialize()
        {
            LogInitializer.LazyInitialize();
            LoadCursorTextures();

            _onSceneGuiHandler = (x) => { OnGuiSceneTab(x); };
            _settings = LevelEditorSettings.Instance;

            SceneView.duringSceneGui += _onSceneGuiHandler;
        }

        public void Dispose()
        {
            SceneView.duringSceneGui -= _onSceneGuiHandler;
        }

        Vector3? TryGetGridMouseIntersectionPoint(Vector2 mousePos)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

            Vector3 rayOrigin = ray.origin;
            Vector3 rayDirection = ray.direction;

            if (Mathf.Abs(rayDirection.y) < 1e-5)
            {
                return null;
            }

            float distance = -rayOrigin.y / rayDirection.y;
            Vector3 intersectionPoint = rayOrigin + rayDirection * distance;

            if (distance >= 0)
            {
                return intersectionPoint;
            }

            return null;
        }

        PointData GetClosestPoint(Vector3 pos)
        {
            var levelData = GetLevelData();

            var points = levelData.Points;

            if (points.Count == 0)
            {
                return null;
            }

            var closestPoint = points[0];
            var closestDistance = Vector3.Distance(closestPoint.Position, pos);

            for (int i = 1; i < points.Count; i++)
            {
                var point = points[i];
                var distance = Vector3.Distance(point.Position, pos);

                if (distance < closestDistance)
                {
                    closestPoint = point;
                    closestDistance = distance;
                }
            }

            return closestPoint;
        }

        TileData GetClosestTile(Vector3 pos)
        {
            var levelData = GetLevelData();

            var tiles = levelData.Tiles;

            if (tiles.Count == 0)
            {
                return null;
            }

            var closestTile = tiles[0];
            var closestDistance = Vector3.Distance(closestTile.CenterPosition, pos);

            for (int i = 1; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                var distance = Vector3.Distance(tile.CenterPosition, pos);

                if (distance < closestDistance)
                {
                    closestTile = tile;
                    closestDistance = distance;
                }
            }

            return closestTile;
        }

        PointData TryGetPointAtScreenPoint(Vector2 screenPos, LevelData levelData)
        {
            var gridPoint = TryGetGridMouseIntersectionPoint(screenPos);

            if (!gridPoint.HasValue)
            {
                return null;
            }

            var point = GetClosestPoint(gridPoint.Value);

            if (Vector3.Distance(point.Position, gridPoint.Value) < levelData.TileSize * 0.75)
            {
                return point;
            }

            return null;
        }

        TileData TryGetTileAtScreenPoint(Vector2 screenPos, LevelData levelData)
        {
            var gridPoint = TryGetGridMouseIntersectionPoint(screenPos);

            if (!gridPoint.HasValue)
            {
                return null;
            }

            var tile = GetClosestTile(gridPoint.Value);

            if (Vector3.Distance(tile.CenterPosition, gridPoint.Value) < levelData.TileSize * 0.5)
            {
                return tile;
            }

            return null;
        }

        PointTypes BrushTypeToPointType(BrushType brushType)
        {
            switch (brushType)
            {
                case BrushType.Water:
                    return PointTypes.Water;

                case BrushType.Grass:
                    return PointTypes.Grass;

                case BrushType.Sand:
                    return PointTypes.Sand;

                default:
                    throw new ArgumentException($"Unexpected brush type {brushType}");
            }
        }

        int ClassifyTileType(TileData tile, LevelData levelData)
        {
            var p1 = levelData.GetPointData(tile.Row, tile.Col).Type;
            var p2 = levelData.GetPointData(tile.Row, tile.Col + 1).Type;
            var p3 = levelData.GetPointData(tile.Row + 1, tile.Col + 1).Type;
            var p4 = levelData.GetPointData(tile.Row + 1, tile.Col).Type;

            var allTypes = new [] { p1, p2, p3, p4 };
            int type = 0;

            if (allTypes.Contains(PointTypes.Water) )
            {
                Assert.That(!allTypes.Contains(PointTypes.Grass));

                if (p1 == PointTypes.Sand) type += 1;
                if (p2 == PointTypes.Sand) type += 2;
                if (p3 == PointTypes.Sand) type += 4;
                if (p4 == PointTypes.Sand) type += 8;

                type += 16;
            }
            else
            {
                if (p1 == PointTypes.Grass) type += 1;
                if (p2 == PointTypes.Grass) type += 2;
                if (p3 == PointTypes.Grass) type += 4;
                if (p4 == PointTypes.Grass) type += 8;
            }

            return type;
        }

        List<PointData> GetPointNeighbours(PointData pointData, LevelData levelData)
        {
            var neighbours = new List<PointData>();

            for (int i = pointData.Row - 1; i <= pointData.Row + 1; i++)
            {
                for (int k = pointData.Col - 1; k <= pointData.Col + 1; k++)
                {
                    var neighbour = levelData.TryGetPointData(i, k);

                    if (neighbour != null)
                    {
                        neighbours.Add(neighbour);
                    }
                }
            }

            return neighbours;
        }

        List<TileData> GetAssociatedTiles(PointData pointData, LevelData levelData)
        {
            var tiles = new List<TileData>();

            for (int i = pointData.Row - 1; i <= pointData.Row; i++)
            {
                for (int k = pointData.Col - 1; k <= pointData.Col; k++)
                {
                    var tile = levelData.TryGetTileData(i, k);

                    if (tile != null)
                    {
                        tiles.Add(tile);
                    }
                }
            }

            return tiles;
        }

        void OnGuiSceneTabRepaint(SceneView sceneView, LevelData levelData)
        {
            if (_currentBrush.HasValue)
            {
                if (_brushCursors.TryGetValue(_currentBrush.Value, out var cursorTexture))
                {
                    Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
                    EditorGUIUtility.AddCursorRect(new Rect(0, 0, sceneView.position.width, sceneView.position.height), MouseCursor.CustomCursor);
                }
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                _highlightedTile = null;
                _highlightedPoint = null;
            }

            Handles.color = Color.yellow;

            if (_highlightedTile != null)
            {
                Handles.DrawWireCube(_highlightedTile.CenterPosition, new Vector3(levelData.TileSize, 0, levelData.TileSize));
            }

            if (_highlightedPoint != null)
            {
                Handles.DrawWireArc(_highlightedPoint.Position, Vector3.up, Vector3.forward, 360, levelData.TileSize * 0.25f);
            }
        }

        void ApplyBrushToPoint(BrushType brush, PointData pointData)
        {
            var newPointType = BrushTypeToPointType(brush);

            if (pointData.Type == newPointType)
            {
                return;
            }

            Log.Info("[LevelEditorWindowImpl] Applying brush {0} to point {1}", brush, pointData.Id);

            var levelData = GetLevelData();

            var dirtyPoints = new List<PointData>() { pointData };
            pointData.Type = newPointType;

            if (newPointType == PointTypes.Grass)
            {
                foreach (var neighbour in GetPointNeighbours(pointData, levelData))
                {
                    if (neighbour.Type == PointTypes.Water)
                    {
                        neighbour.Type = PointTypes.Sand;
                        dirtyPoints.Add(neighbour);
                    }
                }
            }
            else if (newPointType == PointTypes.Water)
            {
                foreach (var neighbour in GetPointNeighbours(pointData, levelData))
                {
                    if (neighbour.Type == PointTypes.Grass)
                    {
                        neighbour.Type = PointTypes.Sand;
                        dirtyPoints.Add(neighbour);
                    }
                }
            }

            var dirtyTiles = new HashSet<int>();

            foreach (var dirtyPoint in dirtyPoints)
            {
                foreach (var tileData in GetAssociatedTiles(dirtyPoint, levelData))
                {
                    dirtyTiles.Add(tileData.Id);
                }
            }

            Log.Debug("[LevelEditorWindowImpl] Found {0} dirty tiles after changing point: {1}", dirtyTiles.Count, string.Join(", ", dirtyTiles.Select(x => x.ToString()).ToArray()));

            foreach (var id in dirtyTiles)
            {
                var tile = levelData.GetTileDataById(id);
                var newTileType = ClassifyTileType(tile, levelData);

                if (newTileType != tile.TileType)
                {
                    tile.TileType = newTileType;
                    UpdateTileModel(tile, levelData);

                    Log.Debug("[LevelEditorWindowImpl] Updated tile model for tile {0}", tile.Id);
                }
            }
        }

        void ApplyBrush(Vector2 screenPos, LevelData levelData)
        {
            if (!_currentBrush.HasValue)
            {
                return;
            }

            var currentBrush = _currentBrush.Value;

            if (currentBrush == BrushType.Trees)
            {
                var tileUnderMouse = TryGetTileAtScreenPoint(screenPos, levelData);

                if (tileUnderMouse != null)
                {
                    // currentBrush;
                    // Log.Info("[LevelEditorWindowImpl] intersectionPoint = {0}", gridPoint);
                    Log.Info("[LevelEditorWindowImpl] todo - increase trees");
                }
            }
            else
            {
                var pointUnderMouse = TryGetPointAtScreenPoint(screenPos, levelData);

                if (pointUnderMouse != null)
                {
                    ApplyBrushToPoint(currentBrush, pointUnderMouse);
                }
            }
        }

        void HighlightControlsAt(Vector2 screenPos, LevelData levelData)
        {
            if (!_currentBrush.HasValue)
            {
                return;
            }

            var currentBrush = _currentBrush.Value;

            if (currentBrush == BrushType.Trees)
            {
                TileData tileUnderMouse = TryGetTileAtScreenPoint(screenPos, levelData);

                if (tileUnderMouse != _highlightedTile)
                {
                    _highlightedTile = tileUnderMouse;
                    SceneView.RepaintAll();
                }

                if (_highlightedPoint != null)
                {
                    _highlightedPoint = null;
                    SceneView.RepaintAll();
                }
            }
            else
            {
                PointData pointUnderMouse = TryGetPointAtScreenPoint(screenPos, levelData);

                if (pointUnderMouse != _highlightedPoint)
                {
                    _highlightedPoint = pointUnderMouse;
                    SceneView.RepaintAll();
                }

                if (_highlightedTile != null)
                {
                    _highlightedTile = null;
                    SceneView.RepaintAll();
                }
            }
        }

        void OnGuiSceneTab(SceneView sceneView)
        {
            if (!CheckIsSceneValid())
            {
                return;
            }

            var evt = Event.current;
            var levelData = GetLevelData();

            switch (evt.type)
            {
                case EventType.MouseDrag:
                {
                    if (_isPainting)
                    {
                        HighlightControlsAt(evt.mousePosition, levelData);
                        ApplyBrush(evt.mousePosition, levelData);
                        evt.Use();
                    }
                    break;
                }
                case EventType.MouseMove:
                {
                    HighlightControlsAt(evt.mousePosition, levelData);
                    break;
                }
                case EventType.Repaint:
                {
                    OnGuiSceneTabRepaint(sceneView, levelData);
                    break;
                }
                case EventType.MouseUp:
                {
                    if (_isPainting && evt.button == 0 && !evt.alt)
                    {
                        _isPainting = false;
                        evt.Use();
                    }

                    break;
                }
                case EventType.MouseDown:
                {
                    if (_currentBrush.HasValue && evt.button == 0 && !evt.alt)
                    {
                        ApplyBrush(evt.mousePosition, levelData);
                        _isPainting = true;
                        evt.Use();
                    }

                    break;
                }
            }
        }

        public void Update() { }

        GameObject InstantiateTile(Vector3 center, int type, LevelData levelData)
        {
            var tileSettings = _settings.TileSettings;

            float rotation;
            List<GameObject> prefabVariations;

            if (type > 15)
            {
                type -= 16;

                var linkInfo = tileSettings.SandPrefabLinks[type];
                rotation = linkInfo.Rotation;
                var prefabIndex = linkInfo.PrefabIndex;
                prefabVariations = tileSettings.SandPrefabVariations.Where(x => x.PrefabIndex == prefabIndex).Single().Prefabs;
            }
            else
            {
                var linkInfo = tileSettings.GrassPrefabLinks[type];
                rotation = linkInfo.Rotation;
                var prefabIndex = linkInfo.PrefabIndex;
                prefabVariations = tileSettings.GrassPrefabVariations.Where(x => x.PrefabIndex == prefabIndex).Single().Prefabs;
            }

            var prefab = prefabVariations[UnityEngine.Random.Range(0, prefabVariations.Count)];
            Assert.IsNotNull(prefab);

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            instance.transform.position = center;
            instance.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.up);
            instance.transform.localScale = new Vector3(levelData.TileScale, levelData.TileScale, levelData.TileScale);

            instance.transform.SetParent(GetTilesParent().transform, false);
            return instance;
        }

        void UpdateTileModel(TileData tileData, LevelData levelData)
        {
            if (tileData.Model != null)
            {
                GameObject.DestroyImmediate(tileData.Model);
            }

            tileData.Model = InstantiateTile(tileData.CenterPosition, tileData.TileType, levelData);
            tileData.Model.name = "Tile" + tileData.Id;
        }

        GameObject GetTilesParent()
        {
            var tilesParent = EditorSceneManager.GetActiveScene().GetRootGameObjects().Where(x => x.name == "Tiles").SingleOrDefault();

            if (tilesParent == null)
            {
                tilesParent = new GameObject("Tiles");
            }

            return tilesParent;
        }

        void RegenerateTiles()
        {
            Log.Info("[LevelEditorWindowImpl] Generating tiles...");

            Assert.That(_regeneratePointsPerRow >= 2);
            Assert.That(_regenerateTileScale >= 0);

            var levelData = GetLevelData();

            foreach (GameObject child in UnityUtil.GetDirectChildren(GetTilesParent()))
            {
                GameObject.DestroyImmediate(child);
            }

            _highlightedTile = null;
            _highlightedPoint = null;

            levelData.PointsPerRow = _regeneratePointsPerRow;
            levelData.TileScale = _regenerateTileScale;

            levelData.Tiles.Clear();
            levelData.Points.Clear();

            var pointsPerRow = levelData.PointsPerRow;

            if (levelData.PointsPerRow < 2)
            {
                pointsPerRow = 2;
                levelData.PointsPerRow = pointsPerRow;
            }

            Assert.That(pointsPerRow >= 2);

            var tilesPerRow = pointsPerRow - 1;
            var cellSize = levelData.TileSize;
            var totalSize = cellSize * tilesPerRow;
            var globalOffset = new Vector3(-totalSize / 2.0f, 0, -totalSize / 2.0f);

            for (int i = 0; i < pointsPerRow; i++)
            {
                for (int k = 0; k < pointsPerRow; k++)
                {
                    var pointData = new PointData()
                    {
                        Type = PointTypes.Grass,
                        Position = new Vector3(k * cellSize, 0, i * cellSize) + globalOffset,
                        Row = i,
                        Col = k,
                        Id = levelData.Points.Count,
                    };

                    levelData.Points.Add(pointData);
                }
            }

            var centerOffset = new Vector3(cellSize / 2, 0, cellSize / 2);

            for (int i = 0; i < tilesPerRow; i++)
            {
                for (int k = 0; k < tilesPerRow; k++)
                {
                    var center = new Vector3(k * cellSize, 0, i * cellSize) + centerOffset;

                    var tileData = new TileData()
                    {
                        TileType = TILE_TYPE_ALL_GRASS,
                        CenterPosition = center + globalOffset,
                        Row = i,
                        Col = k,
                        Id = levelData.Tiles.Count,
                    };

                    UpdateTileModel(tileData, levelData);
                    levelData.Tiles.Add(tileData);
                }
            }

            Log.Info("[LevelEditorWindowImpl] Successfully generated {0} tiles", levelData.Tiles.Count);
        }

        GUIStyle GetSelectedButtonStyle()
        {
            if (_selectedButtonStyle == null)
            {
                _selectedButtonStyle = new GUIStyle(GUI.skin.button);
                _selectedButtonStyle.normal.textColor = SELECTED_COLOR;
                _selectedButtonStyle.hover.textColor = SELECTED_COLOR;
                _selectedButtonStyle.active.textColor = SELECTED_COLOR;
            }

            return _selectedButtonStyle;
        }

        void OnGuiBrushSelect()
        {
            var regularButtonStyle = GUI.skin.button;

            using (GuiHelper.VerticalBox("Paint Brush"))
            {
                if (GUILayout.Button("None", _currentBrush == null ? GetSelectedButtonStyle() : regularButtonStyle))
                {
                    _currentBrush = null;
                }

                GUILayout.Label("Tiles", EditorStyles.boldLabel);

                using (GuiHelper.HorizontalBlock())
                {
                    if (GUILayout.Button("Water", _currentBrush == BrushType.Water ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _currentBrush = BrushType.Water;
                    }

                    if (GUILayout.Button("Grass", _currentBrush == BrushType.Grass ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _currentBrush = BrushType.Grass;
                    }

                    if (GUILayout.Button("Sand", _currentBrush == BrushType.Sand ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _currentBrush = BrushType.Sand;
                    }
                }

                GUILayout.Label("Props", EditorStyles.boldLabel);

                using (GuiHelper.HorizontalBlock())
                {
                    if (GUILayout.Button("Trees", _currentBrush == BrushType.Trees ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _currentBrush = BrushType.Trees;
                    }

                    if (GUILayout.Button("Rocks", _currentBrush == BrushType.Rocks ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _currentBrush = BrushType.Rocks;
                    }

                    if (GUILayout.Button("Grass Tufts", _currentBrush == BrushType.GrassTufts ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _currentBrush = BrushType.GrassTufts;
                    }
                }

                using (GuiHelper.HorizontalBlock())
                {
                    if (GUILayout.Button("Flowers", _currentBrush == BrushType.Flowers ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _currentBrush = BrushType.Flowers;
                    }

                    if (GUILayout.Button("Mushrooms", _currentBrush == BrushType.Mushrooms ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _currentBrush = BrushType.Mushrooms;
                    }

                    if (GUILayout.Button("Eraser", _currentBrush == BrushType.PropEraser ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _currentBrush = BrushType.PropEraser;
                    }
                }
            }
        }

        void OnGuiRootPanel()
        {
            OnGuiBrushSelect();

            GUILayout.Space(10);

            using (GuiHelper.VerticalBox("Regenerate Map"))
            {
                var levelData = GetLevelData();

                if (_regeneratePointsPerRow == 0)
                {
                    _regeneratePointsPerRow = levelData.PointsPerRow;
                }

                if (_regenerateTileScale == 0)
                {
                    _regenerateTileScale = levelData.TileScale;
                }

                using (GuiHelper.HorizontalBlock())
                {
                    GUILayout.Label("Points Per Row:", EditorStyles.boldLabel);

                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        _regeneratePointsPerRow--;
                    }

                    _regeneratePointsPerRow = EditorGUILayout.IntField(_regeneratePointsPerRow, GUILayout.Width(40));

                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        _regeneratePointsPerRow++;
                    }
                }

                using (GuiHelper.HorizontalBlock())
                {
                    GUILayout.Label("Tile Scale", EditorStyles.boldLabel);

                    var increment = 0.1f;

                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        _regenerateTileScale = Mathf.Max(MIN_TILE_SCALE, _regenerateTileScale - increment);
                    }

                    _regenerateTileScale = EditorGUILayout.FloatField(_regenerateTileScale, GUILayout.Width(40));

                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        _regenerateTileScale += increment;
                    }
                }

                if (GUILayout.Button("Regenerate"))
                {
                    RegenerateTiles();
                }
            }
        }

        public void OnGUI(Rect windowRect)
        {
            using (GuiHelper.AreaBlock(windowRect))
            {
                if (!CheckIsSceneValid())
                {
                    GUILayout.Space(10);

                    using (GuiHelper.HorizontalBlock())
                    {
                        GUILayout.Space(10);
                        GUILayout.Label("Scene is not a valid FirePatrol level\nIs LevelData game object present?", EditorStyles.boldLabel);
                    }

                    return;
                }

                var marginSize = 10.0f;

                using (GuiHelper.AreaBlock(windowRect))
                {
                    GUILayout.Space(marginSize);

                    using (GuiHelper.HorizontalBlock())
                    {
                        GUILayout.Space(marginSize);

                        using (GuiHelper.VerticalBlock())
                        {
                            OnGuiRootPanel();
                        }

                        GUILayout.Space(marginSize);
                    }

                    GUILayout.Space(marginSize);
                }
            }
        }
    }
}

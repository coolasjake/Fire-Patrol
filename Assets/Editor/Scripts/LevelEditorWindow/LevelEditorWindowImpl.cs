using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

namespace FirePatrol
{
    public class LevelEditorWindowImpl : IDisposable
    {
        public enum BrushTypes
        {
            Grass,
            Water,

            Trees1,
            Trees2,
            Flowers,
            Mushrooms,
            GrassTufts,
            Rocks,
            PropEraser,
        }

        static readonly Color SELECTED_COLOR = Color.green;
        const int TILE_TYPE_ALL_GRASS = 15;
        const int TILE_TYPE_ALL_WATER = 0;

        const float MIN_TILE_SCALE = 0.1f;
        const float LOW_GRASS_HEIGHT = 1.25f;
        const float HIGH_GRASS_HEIGHT = 2.25f;

        BrushTypes? _currentBrush = null;
        PointTypes _defaultPointType = PointTypes.Grass;
        Dictionary<BrushTypes, Texture2D> _brushCursors;
        Action<SceneView> _onSceneGuiHandler;
        GUIStyle _selectedButtonStyle;
        LevelEditorSettings _settings;
        TileData _highlightedTile;
        PointData _highlightedPoint;
        Vector3? _highlightedPropPosition;

        bool _isPainting;
        int _regeneratePointsPerRow = 0;
        float _regenerateTileScale = 0;
        float _treeBrushSize = 25.0f;
        float _nonTreeBrushSize = 5.0f;

        void MarkSceneDirty()
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        void LoadCursorTextures()
        {
            _brushCursors = new Dictionary<BrushTypes, Texture2D>();

            foreach (var brushType in Enum.GetValues(typeof(BrushTypes)).OfType<BrushTypes>())
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

        Vector3? TryGetTerrainIntersectionPoint(Vector2 mousePos)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

            int layerMask = LayerMask.GetMask("Terrain");

            if (Physics.Raycast(ray.origin, ray.direction, out var hit, Mathf.Infinity, layerMask))
            {
                return hit.point;
            }

            return null;
        }

        Vector3? ProjectOntoTerrain(Vector3 pos)
        {
            Ray ray = new Ray(pos + Vector3.up, Vector3.down);
            int layerMask = LayerMask.GetMask("Terrain");

            if (Physics.Raycast(ray.origin, ray.direction, out var hit, Mathf.Infinity, layerMask))
            {
                return hit.point;
            }

            return null;
        }

        Vector3? TryGetGridMouseIntersectionPoint(Vector2 mousePos, float height)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

            Vector3 rayOrigin = ray.origin;
            Vector3 rayDirection = ray.direction;

            if (Mathf.Abs(rayDirection.y) < 1e-5)
            {
                return null;
            }

            float distance = (height - rayOrigin.y) / rayDirection.y;

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

        TileData TryGetClosestTile(Vector3 pos)
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
            var gridPoint = TryGetGridMouseIntersectionPoint(screenPos, 0);

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
            var gridPoint = TryGetGridMouseIntersectionPoint(screenPos, 0);

            if (!gridPoint.HasValue)
            {
                return null;
            }

            var tile = TryGetClosestTile(gridPoint.Value);

            if (tile == null)
            {
                return null;
            }

            if (Vector3.Distance(tile.CenterPosition, gridPoint.Value) < levelData.TileSize * 0.5)
            {
                return tile;
            }

            return null;
        }

        int ClassifyTileType(TileData tile, LevelData levelData)
        {
            var p1 = levelData.GetPointData(tile.Row, tile.Col).Type;
            var p2 = levelData.GetPointData(tile.Row, tile.Col + 1).Type;
            var p3 = levelData.GetPointData(tile.Row + 1, tile.Col + 1).Type;
            var p4 = levelData.GetPointData(tile.Row + 1, tile.Col).Type;

            int type = 0;

            if (p1 == PointTypes.Grass) type += 1;
            if (p2 == PointTypes.Grass) type += 2;
            if (p3 == PointTypes.Grass) type += 4;
            if (p4 == PointTypes.Grass) type += 8;

            return type;
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
                _highlightedPropPosition = null;
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

            if (_highlightedPropPosition != null)
            {
                var radius = _currentBrush == BrushTypes.Trees1 || _currentBrush == BrushTypes.Trees2 ? _treeBrushSize : _nonTreeBrushSize;
                Handles.DrawWireArc(_highlightedPropPosition.Value, Vector3.up, Vector3.forward, 360, radius);
            }

            var totalSize = levelData.TileSize * levelData.TilesPerRow;
            Handles.DrawWireCube(Vector3.zero, new Vector3(totalSize, 0, totalSize));
        }

        PointTypes GetPointTypeForBrush(BrushTypes brush)
        {
            switch (brush)
            {
                case BrushTypes.Grass:
                    return PointTypes.Grass;

                case BrushTypes.Water:
                    return PointTypes.Water;

                default:
                    throw new ArgumentOutOfRangeException(nameof(brush), brush, null);
            }
        }

        void ApplyBrushToPoint(BrushTypes brush, PointData pointData)
        {
            var newPointType = GetPointTypeForBrush(brush);

            if (pointData.Type == newPointType)
            {
                return;
            }

            var levelData = GetLevelData();

            if (pointData.Row == 0 || pointData.Col == 0 || pointData.Row == levelData.PointsPerRow - 1 || pointData.Col == levelData.PointsPerRow - 1)
            {
                if (pointData.Type != _defaultPointType)
                {
                    Log.Warn("Found non default point type at edge.  Expected {0} but found {1}", _defaultPointType, pointData.Type);
                }
                return;
            }

            Log.Info("[LevelEditorWindowImpl] Applying brush {0} to point {1}", brush, pointData.Id);

            MarkSceneDirty();

            pointData.Type = newPointType;

            foreach (var tileData in levelData.GetNeighbourTiles(pointData))
            {
                var newTileType = ClassifyTileType(tileData, levelData);

                if (newTileType != tileData.TileType)
                {
                    tileData.TileType = newTileType;
                    UpdateTileModel(tileData, levelData);

                    Log.Debug("[LevelEditorWindowImpl] Updated tileData model for tileData {0}", tileData.Id);
                }
            }
        }

        void TryAddPropAt(TileData tile, GameObject prefab, Vector3 pos, PropInfo propInfo)
        {
            foreach (var existingProp in tile.Props)
            {
                if (existingProp.GameObject != null && existingProp.PropType == propInfo.PropType && (existingProp.GameObject.transform.position - pos).magnitude < propInfo.MinDistanceToOtherProps)
                {
                    return;
                }
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            instance.transform.position = pos;
            instance.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.up);

            var scale = UnityEngine.Random.Range(propInfo.ScaleMin, propInfo.ScaleMax);
            instance.transform.localScale = Vector3.one * scale;

            instance.transform.SetParent(GetPropsParent().transform, false);

            if (tile.Props == null)
            {
                tile.Props = new List<PropInstance>();
            }

            tile.Props.Add(new PropInstance()
            {
                GameObject = instance,
                PropType = propInfo.PropType
            });
        }

        void TryAddProp(Vector3 pos, BrushTypes brushType, LevelData levelData)
        {
            MarkSceneDirty();

            if (brushType == BrushTypes.PropEraser)
            {
                var radius = _nonTreeBrushSize;

                foreach (var tile in levelData.Tiles)
                {
                    var propsToDelete = new List<PropInstance>();

                    if (tile.Props == null)
                    {
                        tile.Props = new List<PropInstance>();
                    }

                    foreach (var existingProp in tile.Props)
                    {
                        if (existingProp.GameObject != null && (existingProp.GameObject.transform.position - pos).magnitude < radius)
                        {
                            propsToDelete.Add(existingProp);
                        }
                    }

                    foreach (var prop in propsToDelete)
                    {
                        tile.Props.Remove(prop);
                        GameObject.DestroyImmediate(prop.GameObject);
                    }
                }
            }
            else
            {
                var tile = TryGetClosestTile(pos);

                if (tile == null)
                {
                    return;
                }

                if (tile.Props == null)
                {
                    tile.Props = new List<PropInstance>();
                }

                var propType = BrushTypeToPropType(brushType);
                var propInfo = _settings.TileSettings.PropPrefabs.Where(x => x.PropType == propType).Single();

                var prefab = propInfo.Prefabs[UnityEngine.Random.Range(0, propInfo.Prefabs.Count)];
                var radius = _currentBrush == BrushTypes.Trees1 || _currentBrush == BrushTypes.Trees2 ? _treeBrushSize : _nonTreeBrushSize;
                var numAttempts = Mathf.Max(1, (int)Mathf.Floor(0.01f * (_treeBrushSize * _treeBrushSize)));

                for (int i = 0; i < numAttempts; i++)
                {
                    var relativePos = UnityEngine.Random.insideUnitCircle;

                    // Increase probability of being closer to center
                    relativePos = new Vector2(relativePos.x * relativePos.x, relativePos.y * relativePos.y);

                    relativePos = relativePos * radius;

                    var absPos = pos + new Vector3(relativePos.x, 0, relativePos.y);

                    var adjustedPos = ProjectOntoTerrain(absPos);

                    if (adjustedPos.HasValue && IsDirectlyOnFlatGrass(adjustedPos.Value, levelData))
                    {
                        TryAddPropAt(tile, prefab, adjustedPos.Value, propInfo);
                    }
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

            if (currentBrush == BrushTypes.Grass || currentBrush == BrushTypes.Water)
            {
                var pointUnderMouse = TryGetPointAtScreenPoint(screenPos, levelData);

                if (pointUnderMouse != null)
                {
                    ApplyBrushToPoint(currentBrush, pointUnderMouse);
                }
            }
            else
            {
                var gridPoint = TryGetTerrainIntersectionPoint(screenPos);

                if (gridPoint != null && IsDirectlyOnFlatGrass(gridPoint.Value, levelData))
                {
                    TryAddProp(gridPoint.Value, currentBrush, levelData);
                }
            }
        }

        PropType BrushTypeToPropType(BrushTypes brush)
        {
            switch (brush)
            {
                case BrushTypes.Trees1:
                    return PropType.Tree1;

                case BrushTypes.Trees2:
                    return PropType.Tree2;

                case BrushTypes.Flowers:
                    return PropType.Flower;

                case BrushTypes.Mushrooms:
                    return PropType.Mushroom;

                case BrushTypes.GrassTufts:
                    return PropType.GrassTuft;

                case BrushTypes.Rocks:
                    return PropType.Rock;

                default:
                    throw new ArgumentOutOfRangeException(nameof(brush), brush, null);
            }
        }

        bool IsDirectlyOnFlatGrass(Vector3 pos, LevelData levelData)
        {
            if (Mathf.Abs(pos.y - LOW_GRASS_HEIGHT * levelData.TileScale) < 0.01f)
            {
                return true;
            }

            return Mathf.Abs(pos.y - HIGH_GRASS_HEIGHT * levelData.TileScale) < 0.01f;
        }

        void HighlightControlsAt(Vector2 screenPos, LevelData levelData)
        {
            if (!_currentBrush.HasValue)
            {
                return;
            }

            var currentBrush = _currentBrush.Value;

            if (currentBrush == BrushTypes.Grass || currentBrush == BrushTypes.Water)
            {
                PointData pointUnderMouse = TryGetPointAtScreenPoint(screenPos, levelData);

                if (pointUnderMouse != _highlightedPoint)
                {
                    _highlightedPoint = pointUnderMouse;
                    SceneView.RepaintAll();
                }
            }
            else
            {
                var propPos = TryGetTerrainIntersectionPoint(screenPos);

                if (propPos.HasValue && !IsDirectlyOnFlatGrass(propPos.Value, levelData))
                {
                    propPos = null;
                }

                if (_highlightedPropPosition != propPos)
                {
                    _highlightedPropPosition = propPos;
                    SceneView.RepaintAll();
                }
            }

            if (_highlightedTile != null)
            {
                _highlightedTile = null;
                SceneView.RepaintAll();
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
            MarkSceneDirty();

            var tileSettings = _settings.TileSettings;

            List<GameObject> prefabVariations;

            Assert.That(type >= 0 && type <= 15);

            var linkInfo = tileSettings.GrassPrefabLinks[type];
            float rotation;

            if (type == 15)
            {
                rotation = UnityEngine.Random.Range(0, 4) * 90;
            }
            else
            {
                rotation = linkInfo.Rotation;
            }

            var prefabIndex = linkInfo.PrefabIndex;
            prefabVariations = tileSettings.GrassPrefabVariations.Where(x => x.PrefabIndex == prefabIndex).Single().Prefabs;

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
            MarkSceneDirty();

            if (tileData.Model != null)
            {
                GameObject.DestroyImmediate(tileData.Model);

                if (tileData.Props == null)
                {
                    tileData.Props = new List<PropInstance>();
                }

                foreach (var prop in tileData.Props)
                {
                    GameObject.DestroyImmediate(prop.GameObject);
                }

                tileData.Props.Clear();
            }

            tileData.Model = InstantiateTile(tileData.CenterPosition, tileData.TileType, levelData);
        }

        GameObject GetTilesParent()
        {
            var tilesParent = EditorSceneManager.GetActiveScene().GetRootGameObjects().Where(x => x.name == "Tiles").SingleOrDefault();

            if (tilesParent == null)
            {
                tilesParent = new GameObject("Tiles");
                MarkSceneDirty();
            }

            return tilesParent;
        }

        GameObject GetPropsParent()
        {
            var propsParent = EditorSceneManager.GetActiveScene().GetRootGameObjects().Where(x => x.name == "Props").SingleOrDefault();

            if (propsParent == null)
            {
                propsParent = new GameObject("Props");
                MarkSceneDirty();
            }

            return propsParent;
        }

        void RegenerateTiles()
        {
            Log.Info("[LevelEditorWindowImpl] Generating tiles...");

            MarkSceneDirty();
            Assert.That(_regeneratePointsPerRow >= 2);
            Assert.That(_regenerateTileScale >= 0);

            var levelData = GetLevelData();

            foreach (GameObject child in UnityUtil.GetDirectChildren(GetTilesParent()))
            {
                GameObject.DestroyImmediate(child);
            }

            foreach (GameObject child in UnityUtil.GetDirectChildren(GetPropsParent()))
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

            var defaultPointType = _defaultPointType;
            var nonDefaultPointType = defaultPointType == PointTypes.Water ? PointTypes.Grass : PointTypes.Water;

            for (int i = 0; i < pointsPerRow; i++)
            {
                for (int k = 0; k < pointsPerRow; k++)
                {
                    bool isAtEdge = i == 0 || i == pointsPerRow - 1 || k == 0 || k == pointsPerRow - 1;

                    var pointData = new PointData()
                    {
                        Type = isAtEdge ? defaultPointType : nonDefaultPointType,
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
                        CenterPosition = center + globalOffset,
                        Row = i,
                        Col = k,
                        Id = levelData.Tiles.Count,
                    };

                    tileData.TileType = ClassifyTileType(tileData, levelData);
                    UpdateTileModel(tileData, levelData);
                    levelData.Tiles.Add(tileData);
                }
            }

            Log.Info("[LevelEditorWindowImpl] Successfully generated {0} tiles", levelData.Tiles.Count);
        }

        void GenerateBurnEffectData()
        {
            MarkSceneDirty();
            var levelData = GetLevelData();
            foreach (TileData tile in levelData.Tiles)
            {
                if (tile.burntEffect == null)
                    tile.burntEffect = new BurntEffect();

                List<MeshRenderer> meshes = new List<MeshRenderer>();
                foreach (PropInstance prop in tile.Props)
                {
                    MeshRenderer[] propMeshes = prop.GameObject.GetComponentsInChildren<MeshRenderer>();
                    meshes.AddRange(propMeshes);
                }
                tile.burntEffect.SetUpBurnables(meshes.ToArray());
            }
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

            if (GUILayout.Button("Deselect Brush", _currentBrush == null ? GetSelectedButtonStyle() : regularButtonStyle))
            {
                _currentBrush = null;
                _highlightedPoint = null;
                _highlightedPropPosition = null;
                SceneView.RepaintAll();
            }

            using (GuiHelper.VerticalBox("Terrain"))
            {
                using (GuiHelper.HorizontalBlock())
                {
                    if (GUILayout.Button("Grass", _currentBrush == BrushTypes.Grass ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _currentBrush = BrushTypes.Grass;
                        _highlightedPoint = null;
                        _highlightedPropPosition = null;
                        SceneView.RepaintAll();
                    }

                    if (GUILayout.Button("Water", _currentBrush == BrushTypes.Water ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _currentBrush = BrushTypes.Water;
                        _highlightedPoint = null;
                        _highlightedPropPosition = null;
                        SceneView.RepaintAll();
                    }
                }
            }

            using (GuiHelper.VerticalBox("Props:"))
            {
                using (GuiHelper.HorizontalBlock())
                {
                    if (GUILayout.Button("Tree 1", _currentBrush == BrushTypes.Trees1 ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _currentBrush = BrushTypes.Trees1;
                        _highlightedPoint = null;
                        _highlightedPropPosition = null;
                        SceneView.RepaintAll();
                    }

                    if (GUILayout.Button("Tree 2", _currentBrush == BrushTypes.Trees2 ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _currentBrush = BrushTypes.Trees2;
                        _highlightedPoint = null;
                        _highlightedPropPosition = null;
                        SceneView.RepaintAll();
                    }

                    if (GUILayout.Button("Flower", _currentBrush == BrushTypes.Flowers ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _currentBrush = BrushTypes.Flowers;
                        _highlightedPoint = null;
                        _highlightedPropPosition = null;
                        SceneView.RepaintAll();
                    }

                    if (GUILayout.Button("Mushroom", _currentBrush == BrushTypes.Mushrooms ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _currentBrush = BrushTypes.Mushrooms;
                        _highlightedPoint = null;
                        _highlightedPropPosition = null;
                        SceneView.RepaintAll();
                    }
                }

                using (GuiHelper.HorizontalBlock())
                {
                    if (GUILayout.Button("GrassTuft", _currentBrush == BrushTypes.GrassTufts ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _currentBrush = BrushTypes.GrassTufts;
                        _highlightedPoint = null;
                        _highlightedPropPosition = null;
                        SceneView.RepaintAll();
                    }

                    if (GUILayout.Button("Rock", _currentBrush == BrushTypes.Rocks ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _currentBrush = BrushTypes.Rocks;
                        _highlightedPoint = null;
                        _highlightedPropPosition = null;
                        SceneView.RepaintAll();
                    }

                    if (GUILayout.Button("Eraser", _currentBrush == BrushTypes.PropEraser ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _currentBrush = BrushTypes.PropEraser;
                        _highlightedPoint = null;
                        _highlightedPropPosition = null;
                        SceneView.RepaintAll();
                    }
                }

                using (GuiHelper.HorizontalBlock())
                {
                    GUILayout.Label("Tree Brush Size", EditorStyles.boldLabel);

                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        _treeBrushSize -= 5.0f;
                    }

                    _treeBrushSize = EditorGUILayout.FloatField(_treeBrushSize, GUILayout.Width(40));

                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        _treeBrushSize += 5.0f;
                    }

                    if (_treeBrushSize <= 0)
                    {
                        _treeBrushSize = 1;
                    }
                }

                using (GuiHelper.HorizontalBlock())
                {
                    GUILayout.Label("Non Tree Brush Size", EditorStyles.boldLabel);

                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        _nonTreeBrushSize -= 5.0f;
                    }

                    _nonTreeBrushSize = EditorGUILayout.FloatField(_nonTreeBrushSize, GUILayout.Width(40));

                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        _nonTreeBrushSize += 5.0f;
                    }

                    if (_nonTreeBrushSize <= 0)
                    {
                        _nonTreeBrushSize = 1;
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

                var regularButtonStyle = GUI.skin.button;

                using (GuiHelper.HorizontalBlock())
                {
                    GUILayout.Label("Default Tile", EditorStyles.boldLabel);

                    if (GUILayout.Button("Grass", _defaultPointType == PointTypes.Grass ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _defaultPointType = PointTypes.Grass;
                    }

                    if (GUILayout.Button("Water", _defaultPointType == PointTypes.Water ? GetSelectedButtonStyle() : regularButtonStyle))
                    {
                        _defaultPointType = PointTypes.Water;
                    }
                }

                if (GUILayout.Button("Regenerate"))
                {
                    RegenerateTiles();
                }

                if (GUILayout.Button("Generate Burn Effect Data"))
                {
                    GenerateBurnEffectData();
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

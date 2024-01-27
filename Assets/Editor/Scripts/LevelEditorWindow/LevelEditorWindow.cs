#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace FirePatrol
{
    public class LevelEditorWindow : EditorWindow
    {
        [NonSerialized]
        private GUIStyle _errorTextStyle;

        [NonSerialized]
        private Exception _fatalError;

        [NonSerialized]
        private LevelEditorWindowImpl _impl;

        private GUIStyle ErrorTextStyle
        {
            get
            {
                if (_errorTextStyle == null)
                {
                    _errorTextStyle = new GUIStyle(GUI.skin.label);
                    _errorTextStyle.fontSize = 18;
                    _errorTextStyle.normal.textColor = Color.red;
                    _errorTextStyle.wordWrap = true;
                    _errorTextStyle.alignment = TextAnchor.MiddleCenter;
                }

                return _errorTextStyle;
            }
        }

        public void Update()
        {
            if (_fatalError != null)
            {
                return;
            }

            try
            {
                if (_impl != null)
                {
                    _impl.Update();
                }
            }
            catch (Exception e)
            {
                Log.Error("Failure during update: {0}", e);
                _fatalError = e;
            }

            Repaint();
        }

        public void OnEnable()
        {
            if (_fatalError != null)
            {
                return;
            }

            Initialize();
        }

        public void OnDisable()
        {
            if (_fatalError != null)
            {
                return;
            }

            CleanUp();
        }

        public void OnGUI()
        {
            var windowRect = new Rect(0, 0, position.width, position.height);

            if (_fatalError != null)
            {
                var labelWidth = 600;
                var labelHeight = 200;

                GUI.Label(
                    new Rect(
                        windowRect.width / 2 - labelWidth / 2,
                        windowRect.height / 3 - labelHeight / 2,
                        labelWidth,
                        labelHeight
                    ),
                    "Unrecoverable error occurred!  \nSee log for details.",
                    ErrorTextStyle
                );

                var buttonWidth = 100;
                var buttonHeight = 50;
                var offset = new Vector2(0, 100);

                if (
                    GUI.Button(
                        new Rect(
                            windowRect.width / 2 - buttonWidth / 2 + offset.x,
                            windowRect.height / 3 - buttonHeight / 2 + offset.y,
                            buttonWidth,
                            buttonHeight
                        ),
                        "Reload"
                    )
                )
                {
                    ExecuteFullReload();
                }
            }
            else
            {
                try
                {
                    if (_impl != null)
                    {
                        _impl.OnGUI(windowRect);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Error during OnGUI: {0}", e);
                    _fatalError = e;
                }
            }
        }

        private void Initialize()
        {
            _impl = new LevelEditorWindowImpl();
            _impl.Initialize();
        }

        private void CleanUp()
        {
            if (_impl != null)
            {
                _impl.Dispose();
                _impl = null;
            }
        }

        private void ExecuteFullReload()
        {
            CleanUp();

            _fatalError = null;
            _impl = null;

            Initialize();
        }

        [MenuItem("FirePatrol/Open Level Editor")]
        public static LevelEditorWindow GetOrCreateWindow()
        {
            var window = GetWindow<LevelEditorWindow>();
            window.titleContent = new GUIContent("Fire Patrol Level Editor");
            return window;
        }
    }
}
#endif
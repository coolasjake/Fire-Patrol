using System;
using UnityEngine;

namespace FirePatrol
{
    // [CreateAssetMenu(fileName = "GameSettings", menuName = "Game Settings")]
    public class GameSettings : ScriptableObject
    {
        public LogLevel MaxLogLevel = LogLevel.Info;
        public TileTesterSettings TileTester;

        static GameSettings _instance;
        static bool _hasLoaded;

        public static GameSettings Instance
        {
            get
            {
                if (!_hasLoaded)
                {
                    _hasLoaded = true;
                    _instance = Load();
                }

                return _instance;
            }
        }

        private static GameSettings Load()
        {
            var instance = (GameSettings)Resources.Load("FirePatrol/GameSettings");

            if (instance == null)
            {
                instance = ScriptableObject.CreateInstance<GameSettings>();
            }

            return instance;
        }
    }
}

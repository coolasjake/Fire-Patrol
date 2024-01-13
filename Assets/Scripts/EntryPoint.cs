using UnityEngine;

namespace FirePatrol
{
    public class EntryPoint : MonoBehaviour
    {
        public void Awake()
        {
            LogInitializer.LazyInitialize();
        }
    }
}

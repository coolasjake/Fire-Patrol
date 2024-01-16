using UnityEngine;
using System.Collections.Generic;

namespace FirePatrol
{
    public static class UnityUtil
    {
        public static List<GameObject> GetDirectChildren(GameObject obj)
        {
            var result = new List<GameObject>();
            foreach (Transform child in obj.transform)
            {
                result.Add(child.gameObject);
            }
            return result;
        }
    }
}

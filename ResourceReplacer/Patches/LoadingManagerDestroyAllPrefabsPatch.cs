using System;
using Harmony;
using UnityEngine;

namespace ResourceReplacer.Patches {
    [HarmonyPatch(typeof(LoadingManager), "DestroyAllPrefabs")]
    public static class LoadingManagerDestroyAllPrefabsPatch {
        public static void Postfix() {
            ResourceReplacer.instance.ClearCache();
        }
    }
}
using System;
using Harmony;
using UnityEngine;

namespace ResourceReplacer.Patches {
    [HarmonyPatch(typeof(BuildingInfo), nameof(BuildingInfo.InitializePrefab))]
    public static class BuildingInfoInitializePrefabPatch {
        public static void Prefix(BuildingInfo __instance) {
            try {
                ResourceReplacer.instance.ReplaceBuildingTextures(__instance);
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }
    }
}

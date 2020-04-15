using Harmony;

namespace ResourceReplacer.Patches {
    [HarmonyPatch(typeof(BuildingInfo), nameof(BuildingInfo.InitializePrefab))]
    public static class BuildingInfoInitializePrefabPatch {
        public static void Prefix(BuildingInfo __instance) {
            ResourceReplacer.instance.ReplaceTextures(__instance);
        }
    }
}

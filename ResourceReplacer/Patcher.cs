using Harmony;

namespace ResourceReplacer {
    public static class Patcher {
        private const string HarmonyId = "boformer.ResourceReplacer";

        public static void Apply() {
            HarmonyInstance.Create(HarmonyId).PatchAll(typeof(Patcher).Assembly);
        }

        public static void Revert() {
            HarmonyInstance.Create(HarmonyId).UnpatchAll(HarmonyId);
        }
    }
}

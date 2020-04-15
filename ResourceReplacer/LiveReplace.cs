using ICities;
using UnityEngine;

namespace ResourceReplacer {
    public class LiveReload : ThreadingExtensionBase {
        private bool _processed = false;

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.J)) {
                if (_processed) return;
                _processed = true;

                Debug.Log("Live Reload...");

                ResourceReplacer.instance.RestoreAllTextures();
                ResourceReplacer.instance.ClearCache();
                ResourceReplacer.instance.ReplaceAllTextures();
                RegenerateLodAtlases();
            } else {
                _processed = false;
            }
        }

        public static void RegenerateLodAtlases() {
            var prefabCount = PrefabCollection<BuildingInfo>.LoadedCount();
            for (var i = 0u; i < prefabCount; i++) {
                var prefab = PrefabCollection<BuildingInfo>.GetLoaded(i);
                if(prefab != null) prefab.m_hasLodData = false;
            }
            BuildingManager.instance.InitRenderData();
        }
    }

}

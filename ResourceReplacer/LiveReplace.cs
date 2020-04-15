using ICities;
using ResourceReplacer.Editor;
using UnityEngine;

namespace ResourceReplacer {
    public class LiveReload : ThreadingExtensionBase {
        private bool _processed = false;

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.J)) {
                if (_processed) return;
                _processed = true;

                Debug.Log("Live Reload...");

                Restore();
                Replace();
                RefreshRenderData();
            } else {
                _processed = false;
            }
        }

        public static void Restore() {
            ResourceReplacer.instance.RestoreAllBuildingTextures();
            ResourceReplacer.instance.RestoreAllBuildingColors();
            ResourceReplacer.instance.ClearCache();
        }

        public static void Replace() {
            ResourceReplacer.instance.ActivePacks.Clear();
            ResourceReplacer.instance.ActivePacks.Add(ResourcePackEditor.GetOrCreateDevResourcePack());

            ResourceReplacer.instance.ReplaceAllBuildingTextures();
            ResourceReplacer.instance.ReplaceAllBuildingColors();
        }

        public static void RefreshRenderData() {
            // LOD atlases
            var prefabCount = PrefabCollection<BuildingInfo>.LoadedCount();
            for (var i = 0u; i < prefabCount; i++) {
                var prefab = PrefabCollection<BuildingInfo>.GetLoaded(i);
                if (prefab != null) prefab.m_hasLodData = false;
            }
            BuildingManager.instance.InitRenderData();

            // Colors
            BuildingManager.instance.UpdateBuildingColors();
        }
    }

}

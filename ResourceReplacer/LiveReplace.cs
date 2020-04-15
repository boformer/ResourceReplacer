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
                BuildingManager.instance.InitRenderData();
            } else {
                _processed = false;
            }
        }
    }

}

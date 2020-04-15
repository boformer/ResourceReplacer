using ICities;
using ResourceReplacer.Editor;

namespace ResourceReplacer
{
    public class Mod : IUserMod, ILoadingExtension {
        public string Name => "Resource Replacer";

        public string Description => "Replaces the textures and colors of game assets";

        public void OnEnabled() {
            ResourceReplacer.Ensure();
            Patcher.Apply();

            // for hot reload
            if (LoadingComplete) {
                InstallUI();
            }
        }

        public void OnDisabled() {
            // for hot reload
            if (LoadingComplete) {
                UninstallUI();
            }

            Patcher.Revert();
            ResourceReplacer.Uninstall();
        }

        public void OnCreated(ILoading loading) { }

        public void OnReleased() { }

        public void OnLevelLoaded(LoadMode mode) {
            InstallUI();
        }

        public void OnLevelUnloading() {
            UninstallUI();
        }

        public bool LoadingComplete => LoadingManager.exists && LoadingManager.instance.m_loadingComplete;

        public void InstallUI() {
            BuildingConfigPanel.Install();
        }

        public void UninstallUI() {
            BuildingConfigPanel.Uninstall();
        }
    }
}

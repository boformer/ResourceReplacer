using ICities;
using ResourceReplacer.Editor;

namespace ResourceReplacer
{
    public class Mod : IUserMod, ILoadingExtension {
        public string Name => "Resource Replacer";

        public string Description => "Replaces the textures and colors of game assets";

        public void OnEnabled() {
            ResourceReplacer.Ensure();
            ResourcePackEditor.Ensure();
            var devPack = ResourcePackEditor.GetOrCreateDevResourcePack();
            if (devPack != null) {
                ResourcePackEditor.instance.ActivePack = devPack;
                ResourceReplacer.instance.ActivePacks.Add(devPack);
            }

            Patcher.Apply();

            // for hot reload
            if (LoadingComplete) {
                InstallUI();

                ResourceReplacer.instance.ReplaceAllTextures();
                LiveReload.RegenerateLodAtlases();
            }
        }

        public void OnDisabled() {
            // for hot reload
            if (LoadingComplete) {
                UninstallUI();

                ResourceReplacer.instance.RestoreAllTextures();
                LiveReload.RegenerateLodAtlases();
            }

            Patcher.Revert();
            ResourcePackEditor.Uninstall();
            ResourceReplacer.Uninstall();
        }

        public void OnCreated(ILoading loading) { }

        public void OnReleased() {
            ResourceReplacer.instance.RestoreAllTextures();
        }

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

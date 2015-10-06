using ICities;

namespace TextureReplacer
{
    public class TextureReplacerMod : LoadingExtensionBase, IUserMod
    {
        public string Name
        {
            get { return "Texture Replacer"; }
        }
        public string Description
        {
            get { return "Replaces the textures of the game assets with remastered versions"; }
        }

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            TextureReplacer.instance.SearchTexturePacks();

            Detour.BuildingInfoDetour.Deploy();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            TextureReplacer.instance.UnloadUnusedTextures();
        }

        public override void OnReleased()
        {
            base.OnReleased();

            TextureReplacer.instance.Reset();

            Detour.BuildingInfoDetour.Revert();
        }
    }
}

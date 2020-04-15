using UnityEngine;

namespace ResourceReplacer.Pack {
    public abstract class ResourcePack {
        public abstract string Path { get; }
        public abstract Texture2D GetBuildingTexture(string textureName);

        public abstract bool TryGetBuildingColors(string prefabName, out PrefabColors colors);

        public class PrefabColors {
            public bool UseColorVariation;
            public Color Color0;
            public Color Color1;
            public Color Color2;
            public Color Color3;
        }
    }
}

using System;
using System.IO;
using ColossalFramework.IO;
using ResourceReplacer.Pack;
using UnityEngine;

namespace ResourceReplacer.Packs {
    public class FileResourcePack : ResourcePack {
        private static readonly string BuildingsTextureDir = System.IO.Path.Combine("textures", "buildings");
        public static string DumpDir => System.IO.Path.Combine(DataLocation.localApplicationData, System.IO.Path.Combine(BuildingsTextureDir, "dump"));

        public override string Path { get; }

        public FileResourcePack(string path) {
            Path = path;
        }

        public override Texture2D GetBuildingTexture(string textureName) {
            var texturePath = System.IO.Path.Combine(System.IO.Path.Combine(Path, BuildingsTextureDir), textureName + ".png");
            if (!File.Exists(texturePath)) {
                return null;
            }

            #if DEBUG
            Debug.Log("Loading texture " + texturePath + " from disk");
            #endif

            var texture = new Texture2D(1, 1);
            texture.LoadImage(File.ReadAllBytes(texturePath));
            texture.Compress(highQuality: true);
            texture.anisoLevel = 8;

            return texture;
        }

        public override bool TryGetBuildingColors(string prefabName, out PrefabColors colors) {
            colors = new PrefabColors();
            return false;
        }
    }
}

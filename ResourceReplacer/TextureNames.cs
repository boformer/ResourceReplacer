using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace ResourceReplacer {
    public class TextureNames {
        private const string LodModifier = "_LOD";

        public const string ReplacedTexturePrefix = "ResourceReplacer_";

        public static readonly Dictionary<string, string> Properties = new Dictionary<string, string> {
            { "_MainTex", "-rgb" }, { "_XYSMap", "-xys" }, { "_ACIMap", "-aci" }
        };

        public static string GetReplacementTextureName(string prefabName, Texture texture, string propertyName, bool lod) {
            var steamId = GetSteamId(prefabName);


            if (texture != null && steamId == null) {
                return GetOriginalTextureName(texture.name); // use original texture name for vanilla buildings
            }

            var texturePrefix = steamId ?? prefabName;
            if (!lod) {
                return texturePrefix + Properties[propertyName];
            } else {
                return texturePrefix + LodModifier + Properties[propertyName];
            }
        }


        public static bool IsOriginalTexture(Texture texture) {
            return texture == null || !texture.name.StartsWith(ReplacedTexturePrefix);
        }

        private static string GetSteamId(string prefabName) {
            if (!prefabName.Contains(".")) return null;

            var prefix = prefabName.Substring(0, prefabName.IndexOf(".", StringComparison.InvariantCulture));
            if (!ulong.TryParse(prefix, out _)) return null;

            return prefix;
        }
        private static string GetOriginalTextureName(string textureName) {
            if (textureName.StartsWith(ReplacedTexturePrefix)) {
                return textureName.Substring(ReplacedTexturePrefix.Length);
            }
            return textureName;
        }
    }
}

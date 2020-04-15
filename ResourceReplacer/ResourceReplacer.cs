using System;
using System.Collections.Generic;
using ColossalFramework.IO;
using ResourceReplacer.Pack;
using ResourceReplacer.Packs;
using UnityEngine;

namespace ResourceReplacer {
    // TODO restore original textures on reload
    // TODO live reload
    public class ResourceReplacer : ModSingleton<ResourceReplacer> {
        public readonly List<ResourcePack> ActivePacks = new List<ResourcePack>();

        private readonly Dictionary<string, Texture> _originalTextures = new Dictionary<string, Texture>();
        private readonly Dictionary<TextureKey, Texture2D> _replacementTextures = new Dictionary<TextureKey, Texture2D>();

        public void ReplaceAllTextures() {
            var prefabCount = PrefabCollection<BuildingInfo>.LoadedCount();
            for (var i = 0u; i < prefabCount; i++) ReplaceTextures(PrefabCollection<BuildingInfo>.GetLoaded(i));
        }

        public void ReplaceTextures(BuildingInfo prefab) {
            if (prefab == null) return;

            ReplaceTextures(prefab.GetComponent<Renderer>(), prefab.name, false);

            if (prefab.m_lodObject != null) {
                ReplaceTextures(prefab.m_lodObject.GetComponent<Renderer>(), prefab.name, true);
            }
        }

        private void ReplaceTextures(Renderer renderer, string prefabName, bool lod) {
            if (renderer == null) return;

            var material = renderer.sharedMaterial; 
            if (material == null) return;

            foreach (var propertyName in TextureNames.Properties.Keys) {
                var texture = material.GetTexture(propertyName);

                var textureName = TextureNames.GetReplacementTextureName(prefabName, texture, propertyName, lod);

                var replacementTexture = GetReplacementTexture(textureName);
                if (replacementTexture != null) {
                    // Save original texture for restoration purposes
                    if (TextureNames.IsOriginalTexture(texture)) _originalTextures[textureName] = texture;

                    // Apply replacement texture
                    material.SetTexture(propertyName, replacementTexture);
                }
            }
        }

        private Texture2D GetReplacementTexture(string textureName) {
            foreach (var pack in ActivePacks) {
                var key = new TextureKey(pack.Path, textureName);

                if (_replacementTextures.TryGetValue(key, out var replacementTexture)) {
                    return replacementTexture;
                }

                replacementTexture = pack.GetBuildingTexture(textureName);
                if (replacementTexture != null) {
                    // Set correct name and add to cache
                    replacementTexture.name = TextureNames.ReplacedTexturePrefix + textureName;
                    _replacementTextures[key] = replacementTexture;

                    return replacementTexture;
                }
            }

            return null;
        }

        public void RestoreAllTextures() {
            var prefabCount = PrefabCollection<BuildingInfo>.LoadedCount();
            for (var i = 0u; i < prefabCount; i++) RestoreTextures(PrefabCollection<BuildingInfo>.GetLoaded(i));
        }

        public void RestoreTextures(BuildingInfo prefab) {
            if (prefab == null) return;
            
            RestoreTextures(prefab.GetComponent<Renderer>(), prefab.name, false);

            if (prefab.m_lodObject != null) {
                RestoreTextures(prefab.m_lodObject.GetComponent<Renderer>(), prefab.name, false);
            }
        }

        private void RestoreTextures(Renderer renderer, string prefabName, bool lod) {
            if (renderer == null) return;

            var material = renderer.sharedMaterial;
            if (material == null) return;

            foreach (var propertyName in TextureNames.Properties.Keys) {
                var texture = material.GetTexture(propertyName);
                if (TextureNames.IsOriginalTexture(texture)) continue;

                var textureName = TextureNames.GetReplacementTextureName(prefabName, texture, propertyName, lod);

                if (_originalTextures.TryGetValue(textureName, out var originalTexture)) {
                    material.SetTexture(propertyName, originalTexture);
                }
            }
        }

        public void ClearCache() {
            _originalTextures.Clear();

            foreach (var texture in _replacementTextures.Values) {
                UnityEngine.Object.DestroyImmediate(texture, true);
            }

            _replacementTextures.Clear();
        }

        private readonly struct TextureKey : IEquatable<TextureKey>
        {
            private readonly string Path;
            private readonly string Name;

            public TextureKey(string path, string name) {
                Path = path;
                Name = name;
            }

            public bool Equals(TextureKey other)
            {
                return Path == other.Path && Name == other.Name;
            }

            public override bool Equals(object obj)
            {
                return obj is TextureKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (Path.GetHashCode() * 397) ^ Name.GetHashCode();
            }
        }
    }
}

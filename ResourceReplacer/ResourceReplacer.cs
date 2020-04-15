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
        private readonly List<ResourcePack> _activePacks = new List<ResourcePack> {
            new FileResourcePack(DataLocation.localApplicationData) // TODO find resource packs from mod folder
        };
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
                if (ReplaceTextures(prefab.m_lodObject.GetComponent<Renderer>(), prefab.name, true)) {
                    prefab.m_hasLodData = false;
                }
            }
        }

        private bool ReplaceTextures(Renderer renderer, string prefabName, bool lod) {
            if (renderer == null) return false;

            var material = renderer.sharedMaterial;
            if (material == null) return false;

            var replaced = false;
            foreach (var propertyName in TextureNames.Properties.Keys) {
                var texture = material.GetTexture(propertyName);

                var textureName = TextureNames.GetReplacementTextureName(prefabName, texture, propertyName, lod);

                var replacementTexture = GetReplacementTexture(textureName);
                if (replacementTexture != null) {
                    // Save original texture for restoration purposes
                    if (TextureNames.IsOriginalTexture(texture)) _originalTextures[textureName] = texture;

                    // Apply replacement texture
                    material.SetTexture(propertyName, replacementTexture);

                    replaced = true;
                }
            }
            return replaced;
        }

        private Texture2D GetReplacementTexture(string textureName) {
            foreach (var pack in _activePacks) {
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
                if (RestoreTextures(prefab.m_lodObject.GetComponent<Renderer>(), prefab.name, false)) {
                    prefab.m_hasLodData = false;
                }
            }
        }

        private bool RestoreTextures(Renderer renderer, string prefabName, bool lod) {
            if (renderer == null) return false;

            var material = renderer.sharedMaterial;
            if (material == null) return false;

            var restored = false;
            foreach (var propertyName in TextureNames.Properties.Keys) {
                var texture = material.GetTexture(propertyName);
                if (TextureNames.IsOriginalTexture(texture)) continue;

                var textureName = TextureNames.GetReplacementTextureName(prefabName, texture, propertyName, lod);

                if (_originalTextures.TryGetValue(textureName, out var originalTexture)) {
                    material.SetTexture(propertyName, originalTexture); 
                    restored = true;
                }
            }

            return restored;
        }

        public void ClearCache() {
            _originalTextures.Clear();
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

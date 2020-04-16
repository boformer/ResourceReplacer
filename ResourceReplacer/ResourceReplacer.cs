using System;
using System.Collections.Generic;
using ColossalFramework.IO;
using ResourceReplacer.Packs;
using UnityEngine;

namespace ResourceReplacer {
    // TODO restore original textures on reload
    // TODO live reload
    public class ResourceReplacer : ModSingleton<ResourceReplacer> {
        public readonly List<ResourcePack> ActivePacks = new List<ResourcePack>();

        private readonly Dictionary<string, Texture> _originalBuildingTextures = new Dictionary<string, Texture>();
        private readonly Dictionary<TextureKey, Texture2D> _replacementBuildingTextures = new Dictionary<TextureKey, Texture2D>();

        private readonly Dictionary<string, ResourcePack.PrefabColors> _originalBuildingColors = new Dictionary<string, ResourcePack.PrefabColors>();

        private readonly Dictionary<string, TerrainModify.Surface[]> _originalBuildingSurfaces = new Dictionary<string, TerrainModify.Surface[]>();

        #region Textures
        public void ReplaceAllBuildingTextures() {
            var prefabCount = PrefabCollection<BuildingInfo>.LoadedCount();
            for (var i = 0u; i < prefabCount; i++) ReplaceBuildingTextures(PrefabCollection<BuildingInfo>.GetLoaded(i));
        }

        public void ReplaceBuildingTextures(BuildingInfo prefab) {
            if (prefab == null) return;

            ReplaceBuildingTextures(prefab.GetComponent<Renderer>(), prefab.name, false);

            if (prefab.m_lodObject != null) {
                ReplaceBuildingTextures(prefab.m_lodObject.GetComponent<Renderer>(), prefab.name, true);
            }
        }

        private void ReplaceBuildingTextures(Renderer renderer, string prefabName, bool lod) {
            if (renderer == null) return;

            var material = renderer.sharedMaterial; 
            if (material == null) return;

            foreach (var propertyName in TextureNames.Properties.Keys) {
                var texture = material.GetTexture(propertyName);

                var textureName = TextureNames.GetReplacementTextureName(prefabName, texture, propertyName, lod);

                var replacementTexture = GetReplacementBuildingTexture(textureName);
                if (replacementTexture != null) {
                    // Save original texture for restoration purposes
                    if (TextureNames.IsOriginalTexture(texture)) _originalBuildingTextures[textureName] = texture;

                    #if DEBUG
                    UnityEngine.Debug.Log($"Replacing texture {textureName}");
                    #endif

                    // Apply replacement texture
                    material.SetTexture(propertyName, replacementTexture);
                }
            }
        }

        private Texture2D GetReplacementBuildingTexture(string textureName) {
            foreach (var pack in ActivePacks) {
                var key = new TextureKey(pack.Path, textureName);

                if (_replacementBuildingTextures.TryGetValue(key, out var replacementTexture)) {
                    return replacementTexture;
                }

                replacementTexture = pack.GetBuildingTexture(textureName);
                if (replacementTexture != null) {
                    // Set correct name and add to cache
                    replacementTexture.name = TextureNames.ReplacedTexturePrefix + textureName;
                    _replacementBuildingTextures[key] = replacementTexture;

                    return replacementTexture;
                }
            }

            return null;
        }

        public void RestoreAllBuildingTextures() {
            var prefabCount = PrefabCollection<BuildingInfo>.LoadedCount();
            for (var i = 0u; i < prefabCount; i++) RestoreBuildingTextures(PrefabCollection<BuildingInfo>.GetLoaded(i));
        }

        public void RestoreBuildingTextures(BuildingInfo prefab) {
            if (prefab == null) return;
            
            RestoreBuildingTextures(prefab.GetComponent<Renderer>(), prefab.name, false);

            if (prefab.m_lodObject != null) {
                RestoreBuildingTextures(prefab.m_lodObject.GetComponent<Renderer>(), prefab.name, false);
            }
        }

        private void RestoreBuildingTextures(Renderer renderer, string prefabName, bool lod) {
            if (renderer == null) return;

            var material = renderer.sharedMaterial;
            if (material == null) return;

            foreach (var propertyName in TextureNames.Properties.Keys) {
                var texture = material.GetTexture(propertyName);
                if (TextureNames.IsOriginalTexture(texture)) continue;

                var textureName = TextureNames.GetReplacementTextureName(prefabName, texture, propertyName, lod);

                if (_originalBuildingTextures.TryGetValue(textureName, out var originalTexture)) {
                    #if DEBUG
                    UnityEngine.Debug.Log($"Restoring original texture for {textureName}");
                    #endif
                    material.SetTexture(propertyName, originalTexture);
                }
            }
        }
        #endregion

        #region Color Variations
        public void ReplaceAllBuildingColors() {
            var prefabCount = PrefabCollection<BuildingInfo>.LoadedCount();
            for (var i = 0u; i < prefabCount; i++) ReplaceBuildingColors(PrefabCollection<BuildingInfo>.GetLoaded(i));
        }

        public void ReplaceBuildingColors(BuildingInfo prefab) {
            if (prefab == null) return;

            foreach (var pack in ActivePacks) {
                if(pack.TryGetBuildingColors(prefab.name, out var colors)) {
                    SetBuildingColors(prefab, colors);
                    break;
                }
            }
        }

        public void SetBuildingColors(BuildingInfo prefab, ResourcePack.PrefabColors colors) {
            if (prefab == null) return;

            if (!_originalBuildingColors.ContainsKey(prefab.name)) {
                _originalBuildingColors[prefab.name] = ResourcePack.PrefabColors.From(prefab);
            }
            colors.Apply(prefab);
        }

        public ResourcePack.PrefabColors GetOriginalBuildingColors(BuildingInfo prefab) {
            if (prefab == null) return default;

            if (_originalBuildingColors.TryGetValue(prefab.name, out var colors)) {
                return colors;
            } else {
                return ResourcePack.PrefabColors.From(prefab);
            }
        }

        public void RestoreAllBuildingColors() {
            foreach (var prefabName in _originalBuildingColors.Keys) {
                RestoreBuildingColors(PrefabCollection<BuildingInfo>.FindLoaded(prefabName));
            }
        }

        public void RestoreBuildingColors(BuildingInfo prefab) {
            if (prefab == null) return;
            if (_originalBuildingColors.TryGetValue(prefab.name, out var colors)) {
                colors.Apply(prefab);
            }
        }
        #endregion

        #region Surfaces
        public void RemoveBuildingSurfaces(BuildingInfo prefab) {
            if (prefab == null) return;

            if (!_originalBuildingSurfaces.ContainsKey(prefab.name)) {
                _originalBuildingSurfaces[prefab.name] = CopySurfaces(prefab.m_cellSurfaces);
            }

            for (int i = 0; i < prefab.m_cellSurfaces.Length; i++) {
                prefab.m_cellSurfaces[i] = TerrainModify.Surface.None;
            }

            UpdateTerrainOfAllInstances(prefab);
        }

        public void RestoreBuildingSurfaces(BuildingInfo prefab) {
            if (prefab == null) return;
            if (_originalBuildingSurfaces.TryGetValue(prefab.name, out var surfaces)) {
                for (int i = 0; i < surfaces.Length; i++) {
                    prefab.m_cellSurfaces[i] = surfaces[i];
                }

                _originalBuildingSurfaces.Remove(prefab.name);
                //removal is necessary since the dictionary entries are abused to save the state 
                //if the surfaces for a building were already removed or not
            }

            UpdateTerrainOfAllInstances(prefab);
        }

        public bool AreSurfacesEnabled(BuildingInfo prefab) {
            return !_originalBuildingSurfaces.ContainsKey(prefab.name);
        }

        private TerrainModify.Surface[] CopySurfaces(TerrainModify.Surface[] surfaces) {
            var result = new TerrainModify.Surface[surfaces.Length];
            for (int i = 0; i < surfaces.Length; i++) {
                result[i] = surfaces[i];
            }
            return result;
        }

        private void UpdateTerrainOfAllInstances(BuildingInfo prefab)
        {
            for (int i = 0; i < BuildingManager.instance.m_buildings.m_size; i++)
            {
                if (BuildingManager.instance.m_buildings.m_buffer[i].Info == prefab)
                {
                    BuildingManager.instance.m_buildings.m_buffer[i].UpdateTerrain(false, true);
                }
            }
        }
        #endregion

        public void ClearCache() {                    
            #if DEBUG
            UnityEngine.Debug.Log($"Clearing texture cache...");
            #endif

            _originalBuildingTextures.Clear();

            foreach (var texture in _replacementBuildingTextures.Values) {
                UnityEngine.Object.DestroyImmediate(texture, true);
            }

            _replacementBuildingTextures.Clear();
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

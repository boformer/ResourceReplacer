using ColossalFramework;
using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TextureReplacer
{
    public class TextureReplacer : Singleton<TextureReplacer>
    {
        private const string TEXTURE_PACK_PATH = "TexturePack.xml";
        private const string TEXTURE_EXTENSION = ".png";
        private static readonly string BUILDINGS_TEXTURE_DIR = Path.Combine("textures", "buildings");
        private static readonly string[] PROPERTY_NAMES = { "_MainTex", "_XYSMap", "_ACIMap" };

        private TextureManager textureManager;

        // Texture packs
        private readonly List<String> textureDirectories = new List<string>();
        private TexturePack mergedTexturePack;

        public void OnCreated()
        {
            SearchTexturePacks();
            textureManager = GetComponent<TextureManager>();
        }

        public void OnLevelLoaded()
        {
            textureManager.UnloadUnusedTextures();
        }

        public void ProcessBuilding(BuildingInfo prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("Prefab is null!");
                return;
            }

            //Debug.LogFormat("Processing texture of {0}", prefab.name);

            // detailed model
            ReplaceTextures(prefab.GetComponent<Renderer>());

            // lod model
            if (prefab.m_lodObject != null) ReplaceTextures(prefab.m_lodObject.GetComponent<Renderer>());

            // color variations
            var colorConfig = mergedTexturePack.GetBuilding(prefab.name);
            if (colorConfig != null) ReplaceColorVariations(prefab, colorConfig);
        }

        private void ReplaceColorVariations(BuildingInfo prefab, TexturePack.Prefab colorConfig)
        {
            var renderer = prefab.GetComponent<Renderer>();
            if (renderer == null) return;

            var material = renderer.sharedMaterial;
            if (material == null) return;

            if (colorConfig.UseColorVariations != null) prefab.m_useColorVariations = colorConfig.UseColorVariations.Value;

            if (prefab.m_useColorVariations)
            {
                if (colorConfig.Color0 != null) material.SetColor("_ColorV0", colorConfig.Color0.toUnityColor());
                if (colorConfig.Color1 != null) material.SetColor("_ColorV1", colorConfig.Color1.toUnityColor());
                if (colorConfig.Color2 != null) material.SetColor("_ColorV2", colorConfig.Color2.toUnityColor());
                if (colorConfig.Color3 != null) material.SetColor("_ColorV3", colorConfig.Color3.toUnityColor());
            }
            else if (colorConfig.Color0 != null)
            {
                material.color = colorConfig.Color0.toUnityColor();
            }
        }

        private void ReplaceTextures(Renderer renderer)
        {
            if (renderer == null) return;

            var material = renderer.sharedMaterial;
            if (material != null)
            {
                foreach (var propertyName in PROPERTY_NAMES)
                {
                    Texture existingTexture = material.GetTexture(propertyName);

                    // skip already replaced textures
                    if (existingTexture.name.Contains(TextureManager.MOD_TEXTURE_PREFIX)) continue;

                    foreach (string source in textureDirectories)
                    {
                        var texturePath = Path.Combine(Path.Combine(source, BUILDINGS_TEXTURE_DIR), existingTexture.name + TEXTURE_EXTENSION);

                        if (ReplaceTexture(material, propertyName, texturePath)) break;
                    }
                }
            }
        }

        // Returns true if texture was replaced
        private bool ReplaceTexture(Material material, string propertyName, string texturePath)
        {
            var originalTexture = material.GetTexture(propertyName);
            var newTexture = textureManager.GetTexture(texturePath);

            if (newTexture != null)
            {
                // apply new texture
                material.SetTexture(propertyName, newTexture);

                // mark old texture for unload
                textureManager.MarkTextureUnused(originalTexture);

                return true;
            }
            else
            {
                return false;
            }
        }

        private void SearchTexturePacks()
        {
            mergedTexturePack = new TexturePack("<merged>");
            textureDirectories.Clear();

            // user texture directory (SteamApps\common\Cities_Skylines\textures\)
            textureDirectories.Add("");

            foreach (var pluginInfo in PluginManager.instance.GetPluginsInfo().Where(pluginInfo => pluginInfo.isEnabled))
            {
                try
                {
                    var texturePack = TexturePack.Deserialize(Path.Combine(pluginInfo.modPath, TEXTURE_PACK_PATH));
                    if (texturePack != null)
                    {
                        mergedTexturePack.Merge(texturePack);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error while parsing TexturePack.xml of mod " + pluginInfo.name);
                    Debug.LogException(e);
                }

                if (Directory.Exists(Path.Combine(pluginInfo.modPath, BUILDINGS_TEXTURE_DIR)))
                {
                    textureDirectories.Add(pluginInfo.modPath);
                }
            }
        }
    }
}

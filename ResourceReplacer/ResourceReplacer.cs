using ColossalFramework;
using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ResourceReplacer
{
    public class ResourceReplacer : Singleton<ResourceReplacer>
    {
        private const string RESOURCE_PACK_PATH = "ResourcePack.xml";
        private const string TEXTURE_EXTENSION = ".png";
        private static readonly string BUILDINGS_TEXTURE_DIR = Path.Combine("textures", "buildings");
        private static readonly Dictionary<string, string> PROPERTY_NAMES = new Dictionary<string, string>()
        { 
            {"_MainTex", "-rgb"},
            {"_XYSMap", "-xys"}, 
            {"_ACIMap", "-aci"}, 
        };

        private TextureManager textureManager;

        // Resource packs
        private readonly List<String> textureDirectories = new List<string>();
        private ResourcePack mergedResourcePack;

        public void OnCreated()
        {
            SearchResourcePacks();
            if(textureManager == null) textureManager = gameObject.AddComponent<TextureManager>();
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

            // check if the building is a workshop item
            // in that case, use steam id (+ _lod) as the texture name
            var steamID = GetSteamID(prefab.name);
            var texturePrefix = steamID == null ? prefab.name : steamID;

            // detailed model
            ReplaceTextures(prefab.GetComponent<Renderer>(), texturePrefix, steamID == null);

            // lod model
            if (prefab.m_lodObject != null) ReplaceTextures(prefab.m_lodObject.GetComponent<Renderer>(), texturePrefix + "_LOD", steamID == null);

            // color variations
            var colorConfig = mergedResourcePack.GetBuilding(prefab.name);
            if (colorConfig != null) ReplaceColorVariations(prefab, colorConfig);
        }

        private void ReplaceColorVariations(BuildingInfo prefab, ResourcePack.Prefab colorConfig)
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

        private void ReplaceTextures(Renderer renderer, string texturePrefix, bool useExistingTextureName)
        {
            if (renderer == null) return;

            var material = renderer.sharedMaterial;
            if (material != null)
            {
                foreach (var propertyName in PROPERTY_NAMES.Keys)
                {
                    Texture existingTexture = material.GetTexture(propertyName);

                    // skip already replaced textures
                    if (existingTexture != null && existingTexture.name.Contains(TextureManager.MOD_TEXTURE_PREFIX)) continue;

                    string textureName = null;
                    if(useExistingTextureName && existingTexture != null) textureName = existingTexture.name;
                    else textureName = texturePrefix + PROPERTY_NAMES[propertyName];

                    if (!useExistingTextureName) Debug.Log("%%% Searching tex " + textureName);

                    foreach (string source in textureDirectories)
                    {
                        var texturePath = Path.Combine(Path.Combine(source, BUILDINGS_TEXTURE_DIR), textureName + TEXTURE_EXTENSION);

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
                if(originalTexture != null) textureManager.MarkTextureUnused(originalTexture);

                return true;
            }
            else
            {
                return false;
            }
        }

        private void SearchResourcePacks()
        {
            mergedResourcePack = new ResourcePack("<merged>");
            textureDirectories.Clear();

            // user texture directory (SteamApps\common\Cities_Skylines\textures\)
            textureDirectories.Add("");

            foreach (var pluginInfo in PluginManager.instance.GetPluginsInfo().Where(pluginInfo => pluginInfo.isEnabled))
            {
                try
                {
                    var resourcePack = ResourcePack.Deserialize(Path.Combine(pluginInfo.modPath, RESOURCE_PACK_PATH));
                    if (resourcePack != null)
                    {
                        mergedResourcePack.Merge(resourcePack);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error while parsing " + RESOURCE_PACK_PATH + " of mod " + pluginInfo.name);
                    Debug.LogException(e);
                }

                if (Directory.Exists(Path.Combine(pluginInfo.modPath, BUILDINGS_TEXTURE_DIR)))
                {
                    textureDirectories.Add(pluginInfo.modPath);
                }
            }
        }

        private string GetSteamID(string prefabName) 
        {
            string steamID = null;
            
            if (prefabName.Contains("."))
            {
                steamID = prefabName.Substring(0, prefabName.IndexOf("."));

                ulong result;
                if (!ulong.TryParse(steamID, out result) || result == 0)
                    steamID = null;
            }

            return steamID;
        }
    }
}

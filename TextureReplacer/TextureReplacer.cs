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

        private readonly string BUILDINGS_TEXTURE_DIR = Path.Combine("textures", "buildings");
        private const string TEXTURE_LOD_SUFFIX = "_LOD";
        private const string TEXTURE_EXTENSION = ".png";

        private static readonly string[] propertyNames = { "_MainTex", "_XYSMap", "_ACIMap" };

        private static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<Texture, bool> textureUsedMap = new Dictionary<Texture, bool>();

        private static readonly List<String> textureSources = new List<string>();
        private static TexturePack mergedPack;

        private uint counter = 0;

        public void UnloadUnusedTextures()
        {
            // unload unused textures
            foreach (var pair in textureUsedMap)
            {
                if (!pair.Value) UnityEngine.Object.Destroy(pair.Key);
            }
            //textureUsedMap.Clear();
        }

        public void Reset()
        {
            /*
            foreach (var texture in textures.Values)
            {
                UnityEngine.Object.Destroy(texture);
            }
            textures.Clear();
            textureUsedMap.Clear();*/
        }

        public void ProcessBuilding(BuildingInfo prefab)
        {
            if (prefab == null)
            {
                Debug.Log("Prefab is null!");
                return;
            }

            Debug.LogFormat("Processing texture of {0}", prefab.name);

            // detailed model
            ReplaceTextures(prefab.GetComponent<Renderer>());

            // lod model
            if (prefab.m_lodObject != null) ReplaceTextures(prefab.m_lodObject.GetComponent<Renderer>());

            var config = mergedPack.GetBuilding(prefab.name);
            if (config != null) ReplaceColorVariations(prefab, config);
        }

        public void ReplaceColorVariations(BuildingInfo prefab, TexturePack.Prefab config)
        {
            var renderer = prefab.GetComponent<Renderer>();
            if (renderer == null) return;

            var material = renderer.sharedMaterial;
            if (material == null) return;

            if (config.UseColorVariations != null) prefab.m_useColorVariations = config.UseColorVariations.Value;

            if (prefab.m_useColorVariations)
            {
                if (config.Color0 != null) material.SetColor("_ColorV0", config.Color0.toUnityColor());
                if (config.Color1 != null) material.SetColor("_ColorV1", config.Color1.toUnityColor());
                if (config.Color2 != null) material.SetColor("_ColorV2", config.Color2.toUnityColor());
                if (config.Color3 != null) material.SetColor("_ColorV3", config.Color3.toUnityColor());
            }
            else if (config.Color0 != null)
            {
                material.color = config.Color0.toUnityColor();
            }
        }

        private void ReplaceTextures(Renderer renderer)
        {
            if (renderer == null) return;

            var material = renderer.sharedMaterial;
            if (material != null)
            {
                foreach (var propertyName in propertyNames)
                {
                    Texture existingTexture = material.GetTexture(propertyName);

                    if (existingTexture.name.Contains("TextureReplacer")) continue;

                    foreach (string source in textureSources)
                    {
                        var texturePath = Path.Combine(Path.Combine(source, BUILDINGS_TEXTURE_DIR), existingTexture.name + TEXTURE_EXTENSION);

                        if (ReplaceTexture(material, propertyName, texturePath)) break;
                    }
                }
            }
        }

        private bool ReplaceTexture(Material material, string propertyName, string texturePath)
        {
            var originalTexture = material.GetTexture(propertyName);
            var newTexture = GetTexture(texturePath);

            if (newTexture != null)
            {
                // apply new texture
                material.SetTexture(propertyName, newTexture);

                // mark old texture for unload
                if (originalTexture != null && !textureUsedMap.ContainsKey(originalTexture)) textureUsedMap[originalTexture] = false;

                return true;
            }
            else
            {
                // no new texture? keep old texture
                if (originalTexture != null) textureUsedMap[originalTexture] = true;

                return false;
            }
        }

        private Texture2D GetTexture(string texturePath)
        {
            Texture2D texture = null;

            if (!textures.TryGetValue(texturePath, out texture))
            {
                if (File.Exists(texturePath))
                {
                    texture = new Texture2D(1, 1);
                    texture.LoadImage(File.ReadAllBytes(texturePath));
                    texture.Compress(true);
                    texture.anisoLevel = 8;
                    texture.name = "TextureReplacer " + counter++;

                    //TODO save memory

                    textures[texturePath] = texture;
                }
            }

            return texture;
        }


        public void SearchTexturePacks()
        {
            mergedPack = new TexturePack("<merged>");
            textureSources.Clear();

            // User texture directory
            textureSources.Add("");

            foreach (var pluginInfo in PluginManager.instance.GetPluginsInfo().Where(pluginInfo => pluginInfo.isEnabled))
            {
                try
                {
                    var texturePack = TexturePack.Deserialize(Path.Combine(pluginInfo.modPath, TEXTURE_PACK_PATH));
                    if (texturePack != null)
                    {
                        mergedPack.Merge(texturePack);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error while parsing TexturePack.xml of mod " + pluginInfo.name);
                    Debug.LogException(e);
                }

                if (Directory.Exists(Path.Combine(pluginInfo.modPath, BUILDINGS_TEXTURE_DIR)))
                {
                    textureSources.Add(pluginInfo.modPath);
                }
            }
        }
    }
}

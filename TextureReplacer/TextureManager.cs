using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TextureReplacer
{
    public class TextureManager : MonoBehaviour
    {
        public const string MOD_TEXTURE_PREFIX = "TextureReplacer_";

        private readonly HashSet<Texture> unusedTextures = new HashSet<Texture>();

        private readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        private uint counter = 0;

        // TODO unload textures at some point?
        // Game breaks when you destroy textures when going back to main menu

        public Texture2D GetTexture(string texturePath)
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
                    texture.name = MOD_TEXTURE_PREFIX + counter++;

                    // TODO save RAM, send texture to graphics card

                    textures[texturePath] = texture;
                }
            }

            return texture;
        }

        public void MarkTextureUnused(Texture texture)
        {
            unusedTextures.Add(texture);
        }

        public void UnloadUnusedTextures()
        {
            foreach (var texture in unusedTextures)
            {
                UnityEngine.Object.Destroy(texture);
            }
            unusedTextures.Clear();
        }
    }
}

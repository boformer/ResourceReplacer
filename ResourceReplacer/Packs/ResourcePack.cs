using System;
using System.Collections.Generic;
using UnityEngine;

namespace ResourceReplacer.Packs {
    public abstract class ResourcePack {
        public abstract string Path { get; set; }
        public abstract Texture2D GetBuildingTexture(string textureName);

        public abstract bool TryGetBuildingColors(string prefabName, out PrefabColors colors);

        public struct PrefabColors : IEquatable<PrefabColors> {
            public bool UseColorVariations;
            public Color Color0;
            public Color Color1;
            public Color Color2;
            public Color Color3;

            public void SetColor(int index, Color color) {
                UseColorVariations = true;

                switch (index) {
                    case 0:
                        Color0 = color;
                        break;
                    case 1:
                        Color1 = color;
                        break;
                    case 2:
                        Color2 = color;
                        break;
                    case 3:
                        Color3 = color;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index), "Only values in the range [0,3] are supported.");
                }
            }

            public static PrefabColors From(BuildingInfo prefab) {
                return new PrefabColors {
                    UseColorVariations = prefab.m_useColorVariations,
                    Color0 = prefab.m_color0,
                    Color1 = prefab.m_color1,
                    Color2 = prefab.m_color2,
                    Color3 = prefab.m_color3
                };
            }

            public void Apply(BuildingInfo prefab) {
                prefab.m_useColorVariations = UseColorVariations;
                prefab.m_color0 = Color0;
                prefab.m_color1 = Color1;
                prefab.m_color2 = Color2;
                prefab.m_color3 = Color3;

                var material = prefab.GetComponent<Renderer>()?.sharedMaterial;
                if (material != null) {
                    material.SetColor("_ColorV0", Color0);
                    material.SetColor("_ColorV1", Color1);
                    material.SetColor("_ColorV2", Color2);
                    material.SetColor("_ColorV3", Color3);
                }
            }
            public bool Equals(PrefabColors other) {
                return UseColorVariations == other.UseColorVariations
                    && EqualityComparer<Color>.Default.Equals(Color0, other.Color0)
                    && EqualityComparer<Color>.Default.Equals(Color1, other.Color1)
                    && EqualityComparer<Color>.Default.Equals(Color2, other.Color2)
                    && EqualityComparer<Color>.Default.Equals(Color3, other.Color3);
            }

            public override string ToString() {
                return $"{nameof(UseColorVariations)}: {UseColorVariations.ToString()}, {nameof(Color0)}: {Color0.ToString()}, {nameof(Color1)}: {Color1.ToString()}, {nameof(Color2)}: {Color2.ToString()}, {nameof(Color3)}: {Color3.ToString()}";
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using ResourceReplacer.Pack;
using UnityEngine;

namespace ResourceReplacer.Packs {
    public class FileResourcePack : ResourcePack {
        public const string DefinitionFileName = "ResourcePack.xml";
        public const string BuildingTexturesDirectoryName = "Buildings";

        [XmlIgnore]
        public override string Path { get; set; }

        [XmlIgnore] public string BuildingTexturesPath => System.IO.Path.Combine(Path, BuildingTexturesDirectoryName);

        [XmlAttribute]
        public string Name { get; set; }

        [XmlArray, XmlArrayItem(ElementName = "Building"), DefaultValue(null)]
        public List<NamedPrefabColors> BuildingColors { get; set; }

        public override Texture2D GetBuildingTexture(string textureName) {
            if (BuildingTexturesPath == null) return null;

            var texturePath = System.IO.Path.Combine(BuildingTexturesPath, textureName + ".png");
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
            var item = BuildingColors?.FirstOrDefault(e => e.Name == prefabName);
            if (item != null) {
                colors = new PrefabColors {
                    Color0 = item.Color0,
                    Color1 = item.Color1,
                    Color2 = item.Color2,
                    Color3 = item.Color3
                };
                return true;
            } else {
                colors = default;
                return false;
            }
        }

        public void SetBuildingColors(string prefabName, PrefabColors colors) {
            var item = BuildingColors?.FirstOrDefault(e => e.Name == prefabName);
            if (item == null) {
                if(BuildingColors == null) BuildingColors = new List<NamedPrefabColors>();
                BuildingColors.Add(item = new NamedPrefabColors {Name = prefabName});
            }

            item.UseColorVariations = colors.UseColorVariation;
            item.Color0 = colors.Color0;
            item.Color1 = colors.Color1;
            item.Color2 = colors.Color2;
            item.Color3 = colors.Color3;
        }

        public void RemoveBuildingColors(string prefabName) {
            BuildingColors.RemoveAll(e => e.Name == prefabName);
        }


        public class NamedPrefabColors {
            [XmlAttribute]
            public string Name { get; set; }

            [XmlAttribute, DefaultValue(true)]
            public bool UseColorVariations { get; set; }

            [DefaultValue(null)]
            public Color Color0 { get; set; }

            [DefaultValue(null)]
            public Color Color1 { get; set; }

            [DefaultValue(null)]
            public Color Color2 { get; set; }

            [DefaultValue(null)]
            public Color Color3 { get; set; }
        }

        public class Color {
            [XmlAttribute]
            public byte r = 255;

            [XmlAttribute]
            public byte g = 255;

            [XmlAttribute]
            public byte b = 255;

            [XmlAttribute, DefaultValue(255)]
            public byte a = 255;

            public static implicit operator Color(UnityEngine.Color e) {
                var e32 = (Color32) e;
                return new Color { r = e32.r, g = e32.g, b = e32.b, a = e32.a };
            }
            public static implicit operator UnityEngine.Color(Color e) => new UnityEngine.Color32(e.r, e.g, e.b, e.a);
        }

        public static FileResourcePack Deserialize(string packPath) {
            try {
                var definitionFilePath = System.IO.Path.Combine(packPath, DefinitionFileName);
                if (!File.Exists(definitionFilePath)) return null;

                var pack = DeserializeDefinition(definitionFilePath);
                if (pack == null) return null;

                pack.Path = packPath;
                
                return pack;
            } catch {
                Debug.LogError("Couldn't load resource pack (XML malformed?)");
                throw;
            }
        }

        private static FileResourcePack DeserializeDefinition(string filePath) {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(FileResourcePack));
            using (var streamReader = new StreamReader(filePath)) {
                return (FileResourcePack)xmlSerializer.Deserialize(streamReader);
            }
        }

        public static void SerializeDefinition(string filePath, FileResourcePack pack) {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(FileResourcePack));
            using (var streamWriter = new StreamWriter(filePath)) {
                xmlSerializer.Serialize(streamWriter, pack);
            }
        }
    }
}

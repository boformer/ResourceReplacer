using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using UnityEngine;
namespace ResourceReplacer
{
    public class ResourcePack
    {
        [XmlAttribute]
        public string Name { get; set; }
        public int Version { get; set; }

        [XmlArray, XmlArrayItem(ElementName = "Building"), DefaultValue(null)]
        public List<Prefab> Buildings { get; set; }

        [XmlArray, DefaultValue(null)]
        public List<Entry> Changelog { get; set; }

        public ResourcePack(string name)
        {
            Name = name;
        }
        public ResourcePack() { }

        public Prefab GetBuilding(string name)
        {
            if (Buildings == null) return null;

            foreach (var prefab in Buildings)
            {
                if (prefab.Name == name) return prefab;
            }
            return null;
        }

        public String GetChangelog(int fromVersion) 
        {
            if (Changelog == null) return null;

            String log = "";

            foreach (var entry in Changelog.Where(entry => (entry.Version <= Version && entry.Version > fromVersion)).OrderByDescending(entry => entry.Version)) 
            {
                log += entry.Text;
                log += "\n";
            }

            return log == "" ? null : log;
        }

        public class Prefab
        {
            [XmlAttribute]
            public string Name { get; set; }

            [XmlAttribute, DefaultValue(null)]
            public bool? UseColorVariations { get; set; }

            /*
            [DefaultValue(null)]
            public string Texture {get; set;}
            */
            [DefaultValue(null)]
            public Color Color0 { get; set; }

            [DefaultValue(null)]
            public Color Color1 { get; set; }

            [DefaultValue(null)]
            public Color Color2 { get; set; }

            [DefaultValue(null)]
            public Color Color3 { get; set; }
        }

        public class Color
        {
            [XmlAttribute]
            public int r = 255;

            [XmlAttribute]
            public int g = 255;

            [XmlAttribute]
            public int b = 255;

            [XmlAttribute, DefaultValue(255)]
            public int a = 255;

            public Color() { }

            public UnityEngine.Color toUnityColor()
            {
                return new UnityEngine.Color(r / 255f, g / 255f, b / 255f, a / 255f);
            }
        }

        public class Entry
        {
            [XmlAttribute]
            public int Version { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        public void Merge(ResourcePack pack)
        {
            if (pack.Buildings == null) return;
            
            foreach (var prefab in pack.Buildings)
            {
                if (this.GetBuilding(prefab.Name) == null)
                {
                    if (Buildings == null) Buildings = new List<Prefab>();
                    Buildings.Add(prefab);
                }
            }
        }

        public static ResourcePack Deserialize(string fileName)
        {
            if (!File.Exists(fileName)) return null;

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ResourcePack));
            try
            {
                using (System.IO.StreamReader streamReader = new System.IO.StreamReader(fileName))
                {
                    return (ResourcePack)xmlSerializer.Deserialize(streamReader);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't load resource pack (XML malformed?)");
                throw e;
            }
        }
    }
}

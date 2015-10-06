using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
namespace ResourceReplacer
{
    public class ResourcePack
    {
        [XmlAttribute]
        public string Name { get; set; }

        private List<Prefab> _buildings;
        [XmlArray, XmlArrayItem(ElementName = "Building"), DefaultValue(null)]
        public List<Prefab> Buildings
        {
            get { return _buildings == null ? _buildings = new List<Prefab>() : _buildings; }
            set { _buildings = value; }
        }

        public ResourcePack(string name)
        {
            Name = name;
        }
        public ResourcePack() { }

        public Prefab GetBuilding(string name)
        {
            foreach (var prefab in Buildings)
            {
                if (prefab.Name == name) return prefab;
            }
            return null;
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

        public void Merge(ResourcePack pack)
        {
            foreach (var prefab in pack.Buildings)
            {
                if (this.GetBuilding(prefab.Name) == null) Buildings.Add(prefab);
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

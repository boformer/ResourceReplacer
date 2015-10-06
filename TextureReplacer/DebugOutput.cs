/*
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
namespace TextureReplacer
{
    public class DebugOutput
    {
        [XmlArray, XmlArrayItem(ElementName = "Texture")]
        public List<Texture> Textures = new List<Texture>();
        public Texture GetTexture(string name)
        {
            foreach (var texture in Textures)
            {
                if (texture.Name == name) return texture;
            }

            var tex = new Texture { Name = name };

            Textures.Add(tex);

            return tex;
        }

        public void Log(UnityEngine.Texture texture, string building)
        {
            if(texture != null) GetTexture(texture.name).Buildings.Add(building);
        }

        public class Texture
        {
            [XmlAttribute]
            public string Name { get; set; }

            [XmlArray, XmlArrayItem(ElementName = "Building")]
            public List<string> Buildings = new List<string>();
        }

        public static DebugOutput Deserialize(string fileName)
        {
            if (!File.Exists(fileName)) return null;

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DebugOutput));
            try
            {
                using (System.IO.StreamReader streamReader = new System.IO.StreamReader(fileName))
                {
                    return (DebugOutput)xmlSerializer.Deserialize(streamReader);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't load debug output (XML malformed?)");
                throw e;
            }
        }

        public static void Serialize(string fileName, DebugOutput config)
        {
            var xmlSerializer = new XmlSerializer(typeof(DebugOutput));
            try
            {
                using (System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(fileName))
                {
                    xmlSerializer.Serialize(streamWriter, config);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't create debug output file at \"" + Directory.GetCurrentDirectory() + "\"");
                throw e;
            }
        }
    }
}
*/
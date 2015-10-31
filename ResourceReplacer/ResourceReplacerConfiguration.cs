using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace ResourceReplacer
{
    public class ResourceReplacerConfiguration
    {
        [DefaultValue(true)]
        public bool DisplayChangelog { get; set; }
        
        [XmlArray, DefaultValue(null)]
        public List<ResourcePack> InstalledResourcePacks { get; set; }

        public class ResourcePack 
        {
            [XmlAttribute]
            public string Name { get; set; }

            [XmlAttribute]
            public int Version { get; set; }
        }
    }
}

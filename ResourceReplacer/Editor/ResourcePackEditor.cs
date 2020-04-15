using System;
using System.IO;
using ColossalFramework.IO;
using ResourceReplacer.Packs;

namespace ResourceReplacer.Editor {
    public class ResourcePackEditor : ModSingleton<ResourcePackEditor> {
        public FileResourcePack ActivePack { get; set; }

        public void SaveChanges() {
            Directory.CreateDirectory(ActivePack.Path);
            FileResourcePack.SerializeDefinition(ActivePack);
        }

        public static FileResourcePack GetOrCreateDevResourcePack() {
            var packPath = Path.Combine(DataLocation.localApplicationData, "DevResourcePack");

            try {
                var pack = FileResourcePack.Deserialize(packPath);

                if (pack == null) {
                    pack = new FileResourcePack {
                        Path = packPath,
                        Name = "Dev Pack"
                    };
                    Directory.CreateDirectory(packPath);
                    FileResourcePack.SerializeDefinition(pack);
                }

                return pack;
            } catch(Exception e) {
                UnityEngine.Debug.LogException(e);
                return null;
            }
        }
    }
}

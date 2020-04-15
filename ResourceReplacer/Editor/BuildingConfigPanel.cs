using System;
using System.IO;
using ColossalFramework.UI;
using ResourceReplacer.Pack;
using ResourceReplacer.Packs;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ResourceReplacer.Editor {
    public static class BuildingConfigPanel {
        private const string PanelName = "BuildingConfigPanel";
        public static void Install() {
            var parentPanel = GetParentPanel();
            if (parentPanel == null) {
                Debug.Log("ZonedBuildingWorldInfoPanel not found!");
                return;
            }

            if(parentPanel.Find<UIPanel>(PanelName)) Uninstall();

            var panel = parentPanel.AddUIComponent<UIPanel>();
            panel.name = PanelName;
            panel.width = parentPanel.width;
            panel.height = 35;
            panel.backgroundSprite = "SubcategoriesPanel";
            panel.opacity = 0.75f;
            panel.relativePosition = new Vector3(0f, -40f);

            panel.padding = new RectOffset(5, 5, 5, 5);
            panel.autoLayoutPadding = new RectOffset(0, 4, 0, 0);
            panel.autoLayout = true;
            panel.autoLayoutDirection = LayoutDirection.Horizontal;

            var dumpDiffuseButton = AddButton(panel, "Dump Diffuse");
            dumpDiffuseButton.eventClick += (comp, ev) => DumpTexture("_MainTex");

            var dumpXysButton = AddButton(panel, "Dump XYS");
            dumpXysButton.eventClick += (comp, ev) => DumpTexture("_XYSMap");

            var dumpAciButton = AddButton(panel, "Dump ACI");
            dumpAciButton.eventClick += (comp, ev) => DumpTexture("_ACIMap");

            var colorTestButton = AddButton(panel, "ColorTest");
            colorTestButton.eventClick += (comp, ev) => SetColors();

            var resetColorsButton = AddButton(panel, "Reset Colors");
            resetColorsButton.eventClick += (comp, ev) => ResetColors();
        }

        public static void Uninstall() {
            var parentPanel = GetParentPanel();
            if (parentPanel == null) return;

            var panel = parentPanel.Find<UIPanel>(PanelName);
            if (panel == null) return;

            Object.Destroy(panel.gameObject);
        }

        private static UIPanel GetParentPanel() {
            return UIView.Find<UIPanel>("(Library) ZonedBuildingWorldInfoPanel");
        }

        private static UIButton AddButton(UIComponent parent, string text) {
            var button = parent.AddUIComponent<UIButton>();
            button.text = text;
            button.textScale = 0.8f;
            button.autoSize = true;
            button.textPadding = new RectOffset(10, 10, 5, 5);
            button.normalBgSprite = "ButtonMenu";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.focusedBgSprite = "ButtonMenu";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.textColor = new Color32(255, 255, 255, 255);
            button.disabledTextColor = new Color32(7, 7, 7, 255);
            button.hoveredTextColor = new Color32(255, 255, 255, 255);
            button.focusedTextColor = new Color32(255, 255, 255, 255);
            button.pressedTextColor = new Color32(30, 30, 44, 255);
            return button;
        }

        private static void DumpTexture(string propertyName) {
            if (!ResourcePackEditor.exists || ResourcePackEditor.instance.ActivePack == null) {
                Debug.Log("No pack selected!");
                return;
            }

            var dumpDir = Path.Combine(ResourcePackEditor.instance.ActivePack.BuildingTexturesPath, "dump");

            var prefab = GetSelectedPrefab();
            if (prefab == null) return;

            if (prefab.m_material != null) {
                var texture = prefab.m_material.GetTexture(propertyName) as Texture2D;
                if (texture != null) {
                    var textureName = TextureNames.GetReplacementTextureName(prefab.name, texture, propertyName, false);
                    var fileName = Path.Combine(dumpDir, textureName + ".png");
                    DumpTexture2D(texture, fileName);
                }
            }

            if (prefab.m_lodMaterial != null) {
                var texture = prefab.m_lodMaterial.GetTexture(propertyName) as Texture2D;
                if (texture != null) {
                    var textureName = TextureNames.GetReplacementTextureName(prefab.name, texture, propertyName, true);
                    var fileName = Path.Combine(dumpDir, textureName + ".png");
                    DumpTexture2D(texture, fileName);
                }
            }
        }

        private static void DumpTexture2D(Texture2D texture, string fileName) {
            byte[] bytes;

            try {
                var readable = texture.MakeReadable();
                bytes = readable.EncodeToPNG();
                Object.Destroy(readable);
            } catch (Exception ex) {
                Debug.LogError("There was an error while dumping the texture - " + ex.Message);
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            File.WriteAllBytes(fileName, bytes);
            Debug.Log($"Texture dumped to \"{fileName}\"");
        }

        private static void SetColors() {
            if (!ResourcePackEditor.exists || ResourcePackEditor.instance.ActivePack == null) {
                Debug.Log("No pack selected!");
                return;
            }

            var prefab = GetSelectedPrefab();
            if (prefab == null) return;

            ResourceReplacer.instance.SetBuildingColors(prefab, new ResourcePack.PrefabColors {
                UseColorVariation = true,
                Color0 = Color.red,
                Color1 = Color.yellow,
                Color2 = Color.blue,
                Color3 = Color.green
            });
            BuildingManager.instance.UpdateBuildingColors();
        }

        private static void ResetColors() {
            if (!ResourcePackEditor.exists || ResourcePackEditor.instance.ActivePack == null) {
                Debug.Log("No pack selected!");
                return;
            }

            var prefab = GetSelectedPrefab();
            if (prefab == null) return;

            ResourceReplacer.instance.RestoreBuildingColors(prefab);
            BuildingManager.instance.UpdateBuildingColors();
        }

        private static BuildingInfo GetSelectedPrefab() {
            var currentBuilding = WorldInfoPanel.GetCurrentInstanceID().Building;
            if (currentBuilding == 0) {
                Debug.Log("No building selected!");
                return null;
            }

            return BuildingManager.instance.m_buildings.m_buffer[currentBuilding].Info;
        }
    }
}

using System;
using System.IO;
using ColossalFramework.UI;
using ResourceReplacer.Pack;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ResourceReplacer.Editor {
    public class BuildingConfigPanel : UIPanel {
        private const string PanelName = "BuildingConfigPanel";
        private const float PanelHeight = 5 + 25 + 5 + 25 + 5;

        private UIColorField colorPicker0;
        private UIColorField colorPicker1;
        private UIColorField colorPicker2;
        private UIColorField colorPicker3;
        private bool ignoreEvents = false;

        public static void Install() {
            var parentPanel = GetParentPanel();
            if (parentPanel == null) {
                Debug.Log("ZonedBuildingWorldInfoPanel not found!");
                return;
            }

            if(parentPanel.Find<UIPanel>(PanelName)) Uninstall();

            var panel = parentPanel.AddUIComponent<BuildingConfigPanel>();
            panel.name = PanelName;
            panel.width = parentPanel.width;
            panel.relativePosition = new Vector3(0f, -PanelHeight - 10);
        }

        public static void Uninstall() {
            var parentPanel = GetParentPanel();
            if (parentPanel == null) return;

            var panel = parentPanel.Find<UIPanel>(PanelName);
            if (panel == null) return;

            Object.Destroy(panel.gameObject);
        }

        public override void Awake() {
            base.Awake();

            backgroundSprite = "SubcategoriesPanel";

            padding = new RectOffset(5, 5, 5, 5);
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutPadding = new RectOffset(0, 0, 0, 5);
            height = PanelHeight;

            var textureRow = AddUIComponent<UIPanel>();
            textureRow.autoLayout = true;
            textureRow.autoLayoutDirection = LayoutDirection.Horizontal;
            textureRow.autoLayoutPadding = new RectOffset(0, 4, 0, 0);
            textureRow.height = 25;

            var dumpDiffuseButton = UIUtils.AddButton(textureRow, "Dump Diffuse");
            dumpDiffuseButton.eventClick += (comp, ev) => DumpTexture("_MainTex");

            var dumpXysButton = UIUtils.AddButton(textureRow, "Dump XYS");
            dumpXysButton.eventClick += (comp, ev) => DumpTexture("_XYSMap");

            var dumpAciButton = UIUtils.AddButton(textureRow, "Dump ACI");
            dumpAciButton.eventClick += (comp, ev) => DumpTexture("_ACIMap");

            var colorRow = AddUIComponent<UIPanel>();
            colorRow.autoLayout = true;
            colorRow.autoLayoutDirection = LayoutDirection.Horizontal;
            colorRow.autoLayoutPadding = new RectOffset(0, 4, 0, 0);
            colorRow.autoFitChildrenVertically = true;
            colorRow.height = 25;

            colorPicker0 = UIUtils.AddColorPicker(colorRow);
            colorPicker0.eventSelectedColorChanged += (comp, color) => {
                if (!ignoreEvents) SetColorTemp(0, color);
            };

            colorPicker1 = UIUtils.AddColorPicker(colorRow);
            colorPicker1.eventSelectedColorChanged += (comp, color) => {
                if (!ignoreEvents) SetColorTemp(1, color);
            };

            colorPicker2 = UIUtils.AddColorPicker(colorRow);
            colorPicker2.eventSelectedColorChanged += (comp, color) => {
                if (!ignoreEvents) SetColorTemp(2, color);
            };

            colorPicker3 = UIUtils.AddColorPicker(colorRow);
            colorPicker3.eventSelectedColorChanged += (comp, color) => {
                if (!ignoreEvents) SetColorTemp(3, color);
            };

            var saveColorsButton = UIUtils.AddButton(colorRow, "Save Colors");
            saveColorsButton.eventClick += (comp, ev) => SaveColors();

            var resetColorsButton = UIUtils.AddButton(colorRow, "Reset Colors");
            resetColorsButton.eventClick += (comp, ev) => ResetColors();
        }

        public override void Update() {
            base.Update();

            var prefab = GetSelectedPrefab();
            if (prefab != null) {
                ignoreEvents = true;
                colorPicker0.selectedColor = prefab.m_color0;
                colorPicker1.selectedColor = prefab.m_color1;
                colorPicker2.selectedColor = prefab.m_color2;
                colorPicker3.selectedColor = prefab.m_color3;
                ignoreEvents = false;
            }
        }

        private static UIPanel GetParentPanel() {
            return UIView.Find<UIPanel>("(Library) ZonedBuildingWorldInfoPanel");
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

        private static void SetColorTemp(int index, Color color) {
            var prefab = GetSelectedPrefab();
            if (prefab == null) return;

            var colors = ResourcePack.PrefabColors.From(prefab);
            colors.SetColor(index, color);
            ResourceReplacer.instance.SetBuildingColors(prefab, colors);
            BuildingManager.instance.UpdateBuildingColors();
        }

        private static void SaveColors() {
            if (!ResourcePackEditor.exists || ResourcePackEditor.instance.ActivePack == null) {
                Debug.Log("No pack selected!");
                return;
            }

            var prefab = GetSelectedPrefab();
            if (prefab == null) return;

            var defaultColors = ResourceReplacer.instance.GetOriginalBuildingColors(prefab);
            var colors = ResourcePack.PrefabColors.From(prefab);
            if (!colors.Equals(defaultColors)) {
                ResourcePackEditor.instance.ActivePack.SetBuildingColors(prefab.name, colors);
            } else {
                ResourcePackEditor.instance.ActivePack.RemoveBuildingColors(prefab.name);
            }

            ResourcePackEditor.instance.SaveChanges();
        }

        private static void ResetColors() {
            var prefab = GetSelectedPrefab();
            if (prefab == null) return;

            ResourcePackEditor.instance.ActivePack.RemoveBuildingColors(prefab.name);

            ResourceReplacer.instance.RestoreBuildingColors(prefab);
            BuildingManager.instance.UpdateBuildingColors();
        }

        private static BuildingInfo GetSelectedPrefab() {
            var currentBuilding = WorldInfoPanel.GetCurrentInstanceID().Building;
            if (currentBuilding == 0) return null;

            return BuildingManager.instance.m_buildings.m_buffer[currentBuilding].Info;
        }
    }
}

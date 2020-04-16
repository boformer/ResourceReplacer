using ColossalFramework.UI;
using UnityEngine;

namespace ResourceReplacer.Editor {
    public static class UIUtils {
        public static UIButton AddButton(UIComponent parent, string text) {
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

        public static UIColorField AddColorPicker(UIComponent component) {
            var colorField = Object.Instantiate<UIColorField>(
                UITemplateManager.Get<UIPanel>("LineTemplate")
                    .Find<UIColorField>("LineColor")
            );
            component.AttachUIComponent(colorField.gameObject);

            colorField.width = 40f;
            colorField.height = 25f;
            colorField.color = new Color32(50, 50, 50, 255);
            colorField.normalBgSprite = "ColorPickerOutline";
            colorField.normalFgSprite = "ColorPickerColor";
            colorField.enabled = true;
            colorField.eventColorPickerOpen += (UIColorField field, UIColorPicker picker, ref bool overriden) => { field.isInteractive = false; };
            colorField.eventColorPickerClose += (UIColorField field, UIColorPicker picker, ref bool overriden) => { field.isInteractive = true; };

            return colorField;
        }

        public static UICheckBox AddCheckBox(UIComponent parent, string text)
        {
            var checkboxTemplate = UITemplateManager.GetAsGameObject("OptionsCheckBoxTemplate");
            var checkbox = parent.AttachUIComponent(checkboxTemplate) as UICheckBox;
            checkbox.text = text;
            checkbox.width = 40f;
            checkbox.height = 25f;
            checkbox.enabled = true;
            checkbox.isChecked = true;
            return checkbox;
        }
    }
}

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LWalida
{
    public static class UIFactory
    {
        private static Font _font;
        public static Sprite PanelSprite { get; private set; }
        public static Sprite WhiteSprite { get; private set; }
        public static Sprite PaperSprite { get; private set; }
        public static Sprite ZelligeSprite { get; private set; }
        public static Sprite VignetteSprite { get; private set; }
        public static Sprite GrainSprite { get; private set; }

        public static Font DefaultFont
        {
            get
            {
                if (_font != null) return _font;
                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                return _font;
            }
        }

        public static void Initialize(AssetCatalog content)
        {
            PanelSprite = content != null ? content.panelDark : null;
            WhiteSprite = content != null ? content.whitePixel : null;
            PaperSprite = content != null ? content.paperTexture : null;
            ZelligeSprite = content != null ? content.zelligePattern : null;
            VignetteSprite = content != null ? content.vignette : null;
            GrainSprite = content != null ? content.grain : null;

            if (WhiteSprite == null) WhiteSprite = CreateRuntimeWhite();
            if (PanelSprite == null) PanelSprite = WhiteSprite;
        }

        private static Sprite _runtimeWhite;

        private static Sprite CreateRuntimeWhite()
        {
            if (_runtimeWhite != null) return _runtimeWhite;
            var texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            var pixels = new Color32[64];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
            texture.SetPixels32(pixels);
            texture.Apply();
            texture.name = "LWalidaRuntimeWhite";
            _runtimeWhite = Sprite.Create(texture, new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 100f,
                0, SpriteMeshType.FullRect, new Vector4(2f, 2f, 2f, 2f));
            _runtimeWhite.name = "WhitePixel";
            return _runtimeWhite;
        }

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.localScale = Vector3.one;
            return rect;
        }

        public static RectTransform CreatePanel(string name, Transform parent, Color color, Sprite sprite = null,
            bool raycast = true)
        {
            var rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            if (sprite == null) sprite = PanelSprite;
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = sprite.border.sqrMagnitude > 0.01f ? Image.Type.Sliced : Image.Type.Simple;
            }
            image.raycastTarget = raycast;
            return rect;
        }

        public static Image CreateImage(string name, Transform parent, Color color, Sprite sprite = null,
            bool raycast = false)
        {
            var rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.sprite = sprite != null ? sprite : WhiteSprite;
            image.raycastTarget = raycast;
            image.preserveAspect = false;
            return image;
        }

        public static Text CreateText(string name, Transform parent, string value, int fontSize, Color color,
            TextAnchor alignment = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
        {
            var rect = CreateRect(name, parent);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = DefaultFont;
            text.text = value;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = true;
            text.raycastTarget = false;
            return text;
        }

        public static Button CreateButton(Transform parent, string label, UnityAction onClick, int fontSize = 28,
            float height = 62f, Color? background = null)
        {
            var rect = CreateRect("Button_" + label, parent);
            var image = rect.gameObject.AddComponent<Image>();
            var baseColor = background ?? Theme.OudayaBlue;
            image.color = baseColor;
            if (PanelSprite != null)
            {
                image.sprite = PanelSprite;
                image.type = PanelSprite.border.sqrMagnitude > 0.01f ? Image.Type.Sliced : Image.Type.Simple;
            }

            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.18f, 1.10f, 0.94f, 1f);
            colors.pressedColor = new Color(0.82f, 0.72f, 0.58f, 1f);
            colors.selectedColor = new Color(1.12f, 1.04f, 0.88f, 1f);
            colors.disabledColor = new Color(0.45f, 0.46f, 0.45f, 0.65f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            var layout = rect.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = height;
            layout.minHeight = height;

            var text = CreateText("Label", rect, label, fontSize, Theme.ChalkWhite, TextAnchor.MiddleCenter,
                FontStyle.Normal);
            Stretch(text.rectTransform, 22f, 6f, 22f, 6f);

            button.onClick.AddListener(() =>
            {
                var game = GameManager.Instance;
                if (game != null && game.Audio != null) game.Audio.PlayUi();
                onClick?.Invoke();
            });
            return button;
        }

        public static Slider CreateSlider(Transform parent, float value, UnityAction<float> onChanged)
        {
            var rect = CreateRect("Slider", parent);
            rect.sizeDelta = new Vector2(460f, 34f);

            var background = CreateImage("Background", rect, new Color(0.04f, 0.08f, 0.10f, 0.85f), null, true);
            Stretch(background.rectTransform, 0f, 13f, 0f, 13f);

            var fillArea = CreateRect("Fill Area", rect);
            Stretch(fillArea, 0f, 13f, 0f, 13f);
            var fill = CreateImage("Fill", fillArea, Theme.HassanSand);
            Stretch(fill.rectTransform);

            var handleArea = CreateRect("Handle Slide Area", rect);
            Stretch(handleArea, 8f, 0f, -8f, 0f);
            var handle = CreateImage("Handle", handleArea, Theme.ChalkWhite, null, true);
            handle.rectTransform.sizeDelta = new Vector2(22f, 34f);

            var slider = rect.gameObject.AddComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.value = Mathf.Clamp01(value);
            slider.onValueChanged.AddListener(v => onChanged?.Invoke(v));
            return slider;
        }

        public static Toggle CreateToggle(Transform parent, bool value, UnityAction<bool> onChanged)
        {
            var rect = CreateRect("Toggle", parent);
            rect.sizeDelta = new Vector2(38f, 34f);

            var background = CreateImage("Background", rect, new Color(0.05f, 0.10f, 0.12f, 0.9f), null, true);
            Stretch(background.rectTransform, 2f, 2f, 2f, 2f);

            var check = CreateImage("Checkmark", background.rectTransform, Theme.HassanSand);
            Stretch(check.rectTransform, 7f, 7f, 7f, 7f);

            var toggle = rect.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = background;
            toggle.graphic = check;
            toggle.isOn = value;
            toggle.onValueChanged.AddListener(v => onChanged?.Invoke(v));
            return toggle;
        }

        public static CanvasGroup AddGroup(GameObject go, float alpha = 1f)
        {
            var group = go.GetComponent<CanvasGroup>();
            if (group == null) group = go.AddComponent<CanvasGroup>();
            group.alpha = alpha;
            return group;
        }

        public static void Stretch(RectTransform rect, float left = 0f, float bottom = 0f, float right = 0f,
            float top = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        public static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        public static void PlaceLeft(RectTransform rect, float x, float y, float width, float height)
        {
            Anchor(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x, -y), new Vector2(width, height));
        }

        public static void PlaceCenter(RectTransform rect, float width, float height)
        {
            Anchor(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(width, height));
        }

        public static void SetButtonColor(Button button, Color color)
        {
            if (button == null) return;
            var image = button.targetGraphic as Image;
            if (image != null) image.color = color;
        }

        public static void DestroyChildren(Transform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(parent.GetChild(i).gameObject);
            }
        }
    }
}

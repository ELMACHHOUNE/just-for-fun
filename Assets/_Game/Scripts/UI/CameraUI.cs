using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LWalida
{
    public class CameraUI : ScreenBase
    {
        private Text _doorName;
        private Text _feedback;
        private Image _reference;
        private Image _flash;
        private Coroutine _flashRoutine;

        protected override void OnBuild()
        {
            var dim = UIFactory.CreateImage("Dim", transform, new Color(0.01f, 0.03f, 0.04f, 0.52f), null, true);
            UIFactory.Stretch(dim.rectTransform);

            var title = UIFactory.CreateText("Title", transform, "Camera", 40, Theme.ChalkWhite,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFactory.PlaceLeft(title.rectTransform, 74f, 54f, 400f, 52f);

            var hint = UIFactory.CreateText("Hint", transform, "Frame a door and compare it with the photograph.", 24,
                Theme.TextDim, TextAnchor.MiddleLeft);
            UIFactory.PlaceLeft(hint.rectTransform, 76f, 106f, 760f, 34f);

            CreateFrame(transform);

            var referencePanel = UIFactory.CreatePanel("Reference", transform, new Color(0.949f, 0.929f, 0.890f, 0.96f),
                UIFactory.PaperSprite, true);
            UIFactory.Anchor(referencePanel, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-64f, 10f), new Vector2(430f, 600f));

            var refTitle = UIFactory.CreateText("RefTitle", referencePanel, "The photograph", 25, Theme.MidnightBlue,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFactory.PlaceLeft(refTitle.rectTransform, 28f, 22f, 370f, 34f);

            var photoFrame = UIFactory.CreatePanel("PhotoFrame", referencePanel, Theme.MidnightBlue, null, false);
            UIFactory.Anchor(photoFrame, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -72f), new Vector2(370f, 410f));

            _reference = UIFactory.CreateImage("ReferencePhoto", photoFrame, Color.white, null);
            UIFactory.Stretch(_reference.rectTransform, 12f, 12f, 12f, 12f);
            _reference.preserveAspect = true;

            var refCaption = UIFactory.CreateText("RefCaption", referencePanel,
                "Horseshoe arch \u00b7 paired round handles \u00b7 three rows of studs", 18, Theme.MemorySepia,
                TextAnchor.UpperCenter, FontStyle.Italic);
            UIFactory.Anchor(refCaption.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 24f), new Vector2(-40f, 54f));

            _doorName = UIFactory.CreateText("DoorName", transform, "", 30, Theme.HassanSand,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFactory.Anchor(_doorName.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(78f, 190f), new Vector2(800f, 44f));

            _feedback = UIFactory.CreateText("Feedback", transform, "", 27, Theme.ChalkWhite,
                TextAnchor.MiddleLeft);
            UIFactory.Anchor(_feedback.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(78f, 116f), new Vector2(1180f, 62f));

            var controls = UIFactory.CreateText("Controls", transform,
                "Space \u2014 Capture    \u00b7    Esc \u2014 Close", 23, Theme.TextDim, TextAnchor.LowerLeft);
            UIFactory.Anchor(controls.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(78f, 48f), new Vector2(760f, 36f));

            var capture = UIFactory.CreateButton(transform, "Capture", () =>
            {
                if (GameManager.Instance != null && GameManager.Instance.Camera != null)
                {
                    GameManager.Instance.Camera.Capture();
                }
            }, 26, 62f, Theme.OudayaBlue);
            UIFactory.Anchor(capture.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 0f), new Vector2(-574f, 48f), new Vector2(240f, 62f));

            var close = UIFactory.CreateButton(transform, "Close", () =>
            {
                if (GameManager.Instance != null && GameManager.Instance.Camera != null)
                {
                    GameManager.Instance.Camera.Close();
                }
            }, 26, 62f, Theme.MidnightBlue);
            UIFactory.Anchor(close.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 0f), new Vector2(-310f, 48f), new Vector2(240f, 62f));

            _flash = UIFactory.CreateImage("Flash", transform, new Color(1f, 1f, 1f, 0f), null, false);
            UIFactory.Stretch(_flash.rectTransform);

            Hide();
        }

        private void CreateFrame(Transform parent)
        {
            float alpha = 0.75f;
            var color = new Color(0.95f, 0.93f, 0.89f, alpha);
            float inset = 120f;
            float length = 96f;
            float thickness = 4f;

            CreateBar(parent, "TopLeftH", new Vector2(0f, 1f), new Vector2(inset, -inset), length, thickness, color, false);
            CreateBar(parent, "TopLeftV", new Vector2(0f, 1f), new Vector2(inset, -inset), thickness, length, color, true);
            CreateBar(parent, "TopRightH", new Vector2(1f, 1f), new Vector2(-inset, -inset), length, thickness, color, false, true);
            CreateBar(parent, "TopRightV", new Vector2(1f, 1f), new Vector2(-inset, -inset), thickness, length, color, true);
            CreateBar(parent, "BottomLeftH", new Vector2(0f, 0f), new Vector2(inset, inset), length, thickness, color, false);
            CreateBar(parent, "BottomLeftV", new Vector2(0f, 0f), new Vector2(inset, inset), thickness, length, color, true);
            CreateBar(parent, "BottomRightH", new Vector2(1f, 0f), new Vector2(-inset, inset), length, thickness, color, false, true);
            CreateBar(parent, "BottomRightV", new Vector2(1f, 0f), new Vector2(-inset, inset), thickness, length, color, true);
        }

        private static void CreateBar(Transform parent, string name, Vector2 anchor, Vector2 offset, float width,
            float height, Color color, bool tall, bool flipX = false)
        {
            var bar = UIFactory.CreateImage(name, parent, color);
            var pivot = anchor;
            if (flipX) pivot.x = 1f;
            if (anchor.y < 0.5f) pivot.y = 0f;
            else pivot.y = 1f;
            UIFactory.Anchor(bar.rectTransform, anchor, anchor, pivot, offset, new Vector2(width, height));
        }

        public override void Show()
        {
            base.Show();
            if (Root != null) Root.ClearSelection();
        }

        public void Show(PhotoDoor door, Sprite reference)
        {
            if (!gameObject.activeSelf) Show();
            _doorName.text = door != null ? "Door: " + door.DisplayName : "";
            _reference.sprite = reference;
            _reference.enabled = reference != null;
            _feedback.text = "Press Space to capture. Compare the arch, handles and studs.";
            _feedback.color = Theme.ChalkWhite;
            if (_flash != null) _flash.color = new Color(1f, 1f, 1f, 0f);
            if (Root != null) Root.ClearSelection();
        }

        public void SetFeedback(string text, bool positive)
        {
            if (_feedback == null) return;
            _feedback.text = text;
            _feedback.color = positive ? Theme.Positive : Theme.Negative;
        }

        private void Update()
        {
            var game = GameManager.Instance;
            if (game == null || game.Mode != GameMode.Camera) return;
            if (LWalidaInput.SpacePressed && game.Camera != null)
            {
                game.Camera.Capture();
            }
        }

        public void Flash()
        {
            if (_flash == null) return;
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            float peak = SettingsService.ReducedFlashes ? 0.16f : 0.82f;
            float fade = SettingsService.ReducedFlashes ? 0.12f : 0.35f;
            _flash.color = new Color(1f, 1f, 1f, peak);
            float t = 0f;
            while (t < fade)
            {
                t += Time.unscaledDeltaTime;
                _flash.color = new Color(1f, 1f, 1f, Mathf.Lerp(peak, 0f, t / fade));
                yield return null;
            }
            _flash.color = new Color(1f, 1f, 1f, 0f);
            _flashRoutine = null;
        }

        protected override void OnHide()
        {
            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
                _flashRoutine = null;
            }
            if (_flash != null) _flash.color = new Color(1f, 1f, 1f, 0f);
        }
    }
}

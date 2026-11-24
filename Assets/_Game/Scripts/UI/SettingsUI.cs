using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LWalida
{
    public class SettingsUI : ScreenBase
    {
        private readonly List<Slider> _sliders = new List<Slider>();
        private readonly List<Text> _sliderValues = new List<Text>();
        private readonly List<Toggle> _toggles = new List<Toggle>();
        private Slider _firstSlider;
        private bool _refreshing;

        protected override void OnBuild()
        {
            var dim = UIFactory.CreateImage("Dim", transform, new Color(0.01f, 0.02f, 0.03f, 0.72f), null, true);
            UIFactory.Stretch(dim.rectTransform);

            var panel = UIFactory.CreatePanel("Panel", transform, Theme.Panel, UIFactory.PanelSprite, true);
            UIFactory.PlaceCenter(panel, 1040f, 860f);

            var title = UIFactory.CreateText("Title", panel, "Settings", 44, Theme.ChalkWhite, TextAnchor.MiddleCenter,
                FontStyle.Bold);
            UIFactory.Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(900f, 54f));

            var subtitle = UIFactory.CreateText("Subtitle", panel,
                "Audio, accessibility and display. Changes are saved immediately.", 20, Theme.TextDim,
                TextAnchor.MiddleCenter, FontStyle.Italic);
            UIFactory.Anchor(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -84f), new Vector2(900f, 28f));

            _firstSlider = AddSliderRow(panel, "Master volume", -150, v =>
            {
                if (_refreshing) return;
                SettingsService.SetMaster(v);
                UpdateValueLabel(0, v);
            });
            _sliders.Add(_firstSlider);
            _sliders.Add(AddSliderRow(panel, "Music", -214, v =>
            {
                if (_refreshing) return;
                SettingsService.SetMusic(v);
                UpdateValueLabel(1, v);
            }));
            _sliders.Add(AddSliderRow(panel, "Ambience", -278, v =>
            {
                if (_refreshing) return;
                SettingsService.SetAmbience(v);
                UpdateValueLabel(2, v);
            }));
            _sliders.Add(AddSliderRow(panel, "Dialogue", -342, v =>
            {
                if (_refreshing) return;
                SettingsService.SetDialogue(v);
                UpdateValueLabel(3, v);
            }));
            _sliders.Add(AddSliderRow(panel, "Effects", -406, v =>
            {
                if (_refreshing) return;
                SettingsService.SetSfx(v);
                UpdateValueLabel(4, v);
            }));

            var accessibility = UIFactory.CreateText("Section", panel, "Accessibility", 22, Theme.HassanSand,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFactory.PlaceLeft(accessibility.rectTransform, 56f, 486f, 400f, 30f);

            AddToggleRow(panel, "Subtitles & captions", -520, v =>
            {
                if (!_refreshing) SettingsService.SetSubtitles(v);
            });
            AddToggleRow(panel, "Fullscreen", -576, v =>
            {
                if (!_refreshing) SettingsService.SetFullscreen(v);
            });
            AddToggleRow(panel, "Reduce camera flashes", -632, v =>
            {
                if (!_refreshing) SettingsService.SetReducedFlashes(v);
            });
            AddToggleRow(panel, "Reduce motion & camera sway", -688, v =>
            {
                if (!_refreshing) SettingsService.SetReducedMotion(v);
            });

            var reset = UIFactory.CreateButton(panel, "Reset defaults", () =>
            {
                SettingsService.ResetToDefaults();
                RefreshValues();
            }, 22, 50f, Theme.MidnightBlue);
            UIFactory.Anchor(reset.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(-170f, 32f), new Vector2(280f, 50f));

            var close = UIFactory.CreateButton(panel, "Close", Close, 24, 54f, Theme.OudayaBlue);
            UIFactory.Anchor(close.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(170f, 32f), new Vector2(280f, 54f));

            Hide();
        }

        private Slider AddSliderRow(Transform panel, string label, float top, UnityEngine.Events.UnityAction<float> onChange)
        {
            int index = _sliderValues.Count;
            var name = UIFactory.CreateText("Label_" + label, panel, label, 24, Theme.ChalkWhite,
                TextAnchor.MiddleLeft);
            UIFactory.PlaceLeft(name.rectTransform, 56f, -top, 280f, 40f);

            var slider = UIFactory.CreateSlider(panel, 0.8f, onChange);
            UIFactory.PlaceLeft(slider.GetComponent<RectTransform>(), 352f, -top - 4f, 520f, 40f);

            var value = UIFactory.CreateText("Value_" + label, panel, "", 22, Theme.HassanSand,
                TextAnchor.MiddleRight);
            UIFactory.PlaceLeft(value.rectTransform, 890f, -top, 94f, 40f);

            _sliderValues.Add(value);
            UpdateValueLabel(index, 0.8f);
            return slider;
        }

        private void AddToggleRow(Transform panel, string label, float top, UnityEngine.Events.UnityAction<bool> onChange)
        {
            var name = UIFactory.CreateText("ToggleLabel_" + label, panel, label, 23, Theme.ChalkWhite,
                TextAnchor.MiddleLeft);
            UIFactory.PlaceLeft(name.rectTransform, 56f, -top, 700f, 40f);

            var toggle = UIFactory.CreateToggle(panel, true, onChange);
            UIFactory.PlaceLeft(toggle.GetComponent<RectTransform>(), 916f, -top - 2f, 44f, 40f);
            _toggles.Add(toggle);
        }

        private void UpdateValueLabel(int index, float value)
        {
            if (index >= 0 && index < _sliderValues.Count && _sliderValues[index] != null)
            {
                _sliderValues[index].text = Mathf.RoundToInt(value * 100f) + "%";
            }
        }

        public void Open()
        {
            base.Show();
            RefreshValues();
            if (Root != null) Root.Select(_firstSlider);
        }

        public void Close()
        {
            SettingsService.Save();
            Hide();
        }

        private void RefreshValues()
        {
            _refreshing = true;
            if (_sliders.Count >= 5)
            {
                _sliders[0].SetValueWithoutNotify(SettingsService.Master);
                _sliders[1].SetValueWithoutNotify(SettingsService.Music);
                _sliders[2].SetValueWithoutNotify(SettingsService.Ambience);
                _sliders[3].SetValueWithoutNotify(SettingsService.Dialogue);
                _sliders[4].SetValueWithoutNotify(SettingsService.Sfx);
                UpdateValueLabel(0, SettingsService.Master);
                UpdateValueLabel(1, SettingsService.Music);
                UpdateValueLabel(2, SettingsService.Ambience);
                UpdateValueLabel(3, SettingsService.Dialogue);
                UpdateValueLabel(4, SettingsService.Sfx);
            }

            if (_toggles.Count >= 4)
            {
                _toggles[0].SetIsOnWithoutNotify(SettingsService.Subtitles);
                _toggles[1].SetIsOnWithoutNotify(SettingsService.Fullscreen);
                _toggles[2].SetIsOnWithoutNotify(SettingsService.ReducedFlashes);
                _toggles[3].SetIsOnWithoutNotify(SettingsService.ReducedMotion);
            }
            _refreshing = false;
        }
    }
}

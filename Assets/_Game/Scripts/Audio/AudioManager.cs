using System.Collections;
using UnityEngine;

namespace LWalida
{
    public class AudioManager : MonoBehaviour
    {
        private AudioSource _music;
        private AudioSource _ambience;
        private AudioSource _dialogue;
        private AudioSource _sfx;
        private AudioSource _ui;
        private AudioSource _memory;
        private Coroutine _musicRoutine;
        private Coroutine _ambienceRoutine;

        public AudioClip CurrentMusic { get; private set; }
        public AudioClip CurrentAmbience { get; private set; }

        public void Build()
        {
            _music = CreateSource("Music", true);
            _ambience = CreateSource("Ambience", true);
            _dialogue = CreateSource("Dialogue", false);
            _sfx = CreateSource("SFX", false);
            _ui = CreateSource("UI", false);
            _memory = CreateSource("MemoryPlayback", false);
            SettingsService.Changed += ApplySettings;
            ApplySettings();
        }

        private AudioSource CreateSource(string sourceName, bool loop)
        {
            var go = new GameObject("Audio_" + sourceName);
            go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            source.volume = 0f;
            return source;
        }

        private void OnDestroy()
        {
            SettingsService.Changed -= ApplySettings;
        }

        public void ApplySettings()
        {
            if (_music == null) return;
            float master = SettingsService.Master;
            _music.volume = CurrentMusic != null ? master * SettingsService.Music : 0f;
            _ambience.volume = CurrentAmbience != null ? master * SettingsService.Ambience : 0f;
            _dialogue.volume = master * SettingsService.Dialogue;
            _sfx.volume = master * SettingsService.Sfx;
            _ui.volume = master * SettingsService.Sfx;
            _memory.volume = master * SettingsService.Ambience;
        }

        private float MusicTarget => SettingsService.Master * SettingsService.Music;
        private float AmbienceTarget => SettingsService.Master * SettingsService.Ambience;

        public void SetSceneAudio(AudioClip ambience, AudioClip music)
        {
            if (_music == null) Build();

            if (CurrentMusic != music)
            {
                CurrentMusic = music;
                if (_musicRoutine != null) StopCoroutine(_musicRoutine);
                _musicRoutine = StartCoroutine(Crossfade(_music, music, MusicTarget, 0.8f));
            }
            else if (music != null && !_music.isPlaying)
            {
                _music.clip = music;
                _music.volume = 0f;
                _music.Play();
                _musicRoutine = StartCoroutine(FadeTo(_music, MusicTarget, 0.8f));
            }

            if (CurrentAmbience != ambience)
            {
                CurrentAmbience = ambience;
                if (_ambienceRoutine != null) StopCoroutine(_ambienceRoutine);
                _ambienceRoutine = StartCoroutine(Crossfade(_ambience, ambience, AmbienceTarget, 0.9f));
            }
            else if (ambience != null && !_ambience.isPlaying)
            {
                _ambience.clip = ambience;
                _ambience.volume = 0f;
                _ambience.Play();
                _ambienceRoutine = StartCoroutine(FadeTo(_ambience, AmbienceTarget, 0.9f));
            }
        }

        private IEnumerator Crossfade(AudioSource source, AudioClip next, float target, float duration)
        {
            float start = source.volume;
            float t = 0f;
            float fadeOut = duration * 0.5f;
            while (t < fadeOut)
            {
                t += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(start, 0f, t / fadeOut);
                yield return null;
            }

            source.Stop();
            source.volume = 0f;
            if (next != null)
            {
                source.clip = next;
                source.Play();
                yield return FadeTo(source, target, duration * 0.65f);
            }
        }

        private IEnumerator FadeTo(AudioSource source, float target, float duration)
        {
            float start = source.volume;
            float t = 0f;
            if (duration <= 0.01f)
            {
                source.volume = target;
                yield break;
            }
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(start, target, t / duration);
                yield return null;
            }
            source.volume = target;
        }

        public void PlaySfx(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null || _sfx == null) return;
            _sfx.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
        }

        public void PlayUi()
        {
            if (GameManager.Instance != null && GameManager.Instance.Content != null)
            {
                PlayUi(GameManager.Instance.Content.sfxUiClick);
            }
        }

        public void PlayUi(AudioClip clip)
        {
            if (clip == null || _ui == null) return;
            _ui.PlayOneShot(clip, 1f);
        }

        public void PlayFootstep()
        {
            if (GameManager.Instance != null && GameManager.Instance.Content != null)
            {
                PlaySfx(GameManager.Instance.Content.sfxFootstep, 0.25f);
            }
        }

        public void PlayShutter()
        {
            if (GameManager.Instance != null && GameManager.Instance.Content != null)
            {
                PlaySfx(GameManager.Instance.Content.sfxShutter, 0.8f);
            }
        }

        public void PlayKnock()
        {
            if (GameManager.Instance != null && GameManager.Instance.Content != null)
            {
                PlaySfx(GameManager.Instance.Content.sfxKnock, 0.9f);
            }
        }

        public void PlayMemoryUnlock()
        {
            if (GameManager.Instance != null && GameManager.Instance.Content != null)
            {
                PlaySfx(GameManager.Instance.Content.sfxMemoryUnlock, 0.9f);
            }
        }

        public void PlayRecordingComplete()
        {
            if (GameManager.Instance != null && GameManager.Instance.Content != null)
            {
                PlaySfx(GameManager.Instance.Content.sfxRecordComplete, 0.9f);
            }
        }

        public void PlayDialogueVoice(AudioClip clip)
        {
            if (_dialogue == null) return;
            _dialogue.Stop();
            if (clip == null) return;
            _dialogue.clip = clip;
            _dialogue.Play();
        }

        public void StopDialogueVoice()
        {
            if (_dialogue != null) _dialogue.Stop();
        }

        public void PlayMemoryClip(AudioClip clip)
        {
            if (_memory == null) return;
            _memory.Stop();
            if (clip == null) return;
            _memory.clip = clip;
            _memory.loop = false;
            _memory.Play();
        }

        public void StopMemoryClip()
        {
            if (_memory != null) _memory.Stop();
        }

        public bool IsMemoryClipPlaying => _memory != null && _memory.isPlaying;
    }
}

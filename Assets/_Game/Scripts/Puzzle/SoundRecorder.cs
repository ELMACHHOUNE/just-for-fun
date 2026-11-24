using System.Collections;
using UnityEngine;

namespace LWalida
{
    public class SoundRecorder : MonoBehaviour
    {
        public const float RecordDuration = 3f;

        private RecordingZone _zone;
        private float _elapsed;
        private bool _recording;
        private bool _finishing;
        private Coroutine _finishRoutine;

        public bool IsRecording => _recording;
        public float Elapsed => _elapsed;

        private void Update()
        {
            var game = GameManager.Instance;
            if (game == null) return;

            if (game.Mode == GameMode.Exploring && LWalidaInput.RecordPressed)
            {
                TryStartNearby(game);
                return;
            }

            if (game.Mode != GameMode.Recording || !_recording) return;

            _elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsed / RecordDuration);
            if (game.UI != null) game.UI.SetRecordingProgress(progress);

            if (progress >= 1f) Complete();
        }

        private void TryStartNearby(GameManager game)
        {
            var player = PlayerController.Instance;
            if (player == null) return;

            RecordingZone nearest = null;
            float best = float.MaxValue;
            var zones = RecordingZone.All;
            for (int i = 0; i < zones.Count; i++)
            {
                var zone = zones[i];
                if (zone == null || !zone.isActiveAndEnabled) continue;
                float sqr = zone.SqrDistanceTo(player.transform.position);
                if (sqr <= 3.2f * 3.2f && sqr < best)
                {
                    best = sqr;
                    nearest = zone;
                }
            }

            if (nearest == null) return;

            if (nearest.IsRecorded)
            {
                game.Notify("You already recorded this sound.");
                return;
            }

            if (!nearest.IsUnlocked)
            {
                game.Notify("The recorder stays quiet. Speak with Allal first.");
                return;
            }

            StartRecording(nearest);
        }

        public void StartRecording(RecordingZone zone)
        {
            var game = GameManager.Instance;
            if (game == null || zone == null) return;
            if (game.Mode != GameMode.Exploring) return;

            _zone = zone;
            _elapsed = 0f;
            _recording = true;
            _finishing = false;
            game.SetMode(GameMode.Recording);
            if (game.UI != null) game.UI.ShowRecording();
        }

        private void Complete()
        {
            if (!_recording || _finishing) return;
            _finishing = true;
            _recording = false;

            var game = GameManager.Instance;
            if (game == null) return;

            bool added = game.CollectMemory(MemoryIds.Souk);
            if (game.Audio != null) game.Audio.PlayRecordingComplete();

            string message = added
                ? "Saved: Voices of the medina."
                : "This sound is already in your journal.";
            var memory = game.Content != null ? game.Content.GetMemory(MemoryIds.Souk) : null;
            if (memory != null && memory.audio == null)
            {
                message += " (Silent placeholder audio.)";
            }

            if (game.UI != null) game.UI.ShowRecordingComplete(message);
            _finishRoutine = StartCoroutine(CloseAfter(1.7f));
        }

        private IEnumerator CloseAfter(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            CloseRecordingUI();
            var game = GameManager.Instance;
            if (game != null && game.Mode == GameMode.Recording) game.SetMode(GameMode.Exploring);
        }

        public void Cancel()
        {
            if (!_recording) return;
            if (_finishRoutine != null)
            {
                StopCoroutine(_finishRoutine);
                _finishRoutine = null;
            }
            _recording = false;
            _finishing = false;
            _zone = null;
            _elapsed = 0f;
            CloseRecordingUI();
            var game = GameManager.Instance;
            if (game != null)
            {
                game.SetMode(GameMode.Exploring);
                game.Notify("Recording cancelled. Nothing was saved.");
            }
        }

        private void CloseRecordingUI()
        {
            var game = GameManager.Instance;
            if (game != null && game.UI != null) game.UI.HideRecording();
        }
    }
}

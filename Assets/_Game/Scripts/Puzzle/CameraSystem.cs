using System.Collections;
using UnityEngine;

namespace LWalida
{
    public class CameraSystem : MonoBehaviour
    {
        private PhotoDoor _currentDoor;
        private Coroutine _closeRoutine;
        private bool _capturing;

        public PhotoDoor CurrentDoor => _currentDoor;

        private void Update()
        {
            var game = GameManager.Instance;
            if (game == null || game.Mode != GameMode.Exploring) return;
            if (!LWalidaInput.CameraPressed) return;

            var player = PlayerController.Instance;
            if (player == null) return;

            PhotoDoor nearest = FindNearest(player.transform.position, 4.5f);
            if (nearest == null)
            {
                game.Notify("Nothing to photograph here. Move closer to one of the wooden doors.", 4f);
                return;
            }

            Open(nearest);
        }

        private static PhotoDoor FindNearest(Vector3 position, float radius)
        {
            PhotoDoor best = null;
            float bestDistance = radius * radius;
            var doors = PhotoDoor.All;
            for (int i = 0; i < doors.Count; i++)
            {
                var door = doors[i];
                if (door == null || !door.isActiveAndEnabled) continue;
                float sqr = door.SqrDistanceTo(position);
                if (sqr < bestDistance)
                {
                    bestDistance = sqr;
                    best = door;
                }
            }
            return best;
        }

        public void Open(PhotoDoor door)
        {
            var game = GameManager.Instance;
            if (game == null || door == null) return;
            if (game.Mode != GameMode.Exploring) return;
            if (_closeRoutine != null)
            {
                StopCoroutine(_closeRoutine);
                _closeRoutine = null;
            }
            _currentDoor = door;
            _capturing = false;
            game.SetMode(GameMode.Camera);
            if (game.UI != null)
            {
                Sprite reference = game.Content != null ? game.Content.referencePhoto : null;
                game.UI.ShowCamera(door, reference);
            }
        }

        public void Close()
        {
            var game = GameManager.Instance;
            if (game == null) return;
            if (_closeRoutine != null)
            {
                StopCoroutine(_closeRoutine);
                _closeRoutine = null;
            }
            _capturing = false;
            _currentDoor = null;
            if (game.UI != null) game.UI.HideCamera();
            if (game.Mode == GameMode.Camera) game.SetMode(GameMode.Exploring);
        }

        public void Capture()
        {
            var game = GameManager.Instance;
            if (game == null || game.Mode != GameMode.Camera || _currentDoor == null || _capturing) return;

            if (game.Audio != null) game.Audio.PlayShutter();
            if (game.UI != null && game.UI.CameraUI != null) game.UI.CameraUI.Flash();

            if (_currentDoor.isCorrect)
            {
                bool already = game.HasMemory(MemoryIds.Door);
                if (already)
                {
                    game.UI.SetCameraFeedback("You already have this door in your journal.", true);
                    return;
                }

                game.CollectMemory(MemoryIds.Door);
                game.UI.SetCameraFeedback("The arch, the handles, the studs \u2014 it matches the photograph.", true);
                _capturing = true;
                _closeRoutine = StartCoroutine(CloseAfter(1.9f));
            }
            else
            {
                string hint = string.IsNullOrEmpty(_currentDoor.mismatchHint)
                    ? "The details don't quite match the photograph."
                    : "The details don't quite match the photograph. " + _currentDoor.mismatchHint;
                game.UI.SetCameraFeedback(hint, false);
            }
        }

        private IEnumerator CloseAfter(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            _capturing = false;
            Close();
        }
    }
}

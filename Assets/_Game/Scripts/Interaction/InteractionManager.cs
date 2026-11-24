using UnityEngine;

namespace LWalida
{
    public class InteractionManager : MonoBehaviour
    {
        private InteractableBase _nearest;
        private GameMode _mode = GameMode.Boot;

        public InteractableBase Nearest => _nearest;

        public void OnModeChanged(GameMode mode)
        {
            _mode = mode;
            if (mode != GameMode.Exploring)
            {
                _nearest = null;
                var hud = GameManager.Instance != null && GameManager.Instance.UI != null
                    ? GameManager.Instance.UI.HUD
                    : null;
                if (hud != null) hud.ClearPrompt();
            }
        }

        private void Update()
        {
            var game = GameManager.Instance;
            if (game == null) return;
            if (game.Mode != GameMode.Exploring)
            {
                if (_nearest != null)
                {
                    _nearest = null;
                    if (game.UI != null && game.UI.HUD != null) game.UI.HUD.ClearPrompt();
                }
                return;
            }

            var player = PlayerController.Instance;
            if (player == null) return;

            _nearest = FindNearest(player.transform.position);
            if (game.UI != null && game.UI.HUD != null)
            {
                if (_nearest != null) game.UI.HUD.SetPrompt(_nearest.GetPromptText());
                else game.UI.HUD.ClearPrompt();
            }

            if (LWalidaInput.InteractPressed && _nearest != null && _nearest.CanInteract)
            {
                _nearest.Interact();
            }
        }

        private InteractableBase FindNearest(Vector3 position)
        {
            InteractableBase best = null;
            float bestDistance = float.MaxValue;
            var registry = InteractableBase.Registry;
            for (int i = 0; i < registry.Count; i++)
            {
                var candidate = registry[i];
                if (candidate == null || !candidate.isActiveAndEnabled) continue;
                if (!candidate.showPromptWhenLocked && !candidate.CanInteract) continue;
                float radius = candidate.InteractionRadius;
                float sqr = candidate.SqrDistanceTo(position);
                if (sqr > radius * radius) continue;
                if (sqr < bestDistance)
                {
                    bestDistance = sqr;
                    best = candidate;
                }
            }
            return best;
        }
    }
}

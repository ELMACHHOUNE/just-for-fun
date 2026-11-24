using UnityEngine;

namespace LWalida
{
    public class ObjectiveManager : MonoBehaviour
    {
        private GameManager _game;

        public string CurrentObjective { get; private set; } = "";

        public void Initialize(GameManager game)
        {
            _game = game;
            Refresh();
        }

        public void Refresh()
        {
            if (_game == null) return;
            CurrentObjective = ProgressionRules.ObjectiveFor(_game.CurrentSceneId,
                _game.Memories != null ? _game.Memories.CollectedIds : null, _game.Flags);

            int count = _game.Memories != null ? _game.Memories.Count : 0;
            if (_game.UI != null && _game.UI.HUD != null)
            {
                _game.UI.HUD.SetObjective(CurrentObjective);
                _game.UI.HUD.SetMemoryCount(count, MemoryIds.RequiredCount);
            }
        }
    }
}

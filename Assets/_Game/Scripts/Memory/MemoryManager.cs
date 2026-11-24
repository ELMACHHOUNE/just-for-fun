using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWalida
{
    public class MemoryManager : MonoBehaviour
    {
        private readonly List<MemoryData> _catalog = new List<MemoryData>();
        private readonly HashSet<string> _collected = new HashSet<string>();

        public event Action<MemoryData> MemoryCollected;
        public event Action Changed;

        public IReadOnlyList<MemoryData> Catalog => _catalog;
        public int Count => _collected.Count;
        public int Total => MemoryIds.RequiredCount;
        public IReadOnlyCollection<string> CollectedIds => _collected;

        public void Initialize(IEnumerable<MemoryData> memories)
        {
            _catalog.Clear();
            if (memories != null)
            {
                foreach (var memory in memories)
                {
                    if (memory != null && memory.IsValid() && !_catalog.Exists(m => m.id == memory.id))
                    {
                        _catalog.Add(memory);
                    }
                }
            }

            _catalog.Sort((a, b) => a.order.CompareTo(b.order));
            Changed?.Invoke();
        }

        public MemoryData Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < _catalog.Count; i++)
            {
                if (_catalog[i].id == id) return _catalog[i];
            }
            return null;
        }

        public bool Has(string id)
        {
            return !string.IsNullOrEmpty(id) && _collected.Contains(id);
        }

        public bool TryCollect(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            if (_collected.Contains(id)) return false;
            if (!IsKnownMemory(id))
            {
                Debug.LogWarning("[LWalida] Tried to collect unknown memory id '" + id + "'.");
                return false;
            }

            _collected.Add(id);
            var data = Get(id);
            MemoryCollected?.Invoke(data);
            Changed?.Invoke();
            return true;
        }

        public void Restore(IEnumerable<string> ids)
        {
            _collected.Clear();
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    if (IsKnownMemory(id)) _collected.Add(id);
                }
            }
            Changed?.Invoke();
        }

        public void Clear()
        {
            _collected.Clear();
            Changed?.Invoke();
        }

        public void ResetAll()
        {
            Clear();
            _catalog.Clear();
        }

        private static bool IsKnownMemory(string id)
        {
            for (int i = 0; i < MemoryIds.All.Length; i++)
            {
                if (MemoryIds.All[i] == id) return true;
            }
            return false;
        }
    }
}

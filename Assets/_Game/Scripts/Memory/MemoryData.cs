using System;
using UnityEngine;

namespace LWalida
{
    [CreateAssetMenu(fileName = "Memory", menuName = "LWalida/Memory", order = 0)]
    public class MemoryData : ScriptableObject
    {
        public string id;
        public string title;
        [TextArea(2, 5)] public string description;
        public MemoryType type = MemoryType.Photograph;
        public Sprite image;
        public AudioClip audio;
        public bool audioIsPlaceholder;
        [Tooltip("Order used in the journal (0-3).")]
        public int order;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(title);
        }
    }
}

using UnityEngine;

namespace LWalida
{
    public static class Theme
    {
        public static readonly Color OudayaBlue = FromHex(0x27, 0x6B, 0x8C);
        public static readonly Color MidnightBlue = FromHex(0x18, 0x37, 0x44);
        public static readonly Color ChalkWhite = FromHex(0xF2, 0xED, 0xE3);
        public static readonly Color HassanSand = FromHex(0xC8, 0xA4, 0x77);
        public static readonly Color MemorySepia = FromHex(0xA8, 0x7C, 0x52);
        public static readonly Color OliveGreen = FromHex(0x66, 0x76, 0x5A);
        public static readonly Color Terracotta = FromHex(0xAB, 0x60, 0x48);
        public static readonly Color Charcoal = FromHex(0x25, 0x28, 0x27);

        public static readonly Color Panel = new Color(0.094f, 0.216f, 0.267f, 0.94f);
        public static readonly Color PanelSoft = new Color(0.094f, 0.216f, 0.267f, 0.82f);
        public static readonly Color Overlay = new Color(0.027f, 0.043f, 0.051f, 0.72f);
        public static readonly Color TextDim = new Color(0.949f, 0.929f, 0.890f, 0.72f);
        public static readonly Color Positive = new Color(0.65f, 0.85f, 0.62f);
        public static readonly Color Negative = new Color(0.93f, 0.62f, 0.50f);
        public static readonly Color Disabled = new Color(0.38f, 0.40f, 0.39f, 0.6f);

        private static Color FromHex(int r, int g, int b)
        {
            return new Color(r / 255f, g / 255f, b / 255f, 1f);
        }
    }
}

using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Texture Switcher", menuName = "Scene Switcher/Texture Asset")]
public sealed class SceneTextureAsset : ScriptableObject
{
    [Serializable]
    public sealed class TextureEntry
    {
        public Texture2D texture;

        public enum TextureSize
        {
            _32 = 32,
            _64 = 64,
            _128 = 128,
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096,
            _8192 = 8192,
            _16384 = 16384
        }

        public enum TextureCompression
        {
            None,
            LowQuality,
            NormalQuality,
            HighQuality
        }

        public TextureSize maxSize = TextureSize._4096;
        public TextureCompression compression = TextureCompression.NormalQuality;
    }

    [SerializeField] private TextureEntry[] textures = Array.Empty<TextureEntry>();

    public TextureEntry[] Textures => textures;
}
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;


public sealed class TextureResolutionApplier : MonoBehaviour
{
    [SerializeField] private SceneTextureAsset config;

    public void ApplyTextureResolution()
    {
        if (config == null || config.Textures == null || config.Textures.Length == 0)
            return;

        foreach (var entry in config.Textures)
        {
            if (entry == null || entry.texture == null)
                continue;

            var path = AssetDatabase.GetAssetPath(entry.texture);
            if (string.IsNullOrEmpty(path))
                continue;

            if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                continue;

            var changed = false;

            var newSize = (int)entry.maxSize;
            if (importer.maxTextureSize != newSize)
            {
                importer.maxTextureSize = newSize;
                changed = true;
            }

            var newCompression = ToImporterCompression(entry.compression);
            if (importer.textureCompression != newCompression)
            {
                importer.textureCompression = newCompression;
                changed = true;
            }

            // reimport only when importer settings actually changed
            if (changed)
                importer.SaveAndReimport();
        }
    }

    private static TextureImporterCompression ToImporterCompression(
        SceneTextureAsset.TextureEntry.TextureCompression compression)
    {
        return compression switch
        {
            SceneTextureAsset.TextureEntry.TextureCompression.None => TextureImporterCompression.Uncompressed,
            SceneTextureAsset.TextureEntry.TextureCompression.LowQuality => TextureImporterCompression.CompressedLQ,
            SceneTextureAsset.TextureEntry.TextureCompression.NormalQuality => TextureImporterCompression.Compressed,
            SceneTextureAsset.TextureEntry.TextureCompression.HighQuality => TextureImporterCompression.CompressedHQ,
            _ => TextureImporterCompression.Compressed
        };
    }
}

#endif
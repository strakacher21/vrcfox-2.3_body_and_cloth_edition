#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class TextureResolutionApplier : MonoBehaviour
{
    public SceneTextureAsset config;

    public void ApplyTextureResolution()
    {
        if (config == null) return;
        if (config.textures.Length == 0) return;

        foreach (SceneTextureAsset.TextureEntry entry in config.textures)
        {
            if (entry.texture == null) continue;

            string path = AssetDatabase.GetAssetPath(entry.texture);
            if (string.IsNullOrEmpty(path)) continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            bool changed = false;

            int newSize = (int)entry.maxSize;
            if (importer.maxTextureSize != newSize)
            {
                importer.maxTextureSize = newSize;
                changed = true;
            }

            TextureImporterCompression newCompression = ToImporterCompression(entry.compression);
            if (importer.textureCompression != newCompression)
            {
                importer.textureCompression = newCompression;
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    private static TextureImporterCompression ToImporterCompression(
        SceneTextureAsset.TextureEntry.TextureCompression compression)
    {
        switch (compression)
        {
            case SceneTextureAsset.TextureEntry.TextureCompression.None:
                return TextureImporterCompression.Uncompressed;
            case SceneTextureAsset.TextureEntry.TextureCompression.LowQuality:
                return TextureImporterCompression.CompressedLQ;
            case SceneTextureAsset.TextureEntry.TextureCompression.NormalQuality:
                return TextureImporterCompression.Compressed;
            case SceneTextureAsset.TextureEntry.TextureCompression.HighQuality:
                return TextureImporterCompression.CompressedHQ;
            default:
                return TextureImporterCompression.Compressed;
        }
    }
}
#endif
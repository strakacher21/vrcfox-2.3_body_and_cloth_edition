using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TextureResolutionApplier : MonoBehaviour
{
    public SceneTextureAsset config;

    public void ApplyTextureResolution()
    {
#if UNITY_EDITOR
        if (config == null) return;
        if (config.textures.Length == 0) return;

        foreach (SceneTextureAsset.TextureEntry entry in config.textures)
        {
            if (entry.texture == null) continue;

            string path = AssetDatabase.GetAssetPath(entry.texture);
            if (string.IsNullOrEmpty(path)) continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            int newSize = (int)entry.maxSize;
            if (importer.maxTextureSize != newSize)
            {
                importer.maxTextureSize = newSize;
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
#endif
    }
}
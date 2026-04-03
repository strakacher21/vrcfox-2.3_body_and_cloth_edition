#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class SceneLabel
{
    private const string PrefKey = "SceneLabel_SwitcherAssetPath";

    private static SceneSwitcherAsset _switcherAsset;
    private static bool _searchedOnce;
    private static GUIStyle _style;

    private static GUIStyle Style => _style ??= new GUIStyle
    {
        normal = { textColor = Color.yellow },
        fontSize = 50,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter
    };

    static SceneLabel()
    {
        SceneView.duringSceneGui += OnScene;
        EnsureSwitcherAssetLoaded();
    }

    private static void OnScene(SceneView sceneView)
    {
        EnsureSwitcherAssetLoaded();

        Handles.BeginGUI();

        float width = sceneView.camera.pixelWidth / EditorGUIUtility.pixelsPerPoint;
        float height = sceneView.camera.pixelHeight / EditorGUIUtility.pixelsPerPoint;

        GUI.Label(new Rect(0, 0, width, 100), EditorSceneManager.GetActiveScene().name, Style);

        const float LEFT_MARGIN = 10f;
        const float BOTTOM_MARGIN = 10f;
        const float BUTTON_WIDTH = 150f;
        const float BUTTON_HEIGHT = 30f;
        const float SPACING = 5f;

        if (_switcherAsset != null && _switcherAsset.Scenes != null && _switcherAsset.Scenes.Length > 0)
        {
            int sceneCount = _switcherAsset.Scenes.Length;
            float totalHeight = sceneCount * (BUTTON_HEIGHT + SPACING) - SPACING;
            float startY = height - BOTTOM_MARGIN - SPACING - totalHeight;

            for (int i = 0; i < sceneCount; i++)
            {
                var sceneAsset = _switcherAsset.Scenes[i];
                if (sceneAsset == null) continue;

                float btnY = startY + i * (BUTTON_HEIGHT + SPACING);

                if (GUI.Button(new Rect(LEFT_MARGIN, btnY, BUTTON_WIDTH, BUTTON_HEIGHT), sceneAsset.name))
                {
                    var activeScene = EditorSceneManager.GetActiveScene();
                    if (activeScene.isDirty)
                        EditorSceneManager.SaveScene(activeScene);

                    EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneAsset));

                    //run scene-dependent logic on the next editor tick after the new scene is fully opened
                    EditorApplication.delayCall += () =>
                    {
                        var applier = Object.FindAnyObjectByType<TextureResolutionApplier>();
                        applier?.ApplyTextureResolution();
                    };
                }
            }
        }

        Handles.EndGUI();
    }

    private static void EnsureSwitcherAssetLoaded()
    {
        if (_switcherAsset != null) return;
        if (_searchedOnce) return;

        string path = EditorPrefs.GetString(PrefKey, "");

        if (!string.IsNullOrEmpty(path))
            _switcherAsset = AssetDatabase.LoadAssetAtPath<SceneSwitcherAsset>(path);

        if (_switcherAsset == null)
        {
            EditorPrefs.DeleteKey(PrefKey);
            // avoid scanning the project on every sceneview repaint if the asset was not found
            string[] guids = AssetDatabase.FindAssets("t:SceneSwitcherAsset");
            _searchedOnce = true;
            if (guids == null || guids.Length == 0) return;

            path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _switcherAsset = AssetDatabase.LoadAssetAtPath<SceneSwitcherAsset>(path);
        }

        if (_switcherAsset != null)
            EditorPrefs.SetString(PrefKey, AssetDatabase.GetAssetPath(_switcherAsset));
    }
}

#endif
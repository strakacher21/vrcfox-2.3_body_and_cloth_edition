#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class SceneLabel
{
    private const string PrefKey = "SceneLabel_SwitcherAssetPath";
    private static GUIStyle style = new GUIStyle();
    private static SceneSwitcherAsset switcherAsset;

    static SceneLabel()
    {
        style.normal.textColor = Color.yellow;
        style.fontSize = 50;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;

        SceneView.duringSceneGui += OnScene;

        string savedPath = EditorPrefs.GetString(PrefKey, "");
        if (!string.IsNullOrEmpty(savedPath))
        {
            var asset = AssetDatabase.LoadAssetAtPath(savedPath, typeof(SceneSwitcherAsset));
            if (asset != null)
            {
                switcherAsset = asset as SceneSwitcherAsset;
            }
            else
            {
                EditorPrefs.DeleteKey(PrefKey);
            }
        }
    }

    private static void OnScene(SceneView sceneview)
    {
        EnsureSwitcherAssetLoaded();

        Handles.BeginGUI();

        float pixelWidth = sceneview.camera.pixelWidth;
        float pixelHeight = sceneview.camera.pixelHeight;
        float ppp = EditorGUIUtility.pixelsPerPoint;
        float width = pixelWidth / ppp;
        float height = pixelHeight / ppp;

        GUI.Label(new Rect(0, 0, width, 100), SceneManager.GetActiveScene().name, style);

        const float LEFT_MARGIN = 10f;
        const float BOTTOM_MARGIN = 10f;
        const float BUTTON_WIDTH = 150f;
        const float BUTTON_HEIGHT = 30f;
        const float SPACING = 5f;

        //float singleH = EditorGUIUtility.singleLineHeight;
        float y = height - BOTTOM_MARGIN;
        //y -= singleH;

        //Rect fieldRect = new Rect(LEFT_MARGIN, y, BUTTON_WIDTH, singleH);

        //EditorGUI.BeginChangeCheck();
        //var newAsset = (SceneSwitcherAsset)EditorGUI.ObjectField(
        //    fieldRect,
        //    switcherAsset,
        //    typeof(SceneSwitcherAsset),
        //    false
        //);

        //if (EditorGUI.EndChangeCheck())
        //{
        //    switcherAsset = newAsset;
        //    if (switcherAsset != null)
        //    {
        //        string path = AssetDatabase.GetAssetPath(switcherAsset);
        //        EditorPrefs.SetString(PrefKey, path);
        //    }
        //    else
        //    {
        //        EditorPrefs.DeleteKey(PrefKey);
        //    }
        //}

        if (switcherAsset != null && switcherAsset.scenes != null && switcherAsset.scenes.Length > 0)
        {
            int sceneCount = switcherAsset.scenes.Length;
            float totalHeight = sceneCount * (BUTTON_HEIGHT + SPACING) - SPACING;
            float startY = y - SPACING - totalHeight;
            //string activePath = EditorSceneManager.GetActiveScene().path;

            for (int i = 0; i < sceneCount; i++)
            {
                var sceneAsset = switcherAsset.scenes[i];
                if (sceneAsset == null) continue;

                float btnY = startY + i * (BUTTON_HEIGHT + SPACING);

                if (GUI.Button(new Rect(LEFT_MARGIN, btnY, BUTTON_WIDTH, BUTTON_HEIGHT), sceneAsset.name))
                {
                    var activeScene = EditorSceneManager.GetActiveScene();
                    if (activeScene.isDirty)
                    {
                        EditorSceneManager.SaveScene(activeScene);
                    }

                    string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                    EditorSceneManager.OpenScene(scenePath);

                    EditorApplication.delayCall += () =>
                    {
                        var applier = Object.FindObjectOfType<TextureResolutionApplier>();
                        if (applier != null)
                        {
                            applier.ApplyTextureResolution();
                        }
                    };
                }

                //Color originalBg = GUI.backgroundColor;
                //Color originalContent = GUI.contentColor;
                //bool isActive = AssetDatabase.GetAssetPath(sceneAsset) == activePath;
                //if (isActive)
                //{
                // GUI.backgroundColor = new Color(70f/255f, 96f/255f, 124f/255f);
                // GUI.contentColor = Color.white;
                //}

                //GUI.backgroundColor = originalBg;
                //GUI.contentColor = originalContent;
            }
        }

        Handles.EndGUI();
    }
    private static void EnsureSwitcherAssetLoaded()
    {
        if (switcherAsset != null) return;

        string path = EditorPrefs.GetString(PrefKey, "");

        if (!string.IsNullOrEmpty(path))
            switcherAsset = AssetDatabase.LoadAssetAtPath<SceneSwitcherAsset>(path);

        if (switcherAsset == null)
        {
            EditorPrefs.DeleteKey(PrefKey);

            string[] guids = AssetDatabase.FindAssets("t:SceneSwitcherAsset");
            if (guids == null || guids.Length == 0) return;

            path = AssetDatabase.GUIDToAssetPath(guids[0]);
            switcherAsset = AssetDatabase.LoadAssetAtPath<SceneSwitcherAsset>(path);
        }

        if (switcherAsset != null)
            EditorPrefs.SetString(PrefKey, AssetDatabase.GetAssetPath(switcherAsset));
    }
}
#endif
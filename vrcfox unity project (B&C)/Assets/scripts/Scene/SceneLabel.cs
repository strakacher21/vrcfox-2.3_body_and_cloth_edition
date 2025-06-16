#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class SceneLabel
{
    private static GUIStyle style = new GUIStyle();
    private static SceneSwitcherAsset switcherAsset;

    static SceneLabel()
    {
        style.normal.textColor = Color.yellow;
        style.fontSize = 50;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        SceneView.duringSceneGui += OnScene;

        string[] guids = AssetDatabase.FindAssets("t:SceneSwitcherAsset");
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        switcherAsset = AssetDatabase.LoadAssetAtPath<SceneSwitcherAsset>(path);

    }

    private static void OnScene(SceneView sceneview)
    {
        Handles.BeginGUI();
        float width = sceneview.camera.pixelWidth;
        float height = sceneview.camera.pixelHeight;


        GUI.Label(new Rect(0, 0, width, 100), SceneManager.GetActiveScene().name, style);

        if (switcherAsset != null && switcherAsset.scenes != null)
        {
            const float BUTTON_WIDTH = 150f;
            const float BUTTON_HEIGHT = 30f;
            const float LEFT_MARGIN = 10f;
            const float BOTTOM_MARGIN = 10f;
            const float SPACING = 5f;

            int sceneCount = switcherAsset.scenes.Length;
            string activeScenePath = EditorSceneManager.GetActiveScene().path;

            float startY = height - BOTTOM_MARGIN - sceneCount * (BUTTON_HEIGHT + SPACING) + SPACING;

            for (int i = 0; i < sceneCount; i++)
            {
                SceneAsset scene = switcherAsset.scenes[i];
                if (scene == null) continue;

                string scenePath = AssetDatabase.GetAssetPath(scene);
                bool isActive = scenePath == activeScenePath;

                float yPos = startY + i * (BUTTON_HEIGHT + SPACING);

                //Color originalBg = GUI.backgroundColor;
                //Color originalContent = GUI.contentColor;

                //if (isActive)
                //{
                //    GUI.backgroundColor = new Color(70f / 255f, 96f / 255f, 124f / 255f); // #46607c
                //    GUI.contentColor = Color.white;
                //}

                if (GUI.Button(new Rect(LEFT_MARGIN, yPos, BUTTON_WIDTH, BUTTON_HEIGHT), scene.name))
                {
                    EditorSceneManager.OpenScene(scenePath);
                }

                //GUI.backgroundColor = originalBg;
                //GUI.contentColor = originalContent;
            }
        }

        Handles.EndGUI();
    }
}
#endif

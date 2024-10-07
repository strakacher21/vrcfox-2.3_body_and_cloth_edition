#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class SceneLabel
{
	private static GUIStyle style= new GUIStyle();

	static SceneLabel()
	{
		style.normal.textColor = Color.yellow;
		style.fontSize = 50;
		style.fontStyle = FontStyle.Bold;
		style.alignment = TextAnchor.MiddleCenter;
		SceneView.duringSceneGui += OnScene;
	}

	private static void OnScene(SceneView sceneview)
	{
		Handles.BeginGUI();
		GUILayout.BeginArea(new Rect(0, 0, sceneview.camera.pixelWidth, 100));
		GUILayout.Label(SceneManager.GetActiveScene().name, style);
		GUILayout.EndArea();

		Handles.EndGUI();
	}
}
#endif
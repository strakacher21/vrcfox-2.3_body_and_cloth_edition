#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Scene Switcher", menuName = "Scene Switcher/Scene Asset")]
public class SceneSwitcherAsset : ScriptableObject
{
    public SceneAsset[] scenes;
}
#endif
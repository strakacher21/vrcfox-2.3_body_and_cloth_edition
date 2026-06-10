#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Scene Switcher", menuName = "Scene Switcher/Scene Asset")]
public sealed class SceneSwitcherAsset : ScriptableObject
{
    [SerializeField] private SceneAsset[] scenes;

    public SceneAsset[] Scenes => scenes;
}

#endif
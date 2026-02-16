#if UNITY_EDITOR

using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

public partial class AnimatorWizard : MonoBehaviour
{
    [Serializable]
    public struct ShapePreferenceEntry
    {
        public string blendShapeName;
        public bool useBool;
        public bool useFloat;
        [HideInInspector] public int lastMode; // 0 = Bool, 1 = Float
    }

    public bool createShapePreferences = true;

    //base prefix for generated VRC parameters (we append "bool/" or "float/" after this)
    public string shapePreferencePrefix = "pref/body/";

    // User-defined list: each entry maps a blendshape name to either bool-style or float-style preference
    public List<ShapePreferenceEntry> shapePreferences = new List<ShapePreferenceEntry>();

    private void InitializeShapePreferences(SkinnedMeshRenderer skin)
    {
        if (!createShapePreferences)
            return;

        if (skin == null) return;

        // Normalize prefix once so we can safely build parameter names
        var prefix = string.IsNullOrWhiteSpace(shapePreferencePrefix) ? "pref/body/" : shapePreferencePrefix.Trim();
        if (!prefix.EndsWith("/")) prefix += "/";

        // Toggle drivers (common to prefs and cloth)
        // this state transitions to itself every half second to update toggles. it sucks
        // TODO: not use this awful driver updating
        var fxDriverLayer = _aac.CreateSupportingFxLayer("preferences drivers").WithAvatarMask(fxMask);
        var fxDriverState = fxDriverLayer.NewState("preferences drivers");
        fxDriverState.TransitionsTo(fxDriverState)
            .AfterAnimationFinishes()
            .WithTransitionDurationSeconds(0.5f)
            .WithTransitionToSelf();

        var drivers = fxDriverState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
        drivers.parameters ??= new List<VRCAvatarParameterDriver.Parameter>();

        var tree = _masterTree.CreateBlendTreeChild(0);
        tree.name = "Shape Preferences";
        tree.blendType = BlendTreeType.Direct;

        // working with prefs blend shapes
        for (var i = 0; i < shapePreferences.Count; i++)
        {
            var entry = shapePreferences[i];
            var input = entry.blendShapeName?.Trim();
            if (string.IsNullOrWhiteSpace(input)) continue;

            // Accept both "SomeShape" and "pref/body/SomeShape"
            var shortName = input.StartsWith(prefix, StringComparison.Ordinal) ? input.Substring(prefix.Length) : input;
            shortName = shortName.TrimStart('/').Trim();
            if (string.IsNullOrWhiteSpace(shortName)) continue;

            // Full blendshape name on the mesh (includes prefix)
            var fullBlendShapeName = prefix + shortName;

            var boolParamName = $"{prefix}bool/{shortName}";
            var floatParamName = $"{prefix}float/{shortName}";

            if (entry.useFloat)
            {
                var param = CreateFloatParam(_fxTreeLayer, floatParamName, true, 0);
                tree.AddChild(BlendshapeTree(_fxTreeLayer, skin, fullBlendShapeName, param));
            }
            else if (entry.useBool)
            {
                // Bool mode: store a bool param, then copy it into a float param used by the blendshape animation
                var boolParam = CreateBoolParam(_fxTreeLayer, boolParamName, true, false);
                var floatParam = _fxTreeLayer.FloatParameter(floatParamName);

                drivers.parameters.Add(new VRCAvatarParameterDriver.Parameter
                {
                    type = VRCAvatarParameterDriver.ChangeType.Copy,
                    source = boolParam.Name,
                    name = floatParam.Name
                });

                tree.AddChild(BlendshapeTree(_fxTreeLayer, skin, fullBlendShapeName, floatParam));
            }
        }
    }

    [CustomPropertyDrawer(typeof(AnimatorWizard.ShapePreferenceEntry))]
    public class ShapePreferenceEntryDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var blendShapeName = property.FindPropertyRelative("blendShapeName");
            var useBool = property.FindPropertyRelative("useBool");
            var useFloat = property.FindPropertyRelative("useFloat");
            var lastMode = property.FindPropertyRelative("lastMode");

            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            const float spacing = 6f;
            const float toggleW = 54f;

            var nameRect = new Rect(position.x, position.y, position.width - (toggleW * 2 + spacing * 2), position.height);
            var boolRect = new Rect(nameRect.xMax + spacing, position.y, toggleW, position.height);
            var floatRect = new Rect(boolRect.xMax + spacing, position.y, toggleW, position.height);

            EditorGUI.PropertyField(nameRect, blendShapeName, GUIContent.none);

            // Make Bool/Float mutually exclusive
            EditorGUI.BeginChangeCheck();
            var newBool = EditorGUI.ToggleLeft(boolRect, "Bool", useBool.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                useBool.boolValue = newBool;
                if (newBool) useFloat.boolValue = false;
                lastMode.intValue = 0;
            }

            EditorGUI.BeginChangeCheck();
            var newFloat = EditorGUI.ToggleLeft(floatRect, "Float", useFloat.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                useFloat.boolValue = newFloat;
                if (newFloat) useBool.boolValue = false;
                lastMode.intValue = 1;
            }

            //safety for edge cases (multi-edit / serialized state)
            if (useBool.boolValue && useFloat.boolValue)
            {
                var keepFloat = lastMode.intValue == 1;
                useFloat.boolValue = keepFloat;
                useBool.boolValue = !keepFloat;
            }

            EditorGUI.EndProperty();
        }
    }
}

#endif
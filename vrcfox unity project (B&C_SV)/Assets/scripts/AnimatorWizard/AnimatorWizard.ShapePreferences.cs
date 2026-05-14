#if UNITY_EDITOR

using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRC;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using System;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

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

    private void InitializeShapePreferences(SkinnedMeshRenderer[] skins)
    {
        if (!createShapePreferences)
            return;

        if (skins == null || skins.Length == 0) return;

        // Normalize prefix once so we can safely build parameter names
        var prefix = string.IsNullOrWhiteSpace(shapePreferencePrefix) ? "pref/body/" : shapePreferencePrefix.Trim();
        if (!prefix.EndsWith("/")) prefix += "/";

        // Toggle drivers (common to prefs and cloth)
        // this state transitions to itself every half second to update toggles. it sucks
        // TODO: not use this awful driver updating
        var fxDriverLayer = _aac.CreateSupportingFxLayer("bool preferences drivers").WithAvatarMask(fxMask);
        var fxDriverState = fxDriverLayer.NewState("bool preferences drivers");
        fxDriverState.TransitionsTo(fxDriverState)
            .AfterAnimationFinishes()
            .WithTransitionDurationSeconds(0.5f)
            .WithTransitionToSelf();

        var tree = _masterTree.CreateBlendTreeChild(0);
        tree.name = "Shape Preferences";
        tree.blendType = BlendTreeType.Direct;

        // working with prefs blend shapes
        foreach (var entry in shapePreferences)
        {
            var input = entry.blendShapeName?.Trim();
            if (string.IsNullOrWhiteSpace(input)) continue;

            // Accept both "SomeShape" and "pref/body/SomeShape"
            var shortName = input.StartsWith(prefix, StringComparison.Ordinal) ? input.Substring(prefix.Length) : input;
            shortName = shortName.TrimStart('/').Trim();
            if (string.IsNullOrWhiteSpace(shortName)) continue;

            // Full blendshape name on the mesh (includes prefix)
            var fullBlendShapeName = prefix + shortName;

            if (!HasBlendShapeOnAnyMatchingMesh(skins, fullBlendShapeName))
                continue;

            var boolParamName = $"{prefix}bool/{shortName}";
            var floatParamName = $"{prefix}float/{shortName}";

            if (entry.useFloat)
            {
                var param = CreateFloatParam(_fxTreeLayer, floatParamName, true, 0);
                tree.AddChild(BuildBlendshapeTreeForSkins(fullBlendShapeName, param, skins));
                ApplyCompressedParams(floatParamName, false);
            }
            else if (entry.useBool)
            {
                // Bool mode: store a bool param, then copy it into a float param used by the blendshape animation
                var boolParam = CreateBoolParam(_fxTreeLayer, boolParamName, true, false);
                var floatParam = _fxTreeLayer.FloatParameter(floatParamName);

                fxDriverState.DrivingCopies(boolParam, floatParam);

                tree.AddChild(BuildBlendshapeTreeForSkins(fullBlendShapeName, floatParam, skins));
            }
        }
    }

    private BlendTree BuildBlendshapeTreeForSkins(string shapeName, AacFlParameter param, SkinnedMeshRenderer[] skins, float min = 0f, float max = 100f)
    {
        var state000 = _aac.NewClip();
        AddBlendShapeOnAllMatchingMeshes(state000, skins, shapeName, min);
        state000.Clip.name = $"{param.Name} 0";

        var state100 = _aac.NewClip();
        AddBlendShapeOnAllMatchingMeshes(state100, skins, shapeName, max);
        state100.Clip.name = $"{param.Name} 1";

        return Subtree(new Motion[] { state000.Clip, state100.Clip }, new[] { 0f, 1f }, param);
    }

}

#endif
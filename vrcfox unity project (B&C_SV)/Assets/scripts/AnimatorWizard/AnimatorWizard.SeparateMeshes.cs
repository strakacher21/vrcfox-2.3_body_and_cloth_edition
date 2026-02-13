#if UNITY_EDITOR

using System;
using System.Collections.Generic;

using AnimatorAsCode.V1;

using UnityEditor.Animations;
using UnityEngine;

using VRC.SDK3.Avatars.Components;

public partial class AnimatorWizard : MonoBehaviour
{
    public bool UseSeparateClothMeshes = false;

    private SkinnedMeshRenderer[] _allSkins = Array.Empty<SkinnedMeshRenderer>();
    private readonly Dictionary<string, SkinnedMeshRenderer[]> _skinsByBlendshape = new Dictionary<string, SkinnedMeshRenderer[]>(64);

    private void InitSeparateMeshesCache(VRCAvatarDescriptor avatar)
    {
        _skinsByBlendshape.Clear();

        if (!UseSeparateClothMeshes || avatar == null)
        {
            _allSkins = Array.Empty<SkinnedMeshRenderer>();
            return;
        }

        _allSkins = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true) ?? Array.Empty<SkinnedMeshRenderer>();
    }

    private SkinnedMeshRenderer[] GetSkinsWithBlendshape(string blendShapeName)
    {
        if (!UseSeparateClothMeshes) return Array.Empty<SkinnedMeshRenderer>();
        if (_allSkins == null || _allSkins.Length == 0) return Array.Empty<SkinnedMeshRenderer>();
        if (string.IsNullOrWhiteSpace(blendShapeName)) return Array.Empty<SkinnedMeshRenderer>();

        if (_skinsByBlendshape.TryGetValue(blendShapeName, out var cached)) return cached;

        var list = new List<SkinnedMeshRenderer>();
        foreach (var smr in _allSkins)
        {
            if (smr == null) continue;
            var mesh = smr.sharedMesh;
            if (mesh == null) continue;
            if (mesh.GetBlendShapeIndex(blendShapeName) >= 0) list.Add(smr);
        }

        var arr = list.ToArray();
        _skinsByBlendshape[blendShapeName] = arr;
        return arr;
    }

    protected BlendTree BlendshapeTree(
        AacFlLayer layer,
        SkinnedMeshRenderer[] skins,
        string shapeName,
        AacFlParameter param,
        float min = 0,
        float max = 100)
    {
        var state000 = _aac.NewClip().BlendShape(skins, shapeName, min);
        state000.Clip.name = param.Name + ":0";

        var state100 = _aac.NewClip().BlendShape(skins, shapeName, max);
        state100.Clip.name = param.Name + ":1";

        return Subtree(new Motion[] { state000.Clip, state100.Clip }, new[] { 0f, 1f }, param);
    }
}

#endif

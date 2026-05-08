#if UNITY_EDITOR

using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

public partial class AnimatorWizard : MonoBehaviour
{
    public bool createClothCustomization = true;
    public bool ClothSyncParamsOptimizedAlgorithm = true;
    public string ClothTogglesPrefix = "cloth/";

    public string[] ClothUpperBodyNames =
    {
        "coat",
        "coat_v2",
        "T-shirt",
    };

    public string[] ClothLowerBodyNames =
    {
        "jeans",
        "pants",
        "shorts",
    };

    public string[] ClothFootNames =
    {
        "shoes",
        "boots",
        "slaps",
    };

    private void InitializeClothingCustomization(SkinnedMeshRenderer[] skins)
    {
        if (!createClothCustomization || skins == null || skins.Length == 0)
            return;

        SetupClothes(ClothUpperBodyNames, skins, "cloth_upper_body", ClothSyncParamsOptimizedAlgorithm);
        SetupClothes(ClothLowerBodyNames, skins, "cloth_lower_body", ClothSyncParamsOptimizedAlgorithm);
        SetupClothes(ClothFootNames, skins, "cloth_foot", ClothSyncParamsOptimizedAlgorithm);
    }

    private void SetupClothes(string[] clothNames, SkinnedMeshRenderer[] skins, string layerName, bool ClothSyncParamsOptimizedAlgorithm)
    {
        if (clothNames == null || clothNames.Length == 0 || skins == null || skins.Length == 0)
            return;

        var layer = _aac.CreateSupportingFxLayer(layerName).WithAvatarMask(fxMask);
        var waitingState = layer.NewState("Waiting command");

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var allPossibleClothes = new List<string>(clothNames.Length);
        foreach (var name in clothNames)
        {
            if (string.IsNullOrWhiteSpace(name))
                continue;

            if (seen.Add(name))
                allPossibleClothes.Add(name);
        }

        VRCAvatarParameterDriver clothDriverSetsFalse = null;
        AacFlTransition waitingTransition = null;
        AacFlIntParameter intParam = null;
        var stateIndex = 1;

        if (ClothSyncParamsOptimizedAlgorithm)
        {
            waitingTransition = layer.AnyTransitionsTo(waitingState);
            clothDriverSetsFalse = waitingState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            if (clothDriverSetsFalse.parameters == null)
                clothDriverSetsFalse.parameters = new List<VRCAvatarParameterDriver.Parameter>();
        }
        else
        {
            var waitingClip = _aac.NewClip($"{layerName}_Waiting");
            foreach (var clothName in allPossibleClothes)
                AddBlendShapeOnAllMatchingMeshes(waitingClip, skins, ClothTogglesPrefix + clothName, 0f);

            waitingState.WithAnimation(waitingClip);

            intParam = CreateIntParam(layer, "cloth/" + layerName.Substring("cloth_".Length), true, 0);
            ApplyCompressedParams(intParam.Name, true);
            layer.AnyTransitionsTo(waitingState).When(intParam.IsEqualTo(0));
        }

        foreach (var clothName in allPossibleClothes)
        {
            var fullBlendShapeName = ClothTogglesPrefix + clothName;
            if (!HasBlendShapeOnAnyMatchingMesh(skins, fullBlendShapeName))
                continue;

            var clothClip = _aac.NewClip($"Cloth_{clothName}");
            AddBlendShapeOnAllMatchingMeshes(clothClip, skins, fullBlendShapeName, 100f);

            foreach (var otherClothName in allPossibleClothes)
            {
                if (otherClothName == clothName)
                    continue;

                var otherFullBlendShapeName = ClothTogglesPrefix + otherClothName;
                AddBlendShapeOnAllMatchingMeshes(clothClip, skins, otherFullBlendShapeName, 0f);
            }

            var clothState = layer.NewState(clothName);
            clothState.WithAnimation(clothClip);

            if (ClothSyncParamsOptimizedAlgorithm)
            {
                var clothDriverSetsTrue = clothState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                if (clothDriverSetsTrue.parameters == null)
                    clothDriverSetsTrue.parameters = new List<VRCAvatarParameterDriver.Parameter>();

                var boolParam = CreateBoolParam(layer, fullBlendShapeName, true, false);

                clothDriverSetsFalse.parameters.Add(new VRCAvatarParameterDriver.Parameter
                {
                    name = fullBlendShapeName,
                    type = VRCAvatarParameterDriver.ChangeType.Set,
                    value = 0
                });

                clothDriverSetsTrue.parameters.Add(new VRCAvatarParameterDriver.Parameter
                {
                    name = fullBlendShapeName,
                    type = VRCAvatarParameterDriver.ChangeType.Set,
                    value = 1
                });

                foreach (var otherClothName in allPossibleClothes)
                {
                    if (otherClothName == clothName)
                        continue;

                    clothDriverSetsTrue.parameters.Add(new VRCAvatarParameterDriver.Parameter
                    {
                        name = ClothTogglesPrefix + otherClothName,
                        type = VRCAvatarParameterDriver.ChangeType.Set,
                        value = 0
                    });
                }

                waitingTransition.When(boolParam.IsFalse());
                layer.AnyTransitionsTo(clothState).When(boolParam.IsTrue());
            }
            else
            {
                layer.AnyTransitionsTo(clothState).When(intParam.IsEqualTo(stateIndex));
                stateIndex++;
            }
        }
    }
}

#endif
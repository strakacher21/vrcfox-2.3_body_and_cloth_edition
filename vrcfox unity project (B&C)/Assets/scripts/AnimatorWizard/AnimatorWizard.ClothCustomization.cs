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
    public string ClothTogglesPrefix = "cloth/toggle/";

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

        setupClothes(ClothUpperBodyNames, skins, "cloth_upper_body");
        setupClothes(ClothLowerBodyNames, skins, "cloth_lower_body");
        setupClothes(ClothFootNames, skins, "cloth_foot");
    }

    private void setupClothes(string[] clothNames, SkinnedMeshRenderer[] skins, string layerName)
    {
        if (clothNames == null || clothNames.Length == 0 || skins == null || skins.Length == 0)
            return;

        var layer = _aac.CreateSupportingFxLayer(layerName).WithAvatarMask(fxMask);

        var waitingState = layer.NewState("Waiting command");
        var waitingTransition = layer.AnyTransitionsTo(waitingState);

        var clothDriverSetsFalse = waitingState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
        if (clothDriverSetsFalse.parameters == null)
            clothDriverSetsFalse.parameters = new List<VRCAvatarParameterDriver.Parameter>();

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var allPossibleClothes = new List<string>(clothNames.Length);
        foreach (var name in clothNames)
        {
            if (string.IsNullOrWhiteSpace(name))
                continue;

            if (seen.Add(name))
                allPossibleClothes.Add(name);
        }

        foreach (var clothName in allPossibleClothes)
        {
            var fullBlendShapeName = ClothTogglesPrefix + clothName;
            var clothClip = _aac.NewClip($"Cloth_{clothName}");

            if (!HasBlendShapeOnAnyMatchingMesh(skins, fullBlendShapeName))
                continue;

            AddBlendShapeOnAllMatchingMeshes(clothClip, skins, fullBlendShapeName, 100f);

            var clothState = layer.NewState(clothName);
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

                var otherFullBlendShapeName = ClothTogglesPrefix + otherClothName;

                clothDriverSetsTrue.parameters.Add(new VRCAvatarParameterDriver.Parameter
                {
                    name = otherFullBlendShapeName,
                    type = VRCAvatarParameterDriver.ChangeType.Set,
                    value = 0
                });

                AddBlendShapeOnAllMatchingMeshes(clothClip, skins, otherFullBlendShapeName, 0f);
            }

            clothState.WithAnimation(clothClip);

            waitingTransition.When(boolParam.IsFalse());
            layer.AnyTransitionsTo(clothState).When(boolParam.IsTrue());
        }
    }
}

#endif
#if UNITY_EDITOR

using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

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
        var layer = _aac.CreateSupportingFxLayer(layerName).WithAvatarMask(fxMask);

        var waitingState = layer.NewState("Waiting command");
        var waitingTransition = layer.AnyTransitionsTo(waitingState);
        var clothDriverSetsFalse = waitingState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
        if (clothDriverSetsFalse.parameters == null)
            clothDriverSetsFalse.parameters = new List<VRCAvatarParameterDriver.Parameter>();

        var clothStates = new Dictionary<string, AacFlState>();

        List<string> allPossibleClothes = new List<string>();
        foreach (var clothName in clothNames)
        {
            if (!allPossibleClothes.Contains(clothName))
                allPossibleClothes.Add(clothName);
        }

        foreach (var clothName in allPossibleClothes)
        {
            string fullBlendShapeName = ClothTogglesPrefix + clothName;
            var clothClip = _aac.NewClip($"Cloth_{clothName}");

            bool foundMainShape = AddClothBlendShapeOnAllMatchingMeshes(clothClip, skins, fullBlendShapeName, 100f);
            if (!foundMainShape)
                continue;

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

                string otherFullBlendShapeName = ClothTogglesPrefix + otherClothName;

                clothDriverSetsTrue.parameters.Add(new VRCAvatarParameterDriver.Parameter
                {
                    name = otherFullBlendShapeName,
                    type = VRCAvatarParameterDriver.ChangeType.Set,
                    value = 0
                });

                AddClothBlendShapeOnAllMatchingMeshes(clothClip, skins, otherFullBlendShapeName, 0f);
            }

            clothState.WithAnimation(clothClip);
            clothStates[clothName] = clothState;

            waitingTransition.When(boolParam.IsFalse());
            layer.AnyTransitionsTo(clothState).When(boolParam.IsTrue());
        }
    }

    private bool AddClothBlendShapeOnAllMatchingMeshes(AacFlClip clip, SkinnedMeshRenderer[] skins, string blendShapeName, float value)
    {
        bool found = false;

        foreach (var skin in skins)
        {
            if (skin == null || skin.sharedMesh == null)
                continue;

            int blendShapeCount = skin.sharedMesh.blendShapeCount;
            for (int i = 0; i < blendShapeCount; i++)
            {
                if (!skin.sharedMesh.GetBlendShapeName(i).Equals(blendShapeName))
                    continue;

                clip.BlendShape(skin, blendShapeName, value);
                found = true;
                break;
            }
        }

        return found;
    }
}

#endif
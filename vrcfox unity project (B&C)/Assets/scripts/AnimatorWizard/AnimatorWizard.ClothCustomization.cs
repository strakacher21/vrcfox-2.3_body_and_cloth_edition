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

    private void InitializeClothingCustomization(SkinnedMeshRenderer skin)
    {
        if (!createClothCustomization)
            return;

        setupClothes(ClothUpperBodyNames, skin, "cloth_upper_body");
        setupClothes(ClothLowerBodyNames, skin, "cloth_lower_body");
        setupClothes(ClothFootNames, skin, "cloth_foot");
    }

    private void setupClothes(string[] clothNames, SkinnedMeshRenderer skin, string layerName)
    {
        var layer = _aac.CreateSupportingFxLayer(layerName).WithAvatarMask(fxMask);

        var waitingState = layer.NewState("Waiting command");
        var waitingTransition = layer.AnyTransitionsTo(waitingState);
        var ClothDriverSetsFalse = waitingState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();

        var clothStates = new Dictionary<string, AacFlState>();

        int blendShapeCount = skin.sharedMesh.blendShapeCount;

        List<string> allPossibleClothes = new List<string>();
        foreach (var clothName in clothNames)
            if (!allPossibleClothes.Contains(clothName))
                allPossibleClothes.Add(clothName);

        foreach (var clothName in allPossibleClothes)
        {
            var clothState = layer.NewState(clothName);
            var ClothDriverSetsTrue = clothState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            var ClothClip = _aac.NewClip($"Cloth_{clothName}");

            AacFlBoolParameter boolParam = null;

            for (int i = 0; i < blendShapeCount; i++)
            {
                string blendShapeName = skin.sharedMesh.GetBlendShapeName(i);

                if (blendShapeName.Equals(ClothTogglesPrefix + clothName))
                {
                    boolParam = CreateBoolParam(layer, ClothTogglesPrefix + clothName, true, false);

                    ClothDriverSetsFalse.parameters.Add(new VRCAvatarParameterDriver.Parameter
                    {
                        name = ClothTogglesPrefix + clothName,
                        type = VRCAvatarParameterDriver.ChangeType.Set,
                        value = 0
                    });

                    ClothDriverSetsTrue.parameters.Add(new VRCAvatarParameterDriver.Parameter
                    {
                        name = ClothTogglesPrefix + clothName,
                        type = VRCAvatarParameterDriver.ChangeType.Set,
                        value = 1
                    });

                    ClothClip.BlendShape(skin, blendShapeName, 100);
                }
            }

            if (boolParam != null)
            {
                foreach (var otherClothName in allPossibleClothes)
                {
                    if (otherClothName != clothName)
                    {
                        ClothDriverSetsTrue.parameters.Add(new VRCAvatarParameterDriver.Parameter
                        {
                            name = ClothTogglesPrefix + otherClothName,
                            type = VRCAvatarParameterDriver.ChangeType.Set,
                            value = 0
                        });

                        for (int i = 0; i < blendShapeCount; i++)
                        {
                            string blendShapeName = skin.sharedMesh.GetBlendShapeName(i);

                            if (blendShapeName.Equals(ClothTogglesPrefix + otherClothName))
                                ClothClip.BlendShape(skin, blendShapeName, 0);
                        }
                    }
                }
            }

            clothState.WithAnimation(ClothClip);
            clothStates[clothName] = clothState;

            waitingTransition.When(layer.BoolParameter(ClothTogglesPrefix + clothName).IsFalse());
            layer.AnyTransitionsTo(clothState).When(layer.BoolParameter(ClothTogglesPrefix + clothName).IsTrue());
        }
    }
}

#endif
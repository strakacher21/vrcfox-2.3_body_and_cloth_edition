#if UNITY_EDITOR

using AnimatorAsCode.V1.VRCDestructiveWorkflow;

using System.Collections.Generic;

using UnityEditor.Animations;
using UnityEngine;

using VRC.SDK3.Avatars.Components;

public partial class AnimatorWizard : MonoBehaviour
{
    public bool createShapePreferences = true;

    private void InitializeShapePreferences(SkinnedMeshRenderer skin)
    {
        if (!createShapePreferences) return;

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

        for (var i = 0; i < skin.sharedMesh.blendShapeCount; i++)
        {
            string blendShapeName = skin.sharedMesh.GetBlendShapeName(i);

            if (blendShapeName.StartsWith(shapePreferenceSliderPrefix))
            {
                var param = CreateFloatParam(_fxTreeLayer, blendShapeName, true, 0);

                var targets = GetSkinsWithBlendshape(blendShapeName);
                tree.AddChild(targets.Length > 0
                    ? BlendshapeTree(_fxTreeLayer, targets, blendShapeName, param)
                    : BlendshapeTree(_fxTreeLayer, skin, param));
            }
            else if (blendShapeName.StartsWith(shapePreferenceTogglesPrefix))
            {
                var boolParam = CreateBoolParam(_fxTreeLayer, blendShapeName, true, false);
                var floatParam = _fxTreeLayer.FloatParameter(blendShapeName + "-float");

                drivers.parameters.Add(new VRCAvatarParameterDriver.Parameter
                {
                    type = VRCAvatarParameterDriver.ChangeType.Copy,
                    source = boolParam.Name,
                    name = floatParam.Name
                });

                var targets = GetSkinsWithBlendshape(blendShapeName);
                tree.AddChild(targets.Length > 0
                    ? BlendshapeTree(_fxTreeLayer, targets, blendShapeName, floatParam)
                    : BlendshapeTree(_fxTreeLayer, skin, blendShapeName, floatParam));
            }
        }
    }
}

#endif

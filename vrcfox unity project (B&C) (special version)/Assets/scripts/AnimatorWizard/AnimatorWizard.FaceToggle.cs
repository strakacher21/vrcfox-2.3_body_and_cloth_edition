#if UNITY_EDITOR

using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRC;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using System;
using System.Collections.Generic;
using UnityEngine;

public partial class AnimatorWizard : MonoBehaviour
{
    public bool createFaceToggle = true;
    public Motion[] FaceToggleNames;
    public string[] FaceToggleBlockParamNames =
    {
        "FaceToggleActive",
        "contact/confuse",
        "AFK"
    };
    private void InitializeFaceToggle()
    {
        if (!createFaceToggle)
            return;

        var FaceToggleLayer = _aac.CreateSupportingFxLayer("Face Toggle").WithAvatarMask(fxMask);

        var customFaceToggleBlocksNames = BuildBlockBoolListParams(FaceToggleLayer, FaceToggleBlockParamNames);

        var FaceToggleActive = FaceToggleLayer.BoolParameter("FaceToggleActive");
        var FaceToggleActiveParam = CreateIntParam(FaceToggleLayer, FullFaceTrackingPrefix + "anim/FacePresets", false, 0);

        var ftActiveParam = FaceToggleLayer.BoolParameter(FullFaceTrackingPrefix + "LipTrackingActive");

        var FaceToggleWaitingState = FaceToggleLayer.NewState("Waiting command")
            .Drives(FaceToggleActive, false);

        var waitingTransition = FaceToggleLayer.AnyTransitionsTo(FaceToggleWaitingState)
            .WithTransitionDurationSeconds(0.25f)
            .When(FaceToggleActiveParam.IsEqualTo(0));

        if (createFaceTracking)
            waitingTransition.Or().When(ftActiveParam.IsTrue());

        if (customFaceToggleBlocksNames != null && customFaceToggleBlocksNames.Count > 0)
            foreach (var block in customFaceToggleBlocksNames)
            {
                if (block == null) continue;
                waitingTransition.Or().When(block.IsTrue());
            }

        if (FaceToggleNames == null)
            throw new Exception("FaceToggleNames array is not assigned.");

        for (int i = 0; i < FaceToggleNames.Length; i++)
        {
            setupFaceToggle(FaceToggleLayer, FaceToggleNames[i], ftActiveParam, FaceToggleActiveParam, FaceToggleActive, i, customFaceToggleBlocksNames);
        }
    }

    private void setupFaceToggle(
        AacFlLayer FaceToggleLayer,
        Motion motion,
        AacFlBoolParameter ftActiveParam,
        AacFlIntParameter FaceToggleActiveParam,
        AacFlBoolParameter FaceToggleActive,
        int index,
        List<AacFlBoolParameter> customFaceToggleBlocksNames)
    {
        if (motion == null)
            throw new Exception($"Face toggle motion at index {index} is missing!");

        var faceToggleState = FaceToggleLayer.NewState(motion.name)
            .Drives(FaceToggleActive, true)
            .WithAnimation(motion);

        var tr = FaceToggleLayer.AnyTransitionsTo(faceToggleState)
            .WithTransitionDurationSeconds(0.25f)
            .When(FaceToggleActiveParam.IsEqualTo(index + 1));

        if (createFaceTracking)
            tr.And(ftActiveParam.IsFalse());

        if (customFaceToggleBlocksNames != null && customFaceToggleBlocksNames.Count > 0)
            foreach (var block in customFaceToggleBlocksNames)
            {
                if (block == null) continue;
                tr.And(block.IsFalse());
            }
    }
}

#endif
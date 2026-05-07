#if UNITY_EDITOR

using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using System;
using System.Collections.Generic;
using UnityEngine;

public partial class AnimatorWizard : MonoBehaviour
{

    public string[] GestureExpressionsBlockParamNames =
    {
        "FaceToggleActive",
        "contact/confuse",
        "AFK"
    };

    public bool createFacialExpressionsControl = false;
    public string expTrackName = "ExpressionTrackingActive";

    public string mouthPrefix = "exp/mouth/";
    public string[] mouthShapeNames =
    {
        "basis",
        "frown",
        "smile",
        "grimace",
        "smile",
        "grimace",
        "grimace",
        "frown",
    };

    public string browPrefix = "exp/brows/";
    public string[] browShapeNames =
    {
        "basis",
        "down",
        "up",
        "curious",
        "up",
        "worried",
        "curious",
        "down",
    };

    protected void InitializeGestureExpressions(
        SkinnedMeshRenderer[] skins
        )
    {
        if (skins == null || skins.Length == 0)
            return;

        // brow Gesture expressions
        MapHandPosesToShapes("brow expressions", skins, browShapeNames, browPrefix, false, GestureExpressionsBlockParamNames);

        // mouth Gesture expressions
        MapHandPosesToShapes("mouth expressions", skins, mouthShapeNames, mouthPrefix, true, GestureExpressionsBlockParamNames);
    }

    private void MapHandPosesToShapes(
        string layerName,
        SkinnedMeshRenderer[] skins,
        string[] shapeNames,
        string prefix,
        bool rightHand,
        IEnumerable<string> blockNames
        )
    {
        var layer = _aac.CreateSupportingFxLayer(layerName).WithAvatarMask(fxMask);
        var customGestureBlocksNames = BuildBlockBoolListParams(layer, blockNames);
        var Gesture = layer.IntParameter("Gesture" + (rightHand ? Right : Left));

        AacFlBoolParameter ftActiveParam = null;
        if (createFaceTracking)
        {
            ftActiveParam = layer.BoolParameter(FullFaceTrackingPrefix + "LipTrackingActive");
        }

        if (shapeNames.Length != 8)
            throw new Exception("Number of face poses must equal number of hand gestures (8)!");

        for (int i = 0; i < shapeNames.Length; i++)
        {
            var clip = _aac.NewClip();

            foreach (var name in shapeNames)
            {
                AddBlendShapeOnAllMatchingMeshes(clip, skins, prefix + name, 0f);
                AddBlendShapeOnAllMatchingMeshes(clip, skins, prefix + shapeNames[i], 100f);
            }

            var state = layer.NewState(shapeNames[i], 1, i).WithAnimation(clip);

            var enter = layer.EntryTransitionsTo(state).When(Gesture.IsEqualTo(i));
            var exit = state.Exits()
                .WithTransitionDurationSeconds(TransitionSpeed)
                .When(Gesture.IsNotEqualTo(i));

            if (ftActiveParam != null)
            {
                if (i == 0)
                {
                    enter.Or().When(ftActiveParam.IsTrue());
                    exit.And(ftActiveParam.IsFalse());
                }
                else
                {
                    enter.And(ftActiveParam.IsFalse());
                    exit.Or().When(ftActiveParam.IsTrue());
                }
            }

            if (customGestureBlocksNames != null && customGestureBlocksNames.Count > 0)
            {
                foreach (var block in customGestureBlocksNames)
                {
                    if (block == null) continue;

                    if (i == 0) { enter.Or().When(block.IsTrue()); exit.And(block.IsFalse()); }
                    else { enter.And(block.IsFalse()); exit.Or().When(block.IsTrue()); }
                }
            }
        }
    }
}

#endif
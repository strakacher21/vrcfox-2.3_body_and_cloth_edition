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
        SkinnedMeshRenderer[] skins,
        AacFlBoolParameter ftActiveParam
        )
    {
        if (skins == null || skins.Length == 0)
            return;

        // brow Gesture expressions
        MapHandPosesToShapes("brow expressions", skins, browShapeNames, browPrefix, false, ftActiveParam, GestureExpressionsBlockParamNames);

        // mouth Gesture expressions
        MapHandPosesToShapes("mouth expressions", skins, mouthShapeNames, mouthPrefix, true, ftActiveParam, GestureExpressionsBlockParamNames);
    }

    private void MapHandPosesToShapes(
        string layerName,
        SkinnedMeshRenderer[] skins,
        string[] shapeNames,
        string prefix,
        bool rightHand,
        AacFlBoolParameter ftActiveParam,
        IEnumerable<string> blockNames
        )
    {
        var layer = _aac.CreateSupportingFxLayer(layerName).WithAvatarMask(fxMask);
        var customGestureBlocksNames = BuildBlockBoolListParams(layer, blockNames);
        var Gesture = layer.IntParameter("Gesture" + (rightHand ? Right : Left));

        List<string> allExpressions = new List<string>();
        foreach (var shapeName in shapeNames)
        {
            if (!allExpressions.Contains(shapeName))
                allExpressions.Add(shapeName);
        }

        if (shapeNames.Length != 8)
            throw new Exception("Number of face poses must equal number of hand gestures (8)!");

        for (int i = 0; i < shapeNames.Length; i++)
        {
            var clip = _aac.NewClip();

            foreach (var shapeName in shapeNames)
            {
                AddGestureBlendShapeOnAllMatchingMeshes(clip, skins, prefix + shapeName, shapeName == shapeNames[i] ? 100 : 0);
            }

            var state = layer.NewState(shapeNames[i], 1, i).WithAnimation(clip);

            var enter = layer.EntryTransitionsTo(state).When(Gesture.IsEqualTo(i));
            var exit = state.Exits()
                .WithTransitionDurationSeconds(TransitionSpeed)
                .When(Gesture.IsNotEqualTo(i));

            if (createFaceTracking)
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

    private void AddGestureBlendShapeOnAllMatchingMeshes(AacFlClip clip, SkinnedMeshRenderer[] skins, string blendShapeName, float value)
    {
        foreach (var skin in skins)
        {
            if (skin == null || skin.sharedMesh == null)
                continue;

            if (skin.sharedMesh.GetBlendShapeIndex(blendShapeName) < 0)
                continue;

            clip.BlendShape(skin, blendShapeName, value);
        }
    }
}

#endif
#if UNITY_EDITOR
using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRC;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using System;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

[Serializable]
public struct SingleFtShape
{
    public string shapeName;
    public bool leftAndRightShapes;

    public SingleFtShape(string shapeName, bool leftAndRightShapes = false)
    {
        this.shapeName = shapeName;
        this.leftAndRightShapes = leftAndRightShapes;
    }
}

[Serializable]
public struct DualFtShape
{
    public string paramName;
    public string minShapeName;
    public string maxShapeName;
    public float minValue;
    public float neutralValue;
    public float maxValue;
    public bool leftAndRightShapes;

    public DualFtShape(string paramName, string minShapeName, string maxShapeName, float minValue, float neutralValue, float maxValue, bool leftAndRightShapes = false)
    {
        this.paramName = paramName;
        this.minShapeName = minShapeName;
        this.maxShapeName = maxShapeName;
        this.minValue = minValue;
        this.neutralValue = neutralValue;
        this.maxValue = maxValue;
        this.leftAndRightShapes = leftAndRightShapes;
    }

    public DualFtShape(string paramName, string minShapeName, string maxShapeName, bool leftAndRightShapes = false)
    {
        this.paramName = paramName;
        this.minShapeName = minShapeName;
        this.maxShapeName = maxShapeName;
        minValue = -1;
        neutralValue = 0;
        maxValue = 1;
        this.leftAndRightShapes = leftAndRightShapes;
    }

}

public partial class AnimatorWizard : MonoBehaviour
{
    public bool createFaceTracking = true;
    public bool MirrorFTparams = false;
    public bool createFTLipSyncControl = false;
    public string lipSyncName = "LipSyncTrackingActive";

    public string[] FaceTrackingBlockParamNames =
    {
        "AFK"
    };

    public SingleFtShape[] SingleFtShapes = new[]
    {
        new SingleFtShape("JawOpen"),
        new SingleFtShape("LipFunnel"),
        new SingleFtShape("LipPucker"),
        new SingleFtShape("MouthClosed"),
        new SingleFtShape("MouthStretch"),
        new SingleFtShape("MouthUpperUpLeft"),
        new SingleFtShape("MouthLowerDownLeft"),
        new SingleFtShape("MouthRaiserLower"),
        new SingleFtShape("TongueOut"),
        new SingleFtShape("EyeSquintLeft"),
    };

    public DualFtShape[] DualFtShapes = new[]
    {
        new DualFtShape("SmileSad", "MouthSad", "MouthSmile"),
        new DualFtShape("JawX", "JawLeft", "JawRight"),
        new DualFtShape("JawZ", "JawBackward", "JawForward"),
        new DualFtShape("MouthX", "MouthLeft", "MouthRight"),
        new DualFtShape("EyeLidLeft", "EyeClosedLeft", "EyeWideLeft", 0, 0.75f, 1),
        new DualFtShape("BrowExpressionLeft", "BrowDown", "BrowUp"),
        new DualFtShape("CheekPuffSuck", "CheekSuck", "CheekPuff"),
    };

    private void InitializeFaceTracking(SkinnedMeshRenderer skin, VRCAvatarDescriptor avatar)
    {
        if (!createFaceTracking)
            return;

        var layer = _aac.CreateSupportingFxLayer("face tracking toggle").WithAvatarMask(fxMask);
        var customFaceTrackingBlocksNames = BuildBlockBoolListParams(layer, FaceTrackingBlockParamNames);
        var ftActiveParam = CreateBoolParam(layer, FullFaceTrackingPrefix + "LipTrackingActive", true, false);
        var ftBlendParam = layer.FloatParameter(FullFaceTrackingPrefix + "LipTrackingActive-float");

        // States with Lip Sync Control
        if (createFTLipSyncControl)
        {
            AacFlBoolParameter lipSyncActiveParam;
            if (createFTLipSyncControl)
                lipSyncActiveParam = CreateBoolParam(layer, FullFaceTrackingPrefix + lipSyncName, true, false);
            else
                lipSyncActiveParam = layer.BoolParameter(FullFaceTrackingPrefix + lipSyncName);

            var offFaceTrackingLipSyncTrackingAnimatesState = layer.NewState("face tracking off")
            .Drives(ftBlendParam, 0)
            .TrackingAnimates(AacAv3.Av3TrackingElement.Mouth);

            var onFaceTrackingLipSyncTrackingAnimatesState = layer.NewState("face tracking on")
                .Drives(ftBlendParam, 1)
                .TrackingAnimates(AacAv3.Av3TrackingElement.Mouth);

            var offFaceTrackingLipSyncTrackingTracksState = layer.NewState("face tracking off (LipSync Enabled)")
                .Drives(ftBlendParam, 0)
                .TrackingTracks(AacAv3.Av3TrackingElement.Mouth);

            var onFaceTrackingLipSyncTrackingTracksState = layer.NewState("face tracking on (LipSync Enabled)")
                .Drives(ftBlendParam, 1)
                .TrackingTracks(AacAv3.Av3TrackingElement.Mouth);

            // Transitions
            var offLipSyncTracksTransition = layer.AnyTransitionsTo(offFaceTrackingLipSyncTrackingTracksState)
                .When(lipSyncActiveParam.IsTrue())
                .And(ftActiveParam.IsFalse());

            var onLipSyncTracksTransition = layer.AnyTransitionsTo(onFaceTrackingLipSyncTrackingTracksState)
                //.WithTransitionToSelf()
                .When(lipSyncActiveParam.IsTrue())
                .And(ftActiveParam.IsTrue());

            var offLipSyncAnimatesTransition = layer.AnyTransitionsTo(offFaceTrackingLipSyncTrackingAnimatesState)
                .When(lipSyncActiveParam.IsFalse())
                .And(ftActiveParam.IsFalse());

            var onLipSyncAnimatesTransition = layer.AnyTransitionsTo(onFaceTrackingLipSyncTrackingAnimatesState)
                //.WithTransitionToSelf()
                .When(lipSyncActiveParam.IsFalse())
                .And(ftActiveParam.IsTrue());

            if (customFaceTrackingBlocksNames != null && customFaceTrackingBlocksNames.Count > 0)
                foreach (var block in customFaceTrackingBlocksNames)
                {
                    if (block == null) continue;

                    offLipSyncTracksTransition
                        .Or().When(lipSyncActiveParam.IsTrue()).And(block.IsTrue());

                    offLipSyncAnimatesTransition
                        .Or().When(lipSyncActiveParam.IsFalse()).And(block.IsTrue());

                    onLipSyncTracksTransition.And(block.IsFalse());
                    onLipSyncAnimatesTransition.And(block.IsFalse());
                }
        }

        // States without Lip Sync Control
        else
        {
            var offFaceTrackingState = layer.NewState("face tracking off")
                .Drives(ftBlendParam, 0);

            var onFaceTrackingState = layer.NewState("face tracking on")
                .Drives(ftBlendParam, 1);

            // Transitions
            var offFaceTrackingTransition = layer.AnyTransitionsTo(offFaceTrackingState)
                .When(ftActiveParam.IsFalse());

            var onFaceTrackingTransition = layer.AnyTransitionsTo(onFaceTrackingState)
                //.WithTransitionToSelf()
                .When(ftActiveParam.IsTrue());

            if (customFaceTrackingBlocksNames != null && customFaceTrackingBlocksNames.Count > 0)
                foreach (var block in customFaceTrackingBlocksNames)
                {
                    if (block == null) continue;
                    offFaceTrackingTransition.Or().When(block.IsTrue());
                    onFaceTrackingTransition.And(block.IsFalse());
                }
        }

        // Tree face tracking
        var tree = _masterTree.CreateBlendTreeChild(0);
        tree.name = "Face Tracking";
        tree.blendType = BlendTreeType.Direct;

        var allShapes = new List<string>();

        // adding blend shapes
        for (int i = 0; i < SingleFtShapes.Length; i++)
        {
            var entry = SingleFtShapes[i];
            string shapeName = entry.shapeName;

            if (entry.leftAndRightShapes)
            {
                var baseName = StripSide(shapeName);
                var leftName = baseName + Left;
                var rightName = baseName + Right;

                var leftParam = CreateFloatParam(_fxTreeLayer, FullFaceTrackingPrefix + leftName, false, 0);
                tree.AddChild(BlendshapeTree(_fxTreeLayer, skin, leftParam));
                if (createOSCsmooth) allShapes.Add(FullFaceTrackingPrefix + leftName);

                var rightParam = CreateFloatParam(_fxTreeLayer, FullFaceTrackingPrefix + rightName, false, 0);
                tree.AddChild(BlendshapeTree(_fxTreeLayer, skin, rightParam));
                if (createOSCsmooth) allShapes.Add(FullFaceTrackingPrefix + rightName);
            }
            else
            {
                var param = CreateFloatParam(_fxTreeLayer, FullFaceTrackingPrefix + shapeName, false, 0);
                tree.AddChild(BlendshapeTree(_fxTreeLayer, skin, param));

                if (createOSCsmooth)
                    allShapes.Add(FullFaceTrackingPrefix + shapeName);
            }
        }

        // adding dual blend shapes
        for (int i = 0; i < DualFtShapes.Length; i++)
        {
            DualFtShape dualshape = DualFtShapes[i];
            string dualshapeName = dualshape.paramName;

            if (dualshape.leftAndRightShapes)
            {
                var baseParam = StripSide(dualshape.paramName);
                var baseMin = StripSide(dualshape.minShapeName);
                var baseMax = StripSide(dualshape.maxShapeName);

                var leftParamName = baseParam + Left;
                var leftParam = CreateFloatParam(_fxTreeLayer, FullFaceTrackingPrefix + leftParamName, false, 0);
                tree.AddChild(DualBlendshapeTree(
                    _fxTreeLayer,
                    leftParam,
                    skin,
                    FullFaceTrackingPrefix + baseMin + Left,
                    FullFaceTrackingPrefix + baseMax + Left,
                    dualshape.minValue,
                    dualshape.neutralValue,
                    dualshape.maxValue
                ));
                if (createOSCsmooth) allShapes.Add(FullFaceTrackingPrefix + leftParamName);

                var rightParamName = baseParam + Right;
                var rightParam = CreateFloatParam(_fxTreeLayer, FullFaceTrackingPrefix + rightParamName, false, 0);
                tree.AddChild(DualBlendshapeTree(
                    _fxTreeLayer,
                    rightParam,
                    skin,
                    FullFaceTrackingPrefix + baseMin + Right,
                    FullFaceTrackingPrefix + baseMax + Right,
                    dualshape.minValue,
                    dualshape.neutralValue,
                    dualshape.maxValue
                ));
                if (createOSCsmooth) allShapes.Add(FullFaceTrackingPrefix + rightParamName);
            }
            else
            {
                var param = CreateFloatParam(_fxTreeLayer, FullFaceTrackingPrefix + dualshapeName, false, 0);
                tree.AddChild(DualBlendshapeTree(
                    _fxTreeLayer,
                    param,
                    skin,
                    FullFaceTrackingPrefix + dualshape.minShapeName,
                    FullFaceTrackingPrefix + dualshape.maxShapeName,
                    dualshape.minValue,
                    dualshape.neutralValue,
                    dualshape.maxValue
                ));

                if (createOSCsmooth)
                    allShapes.Add(FullFaceTrackingPrefix + dualshapeName);
            }
        }

        var children = _masterTree.children;
        children[children.Length - 1].directBlendParameter = ftBlendParam.Name;
        _masterTree.children = children;

        // OSC Face Tracking smooth
        if (createOSCsmooth)
        {
            var oscLayer = _aac.CreateSupportingFxLayer("OSC smoothing").WithAvatarMask(fxMask);
            ApplyOSCSmoothing(oscLayer, localSmoothness, remoteSmoothness, allShapes, new List<BlendTree> { _masterTree });
        }
    }
}
#endif
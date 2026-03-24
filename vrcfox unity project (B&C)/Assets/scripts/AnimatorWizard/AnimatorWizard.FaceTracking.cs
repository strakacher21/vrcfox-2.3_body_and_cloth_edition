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

    private void InitializeFaceTracking(SkinnedMeshRenderer[] skins, VRCAvatarDescriptor avatar)
    {
        if (!createFaceTracking)
            return;

        if (skins == null || skins.Length == 0)
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
                if (HasFaceTrackingBlendShapeOnAnyMesh(skins, leftParam.Name))
                {
                    tree.AddChild(BuildFaceTrackingBlendshapeTreeForSkins(leftParam.Name, leftParam, skins));
                    if (createOSCsmooth) allShapes.Add(FullFaceTrackingPrefix + leftName);
                }

                var rightParam = CreateFloatParam(_fxTreeLayer, FullFaceTrackingPrefix + rightName, false, 0);
                if (HasFaceTrackingBlendShapeOnAnyMesh(skins, rightParam.Name))
                {
                    tree.AddChild(BuildFaceTrackingBlendshapeTreeForSkins(rightParam.Name, rightParam, skins));
                    if (createOSCsmooth) allShapes.Add(FullFaceTrackingPrefix + rightName);
                }
            }
            else
            {
                var param = CreateFloatParam(_fxTreeLayer, FullFaceTrackingPrefix + shapeName, false, 0);
                if (HasFaceTrackingBlendShapeOnAnyMesh(skins, param.Name))
                {
                    tree.AddChild(BuildFaceTrackingBlendshapeTreeForSkins(param.Name, param, skins));

                    if (createOSCsmooth)
                        allShapes.Add(FullFaceTrackingPrefix + shapeName);
                }
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
                var leftMinShape = FullFaceTrackingPrefix + baseMin + Left;
                var leftMaxShape = FullFaceTrackingPrefix + baseMax + Left;
                if (HasFaceTrackingBlendShapeOnAnyMesh(skins, leftMinShape) || HasFaceTrackingBlendShapeOnAnyMesh(skins, leftMaxShape))
                {
                    tree.AddChild(BuildFaceTrackingDualBlendshapeTreeForSkins(
                        leftParam,
                        skins,
                        leftMinShape,
                        leftMaxShape,
                        dualshape.minValue,
                        dualshape.neutralValue,
                        dualshape.maxValue
                    ));
                    if (createOSCsmooth) allShapes.Add(FullFaceTrackingPrefix + leftParamName);
                }

                var rightParamName = baseParam + Right;
                var rightParam = CreateFloatParam(_fxTreeLayer, FullFaceTrackingPrefix + rightParamName, false, 0);
                var rightMinShape = FullFaceTrackingPrefix + baseMin + Right;
                var rightMaxShape = FullFaceTrackingPrefix + baseMax + Right;
                if (HasFaceTrackingBlendShapeOnAnyMesh(skins, rightMinShape) || HasFaceTrackingBlendShapeOnAnyMesh(skins, rightMaxShape))
                {
                    tree.AddChild(BuildFaceTrackingDualBlendshapeTreeForSkins(
                        rightParam,
                        skins,
                        rightMinShape,
                        rightMaxShape,
                        dualshape.minValue,
                        dualshape.neutralValue,
                        dualshape.maxValue
                    ));
                    if (createOSCsmooth) allShapes.Add(FullFaceTrackingPrefix + rightParamName);
                }
            }
            else
            {
                var param = CreateFloatParam(_fxTreeLayer, FullFaceTrackingPrefix + dualshapeName, false, 0);
                var minShape = FullFaceTrackingPrefix + dualshape.minShapeName;
                var maxShape = FullFaceTrackingPrefix + dualshape.maxShapeName;
                if (HasFaceTrackingBlendShapeOnAnyMesh(skins, minShape) || HasFaceTrackingBlendShapeOnAnyMesh(skins, maxShape))
                {
                    tree.AddChild(BuildFaceTrackingDualBlendshapeTreeForSkins(
                        param,
                        skins,
                        minShape,
                        maxShape,
                        dualshape.minValue,
                        dualshape.neutralValue,
                        dualshape.maxValue
                    ));

                    if (createOSCsmooth)
                        allShapes.Add(FullFaceTrackingPrefix + dualshapeName);
                }
            }
        }

        var children = _masterTree.children;
        children[children.Length - 1].directBlendParameter = ftBlendParam.Name;
        _masterTree.children = children;

        // OSC Face Tracking smooth
        if (createOSCsmooth)
        {
            var oscLayer = _aac.CreateSupportingFxLayer("OSC smoothing").WithAvatarMask(fxMask);
            ApplyOSCSmooth(oscLayer, localSmoothness, remoteSmoothness, allShapes, new List<BlendTree> { _masterTree });
        }
    }

    private BlendTree BuildFaceTrackingBlendshapeTreeForSkins(string shapeName, AacFlParameter param, SkinnedMeshRenderer[] skins, float min = 0f, float max = 100f)
    {
        var state000 = _aac.NewClip();
        AddFaceTrackingBlendShapeOnAllMatchingMeshes(state000, skins, shapeName, min);
        state000.Clip.name = param.Name + " 0";

        var state100 = _aac.NewClip();
        AddFaceTrackingBlendShapeOnAllMatchingMeshes(state100, skins, shapeName, max);
        state100.Clip.name = param.Name + " 1";

        return Subtree(new Motion[] { state000.Clip, state100.Clip }, new[] { 0f, 1f }, param);
    }

    private BlendTree BuildFaceTrackingDualBlendshapeTreeForSkins(
        AacFlParameter param,
        SkinnedMeshRenderer[] skins,
        string minShapeName,
        string maxShapeName,
        float minValue,
        float neutralValue,
        float maxValue)
    {
        var minClip = _aac.NewClip();
        AddFaceTrackingBlendShapeOnAllMatchingMeshes(minClip, skins, minShapeName, 100f);
        AddFaceTrackingBlendShapeOnAllMatchingMeshes(minClip, skins, maxShapeName, 0f);
        minClip.Clip.name = param.Name + " " + minShapeName;

        var neutralClip = _aac.NewClip();
        AddFaceTrackingBlendShapeOnAllMatchingMeshes(neutralClip, skins, minShapeName, 0f);
        AddFaceTrackingBlendShapeOnAllMatchingMeshes(neutralClip, skins, maxShapeName, 0f);
        neutralClip.Clip.name = param.Name + " neutral";

        var maxClip = _aac.NewClip();
        AddFaceTrackingBlendShapeOnAllMatchingMeshes(maxClip, skins, minShapeName, 0f);
        AddFaceTrackingBlendShapeOnAllMatchingMeshes(maxClip, skins, maxShapeName, 100f);
        maxClip.Clip.name = param.Name + " " + maxShapeName;

        return Subtree(new Motion[] { minClip.Clip, neutralClip.Clip, maxClip.Clip }, new[] { minValue, neutralValue, maxValue }, param);
    }

    private bool HasFaceTrackingBlendShapeOnAnyMesh(SkinnedMeshRenderer[] skins, string blendShapeName)
    {
        foreach (var skin in skins)
        {
            if (skin == null || skin.sharedMesh == null)
                continue;

            if (skin.sharedMesh.GetBlendShapeIndex(blendShapeName) >= 0)
                return true;
        }

        return false;
    }

    private void AddFaceTrackingBlendShapeOnAllMatchingMeshes(AacFlClip clip, SkinnedMeshRenderer[] skins, string blendShapeName, float value)
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
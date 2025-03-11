#if UNITY_EDITOR
using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRC;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;

[Serializable]
public struct DualShape
{
    public string paramName, minShapeName, maxShapeName;
    public float minValue, neutralValue, maxValue;

    public DualShape(string paramName, string minShapeName, string maxShapeName, float minValue,
        float neutralValue, float maxValue)
    {
        this.paramName = paramName;
        this.minShapeName = minShapeName;
        this.maxShapeName = maxShapeName;

        this.minValue = minValue;
        this.neutralValue = neutralValue;
        this.maxValue = maxValue;
    }

    public DualShape(string paramName, string minShapeName, string maxShapeName)
    {
        this.paramName = paramName;
        this.minShapeName = minShapeName;
        this.maxShapeName = maxShapeName;

        minValue = -1;
        neutralValue = 0;
        maxValue = 1;
    }
}

public class AnimatorWizard : MonoBehaviour
{
    private AacFlBase _aac;
    private List<VRCExpressionParameters.Parameter> _vrcParams;

    private const string SystemName = "vrcfox";
    private const bool UseWriteDefaults = true;
    private const string Left = "Left";
    private const string Right = "Right";

    private const float TransitionSpeed = 0.05f;

    public AnimatorController assetContainer;

    public AvatarMask fxMask;
    public AvatarMask EyeLeftMask;
    public AvatarMask EyeRightMask;
    public AvatarMask gestureMask;
    public AvatarMask GestureLeftMask;
    public AvatarMask GestureRightMask;

    public bool MirrorHandposes = true;
    public Motion[] LeftHandPoses;
    public Motion[] RightHandPoses;

    public bool createShapePreferences = true;
    public string shapePreferenceSliderPrefix = "pref/slider/";
    public string shapePreferenceTogglesPrefix = "pref/toggle/";

    public bool createClothCustomization = true;
    public string ClothTogglesPrefix = "cloth/toggle/";

    public bool createColorCustomization = true;
    public Motion primaryColor0;
    public Motion primaryColor1;
    public Motion secondColor0;
    public Motion secondColor1;

    public bool createFacialExpressionsControl = false;
    public string expTrackName = "ExpressionTrackingActive";

    public bool createFTLipSyncControl = false;
    public string lipSyncName = "LipSyncTrackingActive";

    public bool saveVRCExpressionParameters = false;
    public bool MirrorFTparams = false;

    public bool createOSCsmooth = true;
    public float localSmoothness = 0.1f;
    public float remoteSmoothness = 0.7f;

    public bool createFaceTracking = true;
    public string FullFaceTrackingPrefix = "v2/";

    public bool createEyeTracking = true;
    public bool MirrorEyeposes = true;
    public float maxEyeMotionValue = 0.25f;
    public Motion[] LeftEyePoses;
    public Motion[] RightEyePoses;

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

    public bool createFaceToggle = false;
    public Motion[] FaceToggleNames;

    public string[] ftShapes =
    {
        "JawOpen",
        "LipFunnel",
        "LipPucker",
        "MouthClosed",
        "MouthStretch",
        "MouthUpperUpLeft",
        "MouthLowerDownLeft",
        "MouthRaiserLower",
        "TongueOut",
        "EyeSquintLeft",
    };

    public DualShape[] ftDualShapes =
    {
        new DualShape("SmileSad", "MouthSad", "MouthSmile"),
        new DualShape("JawX", "JawLeft", "JawRight"),
        new DualShape("JawZ", "JawBackward", "JawForward"),
        new DualShape("MouthX", "MouthLeft", "MouthRight"),
        new DualShape("EyeLidLeft", "EyeClosedLeft", "EyeWideLeft", 0, 0.75f, 1),
        new DualShape("BrowExpressionLeft", "BrowDown", "BrowUp"),
        new DualShape("CheekPuffSuck", "CheekSuck", "CheekPuff"),
    };

    public void Create()
    {
        SkinnedMeshRenderer skin = GetComponentInChildren<SkinnedMeshRenderer>();
        VRCAvatarDescriptor avatar = GetComponentInChildren<VRCAvatarDescriptor>();

        _vrcParams = new List<VRCExpressionParameters.Parameter>();

        _aac = AacV1.Create(new AacConfiguration
        {
            SystemName = SystemName,
            AnimatorRoot = avatar.transform,
            DefaultValueRoot = avatar.transform,
            AssetContainer = assetContainer,
            ContainerMode = AacConfiguration.Container.Everything,
            AssetKey = SystemName,
            DefaultsProvider = new AacDefaultsProvider(UseWriteDefaults),
            //AssetContainerProvider = null
        }.WithAvatarDescriptor(avatar));

        // clear assetContainer
        //_aac.ClearPreviousAssets(); // Broken in new version
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(assetContainer)))
            if (asset is AnimationClip or BlendTree)
                AssetDatabase.RemoveObjectFromAsset(asset);

        // Gesture Layer
        _aac.CreateMainGestureLayer().WithAvatarMask(gestureMask);

        // Iterate through each hand side (left/right)
        foreach (string side in new[] { Left, Right })
        {
            var layer = _aac.CreateSupportingGestureLayer(side + " hand")
                .WithAvatarMask(side == Left ? GestureLeftMask : GestureRightMask);

            var Gesture = layer.IntParameter("Gesture" + side);
            var GestureWeight = layer.FloatParameter("Gesture" + side + "Weight");

            Motion[] poses = side == Left || MirrorHandposes
                ? LeftHandPoses
                : RightHandPoses;

            if (poses == null || poses.Length != 8)
                throw new Exception($"The {side} hand poses array must contain exactly 8 motions!");

            // Create states for each gesture
            for (int i = 0; i < poses.Length; i++)
            {
                Motion motion = poses[i];

                if (motion == null)
                    throw new Exception($"Gesture animation for {side} hand, index {i}, is not assigned!");

                var state = layer.NewState(motion.name, 1, i)
                    .WithAnimation(motion);

                // Gesture Weight for the element with the index 1
                if (i == 1)
                {
                    state = state.WithMotionTime(GestureWeight);
                }

                layer.EntryTransitionsTo(state).When(Gesture.IsEqualTo(i));
                state.Exits()
                    .WithTransitionDurationSeconds(TransitionSpeed)
                    .When(Gesture.IsNotEqualTo(i));
            }
        }

        // FX layer
        var fxLayer = _aac.CreateMainFxLayer().WithAvatarMask(fxMask);
        fxLayer.OverrideValue(fxLayer.FloatParameter("Blend"), 1);

        // master fx tree
        var fxTreeLayer = _aac.CreateSupportingFxLayer("tree").WithAvatarMask(fxMask);

        var masterTree = _aac.NewBlendTreeAsRaw();
        masterTree.name = "master tree";
        masterTree.blendType = BlendTreeType.Direct;
        fxTreeLayer.NewState(masterTree.name).WithAnimation(masterTree);

        AacFlBoolParameter ftActiveParam = CreateBoolParam(fxLayer, FullFaceTrackingPrefix + "LipTrackingActive", true, false);
        AacFlFloatParameter ftBlendParam = fxLayer.FloatParameter(FullFaceTrackingPrefix + "LipTrackingActive-float");
        AacFlBoolParameter FaceToggleActive = fxLayer.BoolParameter("FaceToggleActive");
        AacFlBoolParameter ExpTrackActiveParam = CreateBoolParam(fxLayer, FullFaceTrackingPrefix + expTrackName, true, true);
        AacFlBoolParameter LipSyncActiveParam = CreateBoolParam(fxLayer, FullFaceTrackingPrefix + lipSyncName, true, false);

        // brow Gesture expressions
        MapHandPosesToShapes("brow expressions", skin, browShapeNames, browPrefix, false, ftActiveParam, ExpTrackActiveParam, FaceToggleActive);

        // mouth Gesture expressions
        MapHandPosesToShapes("mouth expressions", skin, mouthShapeNames, mouthPrefix, true, ftActiveParam, ExpTrackActiveParam, FaceToggleActive);

        // Toggle drivers (common to prefs and cloth)
        // this state transitions to itself every half second to update toggles. it sucks
        // TODO: not use this awful driver updating
        var fxDriverLayer = _aac.CreateSupportingFxLayer("preferences drivers").WithAvatarMask(fxMask);
        var fxDriverState = fxDriverLayer.NewState("preferences drivers");
        fxDriverState.TransitionsTo(fxDriverState).AfterAnimationFinishes().WithTransitionDurationSeconds(0.5f)
            .WithTransitionToSelf();
        var drivers = fxDriverState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();

        // Shape Preferences
        if (createShapePreferences)
        {
            var tree = masterTree.CreateBlendTreeChild(0);
            tree.name = "Shape Preferences";
            tree.blendType = BlendTreeType.Direct;

            // working with prefs blend shapes
            for (var i = 0; i < skin.sharedMesh.blendShapeCount; i++)
            {
                string blendShapeName = skin.sharedMesh.GetBlendShapeName(i);

                if (blendShapeName.StartsWith(shapePreferenceSliderPrefix))
                {
                    var param = CreateFloatParam(fxLayer, blendShapeName, true, 0);
                    tree.AddChild(BlendshapeTree(fxTreeLayer, skin, param));
                }
                else if (blendShapeName.StartsWith(shapePreferenceTogglesPrefix))
                {
                    var boolParam = CreateBoolParam(fxLayer, blendShapeName, true, false);
                    var floatParam = fxLayer.FloatParameter(blendShapeName + "-float");

                    var driverEntry = new VRC_AvatarParameterDriver.Parameter
                    {
                        type = VRC_AvatarParameterDriver.ChangeType.Copy,
                        source = boolParam.Name,
                        name = floatParam.Name
                    };
                    drivers.parameters.Add(driverEntry);

                    tree.AddChild(BlendshapeTree(fxTreeLayer, skin, blendShapeName, floatParam));
                }
            }
        }

        // Cloth Customization
        if (createClothCustomization)
        {
            setupClothes(ClothUpperBodyNames, skin, "cloth_upper_body");
            setupClothes(ClothLowerBodyNames, skin, "cloth_lower_body");
            setupClothes(ClothFootNames, skin, "cloth_foot");
        }

        // Color Customization
        if (createColorCustomization)
        {
            var tree = masterTree.CreateBlendTreeChild(0);
            tree.name = "Color Customization";
            tree.blendType = BlendTreeType.Direct;

            // color changing
            tree.AddChild(Subtree(new[] { primaryColor0, primaryColor1 },
                new[] { 0f, 1f },
                CreateFloatParam(fxTreeLayer, shapePreferenceSliderPrefix + "pcol", true, 0)));

            tree.AddChild(Subtree(new[] { secondColor0, secondColor1 },
                new[] { 0f, 1f },
                CreateFloatParam(fxTreeLayer, shapePreferenceSliderPrefix + "scol", true, 0)));
        }

        if (createEyeTracking)
        {
            var AdditiveLayer = _aac.CreateMainIdleLayer();
            AacFlBoolParameter etActiveParam = CreateBoolParam(AdditiveLayer, FullFaceTrackingPrefix + "EyeTrackingActive", true, false);
            AacFlFloatParameter WORKAROUND_BlendParam = AdditiveLayer.FloatParameter("OSCsmooth/Blend"); // WORKAROUND: OSCsmooth does not work if the Blend param is not updated. need to fix this!
            AacFlFloatParameter EyeXParam = CreateFloatParam(AdditiveLayer, FullFaceTrackingPrefix + "EyeX", false, 0.0f);
            AacFlFloatParameter EyeYParam = CreateFloatParam(AdditiveLayer, FullFaceTrackingPrefix + "EyeY", false, 0.0f);

            BlendTree leftEyeTree = setupEyeTracking(EyeXParam, EyeYParam, etActiveParam, WORKAROUND_BlendParam, "Left", maxEyeMotionValue, LeftEyePoses, EyeLeftMask);
            BlendTree rightEyeTree = setupEyeTracking(EyeXParam, EyeYParam, etActiveParam, WORKAROUND_BlendParam, "Right", maxEyeMotionValue, RightEyePoses, EyeRightMask);

            // OSC Eye Tracking smooth
            if (createOSCsmooth)
            {
                var allEyeParams = new List<string> { EyeXParam.Name, EyeYParam.Name };
                var OSCLayerEye = _aac.CreateSupportingIdleLayer("OSC smoothing");
                setupOSCsmooth(OSCLayerEye, localSmoothness, remoteSmoothness, allEyeParams, new List<BlendTree> { leftEyeTree, rightEyeTree });
            }
        }

        // Face Tracking
        if (createFaceTracking)
        {
            var layer = _aac.CreateSupportingFxLayer("face animations toggle").WithAvatarMask(fxMask);

            // State "face tracking off"
            var offFaceTrackingState = layer.NewState("face tracking off")
                .Drives(ftBlendParam, 0)
                .TrackingAnimates(AacAv3.Av3TrackingElement.Mouth);

            // State "face tracking on"
            var onFaceTrackingState = layer.NewState("face tracking on")
                .Drives(ftBlendParam, 1)
                .TrackingAnimates(AacAv3.Av3TrackingElement.Mouth);

            if (createFTLipSyncControl)
            {
                // State "face tracking off [LipSync]"
                var offFaceTrackingLipSyncState = layer.NewState("face tracking off [LipSync]")
                .Drives(ftBlendParam, 0)
                .TrackingTracks(AacAv3.Av3TrackingElement.Mouth);

                // State "face tracking on [LipSync]"
                var onFaceTrackingLipSyncState = layer.NewState("face tracking on [LipSync]")
                .Drives(ftBlendParam, 1)
                .TrackingTracks(AacAv3.Av3TrackingElement.Mouth);

                // Transitions
                var offFaceTrackingLipSyncTransition = layer.AnyTransitionsTo(offFaceTrackingLipSyncState)
                .When(ftActiveParam.IsFalse())
                .And(LipSyncActiveParam.IsTrue());

                var onFaceTrackingLipSyncTransition = layer.AnyTransitionsTo(onFaceTrackingLipSyncState)
                .WithTransitionToSelf()
                .When(ftActiveParam.IsTrue())
                .And(LipSyncActiveParam.IsTrue());

                layer.AnyTransitionsTo(offFaceTrackingState)
                    .When(ftActiveParam.IsFalse())
                    .And(LipSyncActiveParam.IsFalse());

                layer.AnyTransitionsTo(onFaceTrackingState)
                    .WithTransitionToSelf()
                    .When(ftActiveParam.IsTrue())
                    .And(LipSyncActiveParam.IsFalse());
            }

            else
            {
                layer.AnyTransitionsTo(offFaceTrackingState)
                    .When(ftActiveParam.IsFalse());

                layer.AnyTransitionsTo(onFaceTrackingState)
                    .When(ftActiveParam.IsTrue());
            }

            // Tree "face tracking"
            var tree = masterTree.CreateBlendTreeChild(0);
            tree.name = "Face Tracking";
            tree.blendType = BlendTreeType.Direct;
            tree.blendParameter = ftActiveParam.Name;
            tree.blendParameterY = ftActiveParam.Name;

            var allShapes = new List<string>();

            // adding blend shapes
            for (var i = 0; i < ftShapes.Length; i++)
            {
                string shapeName = ftShapes[i];

                if (MirrorFTparams)
                {
                    for (int flip = 0; flip < EachSide(ref shapeName); flip++)
                    {
                        var param = CreateFloatParam(fxLayer, FullFaceTrackingPrefix + shapeName, false, 0);
                        tree.AddChild(BlendshapeTree(fxTreeLayer, skin, param));
                        if (createOSCsmooth) allShapes.Add(FullFaceTrackingPrefix + shapeName);
                    }
                }

                else
                {
                    var param = CreateFloatParam(fxLayer, FullFaceTrackingPrefix + shapeName, false, 0);
                    tree.AddChild(BlendshapeTree(fxTreeLayer, skin, param));
                    if (createOSCsmooth) allShapes.Add(FullFaceTrackingPrefix + shapeName);
                }
            }

            // adding dual blend shapes
            for (var i = 0; i < ftDualShapes.Length; i++)
            {
                DualShape dualshape = ftDualShapes[i];
                string dualshapeName = dualshape.paramName;

                if (MirrorFTparams)
                {
                    for (int flip = 0; flip < EachSide(ref dualshapeName); flip++)
                    {
                        var param = CreateFloatParam(fxLayer, FullFaceTrackingPrefix + dualshapeName, false, 0);
                        tree.AddChild(DualBlendshapeTree(
                            fxTreeLayer, param, skin,
                            FullFaceTrackingPrefix + dualshape.minShapeName + GetSide(param.Name),
                            FullFaceTrackingPrefix + dualshape.maxShapeName + GetSide(param.Name),
                            dualshape.minValue, dualshape.neutralValue, dualshape.maxValue));
                        if (createOSCsmooth) allShapes.Add(FullFaceTrackingPrefix + dualshapeName);
                    }
                }

                else
                {
                    var param = CreateFloatParam(fxLayer, FullFaceTrackingPrefix + dualshape.paramName, false, 0);
                    tree.AddChild(DualBlendshapeTree(
                        fxTreeLayer, param, skin,
                        FullFaceTrackingPrefix + dualshape.minShapeName,
                        FullFaceTrackingPrefix + dualshape.maxShapeName,
                        dualshape.minValue, dualshape.neutralValue, dualshape.maxValue));
                    if (createOSCsmooth) allShapes.Add(FullFaceTrackingPrefix + dualshapeName);
                }
            }

            var children = masterTree.children;
            children[children.Length - 1].directBlendParameter = ftBlendParam.Name;
            masterTree.children = children;

            // OSC Face Tracking smooth
            if (createOSCsmooth)
            {
                var OSCLayer = _aac.CreateSupportingFxLayer("OSC smoothing").WithAvatarMask(fxMask);
                setupOSCsmooth(OSCLayer, localSmoothness, remoteSmoothness, allShapes, new List<BlendTree> { masterTree });
            }

            if (!saveVRCExpressionParameters)
            {
                // add all the new avatar params to the avatar descriptor
                avatar.expressionParameters.parameters = _vrcParams.ToArray();
                EditorUtility.SetDirty(avatar.expressionParameters);
            }
        }

        if (createFaceToggle)
        {
            var FaceToggleLayer = _aac.CreateSupportingFxLayer("Face Toggle").WithAvatarMask(fxMask);

            AacFlIntParameter FaceToggleActiveParam = CreateIntParam(fxLayer, FullFaceTrackingPrefix + "anim/FacePresets", false, 0);

            var FaceToggleWaitingState = FaceToggleLayer.NewState("Waiting command").Drives(FaceToggleActive, false);
            var waitingTransition = FaceToggleLayer.AnyTransitionsTo(FaceToggleWaitingState)
                .WithTransitionDurationSeconds(0.25f)
                .When(FaceToggleActiveParam.IsEqualTo(0)).Or().When(ftActiveParam.IsTrue());

            for (int i = 0; i < FaceToggleNames.Length; i++)
            {
                setupFaceToggle(FaceToggleLayer, FaceToggleNames[i], ftActiveParam, FaceToggleActiveParam, FaceToggleActive, i);
            }
        }
    }

    private BlendTree BlendshapeTree(AacFlLayer layer, SkinnedMeshRenderer skin, AacFlParameter param,
        float min = 0, float max = 100)
    {
        return BlendshapeTree(layer, skin, param.Name, param, min, max);
    }

    private BlendTree BlendshapeTree(AacFlLayer layer, SkinnedMeshRenderer skin, string shapeName, AacFlParameter param,
    float min = 0, float max = 100)
    {
        var state000 = _aac.NewClip().BlendShape(skin, shapeName, min);
        state000.Clip.name = param.Name + ":0";

        var state100 = _aac.NewClip().BlendShape(skin, shapeName, max);
        state100.Clip.name = param.Name + ":1";

        return Subtree(new Motion[] { state000.Clip, state100.Clip }, new[] { 0f, 1f },
            param);
    }

    private void setupFaceToggle(AacFlLayer FaceToggleLayer, Motion motion, AacFlBoolParameter ftActiveParam,
        AacFlIntParameter FaceToggleActiveParam, AacFlBoolParameter FaceToggleActive, int index)
    {
        var faceToggleState = FaceToggleLayer.NewState(motion.name)
            .Drives(FaceToggleActive, true)
            .WithAnimation(motion);

        FaceToggleLayer.AnyTransitionsTo(faceToggleState)
            .WithTransitionDurationSeconds(0.25f)
            .When(FaceToggleActiveParam.IsEqualTo(index + 1))
            .And(ftActiveParam.IsFalse());
    }

    private void MapHandPosesToShapes(string layerName, SkinnedMeshRenderer skin, string[] shapeNames, string prefix, bool rightHand,
        AacFlBoolParameter ftActiveParam, AacFlBoolParameter ExpTrackActiveParam, AacFlBoolParameter FaceToggleActive)
    {
        var layer = _aac.CreateSupportingFxLayer(layerName).WithAvatarMask(fxMask);
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
                clip.BlendShape(skin, prefix + shapeName, shapeName == shapeNames[i] ? 100 : 0);
            }

            var state = layer.NewState(shapeNames[i], 1, i)
                .WithAnimation(clip);

            var enter = layer.EntryTransitionsTo(state)
                .When(Gesture.IsEqualTo(i));
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
            if (createFacialExpressionsControl)
            {
                if (i == 0)
                {
                    enter.Or().When(ExpTrackActiveParam.IsFalse());
                    exit.And(ExpTrackActiveParam.IsTrue());
                }
                else
                {
                    enter.And(ExpTrackActiveParam.IsTrue());
                    exit.Or().When(ExpTrackActiveParam.IsFalse());
                }
            }

            if (createFaceToggle)
            {
                if (i == 0)
                {
                    enter.Or().When(FaceToggleActive.IsTrue());
                    exit.And(FaceToggleActive.IsFalse());
                }
                else
                {
                    enter.And(FaceToggleActive.IsFalse());
                    exit.Or().When(FaceToggleActive.IsTrue());
                }
            }
        }
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
        {
            if (!allPossibleClothes.Contains(clothName))
                allPossibleClothes.Add(clothName);
        }

        foreach (var clothName in allPossibleClothes)
        {
            var clothState = layer.NewState(clothName);
            var ClothDriverSetsTrue = clothState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();

            var ClothClip = _aac.NewClip($"Cloth_{clothName}");

            for (int i = 0; i < blendShapeCount; i++)
            {
                string blendShapeName = skin.sharedMesh.GetBlendShapeName(i);

                if (blendShapeName.Equals(ClothTogglesPrefix + clothName))
                {
                    var boolParam = CreateBoolParam(layer, ClothTogglesPrefix + clothName, true, false);

                    ClothDriverSetsFalse.parameters.Add(new VRC_AvatarParameterDriver.Parameter
                    {
                        name = ClothTogglesPrefix + clothName,
                        type = VRC_AvatarParameterDriver.ChangeType.Set,
                        value = 0
                    });

                    ClothDriverSetsTrue.parameters.Add(new VRC_AvatarParameterDriver.Parameter
                    {
                        name = ClothTogglesPrefix + clothName,
                        type = VRC_AvatarParameterDriver.ChangeType.Set,
                        value = 1
                    });

                    ClothClip.BlendShape(skin, blendShapeName, 100);
                }
            }

            foreach (var otherClothName in allPossibleClothes)
            {
                if (otherClothName != clothName)
                {
                    ClothDriverSetsTrue.parameters.Add(new VRC_AvatarParameterDriver.Parameter
                    {
                        name = ClothTogglesPrefix + otherClothName,
                        type = VRC_AvatarParameterDriver.ChangeType.Set,
                        value = 0
                    });

                    for (int i = 0; i < blendShapeCount; i++)
                    {
                        string blendShapeName = skin.sharedMesh.GetBlendShapeName(i);
                        if (blendShapeName.Equals(ClothTogglesPrefix + otherClothName))
                        {
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

    private BlendTree setupEyeTracking(AacFlFloatParameter EyeXParam, AacFlFloatParameter EyeYParam,
    AacFlBoolParameter etActiveParam, AacFlFloatParameter etBlendParam, string side, float maxMotionValue, Motion[] poses, AvatarMask mask)
    {
        var layer = _aac.CreateSupportingIdleLayer($"Eye {side} Tracking").WithAvatarMask(mask);

        if (poses == null || poses.Length != 9)
            throw new Exception($"The {side} eye poses array must contain exactly 9 motions!");

        var VRCEyeControlState = layer.NewState($"VRC Eye {side} Control")
            .TrackingTracks(AacAv3.Av3TrackingElement.Eyes);

        var EyeTrackingTree = _aac.NewBlendTreeAsRaw();
        EyeTrackingTree.name = $"Eye {side} Tracking";
        EyeTrackingTree.blendType = BlendTreeType.FreeformCartesian2D;
        EyeTrackingTree.blendParameter = EyeXParam.Name;
        EyeTrackingTree.blendParameterY = EyeYParam.Name;

        // add motions to the blend tree
        var positions = new[]
        {
            new Vector2(-maxMotionValue, maxMotionValue), // LeftUp
            new Vector2(0, maxMotionValue), // Up
            new Vector2(maxMotionValue, maxMotionValue), // RightUp
            new Vector2(-maxMotionValue, 0), // Left
            Vector2.zero, // Neutral
            new Vector2(maxMotionValue, 0), // Right
            new Vector2(-maxMotionValue, -maxMotionValue), // LeftDown
            new Vector2(0, -maxMotionValue), // Down
            new Vector2(maxMotionValue, -maxMotionValue) // RightDown
         };

        for (int i = 0; i < poses.Length; i++)
        {
            if (poses[i] == null)
                throw new Exception($"Eye tracking animation at index {i} is missing!");

            var child = new ChildMotion
            {
                motion = poses[i],
                position = positions[i],
                timeScale = 1f
            };

            var newChildren = new ChildMotion[EyeTrackingTree.children.Length + 1];
            Array.Copy(EyeTrackingTree.children, newChildren, EyeTrackingTree.children.Length);
            newChildren[EyeTrackingTree.children.Length] = child;
            EyeTrackingTree.children = newChildren;
        }

        var EyeTrackingState = layer.NewState($"Eye {side} Tracking")
            .Drives(etBlendParam, 1.0f)
            .WithAnimation(EyeTrackingTree)
            .TrackingAnimates(AacAv3.Av3TrackingElement.Eyes);

        layer.AnyTransitionsTo(VRCEyeControlState).When(etActiveParam.IsFalse());
        layer.AnyTransitionsTo(EyeTrackingState).WithTransitionToSelf().When(etActiveParam.IsTrue());

        return EyeTrackingTree;
    }

    private void setupOSCsmooth(AacFlLayer layer, float localSmoothness, float remoteSmoothness, List<string> list, List<BlendTree> trees)
    {
        AacFlBoolParameter isLocalParam = layer.BoolParameter("IsLocal");
        AacFlFloatParameter BlendOSC = layer.FloatParameter("OSCsmooth/Blend");

        var localTree = _aac.NewBlendTreeAsRaw();
        localTree.name = "OSC Local";
        localTree.blendType = BlendTreeType.Direct;

        var remoteTree = _aac.NewBlendTreeAsRaw();
        remoteTree.name = "OSC Remote";
        remoteTree.blendType = BlendTreeType.Direct;

        var localState = layer.NewState("OSC Local").WithAnimation(localTree);
        var remoteState = layer.NewState("OSC Remote").WithAnimation(remoteTree);

        layer.AnyTransitionsTo(localState).When(isLocalParam.IsTrue());
        layer.AnyTransitionsTo(remoteState).When(isLocalParam.IsFalse());

        foreach (var shape in list)
        {
            AacFlFloatParameter proxyParam = layer.FloatParameter($"OSCsmooth/Proxy/{shape}");
            AacFlFloatParameter localSmootherParam = layer.FloatParameter($"OSCsmooth/Local/{shape}Smoother");
            AacFlFloatParameter remoteSmootherParam = layer.FloatParameter($"OSCsmooth/Remote/{shape}Smoother");

            layer.OverrideValue(proxyParam, 0.0f);
            layer.OverrideValue(localSmootherParam, localSmoothness);
            layer.OverrideValue(remoteSmootherParam, remoteSmoothness);

            foreach (var tree in trees)
            {
                Queue<BlendTree> queue = new Queue<BlendTree>();
                queue.Enqueue(tree);

                while (queue.Count > 0)
                {
                    var currentTree = queue.Dequeue();

                    if (currentTree.blendParameter == shape) currentTree.blendParameter = proxyParam.Name;
                    if (currentTree.blendParameterY == shape) currentTree.blendParameterY = proxyParam.Name;

                    foreach (var child in currentTree.children)
                    {
                        if (child.motion is BlendTree childTree)
                        {
                            queue.Enqueue(childTree);
                        }
                    }
                }
            }

            foreach (var mode in new[] { "Local", "Remote" })
            {
                var currentTree = mode == "Local" ? localTree : remoteTree;
                var smootherParam = mode == "Local" ? localSmootherParam : remoteSmootherParam;

                var rootTree = _aac.NewBlendTreeAsRaw();
                rootTree.name = $"OSCsmooth/{mode}/{shape}Smoother";
                rootTree.blendType = BlendTreeType.Simple1D;
                rootTree.useAutomaticThresholds = false;
                rootTree.blendParameter = smootherParam.Name;

                var inputTree = rootTree.CreateBlendTreeChild(0);
                inputTree.name = $"OSCsmooth Input ({shape})";
                inputTree.blendType = BlendTreeType.Simple1D;
                inputTree.useAutomaticThresholds = false;
                inputTree.blendParameter = shape;

                var clipMin = _aac.NewClip()
                    .Animating(anim => anim.AnimatesAnimator(proxyParam).WithFixedSeconds(0.0f, -1.0f));

                var clipMax = _aac.NewClip()
                    .Animating(anim => anim.AnimatesAnimator(proxyParam).WithFixedSeconds(0.0f, 1.0f));

                inputTree.AddChild(clipMin.Clip, -1.0f);
                inputTree.AddChild(clipMax.Clip, 1.0f);

                var driverTree = rootTree.CreateBlendTreeChild(1);
                driverTree.name = $"OSCsmooth Driver ({shape})";
                driverTree.blendType = BlendTreeType.Simple1D;
                driverTree.useAutomaticThresholds = false;
                driverTree.blendParameter = proxyParam.Name;

                driverTree.AddChild(clipMin.Clip, -1.0f);
                driverTree.AddChild(clipMax.Clip, 1.0f);

                var newChildren = new ChildMotion[currentTree.children.Length + 1];
                for (int i = 0; i < currentTree.children.Length; i++)
                {
                    newChildren[i] = currentTree.children[i];
                }
                newChildren[newChildren.Length - 1] = new ChildMotion
                {
                    directBlendParameter = BlendOSC.Name,
                    motion = rootTree,
                    timeScale = 1
                };
                currentTree.children = newChildren;
            }
        }
    }

    private BlendTree DualBlendshapeTree(
        AacFlLayer layer, AacFlParameter param, SkinnedMeshRenderer skin,
        string minShapeName, string maxShapeName,
        float minValue, float neutralValue, float maxValue)
    {
        var minClip = _aac.NewClip()
            .BlendShape(skin, minShapeName, 100)
            .BlendShape(skin, maxShapeName, 0);
        minClip.Clip.name = param.Name + ":" + minShapeName;

        var neutralClip = _aac.NewClip()
            .BlendShape(skin, minShapeName, 0)
            .BlendShape(skin, maxShapeName, 0);
        neutralClip.Clip.name = param.Name + ":neutral";

        var maxClip = _aac.NewClip()
            .BlendShape(skin, minShapeName, 0)
            .BlendShape(skin, maxShapeName, 100);
        maxClip.Clip.name = param.Name + ":" + maxShapeName;

        return Subtree(new Motion[] { minClip.Clip, neutralClip.Clip, maxClip.Clip },
            new[] { minValue, neutralValue, maxValue }, param);
    }

    private BlendTree Subtree(Motion[] motions, float[] thresholds, AacFlParameter param)
    {
        var tree = Create1DTree(param.Name, 0, 1);

        ChildMotion[] children = new ChildMotion[motions.Length];

        for (int i = 0; i < motions.Length; i++)
        {
            children[i] = new ChildMotion { motion = motions[i], threshold = thresholds[i], timeScale = 1 };
        }

        tree.children = children;

        return tree;
    }
    // 'Int' isn't added to VRCExpressionParameters, it needs to be fixed!
    private AacFlIntParameter CreateIntParam(AacFlLayer layer, string paramName, bool save, int val)
    {
        _vrcParams.Add(new VRCExpressionParameters.Parameter()
        {
            name = paramName,
            valueType = VRCExpressionParameters.ValueType.Int,
            saved = save,
            networkSynced = true,
            defaultValue = val,
        });

        return layer.IntParameter(paramName);
    }

    private AacFlFloatParameter CreateFloatParam(AacFlLayer layer, string paramName, bool save, float val)
    {
        _vrcParams.Add(new VRCExpressionParameters.Parameter()
        {
            name = paramName,
            valueType = VRCExpressionParameters.ValueType.Float,
            saved = save,
            networkSynced = true,
            defaultValue = val,
        });

        return layer.FloatParameter(paramName);
    }

    private AacFlBoolParameter CreateBoolParam(AacFlLayer layer, string paramName, bool save, bool val)
    {

        _vrcParams.Add(new VRCExpressionParameters.Parameter()
        {
            name = paramName,
            valueType = VRCExpressionParameters.ValueType.Bool,
            saved = save,
            networkSynced = true,
            defaultValue = val ? 1 : 0,
        });

        return layer.BoolParameter(paramName);
    }

    private BlendTree Create1DTree(string paramName, float min, float max)
    {
        var tree = _aac.NewBlendTreeAsRaw();
        tree.useAutomaticThresholds = false;
        tree.name = paramName;
        tree.blendParameter = paramName;
        tree.minThreshold = min;
        tree.maxThreshold = max;
        tree.blendType = BlendTreeType.Simple1D;

        return tree;
    }

    private static int EachSide(ref string str)
    {
        if (str.EndsWith(Right))
        {
            str = str.Replace(Right, Left);
        }
        else if (str.EndsWith(Left))
        {
            str = str.Replace(Left, Right);
        }
        else
        {
            return 1;
        }

        return 2;
    }

    private static string GetSide(string str)
    {
        if (str.EndsWith(Right))
            return Right;
        if (str.EndsWith(Left))
            return Left;
        return "";
    }
}

[CustomEditor(typeof(AnimatorWizard), true)]
public class AnimatorGeneratorEditor : Editor
{
    private SerializedProperty saveVRCExpressionParameters, MirrorFTparams, MirrorHandposes, MirrorEyeposes;

    private SerializedProperty assetContainer;

    private SerializedProperty fxMask, EyeLeftMask, EyeRightMask, gestureMask, GestureLeftMask, GestureRightMask;

    private SerializedProperty LeftHandPoses, RightHandPoses;

    private SerializedProperty createShapePreferences, createColorCustomization, createClothCustomization, createFaceToggle, createEyeTracking, createFaceTracking;

    private SerializedProperty createFacialExpressionsControl, createFTLipSyncControl, createOSCsmooth;

    private SerializedProperty localSmoothness, remoteSmoothness;

    private SerializedProperty shapePreferenceSliderPrefix, shapePreferenceTogglesPrefix, mouthPrefix, browPrefix, FullFaceTrackingPrefix, ClothTogglesPrefix;

    private SerializedProperty primaryColor0, primaryColor1, secondColor0, secondColor1;

    private SerializedProperty maxEyeMotionValue;

    private SerializedProperty LeftEyePoses, RightEyePoses;

    private SerializedProperty mouthShapeNames, browShapeNames, expTrackName, ClothUpperBodyNames, ClothLowerBodyNames, ClothFootNames, FaceToggleNames;

    private SerializedProperty lipSyncName, faceToggleName, ftShapes, ftDualShapes;

    private AnimatorWizard wizard;

    private void OnEnable()
    {

        wizard = (AnimatorWizard)target;
        saveVRCExpressionParameters = serializedObject.FindProperty("saveVRCExpressionParameters");
        MirrorFTparams = serializedObject.FindProperty("MirrorFTparams");
        MirrorHandposes = serializedObject.FindProperty("MirrorHandposes");
        MirrorEyeposes = serializedObject.FindProperty("MirrorEyeposes");

        assetContainer = serializedObject.FindProperty("assetContainer");

        fxMask = serializedObject.FindProperty("fxMask");
        EyeLeftMask = serializedObject.FindProperty("EyeLeftMask");
        EyeRightMask = serializedObject.FindProperty("EyeRightMask");
        gestureMask = serializedObject.FindProperty("gestureMask");
        GestureLeftMask = serializedObject.FindProperty("GestureLeftMask");
        GestureRightMask = serializedObject.FindProperty("GestureRightMask");

        LeftHandPoses = serializedObject.FindProperty("LeftHandPoses");
        RightHandPoses = serializedObject.FindProperty("RightHandPoses");

        createShapePreferences = serializedObject.FindProperty("createShapePreferences");
        createFaceTracking = serializedObject.FindProperty("createFaceTracking");
        createColorCustomization = serializedObject.FindProperty("createColorCustomization");
        createClothCustomization = serializedObject.FindProperty("createClothCustomization");
        createFaceToggle = serializedObject.FindProperty("createFaceToggle");
        createEyeTracking = serializedObject.FindProperty("createEyeTracking");
        createFacialExpressionsControl = serializedObject.FindProperty("createFacialExpressionsControl");
        createFTLipSyncControl = serializedObject.FindProperty("createFTLipSyncControl");

        createOSCsmooth = serializedObject.FindProperty("createOSCsmooth");
        localSmoothness = serializedObject.FindProperty("localSmoothness");
        remoteSmoothness = serializedObject.FindProperty("remoteSmoothness");

        shapePreferenceSliderPrefix = serializedObject.FindProperty("shapePreferenceSliderPrefix");
        shapePreferenceTogglesPrefix = serializedObject.FindProperty("shapePreferenceTogglesPrefix");
        mouthPrefix = serializedObject.FindProperty("mouthPrefix");
        browPrefix = serializedObject.FindProperty("browPrefix");
        FullFaceTrackingPrefix = serializedObject.FindProperty("FullFaceTrackingPrefix");
        ClothTogglesPrefix = serializedObject.FindProperty("ClothTogglesPrefix");

        expTrackName = serializedObject.FindProperty("expTrackName");
        lipSyncName = serializedObject.FindProperty("lipSyncName");
        faceToggleName = serializedObject.FindProperty("faceToggleName");
        mouthShapeNames = serializedObject.FindProperty("mouthShapeNames");
        browShapeNames = serializedObject.FindProperty("browShapeNames");
        ClothUpperBodyNames = serializedObject.FindProperty("ClothUpperBodyNames");
        ClothLowerBodyNames = serializedObject.FindProperty("ClothLowerBodyNames");
        ClothFootNames = serializedObject.FindProperty("ClothFootNames");
        FaceToggleNames = serializedObject.FindProperty("FaceToggleNames");

        primaryColor0 = serializedObject.FindProperty("primaryColor0");
        secondColor0 = serializedObject.FindProperty("secondColor0");
        primaryColor1 = serializedObject.FindProperty("primaryColor1");
        secondColor1 = serializedObject.FindProperty("secondColor1");

        maxEyeMotionValue = serializedObject.FindProperty("maxEyeMotionValue");
        LeftEyePoses = serializedObject.FindProperty("LeftEyePoses");
        RightEyePoses = serializedObject.FindProperty("RightEyePoses");

        ftShapes = serializedObject.FindProperty("ftShapes");
        ftDualShapes = serializedObject.FindProperty("ftDualShapes");

    }

    private const string AlertMsg =
        "Running this will destroy any manual animator changes. Are you sure you want to continue?";

    // On Inspector GUI
    public override void OnInspectorGUI()
    {
        GUIStyle headerStyle = new GUIStyle()
        {
            richText = false,
            fontStyle = FontStyle.Bold,
            fontSize = EditorStyles.label.fontSize + 5,
            padding = new RectOffset(3, 3, 40, 8),
            normal = new GUIStyleState()
            {
                textColor = EditorStyles.label.normal.textColor
            }
        };

        GUIStyle headerStyle2 = new GUIStyle()
        {
            richText = false,
            fontStyle = FontStyle.Bold,
            fontSize = EditorStyles.label.fontSize + 1,
            padding = new RectOffset(3, 3, 0, 5),
            normal = new GUIStyleState()
            {
                textColor = EditorStyles.label.normal.textColor
            }
        };

        GUIContent PopUpLabel(string PropertyFieldLabel, string label)
        {
            return new GUIContent(PropertyFieldLabel, label);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Setup animator! (DESTRUCTIVE!!!)", GUILayout.Height(50)))
        {
            if (EditorUtility.DisplayDialog("Animator Wizard", AlertMsg, "yes (DESTRUCTIVE!)", "NO"))
            {
                Create();
            }
        }

        // Save VRC Expression Parameters
        GUILayout.Space(20);
        EditorGUILayout.PropertyField(saveVRCExpressionParameters,
         PopUpLabel("Save VRC Expression Parameters", "Will save your VRC Expression Parameters before setup animator."));

        // Asset Container
        GUILayout.Label("Asset Container", headerStyle);
        EditorGUILayout.PropertyField(assetContainer,
        PopUpLabel("Asset Container", "Asset Container stores all generated animations and Blend Trees."));

        // Avatar animator masks
        GUILayout.Label("Avatar animator masks", headerStyle);
        EditorGUILayout.PropertyField(fxMask);
        if (wizard.createEyeTracking)
        {
            EditorGUILayout.PropertyField(EyeLeftMask);
            EditorGUILayout.PropertyField(EyeRightMask);
        }
        EditorGUILayout.PropertyField(gestureMask);
        EditorGUILayout.PropertyField(GestureLeftMask);
        EditorGUILayout.PropertyField(GestureRightMask);

        // Hand Poses
        GUILayout.Label("Hand Poses", headerStyle);
        GUILayout.Label("Array index maps to hand gesture parameter. Array length should be 8!", headerStyle2);
        EditorGUILayout.PropertyField(MirrorHandposes, PopUpLabel("Mirror Hand Poses", ""));
        GUILayout.Space(10);

        if (wizard.MirrorHandposes)
        {
            EditorGUILayout.PropertyField(LeftHandPoses, PopUpLabel("Hand Poses", ""));
        }

        else
        {
            EditorGUILayout.PropertyField(LeftHandPoses);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(RightHandPoses);
        }

        // Facial expressions
        GUILayout.Label("Facial expressions", headerStyle);
        GUILayout.Label("Brow and mouth blendshapes controlled by left and right hands." +
            "\nArray index maps to hand Gesture parameter. Array length should be 8!", headerStyle2);
        GUILayout.Space(10);
        EditorGUILayout.PropertyField(createFacialExpressionsControl,
        PopUpLabel("Create Facial Expressions Control", "When created, adds a parameter to the VRC to disable/enable expression binding to hands."));
        GUILayout.Space(10);
        EditorGUILayout.PropertyField(mouthPrefix);
        EditorGUILayout.PropertyField(mouthShapeNames);
        GUILayout.Space(20);
        EditorGUILayout.PropertyField(browPrefix);
        EditorGUILayout.PropertyField(browShapeNames);

        // Animator creation flags
        GUILayout.Label("Animator creation flags", headerStyle);
        GUILayout.Label("Choose what parts of the animator are generated." +
            "\nDisabling features saves VRC params budget!", headerStyle2);
        GUILayout.Space(10);
        EditorGUILayout.PropertyField(createShapePreferences);
        EditorGUILayout.PropertyField(createClothCustomization);
        EditorGUILayout.PropertyField(createColorCustomization);
        EditorGUILayout.PropertyField(createFaceToggle);
        if (wizard.createFaceTracking || wizard.createEyeTracking)
            EditorGUILayout.PropertyField(createOSCsmooth);
        EditorGUILayout.PropertyField(createEyeTracking);
        EditorGUILayout.PropertyField(createFaceTracking);

        // Shape Preferences
        if (wizard.createShapePreferences)
        {
            GUILayout.Label("Shape Preferences", headerStyle);
            GUILayout.Label("Creates VRC params for blendshapes with these prefixes.", headerStyle2);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(shapePreferenceSliderPrefix);
            EditorGUILayout.PropertyField(shapePreferenceTogglesPrefix);
        }

        // Cloths customization
        if (wizard.createClothCustomization)
        {
            GUILayout.Label("Cloths customization", headerStyle);
            GUILayout.Label("Creates an algorithm to switch clothes, animations \nand VRC params with these prefixes.", headerStyle2);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(ClothTogglesPrefix,
            PopUpLabel("Cloth Toggles Prefix", "Prefixes roll up clothes and body into \"tube\",\n" +
            "as well as regulates the fit of the cloth lower body to the cloth upper body."));
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(ClothUpperBodyNames);
            EditorGUILayout.PropertyField(ClothLowerBodyNames);
            EditorGUILayout.PropertyField(ClothFootNames);
        }

        // Color customization
        if (wizard.createColorCustomization)
        {
            GUILayout.Label("Color customization UV-offset animations", headerStyle);
            GUILayout.Label("Animations controlling color palette texture UV-offsets (ToonLit shader)" +
                "\nfor in-game color customization.", headerStyle2);
            EditorGUILayout.PropertyField(primaryColor0);
            EditorGUILayout.PropertyField(primaryColor1);
            EditorGUILayout.PropertyField(secondColor0);
            EditorGUILayout.PropertyField(secondColor1);
        }

        // Face Toggle
        if (wizard.createFaceToggle)
        {
            GUILayout.Label("FaceToggle setup animations", headerStyle);
            GUILayout.Label("Creates an algorithm to switch face animations.", headerStyle2);
            EditorGUILayout.PropertyField(FaceToggleNames);
        }

        // OSC smooth
        if ((wizard.createFaceTracking || wizard.createEyeTracking) && wizard.createOSCsmooth)
        {
            GUILayout.Label("OSC smooth setup", headerStyle);
            GUILayout.Label("OSC smooth is needed to fix Face/Eye Tracking params in-game, " +
                "\nas without it animation is choppy and jerky, as if it's lacking FPS.", headerStyle2);
            EditorGUILayout.PropertyField(localSmoothness);
            EditorGUILayout.PropertyField(remoteSmoothness);
        }
        // EyeTracking
        if (wizard.createEyeTracking)
        {
            GUILayout.Label("EyeTracking (Simplified Eye Parameters) settings", headerStyle);
            GUILayout.Label("Creates EyeTracking with these animations.", headerStyle2);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(FullFaceTrackingPrefix);
            EditorGUILayout.PropertyField(MirrorEyeposes, PopUpLabel("Mirror Eye poses", "Don't use other animations for the right side."));
            GUILayout.Space(10);
            if (wizard.MirrorEyeposes)
            {
                EditorGUILayout.PropertyField(LeftEyePoses, PopUpLabel("Eye Poses", ""));
            }
            else
            {
                EditorGUILayout.PropertyField(LeftEyePoses);
                EditorGUILayout.PropertyField(RightEyePoses);
            }
        }

        // FaceTracking
        if (wizard.createFaceTracking)
        {
            GUILayout.Label("FaceTracking (Universal Shapes) settings", headerStyle);
            GUILayout.Label("Creates FaceTracking with these animations.", headerStyle2);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(FullFaceTrackingPrefix);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(createFTLipSyncControl,
            PopUpLabel("Create Face Tracking LipSync Control", "Adds LypSync off/on feature."));
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(MirrorFTparams,
             PopUpLabel("Mirroring shapes", "Reflect automatically blendshapes if they have “Left” in their name."));

            GUILayout.Space(10);
            EditorGUILayout.PropertyField(ftShapes, PopUpLabel("FT Single Shapes", "Single shapes controlled by a float parameter."));
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(ftDualShapes, PopUpLabel("FT Dual Shapes", "Mutually exclusive shape pairs controlled by a single float parameter."));
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void Create()
    {
        ((AnimatorWizard)target).Create();
    }
}
#endif
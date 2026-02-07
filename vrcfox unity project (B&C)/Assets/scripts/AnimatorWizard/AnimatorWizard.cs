#if UNITY_EDITOR

using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

public partial class AnimatorWizard : MonoBehaviour
{
    protected AacFlBase _aac;
    protected List<VRCExpressionParameters.Parameter> _vrcParams;

    protected AacFlLayer _fxTreeLayer;
    protected BlendTree _masterTree;

    private const bool UseWriteDefaults = true;
    protected const float TransitionSpeed = 0.05f;

    public AnimatorController assetContainer;
    public AvatarMask fxMask;

    public bool saveVRCExpressionParameters = false;
    public string SystemName = "AnimatorWizard";

    public string shapePreferenceSliderPrefix = "pref/slider/";
    public string shapePreferenceTogglesPrefix = "pref/toggle/";

    public string FullFaceTrackingPrefix = "v2/";

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

        //_aac.ClearPreviousAssets();
        ClearAssetContainer();
        DeleteAnimatorWizardLayers(avatar, SystemName);

        // FX layer
        var fxLayer = _aac.CreateMainFxLayer().WithAvatarMask(fxMask);

        var blendParam = fxLayer.FloatParameter("Blend");
        fxLayer.OverrideValue(blendParam, 1f);

        // master fx tree
        _fxTreeLayer = _aac.CreateSupportingFxLayer("tree").WithAvatarMask(fxMask);

        _masterTree = _aac.NewBlendTreeAsRaw();
        _masterTree.name = "master tree";
        _masterTree.blendType = BlendTreeType.Direct;
        _masterTree.blendParameter = blendParam.Name;

        _fxTreeLayer.NewState(_masterTree.name).WithAnimation(_masterTree);

        var ftActiveParam = fxLayer.BoolParameter(FullFaceTrackingPrefix + "LipTrackingActive");
        var faceToggleActiveParam = fxLayer.BoolParameter("FaceToggleActive");

        InitializeGestureLayers();
        InitializeGestureExpressions(skin, ftActiveParam);

        InitializeEyeTracking(skin, avatar);
        InitializeFaceTracking(skin, avatar);

        InitializeClothingCustomization(skin);
        InitializeColorCustomization();
        InitializeShapePreferences(skin);
        InitializeFaceToggle();

        if (!saveVRCExpressionParameters)
        {
            avatar.expressionParameters.parameters = _vrcParams.ToArray();
            EditorUtility.SetDirty(avatar.expressionParameters);
        }

        RepackAnimatorControllers(avatar);
        SortAnimatorWizardLayers(avatar, SystemName);
    }

    private void ClearAssetContainer()
    {
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(assetContainer)))
        {
            if (asset is AnimationClip or BlendTree)
                AssetDatabase.RemoveObjectFromAsset(asset);
        }
    }
}

#endif
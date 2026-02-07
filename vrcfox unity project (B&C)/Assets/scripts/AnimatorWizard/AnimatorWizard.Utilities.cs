#if UNITY_EDITOR

using System;
using UnityEditor;
using AnimatorAsCode.V1;
using VRLabs.AV3Manager;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Avatars.Components;

public partial class AnimatorWizard : MonoBehaviour
{
    protected const string Left = "Left";
    protected const string Right = "Right";

    protected BlendTree BlendshapeTree(AacFlLayer layer, SkinnedMeshRenderer skin, AacFlParameter param, float min = 0, float max = 100)
    {
        return BlendshapeTree(layer, skin, param.Name, param, min, max);
    }

    protected BlendTree BlendshapeTree(AacFlLayer layer, SkinnedMeshRenderer skin, string shapeName, AacFlParameter param, float min = 0, float max = 100)
    {
        var state000 = _aac.NewClip().BlendShape(skin, shapeName, min);
        state000.Clip.name = param.Name + ":0";

        var state100 = _aac.NewClip().BlendShape(skin, shapeName, max);
        state100.Clip.name = param.Name + ":1";

        return Subtree(new Motion[] { state000.Clip, state100.Clip }, new[] { 0f, 1f }, param);
    }

    protected BlendTree DualBlendshapeTree(
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

    protected BlendTree Subtree(Motion[] motions, float[] thresholds, AacFlParameter param)
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

    protected BlendTree Create1DTree(string paramName, float min, float max)
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
    protected static List<AacFlBoolParameter> BuildBlockBoolListParams(AacFlLayer layer, IEnumerable<string> names)
    {
        var result = new List<AacFlBoolParameter>();
        if (layer == null || names == null) return result;

        var seen = new HashSet<string>();
        foreach (var name in names)
        {
            if (string.IsNullOrWhiteSpace(name)) continue;
            if (!seen.Add(name)) continue; // avoid duplicate parameter names
            result.Add(layer.BoolParameter(name));
        }

        return result;
    }


    protected static string StripSide(string str)
    {
        if (str.EndsWith(Right)) return str.Substring(0, str.Length - Right.Length);
        if (str.EndsWith(Left)) return str.Substring(0, str.Length - Left.Length);
        return str;
    }

    protected static int EachSide(ref string str)
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

    protected static string GetSide(string str)
    {
        if (str.EndsWith(Right))
            return Right;

        if (str.EndsWith(Left))
            return Left;

        return "";
    }

    protected AacFlIntParameter CreateIntParam(AacFlLayer layer, string paramName, bool save, int val)
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

    protected AacFlFloatParameter CreateFloatParam(AacFlLayer layer, string paramName, bool save, float val)
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

    protected AacFlBoolParameter CreateBoolParam(AacFlLayer layer, string paramName, bool save, bool val)
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
    private HashSet<string> GetAnimatorWizardLayerNames(string systemName)
    {
        if (string.IsNullOrEmpty(systemName)) return new HashSet<string>();
        //Superset of all possible Wizard layer names (so reruns with different flags can still clean old layers)
        var set = new HashSet<string>(16);

        set.Add(systemName);
        set.Add(systemName + "__tree");
        set.Add(systemName + "__brow expressions");
        set.Add(systemName + "__mouth expressions");

        set.Add(systemName + "__preferences drivers");

        set.Add(systemName + "__clothupperbody");
        set.Add(systemName + "__clothlowerbody");
        set.Add(systemName + "__clothfoot");

        set.Add(systemName + "__face tracking toggle");
        set.Add(systemName + "__OSC smoothing");

        set.Add(systemName + "__Left hand");
        set.Add(systemName + "__Right hand");
        set.Add(systemName + "__Eye Left Tracking");
        set.Add(systemName + "__Eye Right Tracking");

        return set;
    }

    protected void DeleteAnimatorWizardLayers(VRCAvatarDescriptor avatar, string systemName)
    {
        if (avatar == null) return;

        var wizardLayerNames = GetAnimatorWizardLayerNames(systemName);
        if (wizardLayerNames.Count == 0) return;

        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        AssetDatabase.StartAssetEditing();

        try
        {
            for (int pass = 0; pass < 2; pass++)
            {
                var animLayers = pass == 0 ? avatar.baseAnimationLayers : avatar.specialAnimationLayers;

                foreach (var l in animLayers)
                {
                    if (l.isDefault) continue;
                    // We only touch controllers that AnimatorWizard actually manages
                    if (l.type != VRCAvatarDescriptor.AnimLayerType.FX &&
                        l.type != VRCAvatarDescriptor.AnimLayerType.Gesture &&
                        l.type != VRCAvatarDescriptor.AnimLayerType.Additive)
                        continue;

                    var controller = l.animatorController as AnimatorController;
                    if (controller == null) continue;
                    // delete only exact Wizard layers (never prefix-based) to avoid nuking user custom layers
                    for (int i = controller.layers.Length - 1; i >= 0; i--)
                        if (wizardLayerNames.Contains(controller.layers[i].name))
                            controller.RemoveLayer(i);

                    EditorUtility.SetDirty(controller);
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }

    protected void SortAnimatorWizardLayers(VRCAvatarDescriptor avatar, string systemName)
    {
        if (avatar == null) return;

        var wizardLayerNames = GetAnimatorWizardLayerNames(systemName);
        if (wizardLayerNames.Count == 0) return;

        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        AssetDatabase.StartAssetEditing();

        try
        {
            for (int pass = 0; pass < 2; pass++)
            {
                var animLayers = pass == 0 ? avatar.baseAnimationLayers : avatar.specialAnimationLayers;

                foreach (var l in animLayers)
                {
                    if (l.isDefault) continue;

                    if (l.type != VRCAvatarDescriptor.AnimLayerType.FX &&
                        l.type != VRCAvatarDescriptor.AnimLayerType.Gesture &&
                        l.type != VRCAvatarDescriptor.AnimLayerType.Additive)
                        continue;

                    var controller = l.animatorController as AnimatorController;
                    if (controller == null) continue;

                    var oldLayers = controller.layers;
                    // Wizard layers go first, user layers go to the bottom
                    var wizardLayers = new List<AnimatorControllerLayer>(oldLayers.Length);
                    var userLayers = new List<AnimatorControllerLayer>(oldLayers.Length);

                    for (int i = 0; i < oldLayers.Length; i++)
                    {
                        var name = oldLayers[i].name;
                        if (wizardLayerNames.Contains(name))
                            wizardLayers.Add(oldLayers[i]);
                        else
                            userLayers.Add(oldLayers[i]);
                    }

                    if (wizardLayers.Count != 0 && userLayers.Count != 0)
                    {
                        var newLayers = new AnimatorControllerLayer[wizardLayers.Count + userLayers.Count];
                        wizardLayers.CopyTo(newLayers, 0);
                        userLayers.CopyTo(newLayers, wizardLayers.Count);
                        controller.layers = newLayers;
                    }

                    // Remove this nonsense called 'Base Layer'
                    for (int i = controller.layers.Length - 1; i >= 0; i--)
                    {
                        if (controller.layers[i].name != "Base Layer") continue;

                        if (controller.layers.Length > 1)
                            controller.RemoveLayer(i);

                        break;
                    }

                    EditorUtility.SetDirty(controller);
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }

    protected void RepackAnimatorControllers(VRCAvatarDescriptor avatar)
    {
        if (avatar == null)
            return;

        var processedPaths = new HashSet<string>();

        UnityEditor.AssetDatabase.StartAssetEditing();
        try
        {
            for (int pass = 0; pass < 2; pass++)
            {
                var isBase = pass == 0;
                var layers = isBase ? avatar.baseAnimationLayers : avatar.specialAnimationLayers;
                var changed = false;

                for (int i = 0; i < layers.Length; i++)
                {
                    var layer = layers[i];
                    if (layer.isDefault)
                        continue;

                    if (layer.type != VRCAvatarDescriptor.AnimLayerType.FX &&
                        layer.type != VRCAvatarDescriptor.AnimLayerType.Gesture &&
                        layer.type != VRCAvatarDescriptor.AnimLayerType.Additive)
                        continue;

                    var sourceController = layer.animatorController as AnimatorController;
                    if (sourceController == null)
                        continue;

                    var originalPath = UnityEditor.AssetDatabase.GetAssetPath(sourceController);
                    if (string.IsNullOrEmpty(originalPath))
                        continue;

                    if (processedPaths.Contains(originalPath))
                    {
                        var reused = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimatorController>(originalPath);
                        if (reused != null)
                        {
                            layer.animatorController = reused;
                            layers[i] = layer;
                            changed = true;
                        }
                        continue;
                    }

                    var expectedOriginalName = System.IO.Path.GetFileNameWithoutExtension(originalPath);
                    var originalFileName = System.IO.Path.GetFileName(originalPath);

                    var tempFolderName = "AnimatorWizard_Temp_" + System.Guid.NewGuid().ToString("N");
                    UnityEditor.AssetDatabase.CreateFolder("Assets", tempFolderName);
                    var tempFolderPath = "Assets/" + tempFolderName;

                    var tempPath = tempFolderPath + "/" + originalFileName;

                    var newController = new AnimatorController { name = expectedOriginalName };
                    UnityEditor.AssetDatabase.CreateAsset(newController, tempPath);
                    UnityEditor.AssetDatabase.SaveAssets();

                    AnimatorCloner.MergeControllers(newController, sourceController, null, false);
                    UnityEditor.AssetDatabase.SaveAssets();

                    var absTempPath = System.IO.Path.GetFullPath(tempPath);
                    var absOriginalPath = System.IO.Path.GetFullPath(originalPath);
                    System.IO.File.Copy(absTempPath, absOriginalPath, true);

                    UnityEditor.AssetDatabase.ImportAsset(originalPath, UnityEditor.ImportAssetOptions.ForceUpdate);

                    var finalController = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimatorController>(originalPath);
                    if (finalController != null)
                    {
                        finalController.name = expectedOriginalName;
                        UnityEditor.EditorUtility.SetDirty(finalController);

                        layer.animatorController = finalController;
                        layers[i] = layer;
                        changed = true;
                    }

                    UnityEditor.AssetDatabase.DeleteAsset(tempFolderPath);
                    UnityEditor.AssetDatabase.SaveAssets();

                    processedPaths.Add(originalPath);
                }

                if (changed)
                {
                    if (isBase)
                        avatar.baseAnimationLayers = layers;
                    else
                        avatar.specialAnimationLayers = layers;

                    UnityEditor.EditorUtility.SetDirty(avatar);
                }
            }
        }
        finally
        {
            UnityEditor.AssetDatabase.StopAssetEditing();
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }
    }

}

#endif
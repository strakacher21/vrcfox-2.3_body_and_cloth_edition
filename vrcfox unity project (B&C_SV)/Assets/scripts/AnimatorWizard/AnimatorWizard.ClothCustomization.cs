#if UNITY_EDITOR

using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

public partial class AnimatorWizard : MonoBehaviour
{
    [Serializable]
    public class ClothEntry
    {
        public string clothName;
        public bool invertAnimation;
    }

    [Serializable]
    public class ClothGroup
    {
        public string layerName = "cloth_upper_body";
        public List<ClothEntry> clothEntries = new List<ClothEntry>();
    }

    public bool createClothCustomization = true;
    public bool ClothSyncParamsOptimizedAlgorithm = true;
    public string ClothTogglesPrefix = "cloth/";

    public List<ClothGroup> ClothGroups = new List<ClothGroup>
    {
        new ClothGroup
        {
            layerName = "cloth_upper_body",
            clothEntries = new List<ClothEntry>
            {
                new ClothEntry { clothName = "coat", invertAnimation = false },
                new ClothEntry { clothName = "coat_v2", invertAnimation = false },
                new ClothEntry { clothName = "T-shirt", invertAnimation = false },
            }
        },
        new ClothGroup
        {
            layerName = "cloth_lower_body",
            clothEntries = new List<ClothEntry>
            {
                new ClothEntry { clothName = "jeans", invertAnimation = false },
                new ClothEntry { clothName = "pants", invertAnimation = false },
                new ClothEntry { clothName = "shorts", invertAnimation = false },
            }
        },
        new ClothGroup
        {
            layerName = "cloth_foot",
            clothEntries = new List<ClothEntry>
            {
                new ClothEntry { clothName = "shoes", invertAnimation = false },
                new ClothEntry { clothName = "boots", invertAnimation = false },
                new ClothEntry { clothName = "slaps", invertAnimation = false },
            }
        }
    };

    private void InitializeClothingCustomization(SkinnedMeshRenderer[] skins)
    {
        if (!createClothCustomization || skins == null || skins.Length == 0 || ClothGroups == null)
            return;

        foreach (var group in ClothGroups)
        {
            if (group == null || string.IsNullOrWhiteSpace(group.layerName) || group.clothEntries == null || group.clothEntries.Count == 0)
                continue;

            SetupClothes(group.clothEntries, skins, group.layerName, ClothSyncParamsOptimizedAlgorithm);
        }
    }

    private static List<ClothEntry> CollectUniqueClothes(List<ClothEntry> clothEntries)
    {
        var result = new List<ClothEntry>();
        if (clothEntries == null)
            return result;

        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var entry in clothEntries)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.clothName))
                continue;

            var trimmedName = entry.clothName.Trim();
            if (!seen.Add(trimmedName))
                continue;

            result.Add(new ClothEntry
            {
                clothName = trimmedName,
                invertAnimation = entry.invertAnimation
            });
        }

        return result;
    }

    private static float GetEnabledValue(ClothEntry entry)
    {
        return entry != null && entry.invertAnimation ? 0f : 100f;
    }

    private static float GetDisabledValue(ClothEntry entry)
    {
        return entry != null && entry.invertAnimation ? 100f : 0f;
    }

    private void SetupClothes(List<ClothEntry> clothEntries, SkinnedMeshRenderer[] skins, string layerName, bool clothSyncParamsOptimizedAlgorithm)
    {
        if (clothEntries == null || clothEntries.Count == 0 || skins == null || skins.Length == 0 || string.IsNullOrWhiteSpace(layerName))
            return;

        var allPossibleClothes = CollectUniqueClothes(clothEntries);
        if (allPossibleClothes.Count == 0)
            return;

        var layer = _aac.CreateSupportingFxLayer(layerName).WithAvatarMask(fxMask);
        var waitingState = layer.NewState("Waiting command");

        var waitingClip = _aac.NewClip($"{layerName}_Waiting");
        foreach (var clothEntry in allPossibleClothes)
        {
            AddBlendShapeOnAllMatchingMeshes(
                waitingClip,
                skins,
                ClothTogglesPrefix + clothEntry.clothName,
                GetDisabledValue(clothEntry)
            );
        }
        waitingState.WithAnimation(waitingClip);

        VRCAvatarParameterDriver clothDriverSetsFalse = null;
        AacFlTransition waitingTransition = null;
        AacFlIntParameter intParam = null;
        var stateIndex = 1;

        if (clothSyncParamsOptimizedAlgorithm)
        {
            waitingTransition = layer.AnyTransitionsTo(waitingState);
            clothDriverSetsFalse = waitingState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            if (clothDriverSetsFalse.parameters == null)
                clothDriverSetsFalse.parameters = new List<VRCAvatarParameterDriver.Parameter>();
        }
        else
        {
            intParam = CreateIntParam(layer, layerName, true, 0);
            ApplyCompressedParams(intParam.Name, true);
            layer.AnyTransitionsTo(waitingState).When(intParam.IsEqualTo(0));
        }

        foreach (var clothEntry in allPossibleClothes)
        {
            var fullBlendShapeName = ClothTogglesPrefix + clothEntry.clothName;
            if (!HasBlendShapeOnAnyMatchingMesh(skins, fullBlendShapeName))
                continue;

            var clothClip = _aac.NewClip($"Cloth_{layerName}_{clothEntry.clothName}");

            foreach (var otherEntry in allPossibleClothes)
            {
                var value = otherEntry.clothName == clothEntry.clothName
                    ? GetEnabledValue(otherEntry)
                    : GetDisabledValue(otherEntry);

                AddBlendShapeOnAllMatchingMeshes(
                    clothClip,
                    skins,
                    ClothTogglesPrefix + otherEntry.clothName,
                    value
                );
            }

            var clothState = layer.NewState(clothEntry.clothName);
            clothState.WithAnimation(clothClip);

            if (clothSyncParamsOptimizedAlgorithm)
            {
                var clothDriverSetsTrue = clothState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                if (clothDriverSetsTrue.parameters == null)
                    clothDriverSetsTrue.parameters = new List<VRCAvatarParameterDriver.Parameter>();

                var boolParam = CreateBoolParam(layer, fullBlendShapeName, true, false);

                clothDriverSetsFalse.parameters.Add(new VRCAvatarParameterDriver.Parameter
                {
                    name = fullBlendShapeName,
                    type = VRCAvatarParameterDriver.ChangeType.Set,
                    value = 0
                });

                clothDriverSetsTrue.parameters.Add(new VRCAvatarParameterDriver.Parameter
                {
                    name = fullBlendShapeName,
                    type = VRCAvatarParameterDriver.ChangeType.Set,
                    value = 1
                });

                foreach (var otherEntry in allPossibleClothes)
                {
                    if (otherEntry.clothName == clothEntry.clothName)
                        continue;

                    clothDriverSetsTrue.parameters.Add(new VRCAvatarParameterDriver.Parameter
                    {
                        name = ClothTogglesPrefix + otherEntry.clothName,
                        type = VRCAvatarParameterDriver.ChangeType.Set,
                        value = 0
                    });
                }

                waitingTransition.When(boolParam.IsFalse());
                layer.AnyTransitionsTo(clothState).When(boolParam.IsTrue());
            }
            else
            {
                layer.AnyTransitionsTo(clothState).When(intParam.IsEqualTo(stateIndex));
                stateIndex++;
            }
        }
    }
}

#endif
#if UNITY_EDITOR

using System;
using UnityEditor.Animations;
using UnityEngine;

public partial class AnimatorWizard : MonoBehaviour
{
    [Serializable]
    public struct ColorProfile
    {
        public string name;
        public Motion primaryColor0;
        public Motion primaryColor1;
        public Motion secondColor0;
        public Motion secondColor1;
    }

    public bool createColorCustomization = true;

    public ColorProfile[] ColorProfiles;

    // Legacy fallback (optional)
    public Motion primaryColor0;
    public Motion primaryColor1;
    public Motion secondColor0;
    public Motion secondColor1;

    private const string ColorParamPrefix = "pref/color/";

    private void InitializeColorCustomization()
    {
        if (!createColorCustomization) return;

        if (_masterTree == null || _fxTreeLayer == null)
        {
            throw new Exception("FX master tree is not initialized (masterTree/fxTreeLayer).");
        }

        var colorRoot = _masterTree.CreateBlendTreeChild(0);
        colorRoot.name = "Color Customization";
        colorRoot.blendType = BlendTreeType.Direct;

        if (ColorProfiles != null && ColorProfiles.Length > 0)
        {
            foreach (var profile in ColorProfiles)
            {
                if (string.IsNullOrEmpty(profile.name)) continue;

                var profileTree = colorRoot.CreateBlendTreeChild(0);
                profileTree.name = profile.name;
                profileTree.blendType = BlendTreeType.Direct;

                if (profile.primaryColor0 != null && profile.primaryColor1 != null)
                {
                    var pcolParam = CreateFloatParam(_fxTreeLayer, $"{ColorParamPrefix}{profile.name}/slider/pcol", true, 0f);

                    profileTree.AddChild(Subtree(
                        new Motion[] { profile.primaryColor0, profile.primaryColor1 },
                        new[] { 0f, 1f },
                        pcolParam
                    ));
                }

                if (profile.secondColor0 != null && profile.secondColor1 != null)
                {
                    var scolParam = CreateFloatParam(_fxTreeLayer, $"{ColorParamPrefix}{profile.name}/slider/scol", true, 0f);

                    profileTree.AddChild(Subtree(
                        new Motion[] { profile.secondColor0, profile.secondColor1 },
                        new[] { 0f, 1f },
                        scolParam
                    ));
                }
            }

            return;
        }

        // Fallback if ColorProfiles is empty
        if (primaryColor0 != null && primaryColor1 != null)
        {
            var pcolParam = CreateFloatParam(_fxTreeLayer, $"{ColorParamPrefix}Default/slider/pcol", true, 0f);

            colorRoot.AddChild(Subtree(
                new Motion[] { primaryColor0, primaryColor1 },
                new[] { 0f, 1f },
                pcolParam
            ));
        }

        if (secondColor0 != null && secondColor1 != null)
        {
            var scolParam = CreateFloatParam(_fxTreeLayer, $"{ColorParamPrefix}Default/slider/scol", true, 0f);

            colorRoot.AddChild(Subtree(
                new Motion[] { secondColor0, secondColor1 },
                new[] { 0f, 1f },
                scolParam
            ));
        }
    }
}

#endif

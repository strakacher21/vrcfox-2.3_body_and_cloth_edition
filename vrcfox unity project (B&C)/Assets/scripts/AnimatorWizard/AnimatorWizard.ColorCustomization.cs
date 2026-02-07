#if UNITY_EDITOR

using System;
using UnityEditor.Animations;
using UnityEngine;

public partial class AnimatorWizard : MonoBehaviour
{
    public bool createColorCustomization = true;

    public Motion primaryColor0;
    public Motion primaryColor1;

    public Motion secondColor0;
    public Motion secondColor1;

    private void InitializeColorCustomization()
    {
        if (!createColorCustomization)
            return;

        // Color Customization
        if (_masterTree == null || _fxTreeLayer == null)
            throw new Exception("FX master tree is not initialized (masterTree/fxTreeLayer).");

        var tree = _masterTree.CreateBlendTreeChild(0);
        tree.name = "Color Customization";
        tree.blendType = BlendTreeType.Direct;

        // color changing
        tree.AddChild(Subtree(
            new[] { primaryColor0, primaryColor1 },
            new[] { 0f, 1f },
            CreateFloatParam(_fxTreeLayer, shapePreferenceSliderPrefix + "pcol", true, 0)
        ));

        tree.AddChild(Subtree(
            new[] { secondColor0, secondColor1 },
            new[] { 0f, 1f },
            CreateFloatParam(_fxTreeLayer, shapePreferenceSliderPrefix + "scol", true, 0)
        ));
    }
}

#endif
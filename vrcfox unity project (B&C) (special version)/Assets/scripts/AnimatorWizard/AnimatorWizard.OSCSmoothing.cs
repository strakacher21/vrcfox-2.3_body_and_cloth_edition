#if UNITY_EDITOR

using AnimatorAsCode.V1;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public partial class AnimatorWizard : MonoBehaviour
{
    public bool createOSCsmooth = true;

    public float localSmoothness = 0.1f;
    public float remoteSmoothness = 0.7f;

    partial void ApplyOSCSmoothing(
        AacFlLayer layer,
        float localSmoothness,
        float remoteSmoothness,
        List<string> parameters,
        List<BlendTree> trees)
    {
        setupOSCsmooth(layer, localSmoothness, remoteSmoothness, parameters, trees);
    }

    private void setupOSCsmooth(
        AacFlLayer layer,
        float localSmoothness,
        float remoteSmoothness,
        List<string> list,
        List<BlendTree> trees)
    {
        AacFlBoolParameter isLocalParam = layer.BoolParameter("IsLocal");

        AacFlFloatParameter BlendOSC = layer.FloatParameter("OSCsmooth/Blend");
        layer.OverrideValue(BlendOSC, 1.0f);

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
            AacFlFloatParameter proxyParam = layer.FloatParameter("OSCsmooth/Proxy/" + shape);
            AacFlFloatParameter localSmootherParam = layer.FloatParameter("OSCsmooth/Local/" + shape + "/Smoother");
            AacFlFloatParameter remoteSmootherParam = layer.FloatParameter("OSCsmooth/Remote/" + shape + "/Smoother");

            layer.OverrideValue(proxyParam, 0.0f);
            layer.OverrideValue(localSmootherParam, localSmoothness);
            layer.OverrideValue(remoteSmootherParam, remoteSmoothness);

            foreach (var tree in trees)
            {
                var queue = new Queue<BlendTree>();
                queue.Enqueue(tree);

                while (queue.Count > 0)
                {
                    var currentTree = queue.Dequeue();

                    if (currentTree.blendParameter == shape)
                        currentTree.blendParameter = proxyParam.Name;

                    if (currentTree.blendParameterY == shape)
                        currentTree.blendParameterY = proxyParam.Name;

                    foreach (var child in currentTree.children)
                    {
                        if (child.motion is BlendTree childTree)
                            queue.Enqueue(childTree);
                    }
                }
            }

            foreach (var modeLocal in new[] { true, false })
            {
                var currentTree = modeLocal ? localTree : remoteTree;
                var smootherParam = modeLocal ? localSmootherParam : remoteSmootherParam;

                var rootTree = _aac.NewBlendTreeAsRaw();
                rootTree.name = "OSCsmooth/" + (modeLocal ? "Local" : "Remote") + "/" + shape + "/Root";
                rootTree.blendType = BlendTreeType.Simple1D;
                rootTree.useAutomaticThresholds = false;
                rootTree.blendParameter = smootherParam.Name;

                var inputTree = _aac.NewBlendTreeAsRaw();
                inputTree.name = "OSCsmooth Input " + shape;
                inputTree.blendType = BlendTreeType.Simple1D;
                inputTree.useAutomaticThresholds = false;
                inputTree.blendParameter = shape;

                var clipMin = _aac.NewClip()
                    .Animating(a => a.AnimatesAnimator(proxyParam).WithFixedSeconds(0.0f, -1.0f));

                var clipMax = _aac.NewClip()
                    .Animating(a => a.AnimatesAnimator(proxyParam).WithFixedSeconds(0.0f, 1.0f));

                inputTree.AddChild(clipMin.Clip, -1.0f);
                inputTree.AddChild(clipMax.Clip, 1.0f);

                var driverTree = _aac.NewBlendTreeAsRaw();
                driverTree.name = "OSCsmooth Driver " + shape;
                driverTree.blendType = BlendTreeType.Simple1D;
                driverTree.useAutomaticThresholds = false;
                driverTree.blendParameter = proxyParam.Name;

                driverTree.AddChild(clipMin.Clip, -1.0f);
                driverTree.AddChild(clipMax.Clip, 1.0f);

                rootTree.AddChild(inputTree, 0.0f);
                rootTree.AddChild(driverTree, 1.0f);

                var newChildren = new ChildMotion[currentTree.children.Length + 1];
                for (int i = 0; i < currentTree.children.Length; i++)
                    newChildren[i] = currentTree.children[i];

                newChildren[newChildren.Length - 1] = new ChildMotion
                {
                    directBlendParameter = BlendOSC.Name,
                    motion = rootTree,
                    timeScale = 1f
                };

                currentTree.children = newChildren;
            }
        }
    }
}

#endif
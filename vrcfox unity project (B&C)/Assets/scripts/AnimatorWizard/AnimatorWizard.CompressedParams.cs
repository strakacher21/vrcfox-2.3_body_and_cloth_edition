#if UNITY_EDITOR
using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRC;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using VRC.SDK3.Avatars.Components;
using UnityEngine;

public partial class AnimatorWizard : MonoBehaviour
{
    public bool createParamsCompressor = true;

    private AacFlLayer cpLayer;
    private AacFlState cpLastStabilization;
    private AacFlStateMachine cpSender;
    private int cpSenderStateY = 0;
    private AacFlBase cpAacSnapshot;

    private void InitCompressedParamsLayer()
    {
        if (cpSender != null && cpAacSnapshot == _aac) return;

        cpLayer = null;
        cpLastStabilization = null;
        cpSender = null;
        cpSenderStateY = 0;
        cpAacSnapshot = _aac;

        cpLayer = _aac.CreateSupportingFxLayer("CompressedParams").WithAvatarMask(fxMask);

        cpLayer.IntParameter("CompressedParams/ParamValue");
        cpLayer.IntParameter("CompressedParams/ParamPack");

        var isLocal = cpLayer.Av3().IsLocal;

        var waiting = cpLayer.NewState("Waiting command", 2, 0);
        cpSender = cpLayer.NewSubStateMachine("Sender", 1, 1);
        var recipient = cpLayer.NewSubStateMachine("Recipient", 3, 1);

        waiting.TransitionsTo(cpSender).When(isLocal.IsTrue());
        waiting.TransitionsTo(recipient).When(isLocal.IsFalse());
    }

    private void ApplyCompressedParams(string paramName)
    {
        if (!createParamsCompressor) return;

        InitCompressedParamsLayer();
        var syncParam = cpLayer.BoolParameter("CompressedParams/Enabled");
        var n = cpSenderStateY + 1;
        var senderState = cpSender.NewState($"Sender №{n}", 0, cpSenderStateY);
        var stabilizationState = cpSender.NewState($"Stabilization №{n}", 1, cpSenderStateY);

        var driver = senderState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
        driver.parameters = new System.Collections.Generic.List<VRCAvatarParameterDriver.Parameter>
        {
            new VRCAvatarParameterDriver.Parameter
            {
                type  = VRCAvatarParameterDriver.ChangeType.Set,
                name  = "CompressedParams/ParamValue",
                value = n
            },

            new VRCAvatarParameterDriver.Parameter
            {
                type         = VRCAvatarParameterDriver.ChangeType.Copy,
                source       = paramName,
                name         = "CompressedParams/ParamPack",
                convertRange = true,
                sourceMin    = -1f,
                sourceMax    =  1f,
                destMin      =  0f,
                destMax      = 254f
            }
        };

        if (cpLastStabilization != null)
            cpLastStabilization.TransitionsTo(senderState)
                .WithTransitionDurationSeconds(0f)
                .When(syncParam.IsTrue());

        cpLastStabilization = stabilizationState;

            senderState.TransitionsTo(stabilizationState)
        .AfterAnimationIsAtLeastAtNormalized(0.1f)
        .WithTransitionDurationSeconds(0f);

        cpSenderStateY++;
    }
}
#endif
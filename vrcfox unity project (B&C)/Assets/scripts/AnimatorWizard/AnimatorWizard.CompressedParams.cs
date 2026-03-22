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
    private AacFlStateMachine cpRecipient;
    private int cpSenderStateY = 0;
    private AacFlBase cpAacSnapshot;

    private void InitCompressedParamsLayer()
    {
        if (cpSender != null && cpAacSnapshot == _aac) return;

        cpLayer = null;
        cpLastStabilization = null;
        cpSender = null;
        cpRecipient = null;
        cpSenderStateY = 0;
        cpAacSnapshot = _aac;

        cpLayer = _aac.CreateSupportingFxLayer("Compressed params").WithAvatarMask(fxMask);

        cpLayer.IntParameter("CompressedParams/ParamValue");
        cpLayer.IntParameter("CompressedParams/ParamPack");

        var isLocal = cpLayer.Av3().IsLocal;

        var waiting = cpLayer.NewState("Waiting command", 3, 0);
        cpSender = cpLayer.NewSubStateMachine("Sender", 2, 1);
        cpRecipient = cpLayer.NewSubStateMachine("Recipient", 4, 1);

        waiting.TransitionsTo(cpSender).When(isLocal.IsTrue());
        waiting.TransitionsTo(cpRecipient).When(isLocal.IsFalse());
    }

    private void ApplyCompressedParams(string paramName, bool isInt)
    {
        if (!createParamsCompressor) return;

        InitCompressedParamsLayer();
        var syncParam = cpLayer.BoolParameter("CompressedParams/Enabled");
        var n = cpSenderStateY + 1;
        var senderState = cpSender.NewState($"Sender №{n}", 3, cpSenderStateY);
        var stabilizationState = cpSender.NewState($"Stabilization №{n}", 4, cpSenderStateY);

        var senderDriverCopyParam = new VRCAvatarParameterDriver.Parameter
        {
            type = VRCAvatarParameterDriver.ChangeType.Copy,
            source = paramName,
            name = "CompressedParams/ParamPack"
        };
        if (!isInt)
        {
            senderDriverCopyParam.convertRange = true;
            senderDriverCopyParam.sourceMin = -1f;
            senderDriverCopyParam.sourceMax = 1f;
            senderDriverCopyParam.destMin = 0f;
            senderDriverCopyParam.destMax = 254f;
        }

        var SenderDriver = senderState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
        SenderDriver.parameters = new System.Collections.Generic.List<VRCAvatarParameterDriver.Parameter>
    {
        new VRCAvatarParameterDriver.Parameter
        {
            type  = VRCAvatarParameterDriver.ChangeType.Set,
            name  = "CompressedParams/ParamValue",
            value = n
        },
        senderDriverCopyParam
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

        var paramValue = cpLayer.IntParameter("CompressedParams/ParamValue");
        var recipientState = cpRecipient.NewState($"Recipient №{n}", 3, cpSenderStateY);

        var recipientDriverCopyParam = new VRCAvatarParameterDriver.Parameter
        {
            type = VRCAvatarParameterDriver.ChangeType.Copy,
            source = "CompressedParams/ParamPack",
            name = paramName
        };
        if (!isInt)
        {
            recipientDriverCopyParam.convertRange = true;
            recipientDriverCopyParam.sourceMin = 0f;
            recipientDriverCopyParam.sourceMax = 254f;
            recipientDriverCopyParam.destMin = -1f;
            recipientDriverCopyParam.destMax = 1f;
        }

        var RecipientDriver = recipientState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
        RecipientDriver.parameters = new System.Collections.Generic.List<VRCAvatarParameterDriver.Parameter>
    {
        recipientDriverCopyParam
    };

        cpRecipient.EntryTransitionsTo(recipientState)
            .When(paramValue.IsEqualTo(n));

        recipientState.Exits()
            .WithTransitionDurationSeconds(0f)
            .When(syncParam.IsFalse());
    }

}
#endif
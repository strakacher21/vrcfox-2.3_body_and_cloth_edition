#if UNITY_EDITOR
using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRC;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using UnityEngine;

public partial class AnimatorWizard : MonoBehaviour
{
    public bool createParamsCompressor = true;
    private void ApplyCompressedParams()
    {
        if (!createParamsCompressor)
            return;

        var layer = _aac.CreateSupportingFxLayer("CompressedParams").WithAvatarMask(fxMask);

        var isLocal = layer.Av3().IsLocal;

        var waiting = layer.NewState("Waiting command", 2, 0);

        var sender = layer.NewSubStateMachine("Sender (Local User)", 1, 1);
        var recipient = layer.NewSubStateMachine("Recipient (Remote User)", 3, 1);

        waiting.TransitionsTo(sender)
            .When(isLocal.IsTrue());

        waiting.TransitionsTo(recipient)
            .When(isLocal.IsFalse());
    }
}
#endif
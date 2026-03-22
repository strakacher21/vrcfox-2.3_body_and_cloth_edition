#if UNITY_EDITOR
using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRC;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

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

        if (isInt)
            cpLayer.IntParameter(paramName);
        else
            cpLayer.FloatParameter(paramName);

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
    [System.Serializable]
    public struct CompressedParamEntry
    {
        public string paramName;
        public bool useFloat;
        public bool useInt;
        [HideInInspector] public int lastMode;
    }

    public List<CompressedParamEntry> compressedParamEntries = new List<CompressedParamEntry>();

    private void InitializeCustomCompressedParams()
    {
        if (!createParamsCompressor) return;

        foreach (var entry in compressedParamEntries)
        {
            var name = entry.paramName?.Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;

            if (entry.useFloat)
                ApplyCompressedParams(name, false);
            else if (entry.useInt)
                ApplyCompressedParams(name, true);
        }
    }

    [UnityEditor.CustomPropertyDrawer(typeof(AnimatorWizard.CompressedParamEntry))]
    public class CompressedParamEntryDrawer : UnityEditor.PropertyDrawer
    {
        public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
            => UnityEditor.EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            var paramName = property.FindPropertyRelative("paramName");
            var useFloat = property.FindPropertyRelative("useFloat");
            var useInt = property.FindPropertyRelative("useInt");
            var lastMode = property.FindPropertyRelative("lastMode");

            UnityEditor.EditorGUI.BeginProperty(position, label, property);
            position = UnityEditor.EditorGUI.PrefixLabel(position, label);

            const float spacing = 6f;
            const float toggleW = 54f;

            var nameRect = new Rect(position.x, position.y, position.width - (toggleW * 2 + spacing * 2), position.height);
            var floatRect = new Rect(nameRect.xMax + spacing, position.y, toggleW, position.height);
            var intRect = new Rect(floatRect.xMax + spacing, position.y, toggleW, position.height);

            UnityEditor.EditorGUI.PropertyField(nameRect, paramName, GUIContent.none);

            UnityEditor.EditorGUI.BeginChangeCheck();
            var newFloat = UnityEditor.EditorGUI.ToggleLeft(floatRect, "Float", useFloat.boolValue);
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                useFloat.boolValue = newFloat;
                if (newFloat) useInt.boolValue = false;
                lastMode.intValue = 0;
            }

            UnityEditor.EditorGUI.BeginChangeCheck();
            var newInt = UnityEditor.EditorGUI.ToggleLeft(intRect, "Int", useInt.boolValue);
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                useInt.boolValue = newInt;
                if (newInt) useFloat.boolValue = false;
                lastMode.intValue = 1;
            }

            if (useFloat.boolValue && useInt.boolValue)
            {
                var keepInt = lastMode.intValue == 1;
                useInt.boolValue = keepInt;
                useFloat.boolValue = !keepInt;
            }

            UnityEditor.EditorGUI.EndProperty();
        }
    }
}
#endif
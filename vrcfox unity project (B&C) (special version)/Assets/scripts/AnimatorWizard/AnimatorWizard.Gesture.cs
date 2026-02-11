#if UNITY_EDITOR

using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using System;
using UnityEngine;

public partial class AnimatorWizard : MonoBehaviour
{
    public AvatarMask gestureMask;
    public AvatarMask GestureLeftMask;
    public AvatarMask GestureRightMask;

    public bool UseSameHandAnimationsForBothHands = true;

    public Motion[] LeftHandPoses;
    public Motion[] RightHandPoses;

    private void InitializeGestureLayers()
    {
        _aac.CreateMainGestureLayer()
            .WithAvatarMask(gestureMask);

        foreach (string side in new[] { Left, Right })
        {
            var layer = _aac.CreateSupportingGestureLayer(side + " hand")
                .WithAvatarMask(side == Left ? GestureLeftMask : GestureRightMask);

            SetupGestureLayer(layer, side);
        }
    }

    private void SetupGestureLayer(AacFlLayer layer, string side)
    {
        var Gesture = layer.IntParameter("Gesture" + side);
        var GestureWeight = layer.FloatParameter("Gesture" + side + "Weight");

        Motion[] poses = side == Left || UseSameHandAnimationsForBothHands ? LeftHandPoses : RightHandPoses;

        if (poses == null || poses.Length != 8)
            throw new Exception($"The {side} hand poses array must contain exactly 8 motions!");

        for (int i = 0; i < poses.Length; i++)
        {
            Motion motion = poses[i];

            if (motion == null)
                throw new Exception($"Gesture animation for {side} hand, index {i}, is not assigned!");

            var state = layer.NewState(motion.name, 1, i)
                .WithAnimation(motion);

            if (i == 1)
                state = state.WithMotionTime(GestureWeight);

            layer.EntryTransitionsTo(state)
                .When(Gesture.IsEqualTo(i));

            state.Exits()
                .WithTransitionDurationSeconds(TransitionSpeed)
                .When(Gesture.IsNotEqualTo(i));
        }
    }
}

#endif
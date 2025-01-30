#if UNITY_EDITOR
using System;
using AnimatorAsCode.V0;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine.Serialization;
using VRC;
using VRC.SDKBase;
using System.ComponentModel.Composition.Primitives;
using UnityEngine.XR;
using static BlackStartX.GestureManager.Editor.Data.GestureManagerStyles.Animations;
using static UnityEditor.Experimental.GraphView.GraphView;

[Serializable]
public struct DualShape
{
	public string paramName, minShapeName, maxShapeName;
	public float minValue, neutralValue, maxValue;

	public DualShape(string paramName, string minShapeName, string maxShapeName, float minValue,
		float neutralValue, float maxValue)
	{
		this.paramName = paramName;
		this.minShapeName = minShapeName;
		this.maxShapeName = maxShapeName;

		this.minValue = minValue;
		this.neutralValue = neutralValue;
		this.maxValue = maxValue;
	}

	public DualShape(string paramName, string minShapeName, string maxShapeName)
	{
		this.paramName = paramName;
		this.minShapeName = minShapeName;
		this.maxShapeName = maxShapeName;

		minValue = -1;
		neutralValue = 0;
		maxValue = 1;
	}
}

public class AnimatorWizard : MonoBehaviour
{
	public AnimatorController assetContainer;
	
	public AvatarMask fxMask;
	public AvatarMask EyeLeftMask;
	public AvatarMask EyeRightMask;
	public AvatarMask gestureMask;
	public AvatarMask lMask;
	public AvatarMask rMask;
	
	public bool MirrorHandposes = true;	
	public Motion[] LeftHandPoses;
	public Motion[] RightHandPoses;
	
	public bool createShapePreferences = true;
	public string shapePreferenceSliderPrefix = "pref/slider/";
	public string shapePreferenceTogglesPrefix = "pref/toggle/";

	public bool createClothCustomization = true;
	public string ClothTogglesPrefix = "cloth/toggle/";
	public string ClothAdjustPrefix = "cloth/adjust/";
    public string ClothAdjustBodyPrefix = "cloth/adjust/Body/with/";

    public bool createColorCustomization = true;
	public Motion primaryColor0;
	public Motion primaryColor1;
	public Motion secondColor0;
	public Motion secondColor1;

	public bool createFacialExpressionsControl = false;
	public string expTrackName = "ExpressionTrackingActive";

	public bool createFTLipSyncControl = false;
	public string lipSyncName = "LipSyncTrackingActive";

	public bool createFaceToggleControl = false;
	public string faceToggleName = "FaceToggleActive";

	public bool createFTreset = false;
	public string resetFTName = "Reset";

	public bool saveVRCExpressionParameters = false;
	public bool MirrorFTparams = false;
	
	public bool createOSCsmooth = true;
	public bool IsLocal = false;
	public float localSmoothness = 0.1f;
    public float remoteSmoothness = 0.7f;

	public bool createEyeTracking = true;
	public bool MirrorEyeposes = true;
	public float maxEyeMotionValue = 0.25f;
	public Motion[] LeftEyePoses;
	public Motion[] RightEyePoses;
	
	public string mouthPrefix = "exp/mouth/";
	public string[] mouthShapeNames =
	{
		"basis",
		"frown",
		"smile",
		"grimace",
		"smile",
		"grimace",
		"grimace",
		"frown",
	};

	public string browPrefix = "exp/brows/";
	public string[] browShapeNames =
	{
		"basis",
		"down",
		"up",
		"curious",
		"up",
		"worried",
		"curious",
		"down",
	};

    public string[] ClothUpperBodyNames =
    {
        "coat",
        "coat_v2",
        "T-shirt",
    };

    public string[] ClothLowerBodyNames =
       {
        "jeans",
        "pants",
        "shorts",
    };

    public string[] ClothFootNames =
       {
        "shoes",
        "boots",
        "slaps",
    };


    public bool createFaceTracking = true;
	public string ftPrefix = "v2/";
	public string[] ftShapes =
	{
		"JawOpen",
		"LipFunnel",
		"LipPucker",
		"MouthClosed",
		"MouthStretch",
		"MouthUpperUpLeft",
		"MouthLowerDownLeft",
		"MouthRaiserLower",
		"TongueOut",
		"EyeSquintLeft",
	};

	public DualShape[] ftDualShapes =
	{
		new DualShape("SmileSad", "MouthSad", "MouthSmile"),
		new DualShape("JawX", "JawLeft", "JawRight"),
		new DualShape("EyeLidLeft", "EyeClosedLeft", "EyeWideLeft", 0, 0.75f, 1),
		new DualShape("BrowExpressionLeft", "BrowDown", "BrowUp"),
		new DualShape("CheekPuffSuck", "CheekSuck", "CheekPuff"),
	};

	private AacFlBase _aac;
	private List<VRCExpressionParameters.Parameter> _vrcParams;

	private const string Left = "Left";
	private const string Right = "Right";

	private const string SystemName = "vrcfox";
	private const float TransitionSpeed = 0.05f;

	public void Create()
	{
		SkinnedMeshRenderer skin = GetComponentInChildren<SkinnedMeshRenderer>();
		VRCAvatarDescriptor avatar = GetComponentInChildren<VRCAvatarDescriptor>();

		_vrcParams = new List<VRCExpressionParameters.Parameter>();

		_aac = AacV0.Create(new AacConfiguration
		{
			SystemName = SystemName,
			AvatarDescriptor = avatar,
			AnimatorRoot = avatar.transform,
			DefaultValueRoot = avatar.transform,
			AssetContainer = assetContainer,
			AssetKey = SystemName,
			DefaultsProvider = new AacDefaultsProvider(false)
		});

		_aac.ClearPreviousAssets();

		// Gesture Layer
		_aac.CreateMainGestureLayer().WithAvatarMask(gestureMask);

		// Iterate through each hand side (left/right)
		foreach (string side in new[] { Left, Right })
		{
			var layer = _aac.CreateSupportingGestureLayer(side + " hand")
				.WithAvatarMask(side == Left ? lMask : rMask);

			var Gesture = layer.IntParameter("Gesture" + side);
			var GestureWeight = layer.FloatParameter("Gesture" + side + "Weight");

			Motion[] poses = side == Left || MirrorHandposes
				? LeftHandPoses
				: RightHandPoses;

			if (poses == null || poses.Length != 8)
				throw new Exception($"The {side} hand poses array must contain exactly 8 motions!");

			// Create states for each gesture
			for (int i = 0; i < poses.Length; i++)
			{
				Motion motion = poses[i];

				if (motion == null)
					throw new Exception($"Gesture animation for {side} hand, index {i}, is not assigned!");

				var state = layer.NewState(motion.name, 1, i)
					.WithAnimation(motion)
					.WithWriteDefaultsSetTo(true);

				layer.EntryTransitionsTo(state).When(Gesture.IsEqualTo(i));
				state.Exits()
					.WithTransitionDurationSeconds(TransitionSpeed)
					.When(Gesture.IsNotEqualTo(i));
			}
		}

		// FX layer
		var fxLayer = _aac.CreateMainFxLayer().WithAvatarMask(fxMask);
		fxLayer.OverrideValue(fxLayer.FloatParameter("Blend"), 1);

		// master fx tree
		var fxTreeLayer = _aac.CreateSupportingFxLayer("tree").WithAvatarMask(fxMask);

		var masterTree = _aac.NewBlendTreeAsRaw();
		masterTree.name = "master tree";
		masterTree.blendType = BlendTreeType.Direct;
		fxTreeLayer.NewState(masterTree.name).WithAnimation(masterTree)
			.WithWriteDefaultsSetTo(true);

		AacFlBoolParameter ftActiveParam = CreateBoolParam(fxLayer, ftPrefix + "LipTrackingActive", true, false);
		AacFlFloatParameter ftBlendParam = fxLayer.FloatParameter(ftPrefix + "LipTrackingActive-float");

		AacFlBoolParameter ExpTrackActiveParam = CreateBoolParam(fxLayer, ftPrefix + expTrackName, true, true);
		AacFlBoolParameter LipSyncActiveParam = CreateBoolParam(fxLayer, lipSyncName, true, false);
		AacFlBoolParameter FaceToggleActiveParam = CreateBoolParam(fxLayer, faceToggleName, false, false);
		AacFlBoolParameter ResetFTActiveParam = CreateBoolParam(fxLayer, ftPrefix + resetFTName, false, false);

		// brow Gesture expressions
		MapHandPosesToShapes("brow expressions", skin, browShapeNames, browPrefix, false, ftActiveParam, ExpTrackActiveParam, FaceToggleActiveParam);

		// mouth Gesture expressions
		MapHandPosesToShapes("mouth expressions", skin, mouthShapeNames, mouthPrefix, true, ftActiveParam, ExpTrackActiveParam, FaceToggleActiveParam);

		// Toggle drivers (common to prefs and cloth)
		// this state transitions to itself every half second to update toggles. it sucks
		// TODO: not use this awful driver updating
        var fxDriverLayer = _aac.CreateSupportingFxLayer("drivers").WithAvatarMask(fxMask);
		var fxDriverState = fxDriverLayer.NewState("drivers").WithWriteDefaultsSetTo(true);
		fxDriverState.TransitionsTo(fxDriverState).AfterAnimationFinishes().WithTransitionDurationSeconds(0.5f)
			.WithTransitionToSelf();
		var drivers = fxDriverState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();

		// Shape Preferences
		if (createShapePreferences)
		{
			var tree = masterTree.CreateBlendTreeChild(0);
			tree.name = "Shape Preferences";
			tree.blendType = BlendTreeType.Direct;

            // working with prefs blend shapes
            for (var i = 0; i < skin.sharedMesh.blendShapeCount; i++)
			{
				string blendShapeName = skin.sharedMesh.GetBlendShapeName(i);

				if (blendShapeName.StartsWith(shapePreferenceSliderPrefix))
				{
                    var param = CreateFloatParam(fxLayer, blendShapeName, true, 0);
					tree.AddChild(BlendshapeTree(fxTreeLayer, skin, param));
				}
				else if (blendShapeName.StartsWith(shapePreferenceTogglesPrefix))
				{
                    var boolParam = CreateBoolParam(fxLayer, blendShapeName, true, false);
					var floatParam = fxLayer.FloatParameter(blendShapeName + "-float");

                    var driverEntry = new VRC_AvatarParameterDriver.Parameter
					{
						type = VRC_AvatarParameterDriver.ChangeType.Copy,
						source = boolParam.Name,
						name = floatParam.Name
					};
					drivers.parameters.Add(driverEntry);

					tree.AddChild(BlendshapeTree(fxTreeLayer, skin, blendShapeName, floatParam));
				}
			}
		}

		// Cloth Customization
		if (createClothCustomization)
		{
			var tree = masterTree.CreateBlendTreeChild(0);
			tree.name = "Cloth Customization";
			tree.blendType = BlendTreeType.Direct;

			var toggleTree = tree.CreateBlendTreeChild(0);
			toggleTree.name = "Cloth Toggle";
			toggleTree.blendType = BlendTreeType.Direct;

			var adjustTree = tree.CreateBlendTreeChild(1);
			adjustTree.name = "Cloth Adjust";
			adjustTree.blendType = BlendTreeType.Direct;

			// Process different parts of body clothing
			ProcessClothPart(ClothUpperBodyNames, skin, toggleTree, adjustTree);
			ProcessClothPart(ClothLowerBodyNames, skin, toggleTree, adjustTree);
			ProcessClothPart(ClothFootNames, skin, toggleTree, adjustTree);

			// Upper Body Clothes
			var upperBodyLayer = _aac.CreateSupportingFxLayer("cloth_upper_body").WithAvatarMask(fxMask);
			SetupClothLayer(upperBodyLayer, ClothUpperBodyNames, skin, ClothTogglesPrefix, ClothAdjustBodyPrefix);

			// Lower Body Clothes
			var lowerBodyLayer = _aac.CreateSupportingFxLayer("cloth_lower_body").WithAvatarMask(fxMask);
			SetupClothLayerWithAdjustments(lowerBodyLayer, ClothLowerBodyNames, skin, ClothTogglesPrefix, ClothAdjustPrefix, ClothAdjustBodyPrefix);

			// Foot Clothes
			var footLayer = _aac.CreateSupportingFxLayer("cloth_foot").WithAvatarMask(fxMask);
			SetupClothLayer(footLayer, ClothFootNames, skin, ClothTogglesPrefix, ClothAdjustBodyPrefix);
		}

		void ProcessClothPart(string[] clothNames, SkinnedMeshRenderer skin, BlendTree toggleTree, BlendTree adjustTree)
		{
			foreach (var clothName in clothNames)
			{
				if (skin.sharedMesh.blendShapeCount > 0)
				{
					for (int i = 0; i < skin.sharedMesh.blendShapeCount; i++)
					{
						string blendShapeName = skin.sharedMesh.GetBlendShapeName(i);
						
                		// Check if the blend shape is for toggling cloth
						if (blendShapeName.StartsWith(ClothTogglesPrefix + clothName))
						{
							var boolParam = CreateBoolParam(fxLayer, ClothTogglesPrefix + clothName, true, false);
							var floatParam = fxLayer.FloatParameter(ClothTogglesPrefix + clothName + "-float");

							// add parameter driver to toggle function
							var driverEntry = new VRC_AvatarParameterDriver.Parameter
							{
								type = VRC_AvatarParameterDriver.ChangeType.Copy,
								source = boolParam.Name,
								name = floatParam.Name
							};
							drivers.parameters.Add(driverEntry);

							// add toggle blend shape to the blend tree
							toggleTree.AddChild(BlendshapeTree(fxTreeLayer, skin, ClothTogglesPrefix + clothName, floatParam, max: 0, min: 100));
						}

						// check if the blend shape is for adjust (cloth adjust or adjust body)
						else if (blendShapeName.StartsWith(ClothAdjustPrefix + clothName) || 
								blendShapeName.StartsWith(ClothAdjustBodyPrefix + clothName))
						{
							// Determine the prefix (adjust or adjust body)
							var prefix = blendShapeName.StartsWith(ClothAdjustPrefix + clothName) ? ClothAdjustPrefix : ClothAdjustBodyPrefix;

							var boolParam = fxLayer.BoolParameter(prefix + clothName);
							var floatParam = fxLayer.FloatParameter(prefix + clothName + "-float");

							// add parameter driver for adjust function
							var driverEntry = new VRC_AvatarParameterDriver.Parameter
							{
								type = VRC_AvatarParameterDriver.ChangeType.Copy,
								source = boolParam.Name,
								name = floatParam.Name
							};
							drivers.parameters.Add(driverEntry);

							// add adjust blend shape to the blend tree
							adjustTree.AddChild(BlendshapeTree(fxTreeLayer, skin, prefix + clothName, floatParam));
						}
					}
				}
			}
		}

		void SetupClothLayer(AacFlLayer layer, string[] clothNames, SkinnedMeshRenderer skin, string togglePrefix, string adjustBodyPrefix)
		{
			var waitingState = layer.NewState("Waiting command").WithWriteDefaultsSetTo(true);
			var waitingTransition = layer.AnyTransitionsTo(waitingState).WithTransitionToSelf();
			var resetDriver = waitingState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
			var clothStates = new Dictionary<string, AacFlState>();

			foreach (var clothName in clothNames)
			{
				var clothState = layer.NewState(clothName).WithWriteDefaultsSetTo(true);
				var driver = clothState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();

				AddParameterIfBlendShapeExists(resetDriver, skin, togglePrefix + clothName, 0);
				AddParameterIfBlendShapeExists(resetDriver, skin, adjustBodyPrefix + clothName, 0);

				AddParameterIfBlendShapeExists(driver, skin, togglePrefix + clothName, 1);
				AddParameterIfBlendShapeExists(driver, skin, adjustBodyPrefix + clothName, 1);

				foreach (var otherClothName in clothNames)
				{
					if (otherClothName != clothName)
					{
						AddParameterIfBlendShapeExists(driver, skin, togglePrefix + otherClothName, 0);
						AddParameterIfBlendShapeExists(driver, skin, adjustBodyPrefix + otherClothName, 0);
					}
				}

				clothStates[clothName] = clothState;

				waitingTransition.When(layer.BoolParameter(togglePrefix + clothName).IsFalse());
				layer.AnyTransitionsTo(clothState).When(layer.BoolParameter(togglePrefix + clothName).IsTrue());
			}
		}

		void SetupClothLayerWithAdjustments(AacFlLayer layer, string[] clothNames, SkinnedMeshRenderer skin, string togglePrefix, string adjustPrefix, string adjustBodyPrefix)
		{
			var waitingState = layer.NewState("Waiting command").WithWriteDefaultsSetTo(true);
			var waitingTransition = layer.AnyTransitionsTo(waitingState).WithTransitionToSelf();
			var resetDriver = waitingState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
			var clothStates = new Dictionary<string, AacFlState>();
			var adjustClothStates = new Dictionary<string, AacFlState>();

			foreach (var clothName in clothNames)
			{
				var clothState = layer.NewState(clothName).WithWriteDefaultsSetTo(true);
				var driver = clothState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
				var adjustClothState = layer.NewState(clothName + " adjusted").WithWriteDefaultsSetTo(true);
				var adjustDriver = adjustClothState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();

				AddParameterIfBlendShapeExists(resetDriver, skin, togglePrefix + clothName, 0);
				AddParameterIfBlendShapeExists(resetDriver, skin, adjustPrefix + clothName, 0);
				AddParameterIfBlendShapeExists(resetDriver, skin, adjustBodyPrefix + clothName, 0);

				AddParameterIfBlendShapeExists(driver, skin, togglePrefix + clothName, 1);
				AddParameterIfBlendShapeExists(driver, skin, adjustPrefix + clothName, 0);
				AddParameterIfBlendShapeExists(driver, skin, adjustBodyPrefix + clothName, 1);

				AddParameterIfBlendShapeExists(adjustDriver, skin, togglePrefix + clothName, 1);
				AddParameterIfBlendShapeExists(adjustDriver, skin, adjustPrefix + clothName, 1);
				AddParameterIfBlendShapeExists(adjustDriver, skin, adjustBodyPrefix + clothName, 1);

				foreach (var otherClothName in clothNames)
				{
					if (otherClothName != clothName)
					{
						AddParameterIfBlendShapeExists(driver, skin, togglePrefix + otherClothName, 0);
						AddParameterIfBlendShapeExists(driver, skin, adjustPrefix + otherClothName, 0);
						AddParameterIfBlendShapeExists(driver, skin, adjustBodyPrefix + otherClothName, 0);

						AddParameterIfBlendShapeExists(adjustDriver, skin, togglePrefix + otherClothName, 0);
						AddParameterIfBlendShapeExists(adjustDriver, skin, adjustPrefix + otherClothName, 0);
						AddParameterIfBlendShapeExists(adjustDriver, skin, adjustBodyPrefix + otherClothName, 0);
					}
				}

				clothStates[clothName] = clothState;
				adjustClothStates[clothName] = adjustClothState;

				// Transitions
				waitingTransition.When(layer.BoolParameter(togglePrefix + clothName).IsFalse());
				var clothTransition = layer.AnyTransitionsTo(clothState).When(layer.BoolParameter(togglePrefix + clothName).IsTrue());
				foreach (var upperClothName in ClothUpperBodyNames)
				{
					clothTransition.And(layer.BoolParameter(togglePrefix + upperClothName).IsFalse());
				}

				// for adjust states
				foreach (var upperClothName in ClothUpperBodyNames)
				{
					layer.AnyTransitionsTo(adjustClothState)
						.When(layer.BoolParameter(togglePrefix + clothName).IsTrue())
						.And(layer.BoolParameter(togglePrefix + upperClothName).IsTrue());
				}
			}
		}

		void AddParameterIfBlendShapeExists(VRCAvatarParameterDriver driver, SkinnedMeshRenderer skin, string paramName, float value)
		{
			for (int i = 0; i < skin.sharedMesh.blendShapeCount; i++)
			{
				if (skin.sharedMesh.GetBlendShapeName(i).StartsWith(paramName))
				{
					driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
					{
						name = paramName,
						type = VRC_AvatarParameterDriver.ChangeType.Set,
						value = value
					});
					return;
				}
			}
		}

		// Color Customization
        if (createColorCustomization)
		{
			var tree = masterTree.CreateBlendTreeChild(0);
			tree.name = "Color Customization";
			tree.blendType = BlendTreeType.Direct;

			// color changing
			tree.AddChild(Subtree(new[] { primaryColor0, primaryColor1 },
				new[] { 0f, 1f },
				CreateFloatParam(fxTreeLayer, shapePreferenceSliderPrefix + "pcol", true, 0)));

			tree.AddChild(Subtree(new[] { secondColor0, secondColor1 },
				new[] { 0f, 1f },
				CreateFloatParam(fxTreeLayer, shapePreferenceSliderPrefix + "scol", true, 0)));
		}

		// Eye Tracking
		if (createEyeTracking)
		{
			var AdditiveLayer = _aac.CreateMainIdleLayer();
			var EyeLeftLayer = _aac.CreateSupportingIdleLayer("Eye Left Tracking").WithAvatarMask(EyeLeftMask);
			var EyeRightLayer = _aac.CreateSupportingIdleLayer("Eye Right Tracking").WithAvatarMask(EyeRightMask);

			AacFlBoolParameter etActiveParam = CreateBoolParam(AdditiveLayer, ftPrefix + "EyeTrackingActive", true, false);
			AacFlFloatParameter etBlendParam = AdditiveLayer.FloatParameter(ftPrefix + "EyeTrackingActive-float");
			AacFlFloatParameter EyeXParam = CreateFloatParam(AdditiveLayer, ftPrefix + "EyeX", false, 0.0f);
			AacFlFloatParameter EyeYParam = CreateFloatParam(AdditiveLayer, ftPrefix + "EyeY", false, 0.0f);

			foreach (string side in new[] { "Left", "Right" })
			{
				var layer = side == "Left" ? EyeLeftLayer : EyeRightLayer;
				Motion[] poses = side == "Left" || MirrorEyeposes ? LeftEyePoses : RightEyePoses;

				if (poses == null || poses.Length != 9)
					throw new Exception($"The {side} eye poses array must contain exactly 9 motions!");

				// VRC Eye Control State
				var VRCEyeControlState = layer.NewState($"VRC Eye {side} Control")
					.WithWriteDefaultsSetTo(true)
					.Drives(etBlendParam, 0.0f)
					.TrackingTracks(AacFlState.TrackingElement.Eyes);

				// Eye Tracking Tree
				var EyeTrackingTree = _aac.NewBlendTreeAsRaw();
				EyeTrackingTree.name = $"Eye {side} Tracking";
				EyeTrackingTree.blendType = BlendTreeType.FreeformCartesian2D;
				EyeTrackingTree.blendParameter = EyeXParam.Name;
				EyeTrackingTree.blendParameterY = EyeYParam.Name;

				AddEyeTrackingMotions(EyeTrackingTree, maxEyeMotionValue, poses);

				// Eye Tracking State
				var EyeTrackingState = layer.NewState($"Eye {side} Tracking")
					.WithAnimation(EyeTrackingTree)
					.WithWriteDefaultsSetTo(true)
					.Drives(etBlendParam, 1.0f)
					.TrackingAnimates(AacFlState.TrackingElement.Eyes);

				// Transitions
				layer.AnyTransitionsTo(VRCEyeControlState)
				.When(etActiveParam.IsFalse());

				layer.AnyTransitionsTo(EyeTrackingState)
				.WithTransitionToSelf()
				.When(etActiveParam.IsTrue());
			}

			// Functions for adding motions and managing child arrays
			void AddEyeTrackingMotions(BlendTree tree, float maxMotionValue, Motion[] poses)
			{
				var positions = new[]
				{
					new Vector2(-maxMotionValue, maxMotionValue), // LeftUp
					new Vector2(0, maxMotionValue), // Up
					new Vector2(maxMotionValue, maxMotionValue), // RightUp
					new Vector2(-maxMotionValue, 0), // Left
					Vector2.zero, // Neutral
					new Vector2(maxMotionValue, 0), // Right
					new Vector2(-maxMotionValue, -maxMotionValue), // LeftDown
					new Vector2(0, -maxMotionValue), // Down
					new Vector2(maxMotionValue, -maxMotionValue) // RightDown
				};

				for (int i = 0; i < poses.Length; i++)
				{
					if (poses[i] == null)
						throw new Exception($"Eye tracking animation at index {i} is missing!");

					var child = new ChildMotion
					{
						motion = poses[i],
						position = positions[i],
						timeScale = 1f
					};
					tree.children = AppendChild(tree.children, child);
				}
			}

			ChildMotion[] AppendChild(ChildMotion[] children, ChildMotion child)
			{
				var newChildren = new ChildMotion[children.Length + 1];
				Array.Copy(children, newChildren, children.Length);
				newChildren[children.Length] = child;
				return newChildren;
			}
		}

		// Face Tracking
		if (createFaceTracking)
		{
			var layer = _aac.CreateSupportingFxLayer("face animations toggle").WithAvatarMask(fxMask);

			// Clip for zeroing all FT blendshapes
			var offClip = _aac.NewClip("zero all FT blendshapes");
			for (var i = 0; i < skin.sharedMesh.blendShapeCount; i++)
			{
				string blendShapeName = skin.sharedMesh.GetBlendShapeName(i);
				if (blendShapeName.StartsWith(ftPrefix))
				{
					offClip.BlendShape(skin, blendShapeName, 0);
				}
			}

			// State "face tracking off"
			var offFaceTrackingState = layer.NewState("face tracking off")
				.Drives(ftBlendParam, 0).WithWriteDefaultsSetTo(true)
				.TrackingAnimates(AacFlState.TrackingElement.Mouth);
			offFaceTrackingState.WithAnimation(offClip);

			// State "face tracking on"
			var onFaceTrackingState = layer.NewState("face tracking on")
				.Drives(ftBlendParam, 1).WithWriteDefaultsSetTo(true)
				.TrackingAnimates(AacFlState.TrackingElement.Mouth);

			if (createFTLipSyncControl)
			{
				// State "face tracking off [LipSync]"
				var offFaceTrackingLipSyncState = layer.NewState("face tracking off [LipSync]")
				.Drives(ftBlendParam, 0).WithWriteDefaultsSetTo(true)
				.TrackingTracks(AacFlState.TrackingElement.Mouth);
				offFaceTrackingLipSyncState.WithAnimation(offClip);

				// State "face tracking on [LipSync]"
				var onFaceTrackingLipSyncState = layer.NewState("face tracking on [LipSync]")
				.Drives(ftBlendParam, 1).WithWriteDefaultsSetTo(true)
				.TrackingTracks(AacFlState.TrackingElement.Mouth);
				
				// Transitions
				var offFaceTrackingLipSyncTransition = layer.AnyTransitionsTo(offFaceTrackingLipSyncState)
				.When(ftActiveParam.IsFalse())
				.And(LipSyncActiveParam.IsTrue()); 

				var onFaceTrackingLipSyncTransition = layer.AnyTransitionsTo(onFaceTrackingLipSyncState)
				.When(ftActiveParam.IsTrue())
				.And(LipSyncActiveParam.IsTrue()); 

				layer.AnyTransitionsTo(offFaceTrackingState)
					.When(ftActiveParam.IsFalse())
					.And(LipSyncActiveParam.IsFalse());

				layer.AnyTransitionsTo(onFaceTrackingState)
					.When(ftActiveParam.IsTrue())
					.And(LipSyncActiveParam.IsFalse());
			}

			else
			{
				layer.AnyTransitionsTo(offFaceTrackingState)
					.When(ftActiveParam.IsFalse());

				layer.AnyTransitionsTo(onFaceTrackingState)
					.When(ftActiveParam.IsTrue());
			}

			if (createFTreset)
			{
				// State "reset face tracking"
				var resetState = layer.NewState("reset face tracking")
				.WithAnimation(offClip)
				.Drives(ftBlendParam, 0)
				.WithWriteDefaultsSetTo(true);
				var resetDriver = resetState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();

				// Combine FTshapes
				var allShapes = new List<string>();

				foreach (var shape in ftShapes)
				{
				AddShapeToList(shape, allShapes, MirrorFTparams);
				}

				foreach (var dualshape in ftDualShapes)
				{
				AddShapeToList(dualshape.paramName, allShapes, MirrorFTparams);
				}

				void ResetFTparams(string parameterName)
				{
				resetDriver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
				{
					type = VRC_AvatarParameterDriver.ChangeType.Set,
					name = parameterName,
					value = 0
				});
				}

				// Iterate over allShapes and call ResetFTparams for each item
				for (int i = 0; i < allShapes.Count; i++)
				{
				string shape = allShapes[i];
				ResetFTparams(ftPrefix+shape);
				}

				layer.AnyTransitionsTo(resetState)
				.WithTransitionToSelf()
				.When(ResetFTActiveParam.IsTrue());    
			}


			// Tree "face tracking"
			var tree = masterTree.CreateBlendTreeChild(0);
			tree.name = "Face Tracking";
			tree.blendType = BlendTreeType.Direct;
			tree.blendParameter = ftActiveParam.Name;
			tree.blendParameterY = ftActiveParam.Name;

			// adding blend shapes
			for (var i = 0; i < ftShapes.Length; i++)
			{
				string shapeName = ftShapes[i];

				if(MirrorFTparams)
				{
					for (int flip = 0; flip < EachSide(ref shapeName); flip++)
					{
						var param = CreateFloatParam(fxLayer, ftPrefix + shapeName, false, 0);
						tree.AddChild(BlendshapeTree(fxTreeLayer, skin, param));
					}
				}

				else
				{
					var param = CreateFloatParam(fxLayer, ftPrefix + shapeName, false, 0);
					tree.AddChild(BlendshapeTree(fxTreeLayer, skin, param));
				}
			}

			// adding dual blend shapes
			for (var i = 0; i < ftDualShapes.Length; i++)
			{
				DualShape shape = ftDualShapes[i];

				if(MirrorFTparams)
				{
					string flippedParamName = shape.paramName;

					for (int flip = 0; flip < EachSide(ref flippedParamName); flip++)
					{
						var param = CreateFloatParam(fxLayer, ftPrefix + flippedParamName, false, 0);

						tree.AddChild(DualBlendshapeTree(fxTreeLayer,
							param, skin,
							ftPrefix + shape.minShapeName + GetSide(param.Name),
							ftPrefix + shape.maxShapeName + GetSide(param.Name),
							shape.minValue, shape.neutralValue, shape.maxValue));
					}
				}	
							
				else
				{
					var param = CreateFloatParam(fxLayer, ftPrefix + shape.paramName, false, 0);
					tree.AddChild(DualBlendshapeTree(
						fxTreeLayer, param, skin,
						ftPrefix + shape.minShapeName,
						ftPrefix + shape.maxShapeName,
						shape.minValue, shape.neutralValue, shape.maxValue));
				}				

			}

			var children = masterTree.children;
			children[children.Length - 1].directBlendParameter = ftBlendParam.Name;
			masterTree.children = children;

			if (createOSCsmooth)
			{
				var OSCLayer = _aac.CreateSupportingFxLayer("OSC smoothing").WithAvatarMask(fxMask);

				// The main OSC trees 
				var OSCLocalTree = _aac.NewBlendTreeAsRaw();
				OSCLocalTree.name = "OSC Local";
				OSCLocalTree.blendType = BlendTreeType.Direct;
				var OSCLocalState = OSCLayer.NewState(OSCLocalTree.name)
					.WithAnimation(OSCLocalTree)
					.WithWriteDefaultsSetTo(true);

				var OSCRemoteTree = _aac.NewBlendTreeAsRaw();
				OSCRemoteTree.name = "OSC Remote";
				OSCRemoteTree.blendType = BlendTreeType.Direct;
				var OSCRemoteState = OSCLayer.NewState(OSCRemoteTree.name)
					.WithAnimation(OSCRemoteTree)
					.WithWriteDefaultsSetTo(true);

				// Combine FTshapes
				var allShapes = new List<string>();	
							
				foreach (var shape in ftShapes)
				{
					AddShapeToList(shape, allShapes, MirrorFTparams);
				}

				foreach (var dualshape in ftDualShapes)
				{
					AddShapeToList(dualshape.paramName, allShapes, MirrorFTparams);
				}

				// Function for creating trees
				void CreateOSCTrees(string type, BlendTree rootTree, float smoothness)
				{
					foreach (var shape in allShapes)
					{
						// Params
						var inputParamName = $"{ftPrefix}{shape}";
						var smootherParamName = $"OSCsmooth/{type}/{ftPrefix}{shape}Smoother";
						var driverParamName = $"OSCsmooth/Proxy/{ftPrefix}{shape}";

						AacFlFloatParameter smootherParam = OSCLayer.FloatParameter(smootherParamName);
						AacFlFloatParameter driverParam = OSCLayer.FloatParameter(driverParamName);

						OSCLayer.OverrideValue(smootherParam, 0.0f);
						OSCLayer.OverrideValue(driverParam, 0.0f);

						// Replace params in the FT tree
						foreach (var child in masterTree.children)
						{
							if (child.motion is BlendTree blendTree)
							{
								ReplaceBlendTreeParameter(blendTree, inputParamName, driverParam.Name);
							}
						}

						var inputParam = OSCLayer.FloatParameter(inputParamName);

						// Root Tree
						var rootSubTree = rootTree.CreateBlendTreeChild(0);
						rootSubTree.name = $"OSCsmooth/{type}/{ftPrefix}{shape}Smoother";
						rootSubTree.blendType = BlendTreeType.Simple1D;
						rootSubTree.useAutomaticThresholds = false;
						rootSubTree.blendParameter = smootherParam.Name;

						// Input Tree
						var inputTree = rootSubTree.CreateBlendTreeChild(0);
						inputTree.name = $"OSCsmooth Input ({ftPrefix}{shape})";
						inputTree.blendType = BlendTreeType.Simple1D;
						inputTree.useAutomaticThresholds = false;
						inputTree.blendParameter = inputParam.Name;

						var clipMin = _aac.NewClip($"Animator.OSCsmooth/Proxy/{ftPrefix}{shape}_Min")
							.Animating(anim => anim.AnimatesAnimator(driverParam).WithFixedSeconds(0.0f, -1.0f));
						var clipMax = _aac.NewClip($"Animator.OSCsmooth/Proxy/{ftPrefix}{shape}_Max")
							.Animating(anim => anim.AnimatesAnimator(driverParam).WithFixedSeconds(0.0f, 1.0f));

						inputTree.AddChild(clipMin.Clip, -1.0f);
						inputTree.AddChild(clipMax.Clip, 1.0f);

						// Driver Tree
						var driverTree = rootSubTree.CreateBlendTreeChild(1);
						driverTree.name = $"OSCsmooth Driver ({ftPrefix}{shape})";
						driverTree.blendType = BlendTreeType.Simple1D;
						driverTree.useAutomaticThresholds = false;
						driverTree.blendParameter = driverParam.Name;

						driverTree.AddChild(clipMin.Clip, -1.0f);
						driverTree.AddChild(clipMax.Clip, 1.0f);
					}
				}

				CreateOSCTrees("Local", OSCLocalTree, localSmoothness);
				CreateOSCTrees("Remote", OSCRemoteTree, remoteSmoothness);

				OSCLocalState.TransitionsTo(OSCRemoteState).When(OSCLayer.BoolParameter("IsLocal").IsFalse());
				OSCRemoteState.TransitionsTo(OSCLocalState).When(OSCLayer.BoolParameter("IsLocal").IsTrue());

				void ReplaceBlendTreeParameter(BlendTree tree, string oldParam, string newParam)
				{
					if (tree.blendParameter == oldParam)
					{
						tree.blendParameter = newParam;
					}

					if (tree.blendParameterY == oldParam)
					{
						tree.blendParameterY = newParam;
					}

					foreach (var child in tree.children)
					{
						if (child.motion is BlendTree childTree)
						{
							ReplaceBlendTreeParameter(childTree, oldParam, newParam);
						}
					}
				}
			}

			// will save your VRC Parameters if True
			if(!saveVRCExpressionParameters)
			{
				// add all the new avatar params to the avatar descriptor
				avatar.expressionParameters.parameters = _vrcParams.ToArray();
				EditorUtility.SetDirty(avatar.expressionParameters);
			}	

		}
	}

	private BlendTree BlendshapeTree(AacFlLayer layer, SkinnedMeshRenderer skin, AacFlParameter param, 
		float min = 0, float max = 100)
	{
		return BlendshapeTree(layer, skin, param.Name, param, min, max);
	}

	private BlendTree BlendshapeTree(AacFlLayer layer, SkinnedMeshRenderer skin, string shapeName, AacFlParameter param,
	float min = 0, float max = 100)
	{
		var state000 = _aac.NewClip().BlendShape(skin, shapeName, min);
		state000.Clip.name = param.Name + ":0";

		var state100 = _aac.NewClip().BlendShape(skin, shapeName, max);
		state100.Clip.name = param.Name + ":1";

		return Subtree(new Motion[] { state000.Clip, state100.Clip }, new[] { 0f, 1f },
			param);
	}

	private void MapHandPosesToShapes(string layerName, SkinnedMeshRenderer skin,
		string[] shapeNames, string prefix, bool rightHand, AacFlBoolParameter ftActiveParam, AacFlBoolParameter ExpTrackActiveParam, AacFlBoolParameter FaceToggleActiveParam)
	{
		var layer = _aac.CreateSupportingFxLayer(layerName).WithAvatarMask(fxMask);
		var Gesture = layer.IntParameter("Gesture" + (rightHand ? Right : Left));

		List<string> allPossibleExpressions = new List<string>();

		foreach (var shapeName in shapeNames)
		{
			if (!allPossibleExpressions.Contains(shapeName))
				allPossibleExpressions.Add(shapeName);
		}

		if (shapeNames.Length != 8)
			throw new Exception("Number of face poses must equal number of hand gestures (8)!");

		for (int i = 0; i < shapeNames.Length; i++)
		{
			var clip = _aac.NewClip();

			foreach (var shapeName in shapeNames)
			{
				clip.BlendShape(skin, prefix + shapeName, shapeName == shapeNames[i] ? 100 : 0);
			}

			var state = layer.NewState(shapeNames[i], 1, i)
				.WithAnimation(clip)
				.WithWriteDefaultsSetTo(true);

			var enter = layer.EntryTransitionsTo(state)
				.When(Gesture.IsEqualTo(i));
			var exit = state.Exits()
				.WithTransitionDurationSeconds(TransitionSpeed)
				.When(Gesture.IsNotEqualTo(i));

			if (createFaceTracking)
			{
				if (i == 0)
				{
					enter.Or().When(ftActiveParam.IsTrue());
					exit.And(ftActiveParam.IsFalse());
				}
				else
				{
					enter.And(ftActiveParam.IsFalse());
					exit.Or().When(ftActiveParam.IsTrue());
				}
			}
			if (createFacialExpressionsControl)
			{
				if (i == 0)
				{
					enter.Or().When(ExpTrackActiveParam.IsFalse());
					exit.And(ExpTrackActiveParam.IsTrue());
				}
				else
				{
					enter.And(ExpTrackActiveParam.IsTrue());
					exit.Or().When(ExpTrackActiveParam.IsFalse());
				}
			}
			
			if (createFaceToggleControl)
			{
				if (i == 0)
				{
					enter.Or().When(FaceToggleActiveParam.IsTrue());
					exit.And(FaceToggleActiveParam.IsFalse());
				}
				else
				{
					enter.And(FaceToggleActiveParam.IsFalse());
					exit.Or().When(FaceToggleActiveParam.IsTrue());
				}
			}
		}
	}

	private BlendTree DualBlendshapeTree(
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


	private BlendTree Subtree(Motion[] motions, float[] thresholds, AacFlParameter param)
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


	private AacFlFloatParameter CreateFloatParam(AacFlLayer layer, string paramName, bool save, float val)
	{
		CreateFloatParamVrcOnly(paramName, save, val);

		return layer.FloatParameter(paramName);
	}


	private void CreateFloatParamVrcOnly(string paramName, bool save, float val)
	{

		{
				_vrcParams.Add(new VRCExpressionParameters.Parameter()
				{
					name = paramName,
					valueType = VRCExpressionParameters.ValueType.Float,
					saved = save,
					networkSynced = true,
					defaultValue = val,
				});
		}
	}

	private AacFlBoolParameter CreateBoolParam(AacFlLayer layer, string paramName, bool save, bool val)
	{
		// will save your VRCParams if True
		if(!saveVRCExpressionParameters)
		{
				_vrcParams.Add(new VRCExpressionParameters.Parameter()
				{
					name = paramName,
					valueType = VRCExpressionParameters.ValueType.Bool,
					saved = save,
					networkSynced = true,
					defaultValue = val ? 1 : 0,
				});
		}
		
		return layer.BoolParameter(paramName);
	}

	private BlendTree Create1DTree(string paramName, float min, float max)
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

	private static int EachSide(ref string str)
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

	private static string GetSide(string str)
	{
		if (str.EndsWith(Right))
			return Right;
		if (str.EndsWith(Left))
			return Left;
		return "";
	}

	private static void AddShapeToList(string shapeName, List<string> allShapes, bool mirrorFTparams)
	{
		if (mirrorFTparams)
		{
			for (int flip = 0; flip < EachSide(ref shapeName); flip++)
			{
				allShapes.Add(shapeName);
			}
		}
		else
		{
			allShapes.Add(shapeName);
		}
	}
}

[CustomEditor(typeof(AnimatorWizard), true)]
public class AnimatorGeneratorEditor : Editor
{
	private SerializedProperty saveVRCExpressionParameters, MirrorFTparams, MirrorHandposes, MirrorEyeposes;

	private SerializedProperty assetContainer;

	private SerializedProperty fxMask, EyeLeftMask, EyeRightMask, gestureMask, lMask, rMask;

	private SerializedProperty LeftHandPoses, RightHandPoses;

	private SerializedProperty createShapePreferences, createColorCustomization, createFaceTracking, createClothCustomization, createEyeTracking;

	private SerializedProperty createFacialExpressionsControl, createFTLipSyncControl, createFaceToggleControl, createFTreset, createOSCsmooth;

	private SerializedProperty localSmoothness, remoteSmoothness;

	private SerializedProperty shapePreferenceSliderPrefix, shapePreferenceTogglesPrefix, mouthPrefix, browPrefix, ftPrefix, ClothTogglesPrefix, ClothAdjustPrefix, ClothAdjustBodyPrefix;

	private SerializedProperty primaryColor0, primaryColor1, secondColor0, secondColor1;

	private SerializedProperty maxEyeMotionValue;

	private SerializedProperty LeftEyePoses, RightEyePoses;

	private SerializedProperty mouthShapeNames, browShapeNames, expTrackName, ClothUpperBodyNames, ClothLowerBodyNames, ClothFootNames;

	private SerializedProperty lipSyncName, faceToggleName, resetFTName, ftShapes, ftDualShapes;

	private AnimatorWizard wizard;
	
	private void OnEnable()
	{

		wizard = (AnimatorWizard)target;
		saveVRCExpressionParameters = serializedObject.FindProperty("saveVRCExpressionParameters");
		MirrorFTparams = serializedObject.FindProperty("MirrorFTparams");
		MirrorHandposes = serializedObject.FindProperty("MirrorHandposes");
		MirrorEyeposes = serializedObject.FindProperty("MirrorEyeposes");

		assetContainer = serializedObject.FindProperty("assetContainer");

		fxMask = serializedObject.FindProperty("fxMask");
		EyeLeftMask = serializedObject.FindProperty("EyeLeftMask");
		EyeRightMask = serializedObject.FindProperty("EyeRightMask");
		gestureMask = serializedObject.FindProperty("gestureMask");
		lMask = serializedObject.FindProperty("lMask");
		rMask = serializedObject.FindProperty("rMask");

		LeftHandPoses = serializedObject.FindProperty("LeftHandPoses");
		RightHandPoses = serializedObject.FindProperty("RightHandPoses");

		createShapePreferences = serializedObject.FindProperty("createShapePreferences");
		createColorCustomization = serializedObject.FindProperty("createColorCustomization");
		createFaceTracking = serializedObject.FindProperty("createFaceTracking");
		createClothCustomization = serializedObject.FindProperty("createClothCustomization");
		createEyeTracking = serializedObject.FindProperty("createEyeTracking");
		createFacialExpressionsControl = serializedObject.FindProperty("createFacialExpressionsControl");
		createFTLipSyncControl = serializedObject.FindProperty("createFTLipSyncControl");
		createFaceToggleControl = serializedObject.FindProperty("createFaceToggleControl");
		createFTreset = serializedObject.FindProperty("createFTreset");

		createOSCsmooth = serializedObject.FindProperty("createOSCsmooth");
		localSmoothness = serializedObject.FindProperty("localSmoothness");
		remoteSmoothness = serializedObject.FindProperty("remoteSmoothness");

		shapePreferenceSliderPrefix = serializedObject.FindProperty("shapePreferenceSliderPrefix");
		shapePreferenceTogglesPrefix = serializedObject.FindProperty("shapePreferenceTogglesPrefix");
		mouthPrefix = serializedObject.FindProperty("mouthPrefix");
		browPrefix = serializedObject.FindProperty("browPrefix");
		ftPrefix = serializedObject.FindProperty("ftPrefix");
		ClothTogglesPrefix = serializedObject.FindProperty("ClothTogglesPrefix");
		ClothAdjustPrefix = serializedObject.FindProperty("ClothAdjustPrefix");
        ClothAdjustBodyPrefix = serializedObject.FindProperty("ClothAdjustBodyPrefix");

        expTrackName = serializedObject.FindProperty("expTrackName");
		lipSyncName = serializedObject.FindProperty("lipSyncName");
		faceToggleName = serializedObject.FindProperty("faceToggleName");
		resetFTName = serializedObject.FindProperty("resetFTName");
		mouthShapeNames = serializedObject.FindProperty("mouthShapeNames");
		browShapeNames = serializedObject.FindProperty("browShapeNames");
        ClothUpperBodyNames = serializedObject.FindProperty("ClothUpperBodyNames");
        ClothLowerBodyNames = serializedObject.FindProperty("ClothLowerBodyNames");
        ClothFootNames = serializedObject.FindProperty("ClothFootNames");

        primaryColor0 = serializedObject.FindProperty("primaryColor0");
		secondColor0 = serializedObject.FindProperty("secondColor0");
		primaryColor1 = serializedObject.FindProperty("primaryColor1");
		secondColor1 = serializedObject.FindProperty("secondColor1");
		
		maxEyeMotionValue = serializedObject.FindProperty("maxEyeMotionValue");
		LeftEyePoses = serializedObject.FindProperty("LeftEyePoses");
		RightEyePoses = serializedObject.FindProperty("RightEyePoses");

		ftShapes = serializedObject.FindProperty("ftShapes");
		ftDualShapes = serializedObject.FindProperty("ftDualShapes");

	}

	private const string AlertMsg =
		"Running this will destroy any manual animator changes. Are you sure you want to continue?";

	// On Inspector GUI
	public override void OnInspectorGUI()
	{
		GUIStyle headerStyle = new GUIStyle()
		{
			richText = false,
			fontStyle = FontStyle.Bold,
			fontSize =  EditorStyles.label.fontSize + 5,
			padding = new RectOffset(3, 3, 40, 8),
			normal = new GUIStyleState()
			{
				textColor = EditorStyles.label.normal.textColor
			}
		};
        GUIStyle headerStyle2 = new GUIStyle()
        {
            richText = false,
            fontStyle = FontStyle.Bold,
            fontSize = EditorStyles.label.fontSize + 1,
            normal = new GUIStyleState()
            {
                textColor = EditorStyles.label.normal.textColor
            }
        };

        GUIStyle TipStyle = new GUIStyle()
        {
            richText = false,
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState()
            {
                textColor = Color.gray
            }
        };

		GUIContent PopUpLabel(string PropertyFieldLabel, string label)
		{
			return new GUIContent(PropertyFieldLabel, label);
		}

        GUILayout.Space(10);
		if (GUILayout.Button("Setup animator! (DESTRUCTIVE!!!)", GUILayout.Height(50)))
		{
			if (EditorUtility.DisplayDialog("Animator Wizard", AlertMsg, "yes (DESTRUCTIVE!)", "NO"))
			{
				Create();
			}
		}

		// Save VRC Expression Parameters
		GUILayout.Space(20);
		EditorGUILayout.PropertyField(saveVRCExpressionParameters,
		 PopUpLabel("Save VRC Expression Parameters","Will save your VRC Expression Parameters before setup animator."));

		// Asset Container
		GUILayout.Label("Asset Container", headerStyle);
		EditorGUILayout.PropertyField(assetContainer, 
		PopUpLabel("Asset Container", "Asset Container stores all generated animations and Blend Trees."));

		// Avatar animator masks
		GUILayout.Label("Avatar animator masks", headerStyle);
		EditorGUILayout.PropertyField(fxMask);
		if (wizard.createEyeTracking)
		{	
			EditorGUILayout.PropertyField(EyeLeftMask);
			EditorGUILayout.PropertyField(EyeRightMask);
		}
		EditorGUILayout.PropertyField(gestureMask);
		EditorGUILayout.PropertyField(lMask);
		EditorGUILayout.PropertyField(rMask);
		
		// Hand Poses
		GUILayout.Label("Hand Poses", headerStyle);
		GUILayout.Label("Array index maps to hand gesture parameter. Array length should be 8!");
		EditorGUILayout.PropertyField(MirrorHandposes,PopUpLabel("Mirror Hand Poses", ""));
		GUILayout.Space(10);

		if (wizard.MirrorHandposes)
		{
			EditorGUILayout.PropertyField(LeftHandPoses,PopUpLabel("Hand Poses", ""));
		}

		else
		{
			EditorGUILayout.PropertyField(LeftHandPoses);
			GUILayout.Space(10);
			EditorGUILayout.PropertyField(RightHandPoses);
		}

		// Facial expressions
		GUILayout.Label("Facial expressions", headerStyle);
		GUILayout.Label("Brow and mouth blendshapes controlled by left and right hands. \nArray index maps to hand Gesture parameter. Array length should be 8!");
		GUILayout.Space(10);
		EditorGUILayout.PropertyField(createFacialExpressionsControl, 
		PopUpLabel("Create Facial Expressions Control", "When created, adds a parameter to the VRC to disable/enable expression binding to hands."));
		EditorGUILayout.PropertyField(createFaceToggleControl,
		 PopUpLabel("Сreate Face Toggle Control", "If you use your Face presets for avatar," + 
			"it disable expression binding to hands when “FaceToggleActive” is true (turn it on in the VRC Parameter Driver when Face preset true and turn it off when false)." + 
			"It also adds a parameter to the VRC."));
		GUILayout.Space(10);
		EditorGUILayout.PropertyField(mouthPrefix);
		EditorGUILayout.PropertyField(mouthShapeNames);
		GUILayout.Space(20);
		EditorGUILayout.PropertyField(browPrefix);
		EditorGUILayout.PropertyField(browShapeNames);
		
		// Animator creation flags
		GUILayout.Label("Animator creation flags", headerStyle);
		GUILayout.Label("Choose what parts of the animator are generated. Disabling features saves VRC params budget!");
        GUILayout.Space(10);
        EditorGUILayout.PropertyField(createShapePreferences);
		EditorGUILayout.PropertyField(createClothCustomization);
		EditorGUILayout.PropertyField(createColorCustomization);
		EditorGUILayout.PropertyField(createEyeTracking);
		EditorGUILayout.PropertyField(createFaceTracking);

		// Shape Preferences
		if (wizard.createShapePreferences)
		{
			GUILayout.Label("Shape Preferences", headerStyle);
			GUILayout.Label("Animator wizard will automatically create VRC params for blendshapes with these prefixes.");
			GUILayout.Space(10);
			EditorGUILayout.PropertyField(shapePreferenceSliderPrefix);
			EditorGUILayout.PropertyField(shapePreferenceTogglesPrefix);
		}

		// Cloths customization
        if (wizard.createClothCustomization)
        {
            GUILayout.Label("Cloths customization", headerStyle);
			GUILayout.Label("Animator wizard will create animations, algorithm to switch clothes \nand VRC params with these prefixes.");
			GUILayout.Space(10);
			EditorGUILayout.PropertyField(ClothTogglesPrefix, 
			PopUpLabel("Cloth Toggles Prefix", "The prefixes ClothToggles roll up clothes into a \"tube\"."));
			EditorGUILayout.PropertyField(ClothAdjustPrefix, 
			PopUpLabel("Cloth Adjust Prefix", "ClothAdjustPrefix regulates the fit of the cloth lower body to the cloth upper body."));
            EditorGUILayout.PropertyField(ClothAdjustBodyPrefix, 
			PopUpLabel("Cloth Adjust Body Prefix", "The prefixes ClothAdjustBody roll up body into a \"tube\"."));
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(ClothUpperBodyNames);
            EditorGUILayout.PropertyField(ClothLowerBodyNames);
            EditorGUILayout.PropertyField(ClothFootNames);
        }
		
		// Color customization
        if (wizard.createColorCustomization)
		{
			GUILayout.Label("Color customization UV-offset animations", headerStyle);
			GUILayout.Label("Animations controlling color palette texture UV-offsets for in-game color customization.");
			EditorGUILayout.PropertyField(primaryColor0);
			EditorGUILayout.PropertyField(primaryColor1);
			EditorGUILayout.PropertyField(secondColor0);
			EditorGUILayout.PropertyField(secondColor1);
		}

		// EyeTracking
		if (wizard.createEyeTracking)
			{		
				GUILayout.Label("EyeTracking (Simplified Eye Parameters).", headerStyle);
				GUILayout.Label("Animator wizard will create EyeTracking with these animations.");
				if(!wizard.createFaceTracking)
				{
					GUILayout.Space(10);
					EditorGUILayout.PropertyField(ftPrefix);
				}
				EditorGUILayout.PropertyField(MirrorEyeposes);
				GUILayout.Space(10);
				if(wizard.MirrorEyeposes)
				{
					EditorGUILayout.PropertyField(LeftEyePoses, PopUpLabel("Eye Poses",""));
				}
				else
				{
					EditorGUILayout.PropertyField(LeftEyePoses);
					EditorGUILayout.PropertyField(RightEyePoses);					
				}
			}

		// FaceTracking
		if (wizard.createFaceTracking)
		{
			GUILayout.Label("FaceTracking (Universal Shapes) settings", headerStyle);
			GUILayout.Space(10);
			EditorGUILayout.PropertyField(ftPrefix);
			GUILayout.Space(10);
			EditorGUILayout.PropertyField(createFTLipSyncControl,
			PopUpLabel("Create Face Tracking LipSync Control", "Adds LypSync off/on feature."));
			EditorGUILayout.PropertyField(createFTreset, 
			PopUpLabel("Create Reset Face Tracking","A parameter that resets blendshapes and parameters in VRChat when rare bugs occur."));
			GUILayout.Space(10);
			EditorGUILayout.PropertyField(MirrorFTparams,
			 PopUpLabel("Mirroring shapes", "Reflect automatically blendshapes if they have “Left” in their name (for example “MouthLowerDownLeft”)." + 
			 " You don't need to write the same blendshape for the right side (i.e. write only “MouthLowerDownLeft” and it will automatically create one for the right side as well)."));
			GUILayout.Space(10);
			EditorGUILayout.PropertyField(createOSCsmooth,
			 PopUpLabel("Create OSC smooth", "OSC smooth is needed to fix Face Tracking params, as without it animation is choppy and jerky, as if it's lacking FPS"));
			if(wizard.createOSCsmooth)
			{
			EditorGUILayout.PropertyField(localSmoothness);
			EditorGUILayout.PropertyField(remoteSmoothness);
			}
			GUILayout.Space(10);
			EditorGUILayout.PropertyField(ftShapes, PopUpLabel("FT Single Shapes","Single shapes controlled by a float parameter."));
			GUILayout.Space(10);
			EditorGUILayout.PropertyField(ftDualShapes, PopUpLabel("FT Dual Shapes", "Mutually exclusive shape pairs controlled by a single float parameter."));
		}

		serializedObject.ApplyModifiedProperties();
	}

	private void Create()
	{
		((AnimatorWizard)target).Create();
	}
}
#endif
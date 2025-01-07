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
	public AvatarMask EyeMask;
	public AvatarMask gestureMask;
	public AvatarMask lMask;
	public AvatarMask rMask;
	
	public Motion[] handPoses;
	
	public bool createShapePreferences = true;
	public string shapePreferenceSliderPrefix = "pref/slider/";
	public string shapePreferenceTogglesPrefix = "pref/toggle/";

	public bool createClothCustomization = true;
	public string ClothTogglesPrefix = "cloth/toggle/";
	public string ClothAdjustPrefix = "cloth/adjust/";
    public string ClothAdjustBodyPrefix = "cloth/adjust/Body/with/"; // need to combine it with ClothAdjustPrefix.


    public bool createColorCustomization = true;
	public Motion primaryColor0;
	public Motion primaryColor1;
	public Motion secondColor0;
	public Motion secondColor1;

	public bool createFacialExpressionsControl = false;
	public string expTrackName = "ExpressionTrackingActive";

	public bool createFaceTrackingLipSyncControl = false;
	public string lipSyncName = "LipSyncTrackingActive";

	public bool createFaceToggleControl = false;
	public string faceToggleName = "FaceToggleActive";

	public bool createParamForResetFaceTracking = false;
	public string resetFTName = "Reset";

	public bool saveVRCExpressionParameters = false;
	public bool MirroringParam = false;
	
	public bool createOSCsmooth = true;
	public bool IsLocal = false;
	public float localSmoothness = 0.1f;
    public float remoteSmoothness = 0.7f;

	public bool createEyeTracking = true;

	public Motion EyeLookDownLeftRot;
	public Motion EyeLookInDownLeftRot;
	public Motion EyeLookInLeftRot;
	public Motion EyeLookInUpLeftRot;
	public Motion EyeLookNeutralLeftRot;
	public Motion EyeLookOutDownLeftRot;
	public Motion EyeLookOutLeftRot;
	public Motion EyeLookOutUPLeftRot;
	public Motion EyeLookUpLeftRot;

	public Motion EyeLookDownRightRot;
	public Motion EyeLookInDownRightRot;
	public Motion EyeLookInRightRot;
	public Motion EyeLookInUpRightRot;
	public Motion EyeLookNeutralRightRot;
	public Motion EyeLookOutDownRightRot;
	public Motion EyeLookOutRightRot;
	public Motion EyeLookOutUPRightRot;
	public Motion EyeLookUpRightRot;
	
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
		"MouthUpperUpRight",
		"MouthLowerDownLeft",
		"MouthLowerDownRight",
		"MouthRaiserLower",
		"TongueOut",
		"EyeSquintLeft",
		"EyeSquintRight",
	};

	public DualShape[] ftDualShapes =
	{
		new DualShape("SmileSadLeft", "MouthSadLeft", "MouthSmileLeft"),
		new DualShape("SmileSadRight", "MouthSadRight", "MouthSmileRight"),
		new DualShape("JawX", "JawLeft", "JawRight"),
		new DualShape("EyeLidLeft", "EyeClosedLeft", "EyeWideLeft", 0, 0.75f, 1),
		new DualShape("EyeLidRight", "EyeClosedRight", "EyeWideRight", 0, 0.75f, 1),
		new DualShape("BrowExpressionLeft", "BrowDownLeft", "BrowUpLeft"),
		new DualShape("BrowExpressionRight", "BrowDownRight", "BrowUpRight"),
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

		// Gesture layer
		_aac.CreateMainGestureLayer().WithAvatarMask(gestureMask);

		// hand gestures
		foreach (string side in new[] { Left, Right })
		{
			var layer = _aac.CreateSupportingGestureLayer(side + " hand")
				.WithAvatarMask(side == Left ? lMask : rMask);

			var gesture = layer.IntParameter("Gesture" + side);

			if (handPoses.Length != 8)
				throw new Exception("Number of hand poses must equal number of hand gestures (8)!");

			for (int i = 0; i < handPoses.Length; i++)
			{
				Motion motion = handPoses[i];

				var state = layer.NewState(motion.name, 1, i)
					.WithAnimation(motion).WithWriteDefaultsSetTo(true);

				layer.EntryTransitionsTo(state)
					.When(gesture.IsEqualTo(i));
				state.Exits()
					.WithTransitionDurationSeconds(TransitionSpeed)
					.When(gesture.IsNotEqualTo(i));
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

		AacFlBoolParameter ExpTrackActiveParam = CreateBoolParam(fxLayer, expTrackName, true, true);
		AacFlBoolParameter LipSyncActiveParam = CreateBoolParam(fxLayer, lipSyncName, true, false);
		AacFlBoolParameter FaceToggleActiveParam = CreateBoolParam(fxLayer, faceToggleName, false, false);
		AacFlBoolParameter ResetFTActiveParam = CreateBoolParam(fxLayer, ftPrefix + resetFTName, false, false);

		// brow gesture expressions
		MapHandPosesToShapes("brow expressions", skin, browShapeNames, browPrefix, false, ftActiveParam, ExpTrackActiveParam, FaceToggleActiveParam);

		// mouth gesture expressions
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

		// Cloth Customization (WIP)
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

						if (blendShapeName.StartsWith(ClothTogglesPrefix + clothName))
						{
							var boolParam = CreateBoolParam(fxLayer, ClothTogglesPrefix + clothName, true, false);
							var floatParam = fxLayer.FloatParameter(ClothTogglesPrefix + clothName + "-float");

							var driverEntry = new VRC_AvatarParameterDriver.Parameter
							{
								type = VRC_AvatarParameterDriver.ChangeType.Copy,
								source = boolParam.Name,
								name = floatParam.Name
							};
							drivers.parameters.Add(driverEntry);

							// Reversed animations for clothes
							toggleTree.AddChild(BlendshapeTree(fxTreeLayer, skin, ClothTogglesPrefix + clothName, floatParam, max: 0, min: 100));
						}
						else if (blendShapeName.StartsWith(ClothAdjustPrefix + clothName) || 
								blendShapeName.StartsWith(ClothAdjustBodyPrefix + clothName))
						{
							var prefix = blendShapeName.StartsWith(ClothAdjustPrefix + clothName) ? ClothAdjustPrefix : ClothAdjustBodyPrefix;
							var boolParam = CreateBoolParam(fxLayer, prefix + clothName, true, false);
							var floatParam = fxLayer.FloatParameter(prefix + clothName + "-float");

							var driverEntry = new VRC_AvatarParameterDriver.Parameter
							{
								type = VRC_AvatarParameterDriver.ChangeType.Copy,
								source = boolParam.Name,
								name = floatParam.Name
							};
							drivers.parameters.Add(driverEntry);

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

				waitingTransition.When(layer.BoolParameter(togglePrefix + clothName).IsFalse());

				var clothTransition = layer.AnyTransitionsTo(clothState).When(layer.BoolParameter(togglePrefix + clothName).IsTrue());
				foreach (var upperClothName in ClothUpperBodyNames)
				{
					clothTransition.And(layer.BoolParameter(togglePrefix + upperClothName).IsFalse());
				}

				// Transitions for adjust states
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

		void AddParameter(VRCAvatarParameterDriver driver, string name, float value)
		{
			driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
			{
				name = name,
				type = VRC_AvatarParameterDriver.ChangeType.Set,
				value = value
			});
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

		// Eye Tracking (WIP)
		if (createEyeTracking)
		{
			var Eyelayer = _aac.CreateSupportingIdleLayer("Eye Tracking").WithAvatarMask(EyeMask);

			// Creating parameters
			AacFlBoolParameter etActiveParam = CreateBoolParam(Eyelayer, ftPrefix + "EyeTrackingActive", true, false);
			AacFlFloatParameter etBlendParam = Eyelayer.FloatParameter(ftPrefix + "EyeTrackingActive-float");
			AacFlFloatParameter EyeXParam = Eyelayer.FloatParameter(ftPrefix + "EyeX");
			AacFlFloatParameter EyeYParam = Eyelayer.FloatParameter(ftPrefix + "EyeY");

			// "VRC Eye Control" state
			var VRCEyeControlState = Eyelayer.NewState("VRC Eye Control")
				.WithWriteDefaultsSetTo(true)
				.Drives(etBlendParam, 0.0f)
				.TrackingTracks(AacFlState.TrackingElement.Eyes);
			
			// Eye Tracking Tree
			var EyeTrackingTree = _aac.NewBlendTreeAsRaw();
			EyeTrackingTree.name = "EyeTracking";
			EyeTrackingTree.blendType = BlendTreeType.Direct;

			// "Eye Tracking" state
			var EyeTrackingState = Eyelayer.NewState("Eye Tracking")
				.WithAnimation(EyeTrackingTree)
				.WithWriteDefaultsSetTo(true)
				.Drives(etBlendParam, 1.0f)
				.TrackingAnimates(AacFlState.TrackingElement.Eyes);

			// Left Eye tree
			var LeftEyeTree = EyeTrackingTree.CreateBlendTreeChild(0);
			LeftEyeTree.name = "Left Eye";
			LeftEyeTree.blendType = BlendTreeType.FreeformCartesian2D;
			LeftEyeTree.blendParameter = EyeXParam.Name;
			LeftEyeTree.blendParameterY = EyeYParam.Name;
      		AddEyePositionClips(LeftEyeTree, isLeft: true);
			
			// Right Eye tree
			var RightEyeTree = EyeTrackingTree.CreateBlendTreeChild(1);
			RightEyeTree.name = "Right Eye";
			RightEyeTree.blendType = BlendTreeType.FreeformCartesian2D;
			RightEyeTree.blendParameter = EyeXParam.Name;
			RightEyeTree.blendParameterY = EyeYParam.Name;
      		AddEyePositionClips(RightEyeTree, isLeft: false);

			// Transitions
			Eyelayer.AnyTransitionsTo(VRCEyeControlState)
				.When(etActiveParam.IsFalse());
			Eyelayer.AnyTransitionsTo(EyeTrackingState).WithTransitionToSelf()
				.When(etActiveParam.IsTrue());

			// Adding param for turning Left&Right Eye trees on/off
			var EyeTrackingTreeChildren = EyeTrackingTree.children;
			for (int i = 0; i < EyeTrackingTreeChildren.Length; i++)
			{
				var child = EyeTrackingTreeChildren[i];
				child.directBlendParameter = etBlendParam.Name;
				EyeTrackingTreeChildren[i] = child;
			}
			EyeTrackingTree.children = EyeTrackingTreeChildren;
			
			// Adding eye motion positions
			void AddEyePositionClips(BlendTree tree, bool isLeft)
			{
				var prefix = isLeft ? "Left" : "Right";
				float maxValue = 0.25f;
				var motions = new Dictionary<string, Motion>
				{
					{ "Down", isLeft ? EyeLookDownLeftRot : EyeLookDownRightRot },
					{ "InDown", isLeft ? EyeLookInDownLeftRot : EyeLookInDownRightRot },
					{ "In", isLeft ? EyeLookInLeftRot : EyeLookInRightRot },
					{ "InUp", isLeft ? EyeLookInUpLeftRot : EyeLookInUpRightRot },
					{ "Neutral", isLeft ? EyeLookNeutralLeftRot : EyeLookNeutralRightRot },
					{ "OutDown", isLeft ? EyeLookOutDownLeftRot : EyeLookOutDownRightRot },
					{ "Out", isLeft ? EyeLookOutLeftRot : EyeLookOutRightRot },
					{ "OutUp", isLeft ? EyeLookOutUPLeftRot : EyeLookOutUPRightRot },
					{ "Up", isLeft ? EyeLookUpLeftRot : EyeLookUpRightRot },
				};

				// Map motions to their positions in the 2D blend space
				var positions = new[] 
				{
					("Down", new Vector2(0, -maxValue)),
					("InDown", new Vector2(isLeft ? maxValue : -maxValue, -maxValue)),
					("In", new Vector2(isLeft ? maxValue : -maxValue, 0)),
					("InUp", new Vector2(isLeft ? maxValue : -maxValue, maxValue)),
					("Neutral", Vector2.zero),
					("OutDown", new Vector2(isLeft ? -maxValue : maxValue, -maxValue)),
					("Out", new Vector2(isLeft ? -maxValue : maxValue, 0)),
					("OutUp", new Vector2(isLeft ? -maxValue : maxValue, maxValue)),
					("Up", new Vector2(0, maxValue)),
				};

				foreach (var (name, position) in positions)
				{
					if (motions.TryGetValue(name, out var motion) && motion != null)
					{
						tree.AddChild(motion, position);
					}
				}
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

			// State "face tracking off [LipSync]"
			var offFaceTrackingLipSyncState = layer.NewState("face tracking off [LipSync]")
				.Drives(ftBlendParam, 0).WithWriteDefaultsSetTo(true)
				.TrackingTracks(AacFlState.TrackingElement.Mouth);
			offFaceTrackingLipSyncState.WithAnimation(offClip);

			// State "face tracking on"
			var onFaceTrackingState = layer.NewState("face tracking on")
				.Drives(ftBlendParam, 1).WithWriteDefaultsSetTo(true)
				.TrackingAnimates(AacFlState.TrackingElement.Mouth);

			// State "face tracking on [LipSync]"
			var onFaceTrackingLipSyncState = layer.NewState("face tracking on [LipSync]")
				.Drives(ftBlendParam, 1).WithWriteDefaultsSetTo(true)
				.TrackingTracks(AacFlState.TrackingElement.Mouth);

			// State "reset face tracking"
			var resetState = layer.NewState("reset face tracking")
				.Drives(ftBlendParam, 0).WithWriteDefaultsSetTo(true);
			resetState.WithAnimation(offClip);
			var resetDriver = resetState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();

			// Transitions of face animations toggle
			layer.AnyTransitionsTo(offFaceTrackingState)
				.When(ftActiveParam.IsFalse())
				.And(LipSyncActiveParam.IsFalse()); 

			layer.AnyTransitionsTo(offFaceTrackingLipSyncState)
				.WithTransitionToSelf()
				.When(ftActiveParam.IsFalse())
				.And(LipSyncActiveParam.IsTrue()); 				

			layer.AnyTransitionsTo(onFaceTrackingState)
				.WithTransitionToSelf()
				.When(ftActiveParam.IsTrue())
				.And(LipSyncActiveParam.IsFalse())
				.And(ResetFTActiveParam.IsFalse());

			layer.AnyTransitionsTo(onFaceTrackingLipSyncState)
				.WithTransitionToSelf()
				.When(ftActiveParam.IsTrue())
				.And(LipSyncActiveParam.IsTrue())
				.And(ResetFTActiveParam.IsFalse());

			layer.AnyTransitionsTo(resetState)
				.WithTransitionToSelf()
				.When(ResetFTActiveParam.IsTrue());

			// For prevent duplicates
			void AddParameterIfNotExists(string parameterName)
			{
				if (!resetDriver.parameters.Exists(p => p.name == parameterName))
				{
					resetDriver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
					{
						type = VRC_AvatarParameterDriver.ChangeType.Set,
						name = parameterName,
						value = 0  
					});
				}
			}

			// zeroing all face tracking parameters
			foreach (var shapeName in ftShapes)
			{
				AddParameterIfNotExists(ftPrefix + shapeName);
			}

			// adding zeroing for dual blendshapes
			foreach (var dualShape in ftDualShapes)
			{
				AddParameterIfNotExists(ftPrefix + dualShape.paramName);
			}

			// adding zeroing for Left/Right parameters
			foreach (var shapeName in ftShapes)
			{
				if (shapeName.EndsWith("Left") || shapeName.EndsWith("Right"))
				{
					foreach (var side in new[] { "Left", "Right" })
					{
						string sidedName = shapeName.Replace("Left", side).Replace("Right", side);
						AddParameterIfNotExists(ftPrefix + sidedName);
					}
				}
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

				if(MirroringParam)
				{
					for (int flip = 0; flip < EachSide(ref shapeName); flip++)
					{
						var param = CreateFloatParam(fxLayer, ftPrefix + shapeName, false, 0);
						tree.AddChild(BlendshapeTree(fxTreeLayer, skin, param));
					}
				}

				if(!MirroringParam)
				{
					var param = CreateFloatParam(fxLayer, ftPrefix + shapeName, false, 0);
					tree.AddChild(BlendshapeTree(fxTreeLayer, skin, param));
				}

			}

			// adding dual blend shapes
			for (var i = 0; i < ftDualShapes.Length; i++)
			{
				DualShape shape = ftDualShapes[i];

				if(MirroringParam)
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
							
				if(!MirroringParam)
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
				var OSCLocalState = OSCLayer.NewState(OSCLocalTree.name).WithAnimation(OSCLocalTree).WithWriteDefaultsSetTo(true);

				var OSCRemoteTree = _aac.NewBlendTreeAsRaw();
				OSCRemoteTree.name = "OSC Remote";
				OSCRemoteTree.blendType = BlendTreeType.Direct;
				var OSCRemoteState = OSCLayer.NewState(OSCRemoteTree.name).WithAnimation(OSCRemoteTree).WithWriteDefaultsSetTo(true);

				// Combine ftShapes and ftDualShapes manually
				var allShapes = new List<string>();
				foreach (var shape in ftShapes)
				{
					allShapes.Add(shape);
				}
				foreach (var ds in ftDualShapes)
				{
					allShapes.Add(ds.paramName);
				}

				// General function for creating trees
				void CreateOSCTrees(string type, BlendTree rootTree, float smoothness)
				{
					foreach (var shape in allShapes)
					{
						// Params
						var inputParamName = $"{ftPrefix}{shape}";
						var smootherParamName = $"OSCsmooth/{type}/{ftPrefix}{shape}Smoother";
						var driverParamName = $"OSCsmooth/Proxy/{ftPrefix}{shape}";

						CreateFloatParam(fxLayer, smootherParamName, true, 0.0f);
						CreateFloatParam(fxLayer, driverParamName, true, 0.0f);

						// Replace params in the FT tree
						foreach (var child in masterTree.children)
						{
							if (child.motion is BlendTree blendTree)
							{
								ReplaceBlendTreeParameter(blendTree, inputParamName, driverParamName);
							}
						}

						var inputParam = OSCLayer.FloatParameter(inputParamName);
						var smootherParam = OSCLayer.FloatParameter(smootherParamName);
						OSCLayer.OverrideValue(smootherParam, smoothness);

						// Root Tree
						var rootSubTree = rootTree.CreateBlendTreeChild(0);
						rootSubTree.name = $"OSCsmooth/{type}/{ftPrefix}{shape}Smoother";
						rootSubTree.blendType = BlendTreeType.Simple1D;
						rootSubTree.useAutomaticThresholds = false;
						rootSubTree.blendParameter = smootherParamName;

						// Input Tree
						var inputTree = rootSubTree.CreateBlendTreeChild(0);
						inputTree.name = $"OSCsmooth Input ({ftPrefix}{shape})";
						inputTree.blendType = BlendTreeType.Simple1D;
						inputTree.useAutomaticThresholds = false;
						inputTree.blendParameter = inputParam.Name;

						var clipMin = _aac.NewClip($"Animator.OSCsmooth/Proxy/{ftPrefix}{shape}_Min")
							.Animating(anim => anim.AnimatesAnimator(OSCLayer.FloatParameter(driverParamName)).WithFixedSeconds(0.0f, -1.0f));
						var clipMax = _aac.NewClip($"Animator.OSCsmooth/Proxy/{ftPrefix}{shape}_Max")
							.Animating(anim => anim.AnimatesAnimator(OSCLayer.FloatParameter(driverParamName)).WithFixedSeconds(0.0f, 1.0f));

						inputTree.AddChild(clipMin.Clip, -1.0f);
						inputTree.AddChild(clipMax.Clip, 1.0f);

						// Driver Tree
						var driverTree = rootSubTree.CreateBlendTreeChild(1);
						driverTree.name = $"OSCsmooth Driver ({ftPrefix}{shape})";
						driverTree.blendType = BlendTreeType.Simple1D;
						driverTree.useAutomaticThresholds = false;
						driverTree.blendParameter = driverParamName;

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
		var gesture = layer.IntParameter("Gesture" + (rightHand ? Right : Left));

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
				.WithAnimation(clip).WithWriteDefaultsSetTo(true);

			var enter = layer.EntryTransitionsTo(state)
				.When(gesture.IsEqualTo(i));
			var exit = state.Exits()
				.WithTransitionDurationSeconds(TransitionSpeed)
				.When(gesture.IsNotEqualTo(i));

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
			if (createFaceTracking)
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
			// Exclude ClothAdjustPrefix and OSC params (костыль, надо решить его)
			if (!paramName.StartsWith(ClothAdjustPrefix) && !paramName.StartsWith("OSCsmooth"))
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
	}


	private AacFlBoolParameter CreateBoolParam(AacFlLayer layer, string paramName, bool save, bool val)
	{
		// will save your VRCParams if True
		if(!saveVRCExpressionParameters)
		{
			// Exclude ClothAdjustPrefix and OSC params (костыль, надо решить его)
			if (!paramName.StartsWith(ClothAdjustPrefix) && !paramName.StartsWith("OSCsmooth"))
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
}

[CustomEditor(typeof(AnimatorWizard), true)]
public class AnimatorGeneratorEditor : Editor
{
	private SerializedProperty saveVRCExpressionParameters, MirroringParam;

	private SerializedProperty assetContainer;

	private SerializedProperty fxMask, EyeMask, gestureMask, lMask, rMask;

	private SerializedProperty handPoses;
	private SerializedProperty createShapePreferences, createColorCustomization, createFaceTracking, createClothCustomization, createEyeTracking;

	private SerializedProperty createFacialExpressionsControl, createFaceTrackingLipSyncControl, createFaceToggleControl, createParamForResetFaceTracking, createOSCsmooth;

	private SerializedProperty localSmoothness, remoteSmoothness;

	private SerializedProperty shapePreferenceSliderPrefix, shapePreferenceTogglesPrefix, mouthPrefix, browPrefix, ftPrefix, ClothTogglesPrefix, ClothAdjustPrefix, ClothAdjustBodyPrefix;

	private SerializedProperty primaryColor0, primaryColor1, secondColor0, secondColor1;

	private SerializedProperty EyeLookDownLeftRot, EyeLookInDownLeftRot, EyeLookInLeftRot, EyeLookInUpLeftRot, EyeLookNeutralLeftRot, EyeLookOutDownLeftRot, EyeLookOutLeftRot, EyeLookOutUPLeftRot, EyeLookUpLeftRot;
	private SerializedProperty EyeLookDownRightRot, EyeLookInDownRightRot, EyeLookInRightRot, EyeLookInUpRightRot, EyeLookNeutralRightRot, EyeLookOutDownRightRot, EyeLookOutRightRot, EyeLookOutUPRightRot, EyeLookUpRightRot;

	private SerializedProperty mouthShapeNames, browShapeNames, expTrackName, ClothUpperBodyNames, ClothLowerBodyNames, ClothFootNames;

	private SerializedProperty lipSyncName, faceToggleName, resetFTName, ftShapes, ftDualShapes;

	private AnimatorWizard wizard;
	
	private void OnEnable()
	{

		wizard = (AnimatorWizard)target;
		saveVRCExpressionParameters = serializedObject.FindProperty("saveVRCExpressionParameters");
		MirroringParam = serializedObject.FindProperty("MirroringParam");

		assetContainer = serializedObject.FindProperty("assetContainer");

		fxMask = serializedObject.FindProperty("fxMask");
		EyeMask = serializedObject.FindProperty("EyeMask");
		gestureMask = serializedObject.FindProperty("gestureMask");
		lMask = serializedObject.FindProperty("lMask");
		rMask = serializedObject.FindProperty("rMask");

		handPoses = serializedObject.FindProperty("handPoses");

		createShapePreferences = serializedObject.FindProperty("createShapePreferences");
		createColorCustomization = serializedObject.FindProperty("createColorCustomization");
		createFaceTracking = serializedObject.FindProperty("createFaceTracking");
		createClothCustomization = serializedObject.FindProperty("createClothCustomization");
		createEyeTracking = serializedObject.FindProperty("createEyeTracking");
		createFacialExpressionsControl = serializedObject.FindProperty("createFacialExpressionsControl");
		createFaceTrackingLipSyncControl = serializedObject.FindProperty("createFaceTrackingLipSyncControl");
		createFaceToggleControl = serializedObject.FindProperty("createFaceToggleControl");
		createParamForResetFaceTracking = serializedObject.FindProperty("createParamForResetFaceTracking");

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

        EyeLookDownLeftRot = serializedObject.FindProperty("EyeLookDownLeftRot");
		EyeLookInDownLeftRot = serializedObject.FindProperty("EyeLookInDownLeftRot");
		EyeLookInLeftRot = serializedObject.FindProperty("EyeLookInLeftRot");
		EyeLookInUpLeftRot = serializedObject.FindProperty("EyeLookInUpLeftRot");
		EyeLookNeutralLeftRot = serializedObject.FindProperty("EyeLookNeutralLeftRot");
		EyeLookOutDownLeftRot = serializedObject.FindProperty("EyeLookOutDownLeftRot");
		EyeLookOutLeftRot = serializedObject.FindProperty("EyeLookOutLeftRot");
		EyeLookOutUPLeftRot = serializedObject.FindProperty("EyeLookOutUPLeftRot");
		EyeLookUpLeftRot = serializedObject.FindProperty("EyeLookUpLeftRot");

		EyeLookDownRightRot = serializedObject.FindProperty("EyeLookDownRightRot");
		EyeLookInDownRightRot = serializedObject.FindProperty("EyeLookInDownRightRot");
		EyeLookInRightRot = serializedObject.FindProperty("EyeLookInRightRot");
		EyeLookInUpRightRot = serializedObject.FindProperty("EyeLookInUpRightRot");
		EyeLookNeutralRightRot = serializedObject.FindProperty("EyeLookNeutralRightRot");
		EyeLookOutDownRightRot = serializedObject.FindProperty("EyeLookOutDownRightRot");
		EyeLookOutRightRot = serializedObject.FindProperty("EyeLookOutRightRot");
		EyeLookOutUPRightRot = serializedObject.FindProperty("EyeLookOutUPRightRot");
		EyeLookUpRightRot = serializedObject.FindProperty("EyeLookUpRightRot");

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
		EditorGUILayout.PropertyField(EyeMask);
		EditorGUILayout.PropertyField(gestureMask);
		EditorGUILayout.PropertyField(lMask);
		EditorGUILayout.PropertyField(rMask);
		
		// Hand Poses
		GUILayout.Label("Hand Poses", headerStyle);
		GUILayout.Label("Array index maps to hand gesture parameter. Array length should be 8!");
		EditorGUILayout.PropertyField(handPoses);

		// Facial expressions
		GUILayout.Label("Facial expressions", headerStyle);
		GUILayout.Label("Brow and mouth blendshapes controlled by left and right hands. \nArray index maps to hand gesture parameter. Array length should be 8!");
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
		EditorGUILayout.PropertyField(createColorCustomization);
		EditorGUILayout.PropertyField(createFaceTracking);
		GUILayout.Space(10);
		GUILayout.Label("WIP:", TipStyle);	
		EditorGUILayout.PropertyField(createClothCustomization);
		EditorGUILayout.PropertyField(createEyeTracking);
		
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
            GUILayout.Label("Cloths customization [WIP]", headerStyle);
			GUILayout.Label("Animator wizard will create animations, algorithm to switch clothes \nand VRC params with these prefixes.");
			GUILayout.Space(10);
			EditorGUILayout.PropertyField(ClothTogglesPrefix, 
			PopUpLabel("Cloth Toggles Prefix", "The prefixes ClothToggles roll up clothes into a \"tube\"."));
			EditorGUILayout.PropertyField(ClothAdjustPrefix, 
			PopUpLabel("Cloth Adjust Prefix", "ClothAdjustPrefix regulates the fit of the cloth lower body to the cloth upper body."));
            EditorGUILayout.PropertyField(ClothAdjustBodyPrefix, 
			PopUpLabel("Cloth Adjust Body Prefix", "The prefixes ClothAdjustBody roll up body into a \"tube\".")); // need to combine it with ClothAdjustPrefix.
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
				GUILayout.Label("EyeTracking (Simplified Eye Parameters). [WIP]", headerStyle);
				GUILayout.Label("Animator wizard will create EyeTracking with these animations.");
				if(!wizard.createFaceTracking)
				{
					GUILayout.Space(10);
					EditorGUILayout.PropertyField(ftPrefix);
				}
				//GUILayout.Space(10);
				//EditorGUILayout.PropertyField(MirroringParam,PopUpLabel("Mirroring Animations", ""));
				GUILayout.Space(10);
				GUILayout.Label("Left Eye", headerStyle2);
				GUILayout.Space(10);
				EditorGUILayout.PropertyField(EyeLookDownLeftRot);
				EditorGUILayout.PropertyField(EyeLookInDownLeftRot);
				EditorGUILayout.PropertyField(EyeLookInLeftRot);
				EditorGUILayout.PropertyField(EyeLookInUpLeftRot);
				EditorGUILayout.PropertyField(EyeLookNeutralLeftRot);
				EditorGUILayout.PropertyField(EyeLookOutDownLeftRot);
				EditorGUILayout.PropertyField(EyeLookOutLeftRot);
				EditorGUILayout.PropertyField(EyeLookOutUPLeftRot);
				EditorGUILayout.PropertyField(EyeLookUpLeftRot);
				GUILayout.Space(10);
				//if(!wizard.MirroringParam)
				//{
					GUILayout.Label("Right Eye", headerStyle2);	
					GUILayout.Space(10);
					EditorGUILayout.PropertyField(EyeLookDownRightRot);
					EditorGUILayout.PropertyField(EyeLookInDownRightRot);
					EditorGUILayout.PropertyField(EyeLookInRightRot);
					EditorGUILayout.PropertyField(EyeLookInUpRightRot);
					EditorGUILayout.PropertyField(EyeLookNeutralRightRot);
					EditorGUILayout.PropertyField(EyeLookOutDownRightRot);
					EditorGUILayout.PropertyField(EyeLookOutRightRot);
					EditorGUILayout.PropertyField(EyeLookOutUPRightRot);
					EditorGUILayout.PropertyField(EyeLookUpRightRot);
				//}
			}


		// FaceTracking
		if (wizard.createFaceTracking)
		{
			GUILayout.Label("FaceTracking (Universal Shapes) settings", headerStyle);
			EditorGUILayout.PropertyField(ftPrefix);
			GUILayout.Space(10);
			EditorGUILayout.PropertyField(createFaceTrackingLipSyncControl,
			PopUpLabel("Create Face Tracking LipSync Control", "Adds LypSync off/on feature."));
			EditorGUILayout.PropertyField(createParamForResetFaceTracking, 
			PopUpLabel("Reset Face Tracking","A parameter is created that resets the values of all blendshape" + 
			"and Face Tracking params to zero when an OSC bug or other causes..."));
			GUILayout.Space(10);
			EditorGUILayout.PropertyField(MirroringParam,
			 PopUpLabel("Mirroring shapes", "Reflect automatically blendshapes if they have “Left” in their name (for example “MouthLowerDownLeft”)." + 
			 " You don't need to write the same blendshape for the right side (i.e. write only “MouthLowerDownLeft” and it will automatically create one for the right side as well)." + 
			 " It's better to leave it off, as bugs are possible!"));
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
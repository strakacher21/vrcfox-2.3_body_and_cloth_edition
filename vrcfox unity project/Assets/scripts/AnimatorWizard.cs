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

	public AvatarMask gestureMask;
	public AvatarMask lMask;
	public AvatarMask rMask;
	
	public Motion[] handPoses;
	
	public bool createShapePreferences = true;
	public string shapeSliderPrefix = "pref/slider/";
	public string shapeTogglesPrefix = "pref/toggle/";

	public bool createColorCustomization = true;

	public Motion primaryColor0;
	public Motion primaryColor1;
	public Motion secondColor0;
	public Motion secondColor1;
	
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
	
	public bool createFaceTracking = true;

	public string ftPrefix = "ft/";

	public string[] ftShapes =
	{
		"BrowDownLeft",
		"BrowUpLeft",
		"JawOpen",
		"LipFunnel",
		"LipPucker",
		"MouthClosed",
		"MouthStretch",
		"MouthUpperUpLeft",
		"MouthLowerDownLeft",
	};

	public DualShape[] ftDualShapes =
	{
		new DualShape("SmileSad", "MouthSad", "MouthSmile"),
		new DualShape("JawX", "JawLeft", "JawRight"),
		new DualShape("EyeLidLeft", "EyeClosed", "EyeWide", 0, 0.8f, 1),
		new DualShape("BrowExpressionLeft", "BrowDown", "BrowUp"),
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
					.WithAnimation(motion);

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

		// FT param (toggles expressions off / on)
		AacFlBoolParameter ftActiveParam =
			CreateBoolParam(fxLayer, ftPrefix + "enabled", true, false);
		AacFlFloatParameter ftBlendParam = fxLayer.FloatParameter(ftPrefix + "enabled-float");

		// brow gesture expressions
		MapHandPosesToShapes("brow expressions", skin, browShapeNames, browPrefix, false, ftActiveParam);

		// mouth gesture expressions
		MapHandPosesToShapes("mouth expressions", skin, mouthShapeNames, mouthPrefix, true, ftActiveParam);

		// body preferences
		if (createShapePreferences)
		{
			var tree = masterTree.CreateBlendTreeChild(0);
			tree.name = "preferences";
			tree.blendType = BlendTreeType.Direct;
			
			// Toggle drivers
			// this state transitions to itself every half second to update toggles. it sucks
			// TODO: not use this awful driver updating
			var fxDriverLayer = _aac.CreateSupportingFxLayer("drivers").WithAvatarMask(fxMask);
			var fxDriverState = fxDriverLayer.NewState("drivers");
			fxDriverState.TransitionsTo(fxDriverState).AfterAnimationFinishes().WithTransitionDurationSeconds(0.5f)
				.WithTransitionToSelf();
			var drivers = fxDriverState.State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();

			// for each blend shape with the 'prefs/' prefix,
			// create a new blend shape control subtree
			for (var i = 0; i < skin.sharedMesh.blendShapeCount; i++)
			{
				string blendShapeName = skin.sharedMesh.GetBlendShapeName(i);

				if (blendShapeName.StartsWith(shapeSliderPrefix))
				{
					var param = CreateFloatParam(fxLayer, blendShapeName, true, 0);
					tree.AddChild(BlendshapeTree(fxTreeLayer, skin, param));
				} else if (blendShapeName.StartsWith(shapeTogglesPrefix))
				{
					var boolParam = CreateBoolParam(fxLayer, blendShapeName, true, false);

					var floatParam = fxLayer.FloatParameter(blendShapeName + "-float");

					var driverEntry = new VRC_AvatarParameterDriver.Parameter();
					driverEntry.type = VRC_AvatarParameterDriver.ChangeType.Copy;
					driverEntry.source = boolParam.Name;
					driverEntry.name = floatParam.Name;

					drivers.parameters.Add(driverEntry);

					tree.AddChild(BlendshapeTree(fxTreeLayer, skin, blendShapeName, floatParam));
				}
			}
		}

		if (createColorCustomization)
		{
			var tree = masterTree.CreateBlendTreeChild(0);
			tree.name = "color customization";
			tree.blendType = BlendTreeType.Direct;

			// color changing
			tree.AddChild(Subtree(new[] { primaryColor0, primaryColor1 },
				new[] { 0f, 1f },
				CreateFloatParam(fxTreeLayer, shapeSliderPrefix + "pcol", true, 0)));

			tree.AddChild(Subtree(new[] { secondColor0, secondColor1 },
				new[] { 0f, 1f },
				CreateFloatParam(fxTreeLayer, shapeSliderPrefix + "scol", true, 0)));
		}

		// face tracking
		if (createFaceTracking)
		{
			var layer = _aac.CreateSupportingFxLayer("face animations toggle").WithAvatarMask(fxMask);

			var offState = layer.NewState("face tracking off")
				.Drives(ftBlendParam, 0);
			var offControl = offState.State.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
			// offControl.trackingEyes = VRC_AnimatorTrackingControl.TrackingType.Tracking;
			offControl.trackingMouth = VRC_AnimatorTrackingControl.TrackingType.Tracking;

			// zero all FT blendshapes when FT disabled
			var offClip = _aac.NewClip("zero all FT blendshapes");

			for (var i = 0; i < skin.sharedMesh.blendShapeCount; i++)
			{
				string blendShapeName = skin.sharedMesh.GetBlendShapeName(i);

				if (blendShapeName.StartsWith(ftPrefix))
				{
					offClip.BlendShape(skin, blendShapeName, 0);
				}
			}

			offState.WithAnimation(offClip);

			var onState = layer.NewState("face tracking on")
				.Drives(ftBlendParam, 1);
			var onControl = onState.State.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
			// onControl.trackingEyes = VRC_AnimatorTrackingControl.TrackingType.Animation;
			onControl.trackingMouth = VRC_AnimatorTrackingControl.TrackingType.Animation;

			layer.AnyTransitionsTo(onState).WithTransitionToSelf().When(ftActiveParam.IsTrue());
			layer.AnyTransitionsTo(offState).When(ftActiveParam.IsFalse());


			var tree = masterTree.CreateBlendTreeChild(0);
			tree.name = "face tracking";
			tree.blendType = BlendTreeType.Direct;

			tree.blendParameter = ftActiveParam.Name;
			tree.blendParameterY = ftActiveParam.Name;

			// straight-forward blendshapes
			for (var i = 0; i < ftShapes.Length; i++)
			{
				string shapeName = ftShapes[i];

				for (int flip = 0; flip < EachSide(ref shapeName); flip++)
				{
					var param = CreateFloatParam(fxLayer, ftPrefix + shapeName, false, 0);
					tree.AddChild(BlendshapeTree(fxTreeLayer, skin, param));
				}
			}

			// dual blendshapes
			for (var i = 0; i < ftDualShapes.Length; i++)
			{
				DualShape shape = ftDualShapes[i];
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

			var children = masterTree.children;
			children[children.Length - 1].directBlendParameter = ftBlendParam.Name;
			masterTree.children = children;

			// eyes
			// {
			// 	CreateFloatParamVrcOnly(ftPrefix + "EyeLeftX", false, 0);
			// 	CreateFloatParamVrcOnly(ftPrefix + "EyeRightX", false, 0);
			// 	CreateFloatParamVrcOnly(ftPrefix + "EyeY", false, 0);
			// }
		}

		// add all the new avatar params to the avatar descriptor
		avatar.expressionParameters.parameters = _vrcParams.ToArray();
		EditorUtility.SetDirty(avatar.expressionParameters);
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
		string[] shapeNames, string prefix, bool rightHand, AacFlBoolParameter disableParam)
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
				.WithAnimation(clip);

			var enter = layer.EntryTransitionsTo(state)
				.When(gesture.IsEqualTo(i));
			var exit = state.Exits()
				.WithTransitionDurationSeconds(TransitionSpeed)
				.When(gesture.IsNotEqualTo(i));

			if (createFaceTracking)
			{
				if (i == 0)
				{
					enter.Or().When(disableParam.IsTrue());
					exit.And(disableParam.IsFalse());
				}
				else
				{
					enter.And(disableParam.IsFalse());
					exit.Or().When(disableParam.IsTrue());
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
		_vrcParams.Add(new VRCExpressionParameters.Parameter()
		{
			name = paramName,
			valueType = VRCExpressionParameters.ValueType.Float,
			saved = save,
			networkSynced = true,
			defaultValue = val,
		});
	}

	private AacFlBoolParameter CreateBoolParam(AacFlLayer layer, string paramName, bool save, bool val)
	{
		_vrcParams.Add(new VRCExpressionParameters.Parameter()
		{
			name = paramName,
			valueType = VRCExpressionParameters.ValueType.Bool,
			saved = save,
			networkSynced = true,
			defaultValue = val ? 1 : 0,
		});

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
	private SerializedProperty assetContainer;
	private SerializedProperty fxMask, gestureMask, lMask, rMask;

	private SerializedProperty handPoses;
	private SerializedProperty createShapePreferences, createColorCustomization, createFaceTracking;

	private SerializedProperty shapeSliderPrefix, shapeTogglesPrefix, mouthPrefix, browPrefix, ftPrefix;

	private SerializedProperty primaryColor0, primaryColor1, secondColor0, secondColor1;

	private SerializedProperty mouthShapeNames, browShapeNames;

	private SerializedProperty ftShapes, ftDualShapes;

	private AnimatorWizard wizard;
	
	private void OnEnable()
	{
		wizard = (AnimatorWizard)target;
		
		assetContainer = serializedObject.FindProperty("assetContainer");
		fxMask = serializedObject.FindProperty("fxMask");
		gestureMask = serializedObject.FindProperty("gestureMask");
		lMask = serializedObject.FindProperty("lMask");
		rMask = serializedObject.FindProperty("rMask");

		handPoses = serializedObject.FindProperty("handPoses");

		createShapePreferences = serializedObject.FindProperty("createShapePreferences");
		createColorCustomization = serializedObject.FindProperty("createColorCustomization");
		createFaceTracking = serializedObject.FindProperty("createFaceTracking");

		shapeSliderPrefix = serializedObject.FindProperty("shapeSliderPrefix");
		shapeTogglesPrefix = serializedObject.FindProperty("shapeTogglesPrefix");
		mouthPrefix = serializedObject.FindProperty("mouthPrefix");
		browPrefix = serializedObject.FindProperty("browPrefix");
		ftPrefix = serializedObject.FindProperty("ftPrefix");

		primaryColor0 = serializedObject.FindProperty("primaryColor0");
		secondColor0 = serializedObject.FindProperty("secondColor0");

		primaryColor1 = serializedObject.FindProperty("primaryColor1");
		secondColor1 = serializedObject.FindProperty("secondColor1");

		mouthShapeNames = serializedObject.FindProperty("mouthShapeNames");
		browShapeNames = serializedObject.FindProperty("browShapeNames");

		ftShapes = serializedObject.FindProperty("ftShapes");
		ftDualShapes = serializedObject.FindProperty("ftDualShapes");
	}

	private const string AlertMsg =
		"Running this will destroy any manual animator changes. Are you sure you want to continue?";

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
		
		if (GUILayout.Button("Setup animator! (DESTRUCTIVE!!!)", GUILayout.Height(50)))
		{
			if (EditorUtility.DisplayDialog("Animator Wizard", AlertMsg, "yes (DESTRUCTIVE!)", "NO"))
			{
				Create();
			}
		}
		
		GUILayout.Space(20);

		EditorGUILayout.PropertyField(assetContainer);

		GUILayout.Label("Avatar animator masks", headerStyle);
		EditorGUILayout.PropertyField(fxMask);
		EditorGUILayout.PropertyField(gestureMask);
		EditorGUILayout.PropertyField(lMask);
		EditorGUILayout.PropertyField(rMask);
		
		GUILayout.Label("Hand Poses", headerStyle);
		GUILayout.Label("Array index maps to hand gesture parameter. Array length should be 8!");
		EditorGUILayout.PropertyField(handPoses);

		GUILayout.Label("Facial expressions", headerStyle);
		GUILayout.Label("Brow and mouth blendshapes controlled by left and right hands." +
		                "Array index maps to hand gesture parameter. Array length should be 8!");
		EditorGUILayout.PropertyField(mouthPrefix);
		EditorGUILayout.PropertyField(mouthShapeNames);
		GUILayout.Space(20);
		EditorGUILayout.PropertyField(browPrefix);
		EditorGUILayout.PropertyField(browShapeNames);
		
		GUILayout.Label("Animator creation flags", headerStyle);
		GUILayout.Label("Choose what parts of the animator are generated. " +
		                "Disabling features saves VRC parameter budget!");
		EditorGUILayout.PropertyField(createShapePreferences);
		EditorGUILayout.PropertyField(createColorCustomization);
		EditorGUILayout.PropertyField(createFaceTracking);
		
		if (wizard.createShapePreferences)
		{
			GUILayout.Label("Preference prefixes", headerStyle);
			GUILayout.Label(
				"Animator wizard will automatically create VRC parameters for blendshapes with these prefixes");
			EditorGUILayout.PropertyField(shapeSliderPrefix);
			EditorGUILayout.PropertyField(shapeTogglesPrefix);
		}

		if (wizard.createColorCustomization)
		{
			GUILayout.Label("Color customization UV-offset animations", headerStyle);
			GUILayout.Label("Animations controlling color palette texture UV-offsets for in-game color customization");
			EditorGUILayout.PropertyField(primaryColor0);
			EditorGUILayout.PropertyField(primaryColor1);
			EditorGUILayout.PropertyField(secondColor0);
			EditorGUILayout.PropertyField(secondColor1);
		}

		if (wizard.createFaceTracking)
		{
			GUILayout.Label("VRCFaceTracking (Universal Shapes) settings", headerStyle);
			EditorGUILayout.PropertyField(ftPrefix);
			GUILayout.Space(10);
			GUILayout.Label("Single shapes controlled by a float parameter");
			EditorGUILayout.PropertyField(ftShapes);
			GUILayout.Space(10);
			GUILayout.Label("Mutually exclusive shape pairs controlled by a single float parameter");
			EditorGUILayout.PropertyField(ftDualShapes);
		}

		serializedObject.ApplyModifiedProperties();
	}

	private void Create()
	{
		((AnimatorWizard)target).Create();
	}
}
#endif
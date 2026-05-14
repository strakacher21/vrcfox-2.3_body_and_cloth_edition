#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimatorWizard), true)]
public class AnimatorGeneratorEditor : Editor
{
    private SerializedProperty saveVRCExpressionParameters;
    private SerializedProperty SystemName;

    private SerializedProperty UseSameHandAnimationsForBothHands;
    private SerializedProperty UseSameEyeAnimationsForBothEyes;

    private SerializedProperty assetContainer;

    private SerializedProperty fxMask;
    private SerializedProperty EyeLeftMask;
    private SerializedProperty EyeRightMask;

    private SerializedProperty gestureMask;
    private SerializedProperty GestureLeftMask;
    private SerializedProperty GestureRightMask;

    private SerializedProperty LeftHandPoses;
    private SerializedProperty RightHandPoses;

    private SerializedProperty createShapePreferences;
    private SerializedProperty createColorCustomization;
    private SerializedProperty createClothCustomization;
    private SerializedProperty createFaceToggle;
    private SerializedProperty createEyeTracking;
    private SerializedProperty createFaceTracking;
    private SerializedProperty createParamsCompressor;
    private SerializedProperty compressedParamEntries;

    private SerializedProperty createFTLipSyncControl;
    private SerializedProperty createOSCsmooth;

    private SerializedProperty localSmoothness;
    private SerializedProperty remoteSmoothness;

    private SerializedProperty shapePreferencePrefix;
    private SerializedProperty shapePreferences;

    private SerializedProperty mouthPrefix;
    private SerializedProperty browPrefix;

    private SerializedProperty FullFaceTrackingPrefix;
    private SerializedProperty ClothTogglesPrefix;

    private SerializedProperty ColorProfiles;

    private SerializedProperty maxEyeMotionValue;

    private SerializedProperty LeftEyePoses;
    private SerializedProperty RightEyePoses;

    private SerializedProperty mouthShapeNames;
    private SerializedProperty browShapeNames;

    private SerializedProperty expTrackName;
    private SerializedProperty ClothGroups;
    private SerializedProperty ClothSyncParamsOptimizedAlgorithm;

    private SerializedProperty FaceToggleNames;

    private SerializedProperty GestureExpressionsBlockParamNames;
    private SerializedProperty FaceToggleBlockParamNames;
    private SerializedProperty FaceTrackingBlockParamNames;
    private SerializedProperty EyeTrackingBlockParamNames;

    private SerializedProperty lipSyncName;

    private SerializedProperty SingleFtShapes;
    private SerializedProperty DualFtShapes;

    private AnimatorWizard wizard;

    private GUIStyle headerStyle;
    private GUIStyle headerStyle2;
    private GUIStyle HeaderStyle => headerStyle ??= new GUIStyle
    {
        richText = false,
        fontStyle = FontStyle.Bold,
        fontSize = EditorStyles.label.fontSize + 5,
        padding = new RectOffset(3, 3, 40, 8),
        normal = new GUIStyleState { textColor = EditorStyles.label.normal.textColor }
    };

    private GUIStyle HeaderStyle2 => headerStyle2 ??= new GUIStyle
    {
        richText = false,
        fontStyle = FontStyle.Bold,
        fontSize = EditorStyles.label.fontSize + 1,
        padding = new RectOffset(3, 3, 0, 5),
        normal = new GUIStyleState { textColor = EditorStyles.label.normal.textColor }
    };

    private const string AlertMsg =
        "Running this will destroy any manual animator changes. Are you sure you want to continue?";

    private void OnEnable()
    {
        wizard = (AnimatorWizard)target;

        saveVRCExpressionParameters = serializedObject.FindProperty("saveVRCExpressionParameters");
        SystemName = serializedObject.FindProperty("SystemName");

        UseSameHandAnimationsForBothHands = serializedObject.FindProperty("UseSameHandAnimationsForBothHands");
        UseSameEyeAnimationsForBothEyes = serializedObject.FindProperty("UseSameEyeAnimationsForBothEyes");

        assetContainer = serializedObject.FindProperty("assetContainer");

        fxMask = serializedObject.FindProperty("fxMask");
        EyeLeftMask = serializedObject.FindProperty("EyeLeftMask");
        EyeRightMask = serializedObject.FindProperty("EyeRightMask");

        gestureMask = serializedObject.FindProperty("gestureMask");
        GestureLeftMask = serializedObject.FindProperty("GestureLeftMask");
        GestureRightMask = serializedObject.FindProperty("GestureRightMask");

        LeftHandPoses = serializedObject.FindProperty("LeftHandPoses");
        RightHandPoses = serializedObject.FindProperty("RightHandPoses");

        createShapePreferences = serializedObject.FindProperty("createShapePreferences");
        createColorCustomization = serializedObject.FindProperty("createColorCustomization");
        createClothCustomization = serializedObject.FindProperty("createClothCustomization");
        createFaceToggle = serializedObject.FindProperty("createFaceToggle");
        createEyeTracking = serializedObject.FindProperty("createEyeTracking");
        createFaceTracking = serializedObject.FindProperty("createFaceTracking");
        createParamsCompressor = serializedObject.FindProperty("createParamsCompressor");
        compressedParamEntries = serializedObject.FindProperty("compressedParamEntries");

        createFTLipSyncControl = serializedObject.FindProperty("createFTLipSyncControl");
        createOSCsmooth = serializedObject.FindProperty("createOSCsmooth");

        localSmoothness = serializedObject.FindProperty("localSmoothness");
        remoteSmoothness = serializedObject.FindProperty("remoteSmoothness");

        shapePreferencePrefix = serializedObject.FindProperty("shapePreferencePrefix");
        shapePreferences = serializedObject.FindProperty("shapePreferences");

        mouthPrefix = serializedObject.FindProperty("mouthPrefix");
        browPrefix = serializedObject.FindProperty("browPrefix");

        FullFaceTrackingPrefix = serializedObject.FindProperty("FullFaceTrackingPrefix");
        ClothTogglesPrefix = serializedObject.FindProperty("ClothTogglesPrefix");

        expTrackName = serializedObject.FindProperty("expTrackName");
        lipSyncName = serializedObject.FindProperty("lipSyncName");

        mouthShapeNames = serializedObject.FindProperty("mouthShapeNames");
        browShapeNames = serializedObject.FindProperty("browShapeNames");

        ClothGroups = serializedObject.FindProperty("ClothGroups");
        ClothSyncParamsOptimizedAlgorithm = serializedObject.FindProperty("ClothSyncParamsOptimizedAlgorithm");

        FaceToggleNames = serializedObject.FindProperty("FaceToggleNames");

        GestureExpressionsBlockParamNames = serializedObject.FindProperty("GestureExpressionsBlockParamNames");
        FaceToggleBlockParamNames = serializedObject.FindProperty("FaceToggleBlockParamNames");
        FaceTrackingBlockParamNames = serializedObject.FindProperty("FaceTrackingBlockParamNames");
        EyeTrackingBlockParamNames = serializedObject.FindProperty("EyeTrackingBlockParamNames");

        ColorProfiles = serializedObject.FindProperty("ColorProfiles");

        maxEyeMotionValue = serializedObject.FindProperty("maxEyeMotionValue");

        LeftEyePoses = serializedObject.FindProperty("LeftEyePoses");
        RightEyePoses = serializedObject.FindProperty("RightEyePoses");

        SingleFtShapes = serializedObject.FindProperty("SingleFtShapes");
        DualFtShapes = serializedObject.FindProperty("DualFtShapes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUILayout.Space(10);

        if (GUILayout.Button("Setup animator! (DESTRUCTIVE!!!)", GUILayout.Height(50)))
        {
            if (EditorUtility.DisplayDialog("Animator Wizard", AlertMsg, "yes (DESTRUCTIVE!)", "NO"))
            {
                ((AnimatorWizard)target).Create();

                EditorUtility.DisplayDialog(
                    "Animator Wizard",
                    "Animator setup has completed successfully.",
                    "OK"
                );
            }
        }

        // Save VRC Expression Parameters
        GUILayout.Space(20);
        EditorGUILayout.PropertyField(saveVRCExpressionParameters,
         PopUpLabel("Save VRC Expression Parameters", "Will save your VRC Expression Parameters before setup animator."));


        GUILayout.Space(20);
        EditorGUILayout.PropertyField(SystemName, PopUpLabel("Layers start name", ""));

        // Asset Container
        GUILayout.Label("Asset Container", HeaderStyle);
        EditorGUILayout.PropertyField(assetContainer,
        PopUpLabel("Asset Container", "Asset Container stores all generated animations and Blend Trees."));

        // Avatar animator masks
        GUILayout.Label("Avatar animator masks", HeaderStyle);
        EditorGUILayout.PropertyField(fxMask);
        if (wizard.createEyeTracking)
        {
            EditorGUILayout.PropertyField(EyeLeftMask);
            EditorGUILayout.PropertyField(EyeRightMask);
        }
        EditorGUILayout.PropertyField(gestureMask);
        EditorGUILayout.PropertyField(GestureLeftMask);
        EditorGUILayout.PropertyField(GestureRightMask);

        // Hand Poses
        GUILayout.Label("Hand Poses", HeaderStyle);
        GUILayout.Label("Array index maps to hand gesture parameter. Array length should be 8!", HeaderStyle2);
        EditorGUILayout.PropertyField(UseSameHandAnimationsForBothHands, PopUpLabel("Same Animations", "Use the same animations for both hands"));
        GUILayout.Space(10);

        if (wizard.UseSameHandAnimationsForBothHands)
        {
            EditorGUILayout.PropertyField(LeftHandPoses, PopUpLabel("Hand Poses", ""));
        }

        else
        {
            EditorGUILayout.PropertyField(LeftHandPoses);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(RightHandPoses);
        }

        // Facial expressions
        GUILayout.Label("Facial expressions", HeaderStyle);
        GUILayout.Label("Brow and mouth blendshapes controlled by left and right hands." +
            "\nArray index maps to hand Gesture parameter. Array length should be 8!", HeaderStyle2);
        // Custom gesture blocks
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(GestureExpressionsBlockParamNames,
            PopUpLabel("Gesture Expressions Block bool list", "Each element is a VRC bool parameter name. When any is True, gestures won't drive expressions."));
        GUILayout.Space(10);
        EditorGUILayout.PropertyField(mouthPrefix);
        EditorGUILayout.PropertyField(mouthShapeNames);
        GUILayout.Space(20);
        EditorGUILayout.PropertyField(browPrefix);
        EditorGUILayout.PropertyField(browShapeNames);

        // Animator creation flags
        GUILayout.Label("Animator creation flags", HeaderStyle);
        GUILayout.Label("Choose what parts of the animator are generated.");
        GUILayout.Space(10);
        EditorGUILayout.PropertyField(createShapePreferences);
        EditorGUILayout.PropertyField(createClothCustomization);
        EditorGUILayout.PropertyField(createColorCustomization);
        EditorGUILayout.PropertyField(createFaceToggle);
        EditorGUILayout.PropertyField(createParamsCompressor);
        if (wizard.createFaceTracking || wizard.createEyeTracking)
            EditorGUILayout.PropertyField(createOSCsmooth);
        EditorGUILayout.PropertyField(createEyeTracking);
        EditorGUILayout.PropertyField(createFaceTracking);

        // Shape Preferences
        if (wizard.createShapePreferences)
        {
            GUILayout.Label("Shape Preferences", HeaderStyle);
            GUILayout.Label("Creates VRC params for blendshapes with bool/float behaviour.", HeaderStyle2);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(shapePreferencePrefix);
            EditorGUILayout.PropertyField(shapePreferences, new GUIContent("Shape Preferences List"), true);
        }

        // Cloths customization
        if (wizard.createClothCustomization)
        {
            GUILayout.Label("Cloths customization", HeaderStyle);
            GUILayout.Label("Creates an algorithm to switch clothes, animations and VRC params with these prefixes.", HeaderStyle2);
            GUILayout.Space(10);

            EditorGUILayout.PropertyField(ClothTogglesPrefix,
                PopUpLabel("Cloth Toggles Prefix", "Prefixes roll up clothes and body into \"tube\",\n" +
                "as well as regulates the fit of the cloth lower body to the cloth upper body."));

            GUILayout.Space(10);
            EditorGUILayout.PropertyField(ClothSyncParamsOptimizedAlgorithm,
                PopUpLabel("Sync Params Optimized Algorithm", "Use bool algorithm to switch clothes. Uncheck to use Int algorithm."));

            GUILayout.Space(10);
            EditorGUILayout.PropertyField(ClothGroups, new GUIContent("Cloth Groups"), true);
        }

        // Color customization
        if (wizard.createColorCustomization)
        {
            GUILayout.Label("Color customization", HeaderStyle);
            GUILayout.Label("Each element defines a name and four color animations.", HeaderStyle2);

            EditorGUILayout.PropertyField(ColorProfiles, new GUIContent("Profiles"), true);
        }

        // Face Toggle
        if (wizard.createFaceToggle)
        {
            GUILayout.Label("FaceToggle setup animations", HeaderStyle);
            GUILayout.Label("Creates an algorithm to switch face animations.", HeaderStyle2);
            // Custom Face Toggle blocks
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(FaceToggleBlockParamNames,
                PopUpLabel("Face Toggle Block bool list", "Each element is a VRC bool parameter name. When any is True, Face Toggle won't work."));
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(FaceToggleNames);
        }

        // Parameter Compressor
        if (wizard.createParamsCompressor)
        {
            GUILayout.Label("Parameter Compressor", HeaderStyle);
            EditorGUILayout.PropertyField(compressedParamEntries, new GUIContent("Custom Params List"), true);
        }

        // OSC smooth
        if ((wizard.createFaceTracking || wizard.createEyeTracking) && wizard.createOSCsmooth)
        {
            GUILayout.Label("OSC smooth setup", HeaderStyle);
            GUILayout.Label("OSC smooth is needed to fix Face/Eye Tracking params in-game, " +
                "\nas without it animation is choppy and jerky, as if it's lacking FPS.", HeaderStyle2);
            EditorGUILayout.PropertyField(localSmoothness);
            EditorGUILayout.PropertyField(remoteSmoothness);
        }
        // Eye Tracking
        if (wizard.createEyeTracking)
        {
            GUILayout.Label("Eye Tracking (Simplified Eye Parameters) settings", HeaderStyle);
            GUILayout.Label("Creates Eye Tracking with these animations.", HeaderStyle2);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(FullFaceTrackingPrefix);
            EditorGUILayout.PropertyField(maxEyeMotionValue);
            EditorGUILayout.PropertyField(UseSameEyeAnimationsForBothEyes, PopUpLabel("Same Animations", "Use the same animations for both eyes."));
            // Custom Eye Tracking blocks
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(EyeTrackingBlockParamNames,
                PopUpLabel("Face Tracking Block bool list", "Each element is a VRC bool parameter name. When any is True, Eye Tracking won't work."));
            GUILayout.Space(10);
            if (wizard.UseSameEyeAnimationsForBothEyes)
            {
                EditorGUILayout.PropertyField(LeftEyePoses, PopUpLabel("Eye Poses", ""));
            }
            else
            {
                EditorGUILayout.PropertyField(LeftEyePoses);
                EditorGUILayout.PropertyField(RightEyePoses);
            }
        }

        // Face Tracking
        if (wizard.createFaceTracking)
        {
            GUILayout.Label("Face Tracking (Universal Shapes) settings", HeaderStyle);
            GUILayout.Label("Creates Face Tracking with these animations.", HeaderStyle2);
            EditorGUILayout.PropertyField(FullFaceTrackingPrefix);
            EditorGUILayout.PropertyField(createFTLipSyncControl,
            PopUpLabel("Face Tracking LipSync Control", "Adds LypSync off/on feature."));
            // Custom Face Toggle blocks
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(FaceTrackingBlockParamNames,
                PopUpLabel("Face Tracking Block bool list", "Each element is a VRC bool parameter name. When any is True, Face Tracking won't work."));
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(SingleFtShapes, PopUpLabel("FT Single Shapes", "Single shapes controlled by a float parameter."));
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(DualFtShapes, PopUpLabel("FT Dual Shapes", "Mutually exclusive shape pairs controlled by a single float parameter."));
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static GUIContent PopUpLabel(string propertyFieldLabel, string label)
    {
        return new GUIContent(propertyFieldLabel, label);
    }
}

// Shared UI logic for drawing an entry with a text field and a toggle button
public static class DrawerUIHelper
{
    public static void DrawEntryWithButton(Rect position, SerializedProperty nameProp, SerializedProperty stateProp, string trueLabel, string falseLabel, string trueTooltip, string falseTooltip)
    {
        const float spacing = 6f;
        const float buttonWidth = 90f;

        var nameRect = new Rect(position.x, position.y, position.width - buttonWidth - spacing, position.height);
        var buttonRect = new Rect(nameRect.xMax + spacing, position.y, buttonWidth, position.height);

        EditorGUI.PropertyField(nameRect, nameProp, GUIContent.none);

        var buttonLabel = stateProp.boolValue ? trueLabel : falseLabel;
        var buttonTooltip = stateProp.boolValue ? trueTooltip : falseTooltip;

        if (GUI.Button(buttonRect, new GUIContent(buttonLabel, buttonTooltip)))
        {
            stateProp.boolValue = !stateProp.boolValue;
        }
    }
}

[CustomPropertyDrawer(typeof(AnimatorWizard.ClothEntry))]
public class ClothEntryDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var clothName = property.FindPropertyRelative("clothName");
        var invertAnimation = property.FindPropertyRelative("invertAnimation");

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);

        DrawerUIHelper.DrawEntryWithButton(
            position, clothName, invertAnimation,
            "Inverted", "Default",
            "Use the inverted animation direction.",
            "Use the default animation direction."
        );

        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(AnimatorWizard.ClothGroup))]
public class ClothGroupDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var layerName = property.FindPropertyRelative("layerName");
        var clothEntries = property.FindPropertyRelative("clothEntries");
        return EditorGUI.GetPropertyHeight(layerName) + 4f + EditorGUI.GetPropertyHeight(clothEntries, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var layerName = property.FindPropertyRelative("layerName");
        var clothEntries = property.FindPropertyRelative("clothEntries");

        var layerRect = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(layerName));
        EditorGUI.PropertyField(layerRect, layerName, new GUIContent("Layer Name"));

        var entriesRect = new Rect(position.x, layerRect.yMax + 4f, position.width, EditorGUI.GetPropertyHeight(clothEntries, true));
        EditorGUI.PropertyField(entriesRect, clothEntries, new GUIContent("Cloth Entries"), true);
    }
}

[CustomPropertyDrawer(typeof(AnimatorWizard.ShapePreferenceEntry))]
public class ShapePreferenceEntryDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var blendShapeName = property.FindPropertyRelative("blendShapeName");
        var useBool = property.FindPropertyRelative("useBool");
        var useFloat = property.FindPropertyRelative("useFloat");
        var lastMode = property.FindPropertyRelative("lastMode");

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);

        // Treat useFloat as the primary toggleable state to match the button UI
        DrawerUIHelper.DrawEntryWithButton(
            position, blendShapeName, useFloat,
            "Float", "Bool",
            "Use Float for this blendshape.",
            "Use Bool for this blendshape."
        );

        // Make Bool/Float mutually exclusive and sync states
        // safety for edge cases (multi-edit serialized state)
        useBool.boolValue = !useFloat.boolValue;
        lastMode.intValue = useFloat.boolValue ? 1 : 0;

        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(AnimatorWizard.CompressedParamEntry))]
public class CompressedParamEntryDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var paramName = property.FindPropertyRelative("paramName");
        var useFloat = property.FindPropertyRelative("useFloat");
        var useInt = property.FindPropertyRelative("useInt");
        var lastMode = property.FindPropertyRelative("lastMode");

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);

        DrawerUIHelper.DrawEntryWithButton(
            position, paramName, useFloat,
            "Float", "Int",
            "Use Float for this parameter.",
            "Use Int for this parameter."
        );

        useInt.boolValue = !useFloat.boolValue;
        lastMode.intValue = useFloat.boolValue ? 1 : 0;

        EditorGUI.EndProperty();
    }
}
#endif
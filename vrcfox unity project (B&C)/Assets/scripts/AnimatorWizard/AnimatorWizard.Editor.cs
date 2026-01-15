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

    private SerializedProperty createFTLipSyncControl;
    private SerializedProperty createOSCsmooth;

    private SerializedProperty localSmoothness;
    private SerializedProperty remoteSmoothness;

    private SerializedProperty shapePreferenceSliderPrefix;
    private SerializedProperty shapePreferenceTogglesPrefix;

    private SerializedProperty mouthPrefix;
    private SerializedProperty browPrefix;

    private SerializedProperty FullFaceTrackingPrefix;
    private SerializedProperty ClothTogglesPrefix;

    private SerializedProperty primaryColor0;
    private SerializedProperty primaryColor1;
    private SerializedProperty secondColor0;
    private SerializedProperty secondColor1;

    private SerializedProperty maxEyeMotionValue;

    private SerializedProperty LeftEyePoses;
    private SerializedProperty RightEyePoses;

    private SerializedProperty mouthShapeNames;
    private SerializedProperty browShapeNames;

    private SerializedProperty expTrackName;
    private SerializedProperty ClothUpperBodyNames;
    private SerializedProperty ClothLowerBodyNames;
    private SerializedProperty ClothFootNames;

    private SerializedProperty FaceToggleNames;

    private SerializedProperty GestureExpressionsBlockParamNames;
    private SerializedProperty FaceToggleBlockParamNames;
    private SerializedProperty FaceTrackingBlockParamNames;
    private SerializedProperty EyeTrackingBlockParamNames;

    private SerializedProperty lipSyncName;

    private SerializedProperty SingleFtShapes;
    private SerializedProperty DualFtShapes;

    private AnimatorWizard wizard;

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

        createFTLipSyncControl = serializedObject.FindProperty("createFTLipSyncControl");
        createOSCsmooth = serializedObject.FindProperty("createOSCsmooth");

        localSmoothness = serializedObject.FindProperty("localSmoothness");
        remoteSmoothness = serializedObject.FindProperty("remoteSmoothness");

        shapePreferenceSliderPrefix = serializedObject.FindProperty("shapePreferenceSliderPrefix");
        shapePreferenceTogglesPrefix = serializedObject.FindProperty("shapePreferenceTogglesPrefix");

        mouthPrefix = serializedObject.FindProperty("mouthPrefix");
        browPrefix = serializedObject.FindProperty("browPrefix");

        FullFaceTrackingPrefix = serializedObject.FindProperty("FullFaceTrackingPrefix");
        ClothTogglesPrefix = serializedObject.FindProperty("ClothTogglesPrefix");

        expTrackName = serializedObject.FindProperty("expTrackName");
        lipSyncName = serializedObject.FindProperty("lipSyncName");

        mouthShapeNames = serializedObject.FindProperty("mouthShapeNames");
        browShapeNames = serializedObject.FindProperty("browShapeNames");

        ClothUpperBodyNames = serializedObject.FindProperty("ClothUpperBodyNames");
        ClothLowerBodyNames = serializedObject.FindProperty("ClothLowerBodyNames");
        ClothFootNames = serializedObject.FindProperty("ClothFootNames");

        FaceToggleNames = serializedObject.FindProperty("FaceToggleNames");

        GestureExpressionsBlockParamNames = serializedObject.FindProperty("GestureExpressionsBlockParamNames");
        FaceToggleBlockParamNames = serializedObject.FindProperty("FaceToggleBlockParamNames");
        FaceTrackingBlockParamNames = serializedObject.FindProperty("FaceTrackingBlockParamNames");
        EyeTrackingBlockParamNames = serializedObject.FindProperty("EyeTrackingBlockParamNames");

        primaryColor0 = serializedObject.FindProperty("primaryColor0");
        secondColor0 = serializedObject.FindProperty("secondColor0");
        primaryColor1 = serializedObject.FindProperty("primaryColor1");
        secondColor1 = serializedObject.FindProperty("secondColor1");

        maxEyeMotionValue = serializedObject.FindProperty("maxEyeMotionValue");

        LeftEyePoses = serializedObject.FindProperty("LeftEyePoses");
        RightEyePoses = serializedObject.FindProperty("RightEyePoses");

        SingleFtShapes = serializedObject.FindProperty("SingleFtShapes");
        DualFtShapes = serializedObject.FindProperty("DualFtShapes");
    }

    public override void OnInspectorGUI()
    {
        GUIStyle headerStyle = new GUIStyle
        {
            richText = false,
            fontStyle = FontStyle.Bold,
            fontSize = EditorStyles.label.fontSize + 5,
            padding = new RectOffset(3, 3, 40, 8),
            normal = new GUIStyleState { textColor = EditorStyles.label.normal.textColor }
        };

        GUIStyle headerStyle2 = new GUIStyle
        {
            richText = false,
            fontStyle = FontStyle.Bold,
            fontSize = EditorStyles.label.fontSize + 1,
            padding = new RectOffset(3, 3, 0, 5),
            normal = new GUIStyleState { textColor = EditorStyles.label.normal.textColor }
        };

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
        EditorGUILayout.PropertyField(GestureLeftMask);
        EditorGUILayout.PropertyField(GestureRightMask);

        // Hand Poses
        GUILayout.Label("Hand Poses", headerStyle);
        GUILayout.Label("Array index maps to hand gesture parameter. Array length should be 8!", headerStyle2);
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
        GUILayout.Label("Facial expressions", headerStyle);
        GUILayout.Label("Brow and mouth blendshapes controlled by left and right hands." +
            "\nArray index maps to hand Gesture parameter. Array length should be 8!", headerStyle2);
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
        GUILayout.Label("Animator creation flags", headerStyle);
        GUILayout.Label("Choose what parts of the animator are generated." +
            "\nDisabling features saves VRC params budget!", headerStyle2);
        GUILayout.Space(10);
        EditorGUILayout.PropertyField(createShapePreferences);
        EditorGUILayout.PropertyField(createClothCustomization);
        EditorGUILayout.PropertyField(createColorCustomization);
        EditorGUILayout.PropertyField(createFaceToggle);
        if (wizard.createFaceTracking || wizard.createEyeTracking)
            EditorGUILayout.PropertyField(createOSCsmooth);
        EditorGUILayout.PropertyField(createEyeTracking);
        EditorGUILayout.PropertyField(createFaceTracking);

        // Shape Preferences
        if (wizard.createShapePreferences)
        {
            GUILayout.Label("Shape Preferences", headerStyle);
            GUILayout.Label("Creates VRC params for blendshapes with these prefixes.", headerStyle2);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(shapePreferenceSliderPrefix);
            EditorGUILayout.PropertyField(shapePreferenceTogglesPrefix);
        }

        // Cloths customization
        if (wizard.createClothCustomization)
        {
            GUILayout.Label("Cloths customization", headerStyle);
            GUILayout.Label("Creates an algorithm to switch clothes, animations \nand VRC params with these prefixes.", headerStyle2);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(ClothTogglesPrefix,
            PopUpLabel("Cloth Toggles Prefix", "Prefixes roll up clothes and body into \"tube\",\n" +
            "as well as regulates the fit of the cloth lower body to the cloth upper body."));
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(ClothUpperBodyNames);
            EditorGUILayout.PropertyField(ClothLowerBodyNames);
            EditorGUILayout.PropertyField(ClothFootNames);
        }

        // Color customization
        if (wizard.createColorCustomization)
        {
            GUILayout.Label("Color customization UV-offset animations", headerStyle);
            GUILayout.Label("Animations controlling color palette texture UV-offsets (ToonLit shader)" +
                "\nfor in-game color customization.", headerStyle2);
            EditorGUILayout.PropertyField(primaryColor0);
            EditorGUILayout.PropertyField(primaryColor1);
            EditorGUILayout.PropertyField(secondColor0);
            EditorGUILayout.PropertyField(secondColor1);
        }

        // Face Toggle
        if (wizard.createFaceToggle)
        {
            GUILayout.Label("FaceToggle setup animations", headerStyle);
            GUILayout.Label("Creates an algorithm to switch face animations.", headerStyle2);
            // Custom Face Toggle blocks
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(FaceToggleBlockParamNames,
                PopUpLabel("Face Toggle Block bool list", "Each element is a VRC bool parameter name. When any is True, Face Toggle won't work."));
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(FaceToggleNames);
        }

        // OSC smooth
        if ((wizard.createFaceTracking || wizard.createEyeTracking) && wizard.createOSCsmooth)
        {
            GUILayout.Label("OSC smooth setup", headerStyle);
            GUILayout.Label("OSC smooth is needed to fix Face/Eye Tracking params in-game, " +
                "\nas without it animation is choppy and jerky, as if it's lacking FPS.", headerStyle2);
            EditorGUILayout.PropertyField(localSmoothness);
            EditorGUILayout.PropertyField(remoteSmoothness);
        }
        // Eye Tracking
        if (wizard.createEyeTracking)
        {
            GUILayout.Label("Eye Tracking (Simplified Eye Parameters) settings", headerStyle);
            GUILayout.Label("Creates Eye Tracking with these animations.", headerStyle2);
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
            GUILayout.Label("Face Tracking (Universal Shapes) settings", headerStyle);
            GUILayout.Label("Creates Face Tracking with these animations.", headerStyle2);
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

#endif
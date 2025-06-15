using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using VRM;

public class ShapeKeyAutoSetup : EditorWindow
{
    //Made by Tracer755, Ritual neo was the one who requested this tools construction

    GameObject Avatar = null;
    string SaveLocation = "";
    string avatarName = "";
    Vector2 scrollPos;
    int blendShapeCount = 0;
    float progress = 0f;
    string[] arkit_lowwer = new string[0];
    string[] Unified_lowwer = new string[0];
    string[] other_lowwer = new string[0];
    int selectedMode = 0;
    string[] modeNames = new string[] {"ARKit", "Unified Expressions"};
    string relativePath = "";

    //these are the blendshapes that the tool looks for
    string[] arkit_BlendShapes = new string[] { "BrowDownLeft", "BrowDownRight", "BrowInnerUp", "BrowOuterUpLeft", "BrowOuterUpRight", "CheekPuff", "CheekSquintLeft", "CheekSquintRight", "EyeBlinkLeft", "EyeBlinkRight", "EyeLookDownLeft", "EyeLookDownRight", "EyeLookInLeft", "EyeLookInRight", "EyeLookOutLeft", "EyeLookOutRight", "EyeLookUpLeft", "EyeLookUpRight", "EyeSquintLeft", "EyeSquintRight", "EyeWideLeft", "EyeWideRight", "JawForward", "JawLeft", "JawOpen", "JawRight", "MouthClose", "MouthDimpleLeft", "MouthDimpleRight", "MouthFrownLeft", "MouthFrownRight", "MouthFunnel", "MouthLeft", "MouthLowerDownLeft", "MouthLowerDownRight", "MouthPressLeft", "MouthPressRight", "MouthPucker", "MouthRight", "MouthRollLower", "MouthRollUpper", "MouthShrugLower", "MouthShrugUpper", "MouthSmileLeft", "MouthSmileRight", "MouthStretchLeft", "MouthStretchRight", "MouthUpperUpLeft", "MouthUpperUpRight", "NoseSneerLeft", "NoseSneerRight", "TongueOut" };
    string[] Other_BlendShapes = new string[] { "v_aa", "v_e", "v_ee", "v_ih", "v_oh", "v_ou", "v_sil", "v_ch", "v_dd", "v_ff", "v_kk", "v_nn", "v_pp", "v_rr", "v_ss", "v_th", "LeftBlink", "RightBlink", "Blink", "vrc.v_aa", "vrc.v_e", "vrc.v_ee", "vrc.v_ih", "vrc.v_oh", "vrc.v_ou", "vrc.v_sil", "vrc.v_ch", "vrc.v_dd", "vrc.v_ff", "vrc.v_kk", "vrc.v_nn", "vrc.v_pp", "vrc.v_rr", "vrc.v_ss", "vrc.v_th", "aa", "e", "ee", "ih", "oh", "ou", "sil", "ch", "dd", "ff", "kk", "nn", "pp", "rr", "ss", "th" };
    string[] Other_BlendShapes_Names = new string[] { "A", "E", "E", "I", "O", "U", "SIL", "CH", "DD", "FF", "KK", "NN", "PP", "RR", "SS", "TH", "Blink_L", "Blink_R", "Blink", "A", "E", "E", "I", "O", "U", "SIL", "CH", "DD", "FF", "KK", "NN", "PP", "RR", "SS", "TH", "A", "E", "E", "I", "O", "U", "SIL", "CH", "DD", "FF", "KK", "NN", "PP", "RR", "SS", "TH" };
    string[] Unified_BlendShapes = new string[] { "EyeLookOutRight", "EyeLookInRight", "EyeLookUpRight", "EyeLookDownRight", "EyeLookOutLeft", "EyeLookInLeft", "EyeLookUpLeft", "EyeLookDownLeft", "EyeClosedRight", "EyeClosedLeft", "EyeSquintRight", "EyeSquintLeft", "EyeWideRight", "EyeWideLeft", "EyeDilationRight", "EyeDilationLeft", "EyeConstrictRight", "EyeConstrictLeft", "BrowPinchRight", "BrowPinchLeft", "BrowLowererRight", "BrowLowererLeft", "BrowInnerUpRight", "BrowInnerUpLeft", "BrowOuterUpRight", "BrowOuterUpLeft", "NoseSneerRight", "NoseSneerLeft", "NasalDilationRight", "NasalDilationLeft", "NasalConstrictRight", "NasalConstrictLeft", "CheekSquintRight", "CheekSquintLeft", "CheekPuffRight", "CheekPuffLeft", "CheekSuckRight", "CheekSuckLeft", "JawOpen", "MouthClosed", "JawRight", "JawLeft", "JawForward", "JawBackward", "JawClench", "JawMandibleRaise", "LipSuckUpperRight", "LipSuckUpperLeft", "LipSuckLowerRight", "LipSuckLowerLeft", "LipSuckCornerRight", "LipSuckCornerLeft", "LipFunnelUpperRight", "LipFunnelUpperLeft", "LipFunnelLowerRight", "LipFunnelLowerLeft", "LipPuckerUpperRight", "LipPuckerUpperLeft", "LipPuckerLowerRight", "LipPuckerLowerLeft", "MouthUpperUpRight", "MouthUpperUpLeft", "MouthLowerDownRight", "MouthLowerDownLeft", "MouthUpperDeepenRight", "MouthUpperDeepenLeft", "MouthUpperRight", "MouthUpperLeft", "MouthLowerRight", "MouthLowerLeft", "MouthCornerPullRight", "MouthCornerPullLeft", "MouthCornerSlantRight", "MouthCornerSlantLeft", "MouthFrownRight", "MouthFrownLeft", "MouthStretchRight", "MouthStretchLeft", "MouthDimpleRight", "MouthDimpleLeft", "MouthRaiserUpper", "MouthRaiserLower", "MouthPressRight", "MouthPressLeft", "MouthTightenerRight", "MouthTightenerLeft", "TongueOut", "TongueUp", "TongueDown", "TongueRight", "TongueLeft", "TongueRoll", "TongueBendDown", "TongueCurlUp", "TongueSquish", "TongueFlat", "TongueTwistRight", "TongueTwistLeft", "SoftPalateClose", "ThroatSwallow", "NeckFlexRight", "NeckFlexLeft", "EyeClosed", "EyeWide", "EyeSquint", "EyeDilation", "EyeConstrict", "BrowDownRight", "BrowDownLeft", "BrowDown", "BrowUpRight", "BrowUpLeft", "BrowUp", "NoseSneer", "NasalDilation", "NasalConstrict", "CheekPuff", "CheekSuck", "CheekSquint", "LipSuckUpper", "LipSuckLower", "LipSuck", "LipFunnelUpper", "LipFunnelLower", "LipFunnel", "LipPuckerUpper", "LipPuckerLower", "LipPucker", "MouthUpperUp", "MouthLowerDown", "MouthOpen", "MouthRight", "MouthLeft", "MouthSmileRight", "MouthSmileLeft", "MouthSmile", "MouthSadRight", "MouthSadLeft", "MouthSad", "MouthStretch", "MouthDimple", "MouthTightener", "MouthPress" };
    NamePreset[] NamePresets = new NamePreset[] { new NamePreset { name="A", blendShapePreset=BlendShapePreset.A}, new NamePreset { name = "E", blendShapePreset = BlendShapePreset.E }, new NamePreset { name = "I", blendShapePreset = BlendShapePreset.I }, new NamePreset { name = "O", blendShapePreset = BlendShapePreset.O }, new NamePreset { name = "U", blendShapePreset = BlendShapePreset.U }, new NamePreset { name = "Blink", blendShapePreset = BlendShapePreset.Blink }, new NamePreset { name = "Blink_R", blendShapePreset = BlendShapePreset.Blink_R }, new NamePreset { name = "Blink_L", blendShapePreset = BlendShapePreset.Blink_L } };
    [MenuItem("Tools/Vrm/Auto setup shapekeys")]
    
    public static void ShowWindow()
    {
        GetWindow(typeof(ShapeKeyAutoSetup));
    }

    private void OnGUI()
    {
        if (arkit_BlendShapes.Length == 0 || other_lowwer.Length == 0)
        {
            arkit_lowwer = Array.ConvertAll(arkit_BlendShapes, d => d.ToLower());
            Unified_lowwer = Array.ConvertAll(Unified_BlendShapes, d => d.ToLower());
            other_lowwer = Array.ConvertAll(Other_BlendShapes, d => d.ToLower());
        }
        EditorGUILayout.LabelField("Auto setup vrm blendshapes", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("\n\n", EditorStyles.whiteLabel);
        SaveLocation = EditorGUILayout.TextField("Save Location", SaveLocation);
        if (GUILayout.Button("Set Save Location"))
        {
            SaveLocation = EditorUtility.OpenFolderPanel("Save Folder", "", "");
        }
        EditorGUILayout.LabelField("\n\n", EditorStyles.whiteLabel);
        avatarName = EditorGUILayout.TextField("Avatar Name", avatarName);
        try
        {
            Avatar = EditorGUILayout.ObjectField("Avatar prefab", Avatar, typeof(GameObject), true) as GameObject;
        }
        catch (UnityEngine.ExitGUIException)
        {
            //Here so unity dosn't freak out
        }
        catch (System.Exception e)
        {
            throw (e);
        }
        EditorGUILayout.LabelField("\n\n", EditorStyles.boldLabel);
        bool buttonEnabled = true;
        string tempBtnMsg = "";

        if (SaveLocation == "")
        {
            tempBtnMsg += "\nPlease add a save location";
            buttonEnabled = false;
        }
        else
        {
            try
            {
                string path = "Assets/" + SaveLocation.Split(new[] { "Assets" }, StringSplitOptions.None)[1] + @"/Clips/test.asset";
            }
            catch
            {
                buttonEnabled = false;
                tempBtnMsg += "\nSave location must be in your projects assets folder";
            }
        }
        if (avatarName == "")
        {
            tempBtnMsg += "\nPlease give the avatar a name";
            buttonEnabled = false;
        }
        if (Avatar == null)
        {
            tempBtnMsg += "\nPlease input an avatar";
            buttonEnabled = false;
        }
        if (blendShapeCount == 0 && Avatar != null)
        {
            tempBtnMsg += "\nThere are no valid blendshapes on this avatar";
            buttonEnabled = false;
        }
        GUILayout.Label(tempBtnMsg.Trim());
        EditorGUILayout.LabelField("\n\n", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        var toggleModeButton = GUILayout.Button("Toggle Mode", GUILayout.Width(Screen.width * .35f));

        if (toggleModeButton)
        {
            selectedMode++;
            if(selectedMode > modeNames.Length - 1)
            {
                selectedMode = 0;
            }
        }

        GUILayout.Label("Mode: " + modeNames[selectedMode]);
        GUILayout.EndHorizontal();

        EditorGUILayout.LabelField("\n\n", EditorStyles.boldLabel);

        GUI.enabled = buttonEnabled;
        var setupBtn = GUILayout.Button("Setup Data!");
        GUI.enabled = true;
        if (setupBtn && selectedMode == 0)
        {
            progress = 0f;
            EditorUtility.DisplayProgressBar("Auto generate vrm | ARkit", "Starting", progress);
            var skinned_mesh = Avatar.GetComponent<SkinnedMeshRenderer>();
            var shared_mesh = skinned_mesh.sharedMesh;
            List<string> BlendShape = new List<string>();
            if (!Directory.Exists(SaveLocation + $@"/{avatarName}_Clips"))
            {
                Directory.CreateDirectory(SaveLocation + $@"/{avatarName}_Clips");
            }
            else
            {
                Directory.Delete(SaveLocation + $@"/{avatarName}_Clips", true);
                Directory.CreateDirectory(SaveLocation + $@"/{avatarName}_Clips");
            }
            //check for a avatar file that already exitsts
            if (File.Exists(Directory.GetCurrentDirectory() + @"/" + "Assets" + SaveLocation.Split(new[] { "Assets" }, StringSplitOptions.None)[1] + "/" + avatarName + "_AvatarBlendShape.asset"))
            {
                File.Delete(Directory.GetCurrentDirectory() + @"/" + "Assets" + SaveLocation.Split(new[] { "Assets" }, StringSplitOptions.None)[1] + "/" + avatarName + "_AvatarBlendShape.asset");
                File.Delete(Directory.GetCurrentDirectory() + @"/" + "Assets" + SaveLocation.Split(new[] { "Assets" }, StringSplitOptions.None)[1] + "/" + avatarName + "_AvatarBlendShape.asset.meta");
            }
            AssetDatabase.Refresh();
            //Generate Clips
            float tempProgValue = (float)(.8 / blendShapeCount);
            for (int i = 0; i < shared_mesh.blendShapeCount; i++)
            {
                string shape = shared_mesh.GetBlendShapeName(i).Trim().ToLower();
                int index = Array.IndexOf(arkit_lowwer, shape);
                //look for arkit face tracking
                if (index != -1)
                {
                    if (Array.IndexOf(BlendShape.ToArray(), arkit_BlendShapes[index]) != -1)
                    {
                        Debug.Log($"Avatar Blendshape {BlendShape[Array.IndexOf(BlendShape.ToArray(), arkit_BlendShapes[index])]} has multiple valid blendshapes, Avatar Blendshape {arkit_BlendShapes[index]} will not be used for this VRM shape");
                    }
                    else
                    {
                        progress += tempProgValue;
                        EditorUtility.DisplayProgressBar("Auto generate vrm", $"Generating {arkit_BlendShapes[index]} shape", progress);
                        BlendShape.Add(arkit_BlendShapes[index]);
                        var Clip = ScriptableObject.CreateInstance<BlendShapeClip>();
                        foreach(NamePreset obj in NamePresets)
                        {
                            if(obj.name == arkit_BlendShapes[index])
                            {
                                Clip.Preset = obj.blendShapePreset;
                            }
                        }
                        string path = "Assets/" + SaveLocation.Split(new[] { "Assets" }, StringSplitOptions.None)[1] + $@"/{avatarName}_Clips/" + arkit_BlendShapes[index] + ".asset";
                        Clip.BlendShapeName = arkit_BlendShapes[index];
                        var Data = new VRM.BlendShapeBinding();
                        Data.Weight = 100;
                        if (relativePath == "")
                        {
                            Data.RelativePath = Avatar.name;
                        }
                        else
                        {
                            Data.RelativePath = relativePath;
                        }
                        Data.Index = i;
                        var array = new VRM.BlendShapeBinding[1];
                        array[0] = Data;
                        Clip.Values = array;
                        AssetDatabase.CreateAsset(Clip, path);
                    }
                }
                else
                {
                    int index2 = Array.IndexOf(other_lowwer, shape);
                    //look for other blendshapes
                    if (index2 != -1)
                    {
                        if (Array.IndexOf(BlendShape.ToArray(), Other_BlendShapes_Names[index2]) != -1)
                        {
                            Debug.Log($"VRM Blendshape {BlendShape[Array.IndexOf(BlendShape.ToArray(), Other_BlendShapes_Names[index2])]} has multiple valid blendshapes, Avatar Blendshape {Other_BlendShapes_Names[index2]} will not be used for this VRM shape");
                        }
                        else
                        {
                            progress += tempProgValue;
                            EditorUtility.DisplayProgressBar("Auto generate vrm", $"Generating {Other_BlendShapes_Names[index2]} shape", progress);
                            BlendShape.Add(Other_BlendShapes_Names[index2]);
                            var Clip = ScriptableObject.CreateInstance<BlendShapeClip>();
                            foreach (NamePreset obj in NamePresets)
                            {
                                if (obj.name == Other_BlendShapes_Names[index2])
                                {
                                    Clip.Preset = obj.blendShapePreset;
                                }
                            }
                            string path = "Assets/" + SaveLocation.Split(new[] { "Assets" }, StringSplitOptions.None)[1] + $@"/{avatarName}_Clips/" + Other_BlendShapes_Names[index2] + ".asset";
                            Clip.BlendShapeName = Other_BlendShapes_Names[index2];
                            var Data = new VRM.BlendShapeBinding();
                            Data.Weight = 100;
                            if (relativePath == "")
                            {
                                Data.RelativePath = Avatar.name;
                            }
                            else
                            {
                                Data.RelativePath = relativePath;
                            }
                            Data.Index = i;
                            var array = new VRM.BlendShapeBinding[1];
                            array[0] = Data;
                            Clip.Values = array;
                            AssetDatabase.CreateAsset(Clip, path);
                        }
                    }
                }
            }
            //generate the avatar blendshape file before the assetdatabase refresh
            var AvatarData = ScriptableObject.CreateInstance<BlendShapeAvatar>();
            AssetDatabase.CreateAsset(AvatarData, "Assets" + SaveLocation.Split(new[] { "Assets" }, StringSplitOptions.None)[1] + "/" + avatarName + "_AvatarBlendShape.asset");
            BlendShape.Sort();
            //generate a neutral clip
            EditorUtility.DisplayProgressBar("Auto generate vrm", $"Generating neural shape", progress += tempProgValue);
            BlendShape.Insert(0, "Neutral");
            var Clip2 = ScriptableObject.CreateInstance<BlendShapeClip>();
            Clip2.Preset = BlendShapePreset.Neutral;
            Clip2.BlendShapeName = "Neutral";
            AssetDatabase.CreateAsset(Clip2, "Assets/" + SaveLocation.Split(new[] { "Assets" }, StringSplitOptions.None)[1] + $@"/{avatarName}_Clips/" + "Neutral" + ".asset");
            AssetDatabase.Refresh();
            //Add clips to a Blend Shape Avatar
            EditorUtility.DisplayProgressBar("Auto generate vrm", $"Adding shapes to VRM avatar file", .9f);
            List<string> guids = new List<string>();
            string path3 = SaveLocation + @"/" + avatarName + "_AvatarBlendShape.asset";
            StreamReader sr = new StreamReader(path3);
            string TmpData = sr.ReadToEnd();
            sr.Close();
            bool latch = true;
            TmpData = TmpData.Replace("  Clips: []", "  Clips:");
            foreach (var obj in BlendShape)
            {
                string[] lines = System.IO.File.ReadAllLines(SaveLocation + $@"/{avatarName}_Clips/" + obj + ".asset.meta");
                if (latch)
                {
                    TmpData += "  - {fileID:" + Convert.ToInt64(lines[4].Split(':')[1].Trim()) + ", guid: " + lines[1].Split(' ')[1].Trim() + ", type: 2}";
                }
                else
                {
                    TmpData += "\n  - {fileID:" + Convert.ToInt64(lines[4].Split(':')[1].Trim()) + ", guid: " + lines[1].Split(' ')[1].Trim() + ", type: 2}";
                }
                latch = false;
            }
            EditorUtility.DisplayProgressBar("Auto generate vrm", $"Finishing up", 1f);
            StreamWriter streamWriter = new StreamWriter(path3);
            streamWriter.Write(TmpData);
            streamWriter.Close();
            AssetDatabase.Refresh();
            Debug.Log($"Sucessfully created: {BlendShape.Count} vrm keys for avatar: {avatarName}");
            EditorGUIUtility.PingObject(AvatarData);
            EditorUtility.ClearProgressBar();
        }
        if (setupBtn && selectedMode == 1)
        {
            Debug.Log(Unified_lowwer);
            progress = 0f;
            EditorUtility.DisplayProgressBar("Auto generate vrm | UE", "Starting", progress);
            var skinned_mesh = Avatar.GetComponent<SkinnedMeshRenderer>();
            var shared_mesh = skinned_mesh.sharedMesh;
            List<string> BlendShape = new List<string>();
            if (!Directory.Exists(SaveLocation + $@"/{avatarName}_Clips"))
            {
                Directory.CreateDirectory(SaveLocation + $@"/{avatarName}_Clips");
            }
            else
            {
                Directory.Delete(SaveLocation + $@"/{avatarName}_Clips", true);
                Directory.CreateDirectory(SaveLocation + $@"/{avatarName}_Clips");
            }
            //check for a avatar file that already exitsts
            if (File.Exists(Directory.GetCurrentDirectory() + @"/" + "Assets" + SaveLocation.Split(new[] { "Assets" }, StringSplitOptions.None)[1] + "/" + avatarName + "_AvatarBlendShape.asset"))
            {
                File.Delete(Directory.GetCurrentDirectory() + @"/" + "Assets" + SaveLocation.Split(new[] { "Assets" }, StringSplitOptions.None)[1] + "/" + avatarName + "_AvatarBlendShape.asset");
                File.Delete(Directory.GetCurrentDirectory() + @"/" + "Assets" + SaveLocation.Split(new[] { "Assets" }, StringSplitOptions.None)[1] + "/" + avatarName + "_AvatarBlendShape.asset.meta");
            }
            AssetDatabase.Refresh();
            //Generate Clips
            float tempProgValue = (float)(.8 / blendShapeCount);
            for (int i = 0; i < shared_mesh.blendShapeCount; i++)
            {
                string shape = shared_mesh.GetBlendShapeName(i).Trim().ToLower();
                int index = Array.IndexOf(Unified_lowwer, shape);
                Debug.LogWarning(shape + " - " + index);
                //look for ue face tracking
                if (index != -1)
                {
                    if (Array.IndexOf(BlendShape.ToArray(), Unified_BlendShapes[index]) != -1)
                    {
                        Debug.Log($"Avatar Blendshape {BlendShape[Array.IndexOf(BlendShape.ToArray(), Unified_BlendShapes[index])]} has multiple valid blendshapes, Avatar Blendshape {Unified_BlendShapes[index]} will not be used for this VRM shape");
                    }
                    else
                    {
                        progress += tempProgValue;
                        EditorUtility.DisplayProgressBar("Auto generate vrm", $"Generating {Unified_BlendShapes[index]} shape", progress);
                        BlendShape.Add(Unified_BlendShapes[index]);
                        var Clip = ScriptableObject.CreateInstance<BlendShapeClip>();
                        foreach (NamePreset obj in NamePresets)
                        {
                            if (obj.name == Unified_BlendShapes[index])
                            {
                                Clip.Preset = obj.blendShapePreset;
                            }
                        }
                        string path = "Assets/" + SaveLocation.Split(new[] { "Assets" }, StringSplitOptions.None)[1] + $@"/{avatarName}_Clips/" + Unified_BlendShapes[index] + ".asset";
                        Clip.BlendShapeName = Unified_BlendShapes[index];
                        var Data = new VRM.BlendShapeBinding();
                        Data.Weight = 100;
                        if (relativePath == "")
                        {
                            Data.RelativePath = Avatar.name;
                        }
                        else
                        {
                            Data.RelativePath = relativePath;
                        }
                        Data.Index = i;
                        var array = new VRM.BlendShapeBinding[1];
                        array[0] = Data;
                        Clip.Values = array;
                        AssetDatabase.CreateAsset(Clip, path);
                    }
                }
                else
                {
                    int index2 = Array.IndexOf(other_lowwer, shape);
                    //look for other blendshapes
                    if (index2 != -1)
                    {
                        if (Array.IndexOf(BlendShape.ToArray(), Other_BlendShapes_Names[index2]) != -1)
                        {
                            Debug.Log($"VRM Blendshape {BlendShape[Array.IndexOf(BlendShape.ToArray(), Other_BlendShapes_Names[index2])]} has multiple valid blendshapes, Avatar Blendshape {Other_BlendShapes_Names[index2]} will not be used for this VRM shape");
                        }
                        else
                        {
                            progress += tempProgValue;
                            EditorUtility.DisplayProgressBar("Auto generate vrm", $"Generating {Other_BlendShapes_Names[index2]} shape", progress);
                            BlendShape.Add(Other_BlendShapes_Names[index2]);
                            var Clip = ScriptableObject.CreateInstance<BlendShapeClip>();
                            foreach (NamePreset obj in NamePresets)
                            {
                                if (obj.name == Other_BlendShapes_Names[index2])
                                {
                                    Clip.Preset = obj.blendShapePreset;
                                }
                            }
                            string path = "Assets/" + SaveLocation.Split(new[] { "Assets" }, StringSplitOptions.None)[1] + $@"/{avatarName}_Clips/" + Other_BlendShapes_Names[index2] + ".asset";
                            Clip.BlendShapeName = Other_BlendShapes_Names[index2];
                            var Data = new VRM.BlendShapeBinding();
                            Data.Weight = 100;
                            if (relativePath == "")
                            {
                                Data.RelativePath = Avatar.name;
                            }
                            else
                            {
                                Data.RelativePath = relativePath;
                            }
                            Data.Index = i;
                            var array = new VRM.BlendShapeBinding[1];
                            array[0] = Data;
                            Clip.Values = array;
                            AssetDatabase.CreateAsset(Clip, path);
                        }
                    }
                }
            }
            //generate the avatar blendshape file before the assetdatabase refresh
            var AvatarData = ScriptableObject.CreateInstance<BlendShapeAvatar>();
            AssetDatabase.CreateAsset(AvatarData, "Assets" + SaveLocation.Split(new[] { "Assets" }, StringSplitOptions.None)[1] + "/" + avatarName + "_AvatarBlendShape.asset");
            BlendShape.Sort();
            //generate a neutral clip
            EditorUtility.DisplayProgressBar("Auto generate vrm", $"Generating neural shape", progress += tempProgValue);
            BlendShape.Insert(0, "Neutral");
            var Clip2 = ScriptableObject.CreateInstance<BlendShapeClip>();
            Clip2.Preset = BlendShapePreset.Neutral;
            Clip2.BlendShapeName = "Neutral";
            AssetDatabase.CreateAsset(Clip2, "Assets/" + SaveLocation.Split(new[] { "Assets" }, StringSplitOptions.None)[1] + $@"/{avatarName}_Clips/" + "Neutral" + ".asset");
            AssetDatabase.Refresh();
            //Add clips to a Blend Shape Avatar
            EditorUtility.DisplayProgressBar("Auto generate vrm", $"Adding shapes to VRM avatar file", .9f);
            List<string> guids = new List<string>();
            string path3 = SaveLocation + @"/" + avatarName + "_AvatarBlendShape.asset";
            StreamReader sr = new StreamReader(path3);
            string TmpData = sr.ReadToEnd();
            sr.Close();
            bool latch = true;
            TmpData = TmpData.Replace("  Clips: []", "  Clips:");
            foreach (var obj in BlendShape)
            {
                string[] lines = System.IO.File.ReadAllLines(SaveLocation + $@"/{avatarName}_Clips/" + obj + ".asset.meta");
                if (latch)
                {
                    TmpData += "  - {fileID:" + Convert.ToInt64(lines[4].Split(':')[1].Trim()) + ", guid: " + lines[1].Split(' ')[1].Trim() + ", type: 2}";
                }
                else
                {
                    TmpData += "\n  - {fileID:" + Convert.ToInt64(lines[4].Split(':')[1].Trim()) + ", guid: " + lines[1].Split(' ')[1].Trim() + ", type: 2}";
                }
                latch = false;
            }
            EditorUtility.DisplayProgressBar("Auto generate vrm", $"Finishing up", 1f);
            StreamWriter streamWriter = new StreamWriter(path3);
            streamWriter.Write(TmpData);
            streamWriter.Close();
            AssetDatabase.Refresh();
            Debug.Log($"Sucessfully created: {BlendShape.Count} vrm keys for avatar: {avatarName}");
            EditorGUIUtility.PingObject(AvatarData);
            EditorUtility.ClearProgressBar();
        }

        if (Avatar != null)
        {
            try
            {
                var skinned_mesh = Avatar.GetComponent<SkinnedMeshRenderer>();
                var shared_mesh = skinned_mesh.sharedMesh;
                int count = 0;
                string tmpMsg = "";
                bool latch = true;
                if (selectedMode == 0)
                {
                    for (int i = 0; i < shared_mesh.blendShapeCount; i++)
                    {
                        if (Array.IndexOf(other_lowwer, shared_mesh.GetBlendShapeName(i).Trim().ToLower()) != -1 || Array.IndexOf(arkit_lowwer, shared_mesh.GetBlendShapeName(i).Trim().ToLower()) != -1)
                        {
                            count++;
                        }

                        if (latch)
                        {
                            tmpMsg += shared_mesh.GetBlendShapeName(i).Trim();
                        }
                        else
                        {
                            tmpMsg += "\n" + shared_mesh.GetBlendShapeName(i).Trim();
                        }

                        latch = false;
                    }
                }
                else if(selectedMode == 1)
                {
                    for (int i = 0; i < shared_mesh.blendShapeCount; i++)
                    {
                        if (Array.IndexOf(other_lowwer, shared_mesh.GetBlendShapeName(i).Trim().ToLower()) != -1 || Array.IndexOf(Unified_lowwer, shared_mesh.GetBlendShapeName(i).Trim().ToLower()) != -1)
                        {
                            count++;
                        }

                        if (latch)
                        {
                            tmpMsg += shared_mesh.GetBlendShapeName(i).Trim();
                        }
                        else
                        {
                            tmpMsg += "\n" + shared_mesh.GetBlendShapeName(i).Trim();
                        }

                        latch = false;
                    }
                }
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
                GUILayout.Label($"{shared_mesh.blendShapeCount} BlendShapes detected on avatar \n{count} are valid VRM shapes\n\n" + tmpMsg);
                EditorGUILayout.EndScrollView();
                blendShapeCount = count;
            }
            catch
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
                GUILayout.Label("No valid blend shapes detected on current avatar");
                EditorGUILayout.EndScrollView();
                blendShapeCount = 0;
            }
        }
        else
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
            GUILayout.Label("No selected avatar");
            EditorGUILayout.EndScrollView();
            blendShapeCount = 0;
        }
        if (GUILayout.Button("Tutorial"))
        {
            Application.OpenURL("https://www.youtube.com/watch?v=Jqlpn2Or6A8");
        }
        EditorGUILayout.LabelField("\n\n", EditorStyles.whiteLabel);
        EditorGUILayout.LabelField("Advanced Settings", EditorStyles.boldLabel);
        relativePath = EditorGUILayout.TextField("Relative Body", relativePath);
        this.Repaint();
    }
}
public class ClipValue
{
    public string RelativePath = "Body";
    public int Index;
    public float Weight;
}
class NamePreset
{
    public string name;
    public BlendShapePreset blendShapePreset;
}
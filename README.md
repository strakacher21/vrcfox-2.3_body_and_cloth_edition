# **VRCfox**
## Minimalistic furry avatar for [VRChat](https://hello.vrchat.com/) with flexible customization options.<br/>This is a fork of [vrcfox](https://github.com/trev3d/vrcfox) [(trev3d)](https://github.com/trev3d).

___

<img src="https://github.com/user-attachments/assets/35b75180-3948-40ba-bf3b-e762263e6933" width="50%">
<img src="https://github.com/user-attachments/assets/2923e6cc-4414-4ce5-bf1c-98b9995fa9a9" width="50%">
___

# А Request
Please do not use this model to post pornographic or suggestive content.<br/>

I will also be grateful if you could indicate my and trev3d's authorship.

*About all possible mistakes and wishes or criticism write me safely. I will be interested to answer everything or realize that I have created some silly thing :D*

# Get
### [:arrow_forward:Download](https://github.com/strakacher21/vrcfox-2.3_body_and_cloth_edition/archive/refs/heads/main.zip)

### [:arrow_forward:Link to the VRChat avatar](https://vrchat.com/home/avatar/avtr_433942b4-d25f-4add-ad34-75c0d20e4ae1)

# Guides
### [:bulb:Unity project setup](Unity-setup.md)
### [:bulb:VRM in Unity project setup](VRM-setup.md)

# Customization
The project includes the **Blend 4.5** file itself and the **Unity 2022.3.22f1** project.

The easiest way to adjust the colors is to vertex paint rather than using a texture. This works well for solid colors, but is not suitable for fancy patterns. Your avatar file size will stay small and load quickly without texture files. If you prefer to use a texture, the model has a set of **UVs**. You will need to change the **active UV layer** of each mesh to **‘UVMap (atlas) / UVMap’** and apply the texture yourself. You will also need to erase all vertex colors, as they will still appear on the default material in the Unity project!

> [!WARNING]
>**To properly export a model from Blender to Unity, you need to use the `export to unity` script in Blender!** 
>
>Simply click the '▶' button in Blender to export the model correctly to Unity </br>
>*(Without — the colors of the model in Unity look faded and the clothes are exported incorrectly, and other problems may occur!)*
>
> You can also adjust the export settings in the script.

The Unity project has a prefab model, as well as two scenes for **PC** and **Quest&IOS**. All prefab changes go into changing the scene. Аlso includes a script **AnimatorWizard** (uses the  [v3-animator-as-code](https://github.com/hai-vr/av3-animator-as-code) [(hai-vr)](https://github.com/hai-vr) package to configure animators) that allows you to customise facial expressions, avatar blend preferences, cloth/color customisation, eye/face tracking, etc. You can disable some features to save [VRChat parameters](https://creators.vrchat.com/avatars/animator-parameters/).<br/>

The project also includes a [VRM file](vrcfox%20unity%20project%20(B&C)/Assets/VRM%20avatar/vrm%20file) and a pre-configured VRM scene. Refer to the [VRM setup guide](VRM-setup.md) for switching to VRM.
___

### [Main avatar performance stats for VRChat](https://creators.vrchat.com/avatars/avatar-performance-ranking-system#avatar-performance-ranking-stats)

| **Category** | **Value** |
|:--------------|:----------|
| **Platform rating** | PC / Android / iOS: **Good** |
| **Triangles** | < 10k |
| **Bones** | < 75 |
| **Phys Bones** | 5 (PC) / 4 (Quest & iOS) |
| **Material** | 1 |
| **Mesh** | 1 |
| **Audio Source** | 1 (PC) |
| **Contact Receivers** | 2 (PC) / 1 (Quest & iOS) |
| **Contact Colliders** | 3 |
| **Download size** | < 1 MB |

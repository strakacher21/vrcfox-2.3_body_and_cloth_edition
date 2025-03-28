# **VRCfox**
## Minimalistic furry avatar for [VRChat](https://hello.vrchat.com/) with flexible customization options.<br>This is a fork of [vrcfox](https://github.com/trev3d/vrcfox) [(trev3d)](https://github.com/trev3d).

___
<img src="https://github.com/user-attachments/assets/35b75180-3948-40ba-bf3b-e762263e6933" width="50%"><img src="https://github.com/user-attachments/assets/2923e6cc-4414-4ce5-bf1c-98b9995fa9a9" width="50%">
___

# А Request
Please do not use this model to post pornographic or suggestive content.</br>
I will also be grateful if you indicate mine and trev3d's authorship (and not indicate the above content).

*About all possible mistakes and wishes or criticism write me safely. I will be interested to answer everything or realize that I have created some silly thing :D*

# Get
### [:arrow_forward:Download](https://github.com/strakacher21/vrcfox-2.3_body_and_cloth_edition/archive/refs/heads/main.zip)

### [:arrow_forward:Link to the avatar in VRChat](https://vrchat.com/home/avatar/avtr_433942b4-d25f-4add-ad34-75c0d20e4ae1)

# Customization
The project includes the **Blend 4.4** file itself and the **Unity 2022.3.22f1** project.

The easiest way to adjust the colors is to vertex paint rather than using a texture. This works well for solid colors, but is not suitable for fancy patterns. Your avatar file size will stay small and load quickly without texture files. If you prefer to use a texture, the model has a second set of **UVs**. You will need to change the **active UV layer** of each mesh to **‘UVMap’** and apply the texture yourself. You will also need to erase all vertex colors, as they will still appear on the default material in the Unity project! 

> [!WARNING]
>**To properly export a model from Blender to Unity, you need to use the export script in Blender!** 
>
>Simply click the '▶' button in Blender to export the model correctly to Unity </br>
>*(Without — the colors of the model in Unity look faded and the clothes are exported incorrectly.)*

The Unity project has a prefab model, as well as two scenes for **PC** and **Quest&IOS**. All prefab changes go into changing the scene. Аlso includes a script **AnimatorWizard** (uses the  [v3-animator-as-code](https://github.com/hai-vr/av3-animator-as-code) [(hai-vr)](https://github.com/hai-vr) package to configure animators) that allows you to customise facial expressions, avatar blend preferences, cloth/color customisation, eye/face tracking, etc. You can disable some features to save [VRChat parameters](https://creators.vrchat.com/avatars/animator-parameters/).

### [Main avatar performance stats for VRChat](https://creators.vrchat.com/avatars/avatar-performance-ranking-system#avatar-performance-ranking-stats)
>**PC/Android/IOS: Good**<br>
>___
>**Triangles: <10k**<br>
>**Bones: <75**<br>
>**Phys bones: 5 (PC) / 4 (Quest&IOS)**<br>
>**Material: 1**<br>
>**Mesh: 1**<br>
>**Audio Source: 1 (PC)**<br>
>**Contact Receivers: 2 (PC) / 1 (Quest&IOS)**<br>
>**Contact Colliders: 3**<br>
>**Download size: <1 mb**<br>
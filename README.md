# **VRCfox with body and clothes**
## Minimalistic optimized furry avatar for [VRChat](https://hello.vrchat.com/) with changeable clothes and [VRCFaceTracking](https://github.com/benaclejames/VRCFaceTracking).<br>This is a modified project [vrcfox](https://github.com/trev3d/vrcfox) [(trev3d)](https://github.com/trev3d).
### [:arrow_forward:Download](https://github.com/strakacher21/vrcfox-2.3_body_and_cloth_edition/archive/refs/heads/main.zip)

### [:arrow_forward:Link to the avatar in VRChat](https://vrchat.com/home/avatar/avtr_433942b4-d25f-4add-ad34-75c0d20e4ae1)
---
<img src="https://github.com/user-attachments/assets/d0bab74a-0902-4086-8fcd-b7249ce140b0" width="50%"><img src="https://github.com/user-attachments/assets/2923e6cc-4414-4ce5-bf1c-98b9995fa9a9" width="50%">

---
# А Request
I agree with trev3d and [also say](https://github.com/trev3d/vrcfox?tab=readme-ov-file#a-request-%EF%B8%8F):
>Given the license, I cannot legally stop you, but I will still politely ask - please do not use this model to post pornographic or suggestive content.

I will also be grateful if you indicate my authorship and the authorship of the original (and do not indicate authorship in pornographic or suggestive content, if you still want to do it).

*About all possible mistakes and wishes or criticism write me safely. I will be interested to answer everything or realize that I have created some silly thing :D*.
# Description
This is a modified [vrcfox-2.3 project](https://github.com/trev3d/vrcfox/releases/tag/2.3) with added new clothes and also made body geometry and many other additional customization options.
The project includes the **Blend 4.3.2** file itself and the **Unity 2022.3.22f1** project.

The easiest way to adjust the colours is to vertex paint rather than using a texture. This works well for solid colours, but is not suitable for fancy patterns. Your avatar file size will stay small and load quickly without texture files. If you prefer to use a texture, the model has a second set of **UVs**. You will need to change the **active UV layer** of each mesh to **‘UVMap’** and apply the texture yourself. You will also need to erase all vertex colours, as they will still appear on the default material in the Unity project! 
> [!WARNING]
**To ensure proper export of the model from Blender to Unity, use the attached export script in Blender!** Without this script, the model's colors may appear faded in Unity, and clothing may be exported incorrectly. Simply click the '▶' button in Blender to export the model correctly to Unity.

The Unity project has a prefab model, as well as two scenes for **PC** and **Quest&IOS**. All prefab changes go into changing the scene. Аlso includes a script **AnimatorWizard** (uses the hai-rus [v3-animator-as-code package](https://github.com/hai-vr/av3-animator-as-code) to configure animators) that allows you to customise facial expressions, avatar blend preferences, cloth customisation, eye tracking, face tracking and more. You can disable some features to save [VRChat parameters](https://creators.vrchat.com/avatars/animator-parameters/).

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
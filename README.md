# **VRCfox** **with body and clothes**
## Minimalistic optimized furry avatar for VRChat with changeable clothes and FaceTracking.<br>This is a **modified project** from [**GitHub**](https://github.com/trev3d/vrcfox) **[(trev3d)](https://github.com/trev3d)**.
# Get this
### [:arrow_forward:Download](https://github.com/strakacher21/vrcfox-2.3_body_and_cloth_edition/archive/refs/heads/main.zip)

### [:arrow_forward:Link to the avatar in VRChat](https://vrchat.com/home/avatar/avtr_433942b4-d25f-4add-ad34-75c0d20e4ae1)

<img src="https://github.com/user-attachments/assets/d0bab74a-0902-4086-8fcd-b7249ce140b0" width="50%"><img src="https://github.com/user-attachments/assets/2923e6cc-4414-4ce5-bf1c-98b9995fa9a9" width="50%">

---
# А Request
I will agree with trev3d and also say 
>Given the license, I cannot legally stop you, but I will still politely ask - please do not use this model to post pornographic or suggestive content.

I will also be grateful if you indicate my authorship and the authorship of the original (and do not indicate authorship in pornographic or suggestive content, if you still want to do it).

*About all possible mistakes and wishes or criticism write me safely. I will be interested to answer everything or realize that I have created some silly thing :D*
# Description
This is a modified vrcfox-2.3 project with added new clothes and also made body geometry.
The project includes the **Blend 4.2** file itself and the **Unity 2022.3.22f1** project.

The easiest way to adjust the colours is to vertex paint rather than using a texture. This works well for solid colours, but is not suitable for fancy patterns. Your avatar file size will stay small and load quickly without texture files. If you prefer to use a texture, the model has a second set of UVs. You will need to change the active UV layer of each mesh to ‘UVMap’ and apply the texture yourself. You will also need to erase all vertex colours, as they will still appear on the default material in the Unity project! 
> [!WARNING]
To properly export the model from blender to unity, you must use the export script attached to blender (otherwise it will ‘fade’ [exports colors type ‘SRGB‘, instead of ‘LINEAR‘] the model and export the clothes to unity incorrectly[without combining meshes and blendshapes]). Clicking the '▶' button will export the model to Unity.

The Unity project has a prefab model, as well as two scenes for **PC** and **Quest&IOS**. All prefab changes go into changing the scene. Аlso includes a script [AnimatorWizard (av3-animator-as-code)](https://github.com/hai-vr/av3-animator-as-code) (*Not pre-modded at the moment, I'll try to find time to make it good*) that allows you to easily customise facial expressions, avatar blend prefs and face tracking features. You can disable some features to save on VRChat settings, or add your own combinations of shapes for facial expressions, shape customisation, clothing switching, face tracking, etc.

### Performans Rating for VRChat
>**PC/Android&IOS: Good**<br>
>___
>**Polygons (Triangles): <10000**<br>
>**Phys bones: 5**<br>
>**Material: 1**<br>
>**Mesh: 1**<br>
>**Audio Source: 1 (PC)**<br>
>**Contact Receivers: 2**<br>
>**Contact Colliders: 3**<br>
>**Download file size: <1 mb** 

>[!NOTE]
> - To think about blendshapes **v2/MouthCornerPullLeft** and **v2/MouthCornerPullRight**, as the **v2/SmileSad** and **v2/MouthStretch** splits were expected to perform the same function, but alas in game they maxed out at **30%** of their value. Most likely we should remove the separation (make it as it was) and make separate blendshapes for **v2/MouthCornerPullLeft** and **v2/MouthCornerPullRight**;
> - Improve blendshapes: **v2/CheekPuff** and **v2/CheekPuffSuck**;
> - Improve the **eyetracking** system through parameters, and make it create automatically (and see if you need to add OSCmooth to it);
> - Add a **clothing creation** system to the **Animator Wizard** and the ability to **add new clothing**;
> - Implement OSCmooth functionality in AnimatorWizard;
> ___
> - Add new presets for faces (FaceToggle);
> - Improve locomotion;
> - Review the settings of all Physbones, especially about the Physbone tongue;
> - Revisit some of the weights for the rig.
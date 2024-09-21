# **VRCfox** <u>**with body and clothes**</u>
## Minimalistic optimized furry avatar for vrchat with <u>changeable clothes</u> and <u>FullFaceTracking.</u><br>This is a **modified project** from [**GitHub**](https://github.com/trev3d/vrcfox) **[(trev3d)](https://github.com/trev3d)**
# Get this
### [:arrow_forward:Download](https://github.com/strakacher21/vrcfox-2.3_body_and_cloth_edition/releases)

### [:arrow_forward:Link to the avatar in VRChat](https://vrchat.com/home/avatar/avtr_433942b4-d25f-4add-ad34-75c0d20e4ae1)

![vrcfox fox edition](https://github.com/user-attachments/assets/8d1e70bf-68d4-401d-a4b9-c471893ad1f3)
---
# А Request
I will agree with trev3d and also say 
>Given the license, I cannot legally stop you, but I will still politely ask - please do not use this model to post pornographic or suggestive content.

I will also be grateful if you indicate my authorship and the authorship of the original (and do not indicate authorship in pornographic or suggestive content, if you still want to do it)
# Description
This is a modified vrcfox-2.3 project with added new clothes and also made body geometry
The project includes the **Blend 4.2** file itself and the **Unity 2022.3.22f1** project.

The easiest way to adjust the colours is to vertex paint rather than using a texture. This works well for solid colours, but is not suitable for fancy patterns. Your avatar file size will stay small and load quickly without texture files. If you prefer to use a texture, the model has a second set of UVs. You will need to change the active UV layer of each mesh to ‘UVMap’ and apply the texture yourself. You will also need to erase all vertex colours, as they will still appear on the default material in the Unity project! 
> [!WARNING]
To properly export the model from blender to unity, you must use the export script attached to blender (otherwise it will ‘fade’ the model and export the clothes to unity incorrectly). Clicking the '▶' button will export the model to Unity.

The Unity project has a prefab model, as well as two scenes for PC and Quest&IOS. All prefab changes go into changing the scene. Аlso includes a script [[av3-animator-as-code]](https://github.com/hai-vr/av3-animator-as-code) that allows you to easily customise facial expressions, player preferences and face tracking features. You can disable some features to save on VRChat settings, or add your own combinations of shapes for facial expressions, shape customisation, clothing switching, face tracking, etc.
### Avatar Features
- Created FullFaceTracking (FFT)
- Numerous body prefs have been created (also applies to clothing)
- A "face constructor" has been made. In fact, this is a manual FFT control
- A collection of faces for different facial expressions without using FFT
- Different finger expressions on each hand
- Animation of idle tail (tail WAG) as desired by the user
- Idle poses for standing still and lying down
- Gesture mimic off (A setting that disables linking facial expressions to hand positions when enabled).
- The sound of pressing on the nose, as well as the reaction of the avatar's face to the approach of another avatar's hand
- The ‘anim base’ controller, which fixes the ‘crab’ movement bug when the stick angle is small to normal.
- The ‘anim sitting‘  controller allows FBT users to move their legs when they are sitting on something (such as a chair)
- The rest is small stuff :D
### Performans Rating</br>
>PC: Excellent </br>
>Android&IOS: Good

>[!TIP]
># Ways to optimize the avatar for parameters
>- If you don't need a ‘collection of faces’, then remove the ‘FaceInt’ variable >from ‘param’ (it is responsible for switching facial expressions) and you also >need to remove the ‘Face Toogle (menu)’ Layer in the animfx controller
>
>- If you need only one type of clothing (without switching) to save parameters, you can also remove the variables ‘Cloth Upper body’, ‘Cloth Lower body’, ‘Cloth Foot’ from ‘param’. Then go to the fx-controller and change the values of the variables in it as you need. You can also delete a certain clothing mesh in Blender and export it to Unity using a script. This will save you triangles, as there will be no extra clothes.
>
>- There is also a colour change in the avatar. If you only want one colour permanently, go to ‘param’ and remove the variables ‘pref/slider/pcol’ and ‘pref/slider/scol’, then open the fx-controller and remove the ‘pref/slider/pcol’ and ‘pref/slider/scol’ motion field in the ‘vrcfox__tree’->‘master tree’ layer. Then find ‘master material’ and change the ‘offset’ to your liking.
>
>- You can also delete "audio source" from "VRC Spatial Audio Source" and "contact Receiver [where there is a parameter: nose_contact_sound]" in the "head" of the avatar for the PC version. Next, anim fx, remove the parameter and player "nose_contact_sound". After that, you can use the Quest&iOS scene for PC as well.
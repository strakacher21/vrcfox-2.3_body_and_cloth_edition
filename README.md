# **VRCfox** **with body and clothes**
## Minimalistic optimized furry avatar for vrchat with changeable clothes and FaceTracking.<br>This is a **modified project** from [**GitHub**](https://github.com/trev3d/vrcfox) **[(trev3d)](https://github.com/trev3d)**
# Get this
### [:arrow_forward:Download](https://github.com/strakacher21/vrcfox-2.3_body_and_cloth_edition/archive/refs/heads/main.zip)

### [:arrow_forward:Link to the avatar in VRChat](https://vrchat.com/home/avatar/avtr_433942b4-d25f-4add-ad34-75c0d20e4ae1)

![vrcfox fox edition](https://github.com/user-attachments/assets/8d1e70bf-68d4-401d-a4b9-c471893ad1f3)
---
# А Request
I will agree with trev3d and also say 
>Given the license, I cannot legally stop you, but I will still politely ask - please do not use this model to post pornographic or suggestive content.

I will also be grateful if you indicate my authorship and the authorship of the original (and do not indicate authorship in pornographic or suggestive content, if you still want to do it).

About all possible mistakes and wishes or criticism write me safely. I will be interested to answer everything or realize that I have created some silly thing :D
# Description
This is a modified vrcfox-2.3 project with added new clothes and also made body geometry
The project includes the **Blend 4.2** file itself and the **Unity 2022.3.22f1** project.

The easiest way to adjust the colours is to vertex paint rather than using a texture. This works well for solid colours, but is not suitable for fancy patterns. Your avatar file size will stay small and load quickly without texture files. If you prefer to use a texture, the model has a second set of UVs. You will need to change the active UV layer of each mesh to ‘UVMap’ and apply the texture yourself. You will also need to erase all vertex colours, as they will still appear on the default material in the Unity project! 
> [!WARNING]
To properly export the model from blender to unity, you must use the export script attached to blender (otherwise it will ‘fade’ [exports colors type ‘SRGB‘, instead of ‘LINEAR‘] the model and export the clothes to unity incorrectly[without combining meshes and blendshapes]). Clicking the '▶' button will export the model to Unity.

The Unity project has a prefab model, as well as two scenes for PC and Quest&IOS. All prefab changes go into changing the scene. Аlso includes a script [[AnimatorWizard (av3-animator-as-code)]](https://github.com/hai-vr/av3-animator-as-code) that allows you to easily customise facial expressions, player preferences and face tracking features. You can disable some features to save on VRChat settings, or add your own combinations of shapes for facial expressions, shape customisation, clothing switching, face tracking, etc.
### Avatar Features
- Created FaceTracking (FT).
- Added [OSCmooth](https://github.com/regzo2/OSCmooth) for better face tracking.
- Numerous body prefs have been created (also applies to clothing).
- A ‘face builder‘ was created. In essence, it is a manual FT control, just like the ‘eye control‘. It is also conventionally a part-time debugger.
- A collection of faces for different facial expressions without using FT.
- Different finger expressions on each hand.
- Animation of idle tail (tail WAG) as desired by the user. You can also disable  floor/leg colliders in the menu for it.
- Idle poses for standing still and lying down.
- Facial Expressions (A setting that enables/disables linking facial expressions to hand positions when enabled).
- Lipsync (The parameter that enables/disables it)
- The sound of pressing on the nose, as well as the reaction of the avatar's face to the approach of another avatar's hand.
- It is possible to disable ’Facial expressions’ and ’Pet expressions’ in the game.
- The ‘anim base’ controller, which fixes the ‘crab’ movement bug when the stick angle is small to normal.
- The ‘anim sitting‘  controller allows FBT users to move their legs when they are sitting on something (such as a chair).
- The ears move from the position of the eyelids [for FT and turn on at the user's request].
- The rest is small stuff :D
### Performans Rating
>**PC: Excellent**<br>
>**Android&IOS: Good**<br>
>___
>**Polygons (Triangles): <10000**<br>
>**Phys bones: 4**<br>
>**Material: 1**<br>
>**Mesh: 1**<br>
>**Audio Source: 1 (PC)**<br>
>**Contact Receivers: 2**<br>
>**Contact Colliders: 3**<br>
>**Download file size: 1 mb** 

>[!TIP]
># Ways to optimize the avatar for parameters
>### Right now 256/256 are occupied, if you want to add something extra to your avatar it's worth thinking about these tips.
>#### There is no full optimization provided here, it only makes the avatar much lighter in parameters .
>- If you don't need a ‘collection of faces’, then remove the ’v2/anim/FacePresets’ variable >from ‘param’ (it is responsible for switching facial expressions) and you also >need to remove the ‘Face Toogle (menu)’ Layer in the ‘animfx‘ controller.
>
>- If you need only one type of clothing (without switching) to save parameters, you can also remove the variables ‘cloth/UpperBody’, ‘cloth/LowerBody’, ‘cloth/Foot’ from ‘param’. Then go to the fx-controller and change the values of the variables in it as you need. You can also delete a certain clothing mesh in Blender and export it to Unity using a script. This will save you triangles, as there will be no extra clothes.
>
>- There is also a colour change in the avatar. If you only want one colour permanently, go to ‘param’ and remove the variables ‘pref/slider/pcol’ and ‘pref/slider/scol’, then open the fx-controller and remove the ‘pref/slider/pcol’ and ‘pref/slider/scol’ motion field in the ‘vrcfox__tree’->‘master tree’ layer. Then find ‘master material’ and change the ‘offset’ to your liking.
>
>- If you do not need ‘Eye Control‘, just remove ‘eye/Control‘ and ‘eye/Control[X/Y]‘ from the parameters, and then go to the ‘anim additive‘ controller and remove the same parameters there and Layer ‘Eye Control‘.
>- If you want Tail WAG animations, then remove ’tail/anim/TaillInt’ from the parameters, and then go to the ‘anime additive‘ controller and remove the Layer ‘Tail Toogle (menu)‘. If you need only one animation, then replace ‘TaillInt‘ with a bool variable, and make the Layer ‘Tail Toggle (menu)‘ toggle similar to Layer ‘Tail Floor Collider‘ or ‘AFK Emote‘.
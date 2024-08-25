# **VRCfox** <u>**with body and clothes**</u>
## Minimalistic optimized furry avatar for vrchat with <u>changeable clothes</u> and <u>FullFaceTracking.</u><br>This is a **modified project** from [**GitHub**](https://github.com/trev3d/vrcfox)<br>The **author** of the original [trev3d](https://github.com/trev3d)
# Get this
## [:arrow_forward:Link to the avatar in VRChat](https://vrchat.com/home/avatar/avtr_433942b4-d25f-4add-ad34-75c0d20e4ae1)<br>[:arrow_forward:Download the modified version](https://github.com/strakacher21/vrcfox-2.3_body_and_cloth_edition/releases)<br>[:arrow_forward:Download the "vanilla" version](https://github.com/cellomonster/vrcfox/releases/latest)
![vrcfox fox edition](vrcfox%20unity%20project/Assets/icons/vrcfox_body_with_background_and_icons.png)
---
# А Request
I will agree with trev3d and also say 
>Given the license, I cannot legally stop you, but I will still politely ask - please do not use this model to post pornographic or suggestive content.

I will also be grateful if you indicate my authorship and the authorship of the original
# Description
This is a modified vrcfox-2.3 project with added new clothes and also made body geometry
The project includes the Blend 4.2 file itself and the Unity 2022.3.22f1 project.

The easiest way to adjust the colours is to vertex paint rather than using a texture. This works well for solid colours, but is not suitable for fancy patterns. Your avatar file size will stay small and load quickly without texture files. If you prefer to use a texture, the model has a second set of UVs. You will need to change the active UV layer of each mesh to ‘UVMap’ and apply the texture yourself. You will also need to erase all vertex colours, as they will still appear on the default material in the Unity project! 

To properly export the model from blender to unity, you must use the export script attached to blender (otherwise it will ‘fade’ the model and export the clothes to unity incorrectly). Clicking the '▶' button will export the model to Unity.

**WHAT DOES A UNITY PROJECT INCLUDE?**

- In the unity itself there are two prefabs - one for PC, the other for Quest (Android) version, as well as a single prefab scene settings. Most importantly, all changes in the prefab, say for PC, remain in it and are not transferred to the Quest-version (except for the scene setting prefab)! This is done in order to avoid audio blocking (pressing on the nose plays the ‘squeaker’ sound) on Quest-version (you simply can't upload audio files to Quest-version because of VRChat optimisation podlicy. When you publish a model, the Quest version will automatically lose the sound audio file).
When exporting for PC use ‘av PC’-scene, and for Quest use ‘av Quest’-scene.
You can remove the audio file inside the PC prefab and use one prefab for two platforms.
- Created FullFaceTracking (FFT)
- A small collection of faces for different facial expressions without using FFT
- Different finger expressions on each hand
- Animation of idle tail as desired by the user
- Small avatar head customisation (I plan to make more of them in the future, which will extend to clothes!), and colour changes.
- Gesture mimic off (A setting that disables linking facial expressions to hand positions when enabled).
- The ‘anim base’ controller, which fixes the ‘crab’ movement bug when the stick angle is small to normal, as well as the ‘Hot Lay Down’ pose when you're lying on your back or ‘gentlemen idle’ pose when you're using an avatar without VR and when it's standing still.
- Controller ‘anim Sitting’ which allows people with FBT to move their legs when sitting on something (This is to keep tracking going, rather than using the standard animation to do so)
- The rest of the things are small things ;)
# Ways to optimize avatar

- If you don't need a ‘collection of faces’, then remove the ‘FaceInt’ variable from ‘param’ (it is responsible for switching facial expressions) and you also need to remove the ‘Face Toogle (menu)’ Layer in the animfx controller

- If you need only one type of clothing (without switching) to save parameters, you can also remove the variables ‘Cloth Upper body’, ‘Cloth Lower body’, ‘Cloth Foot’ from ‘param’. Then go to the fx-controller and change the values of the variables in it as you need. 

- There is also a colour change in the avatar. If you only want one colour permanently, go to ‘param’ and remove the variables ‘pref/slider/pcol’ and ‘pref/slider/scol’, then open the fx-controller and remove the ‘pref/slider/pcol’ and ‘pref/slider/scol’ motion field in the ‘vrcfox__tree’->‘master tree’ layer. Then find ‘master material’ and change the ‘offset’ to your liking.

- The Unity project also includes a script (attached to the avatar build) that allows you to easily customise facial expressions, player preferences and face tracking features. You can disable some features to save on VRChat settings, or add your own combinations of shapes for facial expressions, shape customisation, clothing switching, face tracking, etc.

# Attribution
- To set up the animation, the following was used [av3-animator-as-code](https://github.com/hai-vr/av3-animator-as-code)<br>
- Photo bench: https://www.turbosquid.com/ru/3d-models/3d-model-bench01-1982255<br>
- Animation [spirit's gentalmen idle pose](https://vrcmods.com/download/9473)<br>
- Animation [Hot Lay Down](https://vrcmods.com/item?id=10697)
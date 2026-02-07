# GMod armature setup

Follow the steps below to fit your armature (rig) to ValveBiped and get the final DMX for further setup.

1. **Install Blender and Source Tools**

Download and install Blender 5.0:
https://www.blender.org/download/

Blender is required to work with the scene, run scripts, and export/import DMX via Blender Source Tools.

Download and install the Blender Source Tools add-on:
http://steamreview.org/BlenderSourceTools/

It is required to create a DMX file.

2. **Open `Armature proportion trick.blend`**

3. **Rename bones to match ValveBiped**

Rename bones so they match ValveBiped naming (format `ValveBiped.Bip01...`).
If some bones do not match the list, fix the mapping in the renaming script and run it again.

4. **Scale the avatar**

You need to change the overall avatar scale because Blender and Source use different units.
Open the scale script, set the required factor, and run it.

5. **First export to DMX**

Open Properties → Scene and find the Source Engine Export panel.
Click `Scene Export` and export the scene to DMX.
Make sure `Weight Link Cull Threshold` = `0.20`.

6. **Revert the scene and import DMX back**

Use **File → Revert** to return the file to its original state (before export/changes in the current session).
Switch from workspace `1) rename bones namespace` to workspace `2) proportion trick`.
Import the **DMX file** you exported in the previous step with `Bone Append Mode: Make New Armature`.

7. **Apply proportion trick**

***Main workflow***

Move everything related to the imported DMX into the `main` collection, and make sure nothing is left outside `main` and nothing ends up inside the template collections.

Then run the script `armature-proportion-trick.py`.

<details>
<summary>
<b><i>Legacy workflow</b> (if the automatic setup didn’t work because of the armature spine-bone setup)</b></i>
</summary>

This workflow uses two scripts: `proportion_trick1-legacy.py` and `proportion_trick2-legacy.py`.

You must rename the imported armature to `gg`.

Then run `proportion_trick1-legacy.py`.

After that, do a manual fix in Pose Mode: you need the constraints from `ValveBiped.Bip01_Spine1` to be applied to `ValveBiped.Bip01_Spine` using `Copy Constraints To Selected Bones`.

Then run `Apply Pose as Rest Pose`, after that select all bones and run `Clear Pose Constraints`.
Finally, run `proportion_trick2-legacy.py` - it merges non-ValveBiped bones into the proportion rig and sets the meshes’ Armature modifier to `proportions`.
</details>

8. **Rename main to the final DMX name**

Rename the `main` collection to the desired final filename.

9. **Final export to DMX**

Export the scene again via `Scene Export`, just like in step 5.

The final DMX (after the proportion trick) is the file you use next.
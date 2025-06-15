# VRM in Unity Project Setup

This guide explains how to set up a VRM model for the Unity project, including preparing FBX files and configuring the model in Unity. This process is for transforming the project for VRM use, independent of VRChat.

1. **Complete the Unity Project Setup**  
   Follow the steps in the [Unity project setup guide](Unity-setup.md) to ensure the Unity project is properly configured with all dependencies.

2. **Install Blender 4.4**  
   Download and install [Blender 4.4](https://www.blender.org/download/), as it is required for preparing FBX files for VRM format.

3. **Rename Bones for VRM Compatibility**  
   In Blender, switch to the **Scripting** workspace. Open the script editor by clicking the notepad icon, then select the `"rename bones VRM namespace"` script. Set `VRM_namespace = True` and run the script to rename the bones to the standard VRM format.

4. **Remove Face Tracking shapekeys prefixes**  
   In the same **Scripting** workspace, select the `"batch remove prefix ftshapekeys"` script and set `ftprefix = False`. Run the script to remove unnecessary prefixes from shapekeys.

5. **Export the Model**  
   Export the model using the `"export to unity"` script. *Before exporting, Pay attention to the settings:*  
   ```
   export_uv_map = "ColorMap"  # ColorMap / UVMap
   export_vertex_colors = True  # True / False
   ```  
6. **Configure the Rig in Unity**  
   In the Unity project, select the `vrcfox model (B&C)` in the Project window. In the **Inspector**, locate the **Rig** settings and click **Configure...**. Then, under **Mapping**, click **Load** and select the file at `VRM avatar/avatar rig templates/VRM avatar template.ht`. Reset the pose by clicking **Pose > Reset**, then click **Done**.  
   *This ensures proper bone mapping and enables correct eye tracking via blendshapes.*

7. **Open the VRM Scene**  
   Go to the `av VRM` scene and you're all set!
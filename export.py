# EXPORT SCRIPT - click '▶' on menu bar above to export to Unity
import bpy
import os

# Export settings
export_path = bpy.path.abspath(r"//vrcfox unity project (B&C)/Assets")
file_name = "vrcfox model (B&C).fbx"
desired_model_name = "Body"
export_uv_map = "ColorMap"
export_collection_name = "main"
exclude_collection_name = "disabled"

# Get collections
export_collection = bpy.data.collections.get(export_collection_name)
exclude_collection = bpy.data.collections.get(exclude_collection_name)
if not export_collection:
    raise ValueError(f"Collection '{export_collection_name}' not found.")

# Check if object belongs to an excluded collection
def in_exclude_collection(obj):
    return exclude_collection and obj.name in [o.name for o in exclude_collection.all_objects]

# Prepare objects for export
export_objects = [obj for obj in export_collection.all_objects
                  if obj.type == "MESH" and not in_exclude_collection(obj)]

for obj in export_objects:
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    for m in obj.modifiers:
        if m.type != "ARMATURE":
            bpy.ops.object.modifier_apply(modifier=m.name)

# Process armature objects
for obj in export_collection.all_objects:
    if obj.type == "ARMATURE" and not in_exclude_collection(obj):
        obj.data.pose_position = 'POSE'

# Make all objects visible
for obj in bpy.data.objects:
    obj.hide_set(False)

# Join objects and export
bpy.ops.object.select_all(action='DESELECT')
for obj in export_objects:
    obj.select_set(True)

if bpy.context.selected_objects:
    bpy.context.view_layer.objects.active = bpy.context.selected_objects[0]
    bpy.ops.object.join()
    bpy.context.active_object.name = desired_model_name

    if export_uv_map in bpy.context.object.data.uv_layers:
        bpy.context.object.data.uv_layers[export_uv_map].active = True
    else:
        raise ValueError(f"UV map '{export_uv_map}' not found.")

    # Set main collection as active
    export_layer_collection = bpy.context.view_layer.layer_collection.children[export_collection_name]
    bpy.context.view_layer.active_layer_collection = export_layer_collection

    os.makedirs(export_path, exist_ok=True)
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(export_path, file_name),
        check_existing=False,
        use_active_collection=True,
        bake_space_transform=True,
        object_types={'ARMATURE', 'MESH'},
        use_mesh_modifiers=False,
        use_mesh_modifiers_render=False,
        bake_anim_use_all_bones=False,
        bake_anim_force_startend_keying=False,
        bake_anim_simplify_factor=0.0,
        colors_type="LINEAR",
        use_armature_deform_only=True,
        use_triangles=False,
    )

    bpy.ops.ed.undo_push()
    bpy.ops.ed.undo()
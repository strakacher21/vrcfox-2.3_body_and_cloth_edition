# EXPORT SCRIPT - click '▶' on menu bar above to export to Unity
import bpy
import os
import bmesh

# Export settings
export_path = bpy.path.abspath(r"//../vrcfox unity project (B&C) (special version)/Assets")
file_name = "vrcfox model (B&C).fbx"
desired_model_name = "Body"
export_uv_map = "ColorMap" # ColorMap / UVMap (atlas) / UVMap
export_vertex_colors = True # True / False
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

if not export_objects:
    raise ValueError(f"No mesh objects found in collection '{export_collection_name}' to export.")

for obj in export_objects:
    # Apply all modifiers except Armature
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    for m in obj.modifiers:
        if m.type != "ARMATURE":
            bpy.ops.object.modifier_apply(modifier=m.name)
    
    # Set the desired UV map as active on each object
    if export_uv_map in obj.data.uv_layers:
        obj.data.uv_layers.active = obj.data.uv_layers[export_uv_map]
    else:
        print(f"Warning: Object '{obj.name}' is missing UV map '{export_uv_map}'.")
    
    # INDIVIDUAL MESH PROCESSING (per-object)
    # Delete all UV maps except the selected one
    uv_layers = obj.data.uv_layers
    target_uv_layer = uv_layers.get(export_uv_map)
    
    if target_uv_layer:
        uv_layers.active = target_uv_layer
        maps_to_remove = [uv.name for uv in uv_layers if uv.name != export_uv_map]
        for map_name in maps_to_remove:
            uv_layers.remove(uv_layers.get(map_name))
    else:
        available_maps = [uv.name for uv in uv_layers]
        raise ValueError(f"UV map '{export_uv_map}' not found on object '{obj.name}'! "
                         f"Available maps: {available_maps}")
    
    # Simplified normals setup
    me = obj.data
    if hasattr(me, "calc_normals_split"):
        me.calc_normals_split()
    
    # Triangulation
    bm = bmesh.new()
    bm.from_mesh(me)
    bmesh.ops.triangulate(
        bm,
        faces=bm.faces,
        quad_method='FIXED',
        ngon_method='BEAUTY'
    )
    bm.to_mesh(me)
    bm.free()

# Process armature objects
for obj in export_collection.all_objects:
    if obj.type == "ARMATURE" and not in_exclude_collection(obj):
        obj.data.pose_position = 'POSE'

# Make all objects visible
for obj in bpy.data.objects:
    obj.hide_set(False)

# Select objects for export
bpy.ops.object.select_all(action='DESELECT')
for obj in export_objects:
    obj.select_set(True)

if bpy.context.selected_objects:
    # Set 'main' collection as active
    export_layer_collection = bpy.context.view_layer.layer_collection.children[export_collection_name]
    bpy.context.view_layer.active_layer_collection = export_layer_collection

    # Ensure export path exists
    os.makedirs(export_path, exist_ok=True)

    # Export FBX
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(export_path, file_name),
        check_existing=False,
        use_active_collection=True,
        bake_space_transform=False,
        object_types={'ARMATURE', 'MESH'},
        use_mesh_modifiers=False,
        use_mesh_modifiers_render=False,
        bake_anim_use_all_bones=False,
        bake_anim_force_startend_keying=False,
        bake_anim_simplify_factor=0.0,
        colors_type="LINEAR" if export_vertex_colors else "NONE",
        use_armature_deform_only=True,
        use_triangles=False,
        mesh_smooth_type='EDGE',
        use_tspace=True
    )

    # Revert the join operation to keep the .blend file clean
    bpy.ops.ed.undo_push()
    bpy.ops.ed.undo()
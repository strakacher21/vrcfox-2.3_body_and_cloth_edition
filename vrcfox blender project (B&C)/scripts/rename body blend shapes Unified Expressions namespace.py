# rename facetracking shapes to Unified Expressions namespace from JSON
import bpy
import json

# Settings
json_filename = "vrcfox (B&C) Unified Expressions body namespace.json"
unified_namespace = False # False / True
mesh_name = "Body"

# Load mapping from JSON
path = bpy.path.abspath(f"//{json_filename}")
with open(path, encoding="utf-8") as f:
    data = json.load(f)

# Build rename dictionary
if unified_namespace:
    rename_map = {v: k for k, v in data.items() if v is not None} # old→new
else:
    rename_map = {k: v for k, v in data.items() if v is not None} # new→old
    
# Get the mesh object
mesh_obj = bpy.data.objects.get(mesh_name)
if not mesh_obj or mesh_obj.type != 'MESH':
    raise ValueError(f"Mesh object '{mesh_name}' not found or is not a mesh")

# Rename shape keys
if mesh_obj.data.shape_keys:
    for shape_key in mesh_obj.data.shape_keys.key_blocks:
        if shape_key.name in rename_map:
            shape_key.name = rename_map[shape_key.name]
else:
    print(f"No shape keys found on '{mesh_name}'")

print(f"Renamed shape keys on '{mesh_name}' (unified_namespace={unified_namespace})")
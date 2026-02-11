# renames bones in the specified rig based on namespace mapping from JSON
import bpy
import json

# Settings
json_filename = "vrcfox (B&C) ValveBiped bones namespace.json" # vrcfox (B&C) VRM bones namespace.json / vrcfox (B&C) ValveBiped bones namespace.json
JSON_namespace = True  # False / True
avatar_rig = "rig"

# load mapping from JSON
path = bpy.path.abspath(f"//{json_filename}")
with open(path, encoding="utf-8") as f:
    data = json.load(f)

# build rename dict
if JSON_namespace:
    rename_map = {old: new for new, old in data.items()} # old→new
else:
    rename_map = data.copy() # new→old
    

# get the rig object
arm_obj = bpy.data.objects.get(avatar_rig)

# rename bones
bpy.context.view_layer.objects.active = arm_obj
bpy.ops.object.mode_set(mode='EDIT')
for eb in arm_obj.data.edit_bones:
    if eb.name in rename_map:
        eb.name = rename_map[eb.name]
bpy.ops.object.mode_set(mode='OBJECT')

print(f"Renamed bones on '{avatar_rig}' (JSON_namespace={JSON_namespace})")
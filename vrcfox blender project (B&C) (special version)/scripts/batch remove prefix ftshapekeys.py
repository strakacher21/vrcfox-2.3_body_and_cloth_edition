import bpy

ftprefix = True  # False / True

target_name = "Body"
prefix = "v2/"
excluded_prefixes = ("viseme/", "exp/", "pref/", "eye/", "cloth/")

obj = bpy.data.objects.get(target_name)

if obj and obj.type == 'MESH' and obj.data.shape_keys:
    for key in obj.data.shape_keys.key_blocks:
        name = key.name
        if name.startswith(excluded_prefixes):
            continue
        if ftprefix and not name.startswith(prefix):
            key.name = prefix + name
        elif not ftprefix and name.startswith(prefix):
            key.name = name[len(prefix):]
else:
    print("Object 'Body' not found, is not a mesh, or has no shape keys.")
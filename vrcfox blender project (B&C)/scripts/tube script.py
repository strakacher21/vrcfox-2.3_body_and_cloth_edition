import bpy, bmesh
from mathutils import Vector

obj = bpy.context.edit_object
arm = obj.find_armature()
bm = bmesh.from_edit_mesh(obj.data)
dl = bm.verts.layers.deform.verify()

inv = obj.matrix_world.inverted()
vg = {g.index: g.name for g in obj.vertex_groups}

# bone head positions in object space
heads = {
    i: inv @ (arm.matrix_world @ arm.data.bones[name].head_local)
    for i, name in vg.items()
    if name in arm.data.bones
}

for v in bm.verts:
    d = v[dl]
    s = sum(w for i, w in d.items() if i in heads)
    if not s:
        continue

    target = Vector((0, 0, 0))
    for i, w in d.items():
        if i in heads:
            target += heads[i] * w

    v.co = target / s

bmesh.update_edit_mesh(obj.data)
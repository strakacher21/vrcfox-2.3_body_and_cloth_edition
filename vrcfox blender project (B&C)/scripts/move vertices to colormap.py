# Moves UVs of selected faces to a specific position on the active UV layer
import bpy
import bmesh
from mathutils import Vector

# Target UV position to move the selected polygons
target_uv_position = (0.5, 0.5)
#target_uv_position = (0.502, 0.002)
#target_uv_position = (0.002, 0.502)

# Get the active object
obj = bpy.context.object

# Check if the object is a mesh
if not obj or obj.type != 'MESH':
    raise TypeError("Please select an object of type 'MESH'.")

# Switch to Edit Mode to work with BMesh edit data
if bpy.context.mode != 'EDIT_MESH':
    bpy.ops.object.mode_set(mode='EDIT')

# Get the mesh and BMesh (edit mesh)
me = obj.data
bm = bmesh.from_edit_mesh(me)

# Use the active UV layer
uv_layer = bm.loops.layers.uv.active
if uv_layer is None:
    raise RuntimeError("Active UV layer not found on this mesh.")

# Convert target into Vector for assignment
target = Vector(target_uv_position)

# Move UVs of ALL loops of selected faces (prevents mismatched UV corners)
for face in bm.faces:
    if not face.select:
        continue
    for loop in face.loops:
        loop[uv_layer].uv = target

# Update the edit mesh after changes
bmesh.update_edit_mesh(me, loop_triangles=True, destructive=False)
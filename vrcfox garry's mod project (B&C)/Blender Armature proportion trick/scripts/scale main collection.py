import bpy

SCALE_FACTOR = 45.0

if bpy.ops.object.mode_set.poll():
    bpy.ops.object.mode_set(mode="OBJECT")

bpy.ops.object.select_all(action="SELECT")

bpy.ops.transform.resize(
    value=(SCALE_FACTOR, SCALE_FACTOR, SCALE_FACTOR),
    orient_type="GLOBAL",
    constraint_axis=(True, True, True),
    mirror=False,
    use_proportional_edit=False,
)

bpy.context.view_layer.update()
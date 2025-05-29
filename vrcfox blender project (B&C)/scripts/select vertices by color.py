# selects polygons with average vertex color close to the first selected polygon, within a given threshold
import bpy
from mathutils import Color

threshold = .1
obj = bpy.context.object

bpy.ops.object.mode_set(mode="OBJECT")

colors = obj.data.vertex_colors.active.data
selected_polygons = list(filter(lambda p: p.select, obj.data.polygons))

if len(selected_polygons):
    p = selected_polygons[0]
    r = g = b = 0
    for i in p.loop_indices:
        c = colors[i].color
        r += c[0]
        g += c[1]
        b += c[2]
    r /= p.loop_total
    g /= p.loop_total
    b /= p.loop_total
    target = Color((r, g, b))

    for p in obj.data.polygons:
        r = g = b = 0
        for i in p.loop_indices:
            c = colors[i].color
            r += c[0]
            g += c[1]
            b += c[2]
        r /= p.loop_total
        g /= p.loop_total
        b /= p.loop_total
        source = Color((r, g, b))

        print(target, source)

        if (abs(source.r - target.r) < threshold and
            abs(source.g - target.g) < threshold and
            abs(source.b - target.b) < threshold):

            p.select = True

bpy.ops.object.editmode_toggle()
# moves selected vertices to a specific position on the "ColorMap" UV layer
import bpy

# Target UV position to move the selected vertices
target_uv_position = (0.5, 0.5)

# Get the active object
obj = bpy.context.object

# Check if the object is a mesh
if obj and obj.type == 'MESH':
    # Switch to Object Mode to work with data
    bpy.ops.object.mode_set(mode='OBJECT')

    # Get the UV layer named "ColorMap"
    uv_layer = obj.data.uv_layers.get("ColorMap")
    if uv_layer is None:
        print("UV map 'ColorMap' not found.")
    else:
        # Iterate through all polygons
        for poly in obj.data.polygons:
            for loop_index in poly.loop_indices:
                loop_vert_index = obj.data.loops[loop_index].vertex_index
                # Check if the vertex is selected
                if obj.data.vertices[loop_vert_index].select:
                    # Set the new UV coordinate
                    uv_layer.data[loop_index].uv = target_uv_position

        print(f"All selected vertices have been moved to the UV position {target_uv_position} on 'ColorMap'.")

    # Return to Edit Mode
    bpy.ops.object.mode_set(mode='EDIT')
else:
    print("Please select an object of type 'MESH'.")

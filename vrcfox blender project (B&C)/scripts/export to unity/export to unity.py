# Export to Unity script

import bpy
import bmesh
import os
from bpy.app.handlers import persistent

_export_to_unity_owner_filepath = None

@persistent
def _export_to_unity_autounload(_dummy):
    global _export_to_unity_owner_filepath
    current = bpy.data.filepath
    if _export_to_unity_owner_filepath is None:
        _export_to_unity_owner_filepath = current
        return
    if current != _export_to_unity_owner_filepath:
        try:
            bpy.app.handlers.load_post.remove(_export_to_unity_autounload)
        except ValueError:
            pass
        try:
            unregister()
        except Exception:
            pass

# Settings storage (UI)
class EXPORT_TO_UNITY_PG_settings(bpy.types.PropertyGroup):
    export_path: bpy.props.StringProperty(
        name="Export path",
        subtype='DIR_PATH',
        default=r"//../vrcfox unity project (B&C)/Assets"
    )
    file_name: bpy.props.StringProperty(
        name="File name",
        default="vrcfox model (B&C).fbx"
    )
    desired_model_name: bpy.props.StringProperty(
        name="Mesh name",
        default="Body"
    )
    export_uv_map: bpy.props.StringProperty(
        name="UV map",
        default="ColorMap"
    )
    export_vertex_colors: bpy.props.BoolProperty(
        name="Vertex colors",
        default=True
    )
    export_collection_name: bpy.props.StringProperty(
        name="Export collection",
        default="main"
    )
    exclude_collection_name: bpy.props.StringProperty(
        name="Exclude collection",
        default="disabled"
    )


# Export operator

class EXPORT_TO_UNITY_OT_export(bpy.types.Operator):
    bl_idname = "export_to_unity.export"
    bl_label = "Export to Unity"

    def execute(self, context):
        s = context.scene.export_to_unity_settings
        export_path = bpy.path.abspath(s.export_path)
        file_name = s.file_name
        desired_model_name = s.desired_model_name
        export_uv_map = s.export_uv_map
        export_vertex_colors = s.export_vertex_colors
        export_collection_name = s.export_collection_name
        exclude_collection_name = s.exclude_collection_name

        saved = {
            "export_path": s.export_path,
            "file_name": s.file_name,
            "desired_model_name": s.desired_model_name,
            "export_uv_map": s.export_uv_map,
            "export_vertex_colors": s.export_vertex_colors,
            "export_collection_name": s.export_collection_name,
            "exclude_collection_name": s.exclude_collection_name,
        }

        # Get collections
        export_collection = bpy.data.collections.get(export_collection_name)
        exclude_collection = bpy.data.collections.get(exclude_collection_name)

        if not export_collection:
            raise ValueError(f"Collection '{export_collection_name}' not found.")

        # Check if object belongs to an excluded collection
        def in_exclude_collection(obj):
            return exclude_collection and obj.name in [o.name for o in exclude_collection.all_objects]

        # Prepare objects for export (apply modifiers except armature)
        export_objects = [
            obj for obj in export_collection.all_objects
            if obj.type == "MESH" and not in_exclude_collection(obj)
        ]

        for obj in export_objects:
            obj.select_set(True)
            bpy.context.view_layer.objects.active = obj
            for m in obj.modifiers:
                if m.type != "ARMATURE":
                    bpy.ops.object.modifier_apply(modifier=m.name)

        # Process armature objects (force pose position)
        for obj in export_collection.all_objects:
            if obj.type == "ARMATURE" and not in_exclude_collection(obj):
                obj.data.pose_position = 'POSE'

        # Make all objects visible
        for obj in bpy.data.objects:
            obj.hide_set(False)

        # Join objects
        bpy.ops.object.select_all(action='DESELECT')
        for obj in export_objects:
            obj.select_set(True)

        if bpy.context.selected_objects:
            bpy.context.view_layer.objects.active = bpy.context.selected_objects[0]
            bpy.ops.object.join()
            bpy.context.active_object.name = desired_model_name

            # Triangulation
            obj = bpy.context.active_object
            mesh = obj.data
            bm = bmesh.new()
            bm.from_mesh(mesh)
            bmesh.ops.triangulate(
                bm,
                faces=bm.faces,
                quad_method='FIXED',
                ngon_method='BEAUTY'
            )
            bm.to_mesh(mesh)
            bm.free()
            
        # Delete all UV maps except the selected one
        obj = bpy.context.active_object
        uv_layers = obj.data.uv_layers

        target_name = export_uv_map.strip()
        target = uv_layers.get(target_name)

        if target is None:
            raise ValueError(f"UV map '{export_uv_map}' not found. Existing: {[uv.name for uv in uv_layers]}")

        uv_layers.active = target

        names_to_remove = [uv.name for uv in uv_layers if uv.name != target.name]
        for name in names_to_remove:
            uv = uv_layers.get(name)
            if uv is not None:
                uv_layers.remove(uv)

        uv_layers.active = uv_layers.get(target.name)
        
        # Triangulation
        mesh = obj.data
        bm = bmesh.new()
        bm.from_mesh(mesh)
        bmesh.ops.triangulate(
            bm,
            faces=bm.faces,
            quad_method='FIXED',
            ngon_method='BEAUTY'
        )
        bm.to_mesh(mesh)
        bm.free()

        # Set export collection as active layer collection
        export_layer_collection = bpy.context.view_layer.layer_collection.children[export_collection_name]
        bpy.context.view_layer.active_layer_collection = export_layer_collection

        # additional fbx export settings
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
            colors_type="LINEAR" if export_vertex_colors else "NONE",
            use_armature_deform_only=True,
            mesh_smooth_type='EDGE',
            use_triangles=False
        )

        # keep unchanged after export

        bpy.ops.ed.undo_push()
        bpy.ops.ed.undo()

        s = context.scene.export_to_unity_settings
        for k, v in saved.items():
            setattr(s, k, v)

        return {'FINISHED'}

# UI (Header popover)
class VIEW3D_PT_export_to_unity(bpy.types.Panel):
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'HEADER'
    bl_label = "Export to Unity"
    bl_ui_units_x = 20

    @classmethod
    def poll(cls, context):
        return context.workspace and context.workspace.name == "Layout"

    def draw(self, context):
        layout = self.layout
        s = context.scene.export_to_unity_settings

        layout.use_property_split = True
        layout.use_property_decorate = False
        layout.ui_units_x = 20

        col = layout.column(align=True)
        row = col.row()
        row.alignment = 'CENTER'
        row.label(text="Export settings")
        col.separator(type='LINE')

        col.prop(s, "export_path")
        col.prop(s, "file_name")
        col.prop(s, "desired_model_name")
        col.separator()

        col.prop(s, "export_collection_name")
        col.prop(s, "exclude_collection_name")
        col.separator()

        col.prop(s, "export_uv_map")
        col.prop(s, "export_vertex_colors")
        col.separator()

        row = col.row(align=True)
        row.scale_y = 2.0
        row.operator("export_to_unity.export", text="Export to Unity!", icon='EXPORT')


def export_to_unity_header_button(self, context):
    if not (context.workspace and context.workspace.name == "Layout"):
        return
    layout = self.layout
    layout.separator()
    layout.popover(
        panel="VIEW3D_PT_export_to_unity",
        text="Export to Unity",
        icon='EXPORT'
    )


# Register
classes = (
    EXPORT_TO_UNITY_PG_settings,
    EXPORT_TO_UNITY_OT_export,
    VIEW3D_PT_export_to_unity,
)


def register():
    global _export_to_unity_owner_filepath

    # register classes (safe for re-register)
    for cls in classes:
        try:
            bpy.utils.register_class(cls)
        except ValueError:
            pass

    # scene settings pointer
    if not hasattr(bpy.types.Scene, "export_to_unity_settings"):
        bpy.types.Scene.export_to_unity_settings = bpy.props.PointerProperty(type=EXPORT_TO_UNITY_PG_settings)

    # prevent duplicates on Revert:
    # store previous draw func reference on the header type itself
    try:
        old = getattr(bpy.types.VIEW3D_HT_header, "_export_to_unity_draw_func", None)
        if old:
            bpy.types.VIEW3D_HT_header.remove(old)
    except:
        pass

    bpy.types.VIEW3D_HT_header._export_to_unity_draw_func = export_to_unity_header_button

    # Put button on the left/right
    #bpy.types.VIEW3D_HT_header.prepend(bpy.types.VIEW3D_HT_header._export_to_unity_draw_func)
    bpy.types.VIEW3D_HT_header.append(bpy.types.VIEW3D_HT_header._export_to_unity_draw_func)

    _export_to_unity_owner_filepath = bpy.data.filepath
    if _export_to_unity_autounload not in bpy.app.handlers.load_post:
        bpy.app.handlers.load_post.append(_export_to_unity_autounload)


def unregister():
    global _export_to_unity_owner_filepath

    try:
        bpy.app.handlers.load_post.remove(_export_to_unity_autounload)
    except ValueError:
        pass

    try:
        old = getattr(bpy.types.VIEW3D_HT_header, "_export_to_unity_draw_func", None)
        if old:
            bpy.types.VIEW3D_HT_header.remove(old)
    except:
        pass

    try:
        del bpy.types.VIEW3D_HT_header._export_to_unity_draw_func
    except:
        pass

    if hasattr(bpy.types.Scene, "export_to_unity_settings"):
        del bpy.types.Scene.export_to_unity_settings

    for cls in reversed(classes):
        try:
            bpy.utils.unregister_class(cls)
        except:
            pass

    _export_to_unity_owner_filepath = None


if __name__ == "__main__":
    register()
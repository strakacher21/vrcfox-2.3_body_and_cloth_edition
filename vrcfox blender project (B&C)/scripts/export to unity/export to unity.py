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

# Separate meshes UI
class EXPORT_TO_UNITY_PG_separate_mesh_item(bpy.types.PropertyGroup):
    obj: bpy.props.PointerProperty(
        name="Object",
        type=bpy.types.Object,
        poll=lambda self, o: bool(o) and o.type == "MESH"
    )

# Settings storage UI
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
    
    rig_name: bpy.props.StringProperty(
        name="Rig name",
        default="rig"
    )
    
    export_uv_map: bpy.props.StringProperty(
        name="UV map",
        default="ColorMap"
    )
    
    triangulate: bpy.props.BoolProperty(
    name="Triangulate",
    default=False
    )
    
    export_vertex_colors: bpy.props.BoolProperty(
        name="Vertex colors",
        default=True
    )
    
    export_vertex_colors_name: bpy.props.StringProperty(
        name="",
        default="Col"
    )
    
    export_collection_name: bpy.props.StringProperty(
        name="Export collection",
        default="main"
    )

    export_separate_meshes: bpy.props.BoolProperty(
        name="Separate meshes",
        default=False
    )
    
    separate_meshes: bpy.props.CollectionProperty(
        name="Separate meshes list",
        type=EXPORT_TO_UNITY_PG_separate_mesh_item
    )
    
    separate_meshes_index: bpy.props.IntProperty(
        name="",
        default=0
    )

# Separate meshes UI list
class EXPORT_TO_UNITY_UL_separate_meshes(bpy.types.UIList):
    def draw_item(
        self,
        _context,
        layout,
        _data,
        item,
        _icon,
        _active_data,
        _active_propname,
        _index
    ):
        if self.layout_type in {'DEFAULT', 'COMPACT'}:
            if item.obj:
                layout.prop(item, "obj", text="", emboss=False, icon='MESH_DATA')
            else:
                layout.label(text="(Missing object)", icon='ERROR')
        elif self.layout_type == 'GRID':
            layout.alignment = 'CENTER'
            layout.label(text="", icon='MESH_DATA')

class EXPORT_TO_UNITY_OT_separate_mesh_add(bpy.types.Operator):
    bl_idname = "export_to_unity.separate_mesh_add"
    bl_label = "Add separate mesh"
    bl_description = "Add selected mesh objects to the separate meshes list"

    def execute(self, context):
        s = context.scene.export_to_unity_settings
        export_collection = bpy.data.collections.get(s.export_collection_name)

        selected = [o for o in context.selected_objects if o and o.type == "MESH"]
        if not selected and context.active_object and context.active_object.type == "MESH":
            selected = [context.active_object]

        if export_collection:
            selected = [o for o in selected if export_collection.all_objects.get(o.name) is not None]

        if not selected:
            self.report({'WARNING'}, "No mesh objects selected in export collection")
            return {'CANCELLED'}

        existing = {it.obj for it in s.separate_meshes if it.obj}
        added_any = False

        for obj in selected:
            if obj in existing:
                continue

            it = s.separate_meshes.add()
            it.obj = obj
            added_any = True

        if added_any:
            s.separate_meshes_index = max(0, len(s.separate_meshes) - 1)

        return {'FINISHED'}

class EXPORT_TO_UNITY_OT_separate_mesh_remove(bpy.types.Operator):
    bl_idname = "export_to_unity.separate_mesh_remove"
    bl_label = "Remove separate mesh"
    bl_description = "Remove the active item from the separate meshes list"

    def execute(self, context):
        s = context.scene.export_to_unity_settings
        idx = s.separate_meshes_index

        if idx < 0 or idx >= len(s.separate_meshes):
            return {'CANCELLED'}

        s.separate_meshes.remove(idx)
        s.separate_meshes_index = min(idx, max(0, len(s.separate_meshes) - 1))

        return {'FINISHED'}

# Export operator
class EXPORT_TO_UNITY_OT_export(bpy.types.Operator):
    bl_idname = "export_to_unity.export"
    bl_label = "Export to Unity"

    def execute(self, context):
        s = context.scene.export_to_unity_settings

        export_path = bpy.path.abspath(s.export_path)
        file_name = s.file_name
        desired_model_name = s.desired_model_name
        rig_name = s.rig_name
        export_uv_map = s.export_uv_map
        triangulate = s.triangulate
        export_vertex_colors = s.export_vertex_colors
        export_vertex_colors_name = s.export_vertex_colors_name
        export_collection_name = s.export_collection_name
        export_separate_meshes = s.export_separate_meshes
        
        saved = {
            "export_path": s.export_path,
            "file_name": s.file_name,    
            "desired_model_name": s.desired_model_name,
            "rig_name": s.rig_name,
            "export_uv_map": s.export_uv_map,
            "triangulate": s.triangulate,
            "export_vertex_colors": s.export_vertex_colors,
            "export_vertex_colors_name": s.export_vertex_colors_name,
            "export_collection_name": s.export_collection_name,
            "export_separate_meshes": s.export_separate_meshes,
            "separate_meshes_index": s.separate_meshes_index,
            "separate_meshes_names": [it.obj.name for it in s.separate_meshes if it.obj],
        }

        # Get collections
        export_collection = bpy.data.collections.get(export_collection_name)

        if not export_collection:
            raise ValueError(f"Collection '{export_collection_name}' not found.")

        # Prepare objects for export (apply modifiers except armature)
        export_objects = [
            obj for obj in export_collection.all_objects
            if obj.type == "MESH"
        ]

        for obj in export_objects:
            obj.select_set(True)
            bpy.context.view_layer.objects.active = obj
            for m in obj.modifiers:
                if m.type != "ARMATURE":
                    bpy.ops.object.modifier_apply(modifier=m.name)

        # Process armature objects (force pose position)
        for obj in export_collection.all_objects:
            if obj.type == "ARMATURE":
                obj.data.pose_position = 'POSE'

        # Make all objects visible
        for obj in bpy.data.objects:
            obj.hide_set(False)

        # Join objects
        separate_objects = []
        if export_separate_meshes:
            separate_set = set()
            for it in s.separate_meshes:
                obj = it.obj
                if not obj or obj.type != "MESH":
                    continue
                if export_collection.all_objects.get(obj.name) is None:
                    continue
                separate_set.add(obj)

            separate_objects = list(separate_set)

        separate_set = set(separate_objects)
        body_objects = [obj for obj in export_objects if obj not in separate_set]

        if export_separate_meshes and not body_objects:
            raise ValueError("Separate meshes is enabled, but no objects left to join into Body")

        bpy.ops.object.select_all(action='DESELECT')
        for obj in body_objects:
            obj.select_set(True)

        if bpy.context.selected_objects:
            bpy.context.view_layer.objects.active = bpy.context.selected_objects[0]
            bpy.ops.object.join()
            body_obj = bpy.context.active_object
            body_obj.name = desired_model_name
        else:
            body_obj = None

        # Rename rig
        rig_obj = None

        mesh_obj = bpy.context.active_object
        for m in mesh_obj.modifiers:
            if m.type == "ARMATURE" and m.object:
                rig_obj = m.object
                break

        if rig_obj is None:
            rigs = [
                o for o in export_collection.all_objects
                if o.type == "ARMATURE"
            ]
            rig_obj = rigs[0] if rigs else None

        if rig_obj:
            # free object name
            other_obj = bpy.data.objects.get(rig_name)
            if other_obj and other_obj != rig_obj:
                other_obj.name = rig_name + "__old"

            rig_obj.name = rig_name

            # free armature datablock name
            other_arm = bpy.data.armatures.get(rig_name)
            if other_arm and other_arm != rig_obj.data:
                other_arm.name = rig_name + "__old"

            rig_obj.data.name = rig_name
            
        # Delete all UV maps except the selected one
        def process_mesh(obj):
            if not obj or obj.type != "MESH":
                return

            # UV cleanup
            uv_layers = obj.data.uv_layers
            target_name = export_uv_map.strip()
            target = uv_layers.get(target_name)

            if target is None:
                raise ValueError(
                    f"UV map '{export_uv_map}' not found on '{obj.name}'. "
                    f"Existing: {[uv.name for uv in uv_layers]}"
                )

            uv_layers.active = target

            names_to_remove = [uv.name for uv in uv_layers if uv.name != target.name]
            for name in names_to_remove:
                uv = uv_layers.get(name)
                if uv is not None:
                    uv_layers.remove(uv)

            uv_layers.active = uv_layers.get(target.name)

            # Vertex color cleanup
            if export_vertex_colors:
                color_attrs = obj.data.color_attributes
                target_color_name = export_vertex_colors_name.strip()
                target_color = color_attrs.get(target_color_name)

                if target_color is None:
                    raise ValueError(
                        f"Color attribute '{export_vertex_colors_name}' not found on '{obj.name}'. "
                        f"Existing: {[attr.name for attr in color_attrs]}"
                    )

                # Set as active color attribute
                color_attrs.active_color = target_color

                # Remove all other color attributes
                names_to_remove = [attr.name for attr in color_attrs if attr.name != target_color.name]
                for name in names_to_remove:
                    attr = color_attrs.get(name)
                    if attr is not None:
                        color_attrs.remove(attr)

                # Ensure it's still active after removal
                if color_attrs.get(target_color_name):
                    color_attrs.active_color = color_attrs.get(target_color_name)

            # Triangulation
            if triangulate:
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

        process_mesh(body_obj)

        # Separate meshes
        for obj in separate_objects:
            process_mesh(obj)

        # Set export collection as active layer collection
        export_layer_collection = bpy.context.view_layer.layer_collection.children[export_collection_name]
        bpy.context.view_layer.active_layer_collection = export_layer_collection

        # additional fbx export settings
        os.makedirs(export_path, exist_ok=True)

        fbx_props = bpy.ops.export_scene.fbx.get_rna_type().properties
        fbx_kwargs = dict(
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
            use_armature_deform_only=True,
            mesh_smooth_type='EDGE',
            use_triangles=False
        )

        # Vertex colors
        if "export_vertex_color" in fbx_props and "export_vertex_color_name" in fbx_props:
            if export_vertex_colors:
                fbx_kwargs["export_vertex_color"] = "NAME"
                fbx_kwargs["export_vertex_color_name"] = export_vertex_colors_name
            else:
                fbx_kwargs["export_vertex_color"] = "NONE"

        if "colors_type" in fbx_props:
            fbx_kwargs["colors_type"] = "LINEAR" if export_vertex_colors else "NONE"

        bpy.ops.export_scene.fbx(**fbx_kwargs)

        # keep unchanged after export
        bpy.ops.ed.undo_push()
        bpy.ops.ed.undo()

        s = context.scene.export_to_unity_settings

        # restore simple properties
        for k, v in saved.items():
            if k in {'separate_meshes_names', 'separate_meshes_index'}:
                continue
            setattr(s, k, v)

        # restore separate meshes list
        s.separate_meshes.clear()
        for name in saved.get("separate_meshes_names", []):
            obj = bpy.data.objects.get(name)
            if not obj:
                continue
            it = s.separate_meshes.add()
            it.obj = obj

        idx = saved.get("separate_meshes_index", 0)
        s.separate_meshes_index = min(idx, max(0, len(s.separate_meshes) - 1))

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

        col = layout.column(align=True)

        row = col.row()
        row.alignment = 'CENTER'
        row.label(text="Export settings")

        col.separator(type='LINE')

        col.prop(s, "export_path")
        col.prop(s, "file_name")
        col.prop(s, "desired_model_name")
        col.prop(s, "rig_name")
        col.prop(s, "export_collection_name")
        col.prop(s, "export_uv_map")
        col.prop(s, "triangulate")
        row = col.row(align=True)
        row.prop(s, "export_vertex_colors")
        row.prop(s, "export_vertex_colors_name")
        col.prop(s, "export_separate_meshes")

        if s.export_separate_meshes:
            col.separator(type='LINE')

            box = col.box()

            row = box.row()
            row.label(text="Separate from Body")

            row = box.row()
            row.template_list(
                "EXPORT_TO_UNITY_UL_separate_meshes",
                "",
                s,
                "separate_meshes",
                s,
                "separate_meshes_index",
                rows=5
            )

            col_buttons = row.column(align=True)
            col_buttons.operator("export_to_unity.separate_mesh_add", icon='ADD', text="")
            col_buttons.operator("export_to_unity.separate_mesh_remove", icon='REMOVE', text="")

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
    EXPORT_TO_UNITY_PG_separate_mesh_item,
    EXPORT_TO_UNITY_PG_settings,

    EXPORT_TO_UNITY_UL_separate_meshes,
    EXPORT_TO_UNITY_OT_separate_mesh_add,
    EXPORT_TO_UNITY_OT_separate_mesh_remove,

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

    # prevent duplicates on Revert
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
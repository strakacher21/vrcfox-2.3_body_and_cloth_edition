# Blender auto fbx export

import bpy
import bmesh
import os
from bpy.app.handlers import persistent

@persistent
def _auto_fbx_export_autounload(_dummy):
    try:
        bpy.app.handlers.load_pre.remove(_auto_fbx_export_autounload)
    except ValueError:
        pass

    try:
        unregister()
    except Exception:
        pass

# Separate meshes UI
class AUTO_FBX_EXPORT_PG_separate_mesh_item(bpy.types.PropertyGroup):
    obj: bpy.props.PointerProperty(
        name="Object",
        type=bpy.types.Object,
        poll=lambda self, o: bool(o) and o.type == "MESH"
    )

# UV Maps UI
class AUTO_FBX_EXPORT_PG_uv_map_item(bpy.types.PropertyGroup):
    name: bpy.props.StringProperty(
        name="UV Map Name",
        default="UVMap"
    )

# Settings storage UI
class AUTO_FBX_EXPORT_PG_settings(bpy.types.PropertyGroup):
    export_path: bpy.props.StringProperty(
        name="Export path",
        subtype='DIR_PATH',
        default=r"//../Exports"
    )
    
    file_name: bpy.props.StringProperty(
        name="File name",
        default="model.fbx"
    )
    
    desired_model_name: bpy.props.StringProperty(
        name="Mesh name",
        default="Body"
    )

    export_rig:bpy.props.BoolProperty(
        name="Export rig",
        default=False
    )

    rig_name: bpy.props.StringProperty(
        name="",
        default="rig"
    )
    
    export_uv_maps_active: bpy.props.BoolProperty(
        name="Export UV maps",
        default=True
    )

    export_uv_maps: bpy.props.CollectionProperty(
        name="UV Maps List",
        type=AUTO_FBX_EXPORT_PG_uv_map_item
    )
    
    export_uv_maps_index: bpy.props.IntProperty(
        name="",
        default=0
    )

    uv_maps_initialized: bpy.props.BoolProperty(
        name="",
        default=False
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
        type=AUTO_FBX_EXPORT_PG_separate_mesh_item
    )
    
    separate_meshes_index: bpy.props.IntProperty(
        name="",
        default=0
    )

# Separate meshes UI list
class AUTO_FBX_EXPORT_UL_separate_meshes(bpy.types.UIList):
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
                layout.label(text=item.obj.name, icon='MESH_DATA')
            else:
                layout.label(text="(Missing object)", icon='ERROR')
        elif self.layout_type == 'GRID':
            layout.alignment = 'CENTER'
            layout.label(text="", icon='MESH_DATA')

# UV Maps UI list
class AUTO_FBX_EXPORT_UL_uv_maps(bpy.types.UIList):
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
            layout.prop(item, "name", text="", emboss=False, icon='GROUP_UVS')
        elif self.layout_type == 'GRID':
            layout.alignment = 'CENTER'
            layout.label(text="", icon='GROUP_UVS')

class AUTO_FBX_EXPORT_OT_separate_mesh_add(bpy.types.Operator):
    bl_idname = "auto_fbx_export.separate_mesh_add"
    bl_label = "Add separate mesh"
    bl_description = "Add selected mesh objects to the separate meshes list"

    def execute(self, context):
        s = context.scene.auto_fbx_export_settings
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

class AUTO_FBX_EXPORT_OT_separate_mesh_remove(bpy.types.Operator):
    bl_idname = "auto_fbx_export.separate_mesh_remove"
    bl_label = "Remove separate mesh"
    bl_description = "Remove the active item from the separate meshes list"

    def execute(self, context):
        s = context.scene.auto_fbx_export_settings
        idx = s.separate_meshes_index

        if idx < 0 or idx >= len(s.separate_meshes):
            return {'CANCELLED'}

        s.separate_meshes.remove(idx)
        s.separate_meshes_index = min(idx, max(0, len(s.separate_meshes) - 1))

        return {'FINISHED'}

class AUTO_FBX_EXPORT_OT_uv_map_add(bpy.types.Operator):
    bl_idname = "auto_fbx_export.uv_map_add"
    bl_label = "Add UV map"
    bl_description = "Add a UV map name to the export list"

    def execute(self, context):
        s = context.scene.auto_fbx_export_settings

        # Try to get active UV map name from active object
        default_name = "UVMap"
        if context.active_object and context.active_object.type == 'MESH':
            if context.active_object.data.uv_layers.active:
                default_name = context.active_object.data.uv_layers.active.name

        it = s.export_uv_maps.add()
        it.name = default_name
        s.export_uv_maps_index = len(s.export_uv_maps) - 1

        return {'FINISHED'}

class AUTO_FBX_EXPORT_OT_uv_map_remove(bpy.types.Operator):
    bl_idname = "auto_fbx_export.uv_map_remove"
    bl_label = "Remove UV map"
    bl_description = "Remove the active item from the UV maps list"

    def execute(self, context):
        s = context.scene.auto_fbx_export_settings
        idx = s.export_uv_maps_index

        if idx < 0 or idx >= len(s.export_uv_maps):
            return {'CANCELLED'}

        s.export_uv_maps.remove(idx)
        s.export_uv_maps_index = min(idx, max(0, len(s.export_uv_maps) - 1))

        return {'FINISHED'}

# Export operator
class AUTO_FBX_EXPORT_OT_export(bpy.types.Operator):
    bl_idname = "auto_fbx_export.export"
    bl_label = "FBX Export"

    def execute(self, context):
        s = context.scene.auto_fbx_export_settings

        export_path = bpy.path.abspath(s.export_path)
        file_name = s.file_name
        desired_model_name = s.desired_model_name
        export_rig = s.export_rig
        rig_name = s.rig_name
        
        export_uv_maps_active = s.export_uv_maps_active
        export_uv_map_names = [it.name.strip() for it in s.export_uv_maps if it.name.strip()]
        
        triangulate = s.triangulate
        export_vertex_colors = s.export_vertex_colors
        export_vertex_colors_name = s.export_vertex_colors_name
        export_collection_name = s.export_collection_name
        export_separate_meshes = s.export_separate_meshes
        
        saved = {
            "export_path": s.export_path,
            "file_name": s.file_name,    
            "desired_model_name": s.desired_model_name,
            "export_rig": s.export_rig,
            "rig_name": s.rig_name,
            "export_uv_maps_active": s.export_uv_maps_active,
            "triangulate": s.triangulate,
            "export_vertex_colors": s.export_vertex_colors,
            "export_vertex_colors_name": s.export_vertex_colors_name,
            "export_collection_name": s.export_collection_name,
            "export_separate_meshes": s.export_separate_meshes,
            "separate_meshes_index": s.separate_meshes_index,
            "separate_meshes_names": [it.obj.name for it in s.separate_meshes if it.obj],
            "uv_maps_names": export_uv_map_names,
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
            if export_rig and obj.type == "ARMATURE":
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

            if not export_rig:
                break

            if m.type == "ARMATURE" and m.object:
                rig_obj = m.object
                break

        if export_rig and rig_obj is None:
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
            
        # Delete all UV maps except the selected ones
        def process_mesh(obj):
            if not obj or obj.type != "MESH":
                return

            # UV cleanup
            uv_layers = obj.data.uv_layers

            if not export_uv_maps_active or not export_uv_map_names:
                #Remove all UV maps if disabled or list is empty
                while uv_layers:
                    uv_layers.remove(uv_layers[0])
            else:
                targets = []
                for name in export_uv_map_names:
                    target = uv_layers.get(name)
                    if target:
                        targets.append(target)

                if not targets:
                    print(
                        f"Warning: none of the specified UV maps found on '{obj.name}'. "
                        f"Existing: {[uv.name for uv in uv_layers]}"
                    )
                else:
                    uv_layers.active = targets[0]

                    allowed_names = {t.name for t in targets}
                    names_to_remove = [uv.name for uv in uv_layers if uv.name not in allowed_names]

                    for name in names_to_remove:
                        uv = uv_layers.get(name)
                        if uv is not None:
                            uv_layers.remove(uv)

                    if uv_layers.get(targets[0].name):
                        uv_layers.active = uv_layers.get(targets[0].name)

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
            object_types={'ARMATURE', 'MESH'}
            if export_rig else {'MESH'},
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

        s = context.scene.auto_fbx_export_settings

        # restore simple properties
        for k, v in saved.items():
            if k in {'separate_meshes_names', 'separate_meshes_index', 'uv_maps_names'}:
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
class VIEW3D_PT_auto_fbx_export(bpy.types.Panel):
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'HEADER'
    bl_label = "FBX Export"
    bl_ui_units_x = 20

    @classmethod
    def poll(cls, context):
        return context.workspace and context.workspace.name == "Layout"

    def draw(self, context):
        layout = self.layout
        s = context.scene.auto_fbx_export_settings

        col = layout.column(align=True)

        row = col.row()
        row.alignment = 'CENTER'
        row.label(text="Export settings")

        col.separator(type='LINE')

        col.prop(s, "export_collection_name")
        col.prop(s, "file_name")
        col.prop(s, "desired_model_name")

        row = col.row(align=True)
        row.prop(s, "export_rig")
        row.prop(s, "rig_name")

        row = col.row(align=True)
        row.prop(s, "export_vertex_colors")
        row.prop(s, "export_vertex_colors_name")

        col.prop(s, "triangulate")

        col.prop(s, "export_uv_maps_active")
        
        if s.export_uv_maps_active:
            row = col.row()
            row.template_list(
                "AUTO_FBX_EXPORT_UL_uv_maps",
                "",
                s,
                "export_uv_maps",
                s,
                "export_uv_maps_index",
                rows=3
            )
            col_buttons = row.column(align=True)
            col_buttons.operator("auto_fbx_export.uv_map_add", icon='ADD', text="")
            col_buttons.operator("auto_fbx_export.uv_map_remove", icon='REMOVE', text="")
            col.separator()

        col.prop(s, "export_separate_meshes")
        if s.export_separate_meshes:
            col.separator(type='LINE')
            row = col.row()
            row.template_list(
                "AUTO_FBX_EXPORT_UL_separate_meshes",
                "",
                s,
                "separate_meshes",
                s,
                "separate_meshes_index",
                rows=5
            )
            col_buttons = row.column(align=True)
            col_buttons.operator("auto_fbx_export.separate_mesh_add", icon='ADD', text="")
            col_buttons.operator("auto_fbx_export.separate_mesh_remove", icon='REMOVE', text="")
            col.separator()

        col.prop(s, "export_path")

        col.separator()
        row = col.row(align=True)
        row.scale_y = 2.0
        row.operator("auto_fbx_export.export", text="Export FBX!", icon='EXPORT')

def auto_fbx_export_header_button(self, context):
    if not (context.workspace and context.workspace.name == "Layout"):
        return

    layout = self.layout
    layout.separator()

    layout.popover(
        panel="VIEW3D_PT_auto_fbx_export",
        text="FBX Export",
        icon='EXPORT'
    )

# Register
classes = (
    AUTO_FBX_EXPORT_PG_separate_mesh_item,
    AUTO_FBX_EXPORT_PG_uv_map_item,
    AUTO_FBX_EXPORT_PG_settings,

    AUTO_FBX_EXPORT_UL_separate_meshes,
    AUTO_FBX_EXPORT_UL_uv_maps,
    AUTO_FBX_EXPORT_OT_separate_mesh_add,
    AUTO_FBX_EXPORT_OT_separate_mesh_remove,
    AUTO_FBX_EXPORT_OT_uv_map_add,
    AUTO_FBX_EXPORT_OT_uv_map_remove,

    AUTO_FBX_EXPORT_OT_export,
    VIEW3D_PT_auto_fbx_export,
)

def register():

    # register classes (safe for re-register)
    for cls in classes:
        try:
            bpy.utils.register_class(cls)
        except ValueError:
            pass

    # scene settings pointer
    if not hasattr(bpy.types.Scene, "auto_fbx_export_settings"):
        bpy.types.Scene.auto_fbx_export_settings = bpy.props.PointerProperty(type=AUTO_FBX_EXPORT_PG_settings)
    
    # a dump way to set the default name of a collection of UVMaps
    for scene in bpy.data.scenes:
        s = scene.auto_fbx_export_settings
        if not s.uv_maps_initialized:
            if not s.export_uv_maps:
                s.export_uv_maps.add().name = "UVMap"
            s.uv_maps_initialized = True

    # prevent duplicates on Revert
    try:
        old = getattr(bpy.types.VIEW3D_HT_header, "_auto_fbx_export_draw_func", None)
        if old:
            bpy.types.VIEW3D_HT_header.remove(old)
    except:
        pass

    bpy.types.VIEW3D_HT_header._auto_fbx_export_draw_func = auto_fbx_export_header_button

    # Put button on the right
    bpy.types.VIEW3D_HT_header.append(bpy.types.VIEW3D_HT_header._auto_fbx_export_draw_func)

    if _auto_fbx_export_autounload not in bpy.app.handlers.load_pre:
        bpy.app.handlers.load_pre.append(_auto_fbx_export_autounload)

def unregister():

    try:
        bpy.app.handlers.load_pre.remove(_auto_fbx_export_autounload)
    except ValueError:
        pass

    try:
        old = getattr(bpy.types.VIEW3D_HT_header, "_auto_fbx_export_draw_func", None)
        if old:
            bpy.types.VIEW3D_HT_header.remove(old)
    except:
        pass

    try:
        del bpy.types.VIEW3D_HT_header._auto_fbx_export_draw_func
    except:
        pass

    if hasattr(bpy.types.Scene, "auto_fbx_export_settings"):
        del bpy.types.Scene.auto_fbx_export_settings

    for cls in reversed(classes):
        try:
            bpy.utils.unregister_class(cls)
        except:
            pass


if __name__ == "__main__":
    register()
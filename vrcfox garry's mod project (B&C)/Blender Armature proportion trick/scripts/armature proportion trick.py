# Based (merged and misc) on "Proportion Trick" scripts by Β L Λ Ζ Ξ

import bpy

objects = bpy.context.scene.objects
data_objects = bpy.data.objects

# ValveBiped bones used for constraints / filtering
VALVEBIPED_BONES = [
    "ValveBiped.Bip01_Pelvis",
    "ValveBiped.Bip01_Spine",
    "ValveBiped.Bip01_Spine1",
    "ValveBiped.Bip01_Spine2",
    "ValveBiped.Bip01_Spine4",
    "ValveBiped.Bip01_Neck1",
    "ValveBiped.Bip01_Head1",
    "ValveBiped.Bip01_R_Clavicle",
    "ValveBiped.Bip01_R_UpperArm",
    "ValveBiped.Bip01_R_Forearm",
    "ValveBiped.Bip01_R_Hand",
    "ValveBiped.Bip01_R_Finger0",
    "ValveBiped.Bip01_R_Finger01",
    "ValveBiped.Bip01_R_Finger02",
    "ValveBiped.Bip01_R_Finger1",
    "ValveBiped.Bip01_R_Finger11",
    "ValveBiped.Bip01_R_Finger12",
    "ValveBiped.Bip01_R_Finger2",
    "ValveBiped.Bip01_R_Finger21",
    "ValveBiped.Bip01_R_Finger22",
    "ValveBiped.Bip01_R_Finger3",
    "ValveBiped.Bip01_R_Finger31",
    "ValveBiped.Bip01_R_Finger32",
    "ValveBiped.Bip01_R_Finger4",
    "ValveBiped.Bip01_R_Finger41",
    "ValveBiped.Bip01_R_Finger42",
    "ValveBiped.Bip01_L_Clavicle",
    "ValveBiped.Bip01_L_UpperArm",
    "ValveBiped.Bip01_L_Forearm",
    "ValveBiped.Bip01_L_Hand",
    "ValveBiped.Bip01_L_Finger0",
    "ValveBiped.Bip01_L_Finger01",
    "ValveBiped.Bip01_L_Finger02",
    "ValveBiped.Bip01_L_Finger1",
    "ValveBiped.Bip01_L_Finger11",
    "ValveBiped.Bip01_L_Finger12",
    "ValveBiped.Bip01_L_Finger2",
    "ValveBiped.Bip01_L_Finger21",
    "ValveBiped.Bip01_L_Finger22",
    "ValveBiped.Bip01_L_Finger3",
    "ValveBiped.Bip01_L_Finger31",
    "ValveBiped.Bip01_L_Finger32",
    "ValveBiped.Bip01_L_Finger4",
    "ValveBiped.Bip01_L_Finger41",
    "ValveBiped.Bip01_L_Finger42",
    "ValveBiped.Bip01_R_Thigh",
    "ValveBiped.Bip01_R_Calf",
    "ValveBiped.Bip01_R_Foot",
    "ValveBiped.Bip01_R_Toe0",
    "ValveBiped.Bip01_L_Thigh",
    "ValveBiped.Bip01_L_Calf",
    "ValveBiped.Bip01_L_Foot",
    "ValveBiped.Bip01_L_Toe0",
]

# Pairs: (bone to constrain, target bone for Locked Track)
VALVEBIPED_LOCK_TRACK_PAIRS = [
    ("ValveBiped.Bip01_L_Thigh",   "ValveBiped.Bip01_L_Calf"),
    ("ValveBiped.Bip01_L_Calf",    "ValveBiped.Bip01_L_Foot"),
    ("ValveBiped.Bip01_R_Thigh",   "ValveBiped.Bip01_R_Calf"),
    ("ValveBiped.Bip01_R_Calf",    "ValveBiped.Bip01_R_Foot"),
    ("ValveBiped.Bip01_L_UpperArm","ValveBiped.Bip01_L_Forearm"),
    ("ValveBiped.Bip01_L_Forearm", "ValveBiped.Bip01_L_Hand"),
    ("ValveBiped.Bip01_R_UpperArm","ValveBiped.Bip01_R_Forearm"),
    ("ValveBiped.Bip01_R_Forearm", "ValveBiped.Bip01_R_Hand"),
    ("ValveBiped.Bip01_L_Finger0", "ValveBiped.Bip01_L_Finger01"),
    ("ValveBiped.Bip01_L_Finger01","ValveBiped.Bip01_L_Finger02"),
    ("ValveBiped.Bip01_L_Finger1", "ValveBiped.Bip01_L_Finger11"),
    ("ValveBiped.Bip01_L_Finger11","ValveBiped.Bip01_L_Finger12"),
    ("ValveBiped.Bip01_L_Finger2", "ValveBiped.Bip01_L_Finger21"),
    ("ValveBiped.Bip01_L_Finger21","ValveBiped.Bip01_L_Finger22"),
    ("ValveBiped.Bip01_L_Finger3", "ValveBiped.Bip01_L_Finger31"),
    ("ValveBiped.Bip01_L_Finger31","ValveBiped.Bip01_L_Finger32"),
    ("ValveBiped.Bip01_L_Finger4", "ValveBiped.Bip01_L_Finger41"),
    ("ValveBiped.Bip01_L_Finger41","ValveBiped.Bip01_L_Finger42"),
    ("ValveBiped.Bip01_R_Finger0", "ValveBiped.Bip01_R_Finger01"),
    ("ValveBiped.Bip01_R_Finger01","ValveBiped.Bip01_R_Finger02"),
    ("ValveBiped.Bip01_R_Finger1", "ValveBiped.Bip01_R_Finger11"),
    ("ValveBiped.Bip01_R_Finger11","ValveBiped.Bip01_R_Finger12"),
    ("ValveBiped.Bip01_R_Finger2", "ValveBiped.Bip01_R_Finger21"),
    ("ValveBiped.Bip01_R_Finger21","ValveBiped.Bip01_R_Finger22"),
    ("ValveBiped.Bip01_R_Finger3", "ValveBiped.Bip01_R_Finger31"),
    ("ValveBiped.Bip01_R_Finger31","ValveBiped.Bip01_R_Finger32"),
    ("ValveBiped.Bip01_R_Finger4", "ValveBiped.Bip01_R_Finger41"),
    ("ValveBiped.Bip01_R_Finger41","ValveBiped.Bip01_R_Finger42"),
]

# Imported skeleton (Source skeleton)
armature = bpy.context.view_layer.objects.active
if armature is None or armature.type != "ARMATURE":
    raise Exception('Select imported skeleton Armature (gg) before running this script.')

# Proportion rig must be named "proportions"
try:
    proportions = data_objects["proportions"]
except KeyError:
    raise Exception('Armature "proportions" not found.')

if proportions.type != "ARMATURE":
    raise Exception('"proportions" must be an ARMATURE, not ' + proportions.type)

# FIRST PART: set constraints on proportion rig
for name in VALVEBIPED_BONES:
    src_bone = armature.pose.bones.get(name)
    if not src_bone:
        continue
    constraints = proportions.pose.bones[name].constraints
    c = constraints.new("COPY_LOCATION")
    c.target = armature
    c.subtarget = name

for target_name, look_name in VALVEBIPED_LOCK_TRACK_PAIRS:
    if armature.pose.bones.get(target_name) is None:
        continue
    constraints = proportions.pose.bones[target_name].constraints

    c1 = constraints.new("LOCKED_TRACK")
    c1.target = armature
    c1.subtarget = look_name
    c1.track_axis = "TRACK_X"
    c1.lock_axis = "LOCK_Z"

    c2 = constraints.new("LOCKED_TRACK")
    c2.target = armature
    c2.subtarget = look_name
    c2.track_axis = "TRACK_X"
    c2.lock_axis = "LOCK_Y"

# Remove extra LOCKED_TRACK on parents if children have no constraints
for name in VALVEBIPED_BONES:
    bone = proportions.pose.bones[name]
    for child in bone.children:
        child_constraints = proportions.pose.bones[child.name].constraints
        if not child_constraints.keys() and bone.parent is not None:
            parent_constraints = bone.constraints
            for c in list(parent_constraints):
                if c.name != "Copy Location":
                    parent_constraints.remove(c)
            print(
                f"{name} is a parent of {child.name} with constraint: "
                f"{bone.constraints.keys()}"
            )

# Show proportions, hide source skeleton, go Pose Mode
proportions.hide_set(False)
armature.hide_set(True)
bpy.context.view_layer.objects.active = proportions
proportions.select_set(True)
bpy.ops.object.mode_set(mode="POSE")

# Copy Copy Location constraints from Spine1 / Spine2 to Spine4
pb = proportions.pose.bones
dst = pb.get("ValveBiped.Bip01_Spine4")
if dst:
    for src_name in ("ValveBiped.Bip01_Spine1", "ValveBiped.Bip01_Spine2"):
        src = pb.get(src_name)
        if not src:
            continue
        for c in src.constraints:
            if c.type != "COPY_LOCATION":
                continue
            new_c = dst.constraints.new(type="COPY_LOCATION")
            new_c.name = f"{c.name}_from_{src_name}"
            new_c.target = c.target
            new_c.subtarget = c.subtarget
            new_c.owner_space = c.owner_space
            new_c.target_space = c.target_space
            new_c.influence = c.influence
            new_c.use_x = c.use_x
            new_c.use_y = c.use_y
            new_c.use_z = c.use_z
            new_c.use_offset = c.use_offset

# Apply pose as rest pose and clear pose constraints
bpy.ops.pose.select_all(action="SELECT")
bpy.ops.pose.armature_apply(selected=True)
bpy.ops.pose.constraints_clear()
bpy.ops.object.mode_set(mode="OBJECT")

# SECOND PART: merge non‑ValveBiped bones into proportion rig
arm = armature
arm2 = proportions

proportions.hide_set(True)
armature.hide_set(False)
bpy.context.view_layer.objects.active = armature
armature.select_set(True)
bpy.ops.object.mode_set(mode="OBJECT")
bpy.ops.object.duplicate()
bpy.ops.object.mode_set(mode="EDIT")
bpy.ops.armature.select_all(action="DESELECT")

for bone in bpy.context.object.data.edit_bones:
    if bone.name in VALVEBIPED_BONES:
        bone.select = True
        bone.select_head = True
        bone.select_tail = True

bpy.ops.armature.delete()
bpy.ops.object.mode_set(mode="OBJECT")

proportions.hide_set(False)
armature.hide_set(True)
bpy.context.view_layer.objects.active = proportions
bpy.ops.object.mode_set(mode="OBJECT")
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.join()
bpy.ops.object.mode_set(mode="EDIT")

# Collect non‑ValveBiped bones and their parents
non_vb_child_names = []
non_vb_parent_names = []

for bone in arm.data.bones:
    if bone.name not in VALVEBIPED_BONES:
        non_vb_child_names.append(bone.name)
        non_vb_parent_names.append(
            getattr(bone.parent, "name", "ValveBiped.Bip01_Pelvis")
        )

# Reparent merged non‑ValveBiped bones inside proportion rig
for child_name, parent_name in zip(non_vb_child_names, non_vb_parent_names):
    arm2.data.edit_bones[child_name].parent = arm2.data.edit_bones[parent_name]

bpy.ops.object.mode_set(mode="OBJECT")

# Ensure all meshes use the proportion rig as Armature modifier target
for ob in objects:
    if ob.type != "MESH":
        continue

    arm_mod = None
    for mod in ob.modifiers:
        if mod.type == "ARMATURE":
            arm_mod = mod
            break

    if arm_mod is None:
        arm_mod = ob.modifiers.new("Armature", "ARMATURE")

    arm_mod.object = proportions
# Proportion Trick script by Β L Λ Ζ Ξ
# Blender version: 2.8+
# Aligns proportion to imported skeleton.
# Merge non-ValveBiped bones from imported skeleton to proportion.
# Video (2.79): https://www.youtube.com/watch?v=MSWirIyobb4
# Video (2.8+): https://www.youtube.com/watch?v=n9lmxpjSv0I
# This merged version uses the active armature as imported "gg" skeleton
# and adds optional scaling and extra spine constraints.

import bpy
from collections import OrderedDict

# Scale multiplier for selected skeleton (set to 1.0 to disable scaling)
SCALE_FACTOR = 35.0

obj = bpy.data.objects

# active armature = imported skeleton (originally called "gg")
armature = bpy.context.view_layer.objects.active
if armature is None or armature.type != 'ARMATURE':
    raise Exception('Select imported skeleton Armature (gg) before running this script.')

# proportion rig must still be named "proportions"
try:
    proportions = obj['proportions']
except KeyError:
    raise Exception('Armature "proportions" not found.')

if proportions.type != 'ARMATURE':
    raise Exception('"proportions" must be an ARMATURE, not ' + proportions.type)

objects = bpy.context.scene.objects

valvebipeds = [
    'ValveBiped.Bip01_Pelvis',
    'ValveBiped.Bip01_Spine',
    'ValveBiped.Bip01_Spine1',
    'ValveBiped.Bip01_Spine2',
    'ValveBiped.Bip01_Spine4',
    'ValveBiped.Bip01_Neck1',
    'ValveBiped.Bip01_Head1',
    'ValveBiped.Bip01_R_Clavicle',
    'ValveBiped.Bip01_R_UpperArm',
    'ValveBiped.Bip01_R_Forearm',
    'ValveBiped.Bip01_R_Hand',
    'ValveBiped.Bip01_R_Finger0',
    'ValveBiped.Bip01_R_Finger01',
    'ValveBiped.Bip01_R_Finger02',
    'ValveBiped.Bip01_R_Finger1',
    'ValveBiped.Bip01_R_Finger11',
    'ValveBiped.Bip01_R_Finger12',
    'ValveBiped.Bip01_R_Finger2',
    'ValveBiped.Bip01_R_Finger21',
    'ValveBiped.Bip01_R_Finger22',
    'ValveBiped.Bip01_R_Finger3',
    'ValveBiped.Bip01_R_Finger31',
    'ValveBiped.Bip01_R_Finger32',
    'ValveBiped.Bip01_R_Finger4',
    'ValveBiped.Bip01_R_Finger41',
    'ValveBiped.Bip01_R_Finger42',
    'ValveBiped.Bip01_L_Clavicle',
    'ValveBiped.Bip01_L_UpperArm',
    'ValveBiped.Bip01_L_Forearm',
    'ValveBiped.Bip01_L_Hand',
    'ValveBiped.Bip01_L_Finger0',
    'ValveBiped.Bip01_L_Finger01',
    'ValveBiped.Bip01_L_Finger02',
    'ValveBiped.Bip01_L_Finger1',
    'ValveBiped.Bip01_L_Finger11',
    'ValveBiped.Bip01_L_Finger12',
    'ValveBiped.Bip01_L_Finger2',
    'ValveBiped.Bip01_L_Finger21',
    'ValveBiped.Bip01_L_Finger22',
    'ValveBiped.Bip01_L_Finger3',
    'ValveBiped.Bip01_L_Finger31',
    'ValveBiped.Bip01_L_Finger32',
    'ValveBiped.Bip01_L_Finger4',
    'ValveBiped.Bip01_L_Finger41',
    'ValveBiped.Bip01_L_Finger42',
    'ValveBiped.Bip01_R_Thigh',
    'ValveBiped.Bip01_R_Calf',
    'ValveBiped.Bip01_R_Foot',
    'ValveBiped.Bip01_R_Toe0',
    'ValveBiped.Bip01_L_Thigh',
    'ValveBiped.Bip01_L_Calf',
    'ValveBiped.Bip01_L_Foot',
    'ValveBiped.Bip01_L_Toe0',
]

valvebipeds2 = [
    'ValveBiped.Bip01_L_Thigh',
    'ValveBiped.Bip01_L_Calf',
    'ValveBiped.Bip01_L_Calf',
    'ValveBiped.Bip01_L_Foot',
    'ValveBiped.Bip01_R_Thigh',
    'ValveBiped.Bip01_R_Calf',
    'ValveBiped.Bip01_R_Calf',
    'ValveBiped.Bip01_R_Foot',
    'ValveBiped.Bip01_L_UpperArm',
    'ValveBiped.Bip01_L_Forearm',
    'ValveBiped.Bip01_L_Forearm',
    'ValveBiped.Bip01_L_Hand',
    'ValveBiped.Bip01_R_UpperArm',
    'ValveBiped.Bip01_R_Forearm',
    'ValveBiped.Bip01_R_Forearm',
    'ValveBiped.Bip01_R_Hand',
    'ValveBiped.Bip01_L_Finger0',
    'ValveBiped.Bip01_L_Finger01',
    'ValveBiped.Bip01_L_Finger01',
    'ValveBiped.Bip01_L_Finger02',
    'ValveBiped.Bip01_L_Finger1',
    'ValveBiped.Bip01_L_Finger11',
    'ValveBiped.Bip01_L_Finger11',
    'ValveBiped.Bip01_L_Finger12',
    'ValveBiped.Bip01_L_Finger2',
    'ValveBiped.Bip01_L_Finger21',
    'ValveBiped.Bip01_L_Finger21',
    'ValveBiped.Bip01_L_Finger22',
    'ValveBiped.Bip01_L_Finger3',
    'ValveBiped.Bip01_L_Finger31',
    'ValveBiped.Bip01_L_Finger31',
    'ValveBiped.Bip01_L_Finger32',
    'ValveBiped.Bip01_L_Finger4',
    'ValveBiped.Bip01_L_Finger41',
    'ValveBiped.Bip01_L_Finger41',
    'ValveBiped.Bip01_L_Finger42',
    'ValveBiped.Bip01_R_Finger0',
    'ValveBiped.Bip01_R_Finger01',
    'ValveBiped.Bip01_R_Finger01',
    'ValveBiped.Bip01_R_Finger02',
    'ValveBiped.Bip01_R_Finger1',
    'ValveBiped.Bip01_R_Finger11',
    'ValveBiped.Bip01_R_Finger11',
    'ValveBiped.Bip01_R_Finger12',
    'ValveBiped.Bip01_R_Finger2',
    'ValveBiped.Bip01_R_Finger21',
    'ValveBiped.Bip01_R_Finger21',
    'ValveBiped.Bip01_R_Finger22',
    'ValveBiped.Bip01_R_Finger3',
    'ValveBiped.Bip01_R_Finger31',
    'ValveBiped.Bip01_R_Finger31',
    'ValveBiped.Bip01_R_Finger32',
    'ValveBiped.Bip01_R_Finger4',
    'ValveBiped.Bip01_R_Finger41',
    'ValveBiped.Bip01_R_Finger41',
    'ValveBiped.Bip01_R_Finger42',
]

target = valvebipeds2[::2]
sub = valvebipeds2[1::2]
d = OrderedDict()

for idx, value in enumerate(sub):
    key = 'var' + str(idx)
    d[key] = value

# FIRST SCRIPT: set constraints on "proportions"
for i in valvebipeds:
    objbone = proportions.pose.bones[i].constraints
    if armature.pose.bones.get(i) is not None:
        objbone.new('COPY_LOCATION')
        objbone['Copy Location'].target = armature
        objbone['Copy Location'].subtarget = i

for j, k in enumerate(target):
    objbone2 = proportions.pose.bones[k].constraints
    if armature.pose.bones.get(k) is not None:
        objbone2.new('LOCKED_TRACK')
        objbone2['Locked Track'].target = armature
        objbone2['Locked Track'].subtarget = d["var" + str(j)]
        objbone2['Locked Track'].track_axis = 'TRACK_X'
        objbone2['Locked Track'].lock_axis = 'LOCK_Z'
        objbone2.new('LOCKED_TRACK')
        objbone2['Locked Track.001'].target = armature
        objbone2['Locked Track.001'].subtarget = d["var" + str(j)]
        objbone2['Locked Track.001'].track_axis = 'TRACK_X'
        objbone2['Locked Track.001'].lock_axis = 'LOCK_Y'

# remove LOCKED_TRACK on parent if all children have no constraints
for l in valvebipeds:
    objbone3 = proportions.pose.bones[l]
    for child in objbone3.children:
        objbone4 = proportions.pose.bones[child.name].constraints
        if not objbone4.keys():
            if objbone3.parent is not None:
                objbone5 = proportions.pose.bones[l].constraints
                for constraint in objbone5:
                    if constraint.name != 'Copy Location':
                        objbone5.remove(constraint)
                print(l + ' is a parent of ' + child.name +
                      ' with constraint: ', objbone3.constraints.keys())

# show proportions, hide source skeleton, go Pose Mode
proportions.hide_set(False)
armature.hide_set(True)
bpy.context.view_layer.objects.active = proportions
proportions.select_set(True)
bpy.ops.object.mode_set(mode='POSE')

# EXTRA ACTIONS AFTER FIRST SCRIPT

# 1) scale selected skeleton
if SCALE_FACTOR != 1.0:
    armature.scale = tuple(c * SCALE_FACTOR for c in armature.scale)

# 2) copy Copy Location constraints from Spine1 and Spine2 to Spine4
pb = proportions.pose.bones

src_names = (
    'ValveBiped.Bip01_Spine1',
    'ValveBiped.Bip01_Spine2',
)
dst_name = 'ValveBiped.Bip01_Spine4'
dst = pb[dst_name]

for src_name in src_names:
    src = pb.get(src_name)
    if src is None:
        print(f'Source {src_name} not found, skipping')
        continue

    for c in src.constraints:
        if c.type != 'COPY_LOCATION':
            continue

        new_c = dst.constraints.new(type='COPY_LOCATION')
        new_c.name = f'{c.name}_from_{src_name}'

        new_c.target = c.target
        new_c.subtarget = c.subtarget
        new_c.owner_space = c.owner_space
        new_c.target_space = c.target_space
        new_c.influence = c.influence
        new_c.use_x = c.use_x
        new_c.use_y = c.use_y
        new_c.use_z = c.use_z
        new_c.use_offset = c.use_offset

        print(f'Copied Copy Location from {src_name} to {dst_name}')

# 3) Apply pose as rest pose and clear pose constraints
bpy.ops.pose.select_all(action='SELECT')
bpy.ops.pose.armature_apply(selected=True)
bpy.ops.pose.constraints_clear()

bpy.ops.object.mode_set(mode='OBJECT')

# SECOND SCRIPT: merge non‑ValveBiped bones into "proportions"

arm = armature
arm2 = proportions

bn = []
pr = []

proportions.hide_set(True)
armature.hide_set(False)
bpy.context.view_layer.objects.active = armature
armature.select_set(True)
bpy.ops.object.mode_set(mode='OBJECT')
bpy.ops.object.duplicate()
bpy.ops.object.mode_set(mode='EDIT')
bpy.ops.armature.select_all(action='DESELECT')

for bone in bpy.context.object.data.edit_bones:
    if bone.name in valvebipeds:
        bone.select = True
        bone.select_head = True
        bone.select_tail = True

bpy.ops.armature.delete()
bpy.ops.object.mode_set(mode='OBJECT')
proportions.hide_set(False)
armature.hide_set(True)
bpy.context.view_layer.objects.active = proportions
bpy.ops.object.mode_set(mode='OBJECT')
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.join()
bpy.ops.object.mode_set(mode='EDIT')

for bone in arm.data.bones:
    if bone.name not in valvebipeds:
        bn.append(bone.name)

for bone in arm.data.bones:
    if bone.name not in valvebipeds:
        pr.append(getattr(bone.parent, 'name', 'ValveBiped.Bip01_Pelvis'))

for bone in bpy.context.object.data.edit_bones:
    j = 0
    i = 0
    while j < len(bn) and i < len(pr):
        arm2.data.edit_bones[bn[i]].parent = arm2.data.edit_bones[pr[j]]
        j += 1
        i += 1

bpy.ops.object.mode_set(mode='OBJECT')

# add / update Armature modifier
for ob in objects:
    if ob.type == 'MESH':
        has_arm = False
        for mods in ob.modifiers:
            if mods.type == 'ARMATURE':
                has_arm = True
                mods.object = proportions
        if not has_arm:
            mod = ob.modifiers.new('Armature', 'ARMATURE')
            mod.object = proportions

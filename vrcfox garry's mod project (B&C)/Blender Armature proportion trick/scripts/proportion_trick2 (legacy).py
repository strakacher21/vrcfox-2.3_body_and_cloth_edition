#From https://github.com/sksh70/proportion_trick_script
#
#Proportion Trick script by Β L Λ Ζ Ξ
#Blender version: 2.8+
#Merge non-ValveBiped bones from imported skeleton to proportion.
#Video (2.79): https://www.youtube.com/watch?v=MSWirIyobb4
#Video (2.8+): https://www.youtube.com/watch?v=n9lmxpjSv0I
 
import bpy

arm = bpy.data.objects['gg']
arm2 = bpy.data.objects['proportions']
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

bn = []
pr = []

bpy.data.objects['proportions'].hide_set(True)
bpy.data.objects['gg'].hide_set(False)
bpy.context.view_layer.objects.active = bpy.data.objects['gg']
bpy.data.objects['gg'].select_set(True)
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
bpy.data.objects['proportions'].hide_set(False)
bpy.data.objects['gg'].hide_set(True)
bpy.context.view_layer.objects.active = bpy.data.objects['proportions']
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

#add armature modifier
for ob in objects:
    if ob.type == 'MESH':
        if ob.modifiers.values() == []:
            ob.modifiers.new('Armature', 'ARMATURE')
            ob.modifiers['Armature'].object = bpy.data.objects['proportions']
        else:
            for mods in ob.modifiers.values():
                if mods.name == 'Armature':
                    ob.modifiers['Armature'].object = bpy.data.objects['proportions']
                else:
                    ob.modifiers.new('Armature', 'ARMATURE')
                    ob.modifiers['Armature'].object = bpy.data.objects['proportions']
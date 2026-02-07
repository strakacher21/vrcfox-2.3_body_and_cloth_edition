#From https://github.com/sksh70/proportion_trick_script
#
#Proportion Trick script by Β L Λ Ζ Ξ
#Blender version: 2.8+
#Aligns proportion to imported skeleton.
#Video (2.79): https://www.youtube.com/watch?v=MSWirIyobb4
#Video (2.8+): https://www.youtube.com/watch?v=n9lmxpjSv0I

import bpy
from collections import OrderedDict

obj = bpy.data.objects

try:
    armature = obj['gg']
except:
    raise Exception('gg skeleton not found. You must rename imported skeleton to gg. Watch the video again carefully.')

if armature.type != 'ARMATURE': raise Exception('gg must be an ARMATURE not '+armature.type)

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
    
for i in valvebipeds:
    objbone = bpy.data.objects['proportions'].pose.bones[i].constraints
    
    if armature.pose.bones.get(i) is not None:
        objbone.new('COPY_LOCATION')
        objbone['Copy Location'].target = armature
        objbone['Copy Location'].subtarget = i

for j, k in enumerate(target):
    objbone2 = bpy.data.objects['proportions'].pose.bones[k].constraints
    
    if armature.pose.bones.get(k) is not None:
        objbone2.new('LOCKED_TRACK')
        objbone2['Locked Track'].target = armature
        objbone2['Locked Track'].subtarget = d["var"+str(j)]
        objbone2['Locked Track'].track_axis = 'TRACK_X'
        objbone2['Locked Track'].lock_axis = 'LOCK_Z'
        objbone2.new('LOCKED_TRACK')
        objbone2['Locked Track.001'].target = armature
        objbone2['Locked Track.001'].subtarget = d["var"+str(j)]
        objbone2['Locked Track.001'].track_axis = 'TRACK_X'
        objbone2['Locked Track.001'].lock_axis = 'LOCK_Y'

#removes LOCKED_TRACK on parent if child constraint is empty			
for l in valvebipeds:
    objbone3 = bpy.data.objects['proportions'].pose.bones[l]
        
    for child in objbone3.children:    
        objbone4 = bpy.data.objects['proportions'].pose.bones[child.name].constraints
        
        if not objbone4.keys():
            if objbone3.parent is not None:
                objbone5 = bpy.data.objects['proportions'].pose.bones[l].constraints
                
                for constraint in objbone5:
                    if constraint.name != 'Copy Location':
                        objbone5.remove(constraint)
    
        print(l+' is a parent of '+child.name+' with constraint: ',objbone3.constraints.keys())
        
bpy.data.objects['proportions'].hide_set(False)
bpy.data.objects['gg'].hide_set(True)
bpy.context.view_layer.objects.active = bpy.data.objects['proportions']
bpy.data.objects['proportions'].select_set(True)
bpy.ops.object.mode_set(mode='POSE')
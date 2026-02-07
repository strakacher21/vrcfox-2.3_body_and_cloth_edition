import bpy

BONE_MAPPING = {
    "hip": "ValveBiped.Bip01_Pelvis",
    "belly": "ValveBiped.Bip01_Spine",
    "chest": "ValveBiped.Bip01_Spine4",
    "neck": "ValveBiped.Bip01_Neck1",
    "head": "ValveBiped.Bip01_Head1",
    "shoulder.L": "ValveBiped.Bip01_L_Clavicle",
    "upper_arm.L": "ValveBiped.Bip01_L_UpperArm",
    "forearm.L": "ValveBiped.Bip01_L_Forearm",
    "hand.L": "ValveBiped.Bip01_L_Hand",
    "thumb.01.L": "ValveBiped.Bip01_L_Finger0",
    "thumb.02.L": "ValveBiped.Bip01_L_Finger01",
    "thumb.03.L": "ValveBiped.Bip01_L_Finger02",
    "f_index.01.L": "ValveBiped.Bip01_L_Finger1",
    "f_index.02.L": "ValveBiped.Bip01_L_Finger11",
    "f_index.03.L": "ValveBiped.Bip01_L_Finger12",
    "f_middle.01.L": "ValveBiped.Bip01_L_Finger2",
    "f_middle.02.L": "ValveBiped.Bip01_L_Finger21",
    "f_middle.03.L": "ValveBiped.Bip01_L_Finger22",
    "f_ring.01.L": "ValveBiped.Bip01_L_Finger3",
    "f_ring.02.L": "ValveBiped.Bip01_L_Finger31",
    "f_ring.03.L": "ValveBiped.Bip01_L_Finger32",
    "f_pinky.01.L": "ValveBiped.Bip01_L_Finger4",
    "f_pinky.02.L": "ValveBiped.Bip01_L_Finger41",
    "f_pinky.03.L": "ValveBiped.Bip01_L_Finger42",
    "shoulder.R": "ValveBiped.Bip01_R_Clavicle",
    "upper_arm.R": "ValveBiped.Bip01_R_UpperArm",
    "forearm.R": "ValveBiped.Bip01_R_Forearm",
    "hand.R": "ValveBiped.Bip01_R_Hand",
    "thumb.01.R": "ValveBiped.Bip01_R_Finger0",
    "thumb.02.R": "ValveBiped.Bip01_R_Finger01",
    "thumb.03.R": "ValveBiped.Bip01_R_Finger02",
    "f_index.01.R": "ValveBiped.Bip01_R_Finger1",
    "f_index.02.R": "ValveBiped.Bip01_R_Finger11",
    "f_index.03.R": "ValveBiped.Bip01_R_Finger12",
    "f_middle.01.R": "ValveBiped.Bip01_R_Finger2",
    "f_middle.02.R": "ValveBiped.Bip01_R_Finger21",
    "f_middle.03.R": "ValveBiped.Bip01_R_Finger22",
    "f_ring.01.R": "ValveBiped.Bip01_R_Finger3",
    "f_ring.02.R": "ValveBiped.Bip01_R_Finger31",
    "f_ring.03.R": "ValveBiped.Bip01_R_Finger32",
    "f_pinky.01.R": "ValveBiped.Bip01_R_Finger4",
    "f_pinky.02.R": "ValveBiped.Bip01_R_Finger41",
    "f_pinky.03.R": "ValveBiped.Bip01_R_Finger42",
    "thigh.L": "ValveBiped.Bip01_L_Thigh",
    "shin.L": "ValveBiped.Bip01_L_Calf",
    "foot.L": "ValveBiped.Bip01_L_Foot",
    "toe.L": "ValveBiped.Bip01_L_Toe0",
    "thigh.R": "ValveBiped.Bip01_R_Thigh",
    "shin.R": "ValveBiped.Bip01_R_Calf",
    "foot.R": "ValveBiped.Bip01_R_Foot",
    "toe.R": "ValveBiped.Bip01_R_Toe0"
}


def find_armature_in_collection(collection):
    for obj in collection.objects:
        if obj.type == 'ARMATURE':
            return obj
    
    for child_collection in collection.children:
        armature = find_armature_in_collection(child_collection)
        if armature:
            return armature
    
    return None


def main():
    main_collection = bpy.data.collections.get("main")
    
    armature = find_armature_in_collection(main_collection)
    
    if not armature:
        print("Error: No armature found in 'main' collection")
        return
    
    bpy.context.view_layer.objects.active = armature
    bpy.ops.object.mode_set(mode='EDIT')
    
    renamed_count = 0
    for edit_bone in armature.data.edit_bones:
        if edit_bone.name in BONE_MAPPING:
            new_name = BONE_MAPPING[edit_bone.name]
            print(f"Renaming: {edit_bone.name} -> {new_name}")
            edit_bone.name = new_name
            renamed_count += 1
    
    bpy.ops.object.mode_set(mode='OBJECT')

if __name__ == "__main__":
    main()
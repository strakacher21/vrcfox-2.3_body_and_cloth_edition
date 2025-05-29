# disables IK and Damped Track constraints
import bpy

armature_name = "rig"
disable_bones_targets = False

# toggle IK and Damped Track constraints
def toggle_bones_constraints(toggle: bool, armature_object: bpy.types.Object):
    if armature_object.type != 'ARMATURE':
        print("The selected object is not an armature.")
        return
    
    for bone in armature_object.pose.bones:
        for constraint in bone.constraints:
            
            # Disable IK constraints
            if constraint.type == 'IK':
                constraint.mute = not toggle
                print(f"IK for bone {bone.name} {'enabled' if toggle else 'disabled'}")
                
            # Disable Damped Track constraints
            elif constraint.type == 'DAMPED_TRACK':
                constraint.mute = not toggle
                print(f"Damped Track for bone {bone.name} {'enabled' if toggle else 'disabled'}")

# get armature by name and toggle constraints
armature_object = bpy.data.objects.get(armature_name)
if armature_object:
    toggle_bones_constraints(disable_bones_targets, armature_object)
else:
    print(f"Armature '{armature_name}' not found.")
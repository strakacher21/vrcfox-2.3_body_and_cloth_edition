$modelname "vrcfox (B&C)/vrcfox (B&C).mdl"
$model "vrcfox (B&C)" "vrcfox (B&C).dmx"

$cdmaterials "models/vrcfox (B&C)/"

$attachment "eyes" "ValveBiped.Bip01_Head1" 2.63 -4.13 0.04 rotate 0 -80.1 -90
$attachment "mouth" "ValveBiped.Bip01_Head1" -0.2 -5.8 0 rotate 0 -80 -90
$attachment "chest" "ValveBiped.Bip01_Spine2" 4 4 0 rotate 0 95 90
$attachment "forward" "ValveBiped.forward" 0 0 0 rotate 0 0 0
$attachment "anim_attachment_RH" "ValveBiped.Anim_Attachment_RH" 0 0 0 rotate -90 -90 0
$attachment "anim_attachment_LH" "ValveBiped.Anim_Attachment_LH" 0 0 0 rotate -90 -90 0

//Visit https://developer.valvesoftware.com/wiki/Main_Page if you want to know the function of all commands in this file

$ikchain "rhand" "ValveBiped.Bip01_R_Hand" knee 0.707 0.707 0
$ikchain "lhand" "ValveBiped.Bip01_L_Hand" knee 0.707 0.707 0
$ikchain "rfoot" "ValveBiped.Bip01_R_Foot" knee 0.707 -0.707 0
$ikchain "lfoot" "ValveBiped.Bip01_L_Foot" knee 0.707 -0.707 0

//The first value, 0.5, dictates how high the foot is off the ground. Generally values between 0-1 are acceptable enough, but you can go higher or go lower into the negatives if need be.
//The second value, 0.1 can be left untouched (or even at 0). This controls the rotation of the foot and isn't really necessary.
//Copied from CaptainBigButt's post: https://web.archive.org/web/20160608040725/https://facepunch.com/showthread.php?t=1439159
$ikautoplaylock "rfoot" 0.5 0.1
$ikautoplaylock "lfoot" 0.5 0.1

//Rename reference_male to reference_female if you're using female pm/npc animation
$sequence reference "anims/reference_male" fps 1

$animation a_proportions "anims/proportions" subtract reference 0

$sequence proportions a_proportions predelta autoplay

$Sequence "ragdoll" {
	"anims/proportions"
	activity "ACT_DIERAGDOLL" 1
	fadein 0.2
	fadeout 0.2
	fps 30
}

$includemodel "m_anm.mdl"


$collisionjoints "vrcfox (B&C) physics.dmx"
{
	$mass 60
	$inertia 10
	$damping 0.01
	$rotdamping 1.5
	$concaveperjoint
	$rootbone "ValveBiped.Bip01_Pelvis"


	$jointconstrain "ValveBiped.Bip01_R_Thigh" x limit -42 42 0
	$jointconstrain "ValveBiped.Bip01_R_Thigh" y limit -38 115 0
	$jointconstrain "ValveBiped.Bip01_R_Thigh" z limit -32 86 0

	$jointconstrain "ValveBiped.Bip01_R_Calf" x limit 0 0 0
	$jointconstrain "ValveBiped.Bip01_R_Calf" y limit -125 0 0
	$jointconstrain "ValveBiped.Bip01_R_Calf" z limit 1 1 0

	$jointconstrain "ValveBiped.Bip01_L_Thigh" x limit -42 42 0
	$jointconstrain "ValveBiped.Bip01_L_Thigh" y limit -44 97 0
	$jointconstrain "ValveBiped.Bip01_L_Thigh" z limit -91 26 0

	$jointconstrain "ValveBiped.Bip01_L_Calf" x limit -21 28 0
	$jointconstrain "ValveBiped.Bip01_L_Calf" y limit -113 11 0
	$jointconstrain "ValveBiped.Bip01_L_Calf" z limit 0 0 0.2

	$jointconstrain "ValveBiped.Bip01_L_Foot" x limit -47 47 0
	$jointconstrain "ValveBiped.Bip01_L_Foot" y limit -54 54 0
	$jointconstrain "ValveBiped.Bip01_L_Foot" z limit -64 64 0

	$jointconstrain "ValveBiped.Bip01_Spine1" x limit -58 58 0
	$jointconstrain "ValveBiped.Bip01_Spine1" y limit -35 35 0
	$jointconstrain "ValveBiped.Bip01_Spine1" z limit -35 50 0

	$jointconstrain "ValveBiped.Bip01_Spine2" x limit -58 58 0
	$jointconstrain "ValveBiped.Bip01_Spine2" y limit -35 35 0
	$jointconstrain "ValveBiped.Bip01_Spine2" z limit -35 50 0

    $jointconstrain "ValveBiped.Bip01_Spine4" x limit -58 58 0
	$jointconstrain "ValveBiped.Bip01_Spine4" y limit -35 35 0
	$jointconstrain "ValveBiped.Bip01_Spine4" z limit -35 50 0

	$jointconstrain "ValveBiped.Bip01_R_UpperArm" x limit -39 39 0
	$jointconstrain "ValveBiped.Bip01_R_UpperArm" y limit -84 84 0
	$jointconstrain "ValveBiped.Bip01_R_UpperArm" z limit -95 95 0

	$jointconstrain "ValveBiped.Bip01_L_UpperArm" x limit -30 30 0
	$jointconstrain "ValveBiped.Bip01_L_UpperArm" y limit -58 100 0
	$jointconstrain "ValveBiped.Bip01_L_UpperArm" z limit -82 93 0

	$jointconstrain "ValveBiped.Bip01_L_Forearm" x limit -37 37 0
	$jointconstrain "ValveBiped.Bip01_L_Forearm" y limit -140 3 0
	$jointconstrain "ValveBiped.Bip01_L_Forearm" z limit 1 1 0

	$jointconstrain "ValveBiped.Bip01_L_Hand" x limit -97 97 0
	$jointconstrain "ValveBiped.Bip01_L_Hand" y limit -30 30 0
	$jointconstrain "ValveBiped.Bip01_L_Hand" z limit -77 99 0

	$jointconstrain "ValveBiped.Bip01_R_Forearm" x limit -39 39 0
	$jointconstrain "ValveBiped.Bip01_R_Forearm" y limit -3 140 0
	$jointconstrain "ValveBiped.Bip01_R_Forearm" z limit 1 1 0

	$jointconstrain "ValveBiped.Bip01_R_Hand" x limit -90 90 0
	$jointconstrain "ValveBiped.Bip01_R_Hand" y limit -30 30 0
	$jointconstrain "ValveBiped.Bip01_R_Hand" z limit -77 90 0

	$jointconstrain "ValveBiped.Bip01_Head1" x limit -64 64 0
	$jointconstrain "ValveBiped.Bip01_Head1" y limit -45 45 0
	$jointconstrain "ValveBiped.Bip01_Head1" z limit -88 88 0

	$jointconstrain "ValveBiped.Bip01_R_Foot" x limit -64 64 0
	$jointconstrain "ValveBiped.Bip01_R_Foot" y limit -32 32 0
	$jointconstrain "ValveBiped.Bip01_R_Foot" z limit -57 57 0
}

// Tail
$jigglebone "tail.2"
{
    is_flexible
    {
        length           50
        tip_mass         0
        pitch_stiffness  75
        pitch_damping    7
        yaw_stiffness    75
        yaw_damping      7
        along_stiffness  100
        along_damping    7
        yaw_constraint   -15 60.000002
        yaw_friction     0
        yaw_bounce       0
        angle_constraint 30
    }
}

// Ear Left
$jigglebone "ear.2.L"
{
    is_flexible
    {
        length           50
        tip_mass         0
        pitch_stiffness  50
        pitch_damping    7
        yaw_stiffness    50
        yaw_damping      7
        along_stiffness  100
        along_damping    0
        yaw_constraint   -15 60.000002
        yaw_friction     0
        yaw_bounce       0
        angle_constraint 60.000002
    }
}

// Ear Right
$jigglebone "ear.2.R"
{
    is_flexible
    {
        length           50
        tip_mass         0
        pitch_stiffness  50
        pitch_damping    7
        yaw_stiffness    50
        yaw_damping      7
        along_stiffness  100
        along_damping    0
        yaw_constraint   -15 60.000002
        yaw_friction     0
        yaw_bounce       0
        angle_constraint 60.000002
    }
}

// Hair
$jigglebone "hair"
{
    is_flexible
    {
        length          10
        tip_mass        0
        pitch_stiffness 75
        pitch_damping   7
        yaw_stiffness   75
        yaw_damping     7
        along_stiffness 100
        along_damping   0
        angle_constraint 19.999999
    }
}

#version 3.7; // 3.6
global_settings { assumed_gamma 1.0 }
#default { finish { emission 0.2 diffuse 0.65 } }
#default { pigment { rgb <25, 25, 25>/255 } }

//------------------------------------------
#include "colors.inc"
#include "textures.inc"
#include "math.inc"


#include "rad_def.inc"
global_settings {
	radiosity {
		Rad_Settings(Radiosity_Normal, on, off)
	}
}

//------------------------------------------
#include "LogoGear-final_textures.inc"
#include "LogoGear-final_meshes.inc"

//------------------------------------------
// Camera ----------------------------------
/*
#declare CamUp = < 0, 0, 180>;
#declare CamRight = <180, 0, 0>;
#declare CamRotation = <-102, -19, -11>;
#declare CamPosition = <-42, 41, 157>;
*/
#declare CamUp = < 0, 0, 181>;
#declare CamRight = <181, 0, 0>;
#declare CamRotation = <-102, -19, -11>;
#declare CamPosition = <-42, 41, 157>;
camera {
	orthographic
	location <0, 0, 0>
	direction <0, 1, 0>
	up CamUp
	right CamRight
	rotate CamRotation
	translate CamPosition
}

// FreeCAD Light -------------------------------------
light_source { CamPosition color rgb 0.6 }

// Background ------------------------------
/*
polygon {
	5, <-83.04664611816406, -83.04664611816406>, <-83.04664611816406, 83.04664611816406>, <83.04664611816406, 83.04664611816406>, <83.04664611816406, -83.04664611816406>, <-83.04664611816406, -83.04664611816406>
	pigment {
		gradient y
		color_map {
			[ 0.00  color rgb<0.592, 0.592, 0.667> ]
			[ 0.05  color rgb<0.592, 0.592, 0.667> ]
			[ 0.95  color rgb<0.200, 0.200, 0.396> ]
			[ 1.00  color rgb<0.200, 0.200, 0.396> ]
		}
		scale <1,166.09329223632812,1>
		translate <0,-83.04664611816406,0>
	}
	finish { ambient 1 diffuse 0 }
	rotate <54.73560946600158, 1.9538003484950872e-05, 45.00000263027788>
	translate <94.22171020507812, -88.22097778320312, 113.7808609008789>
	translate <-57735.04376411438, 57735.008001327515, -57735.02588272095>
}*/
/*
sky_sphere {
	pigment {
		rgbt <119, 198, 255, 1>/255.0
	}
	emission rgb <119, 198, 255>/255.0
}
*/
object {
  difference {
    plane { -vnormalize(CamPosition), 900
      hollow
    }
    sphere { <0,0,0>, 1000
      hollow
    }
  }
  texture { 
    pigment {
  		gradient y
  		color_map {
  			[ 0.00  color rgb<0.592, 0.592, 0.867> ]
  			[ 1.00  color rgb<0.200, 0.200, 0.396> ]
  		}
  		scale <1,900,1>
  	}
  	finish { emission 0.5 diffuse 0 }
  }
  hollow
}

//------------------------------------------

// Objects in Scene ------------------------

//----- GearBody -----
object { GearBody_mesh
  material {GearBody_material }
}

//----- Marker -----
object { Marker_mesh
}

//----- Num1 -----
object { Num1_mesh
}

object { Num2_mesh
}

object { Num3_mesh
}

object { Num4_mesh
}

object { Num5_mesh
}

object { Num6_mesh
}

object { Num7_mesh
}

object { Num0_mesh
}

object { MarkerInt_mesh
  material {GearBody_material }
}

//----- ImagePlane -----
polygon { 5, <0, 0>, <101.505, 0>, <101.505, 101.507>, <0, 101.507>, <0, 0>
	pigment {
		image_map {
			png ".\PropellerColorDisk.png"
			map_type 0 once
		}
		scale <101.505, 101.507, 1>
	}
    translate <-50.7525, -50.7535, 0>
    rotate <0,0,45>
    translate <0.0, 0.0, 30.01>
}

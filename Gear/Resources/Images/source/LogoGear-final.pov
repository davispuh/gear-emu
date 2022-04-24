/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * LogoGear-final.pov
 * --------------------------------------------------------------------------------
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 * --------------------------------------------------------------------------------
 */

#version 3.7;

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
//Hollow sphere with a cut on bottom of gear, to generate transparent background AND global illumination
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
         png "./PropellerColorDisk.png"
         map_type 0 once
      }
      scale <101.505, 101.507, 1>
   }
   translate <-50.7525, -50.7535, 0>
   rotate <0,0,45>
   translate <0.0, 0.0, 30.01>
}

# SourceFormatParser
 [C#] Parser for Source Engine files
 
### Supported formats:
| Name | Ext. | Description |
| ------------ | ------------ | ------------ |
| [BSP](#bsp) | **.bsp* | VBSP. Map file |
| [MDL](#mdl) | **.mdl* | 3d-model; prop file |
| [VHV](#vhv) | *sp_\*.vhv* | Valve Hardware Vertexes (Per-vertex lighting for static props) |

## BSP
*Supported versions:* 17-21

*Tested on versions:* 19-21

*Supported lumps:* [BSP_LUMPS.md](https://github.com/antimYT/SourceFormatParser/blob/master/BSP_LUMPS.md)

## MDL
work in progress

## VHV
*VHV* - Valve Hardware Vertexes (Per-vertex lighting for static props).

You can find them in LUMP_PAKFILE of [BSP](#bsp) with the name sp_*.vhv 

*Supported version:* 2

**IMPORTANT NOTE:** VERTEX_COLOR is not supported (used in bsp when g_numVradStaticPropsLightingStreams <= 1; looks like very rarely? This variable equals 3 in CSGO by default)

## To do
MDL, VTX, VVD, VTF, etc.

*Lumps:*
| #Num | Name | Description | Supported |
| ------------ | ------------ | ------------ | ------------ |
| 0 | LUMP_ENTITIES | Entities | ✅Yes |
| 1 | LUMP_PLANES | [not sure] Calculations for movement (if player can collide with walls?) | ✅Yes |
| 2 | LUMP_TEXDATA | texture referencing, texture preloading, lighting calculation (reflectivity) | ✅Yes |
| 3 | LUMP_VERTEXES | brush/displacement rendering | ✅Yes |
| 4 | LUMP_VISIBILITY | visibility optimizing for brushes/displacements/props/entities  | ✅Yes |
| 5 | LUMP_NODES | [not sure] to determine player's position for visibility optimisation? | ✅Yes |
| 6 | LUMP_TEXINFO | texture info. Flags, texture offset, lightmap [offset?], index for texdata | ✅Yes |
| 7 | LUMP_FACES | datas for rendering | ✅Yes |
| 8 | LUMP_LIGHTING | lightmap colors / shadows | ✅Yes |
| 9 | LUMP_OCCLUSION | [not sure] func_occluder entity info for compilation | ✅Yes |
| 10 | LUMP_LEAFS | [not sure] to determine player's position for visibility optimisation?  | ✅Yes |
| 11 | LUMP_FACEIDS | Face IDs from Valve Hammer Editor. Should equal Faces lump count | ✅Yes |
| 12 | LUMP_EDGES | vertex indices for faces | ✅Yes |
| 13 | LUMP_SURFEDGES | Faces.firstedge/numedges (e) -> Surfedge[e] (s) -> Edges[s] | ✅Yes |
| 14 | LUMP_MODELS | Brushes for rendering (mostly). [0] is global, [1 and more] are entities (func_brush/func_detail) | ✅Yes |
| 15 | LUMP_WORLDLIGHTS | Contains info about static lights (NOT LIGHTMAPS) | ✅Yes |
| 16 | LUMP_LEAFFACES | "... which are used to map from faces referenced in the leaf structure to indices in the face array", from Valve Dev Wiki | ✅Yes |
| 17 | LUMP_LEAFBRUSHES | does the same thing [as lump 16 - leaf faces] for brushes referenced in leaves | ✅Yes |
| 18 | LUMP_BRUSHES | Brush info | ✅Yes |
| 19 | LUMP_BRUSHSIDES | Brush side info | ✅Yes |
| 20 | LUMP_AREAS | Areas that are used for switching of areaportals; visibility things | ✅Yes |
| 21 | LUMP_AREAPORTALS | used with func_areaportal and func_areaportalwindow entities to define sections of the map that can be switched to render or not render | ✅Yes |
| 22/1 | LUMP_PORTALS (Source 2004) | [not sure] Used by VVIS and VRAD only | ✅Yes |
| 22/2 | LUMP_PROPCOLLISION (Source 2009) | Static props convex hull lists | ✅Yes |
| 23/1 | LUMP_CLUSTERS (Source 2004) | [not sure] Used by VVIS and VRAD only | ✅Yes |
| 23/2 | LUMP_PROPHULLS (Source 2009) | Static prop convex hulls | ✅Yes |
| 24/1 | LUMP_PORTALVERTS (Source 2004) | [not sure] Used by VVIS and VRAD only | ☑️Yes, but not tested: can't find map with this lump |
| 24/2 | LUMP_PROPHULLVERTS (Source 2009) | Static prop collision vertices | ☑️Yes, but not tested: can't find map with this lump |
| 25/1 | LUMP_CLUSTERPORTALS (Source 2004) | [not sure] Used by VVIS and VRAD only | ☑️Yes, but not tested: can't find map with this lump |
| 25/2 | LUMP_PROPTRIS (Source 2009) | Static prop per hull triangle index start/count | ☑️Yes, but not tested: can't find map with this lump |
| 26 | LUMP_DISPINFO | [not sure] Displacement infos that is used by map compilators | ☑Almost yes. Skipped some of properties. |
| 27 | LUMP_ORIGINALFACES | "Brush faces array before splitting", VDW. Probably, used by map compilators | ✅Yes |
| 28 | LUMP_PHYSDISP | Displacement physics collision data | ☑Not full: SourcePHY parser needed (i guess) |
| 29 | LUMP_PHYSCOLLIDE | Physics collision data. Used to calculate VPhysics (for prop_physics, i guess) | ☑Not full: SourcePHY parser needed (i guess) |
| 30 | LUMP_VERTNORMALS | Face plane normals. "may be related to smoothing of lightmaps on faces", from Valve Dev Wiki | ✅Yes |
| 31 | LUMP_VERTNORMALINDICES | Face plane normal index array. "may be related to smoothing of lightmaps on faces", from Valve Dev Wiki | ✅Yes |
| 32 | LUMP_DISP_LIGHTMAP_ALPHAS | Displacement lightmap alphas (unused/empty since Source 2006) | ✅Yes |
| 33 | LUMP_DISP_VERTS | Info about vertices for displacements | ✅Yes |
| 34 | LUMP_DISP_LIGHTMAP_SAMPLE_POSITIONS | supposed to be ColorRGBExp32, but its byte array in source code. i dunno that's literally boxes on white background (as i can remember). it's useless because lightmaps for disps are in LUMP_LIGHTING. | ✅Yes |
| 35 | LUMP_GAME_LUMP | Game-specific data lump. Contains info about static props and detail prop | ✅Yes |
| 36 | LUMP_LEAFWATERDATA | Data for leaf nodes that are inside water | ✅Yes |
| 37 | LUMP_PRIMITIVES | are used in reference to "non-polygonal primitives" | ✅Yes |
| 38 | LUMP_PRIMVERTS | are used in reference to "non-polygonal primitives" | ☑Yes, but not tested: can't find bsp with this lump |
| 39 | LUMP_PRIMINDICES | are used in reference to "non-polygonal primitives" | ✅Yes |
| 40 | LUMP_PAKFILE | Contains game files for map | ✅Yes, as byte array |
| 41 | LUMP_CLIPPORTALVERTS | [not sure] Used by VVIS and VRAD only | ✅Yes |
| 42 | LUMP_CUBEMAPS | env_cubemap entities info | ✅Yes |
| 43 | LUMP_TEXDATA_STRING_DATA | Paths to VMTs of textures | ✅Yes, as string array (TexdataString) |
| 44 | LUMP_TEXDATA_STRING_TABLE | Paths to VMTs of textures | ✅Yes, as string array (TexdataString) |
| 45 | LUMP_OVERLAYS | infodecal entities - overlays | ✅Yes |
| 46 | LUMP_LEAFMINDISTTOWATER | Distance from leaf to water | ✅Yes |
| 47 | LUMP_FACE_MACRO_TEXTURE_INFO | Macro texture info for faces | ✅Yes |
| 48 | LUMP_DISP_TRIS | Displacement triangle tags | ✅Yes |
| 49/1 | LUMP_PHYSCOLLIDESURFACE (Source 2004) | Compressed physics collision data | 🟥No. Can't find structs for this lump |
| 49/2 | LUMP_PROP_BLOB (Source 2009) | static prop triangle & string data | 🟥No. Can't find structs for this lump |
| 50 | LUMP_WATEROVERLAYS | [unknown] | ✅Yes, but not tested: can't find map with this lump |
| 51/1 | LUMP_LIGHTMAPPAGES (Source 2006 for XBOX) | Lightmaps for XBOX | ✅Yes, but not tested: can't find map with this lump |
| 51/2 | LUMP_LEAF_AMBIENT_INDEX_HDR (Source 2007) | index of LUMP_LEAF_AMBIENT_LIGHTING_HDR | ✅Yes |
| 52/1 | LUMP_LIGHTMAPPAGEINFOS (Source 2006 for XBOX) | Lightmaps for XBOX | ✅Yes |
| 52/2 | LUMP_LEAF_AMBIENT_INDEX (Source 2007) | index of LUMP_LEAF_AMBIENT_LIGHTING | ✅Yes |
| 53 | LUMP_LIGHTING_HDR | lightmap colors / shadows | ✅Yes |
| 54 | LUMP_WORLDLIGHTS_HDR | Contains info about static lights (NOT LIGHTMAPS) | ✅Yes |
| 55 | LUMP_LEAF_AMBIENT_LIGHTING_HDR | Stores volumetric ambient lighting information for each leaf | ✅Yes |
| 56 | LUMP_LEAF_AMBIENT_LIGHTING | Stores volumetric ambient lighting information for each leaf | ✅Yes |
| 57 | LUMP_XZIPPAKFILE | "XZip version of pak file for Xbox. Deprecated.", from Valve Dev Wiki | 🟥No. Can't find structs for this lump |
| 58 | LUMP_FACES_HDR | datas for rendering | ✅Yes |
| 59 | LUMP_MAP_FLAGS | Map compilation flags | ✅Yes |
| 60 | LUMP_OVERLAY_FADES | [not sure] fade distances for overlays? | ✅Yes |
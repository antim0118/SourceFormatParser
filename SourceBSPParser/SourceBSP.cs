using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static SourceFormatParser.BSP.SourceBSPStructs;
using Structs = SourceFormatParser.BSP.SourceBSPStructs;
using static SourceFormatParser.Common.ParsingUtils;
using SourceFormatParser.BigEndian;
using SourceFormatParser.Common;

namespace SourceFormatParser.BSP
{
    /// <summary>
    /// Supported version: 17+
    /// Tested on versions: 19-21
    /// </summary>
    public class SourceBSP : IDisposable
    {
        #region Public variables
        public Structs.dheader_t Header;

        public SourceGame Game;

        //both can be true, both can be false.
        public bool isHDR => (MapFlags.m_LevelFlags & (uint)MAP_FLAGS.LVLFLAGS_BAKED_STATIC_PROP_LIGHTING_HDR) != 0;
        public bool isLDR => (MapFlags.m_LevelFlags & (uint)MAP_FLAGS.LVLFLAGS_BAKED_STATIC_PROP_LIGHTING_NONHDR) != 0;
        #endregion
        #region Private variables
        BinaryReader BR;
        Stream stream;
        #endregion

        #region enums
        /// <summary>
        /// this enum for games with modified structs, to parse them properly later
        /// </summary>
        public enum SourceGame
        {
            Unknown,
            //==valve games
            //TeamFortress2,
            //Left4Dead,
            Left4Dead2,
            //Portal2,
            //CounterStrikeGlobalOffensive,
            CSGO_PS3,
            //Dota2,

            //==idk
            //ZenoClash,
            //DarkMessiah,
            //Vindictus,
            //TheShip,
            //BloodyGoodTime,
            //BlackMesa,
            //AlienSwarm,
            //DearEsther,
            //Titanfall
        }
        #endregion

        #region Init / Dispose
        public SourceBSP(string path, SourceGame game = SourceGame.Unknown)
        {
            stream = File.OpenRead(path);
            Init(stream, game);
        }
        public SourceBSP(Stream stream, SourceGame game = SourceGame.Unknown) => Init(stream, game);

        void Init(Stream stream, SourceGame game)
        {
            Game = game;
            if (game == SourceGame.CSGO_PS3)
                BR = new BigBinaryReader(stream, Encoding.ASCII);
            else
                BR = new BinaryReader(stream, Encoding.ASCII);
            ParseHeader();
        }

        public void Dispose()
        {
            BR.Dispose();
            if (stream != null)
                stream.Dispose();
        }
        #endregion

        #region Private Methods
        void ParseHeader()
        {
            BR.BaseStream.Seek(0, SeekOrigin.Begin);
            int id = BR.ReadInt32();
            BR.BaseStream.Seek(0, SeekOrigin.Begin);
            char[] idc = BR.ReadChars(4);
            if (id != IDBSPHEADER)
                throw new Exception("Not a BSP file! (wrong id)");

            Header = new Structs.dheader_t
            {
                ident = id,
                version = BR.ReadInt32(),
                lumps = ParseLumps(),
                mapRevision = BR.ReadInt32()
            };
        }

        Structs.lump_t[] ParseLumps()
        {
            Structs.lump_t[] lumps = new Structs.lump_t[Structs.HEADER_LUMPS];
            for (int l = 0; l < Structs.HEADER_LUMPS; l++)
            {
                if (Game == SourceGame.Left4Dead2)
                    lumps[l] = new Structs.lump_t
                    {
                        version = BR.ReadInt32(),
                        
                        fileofs = BR.ReadInt32(),
                        filelen = BR.ReadInt32(),
                        uncompressedSize = BR.ReadInt32(),

                        lumpNum = l
                    };
                else
                    lumps[l] = new Structs.lump_t
                    {
                        fileofs = BR.ReadInt32(),
                        filelen = BR.ReadInt32(),
                        version = BR.ReadInt32(),
                        uncompressedSize = BR.ReadInt32(),

                        lumpNum = l
                    };
            }
            return lumps;
        }
        #endregion

        #region Methods for lumps
        private lump_t SetLump(int lumpNum)
        {
            lump_t lump = Header.lumps[lumpNum];
            BR.BaseStream.Seek(lump.fileofs, SeekOrigin.Begin);
            return lump;
        }
        private byte[] ReadLump(int lumpNum) => BR.ReadBytes(SetLump(lumpNum).filelen);

        private int GetLumpCount(int lumpNum, int byteSize) => GetLumpCount(Header.lumps[lumpNum], byteSize);
        private int GetLumpCount(lump_t lump, int byteSize)
        {
            //check if byteSize is correct
            int c = lump.filelen / byteSize;
            if (lump.filelen % byteSize != 0)
            {
                //trying to find nearest byteSizes
                int nearMin=-1, nearMax=-1;
                for(int s = 1; s < 100; s++)
                {
                    if (byteSize - s <= 0) break;
                    if (lump.filelen % (byteSize - s) == 0)
                    {
                        nearMin = byteSize - s;
                        break;
                    }
                }
                for (int s = 1; s < 100; s++)
                {
                    if (lump.filelen % (byteSize + s) == 0)
                    {
                        nearMax = byteSize + s;
                        break;
                    }
                }
                string debug_text = $"[Lump #{lump.lumpNum}:v{lump.version}] Wrong count" + (nearMin != -1 || nearMax != -1 ? $" ({nearMin}; {nearMax})" : "");
                DebugLog.Write(debug_text);
            }
            return c;
        }
        #endregion

        #region Lumps

        #region Lump #0: Entities
        private List<SourceKV> _entities;
        /// <summary>
        /// Lump #0. 
        /// </summary>
        public List<SourceKV> Entities
        {
            get
            {
                if (_entities == null)
                {
                    string[] _ents = Encoding.ASCII.GetString(ReadLump(0)).Split('{', '}');

                    List<SourceKV> ents = new List<SourceKV>();
                    foreach(string e in _ents)
                    {
                        if (e.Length > 2)
                            ents.Add(new SourceKV(e, true));
                    }

                    _entities = ents;
                }
                return _entities;
            }
        }
        #endregion

        #region Lump #1: Planes
        private dplane_t[] _planes;
        /// <summary>
        /// Lump #1.  
        /// Purpose: [not sure] Calculations for movement (if player can collide with walls?)
        /// </summary>
        public dplane_t[] Planes
        {
            get
            {
                if (_planes == null)
                {
                    lump_t lump = SetLump(1);
                    int count = GetLumpCount(lump, 20);
                    _planes = new dplane_t[count];
                    for (int p = 0; p < count; p++)
                    {
                        _planes[p] = new dplane_t
                        {
                            normal = BR.ReadVector(),
                            dist = BR.ReadSingle(),
                            type = BR.ReadInt32()
                        };
                    }
                }
                return _planes;
            }
        }
        #endregion

        #region Lump #2: Texture data (TEXDATA)
        private dtexdata_t[] _texdata;
        /// <summary>
        /// Lump #2. 
        /// Purpose: texture referencing, texture preloading, lighting calculation (reflectivity)
        /// </summary>
        public dtexdata_t[] Texdata
        {
            get
            {
                if (_texdata == null)
                {
                    lump_t lump = SetLump(2);
                    int count = GetLumpCount(lump, 32);
                    _texdata = new dtexdata_t[count];
                    for (int t = 0; t < count; t++)
                    {
                        _texdata[t] = new dtexdata_t
                        {
                            reflectivity = BR.ReadVector(),
                            nameStringTableID = BR.ReadInt32(),
                            width = BR.ReadInt32(),
                            height = BR.ReadInt32(),
                            view_width = BR.ReadInt32(),
                            view_height = BR.ReadInt32()
                        };
                    }
                }
                return _texdata;
            }
        }
        #endregion

        #region Lump #3: Vertexes (vertices)
        private SourceVector[] _vertexes;
        /// <summary>
        /// Lump #3. 
        /// Purpose: brush/displacement rendering
        /// </summary>
        public SourceVector[] Vertexes
        {
            get
            {
                if (_vertexes == null)
                {
                    lump_t lump = SetLump(3);
                    int count = GetLumpCount(lump, 12);
                    _vertexes = new SourceVector[count];
                    for (int v = 0; v < count; v++)
                        _vertexes[v] = BR.ReadVector();
                }
                return _vertexes;
            }
        }
        #endregion

        #region Lump #4: Visibility
        private dvis_t _visibility;
        private bool __visibility;
        /// <summary>
        /// Lump #4. 
        /// Purpose: visibility optimizing for brushes/displacements/props/entities. 
        /// WARNING: may use many of RAM
        /// </summary>
        public dvis_t Visibility
        {
            get
            {
                if (!__visibility)
                {
                    lump_t lump = SetLump(4);
                    if(lump.filelen == 0)
                    {
                        __visibility = true;
                        return _visibility;
                    }

                    int numclusters = BR.ReadInt32();
                    int[][] bitofs = new int[numclusters][];
                    for(int b = 0; b < numclusters; b++)
                        bitofs[b] = BR.ReadInt32Array(2);

                    _visibility = new dvis_t
                    {
                        numclusters = numclusters,
                        bitofs = bitofs
                    };

                    //reading pvs
                    _visibility.pvs = new HashSet<byte>[numclusters];
                    for (int i = 0; i < numclusters; i++)
                    {
                        var ofs = bitofs[i][0];
                        BR.BaseStream.Seek(lump.fileofs + ofs, SeekOrigin.Begin);
                        _visibility.pvs[i] = new HashSet<byte>();
                        int offset = 0;
                        while (offset < numclusters)
                        {
                            byte bits = BR.ReadByte();
                            if (bits == 0)
                            {
                                offset += BR.ReadByte() * 8;
                                continue;
                            }

                            for (var ii = 0; ii < 8 && offset + ii < numclusters; ++ii)
                            {
                                if ((bits & (1 << ii)) != 0) 
                                    _visibility.pvs[i].Add((byte)(offset + ii));
                            }

                            offset += 8;
                        }
                    }

                    //reading pas
                    // === UNCOMMENT THIS IF ITS USEFUL FOR YOU ===
                    // == i commented it because it uses additional RAM.
                    // == don't think it can be useful for someone
                    //_visibility.pas = new HashSet<byte>[numclusters];
                    //for (int i = 0; i < numclusters; i++)
                    //{
                    //    var ofs = bitofs[i][1];
                    //    BR.BaseStream.Seek(lump.fileofs + ofs, SeekOrigin.Begin);
                    //    _visibility.pas[i] = new HashSet<byte>();
                    //    int offset = 0;
                    //    while (offset < numclusters)
                    //    {
                    //        byte bits = BR.ReadByte();
                    //        if (bits == 0)
                    //        {
                    //            offset += BR.ReadByte() * 8;
                    //            continue;
                    //        }

                    //        for (var ii = 0; ii < 8 && offset + ii < numclusters; ++ii)
                    //        {
                    //            if ((bits & (1 << ii)) != 0)
                    //                _visibility.pas[i].Add((byte)(offset + ii));
                    //        }

                    //        offset += 8;
                    //    }
                    //}

                    __visibility = true;
                }
                return _visibility;
            }
        }
        #endregion

        #region Lump #5: Nodes
        private dnode_t[] _nodes;
        /// <summary>
        /// Lump #5. 
        /// Purpose: [not sure] to determine player's position for visibility optimisation?
        /// </summary>
        public dnode_t[] Nodes
        {
            get
            {
                if (_nodes == null)
                {
                    lump_t lump = SetLump(5);
                    int count = GetLumpCount(lump, 32);
                    _nodes = new dnode_t[count];
                    for (int n = 0; n < count; n++)
                    {
                        _nodes[n] = new dnode_t
                        {
                            planenum = BR.ReadInt32(),
                            children = BR.ReadInt32Array(2),
                            mins = BR.ReadVectorShort(),
                            maxs = BR.ReadVectorShort(),
                            firstface = BR.ReadUInt16(),
                            numfaces = BR.ReadUInt16(),
                            area = BR.ReadInt16(),
                            //unused = BR.ReadInt16()
                        };
                        BR.BaseStream.Seek(2, SeekOrigin.Current);
                    }
                }
                return _nodes;
            }
        }
        #endregion

        #region Lump #6: Texture info (TEXINFO)
        private texinfo_t[] _texinfo;
        /// <summary>
        /// Lump #6. 
        /// Purpose: texture info. Flags, texture offset, lightmap [offset?], index for texdata.
        /// </summary>
        public texinfo_t[] Texinfo
        {
            get
            {
                if (_texinfo == null)
                {
                    lump_t lump = SetLump(6);
                    int count = GetLumpCount(lump, 72);
                    _texinfo = new texinfo_t[count];
                    for (int t = 0; t < count; t++)
                    {
                        _texinfo[t] = new texinfo_t
                        {
                            textureVecsTexelsPerWorldUnits = new SourceVector4[2] { BR.ReadVector4(), BR.ReadVector4() },
                            lightmapVecsLuxelsPerWorldUnits = new SourceVector4[2] { BR.ReadVector4(), BR.ReadVector4() },
                            flags = BR.ReadInt32(),
                            texdata = BR.ReadInt32()
                        };
                    }
                }
                return _texinfo;
            }
        }
        #endregion

        #region Lump #7: Faces (LDR)
        private dface_t[] _faces;
        /// <summary>
        /// Lump #7. Version 1.
        /// Purpose: datas for rendering.
        /// Also: Check lump #58 for HDR faces.
        /// </summary>
        public dface_t[] Faces
        {
            get
            {
                if (_faces == null)
                {
                    lump_t lump = SetLump(7);
                    int count = GetLumpCount(lump, 56);
                    _faces = new dface_t[count];
                    for (int f = 0; f < count; f++)
                    {
                        _faces[f] = new dface_t
                        {
                            planenum = BR.ReadUInt16(),
                            side = BR.ReadByte(),
                            onNode = BR.ReadByte(),
                            firstedge = BR.ReadInt32(),
                            numedges = BR.ReadInt16(),
                            texinfo = BR.ReadInt16(),
                            dispinfo = BR.ReadInt16(),
                            surfaceFogVolumeID = BR.ReadInt16(),
                            styles = BR.ReadBytes(4),
                            lightofs = BR.ReadInt32(),
                            area = BR.ReadSingle(),
                            m_LightmapTextureMinsInLuxels = BR.ReadVector2Int(),
                            m_LightmapTextureSizeInLuxels = BR.ReadVector2Int(),
                            origFace = BR.ReadInt32(),
                            m_NumPrims = BR.ReadUInt16(),
                            firstPrimID = BR.ReadUInt16(),
                            smoothingGroups = BR.ReadUInt32()
                        };
                    }
                }
                return _faces;
            }
        }
        #endregion

        #region Lump #8: Lighting (LDR)
        private ColorRGBExp32[] _lighting;
        /// <summary>
        /// Lump #8. Version 1.
        /// Purpose: lightmap colors / shadows.
        /// Also: Check lump #53 for HDR lighting.
        /// </summary>
        public ColorRGBExp32[] Lighting
        {
            get
            {
                if (_lighting == null)
                {
                    lump_t lump = SetLump(8);
                    int count = GetLumpCount(lump, 4);
                    _lighting = new ColorRGBExp32[count];
                    for (int f = 0; f < count; f++)
                    {
                        _lighting[f] = BR.ReadColorRGBExp32();
                    }
                }
                return _lighting;
            }
        }
        #endregion

        #region Lump #9: Occlusion
        private doccluder_t _occlusion;
        private bool __occlusion;
        /// <summary>
        /// Lump #9. Version 1/2.
        /// Purpose: [not sure] func_occluder entity info for compilation. 
        /// Usage for data (depending on version): (doccluderdataV?_t[])bsp.Occlusion.data
        /// </summary>
        public doccluder_t Occlusion
        {
            get
            {
                if (!__occlusion)
                {
                    lump_t lump = SetLump(9);
                    if(lump.filelen == 0)
                    {
                        __occlusion = true;
                        return _occlusion;
                    }

                    _occlusion = new doccluder_t();
                    _occlusion.count = BR.ReadInt32();
                    switch (lump.version)
                    {
                        case 1:
                            doccluderdataV1_t[] _o_v1 = new doccluderdataV1_t[_occlusion.count];
                            for (int o = 0; o < _occlusion.count; o++)
                                _o_v1[o] = new doccluderdataV1_t
                                {
                                    flags = BR.ReadInt32(),
                                    firstpoly = BR.ReadInt32(),
                                    polycount = BR.ReadInt32(),
                                    mins = BR.ReadVector(),
                                    maxs = BR.ReadVector()
                                };
                            _occlusion.data = _o_v1;
                            break;
                        case 2:
                            doccluderdataV2_t[] _o_v2 = new doccluderdataV2_t[_occlusion.count];
                            for (int o = 0; o < _occlusion.count; o++)
                                _o_v2[o] = new doccluderdataV2_t
                                {
                                    flags = BR.ReadInt32(),
                                    firstpoly = BR.ReadInt32(),
                                    polycount = BR.ReadInt32(),
                                    mins = BR.ReadVector(),
                                    maxs = BR.ReadVector(),
                                    area = BR.ReadInt32()
                                };
                            _occlusion.data = _o_v2;
                            break;
                        default:
                            __occlusion = true;
                            throw new NotImplementedException();
                    }

                    _occlusion.polyDataCount = BR.ReadInt32();
                    _occlusion.polyData = new doccluderpolydata_t[_occlusion.polyDataCount];
                    for (int p = 0; p < _occlusion.polyDataCount; p++)
                    {
                        _occlusion.polyData[p] = new doccluderpolydata_t
                        {
                            firstvertexindex = BR.ReadInt32(),
                            vertexcount = BR.ReadInt32(),
                            planenum = BR.ReadInt32()
                        };
                    }

                    _occlusion.vertexIndexCount = BR.ReadInt32();
                    _occlusion.vertexIndices = BR.ReadInt32Array(_occlusion.vertexIndexCount);

                    __occlusion = true;
                }
                return _occlusion;
            }
        }
        #endregion

        #region Lump #10: Leafs
        private object _leafs;
        private bool __leafs;
        /// <summary>
        /// Lump #10. Version 0/1. 
        /// Purpose: [not sure] to determine player's position for visibility optimisation? 
        /// Usage (depending on version): (dleafV?_t[])bsp.Leafs
        /// </summary>
        public object Leafs
        {
            get
            {
                if (!__leafs)
                {
                    lump_t lump = SetLump(10);
                    if (lump.filelen == 0)
                    {
                        __leafs = true;
                        return _leafs;
                    }
                    int count;
                    int contents;
                    short cluster;
                    uint packed;

                    switch (lump.version)
                    {
                        case 0:
                            count = GetLumpCount(lump, 56);
                            dleafV0_t[] _l_0 = new dleafV0_t[count];
                            for (int l = 0; l < count; l++)
                            {
                                contents = BR.ReadInt32();
                                cluster = BR.ReadInt16();
                                packed = BR.ReadUInt32(); //area:9 and flags:7
                                _l_0[l] = new dleafV0_t
                                {
                                    contents = contents,
                                    cluster = cluster,
                                    area = (short)((ushort)(packed << 7) >> 7),
                                    flags = (short)(packed >> 9),
                                    mins = BR.ReadVectorShort(),
                                    maxs = BR.ReadVectorShort(),
                                    firstleafface = BR.ReadUInt16(),
                                    numleaffaces = BR.ReadUInt16(),
                                    firstleafbrush = BR.ReadUInt16(),
                                    numleafbrushes = BR.ReadUInt16(),
                                    leafWaterDataID = BR.ReadInt16(),
                                    m_AmbientLighting = BR.ReadCompressedLightCube()
                                };
                            }
                            _leafs = _l_0;
                            break;
                        case 1:
                            count = GetLumpCount(lump, 32);
                            dleafV1_t[] _l_1 = new dleafV1_t[count];
                            for (int l = 0; l < count; l++)
                            {
                                contents = BR.ReadInt32();
                                cluster = BR.ReadInt16();
                                packed = BR.ReadUInt32(); //area:9 and flags:7
                                _l_1[l] = new dleafV1_t
                                {
                                    contents = contents,
                                    cluster = cluster,
                                    area = (short)((ushort)(packed << 7) >> 7),
                                    flags = (short)(packed >> 9),
                                    mins = BR.ReadVectorShort(),
                                    maxs = BR.ReadVectorShort(),
                                    firstleafface = BR.ReadUInt16(),
                                    numleaffaces = BR.ReadUInt16(),
                                    firstleafbrush = BR.ReadUInt16(),
                                    numleafbrushes = BR.ReadUInt16(),
                                    leafWaterDataID = BR.ReadInt16()
                                };
                            }
                            _leafs = _l_1;
                            break;
                        default:
                            __leafs = true;
                            throw new NotImplementedException();
                    }

                    
                    __leafs = true;
                }
                return _leafs;
            }
        }
        #endregion

        #region Lump #11: Face IDs
        private dfaceid_t[] _faceids;
        /// <summary>
        /// Lump #11. 
        /// Purpose: Face IDs from Valve Hammer Editor. Should equal Faces lump count.
        /// </summary>
        public dfaceid_t[] FaceIDs
        {
            get
            {
                if (_faceids == null)
                {
                    lump_t lump = SetLump(11);
                    int count = GetLumpCount(lump, 2);
                    _faceids = new dfaceid_t[count];
                    for (int f = 0; f < count; f++)
                    {
                        _faceids[f] = new dfaceid_t { hammerfaceid = BR.ReadUInt16() };
                    }
                }
                return _faceids;
            }
        }
        #endregion

        #region Lump #12: Edges
        private dedge_t[] _edges;
        /// <summary>
        /// Lump #12. 
        /// Purpose: vertex indices for faces
        /// </summary>
        public dedge_t[] Edges
        {
            get
            {
                if (_edges == null)
                {
                    lump_t lump = SetLump(12);
                    int count = GetLumpCount(lump, 4);
                    _edges = new dedge_t[count];
                    for (int e = 0; e < count;e++)
                    {
                        _edges[e] = new dedge_t { v = BR.ReadVector2Short() };
                    }
                }
                return _edges;
            }
        }
        #endregion

        #region Lump #13: Surface edges (SURFEDGE)
        private int[] _surfedge;
        /// <summary>
        /// Lump #13. 
        /// Purpose: Faces.firstedge/numedges (e) ->| Surfedge[e] (s) ->| Edges[s].
        /// </summary>
        public int[] SurfEdge
        {
            get
            {
                if (_surfedge == null)
                {
                    lump_t lump = SetLump(13);
                    int count = GetLumpCount(lump, 4);
                    _surfedge = BR.ReadInt32Array(count);
                }
                return _surfedge;
            }
        }
        #endregion

        #region Lump #14: Models
        private dmodel_t[] _models;
        /// <summary>
        /// Lump #14. 
        /// Purpose: Brushes for rendering (mostly). [0] is global, [1 and more] are entities (func_brush/func_detail)
        /// </summary>
        public dmodel_t[] Models
        {
            get
            {
                if (_models == null)
                {
                    lump_t lump = SetLump(14);
                    int count = GetLumpCount(lump, 48);
                    _models = new dmodel_t[count];
                    for (int m = 0; m < count; m++)
                    {
                        _models[m] = new dmodel_t
                        {
                            mins = BR.ReadVector(),
                            maxs = BR.ReadVector(),
                            origin = BR.ReadVector(),
                            headnode = BR.ReadInt32(),
                            firstface = BR.ReadInt32(),
                            numfaces = BR.ReadInt32()
                        };
                    }
                }
                return _models;
            }
        }
        #endregion

        #region Lump #15: World lights (LDR)
        private object _worldlights;
        /// <summary>
        /// Lump #15. Version 0/1. 
        /// Purpose: Contains info about static lights (NOT LIGHTMAPS). 
        /// Usage (depending on version): (dworldlightV?_t[])bsp.WorldLights. 
        /// Also: Check lump #54 for HDR world lights.
        /// </summary>
        public object WorldLights
        {
            get
            {
                if (_worldlights == null)
                {
                    lump_t lump = SetLump(15);
                    int count;
                    switch (lump.version)
                    {
                        case 0:
                            count = GetLumpCount(lump, 88);
                            dworldlightV0_t[] _wl_0 = new dworldlightV0_t[count];
                            for (int wl = 0; wl < count; wl++)
                            {
                                _wl_0[wl] = new dworldlightV0_t
                                {
                                    origin = BR.ReadVector(),
                                    intensity = BR.ReadVector(),
                                    normal = BR.ReadVector(),
                                    cluster = BR.ReadInt32(),
                                    type = (emittype_t)BR.ReadUInt32(),
                                    style = BR.ReadInt32(),
                                    stopdot = BR.ReadSingle(),
                                    stopdot2 = BR.ReadSingle(),
                                    exponent = BR.ReadSingle(),
                                    radius = BR.ReadSingle(),
                                    constant_attn = BR.ReadSingle(),
                                    linear_attn = BR.ReadSingle(),
                                    quadratic_attn = BR.ReadSingle(),
                                    flags = BR.ReadInt32(),
                                    texinfo = BR.ReadInt32(),
                                    owner = BR.ReadInt32()
                                };
                            }
                            _worldlights = _wl_0;
                            break;
                        case 1:
                            count = GetLumpCount(lump, 100);
                            dworldlightV1_t[] _wl_1 = new dworldlightV1_t[count];
                            for (int wl = 0; wl < count; wl++)
                            {
                                _wl_1[wl] = new dworldlightV1_t
                                {
                                    origin = BR.ReadVector(),
                                    intensity = BR.ReadVector(),
                                    normal = BR.ReadVector(),
                                    shadow_cast_offset = BR.ReadVector(),
                                    cluster = BR.ReadInt32(),
                                    type = (emittype_t)BR.ReadUInt32(),
                                    style = BR.ReadInt32(),
                                    stopdot = BR.ReadSingle(),
                                    stopdot2 = BR.ReadSingle(),
                                    exponent = BR.ReadSingle(),
                                    radius = BR.ReadSingle(),
                                    constant_attn = BR.ReadSingle(),
                                    linear_attn = BR.ReadSingle(),
                                    quadratic_attn = BR.ReadSingle(),
                                    flags = BR.ReadInt32(),
                                    texinfo = BR.ReadInt32(),
                                    owner = BR.ReadInt32()
                                };
                            }
                            _worldlights = _wl_1;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    
                }
                return _worldlights;
            }
        }
        #endregion

        #region Lump #16: Leaf faces
        private ushort[] _leaffaces;
        /// <summary>
        /// Lump #16. 
        /// Purpose: "... which are used to map from faces referenced in the leaf structure to indices in the face array", from Valve Dev Wiki. 
        /// </summary>
        public ushort[] LeafFaces
        {
            get
            {
                if (_leaffaces == null)
                {
                    lump_t lump = SetLump(16);
                    int count = GetLumpCount(lump, 2);
                    _leaffaces = BR.ReadUInt16Array(count);
                }
                return _leaffaces;
            }
        }
        #endregion

        #region Lump #17: Leaf brushes
        private ushort[] _leafbrushes;
        /// <summary>
        /// Lump #17. 
        /// Purpose: "does the same thing [as lump 16 - leaf faces] for brushes referenced in leaves", from Valve Dev Wiki. 
        /// </summary>
        public ushort[] LeafBrushes
        {
            get
            {
                if (_leafbrushes == null)
                {
                    lump_t lump = SetLump(17);
                    int count = GetLumpCount(lump, 2);
                    _leafbrushes = BR.ReadUInt16Array(count);
                }
                return _leafbrushes;
            }
        }
        #endregion

        #region Lump #18: Brushes
        private dbrush_t[] _brushes;
        /// <summary>
        /// Lump #18. 
        /// Purpose: Brush info. 
        /// </summary>
        public dbrush_t[] Brushes
        {
            get
            {
                if (_brushes == null)
                {
                    lump_t lump = SetLump(18);
                    int count = GetLumpCount(lump, 12);
                    _brushes = new dbrush_t[count];
                    for (int b = 0; b < count; b++)
                    {
                        _brushes[b] = new dbrush_t
                        {
                            firstside = BR.ReadInt32(),
                            numsides = BR.ReadInt32(),
                            contents = BR.ReadInt32()
                        };
                    }
                }
                return _brushes;
            }
        }
        #endregion

        #region Lump #19: Brush Sides
        private dbrushside_t[] _brushsides;
        /// <summary>
        /// Lump #19. 
        /// Purpose: Brush side info. 
        /// </summary>
        public dbrushside_t[] BrushSides
        {
            get
            {
                if (_brushsides == null)
                {
                    lump_t lump = SetLump(19);
                    int count = GetLumpCount(lump, 8);
                    _brushsides = new dbrushside_t[count];
                    for (int bs = 0; bs < count; bs++)
                    {
                        _brushsides[bs] = new dbrushside_t
                        {
                            planenum = BR.ReadUInt16(),
                            texinfo = BR.ReadInt16(),
                            dispinfo = BR.ReadInt16(),
                            bevel = BR.ReadInt16()
                        };
                    }
                }
                return _brushsides;
            }
        }
        #endregion

        #region Lump #20: Areas
        private darea_t[] _areas;
        /// <summary>
        /// Lump #20. 
        /// Purpose: Areas that are used for switching of areaportals; visibility things. 
        /// </summary>
        public darea_t[] Areas
        {
            get
            {
                if (_areas == null)
                {
                    lump_t lump = SetLump(20);
                    int count = GetLumpCount(lump, 8);
                    _areas = new darea_t[count];
                    for (int a = 0; a < count; a++)
                    {
                        _areas[a] = new darea_t
                        {
                            numareaportals = BR.ReadInt32(),
                            firstareaportal = BR.ReadInt32()
                        };
                    }
                }
                return _areas;
            }
        }
        #endregion

        #region Lump #21: Areaportals
        private dareaportal_t[] _areaportals;
        /// <summary>
        /// Lump #21. 
        /// Purpose: "used with func_areaportal and func_areaportalwindow entities to define sections of the map that can be switched to render or not render", from Valve Dev Wiki.
        /// </summary>
        public dareaportal_t[] Areaportals
        {
            get
            {
                if (_areaportals == null)
                {
                    lump_t lump = SetLump(21);
                    int count = GetLumpCount(lump, 12);
                    _areaportals = new dareaportal_t[count];
                    for (int ap = 0; ap < count; ap++)
                    {
                        _areaportals[ap] = new dareaportal_t
                        {
                            m_PortalKey = BR.ReadUInt16(),
                            otherarea = BR.ReadUInt16(),
                            m_FirstClipPortalVert = BR.ReadUInt16(),
                            m_nClipPortalVerts = BR.ReadUInt16(),
                            planenum = BR.ReadInt32()
                        };
                    }
                }
                return _areaportals;
            }
        }
        #endregion

        #region Lump #22 : 2004/2009
        #region Lump #22/1: Portals [Source 2004]
        private dportal_t[] _portals;
        /// <summary>
        /// Lump #22. 
        /// Purpose: [not sure] Used by VVIS and VRAD only.
        /// </summary>
        public dportal_t[] Portals
        {
            get
            {
                if (_portals == null)
                {
                    lump_t lump = SetLump(22);
                    if(lump.filelen % 16 != 0) //This lump is not for portals
                        return null;
                    int count = GetLumpCount(lump, 16);
                    _portals = new dportal_t[count];
                    for (int p = 0; p < count; p++)
                    {
                        _portals[p] = new dportal_t
                        {
                            firstportalvert = BR.ReadInt32(),
                            numportalverts = BR.ReadInt32(),
                            planenum = BR.ReadInt32(),
                            cluster = BR.ReadUInt16Array(2)
                        };
                    }
                }
                return _portals;
            }
        }
        #endregion
        #region Lump #22/2: Prop collisions (PROPCOLLISION) [Source 2009]
        private dpropcollision_t[] _propcollision;
        /// <summary>
        /// Lump #22. 
        /// Purpose: "Static props convex hull lists", from Valve Dev Wiki.
        /// </summary>
        public dpropcollision_t[] PropCollision
        {
            get
            {
                if (_propcollision == null)
                {
                    lump_t lump = SetLump(22);
                    if (lump.filelen % 8 != 0) //This lump is not for prop collisions
                        return null;

                    int count = GetLumpCount(lump, 8);
                    _propcollision = new dpropcollision_t[count];
                    for (int p = 0; p < count; p++)
                    {
                        _propcollision[p] = new dpropcollision_t
                        {
                            m_nHullCount = BR.ReadInt32(),
                            m_nHullStart = BR.ReadInt32()
                        };
                    }
                }
                return _propcollision;
            }
        }
        #endregion
        #endregion

        #region Lump #23 : 2004/2009
        #region Lump #23/1: Clusters [Source 2004]
        private dcluster_t[] _clusters;
        /// <summary>
        /// Lump #23. 
        /// Purpose: [not sure] Used by VVIS and VRAD only.
        /// </summary>
        public dcluster_t[] Clusters
        {
            get
            {
                if (_clusters == null)
                {
                    lump_t lump = SetLump(23);
                    if (lump.filelen % 8 != 0) //This lump is not for clusters
                        return null;

                    int count = GetLumpCount(lump, 8);
                    _clusters = new dcluster_t[count];
                    for (int c = 0; c < count; c++)
                    {
                        _clusters[c] = new dcluster_t
                        {
                            firstportal = BR.ReadInt32(),
                            numportals = BR.ReadInt32()
                        };
                    }
                }
                return _clusters;
            }
        }
        #endregion
        #region Lump #23/2: Prop hulls (PROPHULLS) [Source 2009]
        private dprophull_t[] _prophulls;
        /// <summary>
        /// Lump #23. 
        /// Purpose: "Static prop convex hulls", from Valve Dev Wiki.
        /// </summary>
        public dprophull_t[] PropHulls
        {
            get
            {
                if (_prophulls == null)
                {
                    lump_t lump = SetLump(23);
                    if (lump.filelen % 16 != 0) //This lump is not for prophulls
                        return null;

                    int count = GetLumpCount(lump, 16);
                    _prophulls = new dprophull_t[count];
                    for (int p = 0; p < count; p++)
                    {
                        _prophulls[p] = new dprophull_t
                        {
                            m_nVertCount = BR.ReadInt32(),
                            m_nVertStart = BR.ReadInt32(),
                            m_nSurfaceProp = BR.ReadInt32(),
                            m_nContents = BR.ReadUInt32()
                        };
                    }
                }
                return _prophulls;
            }
        }
        #endregion
        #endregion

        #region Lump #24 : 2004/2009 - not tested: can't find map with this lump
        #region Lump #24/1: Portal Vertices (PORTALVERTS) [Source 2004]
        private ushort[] _portalverts;
        /// <summary>
        /// Lump #24. 
        /// Purpose: [not sure] Used by VVIS and VRAD only.
        /// </summary>
        public ushort[] PortalVerts
        {
            get
            {
                if (_portalverts == null)
                {
                    lump_t lump = SetLump(24);
                    int count = GetLumpCount(lump, 2);
                    _portalverts = BR.ReadUInt16Array(count);
                }
                return _portalverts;
            }
        }
        #endregion
        #region Lump #24/2: Prop hull vertices (PROPHULLVERTS) [Source 2009]
        private SourceVector[] _prophullverts;
        /// <summary>
        /// Lump #24. 
        /// Purpose: "Static prop collision vertices", from Valve Dev Wiki.
        /// </summary>
        public SourceVector[] PropHullVerts
        {
            get
            {
                if (_prophullverts == null)
                {
                    lump_t lump = SetLump(24);
                    int count = GetLumpCount(lump, 12);
                    _prophullverts = new SourceVector[count];
                    for (int v = 0; v < count; v++)
                    {
                        _prophullverts[v] = BR.ReadVector();
                    }
                }
                return _prophullverts;
            }
        }
        #endregion
        #endregion

        #region Lump #25 : 2004/2009 - not tested: can't find map with this lump
        #region Lump #25/1: Cluster Portals [Source 2004]
        private ushort[] _clusterportals;
        /// <summary>
        /// Lump #25. 
        /// Purpose: [not sure] Used by VVIS and VRAD only.
        /// </summary>
        public ushort[] ClusterPortals
        {
            get
            {
                if (_clusterportals == null)
                {
                    lump_t lump = SetLump(25);
                    int count = GetLumpCount(lump, 2);
                    _clusterportals = BR.ReadUInt16Array(count);
                }
                return _clusterportals;
            }
        }
        #endregion
        #region Lump #25/2: Prop Triangles (PROPTRIS) [Source 2009]
        private dprophulltris_t[] _proptris;
        /// <summary>
        /// Lump #25. 
        /// Purpose: "Static prop per hull triangle index start/count", from Valve Dev Wiki.
        /// </summary>
        public dprophulltris_t[] PropTris
        {
            get
            {
                if (_proptris == null)
                {
                    lump_t lump = SetLump(25);
                    int count = GetLumpCount(lump, 8);
                    _proptris = new dprophulltris_t[count];
                    for (int p = 0; p < count; p++)
                    {
                        _proptris[p] = new dprophulltris_t
                        {
                            m_nIndexStart = BR.ReadInt32(),
                            m_nIndexCount = BR.ReadInt32()
                        };
                    }
                }
                return _proptris;
            }
        }
        #endregion
        #endregion

        #region Lump #26: Displacement infos (DISPINFO) - SKIPPED: m_EdgeNeighbors/m_CornerNeighbors/m_AllowedVerts/unknown
        private ddispinfo_t[] _dispinfo;
        /// <summary>
        /// Lump #26. 
        /// Purpose: [not sure] Displacement infos that is used by map compilators?
        /// </summary>
        public ddispinfo_t[] DispInfo
        {
            get
            {
                if (_dispinfo == null)
                {
                    lump_t lump = SetLump(26);
                    int count = GetLumpCount(lump, 176);
                    _dispinfo = new ddispinfo_t[count];
                    for (int d = 0; d < count; d++)
                    {
                        _dispinfo[d] = new ddispinfo_t
                        {
                            startPosition=BR.ReadVector(),
                            m_iDispVertStart=BR.ReadInt32(),
                            m_iDispTriStart=BR.ReadInt32(),
                            power=BR.ReadInt32(),
                            minTess=BR.ReadInt32(),
                            smoothingAngle=BR.ReadSingle(),
                            contents=BR.ReadInt32(),
                            m_iMapFace=BR.ReadUInt16(),
                            m_iLightmapAlphaStart=BR.ReadInt32(),
                            m_iLightmapSamplePositionStart=BR.ReadInt32()
                        };

                        BR.BaseStream.Seek(130, SeekOrigin.Current);
                    }
                }
                return _dispinfo;
            }
        }
        #endregion

        #region Lump #27: Original faces 
        private dface_t[] _originalfaces;
        /// <summary>
        /// Lump #27. Version 1.
        /// Purpose: "Brush faces array before splitting", VDW. Probably, used by map compilators.
        /// </summary>
        public dface_t[] OriginalFaces
        {
            get
            {
                if (_originalfaces == null)
                {
                    lump_t lump = SetLump(27);
                    int count = GetLumpCount(lump, 56);
                    _originalfaces = new dface_t[count];
                    for (int f = 0; f < count; f++)
                    {
                        _originalfaces[f] = new dface_t
                        {
                            planenum = BR.ReadUInt16(),
                            side = BR.ReadByte(),
                            onNode = BR.ReadByte(),
                            firstedge = BR.ReadInt32(),
                            numedges = BR.ReadInt16(),
                            texinfo = BR.ReadInt16(),
                            dispinfo = BR.ReadInt16(),
                            surfaceFogVolumeID = BR.ReadInt16(),
                            styles = BR.ReadBytes(4),
                            lightofs = BR.ReadInt32(),
                            area = BR.ReadSingle(),
                            m_LightmapTextureMinsInLuxels = BR.ReadVector2Int(),
                            m_LightmapTextureSizeInLuxels = BR.ReadVector2Int(),
                            origFace = BR.ReadInt32(),
                            m_NumPrims = BR.ReadUInt16(),
                            firstPrimID = BR.ReadUInt16(),
                            smoothingGroups = BR.ReadUInt32()
                        };
                    }
                }
                return _originalfaces;
            }
        }
        #endregion

        #region Lump #28: Displacement Physics (PHYSDISP) - not full: SourcePHY parser needed (i guess)
        private dphysdisp_t[] _physdisp;
        /// <summary>
        /// Lump #28. 
        /// Purpose: Displacement physics collision data.
        /// </summary>
        public dphysdisp_t[] PhysDisp
        {
            get
            {
                if (_physdisp == null)
                {
                    lump_t lump = SetLump(28);
                    //int count = GetLumpCount(lump, 2);
                    List<dphysdisp_t> _physdisp_list = new List<dphysdisp_t>();
                    while(BR.BaseStream.Position < lump.fileofs + lump.filelen - 1)
                    {
                        ushort numDisplacements = BR.ReadUInt16();
                        ushort[] dataSize = BR.ReadUInt16Array(numDisplacements);
                        if (dataSize.Length >= 1 && dataSize[0] == 0)
                            break;

                        long totalsize = 0;
                        foreach (ushort s in dataSize) totalsize += s;

                        _physdisp_list.Add(new dphysdisp_t
                        {
                            numDisplacements = numDisplacements,
                            dataSize = dataSize
                        });
                    }

                    //if (lump.filelen > 0 && lump.filelen - (BR.BaseStream.Position - lump.fileofs) > 0)
                    //    DebugLog.Write($"readen={BR.BaseStream.Position - lump.fileofs}; len={lump.filelen}; left={lump.filelen - (BR.BaseStream.Position - lump.fileofs)}");

                    // == There i should parse these displacements, but i dont know how (idk what is that format, mb PHY?).
                    // === g_pPhysDisp, g_PhysDispSize

                    _physdisp = _physdisp_list.ToArray();
                }
                return _physdisp;
            }
        }
        #endregion

        #region Lump #29: Physics collision infos (PHYSCOLLIDE) - not full: SourcePHY parser needed
        private dphysmodel_t[] _physcollide;
        /// <summary>
        /// Lump #29. 
        /// Purpose: Physics collision data. Used to calculate VPhysics (for prop_physics, i guess).
        /// </summary>
        public dphysmodel_t[] PhysCollide
        {
            get
            {
                if (_physcollide == null)
                {
                    lump_t lump = SetLump(29);
                    //int count = GetLumpCount(lump, 2);
                    List<dphysmodel_t> _physcollide_list = new List<dphysmodel_t>();
                    while (BR.BaseStream.Position < lump.fileofs + lump.filelen - 1)
                    {
                        var physmodel = new dphysmodel_t
                        {
                            modelIndex = BR.ReadInt32(),
                            dataSize = BR.ReadInt32(),
                            keydataSize = BR.ReadInt32(),
                            solidCount = BR.ReadInt32()
                        };

                        if (physmodel.modelIndex == -1)
                            break;

                        int size;
                        for (int k = 0; k < physmodel.solidCount; k++)
                        {
                            size = BR.ReadInt32();
                            byte[] collisionData = BR.ReadBytes(size);
                            //sourcephy solid = sourcephy.parsebody?(collisiondata);
                            // = or
                            //sourcephy solid = sourcephy.parsebody?(BR); // - to read bytes dynamically?
                        }

                        physmodel.keydata = Encoding.ASCII.GetString(BR.ReadBytes(physmodel.keydataSize - 1)); //-1 because last is always \0
                        BR.BaseStream.Seek(1, SeekOrigin.Current); //skip 1 byte because of prev line

                        _physcollide_list.Add(physmodel);
                    }

                    // == "The last two parts appear to be identical to the PHY file format, which means their exact contents are unknown."
                    // === - from Valve Dev Wiki.
                    // === Anchored link: https://developer.valvesoftware.com/wiki/Source_BSP_File_Format#Physics

                    _physcollide = _physcollide_list.ToArray();
                }
                return _physcollide;
            }
        }
        #endregion

        #region Lump #30: Vertex Normals (VERTNORMALS)
        private SourceVector[] _vertnormals;
        /// <summary>
        /// Lump #30. 
        /// Purpose: Face plane normals. "may be related to smoothing of lightmaps on faces", from Valve Dev Wiki.
        /// </summary>
        public SourceVector[] VertNormals
        {
            get
            {
                if (_vertnormals == null)
                {
                    lump_t lump = SetLump(30);
                    int count = GetLumpCount(lump, 12);

                    _vertnormals = new SourceVector[count];
                    for (int i = 0; i < count; i++)
                        _vertnormals[i] = BR.ReadVector();
                }
                return _vertnormals;
            }
        }
        #endregion

        #region Lump #31: Vertex Normal Indices (VERTNORMALINDICES)
        private ushort[] _vertnormalindices;
        /// <summary>
        /// Lump #31. 
        /// Purpose: Face plane normal index array. "may be related to smoothing of lightmaps on faces", from Valve Dev Wiki.
        /// </summary>
        public ushort[] VertNormalIndices
        {
            get
            {
                if (_vertnormalindices == null)
                {
                    lump_t lump = SetLump(31);
                    int count = GetLumpCount(lump, 2);
                    _vertnormalindices = BR.ReadUInt16Array(count);
                }
                return _vertnormalindices;
            }
        }
        #endregion

        #region Lump #32: Lightmap alphas for displacements (DISP_LIGHTMAP_ALPHAS)
        private byte[] _disp_lightmap_alphas;
        /// <summary>
        /// Lump #32. 
        /// Purpose: "Displacement lightmap alphas (unused/empty since Source 2006)", from Valve Dev Wiki.
        /// </summary>
        public byte[] DispLightmapAlphas
        {
            get
            {
                if (_disp_lightmap_alphas == null)
                {
                    lump_t lump = SetLump(32);
                    _disp_lightmap_alphas = BR.ReadBytes(lump.filelen);
                }
                return _disp_lightmap_alphas;
            }
        }
        #endregion

        #region Lump #33: Displacement vertices (DISP_VERTS)
        private CDispVert[] _dispverts;
        /// <summary>
        /// Lump #33. 
        /// Purpose: Info about vertices for displacements.
        /// </summary>
        public CDispVert[] DispVerts
        {
            get
            {
                if (_dispverts == null)
                {
                    lump_t lump = SetLump(33);
                    int count = GetLumpCount(lump, 20);
                    _dispverts = new CDispVert[count];
                    for (int v = 0; v < count; v++)
                    {
                        _dispverts[v] = new CDispVert
                        {
                            m_vVector = BR.ReadVector(),
                            m_flDist = BR.ReadSingle(),
                            m_flAlpha = BR.ReadSingle()
                        };
                    }
                }
                return _dispverts;
            }
        }
        #endregion

        #region Lump #34: Displacement Lightmap Sample Positions (DISP_LIGHTMAP_SAMPLE_POSITIONS)
        private byte[] _displmsamplepos;
        /// <summary>
        /// Lump #34. 
        /// Purpose: supposed to be ColorRGBExp32, but its byte array in source code. 
        /// i dunno that's literally boxes on white background (as i can remember). it's useless because lightmaps for disps are in LUMP_LIGHTING.
        /// </summary>
        public byte[] DispLightmapSamplePositions
        {
            get
            {
                if (_displmsamplepos == null)
                {
                    lump_t lump = SetLump(34);
                    _displmsamplepos = BR.ReadBytes(lump.filelen);
                }
                return _displmsamplepos;
            }
        }
        #endregion

        #region Lump #35: Game lump
        private dgamelumpheader_t _gamelump;
        /// <summary>
        /// Lump #35. 
        /// Purpose: Game-specific data lump. Contains info about static props and detail prop.
        /// </summary>
        public dgamelumpheader_t GameLump
        {
            get
            {
                if (_gamelump.lumps == null)
                {
                    lump_t lump = SetLump(35);

                    if (lump.filelen == 0)
                    {
                        _gamelump.lumps = new dgamelump_t[0];
                        return _gamelump;
                    }

                    // Read dictionary...
                    _gamelump = new dgamelumpheader_t { lumpCount = BR.ReadInt32() };
                    _gamelump.lumps = new dgamelump_t[_gamelump.lumpCount];
                    for (int l = 0; l < _gamelump.lumpCount; l++)
                    {
                        _gamelump.lumps[l] = new dgamelump_t
                        {
                            id = BR.ReadInt32(),
                            flags = BR.ReadUInt16(),
                            version = BR.ReadUInt16(),
                            fileofs = BR.ReadInt32(),
                            filelen = BR.ReadInt32()
                        };
                    }
                    for (int l = 0; l < _gamelump.lumpCount; ++l)
                    {
                        dgamelump_t gamelump = _gamelump.lumps[l];
                        long startofs = BR.BaseStream.Position;
                        long endofs = startofs + gamelump.filelen;
                        int count = 0;
                        switch (gamelump.id_code)
                        {
                            case GAMELUMP_CODES.GAMELUMP_STATIC_PROPS:
                                //read names
                                count = BR.ReadInt32();
                                _gamelump.staticPropNames = new StaticPropDictLump_t[count];
                                //Elapsed	{00:00:00.0005491} {00:00:00.0005547}
                                for (int n = 0; n < count; n++)
                                {
                                    string name = string.Empty;

                                    for (int b = 0; b < 128; b++)
                                    {
                                        byte _c = BR.ReadByte();
                                        if (_c == (byte)'\0')
                                        {
                                            BR.BaseStream.Seek(128 - b - 1, SeekOrigin.Current);
                                            break;
                                        }
                                        name += Encoding.ASCII.GetString(new byte[1] { _c });
                                    }

                                    _gamelump.staticPropNames[n].m_Name = name;
                                }

                                //read leafs
                                count = BR.ReadInt32();
                                _gamelump.staticPropLeafs = new StaticPropLeafLump_t[count];
                                for (int lf = 0; lf < count; lf++)
                                    _gamelump.staticPropLeafs[lf].m_Leaf = BR.ReadUInt16();


                                //read the models
                                count = BR.ReadInt32();
                                switch (gamelump.version)
                                {
                                    case 4:
                                        StaticPropLumpV4_t[] sp_v4 = new StaticPropLumpV4_t[count];
                                        for (int sp = 0; sp < count; sp++)
                                        {
                                            sp_v4[sp] = new StaticPropLumpV4_t
                                            {
                                                m_Origin = BR.ReadVector(),
                                                m_Angles = BR.ReadQAngle(),
                                                m_PropType = BR.ReadUInt16(),
                                                m_FirstLeaf = BR.ReadUInt16(),
                                                m_LeafCount = BR.ReadUInt16(),
                                                m_Solid = BR.ReadByte(),
                                                m_Flags = BR.ReadByte(),
                                                m_Skin = BR.ReadInt32(),
                                                m_FadeMinDist = BR.ReadSingle(),
                                                m_FadeMaxDist = BR.ReadSingle(),
                                                m_LightingOrigin = BR.ReadVector()
                                            };
                                        }
                                        _gamelump.staticProps = sp_v4;
                                        break;
                                    case 5:
                                        StaticPropLumpV5_t[] sp_v5 = new StaticPropLumpV5_t[count];
                                        for (int sp = 0; sp < count; sp++)
                                        {
                                            sp_v5[sp] = new StaticPropLumpV5_t
                                            {
                                                m_Origin = BR.ReadVector(),
                                                m_Angles = BR.ReadQAngle(),
                                                m_PropType = BR.ReadUInt16(),
                                                m_FirstLeaf = BR.ReadUInt16(),
                                                m_LeafCount = BR.ReadUInt16(),
                                                m_Solid = BR.ReadByte(),
                                                m_Flags = BR.ReadByte(),
                                                m_Skin = BR.ReadInt32(),
                                                m_FadeMinDist = BR.ReadSingle(),
                                                m_FadeMaxDist = BR.ReadSingle(),
                                                m_LightingOrigin = BR.ReadVector(),
                                                m_flForcedFadeScale = BR.ReadSingle()
                                            };
                                        }
                                        _gamelump.staticProps = sp_v5;
                                        break;
                                    case 6:
                                        StaticPropLumpV6_t[] sp_v6 = new StaticPropLumpV6_t[count];
                                        for (int sp = 0; sp < count; sp++)
                                        {
                                            sp_v6[sp] = new StaticPropLumpV6_t
                                            {
                                                m_Origin = BR.ReadVector(),
                                                m_Angles = BR.ReadQAngle(),
                                                m_PropType = BR.ReadUInt16(),
                                                m_FirstLeaf = BR.ReadUInt16(),
                                                m_LeafCount = BR.ReadUInt16(),
                                                m_Solid = BR.ReadByte(),
                                                m_Flags = BR.ReadByte(),
                                                m_Skin = BR.ReadInt32(),
                                                m_FadeMinDist = BR.ReadSingle(),
                                                m_FadeMaxDist = BR.ReadSingle(),
                                                m_LightingOrigin = BR.ReadVector(),
                                                m_flForcedFadeScale = BR.ReadSingle(),
                                                m_nMinDXLevel = BR.ReadUInt16(),
                                                m_nMaxDXLevel = BR.ReadUInt16()
                                            };
                                        }
                                        _gamelump.staticProps = sp_v6;
                                        break;
                                    case 7: //not tested
                                        StaticPropLumpV7_t[] sp_v7 = new StaticPropLumpV7_t[count];
                                        for (int sp = 0; sp < count; sp++)
                                        {
                                            sp_v7[sp] = new StaticPropLumpV7_t
                                            {
                                                m_Origin = BR.ReadVector(),
                                                m_Angles = BR.ReadQAngle(),
                                                m_PropType = BR.ReadUInt16(),
                                                m_FirstLeaf = BR.ReadUInt16(),
                                                m_LeafCount = BR.ReadUInt16(),
                                                m_Solid = BR.ReadByte(),
                                                m_Flags = BR.ReadByte(),
                                                m_Skin = BR.ReadInt32(),
                                                m_FadeMinDist = BR.ReadSingle(),
                                                m_FadeMaxDist = BR.ReadSingle(),
                                                m_LightingOrigin = BR.ReadVector(),
                                                m_flForcedFadeScale = BR.ReadSingle(),
                                                m_nMinDXLevel = BR.ReadUInt16(),
                                                m_nMaxDXLevel = BR.ReadUInt16(),
                                                m_DiffuseModulation = BR.ReadColor32()
                                            };
                                        }
                                        _gamelump.staticProps = sp_v7;
                                        break;
                                    case 8:
                                        StaticPropLumpV8_t[] sp_v8 = new StaticPropLumpV8_t[count];
                                        for (int sp = 0; sp < count; sp++)
                                        {
                                            sp_v8[sp] = new StaticPropLumpV8_t
                                            {
                                                m_Origin = BR.ReadVector(),
                                                m_Angles = BR.ReadQAngle(),
                                                m_PropType = BR.ReadUInt16(),
                                                m_FirstLeaf = BR.ReadUInt16(),
                                                m_LeafCount = BR.ReadUInt16(),
                                                m_Solid = BR.ReadByte(),
                                                m_Flags = BR.ReadByte(),
                                                m_Skin = BR.ReadInt32(),
                                                m_FadeMinDist = BR.ReadSingle(),
                                                m_FadeMaxDist = BR.ReadSingle(),
                                                m_LightingOrigin = BR.ReadVector(),
                                                m_flForcedFadeScale = BR.ReadSingle(),
                                                m_nMinCPULevel = BR.ReadByte(),
                                                m_nMaxCPULevel = BR.ReadByte(),
                                                m_nMinGPULevel = BR.ReadByte(),
                                                m_nMaxGPULevel = BR.ReadByte(),
                                                m_DiffuseModulation = BR.ReadColor32()
                                            };
                                        }
                                        _gamelump.staticProps = sp_v8;
                                        break;
                                    case 9:
                                        StaticPropLumpV9_t[] sp_v9 = new StaticPropLumpV9_t[count];
                                        for (int sp = 0; sp < count; sp++)
                                        {
                                            sp_v9[sp] = new StaticPropLumpV9_t
                                            {
                                                m_Origin = BR.ReadVector(),
                                                m_Angles = BR.ReadQAngle(),
                                                m_PropType = BR.ReadUInt16(),
                                                m_FirstLeaf = BR.ReadUInt16(),
                                                m_LeafCount = BR.ReadUInt16(),
                                                m_Solid = BR.ReadByte(),
                                                m_Flags = BR.ReadByte(),
                                                m_Skin = BR.ReadInt32(),
                                                m_FadeMinDist = BR.ReadSingle(),
                                                m_FadeMaxDist = BR.ReadSingle(),
                                                m_LightingOrigin = BR.ReadVector(),
                                                m_flForcedFadeScale = BR.ReadSingle(),
                                                m_nMinCPULevel = BR.ReadByte(),
                                                m_nMaxCPULevel = BR.ReadByte(),
                                                m_nMinGPULevel = BR.ReadByte(),
                                                m_nMaxGPULevel = BR.ReadByte(),
                                                m_DiffuseModulation = BR.ReadColor32(),
                                                m_bDisableX360 = BR.ReadInt32()
                                            };
                                        }
                                        _gamelump.staticProps = sp_v9;
                                        break;
                                    case 10:
                                        StaticPropLumpV10_t[] sp_v10 = new StaticPropLumpV10_t[count];
                                        for (int sp = 0; sp < count; sp++)
                                        {
                                            sp_v10[sp] = new StaticPropLumpV10_t
                                            {
                                                m_Origin = BR.ReadVector(),
                                                m_Angles = BR.ReadQAngle(),
                                                m_PropType = BR.ReadUInt16(),
                                                m_FirstLeaf = BR.ReadUInt16(),
                                                m_LeafCount = BR.ReadUInt16(),
                                                m_Solid = BR.ReadByte(),
                                                m_Flags = BR.ReadByte(),
                                                m_Skin = BR.ReadInt32(),
                                                m_FadeMinDist = BR.ReadSingle(),
                                                m_FadeMaxDist = BR.ReadSingle(),
                                                m_LightingOrigin = BR.ReadVector(),
                                                m_flForcedFadeScale = BR.ReadSingle(),
                                                m_nMinCPULevel = BR.ReadByte(),
                                                m_nMaxCPULevel = BR.ReadByte(),
                                                m_nMinGPULevel = BR.ReadByte(),
                                                m_nMaxGPULevel = BR.ReadByte(),
                                                m_DiffuseModulation = BR.ReadColor32(),
                                                m_bDisableX360 = BR.ReadInt32(),
                                                m_FlagsEx = BR.ReadUInt32()
                                            };
                                        }
                                        _gamelump.staticProps = sp_v10;
                                        break;
                                    case 11:
                                        StaticPropLumpV11_t[] sp_v11 = new StaticPropLumpV11_t[count];
                                        for (int sp = 0; sp < count; sp++)
                                        {
                                            sp_v11[sp] = new StaticPropLumpV11_t
                                            {
                                                m_Origin = BR.ReadVector(),
                                                m_Angles = BR.ReadQAngle(),
                                                m_PropType = BR.ReadUInt16(),
                                                m_FirstLeaf = BR.ReadUInt16(),
                                                m_LeafCount = BR.ReadUInt16(),
                                                m_Solid = BR.ReadByte(),
                                                m_Flags = BR.ReadByte(),
                                                m_Skin = BR.ReadInt32(),
                                                m_FadeMinDist = BR.ReadSingle(),
                                                m_FadeMaxDist = BR.ReadSingle(),
                                                m_LightingOrigin = BR.ReadVector(),
                                                m_flForcedFadeScale = BR.ReadSingle(),
                                                m_nMinCPULevel = BR.ReadByte(),
                                                m_nMaxCPULevel = BR.ReadByte(),
                                                m_nMinGPULevel = BR.ReadByte(),
                                                m_nMaxGPULevel = BR.ReadByte(),
                                                m_DiffuseModulation = BR.ReadColor32(),
                                                m_bDisableX360 = BR.ReadInt32(),
                                                m_FlagsEx = BR.ReadUInt32(),
                                                m_UniformScale = BR.ReadSingle()
                                            };
                                        }
                                        _gamelump.staticProps = sp_v11;
                                        break;
                                    default:
                                        DebugLog.Write("Unsupported static prop version: " + gamelump.version);
                                        break;
                                }
                                break;

                            case GAMELUMP_CODES.GAMELUMP_DETAIL_PROPS:
                                //read names
                                count = BR.ReadInt32();
                                _gamelump.detailPropNames = new DetailObjectDictLump_t[count];
                                for (int n = 0; n < count; n++)
                                {
                                    string name = string.Empty;

                                    for (int b = 0; b < 128; b++)
                                    {
                                        byte _c = BR.ReadByte();
                                        if (_c == (byte)'\0')
                                        {
                                            BR.BaseStream.Seek(128 - b - 1, SeekOrigin.Current);
                                            break;
                                        }
                                        name += Encoding.ASCII.GetString(new byte[1] { _c });
                                    }

                                    _gamelump.detailPropNames[n].m_Name = name;
                                }

                                if (gamelump.version == 4)
                                {
                                    if (count > 0)
                                    {
                                        //read detail prop sprites
                                        count = BR.ReadInt32();
                                        _gamelump.detailPropSprites = new DetailSpriteDictLump_t[count];
                                        for (int s = 0; s < count; s++)
                                        {
                                            _gamelump.detailPropSprites[s] = new DetailSpriteDictLump_t
                                            {
                                                m_UL = BR.ReadVector2(),
                                                m_LR = BR.ReadVector2(),
                                                m_TexUL = BR.ReadVector2(),
                                                m_TexLR = BR.ReadVector2()
                                            };
                                        }
                                        //read detail props
                                        count = BR.ReadInt32();

                                        _gamelump.detailProps = new DetailObjectLump_t[count];
                                        for (int p = 0; p < count; p++)
                                        {
                                            _gamelump.detailProps[p] = new DetailObjectLump_t
                                            {
                                                m_Origin = BR.ReadVector(),
                                                m_Angles = BR.ReadQAngle(),
                                                m_DetailModel = BR.ReadUInt16(),
                                                m_Leaf = BR.ReadUInt16(),
                                                m_Lighting = BR.ReadColorRGBExp32(),
                                                m_LightStyles = BR.ReadUInt32(),
                                                m_LightStyleCount = BR.ReadByte(),
                                                m_SwayAmount = BR.ReadByte(),
                                                m_ShapeAngle = BR.ReadByte(),
                                                m_ShapeSize = BR.ReadByte(),
                                                m_Orientation = BR.ReadByte(),
                                                m_Padding2 = BR.ReadBytes(3),
                                                m_Type = BR.ReadByte(),
                                                m_Padding3 = BR.ReadBytes(3),
                                                m_flScale = BR.ReadSingle()
                                            };
                                        }
                                    }
                                }
                                else
                                    DebugLog.Write("Unsupported detail prop version: " + gamelump.version);
                                break;

                            case GAMELUMP_CODES.GAMELUMP_DETAIL_PROP_LIGHTING:
                                // read prop lightings info
                                count = BR.ReadInt32();
                                _gamelump.detailPropLighting = new DetailPropLightstylesLump_t[count];
                                for (int p = 0; p < count; p++)
                                {
                                    _gamelump.detailPropLighting[p] = new DetailPropLightstylesLump_t
                                    {
                                        m_Lighting = BR.ReadColorRGBExp32(),
                                        m_Style = BR.ReadByte()
                                    };
                                }
                                break;

                            case GAMELUMP_CODES.GAMELUMP_DETAIL_PROP_LIGHTING_HDR:
                                // read prop lightings info
                                count = BR.ReadInt32();
                                _gamelump.detailPropLightingHDR = new DetailPropLightstylesLump_t[count];
                                for (int p = 0; p < count; p++)
                                {
                                    _gamelump.detailPropLightingHDR[p] = new DetailPropLightstylesLump_t
                                    {
                                        m_Lighting = BR.ReadColorRGBExp32(),
                                        m_Style = BR.ReadByte()
                                    };
                                }
                                break;

                            default:
                                DebugLog.Write($"Unknown game lump '{gamelump.id}' ({gamelump.id_code}) didn't get swapped!");
                                break;
                        }
                        BR.BaseStream.Seek(endofs, SeekOrigin.Begin); //for some [unknown for me] reasons we should place reader to the end of lump manually
                    }
                }
                return _gamelump;
            }
        }
        #endregion

        #region Lump #36: Water leaf data (LEAFWATERDATA)
        private dleafwaterdata_t[] _leafwaterdata;
        /// <summary>
        /// Lump #36. 
        /// Purpose: Data for leaf nodes that are inside water. 
        /// </summary>
        public dleafwaterdata_t[] LeafWaterData
        {
            get
            {
                if (_leafwaterdata == null)
                {
                    lump_t lump = SetLump(36);
                    int count = GetLumpCount(lump, 12);

                    _leafwaterdata = new dleafwaterdata_t[count];
                    for (int lw = 0; lw < count; lw++)
                    {
                        _leafwaterdata[lw] = new dleafwaterdata_t
                        {
                            surfaceZ = BR.ReadSingle(),
                            minZ = BR.ReadSingle(),
                            surfaceTexInfoID = BR.ReadInt16(),
                            unknown = BR.ReadInt16()
                        };
                    }
                }
                return _leafwaterdata;
            }
        }
        #endregion

        #region Lump #37: Primitives
        private dprimitive_t[] _primitives;
        /// <summary>
        /// Lump #37. 
        /// Purpose: "are used in reference to /non-polygonal primitives/", from Valve Dev Wiki. 
        /// </summary>
        public dprimitive_t[] Primitives
        {
            get
            {
                if (_primitives == null)
                {
                    lump_t lump = SetLump(37);
                    int count = GetLumpCount(lump, 9);

                    _primitives = new dprimitive_t[count];
                    for (int p = 0; p < count; p++)
                    {
                        _primitives[p] = new dprimitive_t
                        {
                            type = BR.ReadByte(),
                            firstIndex = BR.ReadUInt16(),
                            indexCount = BR.ReadUInt16(),
                            firstVert = BR.ReadUInt16(),
                            vertCount = BR.ReadUInt16()
                        };
                    }
                }
                return _primitives;
            }
        }
        #endregion

        #region Lump #38: Primitive vertices - not tested: can't find bsp with this lump
        private SourceVector[] _primverts;
        /// <summary>
        /// Lump #38. 
        /// Purpose: "are used in reference to /non-polygonal primitives/", from Valve Dev Wiki. 
        /// </summary>
        public SourceVector[] PrimVerts
        {
            get
            {
                if (_primverts == null)
                {
                    lump_t lump = SetLump(38);
                    int count = GetLumpCount(lump, 12);

                    _primverts = new SourceVector[count];
                    for (int p = 0; p < count; p++)
                        _primverts[p] = BR.ReadVector();
                }
                return _primverts;
            }
        }
        #endregion

        #region Lump #39: Primitive indices
        private ushort[] _primindices;
        /// <summary>
        /// Lump #39. 
        /// Purpose: "are used in reference to /non-polygonal primitives/", from Valve Dev Wiki. 
        /// </summary>
        public ushort[] Primindices
        {
            get
            {
                if (_primindices == null)
                {
                    lump_t lump = SetLump(39);
                    int count = GetLumpCount(lump, 2);
                    _primindices = BR.ReadUInt16Array(count);
                }
                return _primindices;
            }
        }
        #endregion

        #region Lump #40: PAK file
        private byte[] _pak;
        /// <summary>
        /// Lump #40. 
        /// Purpose: Contains game files for map. 
        /// Note: not really sure if i should use something else (not byte[]) there. 
        /// </summary>
        public byte[] PakFile
        {
            get
            {
                if (_pak == null)
                {
                    lump_t lump = SetLump(40);
                    _pak = ReadLump(lump.lumpNum);
                }
                return _pak;
            }
        }
        #endregion

        #region Lump #41: Clipping portal vertices
        private SourceVector[] _clipportalverts;
        /// <summary>
        /// Lump #41. 
        /// Purpose: [not sure] Used by VVIS and VRAD only.
        /// </summary>
        public SourceVector[] ClipPortalVerts
        {
            get
            {
                if (_clipportalverts == null)
                {
                    lump_t lump = SetLump(41);
                    int count = GetLumpCount(lump, 12);

                    _clipportalverts = new SourceVector[count];
                    for (int p = 0; p < count; p++)
                        _clipportalverts[p] = BR.ReadVector();
                }
                return _clipportalverts;
            }
        }
        #endregion

        #region Lump #42: Cubemaps
        private dcubemapsample_t[] _cubemaps;
        /// <summary>
        /// Lump #42. 
        /// Purpose: env_cubemap entities info.
        /// </summary>
        public dcubemapsample_t[] Cubemaps
        {
            get
            {
                if (_cubemaps == null)
                {
                    lump_t lump = SetLump(42);
                    int count = GetLumpCount(lump, 16);

                    _cubemaps = new dcubemapsample_t[count];
                    for (int p = 0; p < count; p++)
                    {
                        _cubemaps[p] = new dcubemapsample_t
                        {
                            origin = BR.ReadInt32Array(3),
                            size = BR.ReadByte(),
                            //unused = BR.ReadBytes(3)
                        };
                        BR.BaseStream.Seek(3, SeekOrigin.Current);
                    }
                }
                return _cubemaps;
            }
        }
        #endregion

        #region Lump #43/44: Texture data strings (TEXDATA_STRING_DATA / TEXDATA_STRING_TABLE)
        private string[] _texdatastring;
        /// <summary>
        /// Lump #43/44. 
        /// Purpose: Paths to VMTs of textures.
        /// </summary>
        public string[] TexdataString
        {
            get
            {
                if (_texdatastring == null)
                {
                    lump_t lump = SetLump(44);
                    int count = GetLumpCount(lump, 4);
                    int[] ofss = BR.ReadInt32Array(count);
                    _texdatastring = new string[count];

                    lump = SetLump(43);
                    for (int o = 0; o < count; o++)
                    {
                        int ofs = ofss[o];
                        BR.BaseStream.Seek(lump.fileofs + ofs, SeekOrigin.Begin);
                        string tex = string.Empty;
                        char c;
                        while ((c = (char)BR.ReadByte()) != '\0')
                            tex += c;
                        _texdatastring[o] = tex;
                    }
                }
                return _texdatastring;
            }
        }
        #endregion

        #region Lump #45: Overlays
        private doverlay_t[] _overlays;
        /// <summary>
        /// Lump #45. 
        /// Purpose: infodecal entities - overlays.
        /// </summary>
        public doverlay_t[] Overlays
        {
            get
            {
                if (_overlays == null)
                {
                    lump_t lump = SetLump(45);
                    int count = GetLumpCount(lump, 352);

                    _overlays = new doverlay_t[count];
                    for (int p = 0; p < count; p++)
                    {
                        _overlays[p] = new doverlay_t
                        {
                            nId = BR.ReadInt32(),
                            nTexInfo = BR.ReadInt16(),
                            m_nFaceCountAndRenderOrder = BR.ReadUInt16(),
                            aFaces = BR.ReadInt32Array(OVERLAY_BSP_FACE_COUNT),
                            flU = BR.ReadSingleArray(2),
                            flV = BR.ReadSingleArray(2),
                            vecUVPoints = new SourceVector[4] { BR.ReadVector(), BR.ReadVector(), BR.ReadVector(), BR.ReadVector() },
                            vecOrigin = BR.ReadVector(),
                            vecBasisNormal = BR.ReadVector()
                        };
                    }
                }
                return _overlays;
            }
        }
        #endregion

        #region Lump #46: Minimal distance to water from leafs (LEAFMINDISTTOWATER)
        private ushort[] _leafmindisttowater;
        /// <summary>
        /// Lump #46. 
        /// Purpose: Distance from leaf to water. 
        /// </summary>
        public ushort[] LeafMinDistToWater
        {
            get
            {
                if (_leafmindisttowater == null)
                {
                    lump_t lump = SetLump(46);
                    int count = GetLumpCount(lump, 2);
                    _leafmindisttowater = BR.ReadUInt16Array(count);
                }
                return _leafmindisttowater;
            }
        }
        #endregion

        #region Lump #47: Face macro texinfo (FACE_MACRO_TEXTURE_INFO)
        private ushort[] _facemacrotexinfo;
        /// <summary>
        /// Lump #47. 
        /// Purpose: Macro texture info for faces. 
        /// This looks up into g_TexDataStringTable, which looks up into g_TexDataStringData. 
        /// 0xFFFF if the face has no macro texture.
        /// </summary>
        public ushort[] FaceMacroTextureInfo
        {
            get
            {
                if (_facemacrotexinfo == null)
                {
                    lump_t lump = SetLump(47);
                    int count = GetLumpCount(lump, 2);
                    _facemacrotexinfo = BR.ReadUInt16Array(count);
                }
                return _facemacrotexinfo;
            }
        }
        #endregion

        #region Lump #48: Displacement triangles (DISP_TRIS)
        private CDispTri[] _disptris;
        /// <summary>
        /// Lump #48. 
        /// Purpose: Displacement triangle tags. 
        /// </summary>
        public CDispTri[] DispTris
        {
            get
            {
                if (_disptris == null)
                {
                    lump_t lump = SetLump(48);
                    int count = GetLumpCount(lump, 2);
                    _disptris = new CDispTri[count];
                    for (int d = 0; d < count; d++)
                    {
                        _disptris[d] = new CDispTri
                        {
                            m_uiTags = BR.ReadUInt16()
                        };
                    }
                }
                return _disptris;
            }
        }
        #endregion

        #region Lump #49 : 2004/2009 - not implemented: can't find structs for this lump
        #region Lump #49/1: Compressed physics collision data (PHYSCOLLIDESURFACE) [Source 2004]
        private object _physcollidesurface;
        /// <summary>
        /// Lump #49. 
        /// Purpose: "deprecated. We no longer use win32-specific havok compression on terrain", from Source 2013 code. 
        /// </summary>
        public object PhysCollideSurface
        {
            get
            {
                if (_physcollidesurface == null)
                {
                    //lump_t lump = SetLump(49);
                    //int count = GetLumpCount(lump, 2);
                    //_physcollidesurface = new ...[count];
                    throw new NotImplementedException();
                }
                return _physcollidesurface;
            }
        }
        #endregion
        #region Lump #49/2: (PROP_BLOB) [Source 2009]
        private object _propblob;
        /// <summary>
        /// Lump #49. 
        /// Purpose: "static prop triangle & string data", from Source 20?? code. 
        /// </summary>
        public object PropBlob
        {
            get
            {
                if (_propblob == null)
                {
                    //lump_t lump = SetLump(49);
                    //int count = GetLumpCount(lump, 2);
                    //_propblob = new ...[count];
                    throw new NotImplementedException();
                }
                return _propblob;
            }
        }
        #endregion
        #endregion

        #region Lump #50: Water overlays - not tested: can't find map with this lump
        private dwateroverlay_t[] _wateroverlays;
        /// <summary>
        /// Lump #50. 
        /// Purpose: [unknown]. 
        /// </summary>
        public dwateroverlay_t[] WaterOverlays
        {
            get
            {
                if (_wateroverlays == null)
                {
                    lump_t lump = SetLump(50);
                    int count = GetLumpCount(lump, 1120);
                    _wateroverlays = new dwateroverlay_t[count];
                    for (int d = 0; d < count; d++)
                    {
                        _wateroverlays[d] = new dwateroverlay_t
                        {
                            nId = BR.ReadInt32(),
                            nTexInfo = BR.ReadInt16(),
                            m_nFaceCountAndRenderOrder = BR.ReadUInt16(),
                            aFaces = BR.ReadInt32Array(WATEROVERLAY_BSP_FACE_COUNT),
                            flU = BR.ReadSingleArray(2),
                            flV = BR.ReadSingleArray(2),
                            vecUVPoints = new SourceVector[4] { BR.ReadVector(), BR.ReadVector(), BR.ReadVector(), BR.ReadVector() },
                            vecOrigin = BR.ReadVector(),
                            vecBasisNormal = BR.ReadVector()
                        };
                    }
                }
                return _wateroverlays;
            }
        }
        #endregion

        #region Lump #51 XBOX2006/2007
        #region Lump #51/1: Lightmap pages (LIGHTMAPPAGES) [Source 2006 for XBOX] - not tested: can't find map with this lump
        private dlightmappage_t[] _lightmappages;
        /// <summary>
        /// Lump #51. 
        /// Purpose: Lightmaps for XBOX. 
        /// </summary>
        public dlightmappage_t[] LightmapPages
        {
            get
            {
                if (_lightmappages == null)
                {
                    lump_t lump = SetLump(51);
                    if (lump.filelen < 33792) //This lump is not for lightmap pages
                        return null;
                    int count = GetLumpCount(lump, 33792);
                    _lightmappages = new dlightmappage_t[count];
                    for (int d = 0; d < count; d++)
                    {
                        _lightmappages[d] = new dlightmappage_t
                        {
                            data = BR.ReadBytes(MAX_LIGHTMAPPAGE_WIDTH * MAX_LIGHTMAPPAGE_HEIGHT)
                        };
                        _lightmappages[d].palette = new ColorRGBExp32[256];
                        for (int p = 0; p < 256; p++)
                            _lightmappages[d].palette[p] = BR.ReadColorRGBExp32();
                    }
                }
                return _lightmappages;
            }
        }
        #endregion
        #region Lump #51/2: (LEAF_AMBIENT_INDEX_HDR) [Source 2007]
        private dleafambientindex_t[] _leafambientindexhdr;
        /// <summary>
        /// Lump #51. 
        /// Purpose: "index of LUMP_LEAF_AMBIENT_LIGHTING_HDR", from Source 2013 code. 
        /// </summary>
        public dleafambientindex_t[] LeafAmbientIndexHDR
        {
            get
            {
                if (_leafambientindexhdr == null)
                {
                    lump_t lump = SetLump(51);
                    if (lump.filelen == 0 || lump.filelen % 4 != 0) //This lump is not for ... this
                        return null;
                    int count = GetLumpCount(lump, 4);
                    _leafambientindexhdr = new dleafambientindex_t[count];
                    for (int d = 0; d < count; d++)
                    {
                        _leafambientindexhdr[d] = new dleafambientindex_t
                        {
                            ambientSampleCount = BR.ReadUInt16(),
                            firstAmbientSample = BR.ReadUInt16()
                        };
                    }
                }
                return _leafambientindexhdr;
            }
        }
        #endregion
        #endregion

        #region Lump #52 XBOX2006/2007
        #region Lump #52/1: Lightmap page infos (LIGHTMAPPAGEINFOS) [Source 2006 for XBOX] - not tested: can't find map with this lump
        private dlightmappageinfo_t[] _lightmappageinfos;
        /// <summary>
        /// Lump #52. 
        /// Purpose: Lightmaps for XBOX. 
        /// </summary>
        public dlightmappageinfo_t[] LightmapPageInfos
        {
            get
            {
                if (_lightmappageinfos == null)
                {
                    lump_t lump = SetLump(52);
                    if (lump.filelen == 0 || lump.filelen % 8 != 0) //This lump is not for lightmap page infos
                        return null;
                    int count = GetLumpCount(lump, 8);
                    _lightmappageinfos = new dlightmappageinfo_t[count];
                    for (int d = 0; d < count; d++)
                    {
                        _lightmappageinfos[d] = new dlightmappageinfo_t
                        {
                            page = BR.ReadByte(),
                            offset = BR.ReadBytes(2),
                            pad = BR.ReadByte(),
                            avgColor = BR.ReadColorRGBExp32()
                        };
                    }
                }
                return _lightmappageinfos;
            }
        }
        #endregion
        #region Lump #52/2: (LEAF_AMBIENT_INDEX) [Source 2007]
        private dleafambientindex_t[] _leafambientindex;
        /// <summary>
        /// Lump #52. 
        /// Purpose: "index of LUMP_LEAF_AMBIENT_LIGHTING", from Source 2013 code. 
        /// </summary>
        public dleafambientindex_t[] LeafAmbientIndex
        {
            get
            {
                if (_leafambientindex == null)
                {
                    lump_t lump = SetLump(52);
                    if (lump.filelen == 0 || lump.filelen % 4 != 0) //This lump is not for ... this
                        return null;
                    int count = GetLumpCount(lump, 4);
                    _leafambientindex = new dleafambientindex_t[count];
                    for (int d = 0; d < count; d++)
                    {
                        _leafambientindex[d] = new dleafambientindex_t
                        {
                            ambientSampleCount = BR.ReadUInt16(),
                            firstAmbientSample = BR.ReadUInt16()
                        };
                    }
                }
                return _leafambientindex;
            }
        }
        #endregion
        #endregion

        #region Lump #53: Lighting (HDR)
        private ColorRGBExp32[] _lightinghdr;
        /// <summary>
        /// Lump #53. Version 1.
        /// Purpose: lightmap colors / shadows.
        /// Also: Check lump #8 for LDR lighting.
        /// </summary>
        public ColorRGBExp32[] LightingHDR
        {
            get
            {
                if (_lightinghdr == null)
                {
                    lump_t lump = SetLump(53);
                    int count = GetLumpCount(lump, 4);
                    _lightinghdr = new ColorRGBExp32[count];
                    for (int f = 0; f < count; f++)
                    {
                        _lightinghdr[f] = BR.ReadColorRGBExp32();
                    }
                }
                return _lightinghdr;
            }
        }
        #endregion

        #region Lump #54: World lights (HDR)
        private object _worldlightshdr;
        /// <summary>
        /// Lump #54. Version 0/1. 
        /// Purpose: Contains info about static lights (NOT LIGHTMAPS). 
        /// Usage (depending on version): (dworldlightV?_t[])bsp.WorldLightsHDR. 
        /// Also: Check lump #15 for LDR world lights.
        /// </summary>
        public object WorldLightsHDR
        {
            get
            {
                if (_worldlightshdr == null)
                {
                    lump_t lump = SetLump(54);
                    int count;
                    switch (lump.version)
                    {
                        case 0:
                            count = GetLumpCount(lump, 88);
                            dworldlightV0_t[] _wl_0 = new dworldlightV0_t[count];
                            for (int wl = 0; wl < count; wl++)
                            {
                                _wl_0[wl] = new dworldlightV0_t
                                {
                                    origin = BR.ReadVector(),
                                    intensity = BR.ReadVector(),
                                    normal = BR.ReadVector(),
                                    cluster = BR.ReadInt32(),
                                    type = (emittype_t)BR.ReadUInt32(),
                                    style = BR.ReadInt32(),
                                    stopdot = BR.ReadSingle(),
                                    stopdot2 = BR.ReadSingle(),
                                    exponent = BR.ReadSingle(),
                                    radius = BR.ReadSingle(),
                                    constant_attn = BR.ReadSingle(),
                                    linear_attn = BR.ReadSingle(),
                                    quadratic_attn = BR.ReadSingle(),
                                    flags = BR.ReadInt32(),
                                    texinfo = BR.ReadInt32(),
                                    owner = BR.ReadInt32()
                                };
                            }
                            _worldlightshdr = _wl_0;
                            break;
                        case 1:
                            count = GetLumpCount(lump, 100);
                            dworldlightV1_t[] _wl_1 = new dworldlightV1_t[count];
                            for (int wl = 0; wl < count; wl++)
                            {
                                _wl_1[wl] = new dworldlightV1_t
                                {
                                    origin = BR.ReadVector(),
                                    intensity = BR.ReadVector(),
                                    normal = BR.ReadVector(),
                                    shadow_cast_offset = BR.ReadVector(),
                                    cluster = BR.ReadInt32(),
                                    type = (emittype_t)BR.ReadUInt32(),
                                    style = BR.ReadInt32(),
                                    stopdot = BR.ReadSingle(),
                                    stopdot2 = BR.ReadSingle(),
                                    exponent = BR.ReadSingle(),
                                    radius = BR.ReadSingle(),
                                    constant_attn = BR.ReadSingle(),
                                    linear_attn = BR.ReadSingle(),
                                    quadratic_attn = BR.ReadSingle(),
                                    flags = BR.ReadInt32(),
                                    texinfo = BR.ReadInt32(),
                                    owner = BR.ReadInt32()
                                };
                            }
                            _worldlightshdr = _wl_1;
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                }
                return _worldlightshdr;
            }
        }
        #endregion

        #region Lump #55: Per-leaf ambient lighting (HDR) (LEAF_AMBIENT_LIGHTING_HDR)
        private dleafambientlighting_t[] _leafambientlightinghdr;
        /// <summary>
        /// Lump #55. 
        /// Purpose: Stores volumetric ambient lighting information for each leaf. 
        /// Also: Check lump #56 for LDR ambient lighting.
        /// </summary>
        public dleafambientlighting_t[] LeafAmbientLightingHDR
        {
            get
            {
                if (_leafambientlightinghdr == null)
                {
                    lump_t lump = SetLump(55);
                    int count = GetLumpCount(lump, 28);
                    _leafambientlightinghdr = new dleafambientlighting_t[count];
                    for (int l = 0; l < count; l++)
                    {
                        _leafambientlightinghdr[l] = new dleafambientlighting_t
                        {
                            cube = BR.ReadCompressedLightCube(),
                            x = BR.ReadByte(),
                            y = BR.ReadByte(),
                            z = BR.ReadByte(),
                            pad = BR.ReadByte()
                        };
                    }
                }
                return _leafambientlightinghdr;
            }
        }
        #endregion

        #region Lump #56: Per-leaf ambient lighting (LDR) (LEAF_AMBIENT_LIGHTING)
        private dleafambientlighting_t[] _leafambientlighting;
        /// <summary>
        /// Lump #56. 
        /// Purpose: Stores volumetric ambient lighting information for each leaf. 
        /// Also: Check lump #55 for HDR ambient lighting.
        /// </summary>
        public dleafambientlighting_t[] LeafAmbientLighting
        {
            get
            {
                if (_leafambientlighting == null)
                {
                    lump_t lump = SetLump(56);
                    int count = GetLumpCount(lump, 28);
                    _leafambientlighting = new dleafambientlighting_t[count];
                    for (int l = 0; l < count; l++)
                    {
                        _leafambientlighting[l] = new dleafambientlighting_t
                        {
                            cube = BR.ReadCompressedLightCube(),
                            x = BR.ReadByte(),
                            y = BR.ReadByte(),
                            z = BR.ReadByte(),
                            pad = BR.ReadByte()
                        };
                    }
                }
                return _leafambientlighting;
            }
        }
        #endregion

        #region Lump #57: XZIP PAK file (for XBOX) - not implemented: can't find structs for this lump (mb it's a byte array)
        private object _xzippakfile;
        /// <summary>
        /// Lump #57. 
        /// Purpose: "XZip version of pak file for Xbox. Deprecated.", from Valve Dev Wiki. 
        /// </summary>
        public object XZIPPAKFile
        {
            get
            {
                if (_xzippakfile == null)
                {
                    //lump_t lump = SetLump(57);
                    //int count = GetLumpCount(lump, 28);
                    //_xzippakfile = new ...[count];
                    throw new NotImplementedException();
                }
                return _xzippakfile;
            }
        }
        #endregion

        #region Lump #58: Faces (HDR)
        private dface_t[] _faceshdr;
        /// <summary>
        /// Lump #58. Version 1.
        /// Purpose: datas for rendering.
        /// Also: Check lump #7 for LDR faces.
        /// </summary>
        public dface_t[] FacesHDR
        {
            get
            {
                if (_faceshdr == null)
                {
                    lump_t lump = SetLump(58);
                    int count = GetLumpCount(lump, 56);
                    _faceshdr = new dface_t[count];
                    for (int f = 0; f < count; f++)
                    {
                        _faceshdr[f] = new dface_t
                        {
                            planenum = BR.ReadUInt16(),
                            side = BR.ReadByte(),
                            onNode = BR.ReadByte(),
                            firstedge = BR.ReadInt32(),
                            numedges = BR.ReadInt16(),
                            texinfo = BR.ReadInt16(),
                            dispinfo = BR.ReadInt16(),
                            surfaceFogVolumeID = BR.ReadInt16(),
                            styles = BR.ReadBytes(4),
                            lightofs = BR.ReadInt32(),
                            area = BR.ReadSingle(),
                            m_LightmapTextureMinsInLuxels = BR.ReadVector2Int(),
                            m_LightmapTextureSizeInLuxels = BR.ReadVector2Int(),
                            origFace = BR.ReadInt32(),
                            m_NumPrims = BR.ReadUInt16(),
                            firstPrimID = BR.ReadUInt16(),
                            smoothingGroups = BR.ReadUInt32()
                        };
                    }
                }
                return _faceshdr;
            }
        }
        #endregion

        #region Lump #59: Map flags
        private dflagslump_t _mapflags;
        private bool __mapflags;
        /// <summary>
        /// Lump #59. 
        /// Purpose: Map compilation flags.
        /// </summary>
        public dflagslump_t MapFlags
        {
            get
            {
                if (!__mapflags)
                {
                    lump_t lump = SetLump(59);
                    _mapflags = new dflagslump_t
                    {
                        m_LevelFlags = BR.ReadUInt32()
                    };

                    __mapflags = true;
                }
                return _mapflags;
            }
        }
        #endregion

        #region Lump #60: Overlay fades
        private doverlayfade_t[] _overlayfades;
        /// <summary>
        /// Lump #60. 
        /// Purpose: [not sure] fade distances for overlays?
        /// </summary>
        public doverlayfade_t[] OverlayFades
        {
            get
            {
                if (_overlayfades == null)
                {
                    lump_t lump = SetLump(60);
                    int count = GetLumpCount(lump, 8);
                    _overlayfades = new doverlayfade_t[count];
                    for (int of = 0; of < count; of++)
                    {
                        _overlayfades[of] = new doverlayfade_t
                        {
                            flFadeDistMinSq = BR.ReadSingle(),
                            flFadeDistMaxSq = BR.ReadSingle()
                        };
                    }
                }
                return _overlayfades;
            }
        }
        #endregion

        //not gonna try to do 61-63 lumps because i can't find any structs in google/github. Valve's secret?

        #endregion
    }
}

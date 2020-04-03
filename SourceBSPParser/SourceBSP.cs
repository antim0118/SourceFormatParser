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
            if (c != lump.filelen / (float)byteSize)
            {
                //trying to find nearest byteSizes
                int nearMin=-1, nearMax=-1;
                for(int s = 1; s < 100; s++)
                {
                    if (byteSize - s <= 0) break;
                    if (lump.filelen / (byteSize - s) == lump.filelen / (float)(byteSize - s))
                    {
                        nearMin = byteSize - s;
                        break;
                    }
                }
                for (int s = 1; s < 100; s++)
                {
                    if (lump.filelen / (byteSize + s) == lump.filelen / (float)(byteSize + s))
                    {
                        nearMax = byteSize + s;
                        break;
                    }
                }
                string debug_text = $"[Lump #{lump.lumpNum}:v{lump.version}] Wrong count" + (nearMin != -1 || nearMax != -1 ? $" ({nearMin}; {nearMax})" : "");
#if UNITY
                Debug.Log(debug_text);
#else
                Console.WriteLine(debug_text);
#endif
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

        #region Lump #7: Faces
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

        #region Lump #15: World lights
        private object _worldlights;
        /// <summary>
        /// Lump #15. Version 0/1. 
        /// Purpose: Contains info about static lights (NOT LIGHTMAPS). 
        /// Usage (depending on version): (dworldlightV?_t)bsp.WorldLights
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

        #endregion
    }
}

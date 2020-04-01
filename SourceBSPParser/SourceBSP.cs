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

        /// <summary>
        /// assigned by parser
        /// </summary>
        public SourceGame Game;
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
                        filelen = BR.ReadInt32(),
                        fileofs = BR.ReadInt32(),
                        version = BR.ReadInt32(),
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
                string debug_text = $"[Lump #{lump.lumpNum}] Wrong count" + (nearMin != -1 || nearMax != -1 ? $" ({nearMin}; {nearMax})" : "");
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

        #endregion
    }
}

using System;
using System.IO;
using System.Text;
using Structs = SourceFormatParser.BSP.SourceBSPStructs;

namespace SourceFormatParser.BSP
{
    /// <summary>
    /// SourceBSP is BSP which version is more than 17.
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
            //valve games
            TeamFortress2,
            Left4Dead,
            Left4Dead2,
            Portal2,
            CounterStrikeGlobalOffensive,
            Dota2,

            //idk
            ZenoClash,
            DarkMessiah,
            Vindictus,
            TheShip,
            BloodyGoodTime,
            BlackMesa,
            AlienSwarm,
            DearEsther,
            Titanfall
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
            if (id != Structs.IDBSPHEADER)
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
                lumps[l] = new Structs.lump_t
                {
                    fileofs = BR.ReadInt32(),
                    filelen = BR.ReadInt32(),
                    version = BR.ReadInt32(),
                    uncompressedSize = BR.ReadInt32()
                };
            return lumps;
        }
        #endregion
    }
}

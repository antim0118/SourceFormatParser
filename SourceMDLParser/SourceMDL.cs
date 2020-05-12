using SourceFormatParser.BigEndian;
using SourceFormatParser.Common;
using System;
using System.IO;
using System.Text;
using static SourceFormatParser.MDL.SourceMDLStructs;

namespace SourceFormatParser.MDL
{
    /// <summary>
    /// Supported version: ? / 
    /// Tested on versions: 44-49
    /// </summary>
    public class SourceMDL : IDisposable
    {
        #region Public variables
        public studiohdr_t Header;
        public studiohdr2_t Header2;

        public studiohdrFlags[] getFlags => Header.getFlags;
        #endregion
        #region Private variables
        BinaryReader BR;
        Stream stream;
        #endregion

        #region Init / Dispose
        public SourceMDL(string path)
        {
            stream = File.OpenRead(path);
            if (path.EndsWith(".ps3.mdl"))
                Init(stream, true);
            else
                Init(stream, false);
        }
        public SourceMDL(Stream stream, bool isPS3 = false) => Init(stream, isPS3);

        void Init(Stream stream, bool isPS3 = false)
        {
            if (isPS3)
            {
                BR = new BigBinaryReader(stream, Encoding.ASCII);
                throw new NotImplementedException("PS3 format is not supported: LZMA decoder needed");
            }
            else
                BR = new BinaryReader(stream, Encoding.ASCII);
            if (BR.BaseStream.Length < 408)
                throw new Exception("Not an MDL file! (size less than main header)");
            ParseHeader();
            ParseHeader2();
        }

        public void Dispose()
        {
            Header = new studiohdr_t();
            Header2 = new studiohdr2_t();
            BR.Dispose();
            if (stream != null)
                stream.Dispose();
        }
        #endregion

        #region Private Methods - Headers
        void ParseHeader()
        {
            //BR.BaseStream.Seek(0, SeekOrigin.Begin);
            //string c = Encoding.ASCII.GetString(BR.ReadBytes(4));
            BR.BaseStream.Seek(0, SeekOrigin.Begin);
            Header = new studiohdr_t();
            Header.id = BR.ReadInt32();
            if (Header.id != IDMDLHEADER)
                throw new Exception("Not an MDL file! (wrong id)");

            Header.version = BR.ReadInt32();
            Header.checksum = BR.ReadInt32();
            Header.name = BR.ReadString(64);
            Header.length = BR.ReadInt32();

            Header.eyeposition = BR.ReadVector();
            Header.illumposition = BR.ReadVector();
            Header.hull_min = BR.ReadVector();
            Header.hull_max = BR.ReadVector();
            Header.view_bbmin = BR.ReadVector();
            Header.view_bbmax = BR.ReadVector();

            Header.flags = BR.ReadInt32();
            Header.numbones = BR.ReadInt32();
            Header.boneindex = BR.ReadInt32();
            Header.numbonecontrollers = BR.ReadInt32();
            Header.bonecontrollerindex = BR.ReadInt32();
            Header.numhitboxsets = BR.ReadInt32();
            Header.hitboxsetindex = BR.ReadInt32();
            Header.numlocalanim = BR.ReadInt32();
            Header.localanimindex = BR.ReadInt32();
            Header.numlocalseq = BR.ReadInt32();
            Header.localseqindex = BR.ReadInt32();
            Header.activitylistversion = BR.ReadInt32();
            Header.eventsindexed = BR.ReadInt32();
            Header.numtextures = BR.ReadInt32();
            Header.textureindex = BR.ReadInt32();
            Header.numcdtextures = BR.ReadInt32();
            Header.cdtextureindex = BR.ReadInt32();
            Header.numskinref = BR.ReadInt32();
            Header.numskinfamilies = BR.ReadInt32();
            Header.skinindex = BR.ReadInt32();
            Header.numbodyparts = BR.ReadInt32();
            Header.bodypartindex = BR.ReadInt32();
            Header.numlocalattachments = BR.ReadInt32();
            Header.localattachmentindex = BR.ReadInt32();
            Header.numlocalnodes = BR.ReadInt32();
            Header.localnodeindex = BR.ReadInt32();
            Header.localnodenameindex = BR.ReadInt32();
            Header.numflexdesc = BR.ReadInt32();
            Header.flexdescindex = BR.ReadInt32();
            Header.numflexcontrollers = BR.ReadInt32();
            Header.flexcontrollerindex = BR.ReadInt32();
            Header.numflexrules = BR.ReadInt32();
            Header.flexruleindex = BR.ReadInt32();
            Header.numikchains = BR.ReadInt32();
            Header.ikchainindex = BR.ReadInt32();
            Header.nummouths = BR.ReadInt32();
            Header.mouthindex = BR.ReadInt32();
            Header.numlocalposeparameters = BR.ReadInt32();
            Header.localposeparamindex = BR.ReadInt32();
            Header.surfacepropindex = BR.ReadInt32();
            Header.keyvalueindex = BR.ReadInt32();
            Header.keyvaluesize = BR.ReadInt32();
            Header.numlocalikautoplaylocks = BR.ReadInt32();
            Header.localikautoplaylockindex = BR.ReadInt32();

            Header.mass = BR.ReadSingle();
            Header.contents = BR.ReadInt32();
            Header.numincludemodels = BR.ReadInt32();
            Header.includemodelindex = BR.ReadInt32();
            //Header.unused_virtualModel = BR.ReadInt32();
            BR.BaseStream.Seek(4, SeekOrigin.Current);
            Header.szanimblocknameindex = BR.ReadInt32();
            Header.numanimblocks = BR.ReadInt32();
            Header.animblockindex = BR.ReadInt32();
            Header.unused_animblockModel = BR.ReadInt32();
            Header.bonetablebynameindex = BR.ReadInt32();
            //Header.unused_pVertexBase = BR.ReadInt32();
            //Header.unused_pIndexBase = BR.ReadInt32();
            BR.BaseStream.Seek(8, SeekOrigin.Current);

            Header.constdirectionallightdot = BR.ReadByte();
            Header.rootLOD = BR.ReadByte();
            Header.numAllowedRootLODs = BR.ReadByte();
            //Header.unused = BR.ReadByte();
            //Header.unused4 = BR.ReadInt32();
            BR.BaseStream.Seek(5, SeekOrigin.Current);

            Header.numflexcontrollerui = BR.ReadInt32();
            Header.flexcontrolleruiindex = BR.ReadInt32();

            Header.flVertAnimFixedPointScale = BR.ReadSingle();
            Header.surfacepropLookup = BR.ReadInt32();
            Header.studiohdr2index = BR.ReadInt32();
            //Header.unused2 = BR.ReadInt32();
            BR.BaseStream.Seek(4, SeekOrigin.Current);
        }

        void ParseHeader2()
        {
            if (Header.studiohdr2index == 0) return;
            BR.BaseStream.Seek(Header.studiohdr2index, SeekOrigin.Begin);
            Header2 = new studiohdr2_t();

            Header2.numsrcbonetransform = BR.ReadInt32();
            Header2.srcbonetransformindex = BR.ReadInt32();
            Header2.illumpositionattachmentindex = BR.ReadInt32();
            Header2.flMaxEyeDeflection = BR.ReadSingle();
            Header2.linearboneindex = BR.ReadInt32();
            Header2.sznameindex = BR.ReadInt32();
            Header2.m_nBoneFlexDriverCount = BR.ReadInt32();
            Header2.m_nBoneFlexDriverIndex = BR.ReadInt32();
            Header2.m_nBodyGroupPresetCount = BR.ReadInt32();
            Header2.m_nBodyGroupPresetIndex = BR.ReadInt32();
            Header2.padding_unused = BR.ReadInt32();
            //Header2.reserved = BR.ReadInt32Array(44);
            BR.BaseStream.Seek(176, SeekOrigin.Current);
        }
        #endregion

        #region Public Methods - Sectors parsers
        public mstudiobone_t[] Bones
        {
            get
            {
                if (Header.pBone == null)
                {
                    Header.pBone = new mstudiobone_t[Header.numbones];
                    StringBuilder sb;
                    char c;
                    for (int b = 0; b < Header.numbones; b++)
                    {
                        long ofs = Header.boneindex + b * 216;
                        BR.BaseStream.Seek(ofs, SeekOrigin.Begin);
                        mstudiobone_t bone = new mstudiobone_t
                        {
                            sznameindex = BR.ReadInt32(),
                            parent = BR.ReadInt32(),
                            bonecontroller = BR.ReadInt32Array(6),

                            pos = BR.ReadVector(),
                            quat = BR.ReadQuaternion(),
                            rot = BR.ReadRadianEuler(),

                            posscale = BR.ReadVector(),
                            rotscale = BR.ReadVector(),

                            poseToBone = BR.ReadMatrix3x4(),
                            qAlignment = BR.ReadQuaternion(),
                            flags = BR.ReadInt32(),
                            proctype = BR.ReadInt32(),
                            procindex = BR.ReadInt32(),
                            physicsbone = BR.ReadInt32(),
                            surfacepropidx = BR.ReadInt32(),
                            contents = BR.ReadInt32(),
                            surfacepropLookup = BR.ReadInt32(),
                            //unused = BR.ReadInt32Array(7)
                        };
                        //BR.BaseStream.Seek(28, SeekOrigin.Current); //not needed because we seek it

                        //read pszName
                        BR.BaseStream.Seek(ofs + bone.sznameindex, SeekOrigin.Begin);
                        sb = new StringBuilder();
                        while ((c = (char)BR.ReadByte()) != '\0')
                            sb.Append(c);
                        bone.pszName = sb.ToString();

                        //read pszName
                        BR.BaseStream.Seek(ofs + bone.surfacepropidx, SeekOrigin.Begin);
                        sb = new StringBuilder();
                        while ((c = (char)BR.ReadByte()) != '\0')
                            sb.Append(c);
                        bone.pszSurfaceProp = sb.ToString();

                        Header.pBone[b] = bone;
                    }
                }
                return Header.pBone;
            }
        }

        public mstudiobonecontroller_t[] BoneControllers
        {
            get
            {
                if (Header.pBonecontroller == null)
                {
                    Header.pBonecontroller = new mstudiobonecontroller_t[Header.numbonecontrollers];
                    for (int bc = 0; bc < Header.numbonecontrollers; bc++)
                    {
                        long ofs = Header.bonecontrollerindex + bc * 56;
                        BR.BaseStream.Seek(ofs, SeekOrigin.Begin);
                        mstudiobonecontroller_t bonecontroller = new mstudiobonecontroller_t
                        {
                            bone = BR.ReadInt32(),
                            type = BR.ReadInt32(),
                            start = BR.ReadSingle(),
                            end = BR.ReadSingle(),
                            rest = BR.ReadInt32(),
                            inputfield = BR.ReadInt32(),
                            //unused = BR.ReadInt32Array(8)
                        };
                        //BR.BaseStream.Seek(32, SeekOrigin.Current); //not needed because we seek it

                        Header.pBonecontroller[bc] = bonecontroller;
                    }
                }
                return Header.pBonecontroller;
            }
        }
        #endregion
    }
}

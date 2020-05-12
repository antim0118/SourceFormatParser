using SourceFormatParser.Common;
using System;
using System.IO;
using System.Text;
using Structs = SourceFormatParser.VHV.SourceVHVStructs;

namespace SourceFormatParser.VHV
{
    /// <summary>
    /// "valve hardware vertexes", from Source 2013 code.
    /// VERTEX_COLOR is not supported (used in bsp when g_numVradStaticPropsLightingStreams <= 1; looks like very rarely? This variable equals 3 in CSGO by default)
    /// </summary>
    public class SourceVHV
    {
        #region Public variables
        public Structs.FileHeader_t Header;
        #endregion
        #region Private variables
        #endregion

        #region Init / Dispose
        public SourceVHV(string path)
        {
            using (FileStream stream = File.OpenRead(path))
                Init(stream);
        }
        public SourceVHV(Stream stream) => Init(stream);

        void Init(Stream stream)
        {
            using (BinaryReader BR = new BinaryReader(stream, Encoding.ASCII))
                ParseHeader(BR);
        }
        #endregion

        #region Private Methods
        void ParseHeader(BinaryReader BR)
        {
            //used utils/vrad/vradstaticprops.cpp:1506 -- void CVradStaticPropMgr::SerializeLighting()
            if (BR.BaseStream.Length < 40)
                throw new Exception("Not a VHV file! (file size is less than header size)");
            //parsing main header
            BR.BaseStream.Seek(0, SeekOrigin.Begin);
            Header = new Structs.FileHeader_t
            {
                m_nVersion = BR.ReadInt32(),
                m_nChecksum = BR.ReadUInt32(),
                m_nVertexFlags = (Structs.VertexFlags)BR.ReadUInt32(),
                m_nVertexSize = BR.ReadUInt32(),
                m_nVertexes = BR.ReadUInt32(),
                m_nMeshes = BR.ReadInt32()
            };
            //int[] unused = BR.ReadInt32Array(4);
            BR.BaseStream.Seek(16, SeekOrigin.Current); //skip unused
            if (Header.m_nVersion != Structs.VHV_VERSION)
                throw new Exception($"Not supported version or not a VHV file! ({Header.m_nVersion}!={Structs.VHV_VERSION})");

            //parsing mesh headers
            Header.pMesh = new Structs.MeshHeader_t[Header.m_nMeshes];
            for (int n = 0; n < Header.m_nMeshes; n++)
            {
                // construct mesh dictionary
                Header.pMesh[n] = new Structs.MeshHeader_t
                {
                    m_nLod = BR.ReadUInt32(),
                    m_nVertexes = BR.ReadUInt32(),
                    m_nOffset = BR.ReadUInt32()
                };
                BR.BaseStream.Seek(16, SeekOrigin.Current); //skip unused
            }
            //BR.BaseStream.Seek(4 * 109, SeekOrigin.Current);
            for (int n = 0; n < Header.m_nMeshes; n++)
            {
                // construct vertexes
                var pMesh = Header.pMesh[n];
                BR.BaseStream.Seek(pMesh.m_nOffset, SeekOrigin.Begin);
                ColorRGBExp32[] m_VertexColors = new ColorRGBExp32[pMesh.m_nVertexes];
                long POSITION_THERE = BR.BaseStream.Position;
                long BYTES_LEFT = BR.BaseStream.Length - POSITION_THERE;
                for (int k = 0; k < pMesh.m_nVertexes; k++)
                {
                    //Vector vertexColor = m_StaticProps[i].m_MeshData[n].m_VertexColors[k];

                    //ColorRGBExp32 rgbColor;
                    //VectorToColorRGBExp32(vertexColor, rgbColor);
                    //byte[] dstColor;//[4];
                    //ConvertRGBExp32ToRGBA8888(rgbColor, dstColor);

                    // b,g,r,a order
                    //pVertexData[0] = dstColor[2];
                    //pVertexData[1] = dstColor[1];
                    //pVertexData[2] = dstColor[0];
                    //pVertexData[3] = dstColor[3];
                    //pVertexData += 4;

                    //RGBA8888 -> RGBExp32
                    // Linear to Tex light [0-1 to 0-255] (mb to gamma?)

                    //RGBExp32 -> Vector

                    //if (Header.m_nVertexFlags.HasFlag(Structs.VertexFlags.VERTEX_COLOR))
                    //{
                    //    //byte[] colordata = BR.ReadBytes(4);
                    //    byte[] colordata = BR.ReadBytes(3);
                    //    //0 1 2  3
                    //    //4 5 6  7
                    //    //8 9 10 11
                    //    //float exponent = (float)System.Math.Pow(2f, colordata[3]);
                    //    //m_VertexColors[k] = new ColorRGBExp32(colordata[0],
                    //    //                                    colordata[1],
                    //    //                                    colordata[2],
                    //    //                                    255);
                    //    m_VertexColors[k] = new ColorRGBExp32(LightUtils.linearToTexLight(colordata[0], 1f),
                    //                                        LightUtils.linearToTexLight(colordata[1], 1f),
                    //                                        LightUtils.linearToTexLight(colordata[2], 1f),
                    //                                        255);
                    //}
                    //else 
                    if (Header.m_nVertexFlags.HasFlag(Structs.VertexFlags.VERTEX_NORMAL))
                    {
                        byte[] colordata = BR.ReadBytes(12);
                        //0 1 2  3
                        //4 5 6  7
                        //8 9 10 11
                        float exponent = (float)Math.Pow(2f, MathUtils.Avg(colordata[3], colordata[7], colordata[11]));
                        m_VertexColors[k] = new ColorRGBExp32(LightUtils.linearToTexLight(MathUtils.Avg(colordata[2], colordata[6], colordata[10]), exponent),
                                                            LightUtils.linearToTexLight(MathUtils.Avg(colordata[1], colordata[5], colordata[9]), exponent),
                                                            LightUtils.linearToTexLight(MathUtils.Avg(colordata[0], colordata[4], colordata[8]), exponent),
                                                            255);
                    }
                    else
                        throw new NotImplementedException("Not implemented vertex flag: " + Header.m_nVertexFlags);
                }
                Header.pMesh[n].m_VertexColors = m_VertexColors;
            }
        }
        #endregion
    }
}

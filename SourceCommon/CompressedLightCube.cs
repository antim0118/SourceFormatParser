namespace SourceFormatParser.Common
{
	/// <summary>
	/// compressed light cube (6 sides = 6 colors). 
	/// Size: 24 bytes
	/// </summary>
	public struct CompressedLightCube
	{
		public ColorRGBExp32[] m_Color; //[6]
	}
}

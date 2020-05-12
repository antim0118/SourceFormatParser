namespace SourceFormatParser.Common
{
    public static class LightUtils
    {
        public static byte linearToTexLight(byte c, float exponent) => (byte)MathUtils.Clamp(c * exponent * 0.5f, 0, 255);
        public static float linearToTexLightF(byte c, float exponent) => MathUtils.Clamp(c * exponent * 0.5f, 0, 255) / 255.0f;
    }
}

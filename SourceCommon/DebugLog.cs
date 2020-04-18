namespace SourceFormatParser.Common
{
    public static class DebugLog
    {
        public static void Write(object obj)
        {
#if UNITY
            UnityEngine.Debug.Log(obj);
#else
            System.Console.WriteLine(obj);
#endif
        }
    }
}

using System;
using System.Collections.Generic;

namespace SourceFormatParser.Common
{
    public static class FlagsUtils
    {
        public static T[] getFlags<T>(int f) where T : Enum
        {
            T flag = (T)Enum.Parse(typeof(T), f.ToString());
            var vals = Enum.GetValues(typeof(T));
            int vallen = vals.Length;
            List<T> ret = new List<T>();
            for (int i = 0; i < vallen; i++)
            {
                if (f != 0 && (int)vals.GetValue(i) == 0) continue;
                T val = (T)vals.GetValue(i);
                if (flag.HasFlag(val)) ret.Add(val);
            }
            return ret.ToArray();
        }

        public static T[] getFlags<T>(uint f) where T : Enum
        {
            T flag = (T)Enum.Parse(typeof(T), f.ToString());
            var vals = Enum.GetValues(typeof(T));
            int vallen = vals.Length;
            List<T> ret = new List<T>();
            for (int i = 0; i < vallen; i++)
            {
                if (f != 0 && (int)vals.GetValue(i) == 0) continue;
                T val = (T)vals.GetValue(i);
                if (flag.HasFlag(val)) ret.Add(val);
            }
            return ret.ToArray();
        }
    }
}

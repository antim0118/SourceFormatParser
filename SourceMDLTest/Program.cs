using SourceFormatParser.MDL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SourceMDLTest
{
    class Program
    {
        public static ulong readenbytes;
        static void Main(string[] args)
        {
            string[] mdltest = File.ReadAllLines(Path.Combine(Application.StartupPath, "mdltest.txt"));
            List<int> versions = new List<int>();
            foreach (string line in mdltest)
            {
                if (line.Length <= 1 || line[0] == '|' || !line.Contains("###")) continue;
                string[] spl = line.Split(new string[1] { "###" }, StringSplitOptions.None);
                string path = spl[0], comm = spl[1];
                Console.WriteLine($"Testing path: {path} - {comm}");
                string[] models = Directory.GetFiles(path, "*.mdl", SearchOption.AllDirectories);
                for (int mp = 0; mp < models.Length; mp++)
                {
                    string modelpath = models[mp];
                    Console.Title = $"[{BytesFormat(readenbytes)}] [{mp}/{models.Length}] ({(mp / (float)models.Length * 100f).ToString("0.00")}%) - {comm}";
                    try
                    {
                        SourceMDL mdl = new SourceMDL(modelpath);
                        var bonecs = mdl.BoneControllers;
                        if (bonecs != null && bonecs.Length > 0)
                        {
                            if (5 == 6) { }
                            bool unused = true;
                            if (!unused)
                                Console.WriteLine($"=========used {modelpath}");
                        }

                        #region append readenbytes
                        readenbytes += 408
                            - 21 //commented unused
                            - (ulong)(64 - mdl.Header.name.Length);

                        if (mdl.Header.studiohdr2index != 0)
                            readenbytes += 187 //studiohdr2 header
                                - 176; //commented unused

                        readenbytes += (ulong)(mdl.Bones.Length * (216 - 28)); //bones * (size - commented)
                        readenbytes += (ulong)(mdl.BoneControllers.Length * (56 - 32)); //bonecontrollers * (size - commented)
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[EXCEPTION] {modelpath} - {ex.Message}");
                    }
                }
                //GC.Collect();
            }

            Console.Write($"End. Used {BytesFormat(readenbytes)}");
            Console.ReadLine();
        }

        static string BytesFormat(ulong bytes)
        {
            double val = bytes;
            byte f = 0; //0 - bytes; 1 - kb; 2 - mb; 3 - gb
            while (val > 1024)
            {
                val /= 1024d;
                f++;
            }
            string s = val.ToString("0.00");
            switch (f)
            {
                case 0:
                    s += "b";
                    break;
                case 1:
                    s += "Kb";
                    break;
                case 2:
                    s += "Mb";
                    break;
                case 3:
                    s += "Gb";
                    break;
                default:
                    s += "?";
                    break;
            }
            return s;
        }
    }
}

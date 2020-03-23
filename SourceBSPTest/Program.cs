using System;
using System.Collections.Generic;
using System.IO;

namespace SourceFormatParser.BSP.SourceBSPTest
{
    class Program
    {
        static void Main()
        {
            Console.Write("[1] - Search for bsp files on PC\n" +
                "[2] - Test all the bsp files from bsptest.txt\n" +
                "N: ");
            int s = 0;
            try { s = int.Parse(Console.ReadLine()); } catch { }
            Console.Clear();

            string path;
            switch (s)
            {
                case 1:
                    Console.Write("Path for searching [ex. \"E:\\\"]: ");
                    path = Console.ReadLine();
                    IEnumerable<string> files = GetFiles(path, "*.bsp", true);
                    foreach (string f in files)
                    {
                        try
                        {
                            using (SourceBSP bsp = new SourceBSP(f))
                                Console.WriteLine($"v{bsp.Header.version}:{bsp.Header.mapRevision} - {f}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{ex.Message} - {f}");
                        }
                    }
                    break;
                case 2:
                    string[] lines = File.ReadAllLines("bsptest.txt");
                    int tested = 0, successfullytested = 0;
                    foreach(string l in lines)
                    {
                        if (l.Length <= 1 || l.StartsWith("|")) continue;
                        path = l.Split(new string[] { " - " }, StringSplitOptions.None)[1];
                        try
                        {
                            SourceBSP bsp = new SourceBSP(path);
                            successfullytested++;
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"ERROR: {ex.Message} - {path}");
                        }
                        tested++;
                    }
                    Console.WriteLine($"Tested: {successfullytested}/{tested}");
                    break;
                default:
                    Environment.Exit(0);
                    break;
            }

            Console.Write("END");
            Console.ReadLine();
        }

        public static IEnumerable<string> GetFiles(string root, string searchPattern, bool debug_setTitle = false)
        {
            Stack<string> pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count != 0)
            {
                var path = pending.Pop();
                if (debug_setTitle)
                    Console.Title = "Searching in: " + path;

                string[] nextfiles = null, nextdirs = null;
                try
                {
                    nextfiles = Directory.GetFiles(path, searchPattern);
                    nextdirs = Directory.GetDirectories(path);
                    foreach (var subdir in nextdirs) pending.Push(subdir);
                }
                catch { }
                if (nextfiles != null && nextfiles.Length != 0)
                    foreach (var f in nextfiles) yield return f;
            }
        }
    }
}

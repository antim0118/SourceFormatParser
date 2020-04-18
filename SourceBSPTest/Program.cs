using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Stopwatch sw = new Stopwatch();
            sw.Start();

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
                    foreach (string m in lines)
                    {
                        if (m.Length <= 1 || m.StartsWith("|")) continue;

                        string[] _spl = m.Split(new string[] { " - " }, StringSplitOptions.None);
                        string[] _spl0 = _spl[0].Split(':');
                        path = _spl[1];

                        string info = string.Empty;
                        if (_spl0.Length == 3)
                            info = _spl0[0];

                        try
                        {
                            SourceBSP bsp;
                            if (info == "l4d2")
                                bsp = new SourceBSP(path, SourceBSP.SourceGame.Left4Dead2);
                            else if (info == "csgops3")
                                continue; //i will make it later, mb
                                //bsp = new SourceBSP(path, SourceBSP.SourceGame.CSGO_PS3);
                            else
                                bsp = new SourceBSP(path);

                            successfullytested++;
                        }
                        catch (OutOfMemoryException ex) { Console.WriteLine(ex.Message); }
                        catch (Exception ex) { Console.WriteLine(ex.Message); }

                        tested++;
                    }
                    Console.WriteLine($"Tested: {successfullytested}/{tested}");
                    break;
                default:
                    Environment.Exit(0);
                    break;
            }

            sw.Stop();
            Console.Write($"Ended in {sw.ElapsedMilliseconds}ms");
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

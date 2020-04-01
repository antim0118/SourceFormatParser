using System.Collections.Generic;

namespace SourceFormatParser
{
    /// <summary>
    /// Parses Valve's KV file.
    /// Example of KV file:
    /// {
    ///     "key" "value"
    /// }
    /// </summary>
    public struct SourceKV
    {
        public string originalKV;

        private bool fullyParsed;
        private Dictionary<string, string> temp_kv;
        private bool ignoreBranches;
        private string[] lines;

        /// <param name="ignoreBranches">Ignore '{' and '}'</param>
        public SourceKV(string kv, bool ignoreBranches = false)
        {
            this.fullyParsed = false;
            this.temp_kv = new Dictionary<string, string>();
            this.originalKV = kv.ToLowerInvariant();
            if (originalKV.Contains("\n"))
                this.lines = originalKV.Split('\n');
            else
                this.lines = new string[1] { originalKV };
            this.ignoreBranches = ignoreBranches;
        }

        public string getByKey(string key)
        {
            key = key.ToLowerInvariant();

            //search in temp
            if (temp_kv.ContainsKey(key))
                return temp_kv[key];

            //parse kvs
            if (!fullyParsed)
            {
                int lvl = 0;
                bool isStarted = ignoreBranches;
                foreach (string l in lines)
                {
                    if (l.Length == 0) continue;
                    if (isStarted)
                    {
                        if (l[0] == '}')
                        {
                            if (lvl == 0)
                            {
                                fullyParsed = true;
                                break;
                            }
                            else
                                lvl--;
                        }
                        else if (l.Contains("\""))
                        {
                            string _l = l;
                            if (_l.Contains("//"))
                                _l = _l.Split(new string[1] { "//" }, System.StringSplitOptions.None)[0];
                            string[] _kvs = _l.Split('\"'); // 0 "1" 2 "3" 4
                            if (_kvs.Length < 5) continue;
                            string k = _kvs[1], v = _kvs[3];
                            if (key == _kvs[1])
                            {
                                temp_kv.Add(k, v);
                                return v;
                            }
                        }
                    }
                    else
                    {
                        if (l[0] == '{')
                            isStarted = true;
                    }
                }
            }
            return null; //not found key
        }

        public Dictionary<string, string> getAll()
        {
            //parse kvs
            if (!fullyParsed)
            {
                int lvl = 0;
                bool isStarted = ignoreBranches;
                foreach (string l in lines)
                {
                    if (l.Length == 0) continue;
                    if (isStarted)
                    {
                        if (l[0] == '}')
                        {
                            if (lvl == 0)
                            {
                                fullyParsed = true;
                                break;
                            }
                            else
                                lvl--;
                        }
                        else if (l[0] == '{')
                            lvl++;
                        else if (l.Contains("\""))
                        {
                            
                            string[] _kvs = l.Split('\"'); // 0 "1" 2 "3" 4
                            if (_kvs.Length < 5) continue;
                            string k = _kvs[1], v = _kvs[3];
                            if (!temp_kv.ContainsKey(k))
                                temp_kv.Add(k, v);
                        }
                    }
                    else
                    {
                        if (l[0] == '{')
                            isStarted = true;
                    }
                }
            }
            return temp_kv;
        }

    }
}

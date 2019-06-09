using System;
using System.Collections.Generic;

namespace Ranger
{
    public class Config
    {
        Dictionary<string, string> m_values = new Dictionary<string, string>();

        private string m_configPath;

        public string GetValue(string key)
        {
            if (m_values.ContainsKey(key.ToLower()))
            {
                return m_values[key.ToLower()];
            }

            return null;
        }

        public string GetValue(string key, string defaultValue)
        {
            if (m_values.ContainsKey(key.ToLower()))
            {
                return m_values[key.ToLower()];
            }

            return defaultValue;
        }

        public void SetValue(string key, string val)
        {
            m_values[key.ToLower()] = val;
        }

        public void Clear()
        {
            m_values.Clear();
        }

        public void Save()
        {
            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(m_configPath));
                System.IO.TextWriter tw = System.IO.File.CreateText(m_configPath);

                foreach (KeyValuePair<string, string> val in m_values)
                {
                    tw.WriteLine("{0}={1}", val.Key, val.Value);
                }

                tw.Close();
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Failed to save config file {0} - {1}", m_configPath, e.Message);
            }
        }

        public void Load(string path)
        {
            m_configPath = path;

            try
            {
                System.IO.TextReader tr = System.IO.File.OpenText(path);
                string line;
                bool parsing = true;

                while ((line = tr.ReadLine()) != null)
                {
                    line = line.Trim();

                    if (line.StartsWith("/*"))
                    {
                        parsing = false;
                    }
                    else
                    if (line.StartsWith("*/"))
                    {
                        parsing = true;
                    }
                    else
                     if (parsing && line.Length > 0 && !line.StartsWith("//"))
                     {
                         string[] split = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                         if (split.Length == 2)
                         {
                             m_values[split[0].Trim().ToLower()] = split[1].Trim();
                         }
                     }
                }

                tr.Close();
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Failed to load config file {0} - {1}", path, e.Message);
            }
        }
    };
}

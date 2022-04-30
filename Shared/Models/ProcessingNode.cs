﻿using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FileFlows.Shared.Models
{
    public class ProcessingNode: FileFlowObject
    {
        public string TempPath { get; set; }

        public string Address { get; set; }

        public bool Enabled { get; set; }

        public OperatingSystemType OperatingSystem { get; set; }

        public int FlowRunners { get; set; }

        public string SignalrUrl { get; set; }
        public List<KeyValuePair<string, string>> Mappings { get; set; }
        public string Schedule { get; set; }
        public bool DontChangeOwner { get; set; }
        public bool DontSetPermissions { get; set; }
        public string Permissions { get; set; }

        public char DirectorySeperatorChar = System.IO.Path.DirectorySeparatorChar;

        public string Map(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            if (Mappings != null && Mappings.Count > 0)
            {
                // convert all \ to / for now
                path = path.Replace("\\", "/");
                foreach (var mapping in Mappings)
                {
                    if (string.IsNullOrEmpty(mapping.Value) || string.IsNullOrEmpty(mapping.Key))
                        continue;
                    string pattern = Regex.Escape(mapping.Key.Replace("\\", "/"));
                    string replacement = mapping.Value.Replace("\\", "/");
                    path = Regex.Replace(path, "^" + pattern, replacement, RegexOptions.IgnoreCase);
                }
                // now convert / to path charcter
                if (DirectorySeperatorChar != '/')
                    path = path.Replace('/', DirectorySeperatorChar);
                if(path.StartsWith("//")) // special case for SMB paths
                    path = path.Replace('/', '\\');
            }
            return path;
        }
        public string UnMap(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            if (Mappings != null && Mappings.Count > 0)
            {
                foreach (var mapping in Mappings)
                {
                    if (string.IsNullOrEmpty(mapping.Value) || string.IsNullOrEmpty(mapping.Key))
                        continue;
                    path = Regex.Replace(path, "^" + Regex.Escape(mapping.Value), mapping.Key, RegexOptions.IgnoreCase);
                    path = Regex.Replace(path, "^" + Regex.Escape(mapping.Value.Replace("\\", "/")), mapping.Key, RegexOptions.IgnoreCase);
                    path = Regex.Replace(path, "^" + Regex.Escape(mapping.Value.Replace("/", "\\")), mapping.Key, RegexOptions.IgnoreCase);
                }
            }
            return path;
        }
    }
}

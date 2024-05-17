using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DolphinTextureExtraction.Scans.Results
{
    public class ScanResults
    {
        /// <summary>
        /// the time the scan process has taken
        /// </summary>
        public TimeSpan TotalTime { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsCompleted { get; internal set; }

        /// <summary>
        /// Size of all files to be searched in bytes.
        /// </summary>
        public double WorkeLength { get; internal set; }

        /// <summary>
        /// Size of all already searched files in bytes.
        /// </summary>
        public double ProgressLength { get; internal set; } = 0;

        /// <summary>
        /// Full path to the log file.
        /// </summary>
        public string LogFullPath { get; internal set; }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"Scan time: {TotalTime.TotalSeconds:.000}s");
            return sb.ToString();
        }
    }
}

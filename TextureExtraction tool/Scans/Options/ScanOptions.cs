using DolphinTextureExtraction.Scans.Results;
using System.Text;

namespace DolphinTextureExtraction.Scans.Options
{
    public class ScanOptions
    {

        public ParallelOptions Parallel = AppSettings.Parallel;

        internal ParallelOptions SubParallel => new()
        {
            MaxDegreeOfParallelism = Math.Max(1, Parallel.MaxDegreeOfParallelism / 2),
            CancellationToken = Parallel.CancellationToken,
            TaskScheduler = Parallel.TaskScheduler
        };

        /// <summary>
        /// Tries to extract files from unknown files, may cause errors.
        /// </summary>
        public bool Force = AppSettings.Force;

        /// <summary>
        /// Don't actually extract anything.
        /// </summary>
        public bool DryRun = AppSettings.DryRun;

        /// <summary>
        /// Maximum file depth to search,
        /// "0" for max.
        /// </summary>
        public uint Deep = AppSettings.Deep;

        /// <summary>
        /// will be executed if progress was made
        /// </summary>
        public Action<ScanResults> ProgressAction;

        private double LastProgressLength = 0;

        /// <summary>
        /// is executed when a texture is extracted
        /// </summary>
        public ListPrintDelegate ListPrintAction;

        public delegate void ListPrintDelegate(ScanResults result, string type, string filePath, string info = "");

        internal void ProgressUpdate(ScanResults result)
        {

            if (result.Progress >= result.Worke)
            {
                //we have to report the last progress!
                Monitor.Enter(result);
                try
                {
                    LastProgressLength = 0;
                    result.ProgressLength = result.WorkeLength;
                    ProgressAction?.Invoke(result);
                }
                finally
                {
                    Monitor.Exit(result);
                }
            }
            else
            {
                //Try to tell the Progress
                if (!Monitor.TryEnter(result))
                    return;

                try
                {
                    //is there really progress to report.
                    if (result.ProgressLength < LastProgressLength)
                        return;

                    //when data has been compressed, we can achieve more than 100%... we prevent this.
                    if (result.ProgressLength > result.WorkeLength)
                        result.ProgressLength = result.WorkeLength;

                    LastProgressLength = result.ProgressLength;

                    ProgressAction?.Invoke(result);
                }
                finally
                {
                    Monitor.Exit(result);
                }
            }

        }

        public override string ToString()
        {
            StringBuilder sb = new();
            ToString(sb);
            return sb.ToString();
        }

        protected void ToString(StringBuilder sb)
        {
#if DEBUG
            sb.Append($"Debug:True, ");
#endif
            sb.Append($"Tasks:");
            sb.Append(Parallel.MaxDegreeOfParallelism);
            sb.Append(", Force:");
            sb.Append(Force);
            sb.Append(", DryRun:");
            sb.Append(DryRun);
        }
    }
}

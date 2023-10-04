using AuroraLib.Texture;
using System.Text;

namespace DolphinTextureExtraction.Scans.Results
{
    public class FinalizeResults : ScanResults
    {
        private readonly List<int> Hash = new();

        public int Optimizations { get; private set; } = 0;
        public int Duplicates { get; private set; } = 0;

        public long NewSize { get; private set; } = 0;
        public long OldSize { get; private set; } = 0;

        public double OptimizationRate => ((double)NewSize / OldSize) * 100 - 100;

        public override string ToString()
        {
            StringBuilder sb = new();
            if (Hash.Count > 1)
                sb.AppendLine($"Textures processed: {Hash.Count}");
            if (Optimizations > 0)
                sb.AppendLine($"Optimizations: {Optimizations}");
            if (Duplicates > 0)
                sb.AppendLine($"Duplicates: {Duplicates}");
            if (Optimizations > 0)
            {
                sb.AppendLine($"File size from {PathX.AddSizeSuffix(OldSize, 2)} to {PathX.AddSizeSuffix(NewSize, 2)}");
                sb.AppendLine($"File size ratio: {OptimizationRate:+#.##;-#.##;0.00}%");
            }
            sb.AppendLine($"Scan time: {TotalTime.TotalSeconds:.000}s");
            return sb.ToString();
        }

        internal void AddOptimization()
        {
            lock (this)
            {
                Optimizations++;
            }
        }

        internal void AddSize(long oldSize, long newSize)
        {
            lock (this)
            {
                OldSize += oldSize;
                NewSize += newSize;
            }
        }

        internal bool AddHashIfNeeded(DolphinTextureHashInfo hash)
        {
            int hashCode = hash.GetHashCode();

            lock (Hash)
            {
                //Skip duplicate textures
                if (Hash.Contains(hashCode))
                {
                    Duplicates++;
                    return true;
                }
                Hash.Add(hashCode);
            }
            return false;
        }
    }
}

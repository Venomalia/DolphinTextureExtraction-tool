using AuroraLip.Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraLip.Common
{
    public static class Reflection
    {
        public static FileAccessReflection<IFileAccess> FileAccess = new FileAccessReflection<IFileAccess>();

        public static CompressionReflection Compression = new CompressionReflection();
    }
}

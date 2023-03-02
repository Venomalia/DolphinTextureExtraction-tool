using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraLip.Compression.Formats
{
    public class YAZ1 : YAZ0
    {
        public override string Magic => magic;

        public new const string magic = "Yaz1";
    }
}

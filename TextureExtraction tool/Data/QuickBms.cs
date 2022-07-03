using System.IO;

namespace DolphinTextureExtraction_tool.Data
{
    internal class QuickBms
    {
        public readonly static string Folder = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "QuickBms");

        private static string Executable { get; set; }

        public static bool Exists { get; private set; } = false;

        public static bool Use(string path, FileTypInfo fileTyp)
        {
            return false;
        }

        private static void GetQuickBms()
        {
            Executable = Path.Combine(Folder, "quickbms_4gb_files.exe");
            if (!File.Exists(Executable))
            {
                Executable = Path.Combine(Folder, "quickbms.exe");
                if (!File.Exists(Executable))
                {
                    Exists = false;
                    return;
                }
            }
            Exists = true;
        }
    }
}

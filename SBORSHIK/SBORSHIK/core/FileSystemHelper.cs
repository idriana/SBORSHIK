using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SBORSHIK.core
{
    public static class FileSystemHelper
    {
        public static bool CheckDirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static bool CheckFileExists(string path)
        {
            return File.Exists(path);
        }
    }
}

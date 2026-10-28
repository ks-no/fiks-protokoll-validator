using System;
using System.IO;

namespace KS.FiksProtokollValidator.WebAPI.Utilities
{
    public static class PayloadFilePath
    {
        private static readonly string[] AllowedDirectories = { "Samples", "TestCases" };

        public static string Resolve(string baseDirectory, string relativeFilePath)
        {
            if (string.IsNullOrEmpty(baseDirectory) || string.IsNullOrEmpty(relativeFilePath) || Path.IsPathRooted(relativeFilePath))
            {
                return null;
            }

            var fullPath = Path.GetFullPath(Path.Combine(baseDirectory, relativeFilePath));

            foreach (var allowedDirectory in AllowedDirectories)
            {
                var allowedFullPath = Path.GetFullPath(Path.Combine(baseDirectory, allowedDirectory)) + Path.DirectorySeparatorChar;
                if (fullPath.StartsWith(allowedFullPath, StringComparison.Ordinal))
                {
                    return fullPath;
                }
            }

            return null;
        }

        public static bool IsFileName(string fileName)
        {
            return !string.IsNullOrEmpty(fileName) && fileName != "." && fileName != ".." && Path.GetFileName(fileName) == fileName;
        }
    }
}

using System;
using System.IO;

namespace OpenLocalization
{
    internal static class PathUtils
    {
        static public string GetPathRelative(string root, string dest)
        {
            var rootUri = new Uri(Path.GetFullPath(root) + "/", UriKind.Absolute);
            var destUri = new Uri(dest, UriKind.Absolute);
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(destUri).ToString());
        }

        static public string GetPathPossiblyRelativeToGameRoot(string path)
        {
            var relative = GetPathRelative(BepInEx.Paths.GameRootPath, path);
            if (relative.StartsWith(".."))
            {
                return path;
            }
            return relative.Replace('\\', '/');
        }
    }


    internal static class StringExtensions
    {
        public static string NullIfEmpty(this string s)
        {
            return string.IsNullOrEmpty(s) ? null : s;
        }
        public static string NullIfWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }
    }
}

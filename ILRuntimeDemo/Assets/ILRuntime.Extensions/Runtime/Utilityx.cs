using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace UnityEngine.Internal
{
    internal class Utilityx
    {

        //public static string GetPackageName(Type type)
        //{
        //    Assembly assembly = Assembly.GetExecutingAssembly();
        //    if (!assembly.TryGetMetadata("Unity.Package.Name", out var value))
        //        return null;
        //    return value;
        //}


        #region package.json

        [Serializable]
        class PackageInfo
        {
            public string name;
        }

        public static string GetPackageDirectory(string packageName)
        {

            string packageJsonFileName = "package.json";

            // [PackageCache/*]
            string packagePath = $"Packages/{packageName}/{packageJsonFileName}";
            if (File.Exists(Path.Combine(packagePath)))
            {
                try
                {
                    var pkg = JsonUtility.FromJson<PackageInfo>(File.ReadAllText(packagePath));
                    if (pkg.name == packageName)
                        return Path.Combine("Packages", packageName);
                }
                catch { }
            }

            // [Assets/**]
            foreach (var path in Directory.GetFiles("Assets", packageJsonFileName, SearchOption.AllDirectories))
            {
                try
                {
                    var pkg = JsonUtility.FromJson<PackageInfo>(File.ReadAllText(path));
                    if (pkg.name == packageName)
                        return Path.GetDirectoryName(path);
                }
                catch { }
            }

            // [Packages/*]
            foreach (var dir in Directory.GetDirectories("Packages", "*", SearchOption.TopDirectoryOnly))
            {
                string path = Path.Combine(dir, packageJsonFileName);
                Debug.Log(path);
                if (File.Exists(path))
                {
                    try
                    {
                        var pkg = JsonUtility.FromJson<PackageInfo>(File.ReadAllText(path));
                        if (pkg.name == packageName)
                            return Path.GetDirectoryName(path);
                    }
                    catch { }
                }
            }

            throw new Exception($"Not found package directory, require package name '{packageName}' {packageJsonFileName}");
        }


        #endregion


        public static bool IncludeExclude(string input, string includePattern, string excludePattern)
        {
            if (!string.IsNullOrEmpty(includePattern) && !Regex.IsMatch(input, includePattern, RegexOptions.IgnoreCase))
                return false;

            if (!string.IsNullOrEmpty(excludePattern) && Regex.IsMatch(input, excludePattern, RegexOptions.IgnoreCase))
                return false;
            return true;
        }
    }
}
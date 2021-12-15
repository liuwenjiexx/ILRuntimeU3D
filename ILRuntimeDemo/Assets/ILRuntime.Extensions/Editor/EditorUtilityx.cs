using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.Internal
{
    internal static class EditorUtilityx
    {
        private static string packageName;
        static string packageDirectory;
        static string ussDirectory;
        static string uxmlDirectory;


        public static string PackageName
        {
            get
            {
                if (packageName == null)
                {
                    packageName = GetPackageName(typeof(EditorUtilityx).Assembly);
                    if (packageName == null)
                        packageName = string.Empty;
                }
                return packageName;
            }
        }

        public static string PackageDirectory
        {
            get
            {
                if (packageDirectory == null)
                {
                    packageDirectory = GetPackageDirectory(PackageName);                    
                }
                return packageDirectory;
            }
        }

        static string USSDirectory
        {
            get
            {
                if (ussDirectory == null)
                {
                    ussDirectory = PackageDirectory + "/Editor/USS";
                }
                return ussDirectory;
            }
        }
        static string UXMLDirectory
        {
            get
            {
                if (uxmlDirectory == null)
                {
                    uxmlDirectory = PackageDirectory + "/Editor/UXML";
                }
                return uxmlDirectory;
            }
        }


        public static string SelectedDirectory
        {
            get
            {
                string selectedPath = null;

                foreach (var guid in Selection.assetGUIDs)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (AssetDatabase.IsValidFolder(path))
                        {
                            selectedPath = path;
                            break;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(selectedPath))
                            {
                                selectedPath = Path.GetDirectoryName(path);
                            }
                        }
                    }
                }
                return selectedPath;
            }
        }


        public static string GetPackageName(Assembly assembly)
        {
            if (!TryGetMetadata(assembly, "Unity.Package.Name", out var value))
                return null;
            return value;
        }

        public static bool TryGetMetadata(this Assembly assembly, string key, out string value)
        {
            foreach (var attr in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
            {
                if (attr.Key == key)
                {
                    value = attr.Value;
                    return true;
                }
            }
            value = null;
            return false;
        }

        public static string GetMetadata(this Assembly assembly, string key)
        {
            if (!TryGetMetadata(assembly, key, out var value))
                throw new Exception($"Not found AssemblyMetadataAttribute. key: {key}");
            return value;
        }

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


        public static string GetUSSPath(string ussFile)
        {
            return USSDirectory + "/" + ussFile;
        }
        public static string GetUXMLPath(string uxmlFile)
        {
            return UXMLDirectory + "/" + uxmlFile;
        }

        public static bool IncludeExclude(string input, string includePattern, string excludePattern)
        {
            if (!string.IsNullOrEmpty(includePattern) && !Regex.IsMatch(input, includePattern, RegexOptions.IgnoreCase))
                return false;

            if (!string.IsNullOrEmpty(excludePattern) && Regex.IsMatch(input, excludePattern, RegexOptions.IgnoreCase))
                return false;
            return true;
        }


        public static void OpenFolder(string dir)
        {

            EditorUtility.RevealInFinder(dir);
        }


        public static bool ItemsEquals(this IEnumerable source, IEnumerable compare)
        {
            if (source == null)
            {
                if ((compare != null) && compare.GetEnumerator().MoveNext())
                {
                    return false;
                }
                return true;
            }
            if (compare == null)
            {
                if ((source != null) && source.GetEnumerator().MoveNext())
                {
                    return false;
                }
                return true;
            }

            return ItemsEquals(source.GetEnumerator(), compare.GetEnumerator());
        }
        public static bool ItemsEquals(this IEnumerator source, IEnumerator compare)
        {
            while (source.MoveNext())
            {
                if (!compare.MoveNext())
                    return false;

                if (!object.Equals(source.Current, compare.Current))
                    return false;
            }
            return !compare.MoveNext();
        }
        public static bool ItemsEquals<T>(this T[] source, T[] compare)
        {
            if (source == null)
            {
                if ((compare != null) && (compare.Length != 0))
                {
                    return false;
                }
                return true;
            }
            if (compare == null)
            {
                if ((source != null) && (source.Length != 0))
                {
                    return false;
                }
                return true;
            }
            int len = source.Length;
            if (len != compare.Length)
                return false;

            if (len != 0)
            {
                for (int i = 0; i < len; i++)
                {
                    if (!object.Equals(source[i], compare[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }


    }


}
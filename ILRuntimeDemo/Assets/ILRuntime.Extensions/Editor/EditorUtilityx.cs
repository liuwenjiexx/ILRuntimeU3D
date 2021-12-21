using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Internal;

namespace UnityEditor.Internal
{
    internal class EditorUtilityx : Utilityx
    {

        static string packageDirectory;
        static string ussDirectory;
        static string uxmlDirectory;


        public static string PackageName
        {
            get
            {
                return typeof(EditorUtilityx).Assembly.GetUnityPackageName();
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


        public static string GetUSSPath(string ussFile)
        {
            return USSDirectory + "/" + ussFile;
        }
        public static string GetUXMLPath(string uxmlFile)
        {
            return UXMLDirectory + "/" + uxmlFile;
        }



        public static void OpenFolder(string dir)
        {

            EditorUtility.RevealInFinder(dir);
        }



    }


}
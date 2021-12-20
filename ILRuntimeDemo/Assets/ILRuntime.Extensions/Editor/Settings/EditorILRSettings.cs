using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Internal;
using UnityEngine;


namespace UnityEditor.ILRuntime.Extensions
{
    using SettingsProvider = UnityEditor.Internal.SettingsProvider;

    [Serializable]
    public class EditorILRSettings
    {
        [SerializeField]
        private string projectPath;

        [SerializeField]
        private bool autoBuild = true;



        #region Provider


        private static SettingsProvider provider;

        internal static SettingsProvider Provider
        {
            get
            {
                if (provider == null)
                {
                    provider = new SettingsProvider(typeof(EditorILRSettings), EditorUtilityx.PackageName, false, true);
                    provider.FileName = "Settings.json";
                }
                return provider;
            }
        }

        public static EditorILRSettings Settings { get => (EditorILRSettings)Provider.Settings; }

        #endregion

        public static string ProjectPath
        {
            get => Settings.projectPath;
            set => Provider.SetProperty(nameof(ProjectPath), ref Settings.projectPath, value);
        }

        public static bool AutoBuild
        {
            get => Settings.autoBuild;
            set => Provider.SetProperty(nameof(AutoBuild), ref Settings.autoBuild, value);
        }

        [SerializeField]
        private bool isDebug = true;
        public static bool IsDebug
        {
            get => Settings.isDebug;
            set => Provider.SetProperty(nameof(IsDebug), ref Settings.isDebug, value);
        }

        [SerializeField]
        public string assemblyPath = "Assets/StreamingAssets";
        public static string AssemblyPath
        {
            get => Settings.assemblyPath;
            set => Provider.SetProperty(nameof(AssemblyPath), ref Settings.assemblyPath, value);
        }

        [SerializeField]
        public string generatedCodePath="Assets/Samples/ILRuntime/Generated";
        public static string GeneratedCodePath
        {
            get => Settings.generatedCodePath;
            set => Provider.SetProperty(nameof(GeneratedCodePath), ref Settings.generatedCodePath, value);
        }



    }
}
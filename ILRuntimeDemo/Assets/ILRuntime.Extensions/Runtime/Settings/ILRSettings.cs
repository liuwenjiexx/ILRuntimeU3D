using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Internal;

namespace UnityEngine.ILRuntime.Extensions
{
    using SettingsProvider = UnityEngine.Internal.SettingsProvider;

    [Serializable]
    public class ILRSettings
    {

        internal const string ProjectSettingsPath = "Project Settings/Extensions/ILRuntime";

        #region Provider


        private static SettingsProvider provider;

        internal static SettingsProvider Provider
        {
            get
            {
                if (provider == null)
                {
                    provider = new SettingsProvider(typeof(ILRSettings), typeof(ILRSettings).Assembly.GetUnityPackageName(), true, true);
                    provider.FileName = "Settings.json";
                }
                return provider;
            }
        }

        public static ILRSettings Settings { get => (ILRSettings)Provider.Settings; }

        #endregion

        [SerializeField]
        private string assemblyName;

        public static string AssemblyName
        {
            get => Settings.assemblyName ?? string.Empty;
            set => Provider.SetProperty(nameof(AssemblyName), ref Settings.assemblyName, value);
        }



        //[SerializeField]
        //private string streamingAssetsPath = "ILR";

        //public static string StreamingAssetsPath
        //{
        //    get => Settings.streamingAssetsPath ?? string.Empty;
        //    set => Provider.SetProperty(nameof(StreamingAssetsPath), ref Settings.streamingAssetsPath, value);
        //}


    }
}
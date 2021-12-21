using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Internal;

namespace UnityEngine.ILRuntime.Extensions
{
    using SettingsProvider = UnityEngine.Internal.SettingsProvider;

    [Serializable]
    public class ILRSettings
    {

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
        private string streamingAssetsPath = "HotFix_Project.dll";

        public static string StreamingAssetsPath
        {
            get => Settings.streamingAssetsPath;
            set => Provider.SetProperty(nameof(StreamingAssetsPath), ref Settings.streamingAssetsPath, value);
        }


    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UnityEditor.ILRuntime.Extensions
{
    public class ILRProjectHelper
    {
        private static string msBuildPath;
        private static string vsDevenvPath;
        static List<FileSystemWatcher> fsws;
        static bool sourceCodeChanged;
        private static object lockObj = new object();
        private static bool enabledAutoCompile;

        [InitializeOnLoadMethod]
        static void InitializeOnLoadMethod()
        {
            EditorILRSettings.Provider.PropertyChanged += Provider_PropertyChanged;
            if (CanCompile())
            {
                EnableAutoCompile();
            }
        }

        static bool CanCompile()
        {
            try
            {
                return EditorILRSettings.AutoBuild && !string.IsNullOrEmpty(EditorILRSettings.ProjectPath) && !string.IsNullOrEmpty(VSHomePath);
            }
            catch
            {
                return false;
            }
        }

        private static void Provider_PropertyChanged(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(EditorILRSettings.ProjectPath):
                case nameof(EditorILRSettings.AutoBuild):

                    DisableAutoCompile();
                    if (CanCompile())
                    {
                        EnableAutoCompile();
                    }
                    break;
            }
        }

        static void _CompileAssemblies(bool focus)
        {
            if (focus && EditorILRSettings.AutoBuild)
            {
                if (sourceCodeChanged)
                {
                    CompileProject();
                }
            }
        }
        static void EnableAutoCompile()
        {
            if (enabledAutoCompile)
                return;

            enabledAutoCompile = true;

            EditorWindowFocusUtility.OnUnityEditorFocus -= _CompileAssemblies;
            EditorWindowFocusUtility.OnUnityEditorFocus += _CompileAssemblies;
            EnableSourceListening();
        }

        static void DisableAutoCompile()
        {
            enabledAutoCompile = false;
            EditorWindowFocusUtility.OnUnityEditorFocus -= _CompileAssemblies;
            DisableSourceListening();
        }

        static void EnableSourceListening()
        {
            if (string.IsNullOrEmpty(EditorILRSettings.ProjectPath))
                return;

            if (fsws != null)
                return;
            fsws = new List<FileSystemWatcher>();

            string fullDirPath = Path.GetFullPath(Path.GetDirectoryName(EditorILRSettings.ProjectPath));
            var fsw = new FileSystemWatcher(fullDirPath, "*.cs");
            fsw.EnableRaisingEvents = true;
            fsw.Changed += Fsw_Changed;
            fsws.Add(fsw);
        }

        private static void Fsw_Changed(object sender, FileSystemEventArgs e)
        {
            lock (lockObj)
            {
                sourceCodeChanged = true;
            }
        }

        static void DisableSourceListening()
        {
            if (fsws == null)
                return;
            foreach (var fsw in fsws)
            {
                fsw.Dispose();
            }
            fsws.Clear();
            fsws = null;
        }


        [MenuItem("ILRuntime/Compile ILR Assemblies")]
        public static void CompileProject()
        {

            int processId = Progress.Start("Compile ILR Assemblies");
            EditorUtility.DisplayProgressBar("Compile ILR Assemblies", "Compiling", 0f);
            try
            {
                StringBuilder builder = new StringBuilder();
                lock (lockObj)
                {
                    sourceCodeChanged = false;
                }

                string fullProjPath = Path.GetFullPath(EditorILRSettings.ProjectPath);
                using (Process proc = new Process())
                {
                    proc.StartInfo = new ProcessStartInfo()
                    {
                        WorkingDirectory = Path.GetDirectoryName(fullProjPath),
                        FileName = VSDevenvPath,
                        Arguments = $"\"{fullProjPath}\"  /Build {(EditorILRSettings.IsDebug ? "Debug" : "Release")}",
                        //FileName = MSBuildPath,
                        //Arguments = $"\"{fullProjPath}\"  /property:Configuration={(EditorILRSettings.IsDebug ? "Debug" : "Release")}",
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                    };
                    proc.ErrorDataReceived += (sender, e) =>
                    {
                        builder.AppendLine(e.Data);
                    };
                    proc.Start();
                    proc.WaitForExit();
                    if (builder.Length > 0 || proc.ExitCode != 0)
                    {
                        throw new Exception($"Build project error. errorCode: {proc.ExitCode}\narguments: {proc.StartInfo.Arguments}\n{builder.ToString()}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                Progress.Remove(processId);
            }

        }


        [MenuItem("Assets/Open ILR Project")]
        public static void OpenVisualStudioProject()
        {
            StringBuilder builder = new StringBuilder();

            string fullProjPath = Path.GetFullPath(EditorILRSettings.ProjectPath);
            using (Process proc = new Process())
            {
                proc.StartInfo = new ProcessStartInfo()
                {
                    FileName = VSDevenvPath,
                    Arguments = $"\"{fullProjPath}\"",
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };
                proc.ErrorDataReceived += (sender, e) =>
                {
                    builder.AppendLine(e.Data);
                };
                proc.Start();

            }
        }


        private static string vsHomePath;
        public static string VSHomePath
        {
            get
            {
                if (vsHomePath == null)
                {
                    string vsHome = Environment.GetEnvironmentVariable("VisualStudio");
                    if (string.IsNullOrEmpty(vsHome))
                        throw new Exception("Not found environment variable 'VisualStudio'");
                    vsHomePath = vsHome;
                }
                return vsHomePath;
            }
        }
        public static string VSDevenvPath
        {
            get
            {
                if (vsDevenvPath == null)
                {
                    if (!string.IsNullOrEmpty(VSHomePath))
                        vsDevenvPath = Path.Combine(VSHomePath, @"Common7\IDE\devenv.exe");
                    else
                        vsDevenvPath = string.Empty;

                }
                return vsDevenvPath;
            }
        }

        public static string MSBuildPath
        {
            get
            {
                if (msBuildPath == null)
                {
                    if (!string.IsNullOrEmpty(VSHomePath))
                        msBuildPath = Path.Combine(VSHomePath, @"MSBuild\Current\Bin\MSBuild.exe");
                    else
                        msBuildPath = string.Empty;
                }
                return msBuildPath;
            }
        }


        [InitializeOnLoad]
        public class EditorWindowFocusUtility
        {
            public static event Action<bool> OnUnityEditorFocus = (focus) => { };
            private static bool _appFocused;
            static EditorWindowFocusUtility()
            {
                EditorApplication.update += Update;
            }

            private static void Update()
            {
                if (!_appFocused && UnityEditorInternal.InternalEditorUtility.isApplicationActive)
                {
                    _appFocused = UnityEditorInternal.InternalEditorUtility.isApplicationActive;
                    OnUnityEditorFocus(true);
                }
                else if (_appFocused && !UnityEditorInternal.InternalEditorUtility.isApplicationActive)
                {
                    _appFocused = UnityEditorInternal.InternalEditorUtility.isApplicationActive;
                    OnUnityEditorFocus(false);
                }
            }
        }
    }
}
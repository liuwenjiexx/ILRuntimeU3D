using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
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
        static VSSolution vsSolution;
        static string lastCompileHash;

        internal static VSSolution Solution
        {
            get => vsSolution;
        }

        private static string LastCompileHash
        {
            get
            {
                if (lastCompileHash == null)
                {
                    lastCompileHash = EditorPrefs.GetString("ILRuntime.Extensions.LastCompileHash");

                    if (lastCompileHash == null)
                    {
                        lastCompileHash = string.Empty;
                    }
                }
                return lastCompileHash;
            }
            set
            {
                if (lastCompileHash != value)
                {
                    lastCompileHash = value;
                    EditorPrefs.SetString("ILRuntime.Extensions.LastCompileHash", lastCompileHash);
                }
            }
        }


        [InitializeOnLoadMethod]
        static void InitializeOnLoadMethod()
        {
            LoadProject();

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
                return vsSolution != null && EditorILRSettings.AutoBuild && !string.IsNullOrEmpty(VSHomePath);
            }
            catch
            {
                return false;
            }
        }

        static string CalculateSourceCodeHash()
        {
            if (vsSolution == null)
                return string.Empty;

            int hash = 0;
            foreach (var proj in vsSolution.Projects.OrderBy(o => o.Name))
            {
                foreach (var file in Directory.GetFiles(proj.FullDir, "*.cs").OrderBy(o => o))
                {
                    hash = CombineHashCode(hash, new FileInfo(file).LastWriteTimeUtc.ToFileTimeUtc().GetHashCode());
                }
            }
            return hash.ToString();
        }

        internal static int CombineHashCode(int h1, int h2)
        {
            return (((h1 << 5) + h1) ^ h2);
        }


        static void LoadProject()
        {
            string path = EditorILRSettings.ProjectPath;
            vsSolution = null;
            if (string.IsNullOrEmpty(path))
                return;
            try
            {
                VSSolution sln = new VSSolution(path);
                sln.Load();
                vsSolution = sln;
            }
            catch (Exception ex)
            {
                throw ex;
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
            if (LastCompileHash != CalculateSourceCodeHash())
            {
                CompileProject();
            }

        }

        static void DisableAutoCompile()
        {
            enabledAutoCompile = false;
            EditorWindowFocusUtility.OnUnityEditorFocus -= _CompileAssemblies;
            DisableSourceListening();
        }

        static void EnableSourceListening()
        {
            if (vsSolution == null)
                return;

            if (fsws != null)
                return;
            fsws = new List<FileSystemWatcher>();

            foreach (var proj in vsSolution.Projects)
            {
                string fullDir = proj.FullDir;
                var fsw = new FileSystemWatcher(fullDir, "*.cs");
                fsw.Changed += Fsw_Changed;
                fsw.EnableRaisingEvents = true;
                fsws.Add(fsw);
            }
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
                LastCompileHash = CalculateSourceCodeHash();

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

        internal class VSSolution
        {
            Regex keyRegex = new Regex("^\\s*(?<key>.+?)(\\((\"(?<key_param>.+?)\"|(?<key_param>.+?))\\))?(\\s=\\s(?<value>.+))?$", RegexOptions.Multiline);
            Regex multiValueRegex = new Regex(",?\\s*\"(?<value>[^\"]+)\"");

            public VSSolution(string path)
            {
                this.Path = path;
            }


            public string Path { get; set; }

            public string FullPath { get => System.IO.Path.GetFullPath(Path); }

            public string FullDir { get => System.IO.Path.GetDirectoryName(FullPath); }

            public List<VSProject> Projects { get; set; } = new List<VSProject>();

            public void Load()
            {
                string content = File.ReadAllText(Path);

                foreach (Match m in keyRegex.Matches(content))
                {
                    string key = m.Groups["key"].Value;
                    string keyParam = m.Groups["key_param"].Value;
                    string value = m.Groups["value"].Value;
                    switch (key)
                    {
                        case "Project":
                            {
                                int index = 0;
                                string name = null, projPath = null, guid = null;
                                foreach (Match mv in multiValueRegex.Matches(value))
                                {
                                    string v = mv.Groups["value"].Value;
                                    switch (index++)
                                    {
                                        case 0:
                                            name = v;
                                            break;
                                        case 1:
                                            projPath = v;
                                            break;
                                        case 2:
                                            guid = v;
                                            break;
                                    }
                                }
                                var proj = AddProject();
                                proj.Guid = guid;
                                proj.Name = name;
                                proj.Path = projPath;
                                proj.Load();
                            }
                            break;
                    }

                }
            }

            public VSProject AddProject()
            {
                var proj = new VSProject();
                proj.Solution = this;
                Projects.Add(proj);
                return proj;
            }

            public void RemoveProject(string guid)
            {
                var proj = GetProject(guid);
                if (proj != null)
                {
                    proj.Solution = null;
                    Projects.Remove(proj);
                }
            }

            public VSProject GetProject(string guid)
            {
                VSProject proj = default;
                foreach (var item in Projects)
                {
                    if (item.Guid == guid)
                    {
                        proj = item;
                        break;
                    }
                }
                return proj;
            }

        }


        internal class VSProject
        {
            internal VSProject()
            {
            }
            internal VSProject(string guid)
            {
                this.Guid = guid;
            }

            public string Guid { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }

            public VSSolution Solution { get; internal set; }

            public string AssemblyName { get; set; }

            public string RootNamespace { get; set; }

            public string FullPath
            {
                get
                {
                    string path = this.Path;
                    if (!System.IO.Path.IsPathRooted(path))
                    {
                        path = System.IO.Path.Combine(Solution.FullDir, path);
                    }
                    return System.IO.Path.GetFullPath(path);
                }
            }

            public string FullDir
            {
                get
                {
                    string dir = System.IO.Path.GetDirectoryName(FullPath);
                    return dir;
                }
            }

            public void Load()
            {
                if (!File.Exists(FullPath))
                {
                    return;
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(FullPath);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("vs", "http://schemas.microsoft.com/developer/msbuild/2003");


                var projNode = doc.DocumentElement;

                XmlNode defaultPropGroup = null;
                foreach (XmlNode node in projNode.SelectNodes("vs:PropertyGroup", nsmgr))
                {
                    if (node.Attributes["Condition"] == null)
                    {
                        defaultPropGroup = node;
                        break;
                    }
                }

                if (defaultPropGroup == null)
                {
                    defaultPropGroup = projNode.SelectSingleNode("vs:PropertyGroup", nsmgr);
                }

                foreach (XmlNode child in defaultPropGroup.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        string value = child.InnerText;
                        switch (child.LocalName)
                        {
                            case "ProjectGuid":
                                Guid = value;
                                break;
                            case "AssemblyName":
                                AssemblyName = value;
                                break;
                            case "RootNamespace":
                                RootNamespace = value;
                                break;
                        }
                    }
                }

            }

        }

    }
}
using ILRuntime.Runtime.Enviorment;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using ILRuntime.Mono.Cecil.Cil;

namespace UnityEngine.ILRuntime.Extensions
{
    public class ILRuntimeLoader : MonoBehaviour
    {

        AppDomain appDomain;
        protected List<IDisposable> disposeObjs = new List<IDisposable>();


        public AppDomain AppDomain { get => appDomain; }

        public event Action<string> AssemblyLoaded;
        public bool Initalized { get; private set; }

        public delegate void ILRuntimeLoadAssemblyCallback(Stream assemblyReader, Stream symbolReader, bool pdb);


        // Start is called before the first frame update
        protected virtual void Start()
        {
            StartCoroutine(Initalize());
        }

        protected string GetUrl(string url)
        {

            if (url.IndexOf("://") < 0)
                url = "file:///" + url;
            return url;
        }


        protected virtual IEnumerator Initalize()
        {

            if (string.IsNullOrEmpty(ILRSettings.AssemblyName))
            {
                Debug.LogError($"'{ILRSettings.ProjectSettingsPath}/Assembly Name' not set");
                yield break;
            }

            appDomain = new AppDomain();

            if (disposeObjs == null)
                disposeObjs = new List<IDisposable>();

            foreach (var assemblyName in ILRSettings.AssemblyName.Split('|'))
            {
                yield return StartCoroutine(_LoadAssembly(assemblyName));
            }

            OnILRInitialize(appDomain);

            appDomain.DebugService.StartDebugService(56000);

            OnILRLoaded(appDomain);
            
            Initalized = true;

        }

        public void LoadAssembly(string assemblyName)
        {
            StartCoroutine(_LoadAssembly(assemblyName));
        }

        private IEnumerator _LoadAssembly(string assemblyName)
        {
            Stream assemblyReader = null, symbolReader = null;

            bool isDone;
            bool isPdb;
            assemblyReader = null;
            symbolReader = null;
            isPdb = true;
            isDone = false;

            LoadAssembly(assemblyName, (_assemblyReader, _symbolReader, _pdb) =>
            {
                isDone = true;
                assemblyReader = _assemblyReader;
                symbolReader = _symbolReader;
                isPdb = _pdb;
            });

            yield return new WaitUntil(() => isDone);

            if (assemblyReader == null)
            {
                if (symbolReader != null)
                    symbolReader.Dispose();            
                throw new Exception("ILR assembly load fail. " + assemblyName);
            }

            try
            {
                disposeObjs.Add(assemblyReader);

                ISymbolReaderProvider symbolReaderProvider = null;

                if (symbolReader != null)
                {
                    disposeObjs.Add(symbolReader);

                    if (isPdb)
                    {
                        symbolReaderProvider = new global::ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider();
                    }
                    else
                    {
                        symbolReaderProvider = new global::ILRuntime.Mono.Cecil.Mdb.MdbReaderProvider();
                    }
                }

                appDomain.LoadAssembly(assemblyReader, symbolReader, symbolReaderProvider);

                AssemblyLoaded?.Invoke(assemblyName);
            }
            catch
            {
                Debug.LogError("º”‘ÿ»»∏¸DLL ß∞‹");
            }

        }


        protected virtual void LoadAssembly(string assemblyName, ILRuntimeLoadAssemblyCallback result)
        {
            StartCoroutine(_LoadAssembly(assemblyName, result));
        }

        private IEnumerator _LoadAssembly(string assemblyName, ILRuntimeLoadAssemblyCallback result)
        {
            string streamingAssetsPath = Application.streamingAssetsPath;
            if (!string.IsNullOrEmpty(ILRSettings.StreamingAssetsPath))
            {
                streamingAssetsPath += $"/{ILRSettings.StreamingAssetsPath}";
            }
            string url;
            url = GetUrl(streamingAssetsPath + $"/{assemblyName}.dll");
            Stream assemblyReader = null, symbolReader = null;
            using (var request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (!string.IsNullOrEmpty(request.error))
                {
                    throw new Exception(request.error + "\n" + url);
                }

                byte[] bytes = request.downloadHandler.data;
                assemblyReader = new MemoryStream(bytes);

            }

            url = GetUrl(streamingAssetsPath + $"/{assemblyName}.pdb");
            using (var request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (string.IsNullOrEmpty(request.error))
                {
                    byte[] bytes = request.downloadHandler.data;
                    symbolReader = new MemoryStream(bytes);
                }
            }

            result(assemblyReader, symbolReader, true);
        }

        protected virtual void OnILRInitialize(AppDomain appDomain)
        {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
            appDomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif

            appDomain.InitalizeExtensions();
        }

        protected virtual void OnILRLoaded(AppDomain appDomain)
        {

        }

        protected virtual void OnDestroy()
        {
            if (appDomain != null)
            {
                appDomain.Dispose();
                appDomain = null;
            }
            try
            {
                foreach (var o in disposeObjs)
                {
                    o.Dispose();
                }
                disposeObjs.Clear();
            }
            catch { }

        }
    }

}
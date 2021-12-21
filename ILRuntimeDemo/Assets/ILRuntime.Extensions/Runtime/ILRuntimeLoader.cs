using ILRuntime.Runtime.Enviorment;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.ILRuntime.Extensions;
using UnityEngine.Networking;
using System;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace UnityEngine.ILRuntime.Extensions
{
    public class ILRuntimeLoader : MonoBehaviour
    {

        AppDomain appDomain;
        List<IDisposable> disposables;

        public AppDomain AppDomain { get => appDomain; }

        // Start is called before the first frame update
        protected virtual void Start()
        {
            StartCoroutine(LoadILRAssembly());
        }

        string GetUrl(string url)
        {

            if (url.IndexOf("://") < 0)
                url = "file:///" + url;
            return url;
        }



        IEnumerator LoadILRAssembly()
        {
            appDomain = new AppDomain();

            byte[] dllBytes = null, pdbBytes = null;

            string assemblyName;
            disposables = new List<IDisposable>();

            foreach (var assembliyPath in ILRSettings.StreamingAssetsPath.Split('|'))
            {
                assemblyName = Path.GetFileNameWithoutExtension(assembliyPath);
                dllBytes = null;
                pdbBytes = null;

                using (var request = UnityWebRequest.Get(GetUrl(Application.streamingAssetsPath + $"/{assemblyName}.dll")))
                {
                    yield return request.SendWebRequest();
                    if (!string.IsNullOrEmpty(request.error))
                        throw new System.Exception(request.error + "\n" + assembliyPath);

                    dllBytes = request.downloadHandler.data;
                }

                using (var request = UnityWebRequest.Get(GetUrl(Application.streamingAssetsPath + $"/{assemblyName}.pdb")))
                {
                    yield return request.SendWebRequest();
                    if (!string.IsNullOrEmpty(request.error))
                    {
#if UNITY_EDITOR
                        Debug.LogError(request.error);
#endif
                    }
                    else
                    {
                        pdbBytes = request.downloadHandler.data;
                    }
                }

                MemoryStream fs = null, p = null;
                fs = new MemoryStream(dllBytes);
                disposables.Add(fs);

                if (pdbBytes != null)
                {
                    p = new MemoryStream(pdbBytes);
                    disposables.Add(p);
                }

                try
                {
                    appDomain.LoadAssembly(fs, p, new global::ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
                }
                catch
                {
                    Debug.LogError("º”‘ÿ»»∏¸DLL ß∞‹");
                }

            }


            OnILRInitialize();

            appDomain.DebugService.StartDebugService(56000);


            OnILRLoaded();
        }
        protected virtual void OnILRInitialize()
        {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
            appDomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif

            appDomain.InitalizeExtensions();
        }

        protected virtual void OnILRLoaded()
        {

        }

        private void OnDestroy()
        {
            if (appDomain != null)
            {
                appDomain.Dispose();
                appDomain = null;
            }
            try
            {
                foreach (var o in disposables)
                {
                    o.Dispose();
                }
                disposables.Clear();
            }
            catch { }

        }
    }
}
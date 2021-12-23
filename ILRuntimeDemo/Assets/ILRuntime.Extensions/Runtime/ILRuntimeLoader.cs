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
        protected List<IDisposable> disposeObjs = new List<IDisposable>();


        public AppDomain AppDomain { get => appDomain; }

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
            appDomain = new AppDomain();

            byte[] dllBytes = null, pdbBytes = null;

            if (disposeObjs == null)
                disposeObjs = new List<IDisposable>();
            string streamingAssetsPath = Application.streamingAssetsPath;
            if (!string.IsNullOrEmpty(ILRSettings.StreamingAssetsPath))
            {
                streamingAssetsPath += $"/{ILRSettings.StreamingAssetsPath}";
            }
            foreach (var assemblyName in ILRSettings.AssemblyName.Split('|'))
            {
                string url;

                dllBytes = null;
                pdbBytes = null;
                url = GetUrl(streamingAssetsPath + $"/{assemblyName}.dll");
                using (var request = UnityWebRequest.Get(url))
                {
                    yield return request.SendWebRequest();
                    if (!string.IsNullOrEmpty(request.error))
                        throw new System.Exception(request.error + "\n" + url);

                    dllBytes = request.downloadHandler.data;
                }
                url = GetUrl(streamingAssetsPath + $"/{assemblyName}.pdb");
                using (var request = UnityWebRequest.Get(url))
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
                disposeObjs.Add(fs);

                if (pdbBytes != null)
                {
                    p = new MemoryStream(pdbBytes);
                    disposeObjs.Add(p);
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
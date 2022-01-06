using ILRuntime.Runtime.Enviorment;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
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

            if (string.IsNullOrEmpty(ILRSettings.AssemblyName))
            {
                Debug.LogError($"'{ILRSettings.ProjectSettingsPath}/Assembly Name' not set");
                yield break;
            }

            appDomain = new AppDomain();

            byte[] dllBytes = null, pdbBytes = null;

            if (disposeObjs == null)
                disposeObjs = new List<IDisposable>();

            foreach (var assemblyName in ILRSettings.AssemblyName.Split('|'))
            {
                string url;
                bool isDone;
                string fileName;

                dllBytes = null;
                pdbBytes = null;
                fileName = assemblyName + ".dll";
                isDone = false;
                LoadAsset(fileName, (bytes) =>
                {
                    isDone = true;
                    if (bytes == null)
                        throw new Exception("Load dll fail. " + fileName);
                    dllBytes = bytes;
                });
                yield return new WaitUntil(() => isDone);

                if (HasPDB(assemblyName))
                {
                    fileName = assemblyName + ".pdb";
                    isDone = false;
                    LoadAsset(fileName, (bytes) =>
                    {
                        isDone = true;
                        pdbBytes = bytes;
                    });
                    yield return new WaitUntil(() => isDone);
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
        protected virtual bool HasPDB(string assemblyName)
        {
            return true;
        }
        protected virtual void LoadAsset(string fileName, Action<byte[]> result)
        {
            string streamingAssetsPath = Application.streamingAssetsPath;
            if (!string.IsNullOrEmpty(ILRSettings.StreamingAssetsPath))
            {
                streamingAssetsPath += $"/{ILRSettings.StreamingAssetsPath}";
            }
            string url;
            url = GetUrl(streamingAssetsPath + $"/{fileName}");
            StartCoroutine(_LoadAsset(url, fileName, result));
        }

        private IEnumerator _LoadAsset(string url, string fileName, Action<byte[]> result)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (!string.IsNullOrEmpty(request.error))
                {
                    Debug.LogWarning(request.error + "\n" + url);
                }
                else
                {
                    byte[] bytes = request.downloadHandler.data;
                    result(bytes);
                }
            }
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
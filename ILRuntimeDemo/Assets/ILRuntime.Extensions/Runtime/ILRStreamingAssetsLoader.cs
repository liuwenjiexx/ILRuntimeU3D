using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityEngine.ILRuntime.Extensions
{
    /// <summary>
    /// 从 StreamingAssets 目录加载 ILR 程序集
    /// </summary>
    public class ILRStreamingAssetsLoader : ILRuntimeLoader
    {
        public string streamingAssetsPath;
        public string extension = ".dll";

        protected string GetStreamingAssetsUrl(params string[] relativePath)
        {
            string url = Application.streamingAssetsPath;
            if (url.IndexOf("://") < 0)
                url = "file:///" + url;

            for (int i = 0; i < relativePath.Length; i++)
            {
                if (string.IsNullOrEmpty(relativePath[i]))
                    continue;
                url = url + "/" + relativePath[i];
            }
            return url;
        }

        protected override void LoadAssembly(string assemblyName, ILRuntimeLoadAssemblyCallback result)
        {
            StartCoroutine(_LoadAssembly(assemblyName, result));
        }

        private IEnumerator _LoadAssembly(string assemblyName, ILRuntimeLoadAssemblyCallback result)
        {
            string url;
            url = GetStreamingAssetsUrl(streamingAssetsPath, $"{assemblyName}{extension}");
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

            url = GetStreamingAssetsUrl(streamingAssetsPath + $"/{assemblyName}.pdb");
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
    }
}